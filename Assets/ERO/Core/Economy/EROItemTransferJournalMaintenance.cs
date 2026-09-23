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
        public static void Compact(string journalPath)
        {
            if (string.IsNullOrWhiteSpace(journalPath))
                throw new ArgumentException("Journal path is required.", nameof(journalPath));

            string fullPath = Path.GetFullPath(journalPath);
            if (!File.Exists(fullPath))
                return;

            var source = new EROItemTransferJournal(fullPath);
            IReadOnlyList<EROItemTransferJournal.PendingTransfer> pending = source.ReadPending();

            string directory = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrEmpty(directory))
                directory = Directory.GetCurrentDirectory();

            string tempPath = fullPath + ".compact.tmp";
            string backupPath = fullPath + ".compact.bak";
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

                // The compacted file is complete and flushed before replacing the live WAL.
                if (File.Exists(backupPath)) File.Delete(backupPath);
                File.Move(fullPath, backupPath);
                try
                {
                    File.Move(tempPath, fullPath);
                    File.Delete(backupPath);
                }
                catch
                {
                    if (File.Exists(fullPath)) File.Delete(fullPath);
                    if (File.Exists(backupPath)) File.Move(backupPath, fullPath);
                    throw;
                }
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
                if (File.Exists(backupPath)) File.Delete(backupPath);
            }
        }
    }
}
