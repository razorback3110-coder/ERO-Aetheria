#include "EROEnvironmentActor.h"

#include "Engine/StaticMesh.h"
#include "Components/StaticMeshComponent.h"
#include "UObject/UObjectGlobals.h"

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

void AEROEnvironmentActor::AddStaticMesh(
    UStaticMesh* Mesh,
    const FVector& Location,
    const FVector& Scale,
    const FRotator& Rotation)
{
    if (!Mesh)
    {
        return;
    }

    UStaticMeshComponent* Component = NewObject<UStaticMeshComponent>(this);
    Component->SetStaticMesh(Mesh);
    Component->SetMobility(EComponentMobility::Movable);
    Component->SetCollisionEnabled(ECollisionEnabled::QueryAndPhysics);
    Component->SetIsReplicated(true);
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

    // No runtime DirectionalLight/SkyLight/Fog. Visual presentation is now
    // editor/asset driven with Lumen + World Partition + PCG. This removes
    // the recurring Light/lighting rebuild problem from gameplay runtime.

    AddStaticMesh(Plane, FVector(0.0f, 0.0f, -2.0f), FVector(50.0f, 50.0f, 1.0f));
    AddStaticMesh(Cube, FVector(0.0f, 0.0f, 20.0f), FVector(8.0f, 8.0f, 0.4f));

    const FVector Entrances[] =
    {
        FVector(0.0f, 1400.0f, 40.0f),
        FVector(0.0f, -1400.0f, 40.0f),
        FVector(1400.0f, 0.0f, 40.0f),
        FVector(-1400.0f, 0.0f, 40.0f)
    };

    for (const FVector& Location : Entrances)
    {
        AddStaticMesh(Cube, Location, FVector(4.0f, 8.0f, 0.8f));
    }

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
}
