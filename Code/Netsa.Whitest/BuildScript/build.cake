#tool "nuget:?package=xunit.runner.console"
#addin "NuGet.Core"

var target = Argument("target", "Default");
var configuration = Argument("Configuration", "Release");
var solutionPath = Argument("SolutionPath", "../ISC.Whitest.sln");
var buildVersion = Argument("BuildVersion", "0");

if (String.IsNullOrEmpty(solutionPath)) throw new Exception("argument 'SolutionPath' is not provided");

if (TFBuild.IsRunningOnVSTS || TFBuild.IsRunningOnTFS){
    buildVersion = TFBuild.Environment.Build.Number;
}

var tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(),"ISC_" + Guid.NewGuid().ToString().Replace("-","") + @"\");
System.IO.Directory.CreateDirectory(tempPath);
Console.WriteLine("Temp path is " + tempPath);

// Task("Clean")
//     .Does(() =>
// {
//     CleanDirectories(string.Format("../**/obj/{0}",configuration));
//     CleanDirectories(string.Format("../**/bin/{0}",configuration));
// });

// Task("Restore-NuGet-Packages")
//     .IsDependentOn("Clean")
//     .Does(() =>
// {
//     NuGetRestore(solutionPath);
// });

// Task("Build")
//     .IsDependentOn("Restore-NuGet-Packages")
//     .Does(()=>
// {

//     MSBuild(solutionPath, configurator =>
//         configurator.SetConfiguration(configuration)
//             .SetVerbosity(Verbosity.Minimal)
//             .UseToolVersion(MSBuildToolVersion.VS2015)
//             .SetMSBuildPlatform(MSBuildPlatform.x86)
//             .SetPlatformTarget(PlatformTarget.MSIL));
// });

// Task("Run-Tests")
//      .IsDependentOn("Build")
//      .Does(()=>
//  {
//      var pathForTests = "../**/bin/" + configuration + "/*.Tests*.dll";
//      XUnit2(pathForTests);
//  });

Task("Create-Nuget-Packages")
    .Does(() =>
{
    var parsedSolution = ParseSolution(solutionPath);
    var candidateProjects = parsedSolution.Projects
                    .Where(a=>a.Name.StartsWith("ISC.Whitest") && !a.Name.EndsWith("Tests"))
                    .ToList();

    var allDependencies = new List<NuGet.PackageReference>();
    foreach(var project in candidateProjects){
        var directory = System.IO.Path.GetDirectoryName(project.Path.ToString());
        var directoryPath = new DirectoryPath(directory);
        var filePath = directoryPath.CombineWithFilePath(new FilePath("packages.config"));
        var dependencies = new NuGet.PackageReferenceFile(filePath.FullPath).GetPackageReferences();
        allDependencies.AddRange(dependencies);
    }

    var consolidateDependencies = allDependencies
                        .GroupBy(a=> new {a.Id, a.Version}, (key,value)=> new {key,value})
                        .GroupBy(a=>a.key.Id, (key, value)=> new {id= key, count = value.Count()})
                        .Where(a=>a.count > 1)
                        .ToList();
    if (consolidateDependencies.Count() > 0)
    {
        foreach(var package in consolidateDependencies){
            Error("Consolidate dependencies found : " + package.id.ToString());
        }
        throw new Exception("Consolidate dependencies found");
    }
    
    var nuGetPackSettings = new NuGetPackSettings
	{
        Authors = new List<string>(){ "H.Ahmadi"},
		OutputDirectory = tempPath,
		Properties = new Dictionary<string, string>
		{
			{ "Configuration", "Release" }
		},
        Version= string.Format("1.0.0.{0}-alpha", buildVersion),
	};
    
    var paths = candidateProjects.Select(a=> a.Path).ToList();
    NuGetPack(paths, nuGetPackSettings);
});

//  Task("Push-Nuget-Packages")
// .IsDependentOn("Create-Nuget-Packages")
// .Does(() =>
// {
    //  var files = System.IO.Directory.GetFiles(tempPath, "*.nupkg")
    //                                     .Select(z => new FilePath(z)).ToList();

    // foreach(var f in files){
    //     Console.WriteLine(f);
    // }
    //                 var settings = new NuGetPushSettings()
    //                 {
    //                     Source = nugetServerUrl,
    //                     ApiKey = nugetApiKey,
    //                 };
    // NuGetPush(files, settings);
// });

Task("Default")
    .IsDependentOn("Create-Nuget-Packages");

RunTarget(target);