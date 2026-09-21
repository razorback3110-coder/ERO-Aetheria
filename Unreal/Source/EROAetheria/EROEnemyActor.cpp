#include "EROEnemyActor.h"

#include "EROPlayerCharacter.h"
#include "Components/CapsuleComponent.h"
#include "GameFramework/CharacterMovementComponent.h"
#include "Net/UnrealNetwork.h"

AEROEnemyActor::AEROEnemyActor()
{
    bReplicates = true;
    SetReplicateMovement(true);
    GetCapsuleComponent()->InitCapsuleSize(42.0f, 96.0f);
    GetCharacterMovement()->MaxWalkSpeed = 250.0f;
}

void AEROEnemyActor::BeginPlay()
{
    Super::BeginPlay();
    if (HasAuthority())
    {
        SpawnLocation = GetActorLocation();
        SpawnRotation = GetActorRotation();
        CurrentHealth = MaxHealth;
        bDefeated = false;
    }
}

void AEROEnemyActor::GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const
{
    Super::GetLifetimeReplicatedProps(OutLifetimeProps);
    DOREPLIFETIME(AEROEnemyActor, EnemyLevel);
    DOREPLIFETIME(AEROEnemyActor, CurrentHealth);
    DOREPLIFETIME(AEROEnemyActor, bDefeated);
}

float AEROEnemyActor::TakeDamage(float DamageAmount, const FDamageEvent& DamageEvent, AController* EventInstigator, AActor* DamageCauser)
{
    if (!HasAuthority() || bDefeated || DamageAmount <= 0.0f)
    {
        return 0.0f;
    }

    const float AppliedDamage = FMath::Clamp(DamageAmount, 0.0f, CurrentHealth);
    CurrentHealth -= AppliedDamage;

    if (CurrentHealth <= 0.0f)
    {
        CurrentHealth = 0.0f;
        bDefeated = true;
        SetActorEnableCollision(false);
        SetActorHiddenInGame(true);
        GetCharacterMovement()->DisableMovement();

        if (AEROPlayerCharacter* Player = Cast<AEROPlayerCharacter>(EventInstigator ? EventInstigator->GetPawn() : nullptr))
        {
            Player->GrantExperience(ExperienceReward);
        }

        GetWorldTimerManager().SetTimer(RespawnTimerHandle, this, &AEROEnemyActor::RespawnEnemy, RespawnDelay, false);
    }

    return AppliedDamage;
}

void AEROEnemyActor::RespawnEnemy()
{
    if (!HasAuthority())
    {
        return;
    }

    bDefeated = false;
    CurrentHealth = MaxHealth;
    SetActorLocationAndRotation(SpawnLocation, SpawnRotation, false, nullptr, ETeleportType::TeleportPhysics);
    SetActorHiddenInGame(false);
    SetActorEnableCollision(true);
    GetCharacterMovement()->SetMovementMode(MOVE_Walking);
}
