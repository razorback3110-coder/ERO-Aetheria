#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Character.h"
#include "AbilitySystemInterface.h"
#include "EROClassTypes.h"
#include "EROCharacterVisualTypes.h"
#include "EROCharacterAppearanceTypes.h"
#include "EROPlayerCharacter.generated.h"

class UAbilitySystemComponent;
class UEROAttributeSet;

UCLASS()
class EROAETHERIA_API AEROPlayerCharacter final : public ACharacter, public IAbilitySystemInterface
{
    GENERATED_BODY()

public:
    AEROPlayerCharacter();

    virtual void GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const override;
    virtual void SetupPlayerInputComponent(UInputComponent* PlayerInputComponent) override;
    virtual float TakeDamage(float DamageAmount, const FDamageEvent& DamageEvent, AController* EventInstigator, AActor* DamageCauser) override;

    //~ Begin IAbilitySystemInterface
    virtual UAbilitySystemComponent* GetAbilitySystemComponent() const override;
    //~ End IAbilitySystemInterface

    UPROPERTY(VisibleDefaultsOnly, BlueprintReadOnly, Category="ERO|Abilities")
    TObjectPtr<UAbilitySystemComponent> AbilitySystemComponent;

    UPROPERTY(VisibleDefaultsOnly, BlueprintReadOnly, Category="ERO|Abilities")
    TObjectPtr<UEROAttributeSet> AttributeSet;

    UPROPERTY(Replicated, BlueprintReadOnly, Category="ERO|Character")
    EEROPlayerClass PlayerClass = EEROPlayerClass::Warrior;

    UPROPERTY(Replicated, BlueprintReadOnly, Category="ERO|Equipment")
    FName EquippedWeaponId = NAME_None;

    UPROPERTY(Replicated, BlueprintReadOnly, Category="ERO|Appearance")
    FName AppearanceId = TEXT("Warrior_Base");

    UPROPERTY(Replicated, BlueprintReadOnly, Category="ERO|Weapon")
    EEROWeaponFamily EquippedWeaponFamily = EEROWeaponFamily::Sword;

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

    UFUNCTION(BlueprintCallable, Category="ERO|Weapon")
    bool CanSelectWeaponSpecialization() const;

    UFUNCTION(Server, Reliable)
    void ServerSelectWeaponSpecialization(EEROWeaponFamily RequestedWeapon);

    void GrantExperience(int64 Amount);

    UFUNCTION(BlueprintCallable, Category="ERO|Equipment")
    bool CanEquipWeapon(FName WeaponId) const;

    UFUNCTION(Server, Reliable)
    void ServerSelectWeapon(FName RequestedWeaponId);

protected:
    virtual void BeginPlay() override;

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
    bool IsWeaponAllowedForClass(FName WeaponId) const;
    void ApplyClassAppearanceProfile();
    bool IsWeaponFamilyAllowed(EEROWeaponFamily WeaponFamily) const;
    void SyncAttributesFromLegacyProfile();
    void RespawnAfterDeath();
    int64 ExperienceForNextLevel() const;

    FTimerHandle RespawnTimerHandle;
    float LastAttackServerTime = -BIG_NUMBER;
};
