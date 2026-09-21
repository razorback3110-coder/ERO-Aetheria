#include "EROPlayerCharacter.h"

#include "AbilitySystemComponent.h"
#include "EROAttributeSet.h"

#include "Camera/CameraComponent.h"
#include "Components/CapsuleComponent.h"
#include "GameFramework/SpringArmComponent.h"
#include "GameFramework/CharacterMovementComponent.h"
#include "GameFramework/GameModeBase.h"
#include "GameFramework/PlayerController.h"
#include "Kismet/GameplayStatics.h"
#include "Net/UnrealNetwork.h"

AEROPlayerCharacter::AEROPlayerCharacter()
{
    bReplicates = true;
    SetReplicateMovement(true);

    AbilitySystemComponent = CreateDefaultSubobject<UAbilitySystemComponent>(TEXT("AbilitySystemComponent"));
    AbilitySystemComponent->SetIsReplicated(true);
    AbilitySystemComponent->SetReplicationMode(EGameplayEffectReplicationMode::Mixed);

    AttributeSet = CreateDefaultSubobject<UEROAttributeSet>(TEXT("AttributeSet"));

    GetCapsuleComponent()->InitCapsuleSize(42.0f, 96.0f);
    GetCharacterMovement()->MaxWalkSpeed = 500.0f;
    GetCharacterMovement()->BrakingDecelerationWalking = 1800.0f;

    USpringArmComponent* SpringArm = CreateDefaultSubobject<USpringArmComponent>(TEXT("CameraBoom"));
    SpringArm->SetupAttachment(GetRootComponent());
    SpringArm->TargetArmLength = 450.0f;
    SpringArm->bUsePawnControlRotation = true;

    UCameraComponent* Camera = CreateDefaultSubobject<UCameraComponent>(TEXT("FollowCamera"));
    Camera->SetupAttachment(SpringArm, USpringArmComponent::SocketName);
    Camera->bUsePawnControlRotation = false;

    ApplyClassProfile();
    CurrentHealth = MaxHealth;
    SyncAttributesFromLegacyProfile();
}

UAbilitySystemComponent* AEROPlayerCharacter::GetAbilitySystemComponent() const
{
    return AbilitySystemComponent;
}

void AEROPlayerCharacter::BeginPlay()
{
    Super::BeginPlay();
    SyncAttributesFromLegacyProfile();
}

void AEROPlayerCharacter::GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const
{
    Super::GetLifetimeReplicatedProps(OutLifetimeProps);
    DOREPLIFETIME(AEROPlayerCharacter, PlayerClass);
    DOREPLIFETIME(AEROPlayerCharacter, AppearanceId);
    DOREPLIFETIME(AEROPlayerCharacter, EquippedWeaponFamily);
    DOREPLIFETIME(AEROPlayerCharacter, Level);
    DOREPLIFETIME(AEROPlayerCharacter, Experience);
    DOREPLIFETIME(AEROPlayerCharacter, bDefeated);
    DOREPLIFETIME(AEROPlayerCharacter, CurrentHealth);
}

void AEROPlayerCharacter::SetupPlayerInputComponent(UInputComponent* PlayerInputComponent)
{
    Super::SetupPlayerInputComponent(PlayerInputComponent);

    PlayerInputComponent->BindAxis(TEXT("MoveForward"), this, &AEROPlayerCharacter::MoveForward);
    PlayerInputComponent->BindAxis(TEXT("MoveRight"), this, &AEROPlayerCharacter::MoveRight);
    PlayerInputComponent->BindAxis(TEXT("LookUp"), this, &AEROPlayerCharacter::LookUp);
    PlayerInputComponent->BindAxis(TEXT("Turn"), this, &AEROPlayerCharacter::Turn);
    PlayerInputComponent->BindAction(TEXT("Attack"), IE_Pressed, this, &AEROPlayerCharacter::Attack);
}

void AEROPlayerCharacter::MoveForward(float Value)
{
    if (Controller && !FMath::IsNearlyZero(Value) && !bDefeated)
    {
        const FRotator YawRotation(0.0f, Controller->GetControlRotation().Yaw, 0.0f);
        AddMovementInput(FRotationMatrix(YawRotation).GetUnitAxis(EAxis::X), Value);
    }
}

