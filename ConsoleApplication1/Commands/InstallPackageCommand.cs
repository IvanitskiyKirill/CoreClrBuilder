namespace CoreClrBuilder.Commands {
    class InstallPackageCommand : Command {
        readonly EnvironmentSettings settings;
        readonly string pathToPackage;

        string Args {
            get {
                return PlatformPathsCorrector.Inst.Correct(
                    string.Format(
                        "packages add {0} {1}",
                        pathToPackage,
                        InstallationPath),
                    Platform.Windows);
            }
        }

        string InstallationPath {
            get { return string.Format(@"{0}\.dnx\packages", settings.UserProfile); }
        }

        InstallPackageCommand(EnvironmentSettings settings) {
            this.settings = settings;
        }

        public InstallPackageCommand(EnvironmentSettings settings, CoreClrProject project)
            : this(settings) {
            this.settings = settings;
            pathToPackage = string.Format(
                @"{0}\bin\{1}\{2}",
                project.LocalPath,
                project.BuildConfiguration,
                project.NugetPackageName);
        }

        public InstallPackageCommand(EnvironmentSettings settings, string pathToPackage)
            : this(settings) {
            this.settings = settings;
            this.pathToPackage = pathToPackage;
        }

        protected override void PrepareCommand() {
            Init(settings.DNU, Args, "install package", settings.WorkingDir);
        }
    }

}
