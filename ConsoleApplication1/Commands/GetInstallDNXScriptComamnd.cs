namespace CoreClrBuilder.Commands
{
    class GetInstallDNXScriptComamnd : GetFromVCSCommand
    {
        string workingDir;
        public GetInstallDNXScriptComamnd(EnvironmentSettings settings, DNXSettings dnxsettings) :
            base(
                settings,
                string.Format("{0}/dnxInstall.sh", settings.RemoteSettingsPath),
                @"./",
                "get installation script",
                settings.WorkingDir)
        {
            workingDir = settings.WorkingDir;
        }
    }

}
