using System;
using System.Collections.Generic;
using System.IO;

namespace EternalRealmsOnline.Core.Economy
{
    /// <summary>
    /// Maintenance operations for the durable item-transfer WAL.
    /// Compaction is intended for a quiescent server during startup/shutdown maintenance,
    /// after pending transfers have been reconciled and before new transfers are accepted.
    /// </summary>
    public static class EROItemTransferJournalMaintenance
    {
        private const string TempSuffix = ".compact.tmp";
        private const string BackupSuffix = ".compact.bak";

        public static void Compact(string journalPath)
        {
            if (string.IsNullOrWhiteSpace(journalPath))
                throw new ArgumentException("Journal path is required.", nameof(journalPath));

            string fullPath = Path.GetFullPath(journalPath);
            if (!File.Exists(fullPath))
                return;

            RecoverInterruptedCompaction(fullPath);

            var source = new EROItemTransferJournal(fullPath);
            IReadOnlyList<EROItemTransferJournal.PendingTransfer> pending = source.ReadPending();

            string tempPath = fullPath + TempSuffix;
            string backupPath = fullPath + BackupSuffix;
            try
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
                var compacted = new EROItemTransferJournal(tempPath);
                foreach (var transfer in pending)
                {
                    compacted.BeginTransfer(
                        transfer.TransferId,
                        transfer.SourceActorId,
                        transfer.TargetActorId,
                        transfer.Item);
                }

                // The compacted file is fully flushed before the live WAL is moved aside.
                // If the process dies between these moves, the backup remains discoverable
                // and RecoverInterruptedCompaction() restores the previous valid WAL.
                if (File.Exists(backupPath)) File.Delete(backupPath);
                File.Move(fullPath, backupPath);
                try
                {
                    File.Move(tempPath, fullPath);
                }
                catch
                {
                    if (File.Exists(fullPath)) File.Delete(fullPath);
                    if (File.Exists(backupPath)) File.Move(backupPath, fullPath);
                    throw;
                }

                if (File.Exists(backupPath)) File.Delete(backupPath);
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
        }

        /// <summary>
        /// Repairs a compaction interrupted after the live WAL was moved to its backup.
        /// If a valid live WAL already exists, it wins; otherwise the previous WAL is restored.
        /// This method is safe to call before reading or compacting the journal.
        /// </summary>
        public static void RecoverInterruptedCompaction(string journalPath)
        {
            if (string.IsNullOrWhiteSpace(journalPath))
                throw new ArgumentException("Journal path is required.", nameof(journalPath));

            string fullPath = Path.GetFullPath(journalPath);
            string tempPath = fullPath + TempSuffix;
            string backupPath = fullPath + BackupSuffix;

            if (File.Exists(tempPath))
                File.Delete(tempPath);

            if (File.Exists(fullPath))
            {
                if (File.Exists(backupPath))
                    File.Delete(backupPath);
                return;
            }

            if (File.Exists(backupPath))
                File.Move(backupPath, fullPath);
        }
    }
}
