#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "EROEnvironmentActor.generated.h"

UCLASS()
class EROAETHERIA_API AEROEnvironmentActor final : public AActor
{
    GENERATED_BODY()

public:
    AEROEnvironmentActor();

protected:
    virtual void BeginPlay() override;

private:
    void BuildEnvironment();
    UStaticMesh* LoadMesh(const TCHAR* AssetPath) const;
    void AddStaticMesh(UStaticMesh* Mesh, const FVector& Location, const FVector& Scale, const FRotator& Rotation = FRotator::ZeroRotator);
};
