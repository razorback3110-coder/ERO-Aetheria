#!/usr/bin/env bash
set -euo pipefail

ROOT="${GITHUB_WORKSPACE:-$(pwd)}"
SRC="$ROOT/Unreal/Source/EROAetheria"

required_files=(
  "$SRC/EROPlayerCharacter.h"
  "$SRC/EROPlayerCharacter.cpp"
  "$SRC/EROEnemyActor.h"
  "$SRC/EROEnemyActor.cpp"
  "$SRC/EROGameMode.h"
  "$SRC/EROGameMode.cpp"
  "$SRC/EROEncounterDirectorTypes.h"
  "$SRC/EROPlayerEconomyState.h"
  "$SRC/EROPlayerEconomyState.cpp"
  "$SRC/EROPlayerEconomySaveGame.h"
)

for file in "${required_files[@]}"; do
  test -f "$file" || { echo "Missing gameplay foundation file: $file"; exit 1; }
done

player="$SRC/EROPlayerCharacter.h"
enemy="$SRC/EROEnemyActor.h"
gamemode="$SRC/EROGameMode.cpp"
economy="$SRC/EROPlayerEconomyState.cpp"
economy_header="$SRC/EROPlayerEconomyState.h"
savegame="$SRC/EROPlayerEconomySaveGame.h"

grep -q 'ServerAttack' "$player"
grep -q 'GrantExperience' "$player"
grep -q 'CurrentHealth' "$player"
grep -q 'ServerSelectClass' "$player"
grep -q 'RestorePersistentProgression' "$player"
grep -q 'Replicated' "$enemy"
grep -q 'TakeDamage' "$enemy"
grep -q 'RespawnDelay' "$enemy"
grep -q 'StarterEncounters' "$gamemode"
grep -q 'SpawnConfiguredEncounters' "$gamemode"
grep -q 'SavePersistentEconomyState' "$economy_header"
grep -q 'LoadPersistentEconomyState' "$economy"
grep -q 'RestorePersistentCharacterState' "$economy"
grep -q 'SaveGameToSlot' "$economy"
grep -q 'LoadGameFromSlot' "$economy"
grep -q 'SchemaVersion = 2' "$savegame"
grep -q 'CharacterLevel' "$savegame"
grep -q 'CharacterExperience' "$savegame"
grep -q 'CharacterClass' "$savegame"
grep -q 'EquippedWeaponId' "$savegame"
grep -q 'EquippedWeaponFamily' "$savegame"
grep -q 'SetTimer' "$economy"
grep -q 'ClearTimer' "$economy"
grep -q 'EconomyCheckpointIntervalSeconds' "$economy"
grep -q 'CharacterRestoreDelaySeconds' "$economy"

echo "Unreal gameplay contract: OK"
