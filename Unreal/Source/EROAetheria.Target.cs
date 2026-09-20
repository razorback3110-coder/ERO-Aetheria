using UnrealBuildTool;
using System.Collections.Generic;

public class EROAetheriaTarget : TargetRules
{
    public EROAetheriaTarget(TargetInfo Target) : base(Target)
    {
        Type = TargetType.Game;
        DefaultBuildSettings = BuildSettingsVersion.V6;
        IncludeOrderVersion = EngineIncludeOrderVersion.Unreal5_8;
        BuildEnvironment = TargetBuildEnvironment.Unique;
        ExtraModuleNames.Add("EROAetheria");
    }
}
