#include "EROGameMode.h"
#include "EROPlayerCharacter.h"
#include "EROPlayerEconomyState.h"
#include "EROEnvironmentActor.h"
#include "EROEnemyActor.h"

AEROGameMode::AEROGameMode()
{
    bUseSeamlessTravel = true;
    DefaultPawnClass = AEROPlayerCharacter::StaticClass();
    PlayerStateClass = AEROPlayerEconomyState::StaticClass();

    StarterEncounters = {
        { TEXT("Starter_Imp_01"), FVector(900.0f, 0.0f, 100.0f), FRotator::ZeroRotator, 1, 250.0f, 250, 10.0f },
        { TEXT("Starter_Imp_02"), FVector(1300.0f, 650.0f, 100.0f), FRotator::ZeroRotator, 2, 350.0f, 350, 10.0f },
        { TEXT("Starter_Imp_03"), FVector(1700.0f, -550.0f, 100.0f), FRotator::ZeroRotator, 3, 450.0f, 450, 10.0f }
    };
}

void AEROGameMode::BeginPlay()
{
    Super::BeginPlay();

    if (!HasAuthority())
    {
        return;
    }

    GetWorld()->SpawnActor<AEROEnvironmentActor>(FVector::ZeroVector, FRotator::ZeroRotator);
    SpawnConfiguredEncounters();
}

void AEROGameMode::SpawnConfiguredEncounters()
{
    if (!GetWorld())
    {
        return;
    }

    for (const FEROEncounterSpawnDefinition& Definition : StarterEncounters)
    {
        if (!Definition.IsValid())
        {
            continue;
        }

        const FTransform SpawnTransform(Definition.Rotation, Definition.Location);
        AEROEnemyActor* Enemy = GetWorld()->SpawnActorDeferred<AEROEnemyActor>(AEROEnemyActor::StaticClass(), SpawnTransform);
        if (!Enemy)
        {
            continue;
        }

        Enemy->EnemyLevel = Definition.EnemyLevel;
        Enemy->MaxHealth = Definition.MaxHealth;
        Enemy->CurrentHealth = Definition.MaxHealth;
        Enemy->ExperienceReward = Definition.ExperienceReward;
        Enemy->GoldReward = FMath::Max<int64>(1, static_cast<int64>(Definition.EnemyLevel) * 25);
        Enemy->RespawnDelay = Definition.RespawnDelay;
        Enemy->FinishSpawning(SpawnTransform);
    }
}
