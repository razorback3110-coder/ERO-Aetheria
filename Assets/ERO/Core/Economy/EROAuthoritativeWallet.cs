using System;
using System.Collections.Generic;

namespace EternalRealmsOnline.Core.Economy
{
    /// <summary>
    /// Server-authoritative wallet for persistent MMO currencies.
    /// All mutations are bounded by EROCurrencyCatalog and fail atomically.
    /// </summary>
    public sealed class EROAuthoritativeWallet
    {
        private readonly Dictionary<string, Dictionary<string, long>> _balances =
            new Dictionary<string, Dictionary<string, long>>(StringComparer.Ordinal);

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

        private static void ValidateOwner(string ownerId)
        {
            if (string.IsNullOrWhiteSpace(ownerId))
                throw new ArgumentException("Wallet owner id is required.", nameof(ownerId));
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
}
