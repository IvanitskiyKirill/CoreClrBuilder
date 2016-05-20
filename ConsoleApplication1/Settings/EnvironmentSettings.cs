using System;
using System.IO;

namespace CoreClrBuilder
{
    enum Platform {
        Windows,
        Unix,
        Unknown
    }
    class EnvironmentSettings
    {
        public static Platform Platform;

        static EnvironmentSettings() {
            Platform = DetectPlatform();
        }

        public string DotNet { get; private set; }
        public string DotNetInstaller { get; private set; }
        public string DXVCSGet { get; private set; }
        public string UserProfile { get; private set; }
        public string WorkingDir { get; private set; }
        public string ProductConfig { get; private set; }
        public string RemoteSettingsPath { get { return string.Format(@"$/CCNetConfig/LocalProjects/{0}/BuildPortable/", BranchVersionShort); } }
        public string BranchVersion { get; private set; }
        public string BranchVersionShort { get; private set; }
        public string BuildArtifactsFolder {
            get {
                return Platform == Platform.Windows
                    ? string.Format(@"\\corp\builds\testbuilds\testbuild.v{0}.Portable", BranchVersionShort)
                    : PlatformPathsCorrector.Inst.Correct(Path.Combine(@"\\hyper16\testbuilds", string.Format(@"testbuild.v{0}.Portable", BranchVersionShort)), Platform.Windows);
            }
        }
        public string PackagesPath { get; private set; }
        public string LocalTestbuildFolder { get; private set; }

        public EnvironmentSettings()
        {
            Platform = DetectPlatform();
            PlatformPathsCorrector.Inst.Platform = Platform;

            Console.WriteLine("WORKING DIR: {0}", Environment.CurrentDirectory);

            WorkingDir = Environment.CurrentDirectory;
            ProductConfig = Path.Combine(WorkingDir, "Product.xml");
            DXVCSGet = "DXVCSGet.exe";
            LocalTestbuildFolder = PlatformPathsCorrector.Inst.Correct(Path.Combine(WorkingDir, "testbuild"), Platform.Windows);

            if (Platform == Platform.Windows)
                WindowsInit();
            else
                UnixInit();
        }

        private void UnixInit()
        {
            UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            //DotNetInstaller = "DotNetCoreInst.exe";
            PackagesPath = Path.Combine(UserProfile, @".dnx/packages");
        }
        private void WindowsInit()
        {
            UserProfile = Environment.GetEnvironmentVariable("USERPROFILE");
            DotNetInstaller = "DotNetCoreInst.exe";
            PackagesPath = Path.Combine(UserProfile, @".dnx\packages");
            if (WorkingDir[WorkingDir.Length - 1] != '\\')
                WorkingDir += "\\";
        }
        private static Platform DetectPlatform()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    return Platform.Windows;
                case PlatformID.Unix:
                    return Platform.Unix;
                default:
                    return Platform.Unknown;
            }
        }
        public void FindPathToDNX() {
            if (Platform == Platform.Windows)
                DotNet = @"C:\Program Files\dotnet\dotnet.exe";
            else
                DotNet = @"/usr/bin/dotnet";
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
