using System;

namespace CoreClrBuilder.Commands
{
    class RunTestsCommand : Command
    {
        EnvironmentSettings settings;
        CoreClrProject project;
        string runtime;
        public RunTestsCommand(EnvironmentSettings settings, CoreClrProject project, string runtime)
        {
            this.settings = settings;
            this.project = project;
            this.runtime = runtime;
        }

        protected override void PrepareCommand()
        {
            string monoOptions = runtime == DNXSettings.MONO_RUNTIME ? "-parallel none" : string.Empty;
            string args = string.Format(@"-p {0} --configuration {1} test {2} -xml {3}", project.LocalPath, project.BuildConfiguration, monoOptions, project.TestResultFileName);
            Init(settings.DotNet, args, "run tests", settings.WorkingDir);
        }
    }

}
