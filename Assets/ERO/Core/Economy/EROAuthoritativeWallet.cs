using System;
using System.Collections.Generic;
using System.Globalization;

namespace EternalRealmsOnline.Core.Economy
{
    /// <summary>
    /// Server-authoritative wallet for persistent MMO currencies.
    /// All mutations are bounded by EROCurrencyCatalog and fail atomically.
    /// Transaction ids make retried network commands idempotent.
    /// </summary>
    public sealed class EROAuthoritativeWallet
    {
        private readonly Dictionary<string, Dictionary<string, long>> _balances =
            new Dictionary<string, Dictionary<string, long>>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _appliedTransactions =
            new Dictionary<string, string>(StringComparer.Ordinal);

        public long GetBalance(string ownerId, string currencyId)
        {
            ValidateOwner(ownerId);
            if (!EROCurrencyCatalog.IsKnown(currencyId))
                throw new ArgumentException("Unknown ERO currency.", nameof(currencyId));

            if (!_balances.TryGetValue(ownerId, out Dictionary<string, long> wallet))
                return 0L;

            return wallet.TryGetValue(currencyId, out long balance) ? balance : 0L;
        }

        public bool TryCredit(string ownerId, string currencyId, long amount)
        {
            ValidateMutation(ownerId, currencyId, amount);
            long current = GetBalance(ownerId, currencyId);
            long maximum = EROCurrencyCatalog.GetMaxBalance(currencyId);
            if (current > maximum - amount)
                return false;

            SetBalance(ownerId, currencyId, current + amount);
            return true;
        }

        public bool TryDebit(string ownerId, string currencyId, long amount)
        {
            ValidateMutation(ownerId, currencyId, amount);
            long current = GetBalance(ownerId, currencyId);
            if (current < amount)
                return false;

            SetBalance(ownerId, currencyId, current - amount);
            return true;
        }

        /// <summary>
        /// Applies a currency mutation exactly once for a stable server-generated transaction id.
        /// Replaying the same transaction with the same payload is a successful no-op; reusing an
        /// id for a different mutation is rejected to prevent ambiguous economic state.
        /// </summary>
        public bool TryApplyTransaction(string transactionId, string ownerId, string currencyId, long amount, bool credit)
        {
            ValidateTransactionId(transactionId);
            ValidateMutation(ownerId, currencyId, amount);

            string fingerprint = BuildTransactionFingerprint(ownerId, currencyId, amount, credit);
            if (_appliedTransactions.TryGetValue(transactionId, out string existingFingerprint))
            {
                if (!string.Equals(existingFingerprint, fingerprint, StringComparison.Ordinal))
                    throw new InvalidOperationException("Transaction id was already used for a different wallet mutation.");
                return true;
            }

            bool applied = credit
                ? TryCredit(ownerId, currencyId, amount)
                : TryDebit(ownerId, currencyId, amount);
            if (!applied)
                return false;

            _appliedTransactions.Add(transactionId, fingerprint);
            return true;
        }

        public bool HasAppliedTransaction(string transactionId)
        {
            ValidateTransactionId(transactionId);
            return _appliedTransactions.ContainsKey(transactionId);
        }

        public void SetBalanceFromTrustedPersistence(string ownerId, string currencyId, long balance)
        {
            ValidateOwner(ownerId);
            if (!EROCurrencyCatalog.IsValidBalance(currencyId, balance))
                throw new ArgumentOutOfRangeException(nameof(balance), "Currency balance is outside the authoritative bounds.");

            SetBalance(ownerId, currencyId, balance);
        }

        public IReadOnlyList<EROWalletBalanceEntry> CaptureSnapshot()
        {
            var entries = new List<EROWalletBalanceEntry>();
            foreach (KeyValuePair<string, Dictionary<string, long>> owner in _balances)
            {
                foreach (KeyValuePair<string, long> balance in owner.Value)
                    entries.Add(new EROWalletBalanceEntry(owner.Key, balance.Key, balance.Value));
            }

            entries.Sort((left, right) =>
            {
                int ownerCompare = string.CompareOrdinal(left.OwnerId, right.OwnerId);
                return ownerCompare != 0 ? ownerCompare : string.CompareOrdinal(left.CurrencyId, right.CurrencyId);
            });
            return entries;
        }

        public IReadOnlyList<EROWalletTransactionEntry> CaptureTransactionJournal()
        {
            var entries = new List<EROWalletTransactionEntry>(_appliedTransactions.Count);
            foreach (KeyValuePair<string, string> transaction in _appliedTransactions)
                entries.Add(new EROWalletTransactionEntry(transaction.Key, transaction.Value));

            entries.Sort((left, right) => string.CompareOrdinal(left.TransactionId, right.TransactionId));
            return entries;
        }

        public void RestoreSnapshot(IReadOnlyList<EROWalletBalanceEntry> entries)
        {
            if (entries == null) throw new ArgumentNullException(nameof(entries));

            var restored = new Dictionary<string, Dictionary<string, long>>(StringComparer.Ordinal);
            string previousOwner = null;
            string previousCurrency = null;

            foreach (EROWalletBalanceEntry entry in entries)
            {
                ValidateOwner(entry.OwnerId);
                if (!EROCurrencyCatalog.IsValidBalance(entry.CurrencyId, entry.Balance))
                    throw new InvalidOperationException("Wallet snapshot contains an invalid currency balance.");

                if (previousOwner != null)
                {
                    int order = string.CompareOrdinal(previousOwner, entry.OwnerId);
                    if (order > 0 || (order == 0 && string.CompareOrdinal(previousCurrency, entry.CurrencyId) >= 0))
                        throw new InvalidOperationException("Wallet snapshot entries must be unique and ordinally sorted.");
                }

                if (!restored.TryGetValue(entry.OwnerId, out Dictionary<string, long> wallet))
                {
                    wallet = new Dictionary<string, long>(StringComparer.Ordinal);
                    restored.Add(entry.OwnerId, wallet);
                }

                if (!wallet.TryAdd(entry.CurrencyId, entry.Balance))
                    throw new InvalidOperationException("Wallet snapshot contains duplicate currency entries.");

                previousOwner = entry.OwnerId;
                previousCurrency = entry.CurrencyId;
            }

            _balances.Clear();
            foreach (KeyValuePair<string, Dictionary<string, long>> owner in restored)
                _balances.Add(owner.Key, owner.Value);
        }

