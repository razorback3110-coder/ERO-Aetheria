#include "EROEnvironmentActor.h"

#include "Components/DirectionalLightComponent.h"
#include "Components/SceneComponent.h"
#include "Components/SkyLightComponent.h"
#include "Engine/DirectionalLight.h"
#include "Engine/SkyLight.h"
#include "Engine/StaticMesh.h"
#include "Components/StaticMeshComponent.h"
#include "UObject/ConstructorHelpers.h"

AEROEnvironmentActor::AEROEnvironmentActor()
{
    bReplicates = true;
    PrimaryActorTick.bCanEverTick = false;

    USceneComponent* Root = CreateDefaultSubobject<USceneComponent>(TEXT("EnvironmentRoot"));
    SetRootComponent(Root);
}

void AEROEnvironmentActor::BeginPlay()
{
    Super::BeginPlay();
    BuildEnvironment();
}

UStaticMesh* AEROEnvironmentActor::LoadMesh(const TCHAR* AssetPath) const
{
    static ConstructorHelpers::FObjectFinder<UStaticMesh> PlaneMesh(TEXT("/Engine/BasicShapes/Plane.Plane"));
    static ConstructorHelpers::FObjectFinder<UStaticMesh> CubeMesh(TEXT("/Engine/BasicShapes/Cube.Cube"));
    static ConstructorHelpers::FObjectFinder<UStaticMesh> SphereMesh(TEXT("/Engine/BasicShapes/Sphere.Sphere"));

    if (FCString::Stricmp(AssetPath, TEXT("Plane")) == 0)
    {
        return PlaneMesh.Succeeded() ? PlaneMesh.Object : nullptr;
    }

    if (FCString::Stricmp(AssetPath, TEXT("Sphere")) == 0)
    {
        return SphereMesh.Succeeded() ? SphereMesh.Object : nullptr;
    }

    return CubeMesh.Succeeded() ? CubeMesh.Object : nullptr;
}

void AEROEnvironmentActor::AddStaticMesh(UStaticMesh* Mesh, const FVector& Location, const FVector& Scale, const FRotator& Rotation)
{
    if (!Mesh)
    {
        return;
    }

    UStaticMeshComponent* Component = NewObject<UStaticMeshComponent>(this);
    Component->SetStaticMesh(Mesh);
    Component->SetMobility(EComponentMobility::Static);
    Component->SetCollisionEnabled(ECollisionEnabled::QueryAndPhysics);
    Component->SetIsReplicated(true);
    Component->SetupAttachment(GetRootComponent());
    Component->SetRelativeLocation(Location);
    Component->SetRelativeRotation(Rotation);
    Component->SetRelativeScale3D(Scale);
    AddInstanceComponent(Component);
    Component->RegisterComponent();
}

void AEROEnvironmentActor::BuildEnvironment()
{
    if (!HasAuthority())
    {
        return;
    }

    UStaticMesh* Plane = LoadMesh(TEXT("Plane"));
    UStaticMesh* Cube = LoadMesh(TEXT("Cube"));
    UStaticMesh* Sphere = LoadMesh(TEXT("Sphere"));

    // Main playable ground: 50m x 50m.
    AddStaticMesh(Plane, FVector(0.0f, 0.0f, -2.0f), FVector(50.0f, 50.0f, 1.0f));

    // Central plaza.
    AddStaticMesh(Cube, FVector(0.0f, 0.0f, 20.0f), FVector(8.0f, 8.0f, 0.4f));

    // Four entrance platforms.
    AddStaticMesh(Cube, FVector(0.0f, 1400.0f, 40.0f), FVector(4.0f, 8.0f, 0.8f));
    AddStaticMesh(Cube, FVector(0.0f, -1400.0f, 40.0f), FVector(4.0f, 8.0f, 0.8f));
    AddStaticMesh(Cube, FVector(1400.0f, 0.0f, 40.0f), FVector(8.0f, 4.0f, 0.8f));
    AddStaticMesh(Cube, FVector(-1400.0f, 0.0f, 40.0f), FVector(8.0f, 4.0f, 0.8f));

    // Simple landmark pillars around the plaza.
    const FVector Pillars[] =
    {
        FVector(900.0f, 900.0f, 250.0f),
        FVector(-900.0f, 900.0f, 250.0f),
        FVector(900.0f, -900.0f, 250.0f),
        FVector(-900.0f, -900.0f, 250.0f)
    };

    for (const FVector& Location : Pillars)
    {
        AddStaticMesh(Cube, Location, FVector(1.5f, 1.5f, 5.0f));
        AddStaticMesh(Sphere, Location + FVector(0.0f, 0.0f, 650.0f), FVector(2.2f, 2.2f, 2.2f));
    }

    // Directional sun.
    ADirectionalLight* Sun = GetWorld()->SpawnActor<ADirectionalLight>(
        FVector(0.0f, 0.0f, 1800.0f),
        FRotator(-45.0f, -35.0f, 0.0f));

    if (Sun && Sun->GetLightComponent())
    {
        Sun->SetReplicates(false);
        Sun->GetLightComponent()->SetIntensity(6.0f);
        Sun->GetLightComponent()->SetCastShadows(true);
    }

    // Skylight keeps the plaza readable even before a final sky asset is installed.
    ASkyLight* Sky = GetWorld()->SpawnActor<ASkyLight>(FVector(0.0f, 0.0f, 1000.0f), FRotator::ZeroRotator);
    if (Sky && Sky->GetLightComponent())
    {
        Sky->SetReplicates(false);
        Sky->GetLightComponent()->SetIntensity(1.0f);
        Sky->GetLightComponent()->RecaptureSky();
    }
}
