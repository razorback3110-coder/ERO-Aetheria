#include "EROEnvironmentActor.h"

#include "Components/DirectionalLightComponent.h"
#include "Components/ExponentialHeightFogComponent.h"
#include "Components/SceneComponent.h"
#include "Components/SkyLightComponent.h"
#include "Engine/DirectionalLight.h"
#include "Engine/ExponentialHeightFog.h"
#include "Engine/SkyAtmosphere.h"
#include "Engine/SkyLight.h"
#include "Engine/StaticMesh.h"
#include "Components/StaticMeshComponent.h"
#include "UObject/SoftObjectPath.h"

AEROEnvironmentActor::AEROEnvironmentActor()
{
    bReplicates = true;
    PrimaryActorTick.bCanEverTick = false;

    USceneComponent* Root = CreateDefaultSubobject<USceneComponent>(TEXT("EnvironmentRoot"));
    Root->SetMobility(EComponentMobility::Movable);
    SetRootComponent(Root);
}

void AEROEnvironmentActor::BeginPlay()
{
    Super::BeginPlay();
    BuildEnvironment();
}

UStaticMesh* AEROEnvironmentActor::LoadMesh(const TCHAR* AssetPath) const
{
    const TCHAR* ObjectPath = TEXT("/Engine/BasicShapes/Cube.Cube");

    if (FCString::Stricmp(AssetPath, TEXT("Plane")) == 0)
    {
        ObjectPath = TEXT("/Engine/BasicShapes/Plane.Plane");
    }
    else if (FCString::Stricmp(AssetPath, TEXT("Sphere")) == 0)
    {
        ObjectPath = TEXT("/Engine/BasicShapes/Sphere.Sphere");
    }

    return LoadObject<UStaticMesh>(nullptr, ObjectPath);
}

void AEROEnvironmentActor::AddStaticMesh(UStaticMesh* Mesh, const FVector& Location, const FVector& Scale, const FRotator& Rotation)
{
    if (!Mesh)
    {
        return;
    }

    UStaticMeshComponent* Component = NewObject<UStaticMeshComponent>(this);
    Component->SetStaticMesh(Mesh);

    // These meshes are created at runtime, so they must not participate in
    // precomputed static lighting. Movable avoids "lighting needs to be rebuilt"
    // warnings while we are still using the procedural prototype environment.
    Component->SetMobility(EComponentMobility::Movable);
    Component->SetCollisionEnabled(ECollisionEnabled::QueryAndPhysics);
    Component->SetIsReplicated(true);
    // Runtime-generated components are intentionally not attached to the
    // actor root. This avoids Unreal's static/movable attachment validation
    // for components created during BeginPlay.
    Component->SetWorldLocation(Location);
    Component->SetWorldRotation(Rotation);
    Component->SetWorldScale3D(Scale);
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

    // Runtime sky atmosphere gives the prototype a real sky instead of a
    // black unlit background.
    ASkyAtmosphere* Atmosphere = GetWorld()->SpawnActor<ASkyAtmosphere>(
        FVector::ZeroVector,
        FRotator::ZeroRotator);

    // Directional sun.
    ADirectionalLight* Sun = GetWorld()->SpawnActor<ADirectionalLight>(
        FVector(0.0f, 0.0f, 1800.0f),
        FRotator(-45.0f, -35.0f, 0.0f));

    if (Sun && Sun->GetLightComponent())
    {
        Sun->SetReplicates(false);
        Sun->GetLightComponent()->SetMobility(EComponentMobility::Movable);
        Sun->GetLightComponent()->SetIntensity(6.0f);
        Sun->GetLightComponent()->SetCastShadows(true);
        Sun->GetLightComponent()->SetAtmosphereSunLight(true);
    }

    // Movable skylight avoids static-light build requirements for the
    // runtime-generated prototype.
    ASkyLight* Sky = GetWorld()->SpawnActor<ASkyLight>(
        FVector(0.0f, 0.0f, 1000.0f),
        FRotator::ZeroRotator);

    if (Sky && Sky->GetLightComponent())
    {
        Sky->SetReplicates(false);
        Sky->GetLightComponent()->SetMobility(EComponentMobility::Movable);
        Sky->GetLightComponent()->SetIntensity(1.0f);
        Sky->GetLightComponent()->RecaptureSky();
    }

    // A small amount of height fog provides depth while we are still using
    // placeholder geometry. It will be replaced by the final world
    // atmosphere during the visual pass.
    AExponentialHeightFog* Fog = GetWorld()->SpawnActor<AExponentialHeightFog>(
        FVector(0.0f, 0.0f, 0.0f),
        FRotator::ZeroRotator);

    if (Fog && Fog->GetComponent())
    {
        Fog->SetReplicates(false);
        Fog->GetComponent()->SetMobility(EComponentMobility::Movable);
        Fog->GetComponent()->FogDensity = 0.008f;
        Fog->GetComponent()->FogHeightFalloff = 0.2f;
        Fog->GetComponent()->bOverrideLightColorsWithFogInscatteringColors = false;
    }
}
