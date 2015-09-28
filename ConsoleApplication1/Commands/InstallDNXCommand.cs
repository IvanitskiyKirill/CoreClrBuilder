namespace CoreClrBuilder.Commands
{
    class InstallDNXCommand : Command
    {
        public InstallDNXCommand(EnvironmentSettings settings, DNXSettings dnxsettings)
        {
            if (settings.Platform == Platform.Windows)
            {
                Init(settings.DNVM, dnxsettings.CreateArgsForDNX(), "Install dnx", settings.WorkingDir);
            }
            else
            {

                Init("bash", dnxsettings.CreateArgsForBashScript(), "Install dnx", settings.WorkingDir);

            }
        }
    }

}
