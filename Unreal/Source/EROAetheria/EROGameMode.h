#pragma once

#include "CoreMinimal.h"
#include "GameFramework/GameModeBase.h"
#include "EROGameMode.generated.h"

UCLASS()
class EROAETHERIA_API AEROGameMode final : public AGameModeBase
{
    GENERATED_BODY()

public:
    AEROGameMode();

protected:
    virtual void BeginPlay() override;
};
