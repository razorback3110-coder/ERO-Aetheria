using UnrealBuildTool;

public class EROAetheriaEditorTarget : TargetRules
{
    public EROAetheriaEditorTarget(TargetInfo Target) : base(Target)
    {
        Type = TargetType.Editor;
        DefaultBuildSettings = BuildSettingsVersion.V6;
        IncludeOrderVersion = EngineIncludeOrderVersion.Unreal5_8;
        BuildEnvironment = TargetBuildEnvironment.Unique;
        ExtraModuleNames.Add("EROAetheria");
    }
}
