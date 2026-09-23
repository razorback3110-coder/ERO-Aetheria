using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace EternalRealmsOnline.Core.Economy
{
    /// <summary>
    /// Durable write-ahead journal for item ownership transfers.
    /// A transfer intent is persisted before either inventory is mutated; a commit marker
    /// is persisted only after both mutations succeed. Pending intents can therefore be
    /// replayed after a process crash without inventing or deleting an item.
    /// </summary>
    public sealed class EROItemTransferJournal
    {
        private const string Begin = "BEGIN";
        private const string Commit = "COMMIT";
        private readonly string path;
        private readonly object sync = new object();

        public EROItemTransferJournal(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Journal path is required.", nameof(path));
            this.path = Path.GetFullPath(path);
            var directory = Path.GetDirectoryName(this.path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        }

        public void BeginTransfer(string transferId, string sourceActorId, string targetActorId, EROItemInstance item)
        {
            ValidateId(transferId, nameof(transferId));
            ValidateId(sourceActorId, nameof(sourceActorId));
            ValidateId(targetActorId, nameof(targetActorId));
            if (item == null) throw new ArgumentNullException(nameof(item));

            string record = string.Join("\t", Begin,
                Encode(transferId), Encode(sourceActorId), Encode(targetActorId), Encode(item.InstanceId),
                Encode(item.ItemId), item.Quantity.ToString(CultureInfo.InvariantCulture),
                item.MaxStack.ToString(CultureInfo.InvariantCulture),
                item.Level.ToString(CultureInfo.InvariantCulture), EncodeStats(item.Stats));
            Append(record);
        }

        public void CommitTransfer(string transferId)
        {
            ValidateId(transferId, nameof(transferId));
            Append(string.Join("\t", Commit, Encode(transferId)));
        }

        public IReadOnlyList<PendingTransfer> ReadPending()
        {
            lock (sync)
            {
                var pending = new Dictionary<string, PendingTransfer>(StringComparer.Ordinal);
                if (!File.Exists(path)) return new List<PendingTransfer>();

                foreach (string line in File.ReadAllLines(path))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string[] parts = line.Split('\t');
                    try
                    {
                        if (parts.Length == 2 && parts[0] == Commit)
                        {
                            string transferId = Decode(parts[1]);
                            ValidateId(transferId, nameof(transferId));
                            pending.Remove(transferId);
                            continue;
                        }

                        if (parts.Length != 10 || parts[0] != Begin)
                            throw new InvalidDataException("Invalid item transfer journal record.");

                        string transferIdValue = Decode(parts[1]);
                        string sourceActorId = Decode(parts[2]);
                        string targetActorId = Decode(parts[3]);
                        string instanceId = Decode(parts[4]);
                        string itemId = Decode(parts[5]);
                        ValidateId(transferIdValue, nameof(transferId));
                        ValidateId(sourceActorId, nameof(sourceActorId));
                        ValidateId(targetActorId, nameof(targetActorId));
                        ValidateId(instanceId, nameof(instanceId));
                        ValidateId(itemId, nameof(itemId));

                        if (!int.TryParse(parts[6], NumberStyles.Integer, CultureInfo.InvariantCulture, out int quantity) || quantity <= 0)
                            throw new InvalidDataException("Invalid transfer journal quantity.");
                        if (!int.TryParse(parts[7], NumberStyles.Integer, CultureInfo.InvariantCulture, out int maxStack) || maxStack <= 0)
                            throw new InvalidDataException("Invalid transfer journal max stack.");
                        if (!int.TryParse(parts[8], NumberStyles.Integer, CultureInfo.InvariantCulture, out int level) || level < 0)
                            throw new InvalidDataException("Invalid transfer journal level.");

                        var item = new EROItemInstance(instanceId, itemId, quantity, maxStack, level, DecodeStats(parts[9]));
                        var transfer = new PendingTransfer(transferIdValue, sourceActorId, targetActorId, item);
                        pending[transfer.TransferId] = transfer;
                    }
                    catch (FormatException ex)
                    {
                        throw new InvalidDataException("Item transfer journal contains malformed Base64 data.", ex);
                    }
                    catch (ArgumentException ex)
                    {
                        throw new InvalidDataException("Item transfer journal contains an invalid identifier.", ex);
                    }
                }
                return new List<PendingTransfer>(pending.Values);
            }
        }

        public void RecoverPending(Func<string, EROInstanceInventory> inventoryResolver)
        {
            if (inventoryResolver == null) throw new ArgumentNullException(nameof(inventoryResolver));
            foreach (var transfer in ReadPending())
            {
                var source = inventoryResolver(transfer.SourceActorId);
                var target = inventoryResolver(transfer.TargetActorId);
                if (source == null || target == null) continue;

                // Never trust a resolver that returns an inventory for the wrong actor.
                // This protects recovery from misrouting an item into another player's inventory.
                if (!string.Equals(source.ActorId, transfer.SourceActorId, StringComparison.Ordinal) ||
                    !string.Equals(target.ActorId, transfer.TargetActorId, StringComparison.Ordinal))
                    throw new InvalidDataException("Transfer recovery resolved an inventory for the wrong actor.");

                bool sourceOwns = source.Contains(transfer.Item.InstanceId);
                bool targetOwns = target.Contains(transfer.Item.InstanceId);

                // Both sides owning the same immutable instance is an unrecoverable
                // ownership violation. Fail closed rather than marking the transfer committed
                // and allowing a duplicated item to survive a server restart.
                if (sourceOwns && targetOwns)
                    throw new InvalidDataException("Transfer recovery detected duplicate item ownership.");

                if (targetOwns)
                {
                    CommitTransfer(transfer.TransferId);
                    continue;
                }

                // The source still owns the item: the crash happened before removal, so
                // leaving the intent pending is safe and preserves the original owner.
                if (sourceOwns)
                    continue;

                // Neither side owns the item: the durable intent is the authoritative
                // source for reconstruction, so replay the exact item instance.
                if (target.TryAdd(transfer.TransferId + ":recovery:add", transfer.Item))
                    CommitTransfer(transfer.TransferId);
            }
        }

        private void Append(string line)
        {
            lock (sync)
            {
                using (var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(line + Environment.NewLine);
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }
            }
        }

        private static string Encode(string value) => Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        private static string Decode(string value) => Encoding.UTF8.GetString(Convert.FromBase64String(value));

        private static string EncodeStats(IReadOnlyDictionary<string, long> stats)
        {
            if (stats == null || stats.Count == 0) return string.Empty;
            var parts = new List<string>();
            foreach (var pair in stats)
                parts.Add(Encode(pair.Key) + "=" + pair.Value.ToString(CultureInfo.InvariantCulture));
            return string.Join(",", parts);
        }

        private static Dictionary<string, long> DecodeStats(string value)
        {
            var stats = new Dictionary<string, long>(StringComparer.Ordinal);
            if (string.IsNullOrEmpty(value)) return stats;
            foreach (string entry in value.Split(','))
            {
                string[] pair = entry.Split('=');
                if (pair.Length != 2 || !long.TryParse(pair[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out long amount))
                    throw new InvalidDataException("Invalid transfer journal stat.");
                stats.Add(Decode(pair[0]), amount);
            }
            return stats;
        }

        private static void ValidateId(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(name + " is required.", name);
            if (value.Length > 128) throw new ArgumentException(name + " is too long.", name);
        }

        public sealed class PendingTransfer
        {
            public PendingTransfer(string transferId, string sourceActorId, string targetActorId, EROItemInstance item)
            {
                TransferId = transferId;
                SourceActorId = sourceActorId;
                TargetActorId = targetActorId;
                Item = item;
            }
            public string TransferId { get; }
            public string SourceActorId { get; }
            public string TargetActorId { get; }
            public EROItemInstance Item { get; }
        }
    }
}
