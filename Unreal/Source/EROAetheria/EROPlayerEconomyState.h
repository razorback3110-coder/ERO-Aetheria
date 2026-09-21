#pragma once

#include "CoreMinimal.h"
#include "GameFramework/PlayerState.h"
#include "EROPlayerEconomyState.generated.h"

UCLASS()
class EROAETHERIA_API AEROPlayerEconomyState final : public APlayerState
{
    GENERATED_BODY()

public:
    AEROPlayerEconomyState();

    virtual void GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const override;

    UPROPERTY(Replicated, BlueprintReadOnly, Category="ERO|Economy")
    int64 GoldBalance = 0;

    void GrantGold(int64 Amount);
};
