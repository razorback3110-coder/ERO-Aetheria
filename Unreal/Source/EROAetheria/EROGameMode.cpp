#include "EROGameMode.h"
#include "EROPlayerCharacter.h"
#include "EROEnvironmentActor.h"

AEROGameMode::AEROGameMode()
{
    bUseSeamlessTravel = true;
    DefaultPawnClass = AEROPlayerCharacter::StaticClass();
}

void AEROGameMode::BeginPlay()
{
    Super::BeginPlay();

    if (HasAuthority())
    {
        GetWorld()->SpawnActor<AEROEnvironmentActor>(FVector::ZeroVector, FRotator::ZeroRotator);
    }
}
