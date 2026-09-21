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
