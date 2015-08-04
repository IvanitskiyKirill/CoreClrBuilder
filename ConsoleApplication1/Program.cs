using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;

namespace CoreClrBuilder
{
    public class WrongExitCodeException : Exception
    {
        public WrongExitCodeException(string fileName, string arguments, int exitCode, List<string> output)
            :
            base(String.Format("Process \"{0}\" has finished with error code {1}\nArguments :{2}\nOutput :\n{3}", fileName, exitCode, arguments, String.Join("\n", output.ToArray())))
        {
        }
    }
    class Program
    {
        static StreamReader /*outputReader,*/ errorReader;
        static List<string> outputErrors = new List<string>();
        static string WorkingDir;
        static int Main(string[] args)
        {
            Executor executor = new Executor();
            return executor.ExecuteTasks();
            //WorkingDir = Environment.CurrentDirectory;

            //List<string> testResults = new List<string>();
            //testResults.Add("DevExpress.Data-TestResult.xml");
            //testResults.Add("DevExpress.Printing.Core-TestResult.xml");
            //testResults.Add("DevExpress.Office.Core-TestResult.xml");
            //testResults.Add("DevExpress.Charts.Core-TestResult.xml");
            //testResults.Add("DevExpress.Sparkline.Core-TestResult.xml");
            //testResults.Add("DevExpress.Spreadsheet.Core-TestResult.xml");
            //testResults.Add("DevExpress.RichEdit.Core-TestResult.xml");
            //testResults.Add("DevExpress.Pdf.Core-TestResult.xml");

            //Execute("DXVCSGet.exe", "vcsservice.devexpress.devx $/CCNetConfig/LocalProjects/15.2/BuildPortable/build.bat");
            //Execute("build.bat", null);
            //Execute("dnx", @"Win\DevExpress.Data test -xml DevExpress.Data-TestResult.xml");
            //Execute("dnx", @"Win\DevExpress.XtraPrinting\DevExpress.Printing.Core test -xml DevExpress.Printing.Core-TestResult.xml");
            //Execute("dnx", @"Win\DevExpress.Office\DevExpress.Office.Core test -xml DevExpress.Office.Core-TestResult.xml");
            //Execute("dnx", @"Win\DevExpress.XtraCharts\DevExpress.Charts.Core test -xml DevExpress.Charts.Core-TestResult.xml");
            //Execute("dnx", @"Win\DevExpress.XtraCharts\DevExpress.Sparkline.Core test -xml DevExpress.Sparkline.Core-TestResult.xml");
            //Execute("dnx", @"Win\DevExpress.XtraSpreadsheet\DevExpress.Spreadsheet.Core test -xml DevExpress.Spreadsheet.Core-TestResult.xml");
            //Execute("dnx", @"Win\DevExpress.XtraRichEdit\DevExpress.RichEdit.Core test -xml DevExpress.RichEdit.Core-TestResult.xml");
            //Execute("dnx", @"Win\DevExpress.Pdf\DevExpress.Pdf.Core test -xml DevExpress.Pdf.Core-TestResult.xml");

            //Execute("DXVCSGet.exe", "vcsservice.devexpress.devx $/CCNetConfig/LocalProjects/15.2/BuildPortable/NUnitXml.xslt");
            //XslCompiledTransform xslt = new XslCompiledTransform();
            //xslt.Load("NUnitXml.xslt");

            //foreach (var result in testResults)
            //{
            //    string resultPath = Path.Combine(WorkingDir, result);
            //    string xunitResultPath = resultPath.Replace("TestResult", "NunitTestResult");
            //    if (File.Exists(resultPath))
            //        File.Delete(xunitResultPath);
            //    if (File.Exists(resultPath))
            //        xslt.Transform(resultPath, xunitResultPath);

            //}
        }
        static void Execute(string fileName, string args)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            //startInfo = new ProcessStartInfo(fileName, args);
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
            errorReader.BaseStream.Position = 0;
            if (outputErrors.Count > 0)
            {
                //using (FileStream fileStream = new FileStream(Path.Combine(WorkingDir, "errorLog.txt"), FileMode.Append, FileAccess.Write)) {
                //    using (StreamWriter writer = new StreamWriter(fileStream)) {
                Console.WriteLine("-[Error]-----------------------------------------------------------------");
                foreach (var error in outputErrors)
                    Console.WriteLine(error);
                Console.WriteLine("-------------------------------------------------------------------------");
            }
            process.WaitForExit();
            if (process.ExitCode != 0)
                throw new WrongExitCodeException(process.StartInfo.FileName, process.StartInfo.Arguments, process.ExitCode, outputErrors);
            errorReader = null;
            process = null;
        }

        //static bool Execute(string fileName, string args)
        //{
        //    Process process = new Process();
        //    ProcessStartInfo startInfo = new ProcessStartInfo();
        //    //startInfo = new ProcessStartInfo(fileName, args);
        //    startInfo = new ProcessStartInfo(fileName, args);
        //    startInfo.WorkingDirectory = WorkingDir;
        //    startInfo.UseShellExecute = false;
        //    //startInfo.RedirectStandardOutput = true;
        //    startInfo.RedirectStandardError = true;
        //    startInfo.CreateNoWindow = false;

        //    process.StartInfo = startInfo;
        //    process.Start();

        //    //outputReader = process.StandardOutput;
        //    //Thread outputThread = new Thread(new ThreadStart(StreamReaderThread_Output));
        //    //outputThread.Start();

