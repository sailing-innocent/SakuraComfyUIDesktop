using SB;
using SB.Core;

[TargetScript]
public static class SCUIIpc
{
    static SCUIIpc()
    {
        Engine.Module("SCUIIpc", "SCUI_IPC")
            .Depend(Visibility.Public, "SkrCore")
            .Depend(Visibility.Private, "cpp-ipc")
            .IncludeDirs(Visibility.Public, "include")
            .AddCppFiles("src/**.cpp")
            .AddMetaHeaders("include/**.hpp");
    }
}