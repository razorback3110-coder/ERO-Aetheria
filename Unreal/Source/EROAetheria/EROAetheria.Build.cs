using UnrealBuildTool;

public class EROAetheria : ModuleRules
{
    public EROAetheria(ReadOnlyTargetRules Target) : base(Target)
    {
        PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;

        PublicDependencyModuleNames.AddRange(new[]
        {
            "Core",
            "CoreUObject",
            "Engine",
            "InputCore",
            "EnhancedInput",
            "GameplayAbilities",
            "GameplayTags",
            "GameplayTasks",
            "NetCore",
            "MassEntity",
            "Niagara",
            "OnlineSubsystem"
        });
    }
}
