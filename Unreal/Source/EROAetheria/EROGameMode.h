#pragma once

#include "CoreMinimal.h"
#include "GameFramework/GameModeBase.h"
#include "EROEncounterDirectorTypes.h"
#include "EROGameMode.generated.h"

class AEROEnemyActor;

UCLASS()
class EROAETHERIA_API AEROGameMode final : public AGameModeBase
{
    GENERATED_BODY()

public:
    AEROGameMode();

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|World|Encounters")
    TArray<FEROEncounterSpawnDefinition> StarterEncounters;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|World|Streaming", meta=(ClampMin="500.0", UIMin="500.0"))
    float EncounterActivationRadius = 6000.0f;

protected:
    virtual void BeginPlay() override;
    virtual void Tick(float DeltaSeconds) override;

private:
    void LoadEncounterPersistence();
    void SaveEncounterPersistence() const;
    void SpawnConfiguredEncounters();
    void UpdateEncounterStreamingState();
    bool IsEncounterWithinActivationRadius(const FVector& EncounterLocation) const;

    TSet<FName> ActivatedEncounterIds;
    TMap<FName, TWeakObjectPtr<AEROEnemyActor>> ActiveEncounterActors;
    TMap<FName, int64> PersistedRespawnDeadlinesUtc;
    bool bEncounterPersistenceDirty = false;
};
