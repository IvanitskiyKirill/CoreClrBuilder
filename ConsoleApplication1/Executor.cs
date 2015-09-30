using CoreClrBuilder.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace CoreClrBuilder
{
    class Executor
    {
        XmlTextWriter tmpXml;
        StringBuilder taskBreakingLog = new StringBuilder();
        ProjectsInfo productInfo;
        CommandFactory factory;
        StepSettings stepSettings;
        public int ExecuteTasks(DNXSettings dnxSettings, StepSettings stepSettings, EnvironmentSettings envSettings)
        {
            tmpXml = new XmlTextWriter(new StringWriter(taskBreakingLog));
            tmpXml.Formatting = Formatting.Indented;
            int result = 0;
            try
            {
                IEnumerable<ICommand> commands = PrepareCommands(dnxSettings, stepSettings, envSettings, result);
                foreach (var command in commands)
                {
                    BatchCommand batchCommand = command as BatchCommand;
                    if (batchCommand != null && batchCommand.IsBatchOfIndependedCommands)
                    {
                        foreach (var innerCommands in batchCommand.Commands)
                        {
                            result += DoWork(innerCommands);
                        }
                    }
                    else
                    {
                        result += DoWork(command);
                        if (result > 0)
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                result = 1;
            }
            tmpXml.Close();
            if (taskBreakingLog.Length > 0)
            {
                taskBreakingLog.Insert(0, "<vssPathsByTasks>\r\n");
                taskBreakingLog.Append("\r\n</vssPathsByTasks>\r\n");
                string currLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                File.WriteAllText(Path.Combine(currLocation, "vssPathsByTasks.xml"), taskBreakingLog.ToString());
            }
            return result > 0 ? 1 : 0;
        }

        internal IEnumerable<ICommand> PrepareCommands(DNXSettings dnxSettings, StepSettings stepSettings, EnvironmentSettings envSettings, int result)
        {
            this.stepSettings = stepSettings;
            productInfo = new ProjectsInfo(envSettings.ProductConfig, dnxSettings.Framework);
            envSettings.SetBranchVersion(productInfo.ReleaseVersion);

            factory = new CommandFactory(envSettings, productInfo);
            List<ICommand> commands = new List<ICommand>();

            if (stepSettings.EnvironmentInitialization)
                commands.Add(factory.InstallEnvironment(dnxSettings));

            if (stepSettings.Build || stepSettings.RunTests)
                commands.Add(new ActionCommand("init dnx and dnu paths", new Action(envSettings.FindPathToDNX)));

            if (stepSettings.CopyDirs)
                commands.Add(factory.CopyProjects(stepSettings.CopyPath, true));

            if (stepSettings.RemoveProjectsDirectories)
                commands.Add(factory.RemoveProjects());

            if (stepSettings.GetProjectsFromDXVCS)
                commands.Add(factory.GetProjectsFromVCS());

            if (stepSettings.Build) 
                commands.Add(factory.BuildProjects());

            if (stepSettings.RunTests)
                commands.Add(factory.RunTests());

            if (stepSettings.CollectArtifats)
                commands.Add(factory.CollectArtifacts(envSettings, envSettings.BuildArtifactsFolder, dnxSettings.Framework));

            return commands;
        }
        
        int DoWork(ICommand command)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            try
            {
                command.Execute();

                OutputLog.LogTextNewLine("\r\n<<<<done. Elapsed time {0:F2} sec", timer.Elapsed.TotalSeconds);
                return 0;
            }
            catch (Exception e)
            {
                OutputLog.LogTextNewLine("\r\n<<<<exception. Elapsed time {0:F2} sec", timer.Elapsed.TotalSeconds);
                OutputLog.LogException(e);
                lock (tmpXml)
                {
                    tmpXml.WriteStartElement("task");
                    tmpXml.WriteStartElement("error");
                    tmpXml.WriteElementString("message", e.ToString());
                    tmpXml.WriteEndElement();
                    tmpXml.WriteEndElement();
                }
                return 1;
            }
        }
    }
}
