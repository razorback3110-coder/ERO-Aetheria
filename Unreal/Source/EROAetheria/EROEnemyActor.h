#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Character.h"
#include "EROEnemyActor.generated.h"

UCLASS()
class EROAETHERIA_API AEROEnemyActor : public ACharacter
{
    GENERATED_BODY()

public:
    AEROEnemyActor();

    virtual float TakeDamage(float DamageAmount, const FDamageEvent& DamageEvent, AController* EventInstigator, AActor* DamageCauser) override;

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category="ERO|Enemy")
    int32 EnemyLevel = 1;

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category="ERO|Enemy")
    float MaxHealth = 250.0f;

    UPROPERTY(VisibleInstanceOnly, BlueprintReadOnly, Category="ERO|Enemy")
    float CurrentHealth = 250.0f;

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category="ERO|Reward")
    int64 ExperienceReward = 250;

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category="ERO|Enemy")
    float RespawnDelay = 10.0f;

protected:
    virtual void BeginPlay() override;

private:
    void RespawnEnemy();
    FTimerHandle RespawnTimerHandle;
    FVector SpawnLocation = FVector::ZeroVector;
    FRotator SpawnRotation = FRotator::ZeroRotator;
    bool bDefeated = false;
};
