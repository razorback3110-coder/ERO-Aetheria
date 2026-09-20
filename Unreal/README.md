# ERO-Aetheria — Unreal Engine

This directory is the new runtime target for **Eternal Realms Online**.

## Engine

The project baseline is **Unreal Engine 5.8**. Epic's UE 5.8 documentation covers World Partition, Gameplay Ability System, Mass Entity/Mass Gameplay and dedicated-server workflows that match ERO's MMORPG requirements.

The existing Unity project remains in the repository temporarily as a reference/migration source. It is **not** the target runtime.

## Architecture

- World: World Partition + procedural generation/streaming.
- Combat/classes: Gameplay Ability System, server authoritative.
- Large populations: Mass Entity/Mass Gameplay where appropriate.
- Networking: dedicated-server first; clients never own authoritative combat/economy state.
- Persistence/economy: backend-compatible data contracts, independent of presentation.
- UI: UMG/CommonUI-compatible architecture.
- Assets: only assets with commercially verifiable licenses; every imported third-party asset must be recorded in Assets/ERO/Legal/ERO_Asset_License_Registry.md.

## First local action

Install Unreal Engine 5.8 through the Epic Games Launcher, then open:
Unreal/ERO-Aetheria.uproject

Generate Visual Studio project files when prompted. The repository can then be built and iterated locally; GitHub CI will be added once an Unreal-capable Windows runner is available.
