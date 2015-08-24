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
        StreamReader errorReader;
        List<string> outputErrors = new List<string>();
        string workingDir;
        string fileName;
        string args;
        string comment;
        public Command(string fileName, string args, string comment, string workingDir)
        {
            this.fileName = fileName;
            this.args = args;
            this.comment = comment;
            this.workingDir = workingDir;
        }
        public void Execute()
        {
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
        public Command GetProject {
            get  {
                return new Command(settings.DXVCSGet, string.Format("vcsservice.devexpress.devx {0} {1}", Project.VSSPath, Project.LocalPath), "get from VCS", settings.WorkingDir);
            }
        }
        public Command Restore {
            get {
                return new Command(settings.DNU, string.Format("restore {0}", Project.LocalPath), "call dnu restore", settings.WorkingDir);
            }
        }
        public Command Build
        {
            get
            {
                string buildParams = string.Format("pack {0} --configuration {1}", Project.LocalPath, Project.BuildConfiguration);
                if (!string.IsNullOrEmpty(Project.BuildFramework))
                    buildParams += string.Format(" --framework {0}", Project.BuildFramework);
                return new Command(settings.DNU, buildParams, "build", settings.WorkingDir);
            }
        }
        public Command InstallPackage {
            get {
                return new Command(settings.DNU, string.Format(@"packages add {0}\bin\{1}\{2} {3}\.dnx\packages", Project.LocalPath, Project.BuildConfiguration, Project.NugetPackageName, settings.UserProfile), "install package", settings.WorkingDir);
            }
        }
        public Command RunTests {
            get {
                return new Command(settings.DNX, string.Format(@"{0} --configuration {1} test -xml {2}", Project.LocalPath, Project.BuildConfiguration, Project.TestResultFileName), "run tests", settings.WorkingDir);
            }
        }
        public CoreClrProject Project { get; set; }
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
