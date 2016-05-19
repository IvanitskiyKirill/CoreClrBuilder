using System.IO;

namespace CoreClrBuilder.Commands
{
    class GetNugetConfigCommand : GetFromVCSCommand
    {
        string workingDir;
        public GetNugetConfigCommand(EnvironmentSettings settings, DNXSettings dnxsettings) :
            base(
                settings,
                string.Format("$/{0}/NetCore/NuGet.Config", settings.BranchVersion),
                PlatformPathsCorrector.Inst.Correct(@"NetCore\", Platform.Windows),
                "get nuget.config",
                settings.WorkingDir)
        {
            workingDir = settings.WorkingDir;
        }
        public override void Execute()
        {
            if (!File.Exists(Path.Combine(workingDir, PlatformPathsCorrector.Inst.Correct(@"NetCore\NuGet.Config", Platform.Windows))))
                base.Execute();
        }
    }

}
