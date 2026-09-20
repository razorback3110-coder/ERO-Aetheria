using UnrealBuildTool;

public class EROAetheriaEditorTarget : TargetRules
{
    public EROAetheriaEditorTarget(TargetInfo Target) : base(Target)
    {
        Type = TargetType.Editor;
        DefaultBuildSettings = BuildSettingsVersion.V6;
        IncludeOrderVersion = EngineIncludeOrderVersion.Unreal5_8;
        UnreachableWarningLevel = WarningLevel.Error;
        ReturnTypeWarningLevel = WarningLevel.Error;
        DanglingElseWarningLevel = WarningLevel.Error;
        ExtraModuleNames.Add("EROAetheria");
    }
}
