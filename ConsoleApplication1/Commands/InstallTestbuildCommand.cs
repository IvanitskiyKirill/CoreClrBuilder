using System.IO;

namespace CoreClrBuilder.Commands {
    class InstallTestbuildCommand : BatchCommand {
        readonly EnvironmentSettings environmentSettings;
        readonly string pathToTestbuild;
        readonly string version;

        public InstallTestbuildCommand(EnvironmentSettings settings, string pathToTestbuild, string version) {
            this.version = version;
            this.pathToTestbuild = pathToTestbuild;
            environmentSettings = settings;
        }

        protected override void PrepareCommand() {
            Commands.Clear();

            foreach(var enumerateDirectory in Directory.EnumerateDirectories(pathToTestbuild)) {
                var pathToPackage = Path.Combine(enumerateDirectory, version);
                var packageName = string.Format("{0}.{1}.nupkg", new DirectoryInfo(enumerateDirectory).Name, version);
                var fullPath = Path.Combine(pathToPackage, packageName);

                Add(new InstallPackageCommand(environmentSettings, fullPath));
            }
        }
    }
}