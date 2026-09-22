#pragma once

#include "CoreMinimal.h"
#include "Engine/DataAsset.h"
#include "Animation/AnimInstance.h"
#include "EROCharacterVisualTypes.h"
#include "EROCharacterVisualDefinition.generated.h"

/**
 * Data-driven presentation contract for a playable class.
 * Asset references remain optional so the project can compile before final
 * commercial character meshes, materials, animations and VFX are imported.
 */
UCLASS(BlueprintType)
class EROAETHERIA_API UEROCharacterVisualDefinition : public UPrimaryDataAsset
{
    GENERATED_BODY()

public:
    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Class")
    EEROPlayerClass ClassId = EEROPlayerClass::Warrior;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Appearance")
    FEROCharacterVisualProfile VisualProfile;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Appearance")
    TSoftObjectPtr<USkeletalMesh> CharacterMesh;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Appearance")
    TSoftObjectPtr<USkeletalMesh> ArmorMesh;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Appearance")
    TSoftObjectPtr<USkeletalMesh> HelmetMesh;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Appearance")
    TSoftObjectPtr<USkeletalMesh> HairMesh;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Animation")
    TSoftClassPtr<UAnimInstance> AnimationClass;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Weapon")
    TMap<FName, TSoftObjectPtr<UStaticMesh>> WeaponMeshes;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|VFX")
    TArray<TSoftObjectPtr<class UNiagaraSystem>> ClassVFX;
};
