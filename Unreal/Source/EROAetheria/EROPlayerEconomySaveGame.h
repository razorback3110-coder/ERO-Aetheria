#pragma once

#include "CoreMinimal.h"
#include "GameFramework/SaveGame.h"
#include "EROPlayerEconomyState.h"
#include "EROPlayerEconomySaveGame.generated.h"

UCLASS()
class EROAETHERIA_API UEROPlayerEconomySaveGame final : public USaveGame
{
    GENERATED_BODY()

public:
    UPROPERTY()
    int32 SchemaVersion = 1;

    UPROPERTY()
    int64 GoldBalance = 0;

    UPROPERTY()
    TArray<FEROInventoryStack> Inventory;
};
