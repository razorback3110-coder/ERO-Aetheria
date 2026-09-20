#include "EROGameMode.h"
#include "EROPlayerCharacter.h"

AEROGameMode::AEROGameMode()
{
    bUseSeamlessTravel = true;
    DefaultPawnClass = AEROPlayerCharacter::StaticClass();
}

void AEROGameMode::BeginPlay()
{
    Super::BeginPlay();
}
