using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreClrBuilder
{
    class CoreClrProject
    {
        public bool IsValid { get { return !string.IsNullOrEmpty(VSSPath) && !string.IsNullOrEmpty(LocalPath); } }
        public string VSSPath { get; private set; }
        public string LocalPath { get; private set; }
        public string NugetPackageName { get; private set; }
        public string TestResultFileName { get; private set; }
        public string NunitTestResultFileName { get; private set; }
        public string BuildConfiguration { get; private set; }
        public CoreClrProject(string vssPath, string localPath, string releaseVersion, string buildConfiguration)
        {
            this.VSSPath = vssPath;
            this.LocalPath = localPath;
            this.BuildConfiguration = buildConfiguration;
            string[] paths = localPath.Split('\\');
            if (paths.Length > 0)
            {
                string projectName = paths[paths.Length - 1];
                this.NugetPackageName = string.Format("{0}_.{1}.nupkg", projectName, releaseVersion);
                this.TestResultFileName = string.Format("{0}-TestResult.xml", projectName);
                this.NunitTestResultFileName = string.Format("{0}-NunitTestResult.xml", projectName);
            }
        }
    }
}
