# ERO Audio & Music License Registry

This registry is the source-of-truth for music, ambience, sound effects and voice assets used by Eternal Realms Online.

## Rules

- Only use audio with an explicit license permitting commercial use in a distributed game.
- Prefer CC0/public-domain audio or licenses that clearly permit commercial game distribution.
- Never copy music, sound effects, voice lines or audio extracted from proprietary games.
- Do not redistribute source audio packs as standalone files.
- Record source URL, license, acquisition date/version and any attribution requirement for every imported asset.
- When a license is ambiguous, the asset is REVIEW and must not ship.

## Approved source families

### Kenney Digital Audio
- Source: https://www.kenney.nl/assets/digital-audio
- License: CC0 / public domain according to Kenney's official support and asset page.
- Commercial game use: APPROVED.
- Attribution: not required.
- Intended use: placeholder/interface/gameplay audio and prototypes; verify individual downloaded pack metadata before shipping.

### ERO Original Audio
- Source: commissioned or internally created ERO music/SFX/voice.
- License: ERO-owned or contractually assigned rights.
- Commercial game use: APPROVED only after the contract/right assignment is archived.

## Music production plan

ERO should use an original soundtrack rather than imitating another game's soundtrack. Planned adaptive layers:

- Aetheria Main Theme
- Greenhaven / pastoral
- Everwood / ancient forest
- Elyndor / royal-fantasy
- Frostfall / frozen ruins
- Sunscar / desert-war
- Lunareth / lunar mysticism
- Abyssia / dark corruption
- Eternal Rift / cosmic danger
- Town / sanctuary
- Dungeon / combat / boss
- PvP / GvG
- Victory / defeat / discovery
- Character creation / login / menu

Music should support looping, crossfades and combat-state transitions through Unity's audio system without requiring third-party proprietary content.

## Shipping gate

An audio file is SHIP only when its exact source and license are recorded here or in a linked asset manifest. Assets marked REVIEW or REJECTED must never be included in a release build.
