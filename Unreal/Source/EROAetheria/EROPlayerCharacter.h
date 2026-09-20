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

    UPROPERTY(Replicated, BlueprintReadOnly, Category="ERO|Character")
    EEROPlayerClass PlayerClass = EEROPlayerClass::Warrior;

    UPROPERTY(Replicated, BlueprintReadOnly, Category="ERO|Character")
    int32 Level = 1;

    UPROPERTY(Replicated, BlueprintReadOnly, Category="ERO|Combat")
    float CurrentHealth = 100.0f;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category="ERO|Combat")
    float MaxHealth = 100.0f;

protected:
    void MoveForward(float Value);
    void MoveRight(float Value);
    void LookUp(float Value);
    void Turn(float Value);
};
