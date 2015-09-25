namespace CoreClrBuilder.Commands
{
    class RestoreCommand : Command
    {
        public RestoreCommand(EnvironmentSettings settings, CoreClrProject project) :
            base(settings.DNU, string.Format("restore {0}", project.LocalPath), "call dnu restore", settings.WorkingDir)
        { }
    }

}
