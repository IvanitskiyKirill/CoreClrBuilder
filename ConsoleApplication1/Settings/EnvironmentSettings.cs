using System;
using System.IO;

namespace CoreClrBuilder
{
    class EnvironmentSettings
    {
        public string DNX { get; private set; }
        public string DNU { get; private set; }
        public string DNVM { get; private set; }
        public string DXVCSGet { get; private set; }
        public string UserProfile { get; private set; }
        public string WorkingDir { get; private set; }
        public string ProductConfig { get; private set; }
        public string RemoteSettingsPath { get { return string.Format(@"$/CCNetConfig/LocalProjects/{0}/BuildPortable/", BranchVersionShort); } }
        public string BranchVersion { get; private set; }
        public string BranchVersionShort { get; private set; }
        public EnvironmentSettings(string[] args)
        {
            DXVCSGet = "DXVCSGet.exe";
            WorkingDir = Environment.CurrentDirectory;
            UserProfile = Environment.GetEnvironmentVariable("USERPROFILE");
            DNVM = string.Format(@"{0}\.dnx\bin\dnvm.cmd", UserProfile);
            ProductConfig = Path.Combine(WorkingDir, "Product.xml");
        }

        public void FindPathToDNX()
        {
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
        public void SetBranchVersion(string releaseVersion) {
            string[] parts = releaseVersion.Split('.');
            if (parts.Length < 2)
                return;
            BranchVersionShort = string.Format("{0}.{1}", parts[0], parts[1]);
            BranchVersion = "20" + BranchVersionShort;
        }
    }
}
