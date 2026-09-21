#include "EROPlayerCharacter.h"

void AEROPlayerCharacter::GrantGold(int64 Amount)
{
    if (!HasAuthority() || Amount <= 0 || bDefeated)
    {
        return;
    }

    constexpr int64 MaxGold = MAX_int64;
    const int64 SafeBalance = FMath::Max<int64>(0, GoldBalance);
    GoldBalance = Amount > (MaxGold - SafeBalance) ? MaxGold : SafeBalance + Amount;
}
