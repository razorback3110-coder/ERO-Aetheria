#pragma once

#include "CoreMinimal.h"
#include "GameFramework/SaveGame.h"
#include "EROPlayerEconomyState.h"
#include "EROClassTypes.h"
#include "EROCharacterAppearanceTypes.h"
#include "EROPlayerEconomySaveGame.generated.h"

UCLASS()
class EROAETHERIA_API UEROPlayerEconomySaveGame final : public USaveGame
{
    GENERATED_BODY()

public:
    UPROPERTY()
    int32 SchemaVersion = 2;

    UPROPERTY()
    int64 GoldBalance = 0;

    UPROPERTY()
    TArray<FEROInventoryStack> Inventory;

    UPROPERTY()
    int32 CharacterLevel = 1;

    UPROPERTY()
    int64 CharacterExperience = 0;

    UPROPERTY()
    EEROPlayerClass CharacterClass = EEROPlayerClass::Warrior;

    UPROPERTY()
    FName EquippedWeaponId = NAME_None;

    UPROPERTY()
    EEROWeaponFamily EquippedWeaponFamily = EEROWeaponFamily::Sword;
};
