using CoreClrBuilder.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Xsl;

namespace CoreClrBuilder
{
    class CommandFactory
    {
        EnvironmentSettings envSettings;
        ProjectsInfo productInfo;
        public CommandFactory(EnvironmentSettings settings, ProjectsInfo productInfo)
        {
            this.envSettings = settings;
            this.productInfo = productInfo;
        }
        public ICommand InstallEnvironment(DNXSettings dnxsettings)
        {
            if (EnvironmentSettings.Platform == Platform.Windows)
                return new BatchCommand(
                    new DownloadDNVMCommand(envSettings),
                    new InstallDNXCommand(envSettings, dnxsettings),
                    new GetNugetConfigCommand(envSettings, dnxsettings));
            else
                return new BatchCommand(
                    new GetInstallDNXScriptComamnd(envSettings, dnxsettings),
                    new InstallDNXCommand(envSettings, dnxsettings),
                    new GetNugetConfigCommand(envSettings, dnxsettings));
        }
        public ICommand GetProjectsFromVCS()
        {
            BatchCommand batchCommand = new BatchCommand();
            foreach (var project in productInfo.Projects)
                batchCommand.Add(new GetFromVCSCommand(envSettings, project.VSSPath, project.LocalPath, string.Format("get {0} from VCS", project.ProjectName), envSettings.WorkingDir ));
            return batchCommand;
        }
        public ICommand BuildProjects()
        {
            BatchCommand batchCommand = new BatchCommand();
            foreach (var project in productInfo.Projects)
            {
                if (EnvironmentSettings.Platform == Platform.Unix)
                    batchCommand.Add(new UnixGrantAccessCommand(project.LocalPath, envSettings.WorkingDir));
                batchCommand.Add(new RestoreCommand(envSettings, project));
                batchCommand.Add(new BuildCommand(envSettings, project));
                batchCommand.Add(new InstallPackageCommand(envSettings, project));
            }
            return batchCommand;
        }
        public ICommand RunTests()
        {
            BatchCommand batchCommand = new BatchCommand(true);
            batchCommand.Add(new GetFromVCSCommand(
                envSettings, 
                Path.Combine(envSettings.RemoteSettingsPath, "NUnitXml.xslt"),
                envSettings.WorkingDir,
                "get NUnitXml.xslt",
                envSettings.WorkingDir));
            batchCommand.Add(new ActionCommand("Tests clear", () =>
            {
                foreach (var project in productInfo.Projects)
                {
                    string xUnitResults = Path.Combine(envSettings.WorkingDir, project.TestResultFileName);
                    string nUnitResults = Path.Combine(envSettings.WorkingDir, project.NunitTestResultFileName);

                    if (File.Exists(xUnitResults))
                        File.Delete(xUnitResults);
                }
            }));
            
            foreach (var project in productInfo.Projects)
                batchCommand.Add(new RunTestsCommand(envSettings, project));

            batchCommand.Add(new ActionCommand("Tests transform", () =>
            {
                XslCompiledTransform xslt = new XslCompiledTransform();
                xslt.Load("NUnitXml.xslt");
                List<string> nUnitTestFiles = new List<string>();
                foreach (var project in productInfo.Projects)
                {
                    string xUnitResults = Path.Combine(envSettings.WorkingDir, project.TestResultFileName);
                    string nUnitResults = Path.Combine(envSettings.WorkingDir, project.NunitTestResultFileName);

                    if (File.Exists(nUnitResults))
                        File.Delete(nUnitResults);
                    if (File.Exists(xUnitResults))
                    {
                        xslt.Transform(xUnitResults, nUnitResults);
                        nUnitTestFiles.Add(nUnitResults);
                    }
                }
                NUnitMerger.MergeFiles(nUnitTestFiles, "nunit-result.xml");
            }));
            return batchCommand;
        }
        public ICommand CopyProjects(string copyPath, bool copySubDirs) {
            return new CopyProjectsCommand(productInfo, copyPath, copySubDirs);
        }
        public ICommand RemoveProjects()
        {
            return new RemoveProjectsCommand(productInfo);
        }
        public ICommand CollectArtifacts(string destFolder, string buildFramework)
        {
            return new CollectArtifactsCommand(productInfo, destFolder, buildFramework);
        }
    }
}
