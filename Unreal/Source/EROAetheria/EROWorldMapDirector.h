#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "GameFramework/PlayerController.h"
#include "EROWorldMapDirector.generated.h"

USTRUCT(BlueprintType)
struct FERORegionDefinition
{
    GENERATED_BODY()

    UPROPERTY(EditAnywhere, BlueprintReadOnly)
    FName RegionId = NAME_None;

    UPROPERTY(EditAnywhere, BlueprintReadOnly)
    FString DisplayName;

    UPROPERTY(EditAnywhere, BlueprintReadOnly)
    int32 RecommendedLevel = 1;

    UPROPERTY(EditAnywhere, BlueprintReadOnly)
    FVector WorldLocation = FVector::ZeroVector;

    UPROPERTY(EditAnywhere, BlueprintReadOnly)
    float Radius = 4000.f;

    UPROPERTY(EditAnywhere, BlueprintReadOnly)
    int32 AchievementCount = 0;

    UPROPERTY(EditAnywhere, BlueprintReadOnly)
    int32 MvpCount = 0;
};

USTRUCT(BlueprintType)
struct FEROInstanceDefinition
{
    GENERATED_BODY()

    UPROPERTY(EditAnywhere, BlueprintReadOnly)
    FName InstanceId = NAME_None;

    UPROPERTY(EditAnywhere, BlueprintReadOnly)
    FString DisplayName;

    UPROPERTY(EditAnywhere, BlueprintReadOnly)
    FString MapAssetPath;

    UPROPERTY(EditAnywhere, BlueprintReadOnly)
    FName ActivityType = NAME_None;

    UPROPERTY(EditAnywhere, BlueprintReadOnly)
    int32 RecommendedLevel = 1;

    UPROPERTY(EditAnywhere, BlueprintReadOnly)
    bool bRequiresActiveEvent = false;
};

UCLASS()
class EROAETHERIA_API AEROWorldMapDirector final : public AActor
{
    GENERATED_BODY()

public:
    AEROWorldMapDirector();

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category="ERO|World")
    TArray<FERORegionDefinition> Regions;

    UPROPERTY(EditAnywhere, BlueprintReadOnly, Category="ERO|Instances")
    TArray<FEROInstanceDefinition> Instances;

    UFUNCTION(BlueprintCallable, Category="ERO|World")
    void TravelToRegion(FName RegionId);

    UFUNCTION(Server, Reliable)
    void ServerTravelToRegion(APlayerController* RequestingController, FName RegionId);

    UFUNCTION(BlueprintCallable, Category="ERO|Instances")
    void TravelToInstance(FName InstanceId);

    UFUNCTION(Server, Reliable)
    void ServerTravelToInstance(APlayerController* RequestingController, FName InstanceId);

    UFUNCTION(BlueprintPure, Category="ERO|World")
    bool HasRegion(FName RegionId) const;

    UFUNCTION(BlueprintPure, Category="ERO|Instances")
    bool HasInstance(FName InstanceId) const;

    UFUNCTION(BlueprintPure, Category="ERO|World")
    FERORegionDefinition GetRegionDefinition(FName RegionId) const;

    UFUNCTION(BlueprintPure, Category="ERO|Instances")
    FEROInstanceDefinition GetInstanceDefinition(FName InstanceId) const;

protected:
    virtual void BeginPlay() override;

private:
    const FERORegionDefinition* FindRegion(FName RegionId) const;
    const FEROInstanceDefinition* FindInstance(FName InstanceId) const;
};
