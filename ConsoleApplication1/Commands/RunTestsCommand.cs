namespace CoreClrBuilder.Commands
{
    class RunTestsCommand : Command
    {
        protected override bool ThrowWrongExitCodeException { get { return false; } }
        public RunTestsCommand(EnvironmentSettings settings, CoreClrProject project) :
            base(settings.DNX, string.Format(@"-p {0} --configuration {1} test -xml {2}", project.LocalPath, project.BuildConfiguration, project.TestResultFileName), "run tests", settings.WorkingDir)
        { }
    }

}
