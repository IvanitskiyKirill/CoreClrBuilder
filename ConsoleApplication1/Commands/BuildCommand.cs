using System;

namespace CoreClrBuilder.Commands
{
    class BuildCommand : Command
    {
        EnvironmentSettings settings;
        CoreClrProject project;

        public BuildCommand(EnvironmentSettings settings, CoreClrProject project)
        {
            this.settings = settings;
            this.project = project;
        }

        protected override void PrepareCommand()
        {
            /*
            string buildParams = string.Format("pack {0} --configuration {1}", project.LocalPath, project.BuildConfiguration);
            if (!string.IsNullOrEmpty(project.BuildFramework))
                buildParams += string.Format(" --framework {0}", project.BuildFramework);
            Init(settings.DNU, buildParams, "build", settings.WorkingDir);
            */
            string buildParams = string.Format("build {0}", project.LocalPath, project.BuildConfiguration);
            Init(settings.DotNet, buildParams, "call build", settings.WorkingDir);
        }
    }

}
