#include "EROWorldMapDirector.h"

#include "Engine/World.h"
#include "GameFramework/PlayerController.h"
#include "Kismet/GameplayStatics.h"

AEROWorldMapDirector::AEROWorldMapDirector()
{
    bReplicates = true;
    PrimaryActorTick.bCanEverTick = false;

    Maps = {
        {TEXT("AetheriaCapital"), TEXT("Aetheria Capital"), TEXT("/Game/Maps/Aetheria_CompleteWorld"), 1, FVector(0, 0, 0), true},
        {TEXT("StarterPlains"), TEXT("Starter Plains"), TEXT("/Game/Maps/Aetheria_StarterPlains"), 1, FVector(0, 1800, 0), true},
        {TEXT("WhisperingForest"), TEXT("Whispering Forest"), TEXT("/Game/Maps/Aetheria_WhisperingForest"), 25, FVector(-1800, 0, 0), true},
        {TEXT("SunscarDesert"), TEXT("Sunscar Desert"), TEXT("/Game/Maps/Aetheria_SunscarDesert"), 50, FVector(1800, 0, 0), true},
        {TEXT("FrostpeakMountains"), TEXT("Frostpeak Mountains"), TEXT("/Game/Maps/Aetheria_FrostpeakMountains"), 75, FVector(0, -1800, 0), true},
        {TEXT("MireOfCorruption"), TEXT("Mire of Corruption"), TEXT("/Game/Maps/Aetheria_MireOfCorruption"), 100, FVector(-1800, -1800, 0), true},
        {TEXT("AncientRuins"), TEXT("Ancient Ruins"), TEXT("/Game/Maps/Aetheria_AncientRuins"), 125, FVector(1800, 1800, 0), true},
        {TEXT("ArcaneHighlands"), TEXT("Arcane Highlands"), TEXT("/Game/Maps/Aetheria_ArcaneHighlands"), 150, FVector(-3600, 0, 0), true},
        {TEXT("PvPGvGFrontier"), TEXT("PvP / GvG Frontier"), TEXT("/Game/Maps/Aetheria_PvPGvGFrontier"), 120, FVector(3600, 0, 0), true},
        {TEXT("AbyssalDepths"), TEXT("Abyssal Depths"), TEXT("/Game/Maps/Aetheria_AbyssalDepths"), 175, FVector(0, -3600, 0), true},
        {TEXT("DragonSanctum"), TEXT("Dragon Sanctum"), TEXT("/Game/Maps/Aetheria_DragonSanctum"), 200, FVector(0, 3600, 0), true}
    };
}

void AEROWorldMapDirector::BeginPlay()
{
    Super::BeginPlay();
    if (HasAuthority())
    {
        UE_LOG(LogTemp, Log, TEXT("[ERO] World Map Director online. %d maps registered."), Maps.Num());
    }
}

const FEROMapDefinition* AEROWorldMapDirector::FindMap(FName MapId) const
{
    return Maps.FindByPredicate([MapId](const FEROMapDefinition& Map)
    {
        return Map.MapId == MapId;
    });
}

bool AEROWorldMapDirector::HasMap(FName MapId) const
{
    return FindMap(MapId) != nullptr;
}

FEROMapDefinition AEROWorldMapDirector::GetMapDefinition(FName MapId) const
{
    if (const FEROMapDefinition* Map = FindMap(MapId))
    {
        return *Map;
    }
    return FEROMapDefinition();
}

void AEROWorldMapDirector::TravelToMap(FName MapId)
{
    if (!HasAuthority())
    {
        ServerTravelToMap(MapId);
        return;
    }

    ServerTravelToMap_Implementation(MapId);
}

void AEROWorldMapDirector::ServerTravelToMap_Implementation(FName MapId)
{
    const FEROMapDefinition* Map = FindMap(MapId);
    UWorld* World = GetWorld();

    if (!Map || !World)
    {
        UE_LOG(LogTemp, Warning, TEXT("[ERO] Unknown map travel request: %s"), *MapId.ToString());
        return;
    }

    UE_LOG(LogTemp, Log, TEXT("[ERO] MAP TRAVEL: %s -> %s"), *MapId.ToString(), *Map->MapAssetPath);

    FGameplayTagContainer UnusedTags;
    World->ServerTravel(Map->MapAssetPath + TEXT("?listen"));
}
