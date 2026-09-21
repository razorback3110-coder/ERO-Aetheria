#pragma once

#include "CoreMinimal.h"
#include "GameFramework/PlayerState.h"
#include "EROPlayerEconomyState.generated.h"

USTRUCT(BlueprintType)
struct FEROInventoryStack
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly, Category="ERO|Inventory")
    FName ItemId = NAME_None;

    UPROPERTY(BlueprintReadOnly, Category="ERO|Inventory")
    int32 Quantity = 0;
};

UCLASS()
class EROAETHERIA_API AEROPlayerEconomyState final : public APlayerState
{
    GENERATED_BODY()

public:
    AEROPlayerEconomyState();

    virtual void GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const override;
    virtual void BeginPlay() override;
    virtual void EndPlay(const EEndPlayReason::Type EndPlayReason) override;

    UPROPERTY(Replicated, BlueprintReadOnly, Category="ERO|Economy")
    int64 GoldBalance = 0;

    UPROPERTY(Replicated, BlueprintReadOnly, Category="ERO|Inventory")
    TArray<FEROInventoryStack> Inventory;

    void GrantGold(int64 Amount);
    void GrantItem(FName ItemId, int32 Quantity);
    void SavePersistentEconomyState();

private:
    FString GetPersistenceSlotName() const;
    void LoadPersistentEconomyState();

    FTimerHandle PersistenceSaveTimerHandle;
};
