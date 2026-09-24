#include "EROGameMode.h"
#include "EROPlayerCharacter.h"
#include "EROPlayerEconomyState.h"
#include "EROEnvironmentActor.h"
#include "EROEnemyActor.h"
#include "GameFramework/PlayerController.h"
#include "Dom/JsonObject.h"
#include "Serialization/JsonReader.h"
#include "Serialization/JsonSerializer.h"
#include "Misc/FileHelper.h"
#include "Misc/Paths.h"
#include "HAL/PlatformFileManager.h"

namespace
{
    constexpr const TCHAR* EncounterPersistenceFileName = TEXT("EROEncounterState.json");
}

AEROGameMode::AEROGameMode()
{
    bUseSeamlessTravel = true;
    DefaultPawnClass = AEROPlayerCharacter::StaticClass();
    PlayerStateClass = AEROPlayerEconomyState::StaticClass();
    PrimaryActorTick.bCanEverTick = true;
    PrimaryActorTick.TickInterval = 1.0f;

    StarterEncounters = {
        { TEXT("Starter_Imp_01"), FVector(900.0f, 0.0f, 100.0f), FRotator::ZeroRotator, 1, 250.0f, 250, 25, TEXT("Aetherium_Shard"), 1, 10.0f, false },
        { TEXT("Starter_Imp_02"), FVector(1300.0f, 650.0f, 100.0f), FRotator::ZeroRotator, 2, 350.0f, 350, 50, TEXT("Aetherium_Shard"), 1, 10.0f, false },
        { TEXT("Starter_Imp_03"), FVector(1700.0f, -550.0f, 100.0f), FRotator::ZeroRotator, 3, 450.0f, 450, 75, TEXT("Aetherium_Shard"), 1, 10.0f, false },

        { TEXT("MVP_01"), FVector(4200.0f, 0.0f, 100.0f), FRotator::ZeroRotator, 25, 250000.0f, 25000, 50000, TEXT("MVP_Necklace_Aetheria"), 1, 3600.0f, true },
        { TEXT("MVP_02"), FVector(0.0f, 7300.0f, 100.0f), FRotator::ZeroRotator, 35, 125000.0f, 10000, 20000, TEXT("MVP_Necklace_Valoria"), 1, 3600.0f, true },
        { TEXT("MVP_03"), FVector(4200.0f, 8800.0f, 100.0f), FRotator::ZeroRotator, 45, 160000.0f, 12500, 25000, TEXT("MVP_Necklace_Valoria_Elite"), 1, 3600.0f, true },
        { TEXT("MVP_04"), FVector(6200.0f, 9600.0f, 100.0f), FRotator::ZeroRotator, 55, 185000.0f, 15000, 30000, TEXT("MVP_Necklace_Valoria_Elite"), 1, 3600.0f, true },
        { TEXT("MVP_05"), FVector(-7200.0f, 0.0f, 100.0f), FRotator::ZeroRotator, 70, 210000.0f, 18000, 35000, TEXT("MVP_Necklace_Elderglen"), 1, 3600.0f, true },
        { TEXT("MVP_06"), FVector(-10500.0f, 2800.0f, 100.0f), FRotator::ZeroRotator, 85, 250000.0f, 22000, 45000, TEXT("MVP_Necklace_Elderglen_Elite"), 1, 3600.0f, true },
        { TEXT("MVP_07"), FVector(7200.0f, 0.0f, 100.0f), FRotator::ZeroRotator, 100, 320000.0f, 30000, 60000, TEXT("MVP_Necklace_Sunscar"), 1, 3600.0f, true },
        { TEXT("MVP_08"), FVector(10400.0f, -2500.0f, 100.0f), FRotator::ZeroRotator, 115, 380000.0f, 36000, 70000, TEXT("MVP_Necklace_Sunscar_Elite"), 1, 3600.0f, true },
        { TEXT("MVP_09"), FVector(0.0f, -7200.0f, 100.0f), FRotator::ZeroRotator, 130, 520000.0f, 50000, 90000, TEXT("MVP_Necklace_Frostheim"), 1, 3600.0f, true },
        { TEXT("MVP_10"), FVector(3200.0f, -9400.0f, 100.0f), FRotator::ZeroRotator, 145, 600000.0f, 58000, 105000, TEXT("MVP_Necklace_Frostheim_Elite"), 1, 3600.0f, true },
        { TEXT("MVP_11"), FVector(-7200.0f, -7200.0f, 100.0f), FRotator::ZeroRotator, 160, 850000.0f, 80000, 140000, TEXT("MVP_Necklace_Mirehaven"), 1, 3600.0f, true },
        { TEXT("MVP_12"), FVector(-10400.0f, -9500.0f, 100.0f), FRotator::ZeroRotator, 175, 950000.0f, 90000, 155000, TEXT("MVP_Necklace_Mirehaven_Elite"), 1, 3600.0f, true },
        { TEXT("MVP_13"), FVector(7200.0f, 7200.0f, 100.0f), FRotator::ZeroRotator, 190, 1200000.0f, 115000, 200000, TEXT("MVP_Necklace_Arkenfall"), 1, 3600.0f, true },
        { TEXT("MVP_14"), FVector(10400.0f, 9500.0f, 100.0f), FRotator::ZeroRotator, 205, 1350000.0f, 130000, 220000, TEXT("MVP_Necklace_Arkenfall_Elite"), 1, 3600.0f, true },
        { TEXT("MVP_15"), FVector(-14400.0f, 0.0f, 100.0f), FRotator::ZeroRotator, 220, 1600000.0f, 150000, 260000, TEXT("MVP_Necklace_Astralis"), 1, 3600.0f, true },
        { TEXT("MVP_16"), FVector(-17600.0f, 3000.0f, 100.0f), FRotator::ZeroRotator, 235, 1800000.0f, 165000, 285000, TEXT("MVP_Necklace_Astralis_Elite"), 1, 3600.0f, true },
        { TEXT("MVP_17"), FVector(14400.0f, 0.0f, 100.0f), FRotator::ZeroRotator, 240, 2000000.0f, 180000, 310000, TEXT("MVP_Necklace_Duskmoor"), 1, 3600.0f, true },
        { TEXT("MVP_18"), FVector(17600.0f, -3000.0f, 100.0f), FRotator::ZeroRotator, 245, 2250000.0f, 205000, 340000, TEXT("MVP_Necklace_Duskmoor_Elite"), 1, 3600.0f, true },
        { TEXT("MVP_19"), FVector(0.0f, -14400.0f, 100.0f), FRotator::ZeroRotator, 248, 2500000.0f, 230000, 380000, TEXT("MVP_Necklace_Abyssia"), 1, 3600.0f, true },
        { TEXT("MVP_20"), FVector(3500.0f, -17600.0f, 100.0f), FRotator::ZeroRotator, 249, 2800000.0f, 260000, 420000, TEXT("MVP_Necklace_Abyssia_Elite"), 1, 3600.0f, true },
        { TEXT("MVP_21"), FVector(0.0f, 14400.0f, 100.0f), FRotator::ZeroRotator, 250, 3200000.0f, 300000, 500000, TEXT("MVP_Necklace_Drakoria"), 1, 3600.0f, true },
        { TEXT("MVP_22"), FVector(-3500.0f, 17600.0f, 100.0f), FRotator::ZeroRotator, 250, 3600000.0f, 340000, 560000, TEXT("MVP_Necklace_Drakoria_Elite"), 1, 3600.0f, true }
    };
}

