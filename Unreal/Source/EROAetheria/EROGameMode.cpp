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

        { TEXT("Aetheria_MVP_01"), FVector(4200.0f, 0.0f, 100.0f), FRotator::ZeroRotator, 40, 250000.0f, 25000, 50000, TEXT("MVP_Necklace_Aetheria"), 1, 3600.0f, true },
        { TEXT("Valoria_MVP_01"), FVector(0.0f, 7300.0f, 100.0f), FRotator::ZeroRotator, 20, 125000.0f, 10000, 20000, TEXT("MVP_Necklace_Valoria"), 1, 3600.0f, true },
        { TEXT("Valoria_MVP_02"), FVector(4200.0f, 8800.0f, 100.0f), FRotator::ZeroRotator, 25, 160000.0f, 12500, 25000, TEXT("MVP_Necklace_Valoria_Elite"), 1, 3600.0f, true },
        { TEXT("Elderglen_MVP_01"), FVector(-7200.0f, 0.0f, 100.0f), FRotator::ZeroRotator, 35, 210000.0f, 18000, 35000, TEXT("MVP_Necklace_Elderglen"), 1, 3600.0f, true },
        { TEXT("Elderglen_MVP_02"), FVector(-10500.0f, 2800.0f, 100.0f), FRotator::ZeroRotator, 40, 250000.0f, 22000, 45000, TEXT("MVP_Necklace_Elderglen_Elite"), 1, 3600.0f, true },
        { TEXT("Sunscar_MVP_01"), FVector(7200.0f, 0.0f, 100.0f), FRotator::ZeroRotator, 50, 320000.0f, 30000, 60000, TEXT("MVP_Necklace_Sunscar"), 1, 3600.0f, true },
        { TEXT("Sunscar_MVP_02"), FVector(10400.0f, -2500.0f, 100.0f), FRotator::ZeroRotator, 55, 380000.0f, 36000, 70000, TEXT("MVP_Necklace_Sunscar_Elite"), 1, 3600.0f, true },
        { TEXT("Frostheim_MVP_01"), FVector(0.0f, -7200.0f, 100.0f), FRotator::ZeroRotator, 75, 520000.0f, 50000, 90000, TEXT("MVP_Necklace_Frostheim"), 1, 3600.0f, true },
        { TEXT("Frostheim_MVP_02"), FVector(3200.0f, -9400.0f, 100.0f), FRotator::ZeroRotator, 80, 600000.0f, 58000, 105000, TEXT("MVP_Necklace_Frostheim_Elite"), 1, 3600.0f, true },
        { TEXT("Mirehaven_MVP_01"), FVector(-7200.0f, -7200.0f, 100.0f), FRotator::ZeroRotator, 100, 850000.0f, 80000, 140000, TEXT("MVP_Necklace_Mirehaven"), 1, 3600.0f, true },
        { TEXT("Mirehaven_MVP_02"), FVector(-10400.0f, -9500.0f, 100.0f), FRotator::ZeroRotator, 105, 950000.0f, 90000, 155000, TEXT("MVP_Necklace_Mirehaven_Elite"), 1, 3600.0f, true },
        { TEXT("Arkenfall_MVP_01"), FVector(7200.0f, 7200.0f, 100.0f), FRotator::ZeroRotator, 125, 1200000.0f, 115000, 200000, TEXT("MVP_Necklace_Arkenfall"), 1, 3600.0f, true },
        { TEXT("Arkenfall_MVP_02"), FVector(10400.0f, 9500.0f, 100.0f), FRotator::ZeroRotator, 130, 1350000.0f, 130000, 220000, TEXT("MVP_Necklace_Arkenfall_Elite"), 1, 3600.0f, true },
        { TEXT("Astralis_MVP_01"), FVector(-14400.0f, 0.0f, 100.0f), FRotator::ZeroRotator, 150, 1600000.0f, 150000, 260000, TEXT("MVP_Necklace_Astralis"), 1, 3600.0f, true },
        { TEXT("Astralis_MVP_02"), FVector(-17600.0f, 3000.0f, 100.0f), FRotator::ZeroRotator, 155, 1800000.0f, 165000, 285000, TEXT("MVP_Necklace_Astralis_Elite"), 1, 3600.0f, true },
        { TEXT("Duskmoor_MVP_01"), FVector(14400.0f, 0.0f, 100.0f), FRotator::ZeroRotator, 165, 2000000.0f, 180000, 310000, TEXT("MVP_Necklace_Duskmoor"), 1, 3600.0f, true },
        { TEXT("Duskmoor_MVP_02"), FVector(17600.0f, -3000.0f, 100.0f), FRotator::ZeroRotator, 170, 2250000.0f, 205000, 340000, TEXT("MVP_Necklace_Duskmoor_Elite"), 1, 3600.0f, true },
        { TEXT("Abyssia_MVP_01"), FVector(0.0f, -14400.0f, 100.0f), FRotator::ZeroRotator, 175, 2500000.0f, 230000, 380000, TEXT("MVP_Necklace_Abyssia"), 1, 3600.0f, true },
        { TEXT("Abyssia_MVP_02"), FVector(3500.0f, -17600.0f, 100.0f), FRotator::ZeroRotator, 180, 2800000.0f, 260000, 420000, TEXT("MVP_Necklace_Abyssia_Elite"), 1, 3600.0f, true },
        { TEXT("Drakoria_MVP_01"), FVector(0.0f, 14400.0f, 100.0f), FRotator::ZeroRotator, 200, 3200000.0f, 300000, 500000, TEXT("MVP_Necklace_Drakoria"), 1, 3600.0f, true },
        { TEXT("Drakoria_MVP_02"), FVector(-3500.0f, 17600.0f, 100.0f), FRotator::ZeroRotator, 205, 3600000.0f, 340000, 560000, TEXT("MVP_Necklace_Drakoria_Elite"), 1, 3600.0f, true }
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
