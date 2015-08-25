using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreClrBuilder
{
    class Command
    {
        public static Command CreateEmptyCommand() { return new Command(); }
        StreamReader errorReader;
        List<string> outputErrors = new List<string>();
        string workingDir;
        string fileName;
        string args;
        string comment;
        bool empty;
        private Command()
        {
            empty = true;
        }
        public Command(string fileName, string args, string comment, string workingDir)
        {
            this.fileName = fileName;
            this.args = args;
            this.comment = comment;
            this.workingDir = workingDir;
        }
        public void Execute()
        {
            if (empty)
                return;
            if (!string.IsNullOrEmpty(comment))
                OutputLog.LogText(comment);

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo = new ProcessStartInfo(fileName, args);
            startInfo.WorkingDirectory = workingDir;
            startInfo.UseShellExecute = false;
            //startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.CreateNoWindow = false;

            process.StartInfo = startInfo;
            process.Start();

            //outputReader = process.StandardOutput;
            //Thread outputThread = new Thread(new ThreadStart(StreamReaderThread_Output));
            //outputThread.Start();

            errorReader = process.StandardError;

            for (;;)
            {
                string strLogContents = errorReader.ReadLine();
                if (strLogContents == null)
                    break;
                else
                    outputErrors.Add(strLogContents);
            }
            process.WaitForExit();
            if (process.ExitCode != 0)
                throw new WrongExitCodeException(process.StartInfo.FileName, process.StartInfo.Arguments, process.ExitCode, outputErrors);
            outputErrors.Clear();
            errorReader = null;
            process = null;
        }
    }

    class CommandBuilder {
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
        EnvironmentSettings settings;
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
                return GetFromVCS("$/CCNetConfig/LocalProjects/15.2/BuildPortable/Product.xml");
            return Command.CreateEmptyCommand();
        }

        internal Command DownloadDNVM()
        {
            if(!File.Exists(settings.DNVM))
                return new Command("powershell.exe", "-NoProfile -ExecutionPolicy unrestricted -Command \" &{$Branch = 'dev'; iex((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.ps1'))}\"", "Download dnvm", settings.WorkingDir);
            return Command.CreateEmptyCommand();
        }

        internal Command InstallDNX(string framework, bool isUnstable)
        {
            string runtime = string.IsNullOrEmpty(framework) || string.Compare(framework, "dnx451", true) == 0 ? string.Empty : "-r coreclr";
            string unstable = isUnstable ? "-u" : string.Empty;
            return new Command(settings.DNVM, string.Format("upgrade -arch x64 {0} {1}", runtime, unstable), "Download dnx", settings.WorkingDir);
        }

        internal Command GetNugetConfig()
        {
            return GetFromVCS("$/2015.2/Win/NuGet.Config", @"Win\", "get nuget.config");
        }
    }

    class EnvironmentSettings {
        public string DNX { get; private set; }
        public string DNU { get; private set; }
        public string DNVM { get; private set; }
        public string DXVCSGet { get; private set; } 
        public string UserProfile { get; private set; }
        public string WorkingDir { get; private set; }
        public string ProductConfig { get ; private set; }
        public string RemoteSettingsPath { get { return @"$/CCNetConfig/LocalProjects/15.2/BuildPortable/"; } }
        public EnvironmentSettings()
        {
            DXVCSGet = "DXVCSGet.exe";
            WorkingDir = Environment.CurrentDirectory;
            UserProfile = Environment.GetEnvironmentVariable("USERPROFILE");
            DNVM = string.Format(@"{0}\.dnx\bin\dnvm.cmd", UserProfile);
            ProductConfig = Path.Combine(WorkingDir, "Product.xml");
        }
        public void InitializeDNX() {
            string[] paths = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User).Split(';');
            foreach (var path in paths)
            {
                if (File.Exists(Path.Combine(path, "dnx.exe")) && File.Exists(Path.Combine(path, "dnu.cmd")))
                {
                    DNX = Path.Combine(path, "dnx.exe");
                    DNU = Path.Combine(path, "dnu.cmd");
                }
            }
        }
    }
}