void AEROPlayerCharacter::MoveRight(float Value)
{
    if (Controller && !FMath::IsNearlyZero(Value) && !bDefeated)
    {
        const FRotator YawRotation(0.0f, Controller->GetControlRotation().Yaw, 0.0f);
        AddMovementInput(FRotationMatrix(YawRotation).GetUnitAxis(EAxis::Y), Value);
    }
}

void AEROPlayerCharacter::LookUp(float Value)
{
    AddControllerPitchInput(Value);
}

void AEROPlayerCharacter::Turn(float Value)
{
    AddControllerYawInput(Value);
}

void AEROPlayerCharacter::Attack()
{
    if (!bDefeated && CanAttack())
    {
        ServerAttack();
    }
}

void AEROPlayerCharacter::ServerAttack_Implementation()
{
    if (bDefeated || !CanAttack())
    {
        return;
    }

    LastAttackServerTime = GetWorld()->GetTimeSeconds();

    const FVector Start = GetActorLocation() + FVector(0.0f, 0.0f, 60.0f);
    const FVector End = Start + GetActorForwardVector() * AttackRange;
    const FCollisionShape Shape = FCollisionShape::MakeSphere(60.0f);

    FCollisionQueryParams QueryParams(SCENE_QUERY_STAT(EROPlayerAttack), false, this);
    FHitResult Hit;
    if (GetWorld()->SweepSingleByChannel(Hit, Start, End, FQuat::Identity, ECC_Pawn, Shape, QueryParams))
    {
        AActor* Target = Hit.GetActor();
        if (IsValid(Target) && Target != this)
        {
            UGameplayStatics::ApplyDamage(Target, AttackDamage, GetController(), this, UDamageType::StaticClass());
        }
    }
}

bool AEROPlayerCharacter::CanAttack() const
{
    const UWorld* World = GetWorld();
    return World && HasAuthority() ? (World->GetTimeSeconds() - LastAttackServerTime >= AttackCooldown) : true;
}

void AEROPlayerCharacter::ResetAttackCooldown()
{
    LastAttackServerTime = -BIG_NUMBER;
}

bool AEROPlayerCharacter::CanSelectClass() const
{
    return HasAuthority() && Level >= 18 && !bDefeated;
}

void AEROPlayerCharacter::ServerSelectClass_Implementation(EEROPlayerClass RequestedClass)
{
    if (!CanSelectClass() || RequestedClass == PlayerClass)
    {
        return;
    }

    PlayerClass = RequestedClass;
    ApplyClassProfile();
    CurrentHealth = MaxHealth;
}

bool AEROPlayerCharacter::CanSelectWeaponSpecialization() const
{
    return HasAuthority() && Level >= 18 && !bDefeated;
}

bool AEROPlayerCharacter::IsWeaponFamilyAllowed(EEROWeaponFamily WeaponFamily) const
{
    switch (PlayerClass)
    {
    case EEROPlayerClass::Warrior:
        return WeaponFamily == EEROWeaponFamily::Sword || WeaponFamily == EEROWeaponFamily::Greatsword ||
               WeaponFamily == EEROWeaponFamily::Axe || WeaponFamily == EEROWeaponFamily::Shield;
    case EEROPlayerClass::Ranger:
        return WeaponFamily == EEROWeaponFamily::Bow || WeaponFamily == EEROWeaponFamily::Crossbow ||
               WeaponFamily == EEROWeaponFamily::DualBlades;
    case EEROPlayerClass::Mage:
        return WeaponFamily == EEROWeaponFamily::Staff || WeaponFamily == EEROWeaponFamily::Wand ||
               WeaponFamily == EEROWeaponFamily::Orb || WeaponFamily == EEROWeaponFamily::Grimoire;
    case EEROPlayerClass::Assassin:
        return WeaponFamily == EEROWeaponFamily::Dagger || WeaponFamily == EEROWeaponFamily::DualBlades ||
               WeaponFamily == EEROWeaponFamily::Fist;
    case EEROPlayerClass::Cleric:
        return WeaponFamily == EEROWeaponFamily::Mace || WeaponFamily == EEROWeaponFamily::Staff ||
               WeaponFamily == EEROWeaponFamily::Wand || WeaponFamily == EEROWeaponFamily::Shield;
    case EEROPlayerClass::Paladin:
        return WeaponFamily == EEROWeaponFamily::Sword || WeaponFamily == EEROWeaponFamily::Greatsword ||
               WeaponFamily == EEROWeaponFamily::Mace || WeaponFamily == EEROWeaponFamily::Shield;
    case EEROPlayerClass::Warlock:
        return WeaponFamily == EEROWeaponFamily::Grimoire || WeaponFamily == EEROWeaponFamily::Wand ||
               WeaponFamily == EEROWeaponFamily::Staff || WeaponFamily == EEROWeaponFamily::Orb;
    case EEROPlayerClass::Summoner:
        return WeaponFamily == EEROWeaponFamily::Staff || WeaponFamily == EEROWeaponFamily::Orb ||
               WeaponFamily == EEROWeaponFamily::Wand || WeaponFamily == EEROWeaponFamily::Grimoire;
    default:
        return false;
    }
}

