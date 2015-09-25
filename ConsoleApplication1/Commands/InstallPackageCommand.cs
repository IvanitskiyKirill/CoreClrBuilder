namespace CoreClrBuilder.Commands
{
    class InstallPackageCommand : Command
    {
        public InstallPackageCommand(EnvironmentSettings settings, CoreClrProject project)
        {
            string args = PlatformPathsCorrector.Inst.Correct(string.Format(@"packages add {0}\bin\{1}\{2} {3}\.dnx\packages", project.LocalPath, project.BuildConfiguration, project.NugetPackageName, settings.UserProfile), Platform.Windows);
            Init(settings.DNU, args, "install package", settings.WorkingDir);
        }
    }

}
