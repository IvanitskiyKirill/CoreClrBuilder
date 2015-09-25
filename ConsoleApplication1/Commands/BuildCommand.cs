namespace CoreClrBuilder.Commands
{
    class BuildCommand : Command
    {
        public BuildCommand(EnvironmentSettings settings, CoreClrProject project)
        {
            string buildParams = string.Format("pack {0} --configuration {1}", project.LocalPath, project.BuildConfiguration);
            if (!string.IsNullOrEmpty(project.BuildFramework))
                buildParams += string.Format(" --framework {0}", project.BuildFramework);
            Init(settings.DNU, buildParams, "build", settings.WorkingDir);
        }
    }

}
