using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;

namespace CoreClrBuilder
{
    class Executor
    {
        XmlTextWriter tmpXml;
        StringBuilder taskBreakingLog = new StringBuilder();
        ProductInfo productInfo;
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
                    result += DoWork(command);
                    if (result > 0)
                        break;
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
            productInfo = new ProductInfo(envSettings.ProductConfig, dnxSettings.Framework);
            envSettings.SetBranchVersion(productInfo.ReleaseVersion);

            factory = new CommandFactory(envSettings, productInfo);
            List<ICommand> commands = new List<ICommand>();

            if (stepSettings.EnvironmentInitialization)
                commands.Add(factory.InstallEnvironment(dnxSettings));
            if (stepSettings.Build || stepSettings.RunTests)
                envSettings.FindPathToDNX();
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
                commands.Add(factory.CollectArtifacts(envSettings.BuildArtifactsFolder, dnxSettings.Framework));

            return commands;
        }
        
        int DoWork(ICommand command)
        {
            return DoWork(new ICommand[] { command });
        }
        int DoWork(IEnumerable<ICommand> commands)
        {
            //string cclistner = Environment.GetEnvironmentVariable("CCNetListenerFile", EnvironmentVariableTarget.Process);
            //string cclistner = "output.xml";
            Stopwatch timer = new Stopwatch();
            timer.Start();
            try
            {
                foreach (var command in commands)
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
        static void WriteCCFile(string cclistner, Task task, int pos, int count)
        {
            StringBuilder res = new StringBuilder();
            using (XmlTextWriter xtw = new XmlTextWriter(new StringWriter(res)))
            {
                xtw.WriteStartElement("data");
                xtw.WriteStartElement("Item");
                xtw.WriteAttributeString("Time", DateTime.Now.ToString());
                xtw.WriteAttributeString("Data", String.Format("{2} ({0}/{1})", pos + 1, count, task.GetType().ToString()));
                xtw.WriteEndElement();
                PropertyInfo[] propertyInfos = task.GetType().GetProperties();
                foreach (PropertyInfo propertyInfo in propertyInfos)
                {
                    string propertyValue = FormatPropertyValue(task, propertyInfo);
                    if (!String.IsNullOrEmpty(propertyValue))
                    {
                        xtw.WriteStartElement("Item");
                        xtw.WriteAttributeString("Time", String.Empty);
                        if (propertyValue.Length > 80)
                            propertyValue = propertyValue.Substring(0, 40) + "..." + propertyValue.Substring(propertyValue.Length - 40);
                        xtw.WriteAttributeString("Data", string.Format("    {0}='{1}'", propertyInfo.Name, propertyValue));
                        xtw.WriteEndElement();
                    }
                }
                xtw.WriteEndElement();
            }
            try
            {
                File.WriteAllText(cclistner, res.ToString(), Encoding.Unicode);
            }
            catch
            {
            }
        }
        static string FormatPropertyValue(Task task, PropertyInfo propertyInfo)
        {
            try
            {
                string propertyValue = string.Empty;
                object value = propertyInfo.GetValue(task, null);
                if (value is IEnumerable && !(value is string))
                {
                    foreach (object elem in (IEnumerable)value)
                        propertyValue = string.Format("{0}{1}'{2}'",
                            propertyValue, propertyValue.Length > 0 ? ", " : string.Empty, elem.ToString());
                }
                else
                {
                    if (value != null)
                        propertyValue = value.ToString();
                }
                return propertyValue;
            }
            catch
            {
                return String.Empty;
            }
        }
    }

    class RemoveProjectsCommand : ICommand
    {
        readonly ProductInfo productInfo;
        public RemoveProjectsCommand(ProductInfo productInfo)
        {
            this.productInfo = productInfo;
        }

        void SetAttributesNormal(DirectoryInfo dir)
        {
            foreach (DirectoryInfo subDir in dir.GetDirectories())
                SetAttributesNormal(subDir);
            foreach (FileInfo fileInfo in dir.GetFiles())
                fileInfo.Attributes = FileAttributes.Normal;

        }
        public void Execute()
        {
            Console.WriteLine("Remove projects");
            foreach (var item in productInfo.Projects)
            {
                if (Directory.Exists(item.LocalPath))
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(item.LocalPath);
                    SetAttributesNormal(dirInfo);
                    Console.WriteLine("Remove dir {0}", item.LocalPath);
                    Directory.Delete(item.LocalPath, true);
                }
                else
                {
                    Console.WriteLine("dir {0} doesn't exist", item.LocalPath);
                }
            }
        }
    }
}
