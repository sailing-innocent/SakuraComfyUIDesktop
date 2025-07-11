using SB;
using SB.Core;
using Serilog;
using Serilog.Events;
using System.Diagnostics;

var parser = new CommandParser();
var mainCmd = new MainCommand();
// Configure the parser
parser.MainCmd(mainCmd, "SB", "Sakura Build System", "SB [options] <command> [command-options]");
return parser.ParseSync(args);

// Main command that holds global options
public class MainCommand
{
    [CmdSub(Name = "build", ShortName = 'b', Help = "Build the project")]
    public BuildCommand Build { get; set; } = new BuildCommand();

    [CmdSub(Name = "test", ShortName = 't', Help = "Run tests")]
    public TestCommand Test { get; set; } = new TestCommand();

    [CmdSub(Name = "clean", ShortName = 'c', Help = "Clean build cache and dependency databases")]
    public CleanCommand Clean { get; set; } = new CleanCommand();

    [CmdSub(Name = "vs", Help = "Generate Visual Studio solution")]
    public VSCommand VS { get; set; } = new VSCommand();
}

public abstract class CommandBase
{
    [CmdOption(Name = "verbose", ShortName = 'v', Help = "Enable verbose logging", IsRequired = false)]
    public bool Verbose { get; set; } = false;

    [CmdOption(Name = "mode", ShortName = 'm', Help = "Build mode (debug/release)", IsRequired = false)]
    public string Mode { get; set; } = "debug";

    [CmdOption(Name = "sha-depend", Help = "Use SHA instead of DateTime for dependency checking", IsRequired = false)]
    public bool UseShaDepend { get; set; } = false;

    [CmdOption(Name = "category", ShortName = 'c', Help = "Build tools", IsRequired = false)]
    public string Category { get; set; } = "modules";

    [CmdOption(Name = "toolchain", Help = "Toolchain to use", IsRequired = false)]
    public string ToolchainName { get; set; } = OperatingSystem.IsWindows() ? "clang-cl" : "clang";

    [CmdOption(Name = "proxy", Help = "Set HTTP proxy for downloads")]
    public string Proxy { get; set; } = "";

    [CmdExec]
    public void Exec()
    {
        Stopwatch timer = Stopwatch.StartNew();

        // Use global verbose setting if available
        LogEventLevel LogLevel = LogEventLevel.Information;
        if (Verbose)
        {
            LogLevel = LogEventLevel.Verbose;
        }
        Engine.InitializeLogger(LogLevel);
        // Engine.SetEngineDirectory(SourceLocation.Directory());
        Engine.SetEngineDirectory(Path.Combine(SourceLocation.Directory(), "engine/SakuraEngine/"));


        // set proxy
        if (!string.IsNullOrEmpty(Proxy))
        {
            Log.Information("Setting HTTP proxy to {Proxy}", Proxy);
            Download.HttpProxy = Proxy;
        }

        // use sha to check file dependency instead of using last write time 
        if (UseShaDepend)
            Depend.DefaultUseSHAInsteadOfDateTime = true;

        // Set compiler
        if (ToolchainName == "clang-cl")
            VisualStudio.UseClangCl = true;
        else if (ToolchainName == "msvc")
            VisualStudio.UseClangCl = false;
        else if (ToolchainName == "clang")
        {
            ; //XCode.
        }

        // Set configuration based on mode
        BuildSystem.GlobalConfiguration = Mode.ToLower();
        if (Mode.ToLower() != "debug" && Mode.ToLower() != "release")
        {
            Log.Warning($"Unknown build mode '{Mode}', defaulting to debug");
            BuildSystem.GlobalConfiguration = "debug";
        }
        Log.Information("Build start with configuration: {Configuration}", BuildSystem.GlobalConfiguration);

        // Set categories
        if (Category == "modules")
            Categories |= TargetCategory.Runtime | TargetCategory.DevTime;
        if (Category == "tools")
            Categories |= TargetCategory.Tool;
        Log.Information("Build start with categories: {Categories}", Categories);

        // Bootstrap engine
        _toolchain = Engine.Bootstrap(SourceLocation.Directory(), Categories);


        // run subcmd exec
        OnExecute();

        // stop and dump counters
        timer.Stop();
        Log.Information($"Total: {timer.ElapsedMilliseconds / 1000.0f}s");
        Log.Information($"Execution Total: {timer.ElapsedMilliseconds / 1000.0f}s");
        Log.Information($"Compile Commands Total: {CompileCommandsEmitter.Time / 1000.0f}s");
        Log.Information($"Compile Total: {CppCompileEmitter.Time / 1000.0f}s");
        Log.Information($"Link Total: {CppLinkEmitter.Time / 1000.0f}s");
        Log.CloseAndFlush();
    }

    public abstract void OnExecute();
    public IToolchain Toolchain => _toolchain!;
    private IToolchain? _toolchain;
    protected TargetCategory Categories = TargetCategory.Package;
}

// Build subcommand
public class BuildCommand : CommandBase
{
    [CmdOption(Name = "shader-only", Help = "Build shaders only", IsRequired = false)]
    public bool ShaderOnly { get; set; }

    [CmdOption(Name = "target", Help = "Build a single target", IsRequired = false)]
    public string? SingleTarget { get; set; }