        //    errorReader = process.StandardError;

        //    StringBuilder builder = new StringBuilder();
        //    for (;;)
        //    {
        //        string strLogContents = errorReader.ReadLine();
        //        if (strLogContents == null)
        //            break;
        //        else
        //            builder.AppendLine(strLogContents);
        //    }
        //    string errLog = builder.ToString();
        //    if (errLog.Length > 0)
        //    {
        //        //using (FileStream fileStream = new FileStream(Path.Combine(WorkingDir, "errorLog.txt"), FileMode.Append, FileAccess.Write)) {
        //        //    using (StreamWriter writer = new StreamWriter(fileStream)) {
        //        Console.WriteLine("-[Error]-----------------------------------------------------------------");
        //        Console.WriteLine(errLog);
        //        Console.WriteLine("-------------------------------------------------------------------------");
        //        //}
        //        //}
        //        return false;
        //    }
        //    process.WaitForExit();
        //    return true;
        //}

    }

    class Executor
    {
        static StreamReader /*outputReader,*/ errorReader;
        static List<string> outputErrors = new List<string>();
        static string WorkingDir;
        XmlTextWriter tmpXml;
        StringBuilder taskBreakingLog = new StringBuilder();
        public int ExecuteTasks()
        {
            tmpXml = new XmlTextWriter(new StringWriter(taskBreakingLog));
            tmpXml.Formatting = Formatting.Indented;
            int result = 0;
            try
            {
                WorkingDir = Environment.CurrentDirectory;

                List<string> testResults = new List<string>();
                testResults.Add("DevExpress.Data-TestResult.xml");
                testResults.Add("DevExpress.Printing.Core-TestResult.xml");
                testResults.Add("DevExpress.Office.Core-TestResult.xml");
                testResults.Add("DevExpress.Charts.Core-TestResult.xml");
                testResults.Add("DevExpress.Sparkline.Core-TestResult.xml");
                testResults.Add("DevExpress.Spreadsheet.Core-TestResult.xml");
                testResults.Add("DevExpress.RichEdit.Core-TestResult.xml");
                testResults.Add("DevExpress.Pdf.Core-TestResult.xml");


                result += DoWork("DXVCSGet.exe", "vcsservice.devexpress.devx $/CCNetConfig/LocalProjects/15.2/BuildPortable/build.bat");
                result += DoWork("build.bat", null);
                if (result == 0)
                {
                    result += DoWork("dnx", @"Win\DevExpress.Data test -xml DevExpress.Data-TestResult.xml");
                    result += DoWork("dnx", @"Win\DevExpress.XtraPrinting\DevExpress.Printing.Core test -xml DevExpress.Printing.Core-TestResult.xml");
                    result += DoWork("dnx", @"Win\DevExpress.Office\DevExpress.Office.Core test -xml DevExpress.Office.Core-TestResult.xml");
                    result += DoWork("dnx", @"Win\DevExpress.XtraCharts\DevExpress.Charts.Core test -xml DevExpress.Charts.Core-TestResult.xml");
                    result += DoWork("dnx", @"Win\DevExpress.XtraCharts\DevExpress.Sparkline.Core test -xml DevExpress.Sparkline.Core-TestResult.xml");
                    result += DoWork("dnx", @"Win\DevExpress.XtraSpreadsheet\DevExpress.Spreadsheet.Core test -xml DevExpress.Spreadsheet.Core-TestResult.xml");
                    result += DoWork("dnx", @"Win\DevExpress.XtraRichEdit\DevExpress.RichEdit.Core test -xml DevExpress.RichEdit.Core-TestResult.xml");
                    result += DoWork("dnx", @"Win\DevExpress.Pdf\DevExpress.Pdf.Core test -xml DevExpress.Pdf.Core-TestResult.xml");
                    result += DoWork("DXVCSGet.exe", "vcsservice.devexpress.devx $/CCNetConfig/LocalProjects/15.2/BuildPortable/NUnitXml.xslt");
                    XslCompiledTransform xslt = new XslCompiledTransform();
                    xslt.Load("NUnitXml.xslt");

                    foreach (var testRes in testResults)
                    {
                        string resultPath = Path.Combine(WorkingDir, testRes);
                        string xunitResultPath = resultPath.Replace("TestResult", "NunitTestResult");
                        if (File.Exists(resultPath))
                            File.Delete(xunitResultPath);
                        if (File.Exists(resultPath))
                            xslt.Transform(resultPath, xunitResultPath);

                    }
                }
            }
            catch (Exception e)
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
        static void Execute(string fileName, string args)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            //startInfo = new ProcessStartInfo(fileName, args);
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
            if (outputErrors.Count > 0)
            {
                //using (FileStream fileStream = new FileStream(Path.Combine(WorkingDir, "errorLog.txt"), FileMode.Append, FileAccess.Write)) {
                //    using (StreamWriter writer = new StreamWriter(fileStream)) {
                Console.WriteLine("-[Error]-----------------------------------------------------------------");
                foreach (var error in outputErrors)
                    Console.WriteLine(error);
                Console.WriteLine("-------------------------------------------------------------------------");
            }
            process.WaitForExit();
            if (process.ExitCode != 0)
                throw new WrongExitCodeException(process.StartInfo.FileName, process.StartInfo.Arguments, process.ExitCode, outputErrors);
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
                OutputLog.LogTextNewLine("\r\n<<<<done. Elapsed time {0:F2} sec",  timer.Elapsed.TotalSeconds);
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
