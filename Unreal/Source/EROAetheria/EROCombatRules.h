#pragma once

#include "CoreMinimal.h"
#include "EROCharacterAppearanceTypes.h"

/**
 * Deterministic, server-side combat modifiers for ERO weapon families.
 * Values are intentionally conservative until full GAS weapon data assets exist.
 */
namespace EROCombatRules
{
    FORCEINLINE float GetDamageMultiplier(EEROWeaponFamily WeaponFamily)
    {
        switch (WeaponFamily)
        {
        case EEROWeaponFamily::Greatsword: return 1.15f;
        case EEROWeaponFamily::Axe: return 1.10f;
        case EEROWeaponFamily::Sword: return 1.05f;
        case EEROWeaponFamily::Bow: return 1.08f;
        case EEROWeaponFamily::Crossbow: return 1.12f;
        case EEROWeaponFamily::Dagger: return 1.03f;
        case EEROWeaponFamily::DualBlades: return 1.06f;
        case EEROWeaponFamily::Staff: return 1.10f;
        case EEROWeaponFamily::Wand: return 1.05f;
        case EEROWeaponFamily::Grimoire: return 1.12f;
        case EEROWeaponFamily::Orb: return 1.08f;
        case EEROWeaponFamily::Mace: return 1.04f;
        case EEROWeaponFamily::Spear: return 1.07f;
        case EEROWeaponFamily::Fist: return 0.98f;
        case EEROWeaponFamily::Shield: return 0.90f;
        case EEROWeaponFamily::Scythe: return 1.14f;
        default: return 1.0f;
        }
    }

    FORCEINLINE float GetRange(EEROWeaponFamily WeaponFamily, float BaseRange)
    {
        switch (WeaponFamily)
        {
        case EEROWeaponFamily::Bow:
        case EEROWeaponFamily::Crossbow:
            return FMath::Max(BaseRange, 900.0f);
        case EEROWeaponFamily::Staff:
        case EEROWeaponFamily::Wand:
        case EEROWeaponFamily::Grimoire:
        case EEROWeaponFamily::Orb:
            return FMath::Max(BaseRange, 650.0f);
        case EEROWeaponFamily::Spear:
            return FMath::Max(BaseRange, 420.0f);
        default:
            return BaseRange;
        }
    }

    FORCEINLINE float GetCooldown(EEROWeaponFamily WeaponFamily, float BaseCooldown)
    {
        switch (WeaponFamily)
        {
        case EEROWeaponFamily::Greatsword:
        case EEROWeaponFamily::Axe:
        case EEROWeaponFamily::Scythe:
            return FMath::Max(BaseCooldown, 0.75f);
        case EEROWeaponFamily::Dagger:
        case EEROWeaponFamily::DualBlades:
        case EEROWeaponFamily::Fist:
            return FMath::Min(BaseCooldown, 0.35f);
        case EEROWeaponFamily::Bow:
        case EEROWeaponFamily::Crossbow:
            return FMath::Max(BaseCooldown, 0.65f);
        default:
            return BaseCooldown;
        }
    }
}
