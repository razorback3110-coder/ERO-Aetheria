#!/usr/bin/env bash
set -euo pipefail

SOURCE="Unreal/Source/EROAetheria/EROGameMode.cpp"

if [[ ! -f "$SOURCE" ]]; then
  echo "ERROR: missing $SOURCE" >&2
  exit 1
fi

# Persistence writes are staged to TempPath and must then be promoted to SavePath.
# A reversed MoveFile silently leaves the durable state stale after a restart.
if grep -Fq 'PlatformFile.MoveFile(*SavePath, *TempPath)' "$SOURCE"; then
  echo "ERROR: encounter persistence atomic write is reversed in $SOURCE" >&2
  echo "Expected MoveFile(*TempPath, *SavePath)." >&2
  exit 1
fi

if ! grep -Fq 'PlatformFile.MoveFile(*TempPath, *SavePath)' "$SOURCE"; then
  echo "ERROR: encounter persistence must promote TempPath to SavePath atomically." >&2
  exit 1
fi

echo "ERO encounter persistence atomic write direction validated."
