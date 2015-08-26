using System;
using System.IO;

namespace CoreClrBuilder
{
    class CommandBuilder
    {
        EnvironmentSettings settings;

        public Command GetProject(CoreClrProject project)
        {
            return new Command(settings.DXVCSGet, string.Format("vcsservice.devexpress.devx {0} {1}", project.VSSPath, project.LocalPath), "get from VCS", settings.WorkingDir);
        }
        public Command Restore(CoreClrProject project)
        {
            return new Command(settings.DNU, string.Format("restore {0}", project.LocalPath), "call dnu restore", settings.WorkingDir);
        }
        public Command Build(CoreClrProject project)
        {
            string buildParams = string.Format("pack {0} --configuration {1}", project.LocalPath, project.BuildConfiguration);
            if (!string.IsNullOrEmpty(project.BuildFramework))
                buildParams += string.Format(" --framework {0}", project.BuildFramework);
            return new Command(settings.DNU, buildParams, "build", settings.WorkingDir);
        }
        public Command InstallPackage(CoreClrProject project)
        {
            return new Command(settings.DNU, string.Format(@"packages add {0}\bin\{1}\{2} {3}\.dnx\packages", project.LocalPath, project.BuildConfiguration, project.NugetPackageName, settings.UserProfile), "install package", settings.WorkingDir);
        }
        public Command RunTests(CoreClrProject project)
        {
            return new Command(settings.DNX, string.Format(@"-p {0} --configuration {1} test -xml {2}", project.LocalPath, project.BuildConfiguration, project.TestResultFileName), "run tests", settings.WorkingDir);
        }
        public CommandBuilder(EnvironmentSettings settings)
        {
            this.settings = settings;
        }
        public Command GetFromVCS(string remotePath)
        {
            return GetFromVCS(remotePath, string.Empty);
        }
        public Command GetFromVCS(string remotePath, string localPath)
        {
            return GetFromVCS(remotePath, localPath, string.Empty);
        }
        public Command GetFromVCS(string remotePath, string localPath, string comment)
        {
            if (string.IsNullOrEmpty(remotePath))
                throw new ArgumentNullException("remote path cannot be null");
            return new Command("DXVCSGet.exe", string.Format("vcsservice.devexpress.devx {0} {1}", remotePath, localPath), comment, settings.WorkingDir);
        }
        internal Command GetProductConfig()
        {
            if (!File.Exists(settings.ProductConfig))
                return GetFromVCS(string.Format("$/CCNetConfig/LocalProjects/{0}/BuildPortable/Product.xml", settings.BranchVersionShort), string.Empty, "Get Product.xml");
            return Command.CreateEmptyCommand();
        }
        internal Command DownloadDNVM()
        {
            if (!File.Exists(settings.DNVM))
                return new Command("powershell.exe", "-NoProfile -ExecutionPolicy unrestricted -Command \" &{$Branch = 'dev'; iex((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.ps1'))}\"", "Download dnvm", settings.WorkingDir);
            return Command.CreateEmptyCommand();
        }
        internal Command InstallDNX(DNXSettings dnxsettings)
        {
            return new Command(settings.DNVM, dnxsettings.CreateArgsForDNX(), "Install dnx", settings.WorkingDir);
        }
        internal Command GetNugetConfig()
        {
            return GetFromVCS(string.Format("$/{0}/Win/NuGet.Config", settings.BranchVersion), @"Win\", "get nuget.config");
        }
    }
}
