using UnrealBuildTool;

public class EROAetheriaServerTarget : TargetRules
{
    public EROAetheriaServerTarget(TargetInfo Target) : base(Target)
    {
        Type = TargetType.Server;
        DefaultBuildSettings = BuildSettingsVersion.V6;
        IncludeOrderVersion = EngineIncludeOrderVersion.Unreal5_8;
        UnreachableWarningLevel = WarningLevel.Error;
        ReturnTypeWarningLevel = WarningLevel.Error;
        DanglingElseWarningLevel = WarningLevel.Error;
        ExtraModuleNames.Add("EROAetheria");
    }
}
