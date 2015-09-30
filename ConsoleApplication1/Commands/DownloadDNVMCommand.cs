using System;
using System.IO;

namespace CoreClrBuilder.Commands
{
    class DownloadDNVMCommand : Command
    {
        string dnvm;
        EnvironmentSettings settings;
        public DownloadDNVMCommand(EnvironmentSettings settings)

        {
            this.settings = settings;
        }

        public override void Execute()
        {
            if (!File.Exists(dnvm))
                base.Execute();
        }

        protected override void PrepareCommand()
        {
            dnvm = settings.DNVM;
            string executableFile = "powershell.exe";
            string args = "-NoProfile -ExecutionPolicy unrestricted -Command \" &{$Branch = 'dev'; iex((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.ps1'))}\"";
            Init(executableFile, args, "Download dnvm", settings.WorkingDir);
        }
    }

}
