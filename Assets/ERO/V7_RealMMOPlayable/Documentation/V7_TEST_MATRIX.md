# V7 Test Matrix

## Local
- Host starts.
- Client connects.
- Two players appear.
- Player movement replicates.
- Attack request is server validated.
- Target HP changes only on server.
- Loot can be claimed once.
- XP/gold are server-owned.

## Abuse tests
- Excessive movement request rejected.
- Out-of-range attack rejected.
- Duplicate loot claim rejected.
- Negative/oversized rewards rejected by authority layer.

## Persistence
- Character save serializes.
- Character load restores.
- Reconnect identity remains deterministic.

## Dedicated server
- Linux Server profile builds through ERO/V7/Build Linux Dedicated Server.
- Batch mode starts the NetworkManager as server when present.

## Runtime certification
Not performed in this environment because Unity 6000.0.67f1 is not available here. The archive has been structurally audited and source-checked.
