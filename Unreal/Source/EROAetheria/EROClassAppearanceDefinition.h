#pragma once

#include "CoreMinimal.h"
#include "Engine/DataAsset.h"
#include "EROCharacterAppearanceTypes.h"
#include "EROClassAppearanceDefinition.generated.h"

UCLASS(BlueprintType)
class EROAETHERIA_API UEROClassAppearanceDefinition : public UPrimaryDataAsset
{
    GENERATED_BODY()

public:
    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Class")
    EEROPlayerClass ClassId = EEROPlayerClass::Warrior;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Class")
    FText DisplayName;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Appearance")
    FEROClassAppearanceProfile BaseAppearance;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Appearance")
    FEROClassAppearanceProfile FirstEvolutionAppearance;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Appearance")
    FEROClassAppearanceProfile SecondEvolutionAppearance;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Assets")
    TSoftObjectPtr<USkeletalMesh> BodyMesh;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Assets")
    TSoftObjectPtr<UClass> AnimationBlueprint;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Assets")
    TArray<TSoftObjectPtr<UObject>> OutfitAssets;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Assets")
    TArray<TSoftObjectPtr<UObject>> WeaponAssets;
};
