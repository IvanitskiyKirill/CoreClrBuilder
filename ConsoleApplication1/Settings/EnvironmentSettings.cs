﻿using System;
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
        public Platform Platform { get; private set; }
        public string BuildArtifactsFolder { get; private set; }

        public EnvironmentSettings()
        {
            Platform = DetectPlatform();
            PlatformPathsCorrector.Inst.Platform = Platform;

            WorkingDir = Environment.CurrentDirectory;
            ProductConfig = Path.Combine(WorkingDir, "Product.xml");
            BuildArtifactsFolder = @"\\corp\builds\testbuilds\testbuild.v15.2 Portable";
            DXVCSGet = "DXVCSGet.exe";

            if (Platform == Platform.Windows)
                WindowsInit();
            else
                UnixInit();
        }

        private void UnixInit()
        {
            //TODO KI: after implement service add script calling
            UserProfile = "/home/user";
            DNVM = string.Format(@"{0}/.dnx/dnvm/dnvm.sh", UserProfile);
            //DXVCSGet = "python3";
            
        }
        private void WindowsInit()
        {
            UserProfile = Environment.GetEnvironmentVariable("USERPROFILE");
            DNVM = string.Format(@"{0}\.dnx\bin\dnvm.cmd", UserProfile);
        }
        private Platform DetectPlatform()
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
        public void FindPathToDNX()
        {
            if (Platform == Platform.Windows)
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
            else {
                DNU = "dnu";
                DNX = "dnx";
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
