#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Character.h"
#include "EROPlayerCharacter.generated.h"

UENUM(BlueprintType)
enum class EEROPlayerClass : uint8
{
    Warrior,
    Ranger,
    Mage,
    Assassin,
    Cleric,
    Paladin,
    Warlock,
    Summoner
};

UCLASS()
class EROAETHERIA_API AEROPlayerCharacter final : public ACharacter
{
    GENERATED_BODY()

public:
    AEROPlayerCharacter();

    virtual void GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const override;
    virtual void SetupPlayerInputComponent(UInputComponent* PlayerInputComponent) override;
    virtual float TakeDamage(float DamageAmount, const FDamageEvent& DamageEvent, AController* EventInstigator, AActor* DamageCauser) override;

    UPROPERTY(Replicated, BlueprintReadOnly, Category="ERO|Character")
    EEROPlayerClass PlayerClass = EEROPlayerClass::Warrior;

    UPROPERTY(Replicated, BlueprintReadOnly, Category="ERO|Character")
    int32 Level = 1;

    UPROPERTY(Replicated, BlueprintReadOnly, Category="ERO|Character")
    int64 Experience = 0;

    UPROPERTY(Replicated, BlueprintReadOnly, Category="ERO|Character")
    bool bDefeated = false;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Character")
    float RespawnDelay = 5.0f;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Combat")
    float MaxHealth = 100.0f;

    UPROPERTY(Replicated, BlueprintReadOnly, Category="ERO|Combat")
    float CurrentHealth = 100.0f;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Combat")
    float AttackDamage = 25.0f;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Combat")
    float AttackRange = 300.0f;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Combat")
    float AttackCooldown = 0.5f;

    UFUNCTION(BlueprintCallable, Category="ERO|Character")
    bool CanSelectClass() const;

    UFUNCTION(Server, Reliable)
    void ServerSelectClass(EEROPlayerClass RequestedClass);

    UFUNCTION(Server, Reliable)
    void ServerGrantExperience(int64 Amount);

protected:
    void MoveForward(float Value);
    void MoveRight(float Value);
    void LookUp(float Value);
    void Turn(float Value);

    void Attack();

    UFUNCTION(Server, Reliable)
    void ServerAttack();

    bool CanAttack() const;
    void ResetAttackCooldown();
    void ApplyClassProfile();
    void RespawnAfterDeath();
    int64 ExperienceForNextLevel() const;

    FTimerHandle RespawnTimerHandle;
    float LastAttackServerTime = -BIG_NUMBER;
};
