using System.Text;

namespace CoreClrBuilder
{
    class DNXSettings
    {
        public const string UNSTABLE_FLAGE = "-u";
        public const string VERSION_FLAGE = "-v";

        public const string DOTNET_FRAMEWORK = "dotnet";
        public const string DNX451_FRAMEWORK = "dnx451";
        public const string DNXCORE50_FRAMEWORK = "dnxcore50";

        public const string RUNTIME_FLAGE = "-r";
        public const string MONO_RUNTIME = "mono";
        public const string CORECLR_RUNTIME = "coreclr";
        public const string CLR_RUNTIME = "clr";

        public const string ARCH_FLAGE = "-arch";
        public const string X86_ARCH = "x86";
        public const string X64_ARCH = "x64";


        public bool UnstableChannel { get; set; }
        public string Runtime { get; set; }
        public string Architecture { get; set; }
        public string DNXVersion { get; set; }
        public string Framework { get; set; }

        public DNXSettings(string[] args)
        {
            UnstableChannel = false;
            Architecture = X64_ARCH;

            if (args != null)
                for (int i = 0; i < args.Length; i++)
                {
                    string arg = args[i];
                    if (string.Compare(arg, DOTNET_FRAMEWORK, true) == 0 ||
                        string.Compare(arg, DNX451_FRAMEWORK, true) == 0 ||
                        string.Compare(arg, DNXCORE50_FRAMEWORK, true) == 0)
                        Framework = arg;
                    else if (string.Compare(arg, UNSTABLE_FLAGE, true) == 0)
                        UnstableChannel = true;
                    else if (string.Compare(arg, RUNTIME_FLAGE, true) == 0 && i < args.Length - 1)
                    {
                        if (string.Compare(args[i + 1], MONO_RUNTIME, true) == 0 ||
                            string.Compare(args[i + 1], CORECLR_RUNTIME, true) == 0 ||
                            string.Compare(args[i + 1], CLR_RUNTIME, true) == 0)
                            Runtime = args[i + 1];
                    }
                    else if (string.Compare(arg, ARCH_FLAGE, true) == 0 && i < args.Length - 1)
                    {
                        if (string.Compare(args[i + 1], X64_ARCH, true) == 0 ||
                            string.Compare(args[i + 1], X86_ARCH, true) == 0)
                            Architecture = args[i + 1];
                    }
                    else if (string.Compare(arg, VERSION_FLAGE, true) == 0 && i < args.Length - 1)
                        DNXVersion = args[i + 1];
                }
            if (string.IsNullOrEmpty(Runtime))
                Runtime = string.IsNullOrEmpty(Framework) || string.Compare(Framework, DNX451_FRAMEWORK, true) == 0 ? CLR_RUNTIME : CORECLR_RUNTIME;
        }
        public string CreateArgsForDNX()
        {
            string args = string.IsNullOrEmpty(DNXVersion) ? "upgrade" : "install " + DNXVersion + " -Persist";
            args += string.Format(" {0} {1}",RUNTIME_FLAGE, Runtime);
            args += " -arch " + Architecture;
            if (UnstableChannel)
                args += " -u";
            return args;
        }
        public string CreateArgsForBashScript() {
            //string args = string.Format("dnxInstall.sh {0} {1}", Runtime, Architecture);
            //if (UnstableChannel)
            //    args += " " + UNSTABLE_FLAGE;
            //return args;
            return string.Format("dnxInstall.sh {0} {1}", Runtime, (UnstableChannel ? UNSTABLE_FLAGE : string.Empty));
        }
    }
}
