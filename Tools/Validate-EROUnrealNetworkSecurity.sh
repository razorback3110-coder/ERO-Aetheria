#!/usr/bin/env bash
set -euo pipefail

ROOT="${GITHUB_WORKSPACE:-$(pwd)}"
SRC="$ROOT/Unreal/Source/EROAetheria"

player_h="$SRC/EROPlayerCharacter.h"
player_cpp="$SRC/EROPlayerCharacter.cpp"
enemy_cpp="$SRC/EROEnemyActor.cpp"

for file in "$player_h" "$player_cpp" "$enemy_cpp"; do
  test -f "$file" || { echo "Missing authoritative combat source: $file"; exit 1; }
done

# Client combat input must cross a Server RPC boundary.
grep -Eq 'UFUNCTION\(Server, Reliable\)[[:space:]]*void ServerAttack\(\);' "$player_h"
grep -q 'void AEROPlayerCharacter::ServerAttack_Implementation()' "$player_cpp"

# The server-side attack must enforce the authoritative cooldown and reject defeated actors.
grep -q 'if (bDefeated || !CanAttack())' "$player_cpp"
grep -q 'LastAttackServerTime = GetWorld()->GetTimeSeconds();' "$player_cpp"

# Damage must be applied by the server and must flow through Unreal's damage interface.
grep -q 'UGameplayStatics::ApplyDamage' "$player_cpp"
grep -q 'if (!HasAuthority() || bDefeated || DamageAmount <= 0.0f)' "$enemy_cpp"

# PvE reward authority must validate the real ERO player and controller before granting XP/gold/items.
grep -q 'const AEROPlayerCharacter\* Attacker = Cast<AEROPlayerCharacter>(DamageCauser);' "$enemy_cpp"
grep -q 'EventInstigator != Attacker->GetController()' "$enemy_cpp"
grep -q 'Player->GrantExperience(ExperienceReward);' "$enemy_cpp"
grep -q 'EconomyState->GrantGold(GoldReward);' "$enemy_cpp"
grep -q 'EconomyState->GrantItem(ItemRewardId, ItemRewardQuantity);' "$enemy_cpp"

# Server-side range validation prevents forged remote damage from outside melee range.
grep -q 'MaxAttackDistance' "$enemy_cpp"
grep -q 'Delta.SizeSquared2D() > FMath::Square(MaxAttackDistance)' "$enemy_cpp"

echo "Unreal network/combat authority contract: OK"
