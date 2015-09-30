using System.IO;

namespace CoreClrBuilder.Commands
{
    class GetNugetConfigCommand : GetFromVCSCommand
    {
        string workingDir;
        public GetNugetConfigCommand(EnvironmentSettings settings, DNXSettings dnxsettings) :
            base(
                settings,
                string.Format("$/{0}/Win/NuGet.Config", settings.BranchVersion),
                PlatformPathsCorrector.Inst.Correct(@"Win\", Platform.Windows),
                "get nuget.config",
                settings.WorkingDir)
        {
            workingDir = settings.WorkingDir;
        }
        public override void Execute()
        {
            if (!File.Exists(Path.Combine(workingDir, PlatformPathsCorrector.Inst.Correct(@"Win\NuGet.Config", Platform.Windows))))
                base.Execute();
        }
    }

}
