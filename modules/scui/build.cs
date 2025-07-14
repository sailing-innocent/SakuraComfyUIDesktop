using SB;
using SB.Core;
using Serilog;

[TargetScript]
public static class SCUI 
{
    static SCUI()
    {
        Engine.Module("SCUIRuntime", "SCUI_RUNTIME")
            .Depend(Visibility.Public, "SkrImGui")
            .IncludeDirs(Visibility.Public, "include")
            .AddCppFiles("src/*.cpp")
            .AddMetaHeaders(
                "include/SCUI/element.h"
            );
    }
}