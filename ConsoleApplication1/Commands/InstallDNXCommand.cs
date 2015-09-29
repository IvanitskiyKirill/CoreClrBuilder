using System;

namespace CoreClrBuilder.Commands
{
    class InstallDNXCommand : Command
    {
        EnvironmentSettings settings;
        DNXSettings dnxsettings;
        public InstallDNXCommand(EnvironmentSettings settings, DNXSettings dnxsettings)
        {
            this.settings = settings;
            this.dnxsettings = dnxsettings;
        }
        protected override void PrepareCommand()
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
