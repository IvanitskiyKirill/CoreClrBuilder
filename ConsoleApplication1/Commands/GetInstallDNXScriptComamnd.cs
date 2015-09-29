using System.IO;

namespace CoreClrBuilder.Commands
{
    class GetInstallDNXScriptComamnd : GetFromVCSCommand
    {
        public GetInstallDNXScriptComamnd(EnvironmentSettings settings, DNXSettings dnxsettings) :
            base(
                settings,
                Path.Combine(settings.RemoteSettingsPath, "dnxInstall.sh"),
                settings.WorkingDir,
                "get installation script",
                settings.WorkingDir)
        {
        }
    }

}
