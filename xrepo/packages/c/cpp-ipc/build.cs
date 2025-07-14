using SB;
using SB.Core;

[TargetScript]
public static class CppIpc
{
    static CppIpc()
    {
        BuildSystem.Target("cpp-ipc")
            .TargetType(TargetType.Static)
            .IncludeDirs(Visibility.Public, "include")
            .IncludeDirs(Visibility.Private, "src")
            .AddCppFiles("src/libipc/*.cpp")
            .AddCppFiles("src/libipc/sync/*.cpp")
            .AddCppFiles("src/libipc/platform/*.cpp");
    }
}