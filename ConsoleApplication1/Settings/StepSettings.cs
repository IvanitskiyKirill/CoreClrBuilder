using System;

namespace CoreClrBuilder
{
    class StepSettings
    {
        public bool EnvironmentInitialization { get; set; }
        public bool Build { get; private set; }
        public bool RunTests { get; private set; }
        public bool GetProjectsFromDXVCS { get; private set; }
        public bool RemoveProjectsDirectories { get; private set; }
        public bool CopyDirs { get; private set; }
        public string CopyPath { get; private set; }

        public StepSettings(string [] args)
        {
            bool isDefaultState = true;
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Compare(args[i], "get", true) == 0)
                {
                    isDefaultState = false;
                    GetProjectsFromDXVCS = true;
                }
                else if (string.Compare(args[i], "build", true) == 0)
                {
                    isDefaultState = false;
                    Build = true;
                }
                else if (string.Compare(args[i], "test", true) == 0)
                {
                    isDefaultState = false;
                    RunTests = true;
                }
                else if (string.Compare(args[i], "env_init", true) == 0)
                {
                    isDefaultState = false;
                    EnvironmentInitialization = true;
                }
                else if (string.Compare(args[i], "remove", true) == 0)
                {
                    isDefaultState = false;
                    RemoveProjectsDirectories = true;
                }
                else if (string.Compare(args[i], "copy", true) == 0 && i < args.Length - 1)
                {
                    isDefaultState = false;
                    DisableAllSteps();
                    CopyPath = args[i + 1];
                    CopyDirs = true;
                }
            }
            if (isDefaultState)
                InitDefaultState();
        }

        private void InitDefaultState()
        {
            Build = true;
            RunTests = true;
            GetProjectsFromDXVCS = true;
            EnvironmentInitialization = true;

            RemoveProjectsDirectories = false;
            CopyDirs = false;
        }

        void DisableAllSteps() {
            Build = false;
            RunTests = false;
            GetProjectsFromDXVCS = false;
            EnvironmentInitialization = false;
        }
    }
}
