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

    GoldBalance = FMath::Max<int64>(0, GoldBalance);
    GoldBalance = FMath::AddInt64Checked(GoldBalance, Amount);
}