void AEROGameMode::BeginPlay()
{
    Super::BeginPlay();

    if (!HasAuthority())
    {
        return;
    }

    LoadEncounterPersistence();
    GetWorld()->SpawnActor<AEROEnvironmentActor>(FVector::ZeroVector, FRotator::ZeroRotator);
    SpawnConfiguredEncounters();
}

void AEROGameMode::Tick(float DeltaSeconds)
{
    Super::Tick(DeltaSeconds);

    if (!HasAuthority())
    {
        return;
    }

    SpawnConfiguredEncounters();
    UpdateEncounterStreamingState();

    if (bEncounterPersistenceDirty)
    {
        SaveEncounterPersistence();
        bEncounterPersistenceDirty = false;
    }
}

void AEROGameMode::LoadEncounterPersistence()
{
    PersistedRespawnDeadlinesUtc.Reset();

    FString JsonText;
    if (!FFileHelper::LoadFileToString(JsonText, *(FPaths::ProjectSavedDir() / EncounterPersistenceFileName)))
    {
        return;
    }

    TSharedPtr<FJsonObject> Root;
    const TSharedRef<TJsonReader<>> Reader = TJsonReaderFactory<>::Create(JsonText);
    if (!FJsonSerializer::Deserialize(Reader, Root) || !Root.IsValid())
    {
        UE_LOG(LogTemp, Warning, TEXT("ERO encounter persistence file is invalid; starting with clean encounter state."));
        return;
    }

    const TSharedPtr<FJsonObject>* EntriesObject = nullptr;
    if (!Root->TryGetObjectField(TEXT("respawnDeadlinesUtc"), EntriesObject) || !EntriesObject || !EntriesObject->IsValid())
    {
        return;
    }

    for (const TPair<FString, TSharedPtr<FJsonValue>>& Entry : (*EntriesObject)->Values)
    {
        int64 Deadline = 0;
        if (Entry.Value.IsValid() && Entry.Value->TryGetNumberField(Deadline) && Deadline > 0)
        {
            PersistedRespawnDeadlinesUtc.Add(FName(*Entry.Key), Deadline);
        }
    }
}

