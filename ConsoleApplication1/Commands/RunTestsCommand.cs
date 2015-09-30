using System;

namespace CoreClrBuilder.Commands
{
    class RunTestsCommand : Command
    {
        EnvironmentSettings settings;
        CoreClrProject project;
        public RunTestsCommand(EnvironmentSettings settings, CoreClrProject project)
        {
            this.settings = settings;
            this.project = project;
        }

        protected override void PrepareCommand()
        {
            Init(settings.DNX, string.Format(@"-p {0} --configuration {1} test -xml {2}", project.LocalPath, project.BuildConfiguration, project.TestResultFileName), "run tests", settings.WorkingDir);
        }
    }

}
