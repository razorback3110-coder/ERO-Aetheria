#include "EROWorldMapDirector.h"

#include "Engine/World.h"
#include "GameFramework/Pawn.h"
#include "GameFramework/PlayerController.h"

AEROWorldMapDirector::AEROWorldMapDirector()
{
    bReplicates = true;
    PrimaryActorTick.bCanEverTick = false;

    // One persistent Aetheria World Partition map. These are regions/cities,
    // not separate map assets.
    Regions = {
        {TEXT("AetheriaCapital"), TEXT("Aetheria Capital"), 1, FVector(0, 0, 0), 4200.f, 600, 0},
        {TEXT("ValoriaPlains"), TEXT("Valoria Plains"), 10, FVector(0, 7200, 0), 7200.f, 750, 2},
        {TEXT("Elderglen"), TEXT("Elderglen"), 25, FVector(-7200, 0, 0), 7200.f, 2, 2},
        {TEXT("Sunscar"), TEXT("Sunscar"), 50, FVector(7200, 0, 0), 7200.f, 900, 2},
        {TEXT("Frostheim"), TEXT("Frostheim"), 75, FVector(0, -7200, 0), 7200.f, 1000, 2},
        {TEXT("Mirehaven"), TEXT("Mirehaven"), 100, FVector(-7200, -7200, 0), 7200.f, 1200, 2},
        {TEXT("Arkenfall"), TEXT("Arkenfall"), 125, FVector(7200, 7200, 0), 8500.f, 850, 2},
        {TEXT("Astralis"), TEXT("Astralis"), 150, FVector(-14400, 0, 0), 8500.f, 1300, 2},
        {TEXT("Duskmoor"), TEXT("Duskmoor"), 165, FVector(14400, 0, 0), 8500.f, 950, 2},
        {TEXT("Abyssia"), TEXT("Abyssia"), 175, FVector(0, -14400, 0), 9000.f, 1400, 2},
        {TEXT("Drakoria"), TEXT("Drakoria"), 200, FVector(0, 14400, 0), 9000.f, 1500, 2}
    };

    // Separate maps are reserved for instanced content only.
    Instances = {
        {TEXT("Dungeon_Forest"), TEXT("Elderglen Dungeon"), TEXT("/Game/Maps/Instances/Dungeon_Forest"), TEXT("Dungeon"), 25, false},
        {TEXT("Dungeon_Desert"), TEXT("Sunscar Dungeon"), TEXT("/Game/Maps/Instances/Dungeon_Desert"), TEXT("Dungeon"), 50, false},
        {TEXT("Dungeon_Frost"), TEXT("Frostheim Dungeon"), TEXT("/Game/Maps/Instances/Dungeon_Frost"), TEXT("Dungeon"), 75, false},
        {TEXT("Dungeon_Abyss"), TEXT("Abyssia Dungeon"), TEXT("/Game/Maps/Instances/Dungeon_Abyss"), TEXT("Dungeon"), 175, false},
        {TEXT("Tower_01"), TEXT("Tower of the First Guardian"), TEXT("/Game/Maps/Instances/Tower_01"), TEXT("Tower"), 50, false},
        {TEXT("Tower_02"), TEXT("Tower of the Frozen Warden"), TEXT("/Game/Maps/Instances/Tower_02"), TEXT("Tower"), 100, false},
        {TEXT("Tower_03"), TEXT("Tower of the Ancient King"), TEXT("/Game/Maps/Instances/Tower_03"), TEXT("Tower"), 150, false},
        {TEXT("Tower_04"), TEXT("Tower of the Abyss"), TEXT("/Game/Maps/Instances/Tower_04"), TEXT("Tower"), 200, false},
        {TEXT("Tower_05"), TEXT("Tower of the Dragon"), TEXT("/Game/Maps/Instances/Tower_05"), TEXT("Tower"), 225, false},
        {TEXT("Arena_1v1"), TEXT("PvP Arena 1v1"), TEXT("/Game/Maps/Instances/Arena_1v1"), TEXT("PvP_1v1"), 1, false},
        {TEXT("Arena_4v4"), TEXT("PvP Arena 4v4"), TEXT("/Game/Maps/Instances/Arena_4v4"), TEXT("PvP_4v4"), 1, false},
        {TEXT("GvG_WarOfRealms"), TEXT("GvG - War of Realms"), TEXT("/Game/Maps/Instances/GvG_WarOfRealms"), TEXT("GvG"), 50, true}
    };
}

