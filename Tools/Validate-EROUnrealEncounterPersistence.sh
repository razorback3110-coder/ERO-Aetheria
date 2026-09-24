#!/usr/bin/env bash
set -euo pipefail

build_file="Unreal/Source/EROAetheria/EROAetheria.Build.cs"
game_mode_h="Unreal/Source/EROAetheria/EROGameMode.h"
game_mode_cpp="Unreal/Source/EROAetheria/EROGameMode.cpp"

for file in "$build_file" "$game_mode_h" "$game_mode_cpp"; do
  test -f "$file"
done

grep -q '"Json"' "$build_file"
grep -q 'LoadEncounterPersistence' "$game_mode_h"
grep -q 'SaveEncounterPersistence' "$game_mode_h"
grep -q 'PersistedRespawnDeadlinesUtc' "$game_mode_h"
grep -q 'FPaths::ProjectSavedDir' "$game_mode_cpp"
grep -q 'EROEncounterState.json' "$game_mode_cpp"
grep -q 'FJsonSerializer::Deserialize' "$game_mode_cpp"
grep -q 'FJsonSerializer::Serialize' "$game_mode_cpp"
grep -q 'bEncounterPersistenceDirty' "$game_mode_cpp"
grep -q 'RespawnDelay' "$game_mode_cpp"
# A live actor owns its in-process respawn timer; once it has respawned,
# the persisted deadline must be cleared or a stale cooldown survives forever.
grep -q 'PersistedRespawnDeadlinesUtc.Remove(It.Key())' "$game_mode_cpp"
grep -q 'if (Enemy->bDefeated)' "$game_mode_cpp"

echo "Encounter persistence contract: OK"