void AEROPlayerCharacter::ServerSelectWeaponSpecialization_Implementation(EEROWeaponFamily RequestedWeapon)
{
    if (!CanSelectWeaponSpecialization() || !IsWeaponFamilyAllowed(RequestedWeapon))
    {
        return;
    }

    EquippedWeaponFamily = RequestedWeapon;
    ApplyClassAppearanceProfile();
}

void AEROPlayerCharacter::GrantExperience(int64 Amount)
{
    if (!HasAuthority() || Amount <= 0 || bDefeated)
    {
        return;
    }

    Experience = FMath::Max<int64>(0, Experience) + Amount;
    while (Level < 100 && Experience >= ExperienceForNextLevel())
    {
        Experience -= ExperienceForNextLevel();
        ++Level;
        ApplyClassProfile();
        CurrentHealth = MaxHealth;
    }
}

void AEROPlayerCharacter::ApplyClassProfile()
{
    const float ClassHealthBonus = [this]()
    {
        switch (PlayerClass)
        {
        case EEROPlayerClass::Warrior: return 150.0f;
        case EEROPlayerClass::Ranger: return 100.0f;
        case EEROPlayerClass::Mage: return 80.0f;
        case EEROPlayerClass::Assassin: return 90.0f;
        case EEROPlayerClass::Cleric: return 120.0f;
        case EEROPlayerClass::Paladin: return 180.0f;
        case EEROPlayerClass::Warlock: return 90.0f;
        case EEROPlayerClass::Summoner: return 100.0f;
        default: return 100.0f;
        }
    }();

    const float ClassDamageBonus = [this]()
    {
        switch (PlayerClass)
        {
        case EEROPlayerClass::Warrior: return 30.0f;
        case EEROPlayerClass::Ranger: return 34.0f;
        case EEROPlayerClass::Mage: return 42.0f;
        case EEROPlayerClass::Assassin: return 38.0f;
        case EEROPlayerClass::Cleric: return 22.0f;
        case EEROPlayerClass::Paladin: return 26.0f;
        case EEROPlayerClass::Warlock: return 40.0f;
        case EEROPlayerClass::Summoner: return 32.0f;
        default: return 25.0f;
        }
    }();

    MaxHealth = ClassHealthBonus + static_cast<float>(Level - 1) * 12.0f;
    AttackDamage = ClassDamageBonus + static_cast<float>(Level - 1) * 2.5f;

    switch (PlayerClass)
    {
    case EEROPlayerClass::Warrior: EquippedWeaponFamily = EEROWeaponFamily::Greatsword; break;
    case EEROPlayerClass::Ranger: EquippedWeaponFamily = EEROWeaponFamily::Bow; break;
    case EEROPlayerClass::Mage: EquippedWeaponFamily = EEROWeaponFamily::Staff; break;
    case EEROPlayerClass::Assassin: EquippedWeaponFamily = EEROWeaponFamily::DualBlades; break;
    case EEROPlayerClass::Cleric: EquippedWeaponFamily = EEROWeaponFamily::Mace; break;
    case EEROPlayerClass::Paladin: EquippedWeaponFamily = EEROWeaponFamily::Sword; break;
    case EEROPlayerClass::Warlock: EquippedWeaponFamily = EEROWeaponFamily::Grimoire; break;
    case EEROPlayerClass::Summoner: EquippedWeaponFamily = EEROWeaponFamily::Orb; break;
    default: break;
    }

    ApplyClassAppearanceProfile();
    SyncAttributesFromLegacyProfile();
}

