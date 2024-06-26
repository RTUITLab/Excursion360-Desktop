using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Logger;

[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Publish);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter]
    string Runtime { get; } = "win-x64";

    [Solution] readonly Solution Solution;

    AbsolutePath OutputDirectory => RootDirectory / "output";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            OutputDirectory.CreateOrCleanDirectory();
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution)
                .SetRuntime(Runtime));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });

    Target Publish => _ => _
        .DependsOn(Compile, Clean)
        .Executes(() =>
        {
            var projectName = "Excursion360.Desktop";
            DotNetPublish(s => s
                .SetProject(Solution.GetProject(projectName))
                .SetConfiguration(Configuration)
                .SetOutput(OutputDirectory)
                .SetRuntime(Runtime)
                .EnableSelfContained()
                .EnablePublishSingleFile()
                .EnablePublishTrimmed()
                .SetProperty("DebugType", "None")
                .SetProperty("DebugSymbols", false)
                .SetProperty("PublishIISAssets", false)
                .EnableNoRestore());

            var executableFile = System.IO.Directory.GetFiles(OutputDirectory)
                .Select(System.IO.Path.GetFileName)
                .Where(f => f.StartsWith(projectName))
                .Single();
            RenameFile(OutputDirectory / executableFile,
                $"{OutputDirectory / projectName}.{Runtime}{executableFile.Substring(projectName.Length)}");

        });

}
