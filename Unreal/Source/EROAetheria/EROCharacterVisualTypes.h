#pragma once

#include "CoreMinimal.h"
#include "EROClassTypes.h"
#include "EROCharacterVisualTypes.generated.h"

UENUM(BlueprintType)
enum class EEROWeaponFamily : uint8
{
    Sword,
    Greatsword,
    Axe,
    Mace,
    Shield,
    Bow,
    Crossbow,
    Dagger,
    DualBlades,
    Staff,
    Wand,
    Grimoire,
    Orb,
    Scythe,
    Spear
};

UENUM(BlueprintType)
enum class EEROWeaponSpecialization : uint8
{
    Vanguard,
    Guardian,
    Berserker,
    Marksman,
    Trapper,
    Sharpshooter,
    Spellblade,
    Arcanist,
    Elementalist,
    Shadow,
    Venom,
    Executioner,
    Holy,
    Sanctuary,
    Judgement,
    Pact,
    Hex,
    Soulfire,
    Summoner,
    Beastmaster,
    Conjurer
};

USTRUCT(BlueprintType)
struct EROAETHERIA_API FEROWeaponOption
{
    GENERATED_BODY()

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Weapon")
    FName WeaponId;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Weapon")
    FText DisplayName;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Weapon")
    EEROWeaponFamily Family = EEROWeaponFamily::Sword;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Weapon")
    EEROWeaponSpecialization Specialization = EEROWeaponSpecialization::Vanguard;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Weapon")
    int32 UnlockLevel = 18;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Weapon")
    FName VisualSetId;
};

USTRUCT(BlueprintType)
struct EROAETHERIA_API FEROCharacterVisualProfile
{
    GENERATED_BODY()

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Appearance")
    FName VisualProfileId;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Appearance")
    FText OutfitName;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Appearance")
    FText SilhouetteDescription;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Appearance")
    FName BodyArchetypeId;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Appearance")
    FName ArmorSetId;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Appearance")
    FName HelmetSetId;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Appearance")
    FName VfxSetId;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Weapon")
    TArray<FEROWeaponOption> WeaponOptions;
};
