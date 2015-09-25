using System;
using System.IO;

namespace CoreClrBuilder.Commands
{
    class CollectArtifactsCommand : ICommand
    {
        ProductInfo info;
        string destFolder;
        public CollectArtifactsCommand(ProductInfo info, string destFolder, string buildFramework)
        {
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
                string localPackagePath = PlatformPathsCorrector.Inst.Correct(string.Format(@"{0}\bin\{1}\{2}", project.LocalPath, project.BuildConfiguration, project.NugetPackageName), Platform.Windows);
                if (File.Exists(localPackagePath))
                {
                    Console.WriteLine("Start copy package {0}", project.NugetPackageName);
                    File.Copy(localPackagePath, destFolder + "\\" + project.NugetPackageName);
                }
                else
                    Console.WriteLine("Package {0} doesn't exist", project.NugetPackageName);
            }
        }
    }

}
