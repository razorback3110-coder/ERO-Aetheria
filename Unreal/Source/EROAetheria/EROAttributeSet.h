#pragma once

#include "CoreMinimal.h"
#include "AttributeSet.h"
#include "AbilitySystemComponent.h"
#include "EROAttributeSet.generated.h"

#define ERO_ATTRIBUTE_ACCESSORS(ClassName, PropertyName) \
GAMEPLAYATTRIBUTE_PROPERTY_GETTER(ClassName, PropertyName) \
GAMEPLAYATTRIBUTE_VALUE_GETTER(PropertyName) \
GAMEPLAYATTRIBUTE_VALUE_SETTER(PropertyName) \
GAMEPLAYATTRIBUTE_VALUE_INITTER(PropertyName)

/**
 * Authoritative RPG attributes for ERO.
 * Gameplay Ability System will become the common source of truth for
 * combat, buffs/debuffs, progression modifiers and server validation.
 */
UCLASS()
class EROAETHERIA_API UEROAttributeSet : public UAttributeSet
{
    GENERATED_BODY()

public:
    UPROPERTY(BlueprintReadOnly, ReplicatedUsing=OnRep_Health, Category="ERO|Attributes")
    FGameplayAttributeData Health;
    ERO_ATTRIBUTE_ACCESSORS(UEROAttributeSet, Health)

    UPROPERTY(BlueprintReadOnly, ReplicatedUsing=OnRep_MaxHealth, Category="ERO|Attributes")
    FGameplayAttributeData MaxHealth;
    ERO_ATTRIBUTE_ACCESSORS(UEROAttributeSet, MaxHealth)

    UPROPERTY(BlueprintReadOnly, ReplicatedUsing=OnRep_Mana, Category="ERO|Attributes")
    FGameplayAttributeData Mana;
    ERO_ATTRIBUTE_ACCESSORS(UEROAttributeSet, Mana)

    UPROPERTY(BlueprintReadOnly, ReplicatedUsing=OnRep_MaxMana, Category="ERO|Attributes")
    FGameplayAttributeData MaxMana;
    ERO_ATTRIBUTE_ACCESSORS(UEROAttributeSet, MaxMana)

    UPROPERTY(BlueprintReadOnly, ReplicatedUsing=OnRep_Attack, Category="ERO|Attributes")
    FGameplayAttributeData Attack;
    ERO_ATTRIBUTE_ACCESSORS(UEROAttributeSet, Attack)

    UPROPERTY(BlueprintReadOnly, ReplicatedUsing=OnRep_MagicAttack, Category="ERO|Attributes")
    FGameplayAttributeData MagicAttack;
    ERO_ATTRIBUTE_ACCESSORS(UEROAttributeSet, MagicAttack)

    UPROPERTY(BlueprintReadOnly, ReplicatedUsing=OnRep_Defense, Category="ERO|Attributes")
    FGameplayAttributeData Defense;
    ERO_ATTRIBUTE_ACCESSORS(UEROAttributeSet, Defense)

    UPROPERTY(BlueprintReadOnly, ReplicatedUsing=OnRep_MagicDefense, Category="ERO|Attributes")
    FGameplayAttributeData MagicDefense;
    ERO_ATTRIBUTE_ACCESSORS(UEROAttributeSet, MagicDefense)

    UPROPERTY(BlueprintReadOnly, ReplicatedUsing=OnRep_CritChance, Category="ERO|Attributes")
    FGameplayAttributeData CritChance;
    ERO_ATTRIBUTE_ACCESSORS(UEROAttributeSet, CritChance)

    UPROPERTY(BlueprintReadOnly, ReplicatedUsing=OnRep_MoveSpeed, Category="ERO|Attributes")
    FGameplayAttributeData MoveSpeed;
    ERO_ATTRIBUTE_ACCESSORS(UEROAttributeSet, MoveSpeed)

    virtual void GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const override;

protected:
    UFUNCTION()
    void OnRep_Health(const FGameplayAttributeData& OldValue) const;

    UFUNCTION()
    void OnRep_MaxHealth(const FGameplayAttributeData& OldValue) const;

    UFUNCTION()
    void OnRep_Mana(const FGameplayAttributeData& OldValue) const;

    UFUNCTION()
    void OnRep_MaxMana(const FGameplayAttributeData& OldValue) const;

    UFUNCTION()
    void OnRep_Attack(const FGameplayAttributeData& OldValue) const;

    UFUNCTION()
    void OnRep_MagicAttack(const FGameplayAttributeData& OldValue) const;

    UFUNCTION()
    void OnRep_Defense(const FGameplayAttributeData& OldValue) const;

    UFUNCTION()
    void OnRep_MagicDefense(const FGameplayAttributeData& OldValue) const;

    UFUNCTION()
    void OnRep_CritChance(const FGameplayAttributeData& OldValue) const;

    UFUNCTION()
    void OnRep_MoveSpeed(const FGameplayAttributeData& OldValue) const;
};
