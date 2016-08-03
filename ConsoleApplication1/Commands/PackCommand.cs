using System;

namespace CoreClrBuilder.Commands
{
    class PackCommand : Command
    {
        EnvironmentSettings settings;
        string projectPath;
        string buildConfiguration;

        public PackCommand(EnvironmentSettings settings, string projectPath, string buildConfiguration)
        {
            this.settings = settings;
            this.projectPath = projectPath;
            this.buildConfiguration = buildConfiguration;
        }

        protected override void PrepareCommand()
        {
            /*
            string buildParams = string.Format("pack {0} --configuration {1}", project.LocalPath, project.BuildConfiguration);
            if (!string.IsNullOrEmpty(project.BuildFramework))
                buildParams += string.Format(" --framework {0}", project.BuildFramework);
            Init(settings.DNU, buildParams, "build", settings.WorkingDir);
            */
            string buildParams = string.Format("pack {0} -o NetCore/bin/", projectPath);
            if (!string.IsNullOrEmpty(buildConfiguration))
                buildParams += string.Format(" --configuration={0}", buildConfiguration);

            Init(settings.DotNet, buildParams, "call build", settings.WorkingDir);
        }
    }

}
