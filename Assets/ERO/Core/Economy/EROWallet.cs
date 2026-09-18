using System;
using System.Collections.Generic;

namespace EternalRealmsOnline.Core.Economy
{
    /// <summary>
    /// Authoritative, deterministic wallet for server-side MMO currencies.
    /// Mutations are keyed by a caller supplied sequence so reconnects/retries cannot
    /// duplicate a grant or spend. No Unity dependencies: suitable for dedicated servers.
    /// </summary>
    public sealed class EROWallet
    {
        public const int CurrentSnapshotVersion = 2;

        private readonly object sync = new object();
        private readonly Dictionary<string, long> balances = new Dictionary<string, long>(StringComparer.Ordinal);
        private readonly HashSet<string> appliedTransactions = new HashSet<string>(StringComparer.Ordinal);

        public EROWallet(string actorId)
        {
            if (string.IsNullOrWhiteSpace(actorId)) throw new ArgumentException("ActorId is required.", nameof(actorId));
            ActorId = actorId;
        }

        public string ActorId { get; }

        public long GetBalance(string currencyId)
        {
            ValidateCurrency(currencyId);
            lock (sync)
            {
                return balances.TryGetValue(currencyId, out var value) ? value : 0L;
            }
        }

        public bool TryApply(string transactionId, string currencyId, long delta, out long newBalance)
        {
            if (string.IsNullOrWhiteSpace(transactionId)) throw new ArgumentException("TransactionId is required.", nameof(transactionId));
            ValidateCurrency(currencyId);
            if (delta == 0) throw new ArgumentOutOfRangeException(nameof(delta), "Currency delta cannot be zero.");

            lock (sync)
            {
                if (appliedTransactions.Contains(transactionId))
                {
                    newBalance = balances.TryGetValue(currencyId, out var existing) ? existing : 0L;
                    return false;
                }

                var current = balances.TryGetValue(currencyId, out var balance) ? balance : 0L;
                if (delta < 0)
                {
                    long spend;
                    try
                    {
                        spend = checked(-delta);
                    }
                    catch (OverflowException)
                    {
                        newBalance = current;
                        return false;
                    }

                    if (current < spend)
                    {
                        newBalance = current;
                        return false;
                    }
                }

                long next;
                try
                {
                    next = checked(current + delta);
                }
                catch (OverflowException)
                {
                    newBalance = current;
                    return false;
                }

                if (next < 0)
                {
                    newBalance = current;
                    return false;
                }

                balances[currencyId] = next;
                appliedTransactions.Add(transactionId);
                newBalance = next;
                return true;
            }
        }

        /// <summary>
        /// Captures both balances and the idempotency ledger. Persisting only balances is
        /// unsafe: after restart an old transaction could otherwise be applied again.
        /// </summary>
        public WalletSnapshot CaptureSnapshot()
        {
            lock (sync)
            {
                var balanceCopy = new Dictionary<string, long>(balances, StringComparer.Ordinal);
                var transactionCopy = new List<string>(appliedTransactions);
                transactionCopy.Sort(StringComparer.Ordinal);
                return new WalletSnapshot(CurrentSnapshotVersion, ActorId, balanceCopy, transactionCopy);
            }
        }

        public void RestoreSnapshot(WalletSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            if (snapshot.Version != 1 && snapshot.Version != CurrentSnapshotVersion)
                throw new InvalidOperationException("Unsupported wallet snapshot version.");
            if (!string.Equals(snapshot.ActorId, ActorId, StringComparison.Ordinal))
                throw new InvalidOperationException("Wallet actor mismatch.");

            lock (sync)
            {
                balances.Clear();
                foreach (var pair in snapshot.Balances)
                {
                    ValidateCurrency(pair.Key);
                    if (pair.Value < 0) throw new InvalidOperationException("Wallet balances cannot be negative.");
                    balances[pair.Key] = pair.Value;
                }

                appliedTransactions.Clear();
                if (snapshot.Version >= 2)
                {
                    foreach (var transactionId in snapshot.AppliedTransactions)
                    {
                        if (string.IsNullOrWhiteSpace(transactionId))
                            throw new InvalidOperationException("Wallet transaction id cannot be empty.");
                        if (!appliedTransactions.Add(transactionId))
                            throw new InvalidOperationException("Wallet snapshot contains duplicate transaction ids.");
                    }
                }
            }
        }

        private static void ValidateCurrency(string currencyId)
        {
            if (string.IsNullOrWhiteSpace(currencyId)) throw new ArgumentException("CurrencyId is required.", nameof(currencyId));
            if (currencyId.Length > 64) throw new ArgumentException("CurrencyId is too long.", nameof(currencyId));
        }
    }

    public sealed class WalletSnapshot
    {
        public WalletSnapshot(
            int version,
            string actorId,
            IReadOnlyDictionary<string, long> balances,
            IReadOnlyCollection<string> appliedTransactions = null)
        {
            Version = version;
            ActorId = actorId ?? throw new ArgumentNullException(nameof(actorId));
            Balances = balances ?? throw new ArgumentNullException(nameof(balances));
            AppliedTransactions = appliedTransactions ?? Array.Empty<string>();
        }

        public int Version { get; }
        public string ActorId { get; }
        public IReadOnlyDictionary<string, long> Balances { get; }
        public IReadOnlyCollection<string> AppliedTransactions { get; }
    }
}
