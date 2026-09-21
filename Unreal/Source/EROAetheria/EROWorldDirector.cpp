#include "EROWorldDirector.h"

#include "Engine/World.h"
#include "Net/UnrealNetwork.h"
#include "TimerManager.h"

AEROWorldDirector::AEROWorldDirector()
{
    bReplicates = true;
    PrimaryActorTick.bCanEverTick = false;

    LaunchEvents = {
        {TEXT("ForestInvasion"), TEXT("WhisperingForest"), 900.0f, 35},
        {TEXT("DesertSandstorm"), TEXT("SunscarDesert"), 900.0f, 50},
        {TEXT("CorruptionRift"), TEXT("MireOfCorruption"), 1200.0f, 65},
        {TEXT("AncientAwakening"), TEXT("AncientRuins"), 1200.0f, 80},
        {TEXT("ArcaneSurge"), TEXT("ArcaneHighlands"), 900.0f, 100},
        {TEXT("FrontierWar"), TEXT("PvPGvGFrontier"), 1800.0f, 120},
        {TEXT("ElderTitan"), TEXT("MVP_ElderTitan"), 1800.0f, 150},
        {TEXT("CorruptedKing"), TEXT("MVP_CorruptedKing"), 1800.0f, 175}
    };
}

void AEROWorldDirector::BeginPlay()
{
    Super::BeginPlay();

    if (HasAuthority())
    {
        UE_LOG(LogTemp, Log, TEXT("[ERO] World Director online. %d launch events registered."), LaunchEvents.Num());
    }
}

const FEROWorldEventDefinition* AEROWorldDirector::FindEvent(FName EventId) const
{
    return LaunchEvents.FindByPredicate(
        [EventId](const FEROWorldEventDefinition& Event)
        {
            return Event.EventId == EventId;
        });
}

void AEROWorldDirector::StartWorldEvent(FName EventId)
{
    if (!HasAuthority())
    {
        return;
    }

    const FEROWorldEventDefinition* Event = FindEvent(EventId);
    if (!Event)
    {
        UE_LOG(LogTemp, Warning, TEXT("[ERO] Unknown world event: %s"), *EventId.ToString());
        return;
    }

    GetWorldTimerManager().ClearTimer(EventTimerHandle);

    bEventActive = true;
    ActiveEventId = Event->EventId;
    ActiveRegionId = Event->RegionId;

    UE_LOG(LogTemp, Log, TEXT("[ERO] WORLD EVENT STARTED: %s | Region=%s | RecommendedLevel=%d"),
        *ActiveEventId.ToString(),
        *ActiveRegionId.ToString(),
        Event->RecommendedLevel);

    GetWorldTimerManager().SetTimer(
        EventTimerHandle,
        this,
        &AEROWorldDirector::StopWorldEvent,
        Event->DurationSeconds,
        false);
}

void AEROWorldDirector::StopWorldEvent()
{
    if (!HasAuthority())
    {
        return;
    }

    if (bEventActive)
    {
        UE_LOG(LogTemp, Log, TEXT("[ERO] WORLD EVENT ENDED: %s"), *ActiveEventId.ToString());
    }

    bEventActive = false;
    ActiveEventId = NAME_None;
    ActiveRegionId = NAME_None;
    GetWorldTimerManager().ClearTimer(EventTimerHandle);
}

void AEROWorldDirector::GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const
{
    Super::GetLifetimeReplicatedProps(OutLifetimeProps);

    DOREPLIFETIME(AEROWorldDirector, bEventActive);
    DOREPLIFETIME(AEROWorldDirector, ActiveEventId);
    DOREPLIFETIME(AEROWorldDirector, ActiveRegionId);
}
