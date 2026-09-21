#include "EROAttributeSet.h"

#include "Net/UnrealNetwork.h"

void UEROAttributeSet::GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const
{
    Super::GetLifetimeReplicatedProps(OutLifetimeProps);

    DOREPLIFETIME_CONDITION_NOTIFY(UEROAttributeSet, Health, COND_None, REPNOTIFY_Always);
    DOREPLIFETIME_CONDITION_NOTIFY(UEROAttributeSet, MaxHealth, COND_None, REPNOTIFY_Always);
    DOREPLIFETIME_CONDITION_NOTIFY(UEROAttributeSet, Mana, COND_None, REPNOTIFY_Always);
    DOREPLIFETIME_CONDITION_NOTIFY(UEROAttributeSet, MaxMana, COND_None, REPNOTIFY_Always);
    DOREPLIFETIME_CONDITION_NOTIFY(UEROAttributeSet, Attack, COND_None, REPNOTIFY_Always);
    DOREPLIFETIME_CONDITION_NOTIFY(UEROAttributeSet, MagicAttack, COND_None, REPNOTIFY_Always);
    DOREPLIFETIME_CONDITION_NOTIFY(UEROAttributeSet, Defense, COND_None, REPNOTIFY_Always);
    DOREPLIFETIME_CONDITION_NOTIFY(UEROAttributeSet, MagicDefense, COND_None, REPNOTIFY_Always);
    DOREPLIFETIME_CONDITION_NOTIFY(UEROAttributeSet, CritChance, COND_None, REPNOTIFY_Always);
    DOREPLIFETIME_CONDITION_NOTIFY(UEROAttributeSet, MoveSpeed, COND_None, REPNOTIFY_Always);
}

void UEROAttributeSet::OnRep_Health(const FGameplayAttributeData& OldValue) const
{
    GAMEPLAYATTRIBUTE_REPNOTIFY(UEROAttributeSet, Health, OldValue);
}

void UEROAttributeSet::OnRep_MaxHealth(const FGameplayAttributeData& OldValue) const
{
    GAMEPLAYATTRIBUTE_REPNOTIFY(UEROAttributeSet, MaxHealth, OldValue);
}

void UEROAttributeSet::OnRep_Mana(const FGameplayAttributeData& OldValue) const
{
    GAMEPLAYATTRIBUTE_REPNOTIFY(UEROAttributeSet, Mana, OldValue);
}

void UEROAttributeSet::OnRep_MaxMana(const FGameplayAttributeData& OldValue) const
{
    GAMEPLAYATTRIBUTE_REPNOTIFY(UEROAttributeSet, MaxMana, OldValue);
}

void UEROAttributeSet::OnRep_Attack(const FGameplayAttributeData& OldValue) const
{
    GAMEPLAYATTRIBUTE_REPNOTIFY(UEROAttributeSet, Attack, OldValue);
}

void UEROAttributeSet::OnRep_MagicAttack(const FGameplayAttributeData& OldValue) const
{
    GAMEPLAYATTRIBUTE_REPNOTIFY(UEROAttributeSet, MagicAttack, OldValue);
}

void UEROAttributeSet::OnRep_Defense(const FGameplayAttributeData& OldValue) const
{
    GAMEPLAYATTRIBUTE_REPNOTIFY(UEROAttributeSet, Defense, OldValue);
}

void UEROAttributeSet::OnRep_MagicDefense(const FGameplayAttributeData& OldValue) const
{
    GAMEPLAYATTRIBUTE_REPNOTIFY(UEROAttributeSet, MagicDefense, OldValue);
}

void UEROAttributeSet::OnRep_CritChance(const FGameplayAttributeData& OldValue) const
{
    GAMEPLAYATTRIBUTE_REPNOTIFY(UEROAttributeSet, CritChance, OldValue);
}

void UEROAttributeSet::OnRep_MoveSpeed(const FGameplayAttributeData& OldValue) const
{
    GAMEPLAYATTRIBUTE_REPNOTIFY(UEROAttributeSet, MoveSpeed, OldValue);
}
