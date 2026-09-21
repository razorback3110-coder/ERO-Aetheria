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
        { TEXT("Starter_Imp_01"), FVector(900.0f, 0.0f, 100.0f), FRotator::ZeroRotator, 1, 250.0f, 250, 25, TEXT("Aetherium_Shard"), 1, 10.0f, false },
        { TEXT("Starter_Imp_02"), FVector(1300.0f, 650.0f, 100.0f), FRotator::ZeroRotator, 2, 350.0f, 350, 50, TEXT("Aetherium_Shard"), 1, 10.0f, false },
        { TEXT("Starter_Imp_03"), FVector(1700.0f, -550.0f, 100.0f), FRotator::ZeroRotator, 3, 450.0f, 450, 75, TEXT("Aetherium_Shard"), 1, 10.0f, false },
        { TEXT("Aetheria_MVP_01"), FVector(4200.0f, 0.0f, 100.0f), FRotator::ZeroRotator, 40, 250000.0f, 25000, 50000, TEXT("MVP_Necklace_Aetheria"), 1, 3600.0f, true }
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
        const float MvpScale = Definition.bIsMVP
            ? 1.0f + 0.15f * static_cast<float>(FMath::Max(0, Definition.EnemyLevel - 1))
            : 1.0f;
        Enemy->MaxHealth = FMath::Max(1.0f, Definition.MaxHealth * MvpScale);
        Enemy->CurrentHealth = Enemy->MaxHealth;
        Enemy->ExperienceReward = Definition.ExperienceReward;
        Enemy->GoldReward = Definition.GoldReward;
        Enemy->ItemRewardId = Definition.ItemRewardId;
        Enemy->ItemRewardQuantity = FMath::Clamp(Definition.ItemRewardQuantity, 0, 9999);
        Enemy->RespawnDelay = Definition.RespawnDelay;
        Enemy->FinishSpawning(SpawnTransform);
    }
}
