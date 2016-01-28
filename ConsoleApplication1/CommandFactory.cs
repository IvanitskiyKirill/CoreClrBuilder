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
                //if (EnvironmentSettings.Platform == Platform.Unix) {
                //    batchCommand.Add(new LinuxFreeMemoryStartCommand());
                //    batchCommand.Add(new LinuxFreeMemoryCommand());
                //}
            }
            return batchCommand;
        }
        public ICommand RunTests(string runtime)
        {
            BatchCommand batchCommand = new BatchCommand(true);
            batchCommand.Add(new ActionCommand("Test Clear", () =>
            {
                foreach (var project in productInfo.Projects)
                {
                    string xUnitResults = Path.Combine(envSettings.WorkingDir, project.TestResultFileName);

                    if (File.Exists(xUnitResults))
                        File.Delete(xUnitResults);
                }
            }));
            if (EnvironmentSettings.Platform == Platform.Unix)
                batchCommand.Add(new UnixGrantAccessCommand(envSettings.WorkingDir, envSettings.WorkingDir));
            foreach (var project in productInfo.Projects)
            {
                batchCommand.Add(new RunTestsCommand(envSettings, project, runtime));
                //if (EnvironmentSettings.Platform == Platform.Unix)
                //{
                //    batchCommand.Add(new LinuxFreeMemoryStartCommand());
                //    batchCommand.Add(new LinuxFreeMemoryCommand());
                //}
            }
            batchCommand.Add(new GetFromVCSCommand(
                envSettings,
                Path.Combine(envSettings.RemoteSettingsPath, "NUnitXml.xslt"),
                envSettings.WorkingDir,
                "get NUnitXml.xslt",
                envSettings.WorkingDir));
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
        public ICommand CollectArtifacts(EnvironmentSettings settings, string destFolder, string runtime, string buildFramework)
        {
            return new CollectArtifactsCommand(settings, productInfo, destFolder, runtime, buildFramework);
        }

        public ICommand CopyTestbuildFolder(string runtime, string framework) {
            var batchCommand = new BatchCommand();
            var sourcePath = TestbuildPathHelper.Combine(envSettings.BuildArtifactsFolder, runtime, framework);

            batchCommand.Add(new CreateDirectoryCommand(envSettings.LocalTestbuildFolder));
            batchCommand.Add(new CopyDirectoryCommand(sourcePath, envSettings.LocalTestbuildFolder, true));

            return batchCommand;
        }

        public ICommand UnixMountTestbuildDirectory(string runtime, string framework) {
            var batchCommand = new BatchCommand();
            var sourcePath = TestbuildPathHelper.Combine(envSettings.BuildArtifactsFolder, runtime, framework);

            batchCommand.Add(new CreateDirectoryCommand(envSettings.LocalTestbuildFolder));
            batchCommand.Add(new UnixGrantAccessCommand(envSettings.LocalTestbuildFolder, envSettings.WorkingDir));
            batchCommand.Add(new LinuxMountDirectory(sourcePath, envSettings.LocalTestbuildFolder, envSettings.WorkingDir));
            
            return batchCommand;
        }

        public ICommand InstallTestbuild() {
            var version = envSettings.BranchVersionShort + ".0";
            return new InstallTestbuildCommand(envSettings, envSettings.LocalTestbuildFolder, version);
        }
    }
}
