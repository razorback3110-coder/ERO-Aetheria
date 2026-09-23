using System;

namespace EternalRealmsOnline.Core.Economy
{
    /// <summary>
    /// Server-authoritative ownership transfer for concrete item instances.
    /// The operation is idempotent by transfer id and rolls both inventories back
    /// if either side rejects the mutation. Durable persistence of the resulting
    /// inventories is still owned by EROPlayerPersistenceCoordinator.
    /// </summary>
    public sealed class EROItemTransferService
    {
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

            if (!source.TryGet(instanceId, out var item))
            {
                if (target.Contains(instanceId))
                    return false;
                throw new InvalidOperationException("Source inventory does not own the requested item instance.");
            }

            if (target.Contains(instanceId))
                throw new InvalidOperationException("Target inventory already owns the requested item instance.");

            // Capture both authoritative states so a rejected second mutation cannot
            // leave the two inventories split across different ownership states.
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

                return true;
            }
            catch
            {
                source.RestoreSnapshot(sourceBefore);
                target.RestoreSnapshot(targetBefore);
                throw;
            }
        }

        private static void ValidateId(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(name + " is required.", name);
            if (value.Length > 128) throw new ArgumentException(name + " is too long.", name);
        }
    }
}
