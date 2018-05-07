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
    if(Path.FileExists("./build/version.props")) Path.DeleteFile("./build/version.props");
    Path.WriteFile("./build/version.props",
$@"<Project>
    <PropertyGroup>
        <VersionPrefix>{gitversion.Version}</VersionPrefix>
    </PropertyGroup>
</Project>");
});

targets.Add("publish", () =>
{
    if(Travis.IsTravis)
    {
        // If we are on travis, we only want to deploy if this is a release tag.
        if(Travis.EventType != Travis.EventTypeEnum.Push)
        {
            // Only pushes (no cron jobs/api/pull rqeuests) can deploy.
            Log.Warning("Not a push build, skipping publish...");
            return;
        }

        if(Travis.Branch != "master")
        {
            // We aren't on master.
            // The only time we also auto-publish
            // is if this is a beta build.
            if(gitversion.PreReleaseLabel != "beta")
            {
                Log.Warning("Not on master or beta build, skipping publish...");
                return;
            }
        }
    }

    if(string.IsNullOrEmpty(nugetApiKey))
    {
        throw new Exception("No NUGET_API_KEY provided.");
    }

    foreach(var file in Path.GetFiles("./output", "*.nupkg"))
    {
        Log.Info($"Deploying {file}");
        Process.Run($"dotnet nuget push {file} --source \"{nugetSource}\" --api-key \"{nugetApiKey}\"");
    }
});

targets.Add("ci", SimpleTargets.DependsOn("update-version", "test", "deploy", "publish"), () =>
{
    
});

targets.Add("default", SimpleTargets.DependsOn("build"));

Runner.Run(options, targets);