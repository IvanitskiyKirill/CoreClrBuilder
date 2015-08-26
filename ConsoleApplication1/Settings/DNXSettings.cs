using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreClrBuilder
{
    class DNXSettings
    {
        public bool UnstableChannel { get; set; }
        public string Runtime { get; set; }
        public string Architecture { get; set; }
        public string DNXVersion { get; set; }
        public string Framework { get; set; }

        public DNXSettings(string [] args)
        {
            UnstableChannel = false;
            Architecture = "x64";
            DNXVersion = string.Empty;

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (string.Compare(arg, "dotnet", true) == 0)
                    Framework = "dotnet";
                else if (string.Compare(arg, "dnx451", true) == 0)
                    Framework = "dnx451";
                else if (string.Compare(arg, "dnxcore50", true) == 0)
                    Framework = "dnxcore50";
                else if (string.Compare(arg, "-u", true) == 0)
                    UnstableChannel = true;
                else if (string.Compare(arg, "-r", true) == 0 && i < args.Length - 1)
                {
                    if (string.Compare(args[i + 1], "coreclr", true) == 0)
                        Runtime = "coreclr";
                    else if (string.Compare(args[i + 1], "clr", true) == 0)
                        Runtime = "clr";
                }
                else if (string.Compare(arg, "-arch", true) == 0 && i < args.Length - 1)
                {
                    if (string.Compare(args[i + 1], "x64", true) == 0)
                        Architecture = "x64";
                    else if (string.Compare(args[i + 1], "x86", true) == 0)
                        Architecture = "x86";
                }
                else if (string.Compare(arg, "-v", true) == 0 && i < args.Length - 1)
                    DNXVersion = args[i + 1];
            }
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
