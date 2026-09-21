#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "EROWorldMapDirector.generated.h"

USTRUCT(BlueprintType)
struct FEROMapDefinition
{
    GENERATED_BODY()

    UPROPERTY(EditAnywhere, BlueprintReadOnly)
    FName MapId = NAME_None;

    UPROPERTY(EditAnywhere, BlueprintReadOnly)
    FString DisplayName;

    UPROPERTY(EditAnywhere, BlueprintReadOnly)
    FString MapAssetPath;

    UPROPERTY(EditAnywhere, BlueprintReadOnly)
    int32 RecommendedLevel = 1;

    UPROPERTY(EditAnywhere, BlueprintReadOnly)
    FVector WorldMapPosition = FVector::ZeroVector;

    UPROPERTY(EditAnywhere, BlueprintReadOnly)
    bool bUnlockedByDefault = true;
};

UCLASS()
class EROAETHERIA_API AEROWorldMapDirector final : public AActor
{
    GENERATED_BODY()

public:
    AEROWorldMapDirector();

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category="ERO|World Map")
    TArray<FEROMapDefinition> Maps;

    UFUNCTION(BlueprintCallable, Category="ERO|World Map")
    void TravelToMap(FName MapId);

    UFUNCTION(Server, Reliable)
    void ServerTravelToMap(FName MapId);

    UFUNCTION(BlueprintPure, Category="ERO|World Map")
    bool HasMap(FName MapId) const;

    UFUNCTION(BlueprintPure, Category="ERO|World Map")
    FEROMapDefinition GetMapDefinition(FName MapId) const;

protected:
    virtual void BeginPlay() override;

private:
    const FEROMapDefinition* FindMap(FName MapId) const;
};
