#!/usr/bin/env bash
set -euo pipefail

python3 - <<'PY'
import json
import re
from pathlib import Path

manifest_path = Path("Unreal/Config/AetheriaMVPManifest.json")
game_mode_path = Path("Unreal/Source/EROAetheria/EROGameMode.cpp")

manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
source = game_mode_path.read_text(encoding="utf-8")

mvps = manifest.get("mvps", [])
expected_total = manifest.get("total")
if expected_total != len(mvps):
    raise SystemExit(f"MVP manifest total mismatch: total={expected_total}, entries={len(mvps)}")

expected = [(f"MVP_{index:02d}", int(entry["level"])) for index, entry in enumerate(mvps, start=1)]

pattern = re.compile(
    r'TEXT\\("(?P<id>MVP_\\d{2})"\\).*?Rotator::ZeroRotator,\\s*'
    r'(?P<level>\\d+),\\s*(?P<hp>[0-9.]+f),'
    re.DOTALL,
)
actual = [(match.group("id"), int(match.group("level"))) for match in pattern.finditer(source)]

if len(actual) != expected_total:
    raise SystemExit(f"Runtime MVP encounter count mismatch: expected {expected_total}, found {len(actual)}")

if actual != expected:
    for index, (want, got) in enumerate(zip(expected, actual), start=1):
        if want != got:
            raise SystemExit(f"Runtime MVP mismatch at #{index}: manifest={want}, runtime={got}")
    raise SystemExit("Runtime MVP ordering does not match manifest")

print(f"MVP manifest parity: OK ({expected_total} encounters)")
PY
