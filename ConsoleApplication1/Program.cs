using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        static void Main(string[] args)
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

            Execute("DXVCSGet.exe", "vcsservice.devexpress.devx $/CCNetConfig/LocalProjects/15.2/BuildPortable/build.bat");
            Execute("build.bat", null);
            Execute("dnx", @"Win\DevExpress.Data test -xml DevExpress.Data-TestResult.xml");
            Execute("dnx", @"Win\DevExpress.XtraPrinting\DevExpress.Printing.Core test -xml DevExpress.Printing.Core-TestResult.xml");
            Execute("dnx", @"Win\DevExpress.Office\DevExpress.Office.Core test -xml DevExpress.Office.Core-TestResult.xml");
            Execute("dnx", @"Win\DevExpress.XtraCharts\DevExpress.Charts.Core test -xml DevExpress.Charts.Core-TestResult.xml");
            Execute("dnx", @"Win\DevExpress.XtraCharts\DevExpress.Sparkline.Core test -xml DevExpress.Sparkline.Core-TestResult.xml");
            Execute("dnx", @"Win\DevExpress.XtraSpreadsheet\DevExpress.Spreadsheet.Core test -xml DevExpress.Spreadsheet.Core-TestResult.xml");
            Execute("dnx", @"Win\DevExpress.XtraRichEdit\DevExpress.RichEdit.Core test -xml DevExpress.RichEdit.Core-TestResult.xml");
            Execute("dnx", @"Win\DevExpress.Pdf\DevExpress.Pdf.Core test -xml DevExpress.Pdf.Core-TestResult.xml");

            Execute("DXVCSGet.exe", "vcsservice.devexpress.devx $/CCNetConfig/LocalProjects/15.2/BuildPortable/NUnitXml.xslt");
            XslCompiledTransform xslt = new XslCompiledTransform();
            xslt.Load("NUnitXml.xslt");

            foreach (var result in testResults)
            {
                string resultPath = Path.Combine(WorkingDir, result);
                string xunitResultPath = resultPath.Replace("TestResult", "NunitTestResult");
                if (File.Exists(resultPath))
                    File.Delete(xunitResultPath);
                if (File.Exists(resultPath))
                    xslt.Transform(resultPath, xunitResultPath);

            }
        }
        static void Execute(string fileName, string args) {
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
}