void AEROGameMode::SaveEncounterPersistence() const
{
    const FString SavePath = FPaths::ProjectSavedDir() / EncounterPersistenceFileName;
    const FString TempPath = SavePath + TEXT(".tmp");

    TSharedRef<FJsonObject> Root = MakeShared<FJsonObject>();
    TSharedRef<FJsonObject> Entries = MakeShared<FJsonObject>();
    for (const TPair<FName, int64>& Entry : PersistedRespawnDeadlinesUtc)
    {
        Entries->SetNumberField(Entry.Key.ToString(), static_cast<double>(Entry.Value));
    }
    Root->SetObjectField(TEXT("respawnDeadlinesUtc"), Entries);
    Root->SetStringField(TEXT("schemaVersion"), TEXT("1"));

    FString JsonText;
    const TSharedRef<TJsonWriter<>> Writer = TJsonWriterFactory<>::Create(&JsonText);
    if (!FJsonSerializer::Serialize(Root, Writer) || !FFileHelper::SaveStringToFile(JsonText, *TempPath))
    {
        UE_LOG(LogTemp, Error, TEXT("Failed to persist ERO encounter state."));
        return;
    }

    IPlatformFile& PlatformFile = FPlatformFileManager::Get().GetPlatformFile();
    if (PlatformFile.FileExists(*SavePath) && !PlatformFile.DeleteFile(*SavePath))
    {
        UE_LOG(LogTemp, Error, TEXT("Failed to replace ERO encounter persistence file."));
        PlatformFile.DeleteFile(*TempPath);
        return;
    }

    if (!PlatformFile.MoveFile(*TempPath, *SavePath))
    {
        UE_LOG(LogTemp, Error, TEXT("Failed to atomically finalize ERO encounter persistence file."));
    }
}

