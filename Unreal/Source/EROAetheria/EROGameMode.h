#pragma once

#include "CoreMinimal.h"
#include "GameFramework/GameModeBase.h"
#include "EROEncounterDirectorTypes.h"
#include "EROGameMode.generated.h"

UCLASS()
class EROAETHERIA_API AEROGameMode final : public AGameModeBase
{
    GENERATED_BODY()

public:
    AEROGameMode();

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|World|Encounters")
    TArray<FEROEncounterSpawnDefinition> StarterEncounters;

protected:
    virtual void BeginPlay() override;

private:
    void SpawnConfiguredEncounters();
};
