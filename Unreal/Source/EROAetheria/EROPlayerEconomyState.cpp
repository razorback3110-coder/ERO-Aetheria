#include "EROPlayerEconomyState.h"

#include "EROPlayerEconomySaveGame.h"
#include "EROPlayerCharacter.h"
#include "Kismet/GameplayStatics.h"
#include "Net/UnrealNetwork.h"

namespace
{
constexpr float EconomyCheckpointIntervalSeconds = 60.0f;
constexpr float CharacterRestoreDelaySeconds = 0.1f;
constexpr int32 MaxInventoryEntries = 128;
constexpr int32 MaxStackQuantity = 9999;
}

AEROPlayerEconomyState::AEROPlayerEconomyState()
{
    bReplicates = true;
}

void AEROPlayerEconomyState::GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const
{
    Super::GetLifetimeReplicatedProps(OutLifetimeProps);
    DOREPLIFETIME(AEROPlayerEconomyState, GoldBalance);
    DOREPLIFETIME(AEROPlayerEconomyState, Inventory);
}

void AEROPlayerEconomyState::BeginPlay()
{
    Super::BeginPlay();

    if (HasAuthority())
    {
        LoadPersistentEconomyState();
        GetWorldTimerManager().SetTimer(
            PersistenceSaveTimerHandle,
            this,
            &AEROPlayerEconomyState::SavePersistentEconomyState,
            EconomyCheckpointIntervalSeconds,
            true,
            EconomyCheckpointIntervalSeconds);

        GetWorldTimerManager().SetTimer(
            CharacterRestoreTimerHandle,
            this,
            &AEROPlayerEconomyState::RestorePersistentCharacterState,
            CharacterRestoreDelaySeconds,
            false,
            CharacterRestoreDelaySeconds);
    }
}

void AEROPlayerEconomyState::EndPlay(const EEndPlayReason::Type EndPlayReason)
{
    if (HasAuthority())
    {
        GetWorldTimerManager().ClearTimer(PersistenceSaveTimerHandle);
        GetWorldTimerManager().ClearTimer(CharacterRestoreTimerHandle);
        SavePersistentEconomyState();
    }

    Super::EndPlay(EndPlayReason);
}

void AEROPlayerEconomyState::GrantGold(int64 Amount)
{
    if (!HasAuthority() || Amount <= 0)
    {
        return;
    }

    const int64 SafeBalance = FMath::Max<int64>(0, GoldBalance);
    const int64 MaxGold = MAX_int64;
    GoldBalance = Amount > (MaxGold - SafeBalance) ? MaxGold : SafeBalance + Amount;
}

void AEROPlayerEconomyState::GrantItem(FName ItemId, int32 Quantity)
{
    if (!HasAuthority() || ItemId.IsNone() || Quantity <= 0)
    {
        return;
    }

    const int32 SafeQuantity = FMath::Min(Quantity, MaxStackQuantity);
    for (FEROInventoryStack& Stack : Inventory)
    {
        if (Stack.ItemId == ItemId)
        {
            Stack.Quantity = FMath::Clamp(Stack.Quantity + SafeQuantity, 0, MaxStackQuantity);
            return;
        }
    }

    if (Inventory.Num() >= MaxInventoryEntries)
    {
        return;
    }

    FEROInventoryStack& NewStack = Inventory.AddDefaulted_GetRef();
    NewStack.ItemId = ItemId;
    NewStack.Quantity = SafeQuantity;
}

void AEROPlayerEconomyState::SavePersistentEconomyState()
{
    if (!HasAuthority())
    {
        return;
    }

    UEROPlayerEconomySaveGame* SaveGame = Cast<UEROPlayerEconomySaveGame>(UGameplayStatics::CreateSaveGameObject(UEROPlayerEconomySaveGame::StaticClass()));
    if (!SaveGame)
    {
        return;
    }

    SaveGame->SchemaVersion = 3;
    SaveGame->PersistentPlayerId = GetPersistenceSlotName();
    SaveGame->GoldBalance = FMath::Max<int64>(0, GoldBalance);
    SaveGame->Inventory.Reset();

    for (const FEROInventoryStack& Stack : Inventory)
    {
        if (Stack.ItemId.IsNone() || Stack.Quantity <= 0)
        {
            continue;
        }

        if (SaveGame->Inventory.Num() >= MaxInventoryEntries)
        {
            break;
        }

        FEROInventoryStack& SafeStack = SaveGame->Inventory.AddDefaulted_GetRef();
        SafeStack.ItemId = Stack.ItemId;
        SafeStack.Quantity = FMath::Clamp(Stack.Quantity, 1, MaxStackQuantity);
    }

    if (const AEROPlayerCharacter* Character = Cast<AEROPlayerCharacter>(GetPawn()))
    {
        SaveGame->CharacterLevel = FMath::Clamp(Character->Level, 1, 100);
        SaveGame->CharacterExperience = FMath::Max<int64>(0, Character->Experience);
        SaveGame->CharacterClass = Character->PlayerClass;
        SaveGame->EquippedWeaponId = Character->EquippedWeaponId;
        SaveGame->EquippedWeaponFamily = Character->EquippedWeaponFamily;
    }

    UGameplayStatics::SaveGameToSlot(SaveGame, GetPersistenceSlotName(), 0);
}