        public void RestoreTransactionJournal(IReadOnlyList<EROWalletTransactionEntry> entries)
        {
            if (entries == null) throw new ArgumentNullException(nameof(entries));

            var restored = new Dictionary<string, string>(StringComparer.Ordinal);
            string previousTransactionId = null;
            foreach (EROWalletTransactionEntry entry in entries)
            {
                ValidateTransactionId(entry.TransactionId);
                if (!TryValidateTransactionFingerprint(entry.Fingerprint))
                    throw new InvalidOperationException("Wallet transaction journal contains a non-canonical fingerprint.");
                if (previousTransactionId != null && string.CompareOrdinal(previousTransactionId, entry.TransactionId) >= 0)
                    throw new InvalidOperationException("Wallet transaction journal entries must be unique and ordinally sorted.");
                if (!restored.TryAdd(entry.TransactionId, entry.Fingerprint))
                    throw new InvalidOperationException("Wallet transaction journal contains duplicate transaction ids.");
                previousTransactionId = entry.TransactionId;
            }

            _appliedTransactions.Clear();
            foreach (KeyValuePair<string, string> transaction in restored)
                _appliedTransactions.Add(transaction.Key, transaction.Value);
        }

        private void SetBalance(string ownerId, string currencyId, long balance)
        {
            if (!_balances.TryGetValue(ownerId, out Dictionary<string, long> wallet))
            {
                wallet = new Dictionary<string, long>(StringComparer.Ordinal);
                _balances.Add(ownerId, wallet);
            }

            wallet[currencyId] = balance;
        }

        private static void ValidateMutation(string ownerId, string currencyId, long amount)
        {
            ValidateOwner(ownerId);
            if (!EROCurrencyCatalog.IsKnown(currencyId))
                throw new ArgumentException("Unknown ERO currency.", nameof(currencyId));
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Currency mutation amount must be positive.");
        }

        private static void ValidateTransactionId(string transactionId)
        {
            if (string.IsNullOrWhiteSpace(transactionId) || transactionId.IndexOf('|') >= 0)
                throw new ArgumentException("Transaction id is required and cannot contain '|'.", nameof(transactionId));
        }

        private static bool TryValidateTransactionFingerprint(string fingerprint)
        {
            if (string.IsNullOrWhiteSpace(fingerprint))
                return false;

            string[] parts = fingerprint.Split('|');
            if (parts.Length != 4 || (parts[0] != "credit" && parts[0] != "debit") || string.IsNullOrWhiteSpace(parts[1]) || string.IsNullOrWhiteSpace(parts[2]))
                return false;
            if (parts[1].IndexOf('|') >= 0 || parts[2].IndexOf('|') >= 0)
                return false;
            if (!EROCurrencyCatalog.IsKnown(parts[2]))
                return false;
            if (!long.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out long amount) || amount <= 0)
                return false;

            return string.Equals(fingerprint, BuildTransactionFingerprint(parts[1], parts[2], amount, parts[0] == "credit"), StringComparison.Ordinal);
        }

        private static string BuildTransactionFingerprint(string ownerId, string currencyId, long amount, bool credit)
        {
            return (credit ? "credit" : "debit") + "|" + ownerId + "|" + currencyId + "|" + amount.ToString(CultureInfo.InvariantCulture);
        }

        private static void ValidateOwner(string ownerId)
        {
            if (string.IsNullOrWhiteSpace(ownerId) || ownerId.IndexOf('|') >= 0)
                throw new ArgumentException("Wallet owner id is required and cannot contain '|'.", nameof(ownerId));
        }
    }

    public readonly struct EROWalletBalanceEntry
    {
        public EROWalletBalanceEntry(string ownerId, string currencyId, long balance)
        {
            if (string.IsNullOrWhiteSpace(ownerId)) throw new ArgumentException("Wallet owner id is required.", nameof(ownerId));
            if (!EROCurrencyCatalog.IsValidBalance(currencyId, balance))
                throw new ArgumentOutOfRangeException(nameof(balance), "Currency balance is outside the authoritative bounds.");

            OwnerId = ownerId;
            CurrencyId = currencyId;
            Balance = balance;
        }

        public string OwnerId { get; }
        public string CurrencyId { get; }
        public long Balance { get; }
    }

    public readonly struct EROWalletTransactionEntry
    {
        public EROWalletTransactionEntry(string transactionId, string fingerprint)
        {
            if (string.IsNullOrWhiteSpace(transactionId)) throw new ArgumentException("Transaction id is required.", nameof(transactionId));
            if (string.IsNullOrWhiteSpace(fingerprint)) throw new ArgumentException("Transaction fingerprint is required.", nameof(fingerprint));
            TransactionId = transactionId;
            Fingerprint = fingerprint;
        }

        public string TransactionId { get; }
        public string Fingerprint { get; }
    }
}
