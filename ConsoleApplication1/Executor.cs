using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;

namespace CoreClrBuilder
{
    class Executor
    {
        StreamReader errorReader;
        List<string> outputErrors = new List<string>();
        string WorkingDir;
        string UserProfile;
        XmlTextWriter tmpXml;
        StringBuilder taskBreakingLog = new StringBuilder();
        ProductInfo productInfo;
        string dnxPath, dnuPath, dnvmPath;

        public int ExecuteTasks()
        {
            tmpXml = new XmlTextWriter(new StringWriter(taskBreakingLog));
            tmpXml.Formatting = Formatting.Indented;
            int result = 0;
            try
            {
                WorkingDir = Environment.CurrentDirectory;
                UserProfile = Environment.GetEnvironmentVariable("USERPROFILE");

                result += InstallEnvironment();
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
            string productConfig = Path.Combine(WorkingDir, "Product.xml");
            if (!File.Exists(productConfig))
                result += DoWork("DXVCSGet.exe", "vcsservice.devexpress.devx $/CCNetConfig/LocalProjects/15.2/BuildPortable/Product.xml");

            productInfo = new ProductInfo(productConfig);
            dnvmPath = string.Format(@"{0}\.dnx\bin\dnvm.cmd", UserProfile);

            if (!File.Exists(dnvmPath))
            {
                OutputLog.LogText("Download dnvm");
                result += DoWork("powershell.exe", "-NoProfile -ExecutionPolicy unrestricted -Command \" &{$Branch = 'dev'; iex((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.ps1'))}\"");
            }

            OutputLog.LogText("Download dnx");
            result += DoWork(dnvmPath, "install latest -Persist");

            OutputLog.LogText("get nuget.config");
            result += DoWork("DXVCSGet.exe", @"vcsservice.devexpress.devx $/2015.2/Win/NuGet.Config Win\");
            //File.Copy(string.Format(@"{0}\Win\NuGet.Config", WorkingDir), string.Format(@"{0}\AppData\Roaming\NuGet\NuGet.Config", UserProfile), true);

            string[] paths = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User).Split(';');
            foreach (var path in paths)
            {
                if (File.Exists(Path.Combine(path, "dnx.exe")) && File.Exists(Path.Combine(path, "dnu.cmd"))) {
                    dnxPath = Path.Combine(path, "dnx.exe");
                    dnuPath = Path.Combine(path, "dnu.cmd");
                }
            }

            return result;
        }
        int BuildProjects(bool canRun) {
            int result = 0;
            if (!canRun)
                return result;
            foreach (var project in productInfo.Projects)
            {
                if (BuildProject(project) != 0)
                    break;
            }

            return result;
        }
        int BuildProject(CoreClrProject project) {
            int result = 0;

            OutputLog.LogText("get from VCS");
            result += DoWork("DXVCSGet.exe", string.Format("vcsservice.devexpress.devx {0} {1}", project.VSSPath, project.LocalPath));
            if (result != 0)
                return result;  

            OutputLog.LogText("call dnu restore");
            result += DoWork(dnuPath, string.Format("restore {0}", project.LocalPath));
            if (result != 0)
                return result;

            OutputLog.LogText("build");
            result += DoWork(dnuPath, string.Format("pack {0}", project.LocalPath));//% --configuration % buildConf %
            if (result != 0)
                return result;

            OutputLog.LogText("install package");
            result += DoWork(dnuPath, string.Format(@"packages add {0}\bin\{1}\{2} {3}\.dnx\packages", project.LocalPath, project.BuildConfiguration, project.NugetPackageName, WorkingDir));
            
            return result;
        }
        int RunTests(bool canRun)
        {
            int result = 0;
            if (!canRun)
                return result;
            result += DoWork("DXVCSGet.exe", "vcsservice.devexpress.devx $/CCNetConfig/LocalProjects/15.2/BuildPortable/NUnitXml.xslt");
            XslCompiledTransform xslt = new XslCompiledTransform();
            xslt.Load("NUnitXml.xslt");

            foreach (var project in productInfo.Projects)
            {
                string xUnitResults = Path.Combine(WorkingDir, project.TestResultFileName);
                string nUnitResults = Path.Combine(WorkingDir, project.NunitTestResultFileName);

                if (File.Exists(xUnitResults))
                    File.Delete(xUnitResults);

                result += DoWork(dnxPath, string.Format(@"{0} test -xml {1}", project.LocalPath, project.TestResultFileName));
                
                if (File.Exists(nUnitResults))
                    File.Delete(nUnitResults);
                if (File.Exists(xUnitResults))
                    xslt.Transform(xUnitResults, nUnitResults);
            }
            return result;
        }

        void Execute(string fileName, string args)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo = new ProcessStartInfo(fileName, args);
            startInfo.WorkingDirectory = WorkingDir;
            startInfo.UseShellExecute = false;
            //startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.CreateNoWindow = false;

            process.StartInfo = startInfo;
            process.Start();

            //outputReader = process.StandardOutput;
            //Thread outputThread = new Thread(new ThreadStart(StreamReaderThread_Output));
            //outputThread.Start();

            errorReader = process.StandardError;

            for (;;)
            {
                string strLogContents = errorReader.ReadLine();
                if (strLogContents == null)
                    break;
                else
                    outputErrors.Add(strLogContents);
            }
            process.WaitForExit();
            if (process.ExitCode != 0)
                throw new WrongExitCodeException(process.StartInfo.FileName, process.StartInfo.Arguments, process.ExitCode, outputErrors);
            outputErrors.Clear();
            errorReader = null;
            process = null;
        }

        int DoWork(string fileName, string args)
        {
            //string cclistner = Environment.GetEnvironmentVariable("CCNetListenerFile", EnvironmentVariableTarget.Process);
            //string cclistner = "output.xml";
            Stopwatch timer = new Stopwatch();
            timer.Start();
            try
            {
                Execute(fileName, args);
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

    //class ProjectBuilder {
    //    CoreClrProject project;
    //    public ProjectBuilder(CoreClrProject project)
    //    {

    //    }

    //    public void Build() {

    //    }
    //}

    class CoreClrCommand {
        public string FileName { get; private set; }
        public string Arguments { get; private set; }
        public string Comment { get; private set; }

        public CoreClrCommand(string fileName, string arguments, string comment)
        {
            FileName = fileName;
            Arguments = arguments;
            Comment = comment;
        }
    }
}
