// Variables

var solution = "./src/OCore/OCore.sln";
var buildConfiguration = "Release";
var packageOutputDir = "./packages/";
var target = Argument("target", "Build");
var apiKey = EnvironmentVariable("OCORE_NUGET_API_KEY");

Task("Build")
    .Does(() => {
        var settings = new DotNetCoreBuildSettings
        {
            Configuration = buildConfiguration,
        };

        DotNetCoreBuild(solution, settings);
    });

Task("Test")
    .IsDependentOn("Build")
    .Does(() => {
        Information("Running tests");
    });

Task("Pack")
    .IsDependentOn("Test")
    .Does(() => {
        DeleteDirectory(packageOutputDir, new DeleteDirectorySettings {
            Recursive = true,
            Force = true,
        });
        DotNetCorePack(solution, new DotNetCorePackSettings 
        { 
            Configuration = buildConfiguration,
            OutputDirectory = packageOutputDir,        
        });   
    });

Task("Push")
    .IsDependentOn("Pack")
    .Does(() => {
        Information($"Pushing for API key: {apiKey}");
        NuGetPush(packageOutputDir + "*.nupkg", new NuGetPushSettings {
            ApiKey = apiKey,
            Source = "https://api.nuget.org/v3/index.json",
            SkipDuplicate = true
        });
    });


Task("Publish")
    .IsDependentOn("Test")
    .IsDependentOn("Pack")
    .IsDependentOn("Push");

RunTarget(target);