bool AEROGameMode::IsEncounterWithinActivationRadius(const FVector& EncounterLocation) const
{
    if (!GetWorld())
    {
        return false;
    }

    const float ActivationRadiusSquared = FMath::Square(EncounterActivationRadius);

    for (FConstPlayerControllerIterator It = GetWorld()->GetPlayerControllerIterator(); It; ++It)
    {
        const APlayerController* PlayerController = It->Get();
        const APawn* PlayerPawn = PlayerController ? PlayerController->GetPawn() : nullptr;
        if (!PlayerPawn)
        {
            continue;
        }

        if (FVector::DistSquared(PlayerPawn->GetActorLocation(), EncounterLocation) <= ActivationRadiusSquared)
        {
            return true;
        }
    }

    return false;
}

void AEROGameMode::SpawnConfiguredEncounters()
{
    if (!GetWorld())
    {
        return;
    }

    const int64 NowUtc = FDateTime::UtcNow().ToUnixTimestamp();

    for (const FEROEncounterSpawnDefinition& Definition : StarterEncounters)
    {
        if (!Definition.IsValid() || ActivatedEncounterIds.Contains(Definition.EncounterId))
        {
            continue;
        }

        if (const int64* Deadline = PersistedRespawnDeadlinesUtc.Find(Definition.EncounterId))
        {
            if (*Deadline > NowUtc)
            {
                continue;
            }

            PersistedRespawnDeadlinesUtc.Remove(Definition.EncounterId);
            bEncounterPersistenceDirty = true;
        }

        if (!IsEncounterWithinActivationRadius(Definition.Location))
        {
            continue;
        }

        const FTransform SpawnTransform(Definition.Rotation, Definition.Location);
        AEROEnemyActor* Enemy = GetWorld()->SpawnActorDeferred<AEROEnemyActor>(AEROEnemyActor::StaticClass(), SpawnTransform);
        if (!Enemy)
        {
            continue;
        }

        Enemy->EnemyLevel = Definition.EnemyLevel;
        Enemy->MaxHealth = FMath::Max(1.0f, Definition.MaxHealth);
        Enemy->CurrentHealth = Enemy->MaxHealth;
        Enemy->ExperienceReward = Definition.ExperienceReward;
        Enemy->GoldReward = Definition.GoldReward;
        Enemy->ItemRewardId = Definition.ItemRewardId;
        Enemy->ItemRewardQuantity = FMath::Clamp(Definition.ItemRewardQuantity, 0, 9999);
        Enemy->RespawnDelay = Definition.RespawnDelay;
        Enemy->FinishSpawning(SpawnTransform);
        ActivatedEncounterIds.Add(Definition.EncounterId);
        ActiveEncounterActors.Add(Definition.EncounterId, Enemy);
    }
}

void AEROGameMode::UpdateEncounterStreamingState()
{
    const int64 NowUtc = FDateTime::UtcNow().ToUnixTimestamp();

    for (auto It = ActiveEncounterActors.CreateIterator(); It; ++It)
    {
        AEROEnemyActor* Enemy = It.Value().Get();
        if (!IsValid(Enemy))
        {
            It.RemoveCurrent();
            continue;
        }

        if (Enemy->bDefeated)
        {
            if (!PersistedRespawnDeadlinesUtc.Contains(It.Key()))
            {
                const int64 RespawnSeconds = FMath::Max<int64>(1, FMath::CeilToInt64(Enemy->RespawnDelay));
                PersistedRespawnDeadlinesUtc.Add(It.Key(), NowUtc + RespawnSeconds);
                bEncounterPersistenceDirty = true;
            }

            continue;
        }

        if (PersistedRespawnDeadlinesUtc.Remove(It.Key()) > 0)
        {
            bEncounterPersistenceDirty = true;
        }

        const bool bPlayerNearby = IsEncounterWithinActivationRadius(Enemy->GetActorLocation());
        Enemy->SetActorHiddenInGame(!bPlayerNearby);
        Enemy->SetActorEnableCollision(bPlayerNearby);
    }
}