void AEROPlayerCharacter::ApplyClassAppearanceProfile()
{
    const TCHAR* ClassName = TEXT("Warrior");
    switch (PlayerClass)
    {
    case EEROPlayerClass::Warrior: ClassName = TEXT("Warrior"); break;
    case EEROPlayerClass::Ranger: ClassName = TEXT("Ranger"); break;
    case EEROPlayerClass::Mage: ClassName = TEXT("Mage"); break;
    case EEROPlayerClass::Assassin: ClassName = TEXT("Assassin"); break;
    case EEROPlayerClass::Cleric: ClassName = TEXT("Cleric"); break;
    case EEROPlayerClass::Paladin: ClassName = TEXT("Paladin"); break;
    case EEROPlayerClass::Warlock: ClassName = TEXT("Warlock"); break;
    case EEROPlayerClass::Summoner: ClassName = TEXT("Summoner"); break;
    }

    AppearanceId = FName(*FString::Printf(TEXT("%s_Base"), ClassName));
}

void AEROPlayerCharacter::SyncAttributesFromLegacyProfile()
{
    if (!AttributeSet || !AbilitySystemComponent || !HasAuthority())
    {
        return;
    }

    AttributeSet->SetMaxHealth(MaxHealth);
    AttributeSet->SetHealth(CurrentHealth);
    AttributeSet->SetMaxMana(100.0f + static_cast<float>(Level - 1) * 8.0f);
    AttributeSet->SetMana(AttributeSet->GetMaxMana());
    AttributeSet->SetAttack(AttackDamage);
    AttributeSet->SetMagicAttack(AttackDamage * 1.25f);
    AttributeSet->SetDefense(10.0f + static_cast<float>(Level - 1) * 1.5f);
    AttributeSet->SetMagicDefense(10.0f + static_cast<float>(Level - 1) * 1.5f);
    AttributeSet->SetCritChance(0.05f);
    AttributeSet->SetMoveSpeed(GetCharacterMovement() ? GetCharacterMovement()->MaxWalkSpeed : 500.0f);
}

int64 AEROPlayerCharacter::ExperienceForNextLevel() const
{
    const int64 SafeLevel = FMath::Clamp<int64>(Level, 1, 100);
    return 1000 + ((SafeLevel - 1) * 750);
}

void AEROPlayerCharacter::RespawnAfterDeath()
{
    if (!HasAuthority() || !bDefeated || !Controller)
    {
        return;
    }

    bDefeated = false;
    CurrentHealth = MaxHealth;
    SyncAttributesFromLegacyProfile();
    GetCharacterMovement()->SetMovementMode(MOVE_Walking);
    SetActorHiddenInGame(false);
    SetActorEnableCollision(true);

    if (APlayerController* PlayerController = Cast<APlayerController>(Controller))
    {
        EnableInput(PlayerController);
    }

    if (AGameModeBase* GameMode = GetWorld()->GetAuthGameMode())
    {
        if (AActor* StartSpot = GameMode->FindPlayerStart(Controller))
        {
            SetActorLocationAndRotation(StartSpot->GetActorLocation(), StartSpot->GetActorRotation(), false, nullptr, ETeleportType::TeleportPhysics);
        }
    }
}

float AEROPlayerCharacter::TakeDamage(float DamageAmount, const FDamageEvent& DamageEvent, AController* EventInstigator, AActor* DamageCauser)
{
    if (!HasAuthority() || bDefeated)
    {
        return 0.0f;
    }

    const float AppliedDamage = FMath::Clamp(DamageAmount, 0.0f, CurrentHealth);
    if (AppliedDamage <= 0.0f)
    {
        return 0.0f;
    }

    CurrentHealth -= AppliedDamage;
    if (CurrentHealth <= 0.0f)
    {
        CurrentHealth = 0.0f;
        if (AttributeSet && HasAuthority())
        {
            AttributeSet->SetHealth(0.0f);
        }
        bDefeated = true;
        GetCharacterMovement()->DisableMovement();
        DisableInput(Cast<APlayerController>(Controller));

        GetWorldTimerManager().SetTimer(RespawnTimerHandle, this, &AEROPlayerCharacter::RespawnAfterDeath, RespawnDelay, false);
    }

    return AppliedDamage;
}
