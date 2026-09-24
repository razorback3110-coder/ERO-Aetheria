#!/usr/bin/env bash
set -euo pipefail

GAME_MODE="Unreal/Source/EROAetheria/EROGameMode.cpp"
GAME_MODE_HEADER="Unreal/Source/EROAetheria/EROGameMode.h"

for file in "$GAME_MODE" "$GAME_MODE_HEADER"; do
  test -f "$file"
done

grep -q 'PrimaryActorTick.bCanEverTick = true' "$GAME_MODE"
grep -q 'PrimaryActorTick.TickInterval = 1.0f' "$GAME_MODE"
grep -q 'EncounterActivationRadius' "$GAME_MODE_HEADER"
grep -q 'IsEncounterWithinActivationRadius' "$GAME_MODE"
grep -q 'GetPlayerControllerIterator' "$GAME_MODE"
grep -q 'ActivatedEncounterIds.Contains' "$GAME_MODE"
grep -q 'ActivatedEncounterIds.Add' "$GAME_MODE"

if grep -q 'SpawnConfiguredEncounters();' "$GAME_MODE" && ! grep -q 'IsEncounterWithinActivationRadius(Definition.Location)' "$GAME_MODE"; then
  echo "Encounter streaming gate missing from spawn path."
  exit 1
fi

echo "Encounter activation/streaming contract: OK"
