using System;

namespace CoreClrBuilder.Commands
{
    class RestoreCommand : Command
    {
        EnvironmentSettings settings;
        string projectPath;

        public RestoreCommand(EnvironmentSettings settings, string projectPath)
        {
            this.settings = settings;
            this.projectPath = projectPath;
        }

        protected override void PrepareCommand()
        {
            Init(settings.DotNet, string.Format("restore {0}", projectPath), "call dnu restore", settings.WorkingDir);
        }
    }

}
