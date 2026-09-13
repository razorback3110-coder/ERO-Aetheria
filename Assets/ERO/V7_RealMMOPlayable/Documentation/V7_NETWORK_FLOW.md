# V7 Network Flow

Client
  -> connection
  -> authenticated session identity
  -> server-authoritative player state
  -> movement input
  -> server validation
  -> replicated state
  -> combat request
  -> server validation
  -> HP/XP/loot changes
  -> persistence queue

Recommended production topology:
Client -> Gateway -> World Instance -> Persistence/Services

The V7 gameplay contracts are ERO-owned and are not copied from third-party MMO code.
