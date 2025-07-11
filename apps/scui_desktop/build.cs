using SB;
using SB.Core;
using Serilog;

[TargetScript]
public static class SCUIDesktop {
    static SCUIDesktop()
    {
        Engine.Program("scui_desktop", "SCUI_DESKTOP")
            .Depend(Visibility.Private, "SCUIRuntime")
            .AddCppFiles("src/*.cpp");
    }
}
