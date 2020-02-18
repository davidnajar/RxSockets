using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using Nuke.Common.Tools.NuGet;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using Nuke.Common.CI;
using Nuke.Common.CI.AppVeyor;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
[AppVeyor(AppVeyorImage.VisualStudio2019, AutoGenerate =true)]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Script);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Nuget API Key")]
    readonly string NugetApiKey;

    

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion] readonly GitVersion GitVersion;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestsDirectory => RootDirectory / "tests";
    AbsolutePath OutputDirectory => RootDirectory / "output";
    AbsolutePath Project => SourceDirectory / "RxSockets/RxSockets.csproj";

    Target Clean => _ => _
     
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(OutputDirectory);
        });

    Target Version => _ => _
        .DependsOn(Clean)
    .Executes(() =>
    {
        Logger.Info($"Next version is {GitVersion.AssemblySemVer}");
        Logger.Info($"Variale NugetApiKey is ${NugetApiKey}");

    });



    Target Restore => _ => _
    .DependsOn(Version)
        .Executes(() =>
        {
            DotNetRestore(_ => _
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(_ => _
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetVersion(GitVersion.SemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .EnableNoRestore());
        });

    Target Pack => _ => _
     .DependsOn(Compile)
     .Produces(OutputDirectory / "*.nupkg")
     .Executes(() =>
     {
         DotNetPack(_ => _
         .SetConfiguration(Configuration)
         .SetNoBuild(true)
         .SetVersion(GitVersion.SemVer)
         .SetOutputDirectory(OutputDirectory)
         .SetProject(Project));
     });
    Target Push => _ => _
 .DependsOn(Pack)
    .OnlyWhenDynamic(()=>!IsLocalBuild && GitVersion.BranchName=="master")
    .Requires(()=>NugetApiKey)
 .Executes(() =>
 {
     DotNetNuGetPush(_ => _
        .SetTargetPath(OutputDirectory)
        .SetApiKey(NugetApiKey)


     ) ;
 });
    Target Script  => _ => _
        .DependsOn(Push);




}