    public override void OnExecute()
    {
        Engine.AddShaderTaskEmitters(Toolchain);
        if (!ShaderOnly)
        {
            Engine.AddEngineTaskEmitters(Toolchain);
            Engine.AddCompileCommandsEmitter(Toolchain);
        }

        Engine.RunBuild(SingleTarget);

        // Handle post-build tasks
        if (Categories.HasFlag(TargetCategory.Tool))
        {
            Directory.CreateDirectory(".sb/compile_commands/tools");
            CompileCommandsEmitter.WriteToFile(".sb/compile_commands/tools/compile_commands.json");

            string ToolsDirectory = Path.Combine(SourceLocation.Directory(), ".sb", "tools");
            BuildSystem.Artifacts.AsParallel().ForAll(artifact =>
            {
                if (artifact is LinkResult Program)
                {
                    if (!Program.IsRestored && Program.Target.IsCategory(TargetCategory.Tool))
                    {
                        // copy to /.sb/tools
                        if (File.Exists(Program.PDBFile))
                        {
                            Log.Verbose("Copying PDB file {PDBFile} to {ToolsDirectory}", Program.PDBFile, ToolsDirectory);
                            File.Copy(Program.PDBFile, Path.Combine(ToolsDirectory, Path.GetFileName(Program.PDBFile)), true);
                        }
                        if (File.Exists(Program.TargetFile))
                        {
                            Log.Verbose("Copying target file {TargetFile} to {ToolsDirectory}", Program.TargetFile, ToolsDirectory);
                            File.Copy(Program.TargetFile, Path.Combine(ToolsDirectory, Path.GetFileName(Program.TargetFile)), true);
                        }
                    }
                }
            });
        }
        else
        {
            Directory.CreateDirectory(".sb/compile_commands/modules");
            CompileCommandsEmitter.WriteToFile(".sb/compile_commands/modules/compile_commands.json");

            Directory.CreateDirectory(".sb/compile_commands/shaders");
            CppSLEmitter.WriteCompileCommandsToFile(".sb/compile_commands/shaders/compile_commands.json");
        }
    }
}

// Test subcommand
public class TestCommand : CommandBase
{
    public override void OnExecute()
    {
        // Add necessary emitters for test execution
        Engine.AddEngineTaskEmitters(Toolchain);
        Engine.RunBuild();

        // This assumes build has already been done
        var Programs = BuildSystem.Artifacts.Where(a => a is LinkResult)
            .Select(a => (LinkResult)a)
            .Where(p => p.Target.IsCategory(TargetCategory.Tests) && p.Target.GetTargetType() == TargetType.Executable)
            .ToList();

        if (!Programs.Any())
        {
            Log.Warning("No test programs found. Make sure to build tests first.");
            return;
        }

        Programs.AsParallel().ForAll(program =>
        {
            Stopwatch sw = Stopwatch.StartNew();
            Log.Information("Running test target {TargetName}", program.Target.Name);
            ProcessOptions Options = new ProcessOptions
            {
                WorkingDirectory = Path.GetDirectoryName(program.TargetFile)
            };
            var result = BuildSystem.RunProcess(program.TargetFile, "", out var output, out var error, Options);
            sw.Stop();
            float Seconds = sw.ElapsedMilliseconds / 1000.0f;
            if (result != 0)
            {
                Log.Error("Test target {TargetName} failed with error: {Error}", program.Target.Name, error);
            }
            else
            {
                Log.Information("Test target {TargetName} passed, cost {Seconds}s", program.Target.Name, Seconds);
            }
        });
    }
}

// Clean subcommand
public class CleanCommand : CommandBase
{
    [CmdOption(Name = "database", ShortName = 'd', Help = "Database to clean, 'all | packages' | 'targets' | 'shaders' | 'sdks'", IsRequired = false)]
    public string Database { get; set; } = "compile";

    public override void OnExecute()
    {
        Log.Information("Cleaning build cache dependency databases for {Database}...", Database);

        // Clean dependency databases using API
        try
        {
            bool all = (Database == "all");
            if (all || Database == "targets")
                BuildSystem.CppCompileDepends(false).ClearDatabase();
            if (all || Database == "packages")
                BuildSystem.CppCompileDepends(true).ClearDatabase();
            if (all || Database == "shaders")
                Engine.ShaderCompileDepend.ClearDatabase();
            if (all || Database == "sdks")
            {
                Engine.ConfigureAwareDepend.ClearDatabase();
                Engine.ConfigureNotAwareDepend.ClearDatabase();
            }
        }
        catch (Exception ex)
        {
            Log.Error("Failed to clear databases: {Error}", ex.Message);
        }
    }
}

// VS subcommand
public class VSCommand : CommandBase
{
    [CmdOption(Name = "solution-name", Help = "Name of the solution file (without extension)", IsRequired = false)]
    public string SolutionName { get; set; } = "SakuraComfyUIDesktop";

    [CmdOption(Name = "output", Help = "Output directory for solution files", IsRequired = false)]
    public string OutputDirectory { get; set; } = ".sb/VisualStudioSolution";

    public override void OnExecute()
    {
        Log.Information("Generating Visual Studio solution...");

        // Set output directory for VS emitter
        VSEmitter.RootDirectory = Directory.GetCurrentDirectory();
        VSEmitter.OutputDirectory = OutputDirectory;

        // Add VS emitter to generate project files
        BuildSystem.AddTaskEmitter("VSEmitter", new VSEmitter(Toolchain));

        // Run the build to process all targets
        Engine.RunBuild();

        // Generate solution file
        var solutionPath = Path.Combine(OutputDirectory, $"{SolutionName}.sln");
        VSEmitter.GenerateSolution(solutionPath, SolutionName);

        Log.Information("Visual Studio solution generated at: {Path}", Path.GetFullPath(solutionPath));
    }
}

public static partial class SakuraComfyUIDesktop
{
    public static string RootDirectory => SourceLocation.Directory();
}