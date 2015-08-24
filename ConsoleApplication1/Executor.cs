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
        EnvironmentSettings settings;
        public int ExecuteTasks(string framework)
        {
            tmpXml = new XmlTextWriter(new StringWriter(taskBreakingLog));
            tmpXml.Formatting = Formatting.Indented;
            int result = 0;
            try
            {
                settings = new EnvironmentSettings();
                builder = new CommandBuilder(settings);

                result += InstallEnvironment();
                productInfo = new ProductInfo(settings.ProductConfig, framework);

                result += BuildProjects(result == 0);
                result += RunTests(result == 0);
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

        int InstallEnvironment() {
            int result = 0;
            
            if (!File.Exists(settings.ProductConfig))
                result += DoWork(builder.GetFromVCS("$/CCNetConfig/LocalProjects/15.2/BuildPortable/Product.xml"));

            if (!File.Exists(settings.DNVM))
                result += DoWork("powershell.exe", "-NoProfile -ExecutionPolicy unrestricted -Command \" &{$Branch = 'dev'; iex((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.ps1'))}\"", "Download dnvm");

            result += DoWork(settings.DNVM, "install latest -Persist -arch x64", "Download dnx");
            result += DoWork(builder.GetFromVCS("$/2015.2/Win/NuGet.Config", @"Win\", "get nuget.config"));

            settings.InitializeDNX();

            return result;
        }
        int BuildProjects(bool canRun) {
            int result = 0;
            if (!canRun)
                return result;
            foreach (var project in productInfo.Projects)
            {
                result += BuildProject(project);
                if (result != 0)
                    break;
            }

            return result;
        }
        int BuildProject(CoreClrProject project) {
            int result = 0;

            builder.Project = project;

            result += DoWork(builder.GetProject);
            if (result != 0)
                return result;  

            result += DoWork(builder.Restore);
            if (result != 0)
                return result;

            result += DoWork(builder.Build);
            if (result != 0)
                return result;

            result += DoWork(builder.InstallPackage);
            
            return result;
        }
        int RunTests(bool canRun)
        {
            int result = 0;
            if (!canRun)
                return result;
            result += DoWork(builder.GetFromVCS("$/CCNetConfig/LocalProjects/15.2/BuildPortable/NUnitXml.xslt"));
            XslCompiledTransform xslt = new XslCompiledTransform();
            xslt.Load("NUnitXml.xslt");

            List<string> nUnitTestFiles = new List<string>();
            foreach (var project in productInfo.Projects)
            {
                string xUnitResults = Path.Combine(settings.WorkingDir, project.TestResultFileName);
                string nUnitResults = Path.Combine(settings.WorkingDir, project.NunitTestResultFileName);

                if (File.Exists(xUnitResults))
                    File.Delete(xUnitResults);

                result += DoWork(builder.RunTests);

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
        int DoWork(string fileName, string args)
        {
            return DoWork(new Command(fileName, args, string.Empty, settings.WorkingDir));
        }
        int DoWork(string fileName, string args, string comment) {
            return DoWork(new Command(fileName, args, comment, settings.WorkingDir));
        }
        int DoWork(Command command)
        {
            //string cclistner = Environment.GetEnvironmentVariable("CCNetListenerFile", EnvironmentVariableTarget.Process);
            //string cclistner = "output.xml";
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
