#include "EROPlayerEconomyState.h"

#include "EROPlayerEconomySaveGame.h"
#include "Kismet/GameplayStatics.h"
#include "Net/UnrealNetwork.h"

AEROPlayerEconomyState::AEROPlayerEconomyState()
{
    bReplicates = true;
}

void AEROPlayerEconomyState::GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const
{
    Super::GetLifetimeReplicatedProps(OutLifetimeReplicatedProps);
    DOREPLIFETIME(AEROPlayerEconomyState, GoldBalance);
    DOREPLIFETIME(AEROPlayerEconomyState, Inventory);
}

void AEROPlayerEconomyState::BeginPlay()
{
    Super::BeginPlay();

    if (HasAuthority())
    {
        LoadPersistentEconomyState();
    }
}

void AEROPlayerEconomyState::EndPlay(const EEndPlayReason::Type EndPlayReason)
{
    if (HasAuthority())
    {
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

    constexpr int32 MaxInventoryEntries = 128;
    constexpr int32 MaxStackQuantity = 9999;

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

    SaveGame->SchemaVersion = 1;
    SaveGame->GoldBalance = FMath::Max<int64>(0, GoldBalance);
    SaveGame->Inventory.Reset();

    constexpr int32 MaxInventoryEntries = 128;
    constexpr int32 MaxStackQuantity = 9999;
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
    if (!SaveGame || SaveGame->SchemaVersion != 1)
    {
        return;
    }

    GoldBalance = FMath::Max<int64>(0, SaveGame->GoldBalance);
    Inventory.Reset();

    constexpr int32 MaxInventoryEntries = 128;
    constexpr int32 MaxStackQuantity = 9999;
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

FString AEROPlayerEconomyState::GetPersistenceSlotName() const
{
    FString Identity;
    const FUniqueNetIdRepl UniqueId = GetUniqueId();
    if (UniqueId.IsValid())
    {
        Identity = UniqueId.ToString();
    }

    if (Identity.IsEmpty())
    {
        Identity = FString::Printf(TEXT("PlayerId_%d"), GetPlayerId());
    }

    Identity.ReplaceInline(TEXT(":"), TEXT("_"));
    Identity.ReplaceInline(TEXT("/"), TEXT("_"));
    Identity.ReplaceInline(TEXT("\\"), TEXT("_"));
    return FString::Printf(TEXT("ERO_Economy_%s"), *Identity);
}
