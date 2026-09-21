#pragma once

#include "CoreMinimal.h"
#include "EROClassTypes.generated.h"

UENUM(BlueprintType)
enum class EEROPlayerClass : uint8
{
    Warrior UMETA(DisplayName="Warrior"),
    Ranger UMETA(DisplayName="Ranger"),
    Mage UMETA(DisplayName="Mage"),
    Assassin UMETA(DisplayName="Assassin"),
    Cleric UMETA(DisplayName="Cleric"),
    Paladin UMETA(DisplayName="Paladin"),
    Warlock UMETA(DisplayName="Warlock"),
    Summoner UMETA(DisplayName="Summoner")
};

USTRUCT(BlueprintType)
struct EROAETHERIA_API FEROClassBranch
{
    GENERATED_BODY()

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category="ERO|Class")
    FText Name;

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category="ERO|Class")
    FText Description;

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category="ERO|Class")
    int32 UnlockLevel = 1;
};

USTRUCT(BlueprintType)
struct EROAETHERIA_API FEROClassStats
{
    GENERATED_BODY()

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category="ERO|Stats")
    float Health = 100.0f;

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category="ERO|Stats")
    float Mana = 100.0f;

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category="ERO|Stats")
    float Attack = 25.0f;

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category="ERO|Stats")
    float MagicAttack = 25.0f;

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category="ERO|Stats")
    float Defense = 10.0f;

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category="ERO|Stats")
    float MagicDefense = 10.0f;

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category="ERO|Stats")
    float CritChance = 0.05f;

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category="ERO|Stats")
    float MoveSpeed = 500.0f;
};