void AEROWorldMapDirector::BeginPlay()
{
    Super::BeginPlay();
    if (HasAuthority())
    {
        UE_LOG(LogTemp, Log, TEXT("[ERO] Aetheria world online: %d regions, %d instance maps."), Regions.Num(), Instances.Num());
    }
}

const FERORegionDefinition* AEROWorldMapDirector::FindRegion(FName RegionId) const
{
    return Regions.FindByPredicate([RegionId](const FERORegionDefinition& Region)
    {
        return Region.RegionId == RegionId;
    });
}

const FEROInstanceDefinition* AEROWorldMapDirector::FindInstance(FName InstanceId) const
{
    return Instances.FindByPredicate([InstanceId](const FEROInstanceDefinition& Instance)
    {
        return Instance.InstanceId == InstanceId;
    });
}

bool AEROWorldMapDirector::HasRegion(FName RegionId) const
{
    return FindRegion(RegionId) != nullptr;
}

bool AEROWorldMapDirector::HasInstance(FName InstanceId) const
{
    return FindInstance(InstanceId) != nullptr;
}

FERORegionDefinition AEROWorldMapDirector::GetRegionDefinition(FName RegionId) const
{
    if (const FERORegionDefinition* Region = FindRegion(RegionId))
    {
        return *Region;
    }
    return FERORegionDefinition();
}

FEROInstanceDefinition AEROWorldMapDirector::GetInstanceDefinition(FName InstanceId) const
{
    if (const FEROInstanceDefinition* Instance = FindInstance(InstanceId))
    {
        return *Instance;
    }
    return FEROInstanceDefinition();
}

void AEROWorldMapDirector::TravelToRegion(FName RegionId)
{
    if (HasAuthority())
    {
        UE_LOG(LogTemp, Warning, TEXT("[ERO] TravelToRegion must be requested by a client controller."));
        return;
    }

    UWorld* World = GetWorld();
    if (!World)
    {
        return;
    }

    APlayerController* LocalPC = World->GetFirstPlayerController();
    if (!LocalPC)
    {
        UE_LOG(LogTemp, Warning, TEXT("[ERO] Region travel requested without a local player controller: %s"), *RegionId.ToString());
        return;
    }

    ServerTravelToRegion(LocalPC, RegionId);
}

void AEROWorldMapDirector::ServerTravelToRegion_Implementation(APlayerController* RequestingController, FName RegionId)
{
    const FERORegionDefinition* Region = FindRegion(RegionId);
    if (!Region || !RequestingController || !IsValid(RequestingController) || !RequestingController->GetPawn())
    {
        UE_LOG(LogTemp, Warning, TEXT("[ERO] Rejected region travel request: %s"), *RegionId.ToString());
        return;
    }

    APawn* Pawn = RequestingController->GetPawn();
    Pawn->SetActorLocation(Region->WorldLocation + FVector(0, 0, 120), false, nullptr, ETeleportType::TeleportPhysics);

    UE_LOG(LogTemp, Log, TEXT("[ERO] REGION TRAVEL: %s -> Player=%s"), *RegionId.ToString(), *GetNameSafe(RequestingController));
}

void AEROWorldMapDirector::TravelToInstance(FName InstanceId)
{
    if (HasAuthority())
    {
        UE_LOG(LogTemp, Warning, TEXT("[ERO] TravelToInstance is not available as a per-player operation yet: %s"), *InstanceId.ToString());
        return;
    }

    UWorld* World = GetWorld();
    if (!World)
    {
        return;
    }

    APlayerController* LocalPC = World->GetFirstPlayerController();
    if (!LocalPC)
    {
        return;
    }

    ServerTravelToInstance(LocalPC, InstanceId);
}

void AEROWorldMapDirector::ServerTravelToInstance_Implementation(APlayerController* RequestingController, FName InstanceId)
{
    const FEROInstanceDefinition* Instance = FindInstance(InstanceId);
    if (!Instance || !RequestingController || !IsValid(RequestingController) || !RequestingController->GetPawn())
    {
        UE_LOG(LogTemp, Warning, TEXT("[ERO] Rejected instance travel request: %s"), *InstanceId.ToString());
        return;
    }

    // Instance maps are server-wide until a dedicated instance-session manager exists.
    // Never silently move every player from an untrusted client request.
    UE_LOG(LogTemp, Warning, TEXT("[ERO] Instance travel is deferred until per-party instance sessions are available: %s"), *InstanceId.ToString());
}
