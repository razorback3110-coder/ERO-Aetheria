using System;
using System.Collections.Generic;

namespace EternalRealmsOnline.Core.Economy
{
    /// <summary>
    /// Server-authoritative ownership transfer for concrete item instances.
    /// The operation is idempotent by transfer id and rolls both inventories back
    /// if either side rejects the mutation. When configured, a durable write-ahead
    /// journal makes an in-flight transfer recoverable after a process crash.
    /// </summary>
    public sealed class EROItemTransferService
    {
        private readonly EROItemTransferJournal journal;
        private readonly EROEconomyMutationGate mutationGate;

        public EROItemTransferService(
            EROItemTransferJournal journal = null,
            EROEconomyMutationGate mutationGate = null)
        {
            this.journal = journal;
            this.mutationGate = mutationGate;
        }

        public bool TryTransfer(
            string transferId,
            EROInstanceInventory source,
            EROInstanceInventory target,
            string instanceId)
        {
            ValidateId(transferId, nameof(transferId));
            ValidateId(instanceId, nameof(instanceId));
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (ReferenceEquals(source, target)) throw new InvalidOperationException("Source and target inventories must differ.");

            mutationGate?.EnsureReady();

            if (!source.TryGet(instanceId, out var item))
            {
                if (target.Contains(instanceId))
                    return false;
                throw new InvalidOperationException("Source inventory does not own the requested item instance.");
            }

            if (target.Contains(instanceId))
                throw new InvalidOperationException("Target inventory already owns the requested item instance.");

            // Persist the intent before either inventory changes. A crash after removal
            // can then be reconciled from the exact item payload in the journal.
            journal?.BeginTransfer(transferId, source.ActorId, target.ActorId, item);

            var sourceBefore = source.CaptureSnapshot();
            var targetBefore = target.CaptureSnapshot();
            string removeTransactionId = transferId + ":remove";
            string addTransactionId = transferId + ":add";

            try
            {
                if (!source.TryRemove(removeTransactionId, instanceId, out var removed) || removed == null)
                    throw new InvalidOperationException("Source inventory rejected the transfer removal.");

                if (!target.TryAdd(addTransactionId, item))
                    throw new InvalidOperationException("Target inventory rejected the transfer addition.");

                journal?.CommitTransfer(transferId);
                return true;
            }
            catch
            {
                source.RestoreSnapshot(sourceBefore);
                target.RestoreSnapshot(targetBefore);
                throw;
            }
        }

        /// <summary>
        /// Reconciles transfer intents left without a COMMIT marker after a process crash.
        /// Must be called after all player inventories are loaded and before gameplay writes
        /// are accepted. Recovery is deliberately idempotent and never overwrites an item
        /// already owned by the target inventory.
        /// </summary>
        public IReadOnlyList<EROItemTransferJournal.PendingTransfer> RecoverPendingTransfers(
            Func<string, EROInstanceInventory> inventoryResolver)
        {
            if (journal == null)
                throw new InvalidOperationException("A durable transfer journal is required for recovery.");
            if (inventoryResolver == null)
                throw new ArgumentNullException(nameof(inventoryResolver));

            var pendingBefore = journal.ReadPending();
            journal.RecoverPending(inventoryResolver);
            return pendingBefore;
        }

        private static void ValidateId(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(name + " is required.", name);
            if (value.Length > 128) throw new ArgumentException(name + " is too long.", name);
        }
    }
}
