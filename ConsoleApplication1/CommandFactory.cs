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
                    //new DownloadDNVMCommand(envSettings),
                    //new InstallDNXCommand(envSettings, dnxsettings),
                    new InstallNetCoreCommand(envSettings, dnxsettings),
                    new GetNugetConfigCommand(envSettings, dnxsettings));
            else
                return new BatchCommand(
                    new GetInstallDNXScriptComamnd(envSettings, dnxsettings),
                    new InstallNetCoreCommand(envSettings, dnxsettings),
                    new GetNugetConfigCommand(envSettings, dnxsettings));
        }
        public ICommand GetProjectsFromVCS()
        {
            BatchCommand batchCommand = new BatchCommand();
            foreach (var project in productInfo.Projects) {
                batchCommand.Add(new GetFromVCSCommand(envSettings, project.VSSPath, project.LocalPath, string.Format("get {0} from VCS", project.ProjectName), envSettings.WorkingDir));
            }
            return batchCommand;
        }
        public ICommand BuildProjects()
        {
            BatchCommand batchCommand = new BatchCommand();
            foreach (var project in productInfo.Projects) {
                
                if (EnvironmentSettings.Platform == Platform.Unix)
                    batchCommand.Add(new UnixGrantAccessCommand(project.LocalPath, envSettings.WorkingDir));

                batchCommand.Add(new RestoreCommand(envSettings, project.LocalPath));
                if (!project.IsTestProject)
                    batchCommand.Add(new PackCommand(envSettings, project.LocalPath, project.BuildConfiguration));
                else
                    batchCommand.Add(new BuildCommand(envSettings, project.LocalPath, project.BuildConfiguration));
                //batchCommand.Add(new InstallPackageCommand(envSettings, project));
                //if (EnvironmentSettings.Platform == Platform.Unix) {
                //    batchCommand.Add(new LinuxFreeMemoryStartCommand());
                //    batchCommand.Add(new LinuxFreeMemoryCommand());
                //}
            }
            return batchCommand;
        }
        public ICommand RunTests(string runtime) {
            BatchCommand batchCommand = new BatchCommand(true);
            if (EnvironmentSettings.Platform == Platform.Unix)
                batchCommand.Add(new UnixGrantAccessCommand(envSettings.WorkingDir, envSettings.WorkingDir));
            List<string> nUnitTestFiles = new List<string>();
            foreach (var project in productInfo.Projects) {
                if (!project.IsTestProject)
                    continue;
                //string nUnitResults = Path.Combine(envSettings.WorkingDir, project.NUnit3FileName);
                nUnitTestFiles.Add(project.NUnit2FileName);

                batchCommand.Add(new RunTestsCommand(envSettings, project, runtime));
                batchCommand.Add(new Nunit3To2Coverter(project.NUnit3FileName, project.NUnit2FileName));
                //batchCommand.Add(new ActionCommand("remove nunit results", () => {
                //    if (File.Exists(CoreClrProject.TEST_FILE_NAME)) {
                //        File.Delete(CoreClrProject.TEST_FILE_NAME);
                //    }
                //}));

                //if (EnvironmentSettings.Platform == Platform.Unix)
                //{
                //    batchCommand.Add(new LinuxFreeMemoryStartCommand());
                //    batchCommand.Add(new LinuxFreeMemoryCommand());
                //}
            }
            if (EnvironmentSettings.Platform == Platform.Unix) {
                batchCommand.Add(new ActionCommand("Tests merge", () => {
                    NUnitMerger.MergeFiles(nUnitTestFiles, "nunit-result.xml");
                }));
            }
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
            if(EnvironmentSettings.Platform == Platform.Windows)
                return new CollectArtifactsCommand(settings, productInfo, destFolder, runtime, buildFramework);
            else
                return new CollectArtifactsCommand(settings, productInfo, destFolder, null, null);
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
    }
}
