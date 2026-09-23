using System;
using System.Collections.Generic;

namespace EternalRealmsOnline.Core.Economy
{
    /// <summary>
    /// Server bootstrap boundary for durable item-transfer recovery.
    /// Call once after all player inventories have been loaded and before accepting
    /// any player economy mutations. The operation first repairs an interrupted WAL
    /// compaction, then reconciles uncommitted transfers, and only then opens the
    /// economy mutation gate.
    /// </summary>
    public sealed class EROItemTransferRecoveryCoordinator
    {
        private readonly string journalPath;
        private readonly EROItemTransferService transferService;
        private readonly EROEconomyMutationGate mutationGate;
        private bool completed;

        public EROItemTransferRecoveryCoordinator(
            string journalPath,
            EROItemTransferService transferService,
            EROEconomyMutationGate mutationGate = null)
        {
            if (string.IsNullOrWhiteSpace(journalPath))
                throw new ArgumentException("Journal path is required.", nameof(journalPath));
            this.transferService = transferService ?? throw new ArgumentNullException(nameof(transferService));
            this.journalPath = journalPath;
            this.mutationGate = mutationGate;
        }

        public bool IsCompleted => completed;

        public IReadOnlyList<EROItemTransferJournal.PendingTransfer> Recover(
            Func<string, EROInstanceInventory> inventoryResolver)
        {
            if (inventoryResolver == null)
                throw new ArgumentNullException(nameof(inventoryResolver));
            if (completed)
                throw new InvalidOperationException("Item transfer recovery has already completed for this coordinator.");

            // Repair the filesystem state before the journal is read. This is safe even
            // when no compaction was interrupted.
            EROItemTransferJournalMaintenance.RecoverInterruptedCompaction(journalPath);

            // Recovery must happen before accepting economy writes. If validation or
            // reconciliation fails, neither completed nor the mutation gate is opened.
            IReadOnlyList<EROItemTransferJournal.PendingTransfer> pending =
                transferService.RecoverPendingTransfers(inventoryResolver);

            completed = true;
            mutationGate?.MarkReady();
            return pending;
        }
    }
}
