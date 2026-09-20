using UnrealBuildTool;

public class EROAetheriaEditorTarget : TargetRules
{
    public EROAetheriaEditorTarget(TargetInfo Target) : base(Target)
    {
        Type = TargetType.Editor;
        DefaultBuildSettings = BuildSettingsVersion.V7;
        IncludeOrderVersion = EngineIncludeOrderVersion.Unreal5_8;
        ExtraModuleNames.Add("EROAetheria");
    }
}
