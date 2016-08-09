namespace CoreClrBuilder
{
    class CoreClrProject
    {
        public const string TEST_FILE_NAME = "TestResult.xml";
        public bool IsValid { get { return !string.IsNullOrEmpty(VSSPath) && !string.IsNullOrEmpty(LocalPath); } }
        public string VSSPath { get; private set; }
        public string LocalPath { get; private set; }
        public string NugetPackageName { get; private set; }
        public string BuildConfiguration { get; private set; }
        public string BuildFramework { get; private set; }
        public string ProjectName { get; private set; }
        public string TestResultFileName { get; private set; }
        public bool IsTestProject { get; private set; }
        
        public CoreClrProject(string vssPath, string localPath, string releaseVersion, string buildConfiguration, string framework, bool isTestProject)
        {
            VSSPath = vssPath;
            LocalPath = PlatformPathsCorrector.Inst.Correct(localPath, Platform.Windows);
            BuildConfiguration = buildConfiguration;
            BuildFramework = framework;
            IsTestProject = isTestProject;
            string[] paths = LocalPath.Trim(PlatformPathsCorrector.Inst.PlatformSeparator).Split(PlatformPathsCorrector.Inst.PlatformSeparator);
            if (paths.Length > 0)
            {
                string projectName = paths[paths.Length - 1];
                ProjectName = projectName;
                TestResultFileName = string.Format("nunit_results_{0}.xml", ProjectName);
                NugetPackageName = string.Format("{0}.{1}.nupkg", projectName, releaseVersion);
            }
        }
    }
}
