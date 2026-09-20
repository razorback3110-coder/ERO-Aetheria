#include "EROPlayerCharacter.h"

#include "Camera/CameraComponent.h"
#include "Components/CapsuleComponent.h"
#include "GameFramework/SpringArmComponent.h"
#include "GameFramework/CharacterMovementComponent.h"
#include "Kismet/GameplayStatics.h"
#include "Net/UnrealNetwork.h"

AEROPlayerCharacter::AEROPlayerCharacter()
{
    bReplicates = true;
    SetReplicateMovement(true);

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

    CurrentHealth = MaxHealth;
}

void AEROPlayerCharacter::GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const
{
    Super::GetLifetimeReplicatedProps(OutLifetimeProps);
    DOREPLIFETIME(AEROPlayerCharacter, PlayerClass);
    DOREPLIFETIME(AEROPlayerCharacter, Level);
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
    if (Controller && !FMath::IsNearlyZero(Value))
    {
        const FRotator YawRotation(0.0f, Controller->GetControlRotation().Yaw, 0.0f);
        AddMovementInput(FRotationMatrix(YawRotation).GetUnitAxis(EAxis::X), Value);
    }
}

void AEROPlayerCharacter::MoveRight(float Value)
{
    if (Controller && !FMath::IsNearlyZero(Value))
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
    if (CanAttack())
    {
        ServerAttack();
    }
}

void AEROPlayerCharacter::ServerAttack_Implementation()
{
    if (!CanAttack())
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

float AEROPlayerCharacter::TakeDamage(float DamageAmount, const FDamageEvent& DamageEvent, AController* EventInstigator, AActor* DamageCauser)
{
    const float AppliedDamage = FMath::Clamp(DamageAmount, 0.0f, CurrentHealth);
    if (HasAuthority() && AppliedDamage > 0.0f)
    {
        CurrentHealth -= AppliedDamage;
        if (CurrentHealth <= 0.0f)
        {
            CurrentHealth = 0.0f;
            DisableInput(nullptr);
        }
    }
    return AppliedDamage;
}
