#include "EROPlayerEconomyState.h"

#include "Net/UnrealNetwork.h"

AEROPlayerEconomyState::AEROPlayerEconomyState()
{
    bReplicates = true;
}

void AEROPlayerEconomyState::GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const
{
    Super::GetLifetimeReplicatedProps(OutLifetimeProps);
    DOREPLIFETIME(AEROPlayerEconomyState, GoldBalance);
    DOREPLIFETIME(AEROPlayerEconomyState, Inventory);
}

void AEROPlayerEconomyState::GrantGold(int64 Amount)
{
    if (!HasAuthority() || Amount <= 0)
    {
        return;
    }

    const int64 SafeBalance = FMath::Max<int64>(0, GoldBalance);
    const int64 MaxGold = MAX_int64;
    GoldBalance = Amount > (MaxGold - SafeBalance) ? MaxGold : SafeBalance + Amount;
}

void AEROPlayerEconomyState::GrantItem(FName ItemId, int32 Quantity)
{
    if (!HasAuthority() || ItemId.IsNone() || Quantity <= 0)
    {
        return;
    }

    constexpr int32 MaxInventoryEntries = 128;
    constexpr int32 MaxStackQuantity = 9999;

    const int32 SafeQuantity = FMath::Min(Quantity, MaxStackQuantity);
    for (FEROInventoryStack& Stack : Inventory)
    {
        if (Stack.ItemId == ItemId)
        {
            Stack.Quantity = FMath::Clamp(Stack.Quantity + SafeQuantity, 0, MaxStackQuantity);
            return;
        }
    }

    if (Inventory.Num() >= MaxInventoryEntries)
    {
        return;
    }

    FEROInventoryStack& NewStack = Inventory.AddDefaulted_GetRef();
    NewStack.ItemId = ItemId;
    NewStack.Quantity = SafeQuantity;
}
