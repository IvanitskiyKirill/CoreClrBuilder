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
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 1 && (args[0] == "-h" || args[0] == "-help"))
            {
                Console.Write(@"
""-u"" - use unstable version of dnx
""-r"" - runtime clr or coreclr
""-arch"" - x64 or x86
""-v"" - version of dnx (Example: 1.0.0-beta4-11566)
""dnx451"" or ""dotnet"" or ""dnxcore50"" - target framework
");
                return 0;
            }
            DNXSettings settings = new DNXSettings();
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (string.Compare(arg, "dotnet", true) == 0)
                    settings.Framework = "dotnet";
                else if (string.Compare(arg, "dnx451", true) == 0)
                    settings.Framework = "dnx451";
                else if (string.Compare(arg, "dnxcore50", true) == 0)
                    settings.Framework = "dnxcore50";
                else if (string.Compare(arg, "-u", true) == 0)
                    settings.UnstableChannel = true;
                else if (string.Compare(arg, "-r", true) == 0 && i < args.Length - 1)
                {
                    if (string.Compare(args[i + 1], "coreclr", true) == 0)
                        settings.Runtime = "coreclr";
                    else if (string.Compare(args[i + 1], "clr", true) == 0)
                        settings.Runtime = "clr";
                }
                else if (string.Compare(arg, "-arch", true) == 0 && i < args.Length - 1)
                {
                    if (string.Compare(args[i + 1], "x64", true) == 0)
                        settings.Architecture = "x64";
                    else if (string.Compare(args[i + 1], "x86", true) == 0)
                        settings.Architecture = "x86";
                }
                else if (string.Compare(arg, "-v", true) == 0 && i < args.Length - 1)
                {
                    settings.DNXVersion = args[i + 1];
                }
            }
            Executor executor = new Executor();
            return executor.ExecuteTasks(settings);
        }
    }
    class DNXSettings
    {
        public bool UnstableChannel { get; set; }
        public string Runtime { get; set; }
        public string Architecture { get; set; }
        public string DNXVersion { get; set; }
        public string Framework { get; set; }

        public DNXSettings()
        {
            UnstableChannel = false;
            Architecture = "x64";
            DNXVersion = string.Empty;
        }

        public string CreateArgsForDNX()
        {
            string args = string.IsNullOrEmpty(DNXVersion) ? "upgrade" : "install " + DNXVersion + " -Persist";
            if (string.IsNullOrEmpty(Runtime))
            {
                string runtime = string.IsNullOrEmpty(Framework) || string.Compare(Framework, "dnx451", true) == 0 ? "clr" : "coreclr";
                args += " -r " + runtime;
            }
            else
                args += " -r " + Runtime;
            args += " -arch " + Architecture;
            if (UnstableChannel)
                args += " -u";
            return args;
        }
    }
}
