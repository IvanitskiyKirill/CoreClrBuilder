using System;

namespace CoreClrBuilder.Commands
{
    class InstallPackageCommand : Command
    {
        EnvironmentSettings settings;
        CoreClrProject project;

        public InstallPackageCommand(EnvironmentSettings settings, CoreClrProject project)
        {
            this.settings = settings;
            this.project = project;
        }
        protected override void PrepareCommand()
        {
            string args = PlatformPathsCorrector.Inst.Correct(string.Format(@"packages add {0}\bin\{1}\{2} {3}\.dnx\packages", project.LocalPath, project.BuildConfiguration, project.NugetPackageName, settings.UserProfile), Platform.Windows);
            Init(settings.DNU, args, "install package", settings.WorkingDir);
        }
    }

}
