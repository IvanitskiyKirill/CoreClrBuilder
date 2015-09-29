using System;
using System.IO;

namespace CoreClrBuilder.Commands
{
    class CollectArtifactsCommand : ICommand
    {
        ProjectsInfo info;
        string destFolder;
        EnvironmentSettings settings;

        public CollectArtifactsCommand(EnvironmentSettings settings, ProjectsInfo info, string destFolder, string buildFramework)
        {
            this.settings = settings;
            this.info = info;
            if (!string.IsNullOrEmpty(buildFramework))
                this.destFolder = string.Format(@"{0}\{1}", destFolder, buildFramework);
            else
                this.destFolder = destFolder;
        }
        public void Execute()
        {
            if (Directory.Exists(destFolder))
                Directory.Delete(destFolder, true);
            Directory.CreateDirectory(destFolder);

            foreach (var project in info.Projects)
            {
                //string localPackagePath = PlatformPathsCorrector.Inst.Correct(string.Format(@"{0}\bin\{1}\{2}", project.LocalPath, project.BuildConfiguration, project.NugetPackageName), Platform.Windows);
                string localPackagePath = Path.Combine(settings.PackagesPath, project.ProjectName);
                if (Directory.Exists(localPackagePath))
                {
                    Console.WriteLine("Start copy package {0}", project.NugetPackageName);
                    CopyProjectsCommand.DirectoryCopy(localPackagePath, Path.Combine(destFolder, project.ProjectName), true);
                }
                else
                    Console.WriteLine("Package {0} doesn't exist", project.NugetPackageName);
            }
        }
    }

}
