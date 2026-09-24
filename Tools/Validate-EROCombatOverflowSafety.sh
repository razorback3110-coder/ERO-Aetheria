#!/usr/bin/env bash
set -euo pipefail

resolver='Assets/ERO/Core/EROCombatResolver.cs'
test -f "$resolver"

grep -q 'long scaled = (long)value \* basisPoints;' "$resolver"
grep -q 'long rounded = (scaled + 9999L) / 10000L;' "$resolver"
grep -q 'rounded >= int.MaxValue ? int.MaxValue' "$resolver"

# The authoritative resolver must clamp scaled damage before converting back to int.
# This prevents large-but-valid MMO stat combinations from wrapping into negative damage.
if grep -q 'return (int)Math.Max(1L, (scaled + 9999L) / 10000L);' "$resolver"; then
  echo 'Unsafe unchecked combat damage narrowing detected.'
  exit 1
fi

echo 'Combat overflow safety: OK'
