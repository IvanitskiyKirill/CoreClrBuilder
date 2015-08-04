using System;
using System.IO;

namespace CoreClrBuilder
{
    public class OutputLog
    {
        private static MemoryStream logStream = new MemoryStream();
        private static bool success = true;
        private static int warningCount = 0;
        private static string lastExceptionText = string.Empty;
        public static void Reset()
        {
            logStream.Position = 0;
            warningCount = 0;
            success = true;
        }
        public static void LogTextNewLine(string format, params object[] arg)
        {
            LogText("\r\n" + format, arg);
        }
        public static void LogWarning(string message)
        {
            LogText("\r\n Warning: '" + message, "'");
            warningCount++;
        }
        public static void LogText(string format, params object[] arg)
        {
            lock (logStream)
            {
                if (EnableLog)
                {
                    StreamWriter streamWriter = new StreamWriter(logStream);
                    if (arg.Length > 0)
                        streamWriter.Write(format, arg);
                    else
                        streamWriter.Write(format);
                    streamWriter.Flush();
                }
                if (arg.Length > 0)
                    Console.Write(format, arg);
                else
                    Console.Write(format);
            }
        }
        public static void LogException(Exception e)
        {
            success = false;
            string s = "Error";
            lastExceptionText = e.Message;
            int strikeCount = (70 - s.Length);
            lock (logStream)
            {
                LogTextNewLine(string.Format("\r\n-[{1}]{0}", new String('-', strikeCount), s));
                LogTextNewLine("|\t" + e.ToString().Replace("\r\n", "\r\n|\t"));
                LogTextNewLine(new String('-', 73));
            }
        }
        public static void LogGroupHeader(string format, params object[] args)
        {
            string s = string.Format(format, args);
            int strikeCount = (70 - s.Length) / 2;
            LogTextNewLine(string.Format("\r\n{0}[{1}]{0}", new String('-', strikeCount > 0 ? strikeCount : 0), s));
        }
        public static void LogGroupFooter(string format, params object[] args)
        {
            LogTextNewLine(string.Format("\r\n{0}", new String('-', 70)));
            LogTextNewLine(string.Format(format, args));
            LogTextNewLine(new String('-', 70));
        }
        public static string GetLog()
        {
            if (warningCount > 0)
            {
                LogTextNewLine("----- Warning Count: {0}------", warningCount);
            }
            logStream.Position = 0;
            StreamReader reader = new StreamReader(logStream);
            return reader.ReadToEnd();
        }
        public static bool Success { get { return success; } }
        public static string LastException { get { return lastExceptionText; } }
        public static string BuildResult
        {
            get { return success ? "SUCCESS" : "FAILURE"; ; }
        }
        public static bool EnableLog = true;
    }
}
