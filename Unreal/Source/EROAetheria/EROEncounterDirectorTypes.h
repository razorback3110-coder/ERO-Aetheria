#pragma once

#include "CoreMinimal.h"
#include "EROEncounterDirectorTypes.generated.h"

USTRUCT(BlueprintType)
struct EROAETHERIA_API FEROEncounterSpawnDefinition
{
    GENERATED_BODY()

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category="ERO|Encounter")
    FName EncounterId = NAME_None;

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category="ERO|Encounter")
    FVector Location = FVector::ZeroVector;

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category="ERO|Encounter")
    FRotator Rotation = FRotator::ZeroRotator;

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category="ERO|Encounter")
    int32 EnemyLevel = 1;

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category="ERO|Encounter")
    float MaxHealth = 250.0f;

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category="ERO|Encounter")
    int64 ExperienceReward = 250;

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category="ERO|Encounter")
    float RespawnDelay = 10.0f;

    bool IsValid() const
    {
        return !EncounterId.IsNone() && EnemyLevel > 0 && MaxHealth > 0.0f && ExperienceReward >= 0 && RespawnDelay >= 0.0f;
    }
};
