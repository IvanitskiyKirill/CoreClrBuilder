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
        CommandBuilder builder;
        EnvironmentSettings envSettings;
        StepSettings stepSettings;
        public int ExecuteTasks(DNXSettings dnxSettings, StepSettings stepSettings, EnvironmentSettings envSettings)
        {
            tmpXml = new XmlTextWriter(new StringWriter(taskBreakingLog));
            tmpXml.Formatting = Formatting.Indented;
            int result = 0;
            try
            {
                this.stepSettings = stepSettings;
                this.envSettings = envSettings;
                builder = new CommandBuilder(this.envSettings);

                result += InstallEnvironment(dnxSettings);

                if (stepSettings.RemoveProjectsDirectories)
                    RemoveProjects();

                if (result == 0 && stepSettings.Build)
                    result += BuildProjects();
                if (result == 0 && stepSettings.RunTests)
                    result += RunTests();
            }
            catch (Exception)
            {
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

        private void RemoveProjects()
        {
            Console.WriteLine("Remove projects");
            foreach (var item in productInfo.Projects)
            {
                if (Directory.Exists(item.LocalPath))
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(item.LocalPath);
                    setAttributesNormal(dirInfo);
                    Console.WriteLine("Remove dir {0}", item.LocalPath);
                    Directory.Delete(item.LocalPath, true);
                }
                else
                {
                    Console.WriteLine("dir {0} doesn't exist", item.LocalPath);
                }
            }
        }

        void setAttributesNormal(DirectoryInfo dir)
        {
            foreach (DirectoryInfo subDir in dir.GetDirectories())
                setAttributesNormal(subDir);
            foreach (FileInfo fileInfo in dir.GetFiles())
                fileInfo.Attributes = FileAttributes.Normal;

        }

        int InstallEnvironment(DNXSettings dnxsettings)
        {
            int result = 0;
            if (stepSettings.EnvironmentInitialization)
                result = DoWork(new Command[] {
                    builder.GetProductConfig(),
                    builder.DownloadDNVM(),
                    builder.InstallDNX(dnxsettings) });

            envSettings.InitializeDNX();
            productInfo = new ProductInfo(envSettings.ProductConfig, dnxsettings.Framework);
            envSettings.SetBranchVersion(productInfo.ReleaseVersion);

            if (stepSettings.EnvironmentInitialization)
                result += DoWork(builder.GetNugetConfig());
            return result;
        }
        int BuildProjects()
        {
            List<Command> commands = new List<Command>();
            foreach (var project in productInfo.Projects)
            {
                if (stepSettings.GetProjectsFromDXVCS)
                    commands.Add(builder.GetProject(project));
                if (stepSettings.RestorePackages)
                    commands.Add(builder.Restore(project));
                commands.Add(builder.Build(project));
                commands.Add(builder.InstallPackage(project));
            }
            return DoWork(commands);
        }
        int RunTests()
        {
            int result = DoWork(builder.GetFromVCS(string.Format("$/CCNetConfig/LocalProjects/{0}/BuildPortable/NUnitXml.xslt", envSettings.BranchVersionShort)));
            XslCompiledTransform xslt = new XslCompiledTransform();
            xslt.Load("NUnitXml.xslt");

            List<string> nUnitTestFiles = new List<string>();
            foreach (var project in productInfo.Projects)
            {
                string xUnitResults = Path.Combine(envSettings.WorkingDir, project.TestResultFileName);
                string nUnitResults = Path.Combine(envSettings.WorkingDir, project.NunitTestResultFileName);

                if (File.Exists(xUnitResults))
                    File.Delete(xUnitResults);

                result += DoWork(builder.RunTests(project));

                if (File.Exists(nUnitResults))
                    File.Delete(nUnitResults);
                if (File.Exists(xUnitResults))
                {
                    xslt.Transform(xUnitResults, nUnitResults);
                    nUnitTestFiles.Add(nUnitResults);
                }
            }

            NUnitMerger.MergeFiles(nUnitTestFiles, "nunit-result.xml");
            return result;
        }
        int DoWork(Command command)
        {
            return DoWork(new Command[] { command });
        }
        int DoWork(IEnumerable<Command> commands)
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
}
