# ERO World Streaming Budget

## Runtime policy

`EROWorldChunkStreamer` streams a deterministic square window around the active player. Chunk generation should be amortized across frames rather than generating the entire window in one update, to avoid frame spikes when crossing chunk boundaries.

Recommended production defaults:

- Chunk size: 48 world units
- Stream radius: 2 chunks
- Maximum new chunks generated per frame: 2
- Maximum chunks destroyed per frame: 2
- Generation seed: deterministic coordinate hash
- Gameplay-authoritative entities must remain server-owned; procedural ground is presentation/world infrastructure only.

## Next implementation

The runtime streamer should expose generation/removal budgets and process queued chunk work incrementally. This keeps the current deterministic world compatible while preparing the system for Addressables-backed approved assets and server-side interest management.

No third-party asset is introduced by this specification.
