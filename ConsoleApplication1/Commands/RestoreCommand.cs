using System;

namespace CoreClrBuilder.Commands
{
    class RestoreCommand : Command
    {
        EnvironmentSettings settings;
        CoreClrProject project;

        public RestoreCommand(EnvironmentSettings settings, CoreClrProject project)
        {
            this.settings = settings;
            this.project = project;
        }

        protected override void PrepareCommand()
        {
            Init(settings.DNU, string.Format("restore {0}", project.LocalPath), "call dnu restore", settings.WorkingDir);
        }
    }

}