void AEROPlayerEconomyState::LoadPersistentEconomyState()
{
    const FString SlotName = GetPersistenceSlotName();
    if (!UGameplayStatics::DoesSaveGameExist(SlotName, 0))
    {
        return;
    }

    UEROPlayerEconomySaveGame* SaveGame = Cast<UEROPlayerEconomySaveGame>(UGameplayStatics::LoadGameFromSlot(SlotName, 0));
    if (!SaveGame || SaveGame->SchemaVersion < 1 || SaveGame->SchemaVersion > 3)
    {
        return;
    }

    if (SaveGame->SchemaVersion >= 3 && !SaveGame->PersistentPlayerId.IsEmpty() && SaveGame->PersistentPlayerId != SlotName)
    {
        UE_LOG(LogTemp, Warning, TEXT("Rejected ERO economy save because its persistent identity does not match the authenticated player slot."));
        return;
    }

    GoldBalance = FMath::Max<int64>(0, SaveGame->GoldBalance);
    Inventory.Reset();

    for (const FEROInventoryStack& Stack : SaveGame->Inventory)
    {
        if (Stack.ItemId.IsNone() || Stack.Quantity <= 0 || Inventory.Num() >= MaxInventoryEntries)
        {
            continue;
        }

        FEROInventoryStack& SafeStack = Inventory.AddDefaulted_GetRef();
        SafeStack.ItemId = Stack.ItemId;
        SafeStack.Quantity = FMath::Clamp(Stack.Quantity, 1, MaxStackQuantity);
    }
}

void AEROPlayerEconomyState::RestorePersistentCharacterState()
{
    if (!HasAuthority())
    {
        return;
    }

    const FString SlotName = GetPersistenceSlotName();
    if (!UGameplayStatics::DoesSaveGameExist(SlotName, 0))
    {
        return;
    }

    const UEROPlayerEconomySaveGame* SaveGame = Cast<UEROPlayerEconomySaveGame>(UGameplayStatics::LoadGameFromSlot(SlotName, 0));
    if (!SaveGame || SaveGame->SchemaVersion < 2)
    {
        return;
    }

    if (SaveGame->SchemaVersion >= 3 && !SaveGame->PersistentPlayerId.IsEmpty() && SaveGame->PersistentPlayerId != SlotName)
    {
        UE_LOG(LogTemp, Warning, TEXT("Rejected ERO character restore because its persistent identity does not match the authenticated player slot."));
        return;
    }

    if (AEROPlayerCharacter* Character = Cast<AEROPlayerCharacter>(GetPawn()))
    {
        Character->RestorePersistentProgression(
            SaveGame->CharacterLevel,
            SaveGame->CharacterExperience,
            SaveGame->CharacterClass,
            SaveGame->EquippedWeaponId,
            SaveGame->EquippedWeaponFamily);
    }
}

FString AEROPlayerEconomyState::GetPersistenceSlotName() const
{
    // Prefer the authenticated online-subsystem identity. PlayerId is only a
    // deterministic standalone/editor fallback and must never overwrite an
    // authenticated identity's slot.
    const FUniqueNetIdRepl& UniqueId = GetUniqueId();
    if (UniqueId.IsValid())
    {
        const FString Identity = UniqueId.ToString();
        if (!Identity.IsEmpty())
        {
            // Keep the filesystem-safe slot deterministic without exposing the
            // raw platform/account identifier in a save filename.
            return FString::Printf(TEXT("ERO_Economy_Online_%08X"), FCrc::StrCrc32(*Identity));
        }
    }

    const int32 SafePlayerId = FMath::Max(0, GetPlayerId());
    return FString::Printf(TEXT("ERO_Economy_PlayerId_%d"), SafePlayerId);
}
