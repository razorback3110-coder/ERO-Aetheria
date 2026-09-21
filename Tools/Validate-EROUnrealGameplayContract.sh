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
)

for file in "${required_files[@]}"; do
  test -f "$file" || { echo "Missing gameplay foundation file: $file"; exit 1; }
done

player="$SRC/EROPlayerCharacter.h"
enemy="$SRC/EROEnemyActor.h"
gamemode="$SRC/EROGameMode.cpp"

grep -q 'ServerAttack' "$player"
grep -q 'GrantExperience' "$player"
grep -q 'CurrentHealth' "$player"
grep -q 'ServerSelectClass' "$player"
grep -q 'Replicated' "$enemy"
grep -q 'TakeDamage' "$enemy"
grep -q 'RespawnDelay' "$enemy"
grep -q 'StarterEncounters' "$gamemode"
grep -q 'SpawnConfiguredEncounters' "$gamemode"

echo "Unreal gameplay contract: OK"
