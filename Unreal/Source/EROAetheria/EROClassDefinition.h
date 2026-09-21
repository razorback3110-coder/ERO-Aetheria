#pragma once

#include "CoreMinimal.h"
#include "Engine/DataAsset.h"
#include "EROClassTypes.h"

class UEROClassAppearanceDefinition;
#include "EROClassDefinition.generated.h"

/**
 * Data-driven definition for an ERO class.
 *
 * Class rules are intentionally stored as assets rather than hard-coded
 * gameplay branches so the same data can drive character creation,
 * progression, UI, abilities and server validation.
 */
UCLASS(BlueprintType)
class UEROCharacterVisualDefinition;

UCLASS(BlueprintType)
class EROAETHERIA_API UEROClassDefinition : public UPrimaryDataAsset
{
    GENERATED_BODY()

public:
    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Class")
    EEROPlayerClass ClassId = EEROPlayerClass::Warrior;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Class")
    FText DisplayName;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Class", meta=(MultiLine=true))
    FText Description;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Class")
    FEROClassStats BaseStats;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Appearance")
    TSoftObjectPtr<UEROCharacterVisualDefinition> VisualDefinition;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Progression")
    FEROClassBranch SelectionBranch;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Progression")
    FEROClassBranch FirstEvolution;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Progression")
    FEROClassBranch SecondEvolution;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Progression")
    int32 ClassSelectionLevel = 18;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Progression")
    int32 FirstEvolutionLevel = 40;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Progression")
    int32 SecondEvolutionLevel = 75;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Abilities")
    TArray<TSoftClassPtr<class UGameplayAbility>> StartingAbilities;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Appearance")
    TSoftObjectPtr<UEROClassAppearanceDefinition> AppearanceDefinition;
};
