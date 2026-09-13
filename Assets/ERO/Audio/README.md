# ERO Audio Pipeline

The audio folder is prepared for automatic ingestion of approved music, ambience, SFX and voice assets.

Expected categories:

- `Music/Menu`
- `Music/World`
- `Music/Combat`
- `Music/Boss`
- `Music/PvP`
- `Music/Guild`
- `Music/Dungeons`
- `Ambience/`
- `SFX/Combat`
- `SFX/UI`
- `SFX/World`
- `Voice/`

Imported audio must be listed in `Assets/ERO/Legal/ERO_Audio_License_Registry.md` before it can be considered shippable.

The runtime design uses adaptive music states so exploration, combat, elite encounters and bosses can transition without hard cuts.
