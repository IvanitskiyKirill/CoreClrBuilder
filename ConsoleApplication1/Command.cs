using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace CoreClrBuilder
{
    class Command
    {
        public static Command CreateEmptyCommand() { return new Command(); }
        StreamReader errorReader;
        List<string> outputErrors = new List<string>();
        string workingDir;
        string fileName;
        string args;
        string comment;
        bool empty;
        private Command()
        {
            empty = true;
        }
        public Command(string fileName, string args, string comment, string workingDir)
        {
            this.fileName = fileName;
            this.args = args;
            this.comment = comment;
            this.workingDir = workingDir;
        }
        public void Execute()
        {
            if (empty)
                return;
            if (!string.IsNullOrEmpty(comment))
                OutputLog.LogText(comment);

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo = new ProcessStartInfo(fileName, args);
            startInfo.WorkingDirectory = workingDir;
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
    }
}
