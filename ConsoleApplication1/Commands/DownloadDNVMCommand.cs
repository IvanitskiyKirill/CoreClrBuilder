using System.IO;

namespace CoreClrBuilder.Commands
{
    class DownloadDNVMCommand : Command
    {
        string dnvm;
        public DownloadDNVMCommand(EnvironmentSettings settings)

        {
            dnvm = settings.DNVM;
            string executableFile = "powershell.exe";
            string args = "-NoProfile -ExecutionPolicy unrestricted -Command \" &{$Branch = 'dev'; iex((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.ps1'))}\"";
            Init(executableFile, args, "Download dnvm", settings.WorkingDir);
        }

        public override void Execute()
        {
            if (!File.Exists(dnvm))
                base.Execute();
        }
    }

}
