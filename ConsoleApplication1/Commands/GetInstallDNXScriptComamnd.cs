namespace CoreClrBuilder.Commands
{
    class GetInstallDNXScriptComamnd : GetFromVCSCommand
    {
        string workingDir;
        public GetInstallDNXScriptComamnd(EnvironmentSettings settings, DNXSettings dnxsettings) :
            base(
                settings,
                string.Format("$/CCNetConfig/LocalProjects/{0}/BuildPortable/dnxInstall.sh", settings.BranchVersionShort),
                @"./",
                "get installation script",
                settings.WorkingDir)
        {
            workingDir = settings.WorkingDir;
        }
    }

}
