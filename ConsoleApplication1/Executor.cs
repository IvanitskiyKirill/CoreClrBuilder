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
        //bool hasErrors = false;
        public int ExecuteTasks(DNXSettings dnxSettings, StepSettings stepSettings, EnvironmentSettings envSettings)
        {
            tmpXml = new XmlTextWriter(new StringWriter(taskBreakingLog));
            //string currLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //tmpXml = new XmlTextWriter(Path.Combine(currLocation, "vssPathsByTasks.xml"), Encoding.Unicode);
            tmpXml.Formatting = Formatting.Indented;

            int result = 0;
            try
            {
                IEnumerable<ICommand> commands = PrepareCommands(dnxSettings, stepSettings, envSettings, result);
                foreach (var command in commands)
                {
                    IBatchCommand batchCommand = command as IBatchCommand;
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
                Console.WriteLine("All tasks are completed");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                result = 1;
            }
            //Console.WriteLine("Start write logs");
            //if (hasErrors)
            //    tmpXml.WriteEndElement();
            //tmpXml.Flush();
            //tmpXml.Close();
            //Console.WriteLine("End write logs");
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

            if (EnvironmentSettings.Platform == Platform.Unix && stepSettings.CollectArtifats)
                commands.Add(factory.UnixMountTestbuildDirectory(dnxSettings.Runtime, dnxSettings.Framework));

            //if (EnvironmentSettings.Platform == Platform.Windows)
            //    commands.Add(factory.CopyTestbuildFolder(dnxSettings.Runtime, dnxSettings.Framework));

            if (stepSettings.CopyDirs)
                commands.Add(factory.CopyProjects(stepSettings.CopyPath, true));

            if (stepSettings.RemoveProjectsDirectories)
                commands.Add(factory.RemoveProjects());

            if (stepSettings.GetProjectsFromDXVCS)
                commands.Add(factory.GetProjectsFromVCS());
            
            if (stepSettings.Build || stepSettings.RunTests)
                commands.Add(new RestoreCommand(envSettings, String.Empty)); // restore for all

            if (stepSettings.Build) 
                commands.Add(factory.BuildProjects());

            if (stepSettings.RunTests)
                commands.Add(factory.RunTests(dnxSettings.Runtime));

            //if (stepSettings.CollectArtifats)
            //    commands.Add(factory.CollectArtifacts(envSettings, EnvironmentSettings.Platform == Platform.Windows ? envSettings.BuildArtifactsFolder : envSettings.LocalTestbuildFolder, dnxSettings.Runtime, dnxSettings.Framework));

            return commands;
        }
        
        int DoWork(ICommand command)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            try
            {
                Console.WriteLine("Start command: " + command.ToString());
                command.Execute();
                Console.WriteLine("End command: " + command.ToString());

                OutputLog.LogTextNewLine("\r\n<<<<done. Elapsed time {0:F2} sec", timer.Elapsed.TotalSeconds);
                
                return 0;
            }
            catch (Exception e)
            {
                if (command is RunTestsCommand && e is WrongExitCodeException)
                    return 1;

                Console.WriteLine("Exception in command: " + command.ToString());
                OutputLog.LogTextNewLine("\r\n<<<<exception. Elapsed time {0:F2} sec", timer.Elapsed.TotalSeconds);
                OutputLog.LogException(e);
                lock (tmpXml)
                {
                    //if (!hasErrors)
                    //    tmpXml.WriteStartElement("vssPathsByTasks");
                    //hasErrors = true;
                    tmpXml.WriteStartElement("task");
                    tmpXml.WriteStartElement("error");
                    tmpXml.WriteElementString("message", e.ToString());
                    tmpXml.WriteEndElement();
                    tmpXml.WriteEndElement();
                }
                Console.WriteLine("Exit code 1");

                return 1;
            }
        }
    }
}
