using System;

namespace CoreClrBuilder.Commands
{
    class RestoreCommand : Command
    {
        EnvironmentSettings settings;
        CoreClrProject project;
        DNXSettings dnxSettings;

        public RestoreCommand(EnvironmentSettings settings, CoreClrProject project, DNXSettings dnxSettings)
        {
            this.settings = settings;
            this.project = project;
            this.dnxSettings = dnxSettings;
        }

        protected override void PrepareCommand()
        {
            string additionalSettings = string.Empty;
            if (EnvironmentSettings.Platform == Platform.Unix &&
                dnxSettings.Runtime == DNXSettings.CORECLR_RUNTIME &&
                dnxSettings.Architecture == DNXSettings.X64_ARCH) {
                additionalSettings = " --runtime ubuntu.14.04-x64";
            }
            Init(settings.DNU, string.Format("restore {0}{1}", project.LocalPath, additionalSettings), "call dnu restore", settings.WorkingDir);
        }
    }

}
