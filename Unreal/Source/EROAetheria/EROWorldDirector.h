#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "EROWorldDirector.generated.h"

USTRUCT(BlueprintType)
struct FEROWorldEventDefinition
{
    GENERATED_BODY()

    UPROPERTY(EditAnywhere, BlueprintReadOnly)
    FName EventId = NAME_None;

    UPROPERTY(EditAnywhere, BlueprintReadOnly)
    FName RegionId = NAME_None;

    UPROPERTY(EditAnywhere, BlueprintReadOnly)
    float DurationSeconds = 900.0f;

    UPROPERTY(EditAnywhere, BlueprintReadOnly)
    int32 RecommendedLevel = 1;
};

UCLASS()
class EROAETHERIA_API AEROWorldDirector final : public AActor
{
    GENERATED_BODY()

public:
    AEROWorldDirector();

    UFUNCTION(BlueprintCallable, Category="ERO|World")
    void StartWorldEvent(FName EventId);

    UFUNCTION(BlueprintCallable, Category="ERO|World")
    void StopWorldEvent();

    UFUNCTION(BlueprintPure, Category="ERO|World")
    bool IsWorldEventActive() const { return bEventActive; }

    UFUNCTION(BlueprintPure, Category="ERO|World")
    FName GetActiveEventId() const { return ActiveEventId; }

    UFUNCTION(BlueprintPure, Category="ERO|World")
    FName GetActiveRegionId() const { return ActiveRegionId; }

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category="ERO|World")
    TArray<FEROWorldEventDefinition> LaunchEvents;

protected:
    virtual void BeginPlay() override;
    virtual void GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const override;

private:
    UPROPERTY(Replicated, VisibleInstanceOnly, Category="ERO|World")
    bool bEventActive = false;

    UPROPERTY(Replicated, VisibleInstanceOnly, Category="ERO|World")
    FName ActiveEventId = NAME_None;

    UPROPERTY(Replicated, VisibleInstanceOnly, Category="ERO|World")
    FName ActiveRegionId = NAME_None;

    FTimerHandle EventTimerHandle;

    const FEROWorldEventDefinition* FindEvent(FName EventId) const;
};
