using System;
using System.Collections.Generic;

namespace EternalRealmsOnline.Core.Economy
{
    /// <summary>
    /// Authoritative, deterministic wallet for server-side MMO currencies.
    /// Mutations are keyed by a transaction id so retries cannot duplicate a grant
    /// or spend. Transaction fingerprints prevent reuse of an id for another operation.
    /// </summary>
    public sealed class EROWallet
    {
        public const int CurrentSnapshotVersion = 3;
        private readonly object sync = new object();
        private readonly Dictionary<string, long> balances = new Dictionary<string, long>(StringComparer.Ordinal);
        private readonly Dictionary<string, WalletTransaction> appliedTransactions = new Dictionary<string, WalletTransaction>(StringComparer.Ordinal);

        public EROWallet(string actorId)
        {
            if (string.IsNullOrWhiteSpace(actorId)) throw new ArgumentException("ActorId is required.", nameof(actorId));
            ActorId = actorId;
        }

        public string ActorId { get; }

        public long GetBalance(string currencyId)
        {
            ValidateCurrency(currencyId);
            lock (sync) return balances.TryGetValue(currencyId, out var value) ? value : 0L;
        }

        public bool TryApply(string transactionId, string currencyId, long delta, out long newBalance)
        {
            if (string.IsNullOrWhiteSpace(transactionId)) throw new ArgumentException("TransactionId is required.", nameof(transactionId));
            ValidateCurrency(currencyId);
            if (delta == 0) throw new ArgumentOutOfRangeException(nameof(delta), "Currency delta cannot be zero.");

            lock (sync)
            {
                if (appliedTransactions.TryGetValue(transactionId, out var previous))
                {
                    if (!previous.Matches(currencyId, delta))
                        throw new InvalidOperationException("TransactionId was already used for a different wallet operation.");
                    newBalance = balances.TryGetValue(currencyId, out var existing) ? existing : 0L;
                    return false;
                }

                var current = balances.TryGetValue(currencyId, out var balance) ? balance : 0L;
                if (delta < 0)
                {
                    long spend;
                    try { spend = checked(-delta); }
                    catch (OverflowException) { newBalance = current; return false; }
                    if (current < spend) { newBalance = current; return false; }
                }

                long next;
                try { next = checked(current + delta); }
                catch (OverflowException) { newBalance = current; return false; }
                if (!EROCurrencyCatalog.IsValidBalance(currencyId, next)) { newBalance = current; return false; }

                balances[currencyId] = next;
                appliedTransactions.Add(transactionId, new WalletTransaction(currencyId, delta));
                newBalance = next;
                return true;
            }
        }

        public WalletSnapshot CaptureSnapshot()
        {
            lock (sync)
            {
                return new WalletSnapshot(
                    CurrentSnapshotVersion,
                    ActorId,
                    new Dictionary<string, long>(balances, StringComparer.Ordinal),
                    new Dictionary<string, WalletTransaction>(appliedTransactions, StringComparer.Ordinal));
            }
        }

        public void RestoreSnapshot(WalletSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            if (snapshot.Version < 1 || snapshot.Version > CurrentSnapshotVersion) throw new InvalidOperationException("Unsupported wallet snapshot version.");
            if (!string.Equals(snapshot.ActorId, ActorId, StringComparison.Ordinal)) throw new InvalidOperationException("Wallet actor mismatch.");

            lock (sync)
            {
                balances.Clear();
                foreach (var pair in snapshot.Balances)
                {
                    ValidateCurrency(pair.Key);
                    if (!EROCurrencyCatalog.IsValidBalance(pair.Key, pair.Value))
                        throw new InvalidOperationException("Wallet snapshot contains an invalid currency balance.");
                    balances[pair.Key] = pair.Value;
                }

                appliedTransactions.Clear();
                if (snapshot.Version >= 3)
                {
                    foreach (var pair in snapshot.Transactions)
                    {
                        if (string.IsNullOrWhiteSpace(pair.Key)) throw new InvalidOperationException("Wallet transaction id cannot be empty.");
                        ValidateCurrency(pair.Value.CurrencyId);
                        if (pair.Value.Delta == 0) throw new InvalidOperationException("Wallet transaction delta cannot be zero.");
                        if (!appliedTransactions.TryAdd(pair.Key, pair.Value)) throw new InvalidOperationException("Wallet snapshot contains duplicate transaction ids.");
                    }
                }
                else if (snapshot.Version == 2)
                {
                    foreach (var transactionId in snapshot.AppliedTransactions)
                    {
                        if (string.IsNullOrWhiteSpace(transactionId)) throw new InvalidOperationException("Wallet transaction id cannot be empty.");
                        if (!appliedTransactions.TryAdd(transactionId, WalletTransaction.Legacy)) throw new InvalidOperationException("Wallet snapshot contains duplicate transaction ids.");
                    }
                }
            }
        }

        private static void ValidateCurrency(string currencyId)
        {
            if (string.IsNullOrWhiteSpace(currencyId)) throw new ArgumentException("CurrencyId is required.", nameof(currencyId));
            if (currencyId.Length > 64) throw new ArgumentException("CurrencyId is too long.", nameof(currencyId));
            if (!EROCurrencyCatalog.IsKnown(currencyId)) throw new ArgumentException("Unknown ERO currency.", nameof(currencyId));
        }

        public readonly struct WalletTransaction
        {
            public static WalletTransaction Legacy { get; } = new WalletTransaction(string.Empty, 0L);
            public WalletTransaction(string currencyId, long delta) { CurrencyId = currencyId ?? throw new ArgumentNullException(nameof(currencyId)); Delta = delta; }
            public string CurrencyId { get; }
            public long Delta { get; }
            public bool Matches(string currencyId, long delta) => string.IsNullOrEmpty(CurrencyId) || (string.Equals(CurrencyId, currencyId, StringComparison.Ordinal) && Delta == delta);
        }
    }

    public sealed class WalletSnapshot
    {
        public WalletSnapshot(int version, string actorId, IReadOnlyDictionary<string, long> balances, IReadOnlyCollection<string> appliedTransactions = null, IReadOnlyDictionary<string, EROWallet.WalletTransaction> transactions = null)
        {
            Version = version;
            ActorId = actorId ?? throw new ArgumentNullException(nameof(actorId));
            Balances = balances ?? throw new ArgumentNullException(nameof(balances));
            AppliedTransactions = appliedTransactions ?? Array.Empty<string>();
            Transactions = transactions ?? new Dictionary<string, EROWallet.WalletTransaction>(StringComparer.Ordinal);
        }
        public WalletSnapshot(int version, string actorId, IReadOnlyDictionary<string, long> balances, IReadOnlyDictionary<string, EROWallet.WalletTransaction> transactions) : this(version, actorId, balances, null, transactions) { }
        public int Version { get; }
        public string ActorId { get; }
        public IReadOnlyDictionary<string, long> Balances { get; }
        public IReadOnlyCollection<string> AppliedTransactions { get; }
        public IReadOnlyDictionary<string, EROWallet.WalletTransaction> Transactions { get; }
    }
}
