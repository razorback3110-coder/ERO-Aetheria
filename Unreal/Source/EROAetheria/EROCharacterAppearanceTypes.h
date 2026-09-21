#pragma once

#include "CoreMinimal.h"
#include "EROClassTypes.h"
#include "EROCharacterAppearanceTypes.generated.h"

UENUM(BlueprintType)
enum class EEROWeaponFamily : uint8
{
    Sword,
    Greatsword,
    Axe,
    Bow,
    Crossbow,
    Staff,
    Wand,
    Dagger,
    Mace,
    Shield,
    Grimoire,
    Orb,
    Spear,
    Fist,
    DualBlades
};

UENUM(BlueprintType)
enum class EEROArmorStyle : uint8
{
    Vanguard,
    Ranger,
    Arcanist,
    Shadow,
    Acolyte,
    Templar,
    Hexbinder,
    Conjurer
};

USTRUCT(BlueprintType)
struct EROAETHERIA_API FEROWeaponSpecialization
{
    GENERATED_BODY()

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Weapon")
    FName SpecializationId;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Weapon")
    FText DisplayName;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Weapon")
    EEROWeaponFamily WeaponFamily = EEROWeaponFamily::Sword;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Weapon")
    FText Description;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Weapon")
    bool bPrimary = false;
};

USTRUCT(BlueprintType)
struct EROAETHERIA_API FEROClassAppearanceProfile
{
    GENERATED_BODY()

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Appearance")
    FName AppearanceId;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Appearance")
    EEROArmorStyle ArmorStyle = EEROArmorStyle::Vanguard;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Appearance")
    FName OutfitId;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Appearance")
    FName WeaponSetId;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Appearance")
    FName SilhouetteId;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Appearance")
    TArray<FName> CosmeticSlots;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Appearance")
    TArray<FEROWeaponSpecialization> WeaponSpecializations;
};
