#load "buildary/src/process.csx"
#load "buildary/src/path.csx"
#load "buildary/src/runner.csx"
#load "buildary/src/gitversion.csx"
#load "buildary/src/log.csx"
#load "buildary/src/travis.csx"

public class Options : Runner.BuildOptions
{
    [PowerArgs.ArgShortcut("config"), PowerArgs.ArgDefaultValue("Release")]
    public string Configuration { get; set; }
}

var options = Runner.ParseOptions<Options>(Args);
var gitversion = GitVersion.Get("./");
var nugetSource = "https://api.nuget.org/v3/index.json";
var nugetApiKey = System.Environment.GetEnvironmentVariable("NUGET_API_KEY");

Log.Info($"Version: {gitversion.FullVersion}");

var commandBuildArgs = $"--configuration {options.Configuration} --version-suffix \"{gitversion.PreReleaseTag}\"";

var targets = new SimpleTargets.TargetDictionary();

targets.Add("clean", () =>
{
    Path.CleanDirectory(Path.Expand("./output"));
});

targets.Add("build", () =>
{
    Process.Run($"dotnet build MarkdownTranslator.sln {commandBuildArgs}");
});

targets.Add("test", () =>
{
    Process.Run($"dotnet test test/MarkdownTranslator.Tests/");
});

targets.Add("deploy", SimpleTargets.DependsOn("clean"), () =>
{
    // Deploy our nuget packages.
    Process.Run($"dotnet pack --output {Path.Expand("./output")} {commandBuildArgs}");
});

targets.Add("update-version", () =>
{
    
});

targets.Add("publish", () =>
{
    
});

targets.Add("ci", SimpleTargets.DependsOn("update-version", "test", "deploy", "publish"), () =>
{
    
});

targets.Add("default", SimpleTargets.DependsOn("build"));

Runner.Run(options, targets);