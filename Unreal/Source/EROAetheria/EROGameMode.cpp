#include "EROGameMode.h"
#include "EROPlayerCharacter.h"
#include "EROEnvironmentActor.h"
#include "EROEnemyActor.h"

AEROGameMode::AEROGameMode()
{
    bUseSeamlessTravel = true;
    DefaultPawnClass = AEROPlayerCharacter::StaticClass();
}

void AEROGameMode::BeginPlay()
{
    Super::BeginPlay();

    if (!HasAuthority())
    {
        return;
    }

    GetWorld()->SpawnActor<AEROEnvironmentActor>(FVector::ZeroVector, FRotator::ZeroRotator);

    // Deterministic PvE starter encounters. These are server-owned and can be
    // replaced later by the data-driven world/encounter director without
    // changing player combat code.
    const TArray<FVector> StarterEnemyLocations = {
        FVector(900.0f, 0.0f, 100.0f),
        FVector(1300.0f, 650.0f, 100.0f),
        FVector(1700.0f, -550.0f, 100.0f)
    };

    for (int32 Index = 0; Index < StarterEnemyLocations.Num(); ++Index)
    {
        FTransform SpawnTransform(FRotator::ZeroRotator, StarterEnemyLocations[Index]);
        AEROEnemyActor* Enemy = GetWorld()->SpawnActorDeferred<AEROEnemyActor>(AEROEnemyActor::StaticClass(), SpawnTransform);
        if (!Enemy)
        {
            continue;
        }

        Enemy->EnemyLevel = 1 + Index;
        Enemy->MaxHealth = 250.0f + (Index * 100.0f);
        Enemy->CurrentHealth = Enemy->MaxHealth;
        Enemy->ExperienceReward = 250 + (Index * 100);
        Enemy->RespawnDelay = 10.0f;
        Enemy->FinishSpawning(SpawnTransform);
    }
}
