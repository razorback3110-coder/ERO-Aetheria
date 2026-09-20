using UnrealBuildTool;
using System.Collections.Generic;

public class EROAetheriaTarget : TargetRules
{
    public EROAetheriaTarget(TargetInfo Target) : base(Target)
    {
        Type = TargetType.Game;
        DefaultBuildSettings = BuildSettingsVersion.V6;
        IncludeOrderVersion = EngineIncludeOrderVersion.Unreal5_8;
        UnreachableWarningLevel = WarningLevel.Error;
        ReturnTypeWarningLevel = WarningLevel.Error;
        DanglingElseWarningLevel = WarningLevel.Error;
        ExtraModuleNames.Add("EROAetheria");
    }
}
