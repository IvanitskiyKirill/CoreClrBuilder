using System;

namespace CoreClrBuilder
{

    [Flags]
    enum Steps : int {
        NoSteps = 0x00,
        EnvironmentInitialization = 0x01,
        Build = 0x02,
        RunTests = 0x04,
        GetProjectsFromDXVCS = 0x08,
        RemoveProjectsDirectories = 0x16,
        CopyDirs = 0x32,
        CollectArtifacts = 0x64,
    }
    class StepSettings
    {
        public const string GET_PROJECTS = "get";
        public const string BUILD_PROJECTS = "build";
        public const string TEST_PROJECTS = "test";
        public const string ENV_INIT = "env_init";
        public const string REMOVE_PROJECTS = "remove";
        public const string COPY_PROJECTS = "copy";
        public const string COLLECT_ARTIFATCS = "collect_artifatcs";

        readonly Steps steps;
        readonly Steps allSteps = Steps.Build | Steps.CollectArtifacts | Steps.CopyDirs | Steps.EnvironmentInitialization | Steps.GetProjectsFromDXVCS | Steps.RemoveProjectsDirectories | Steps.RunTests;
        readonly Steps defaultSteps = Steps.Build | Steps.RunTests | Steps.GetProjectsFromDXVCS | Steps.EnvironmentInitialization;

        public Steps AllSteps { get { return allSteps; } }
        public Steps DefaultSteps { get { return defaultSteps; } }

        public Steps Steps { get { return steps; } }
        public bool EnvironmentInitialization { get { return (steps & Steps.EnvironmentInitialization) == Steps.EnvironmentInitialization; } }
        public bool Build { get { return (steps & Steps.Build) == Steps.Build; } }
        public bool RunTests { get { return (steps & Steps.RunTests) == Steps.RunTests; } }
        public bool GetProjectsFromDXVCS { get { return (steps & Steps.GetProjectsFromDXVCS) == Steps.GetProjectsFromDXVCS; } }
        public bool RemoveProjectsDirectories { get { return (steps & Steps.RemoveProjectsDirectories) == Steps.RemoveProjectsDirectories; } }
        public bool CopyDirs { get { return (steps & Steps.CopyDirs) == Steps.CopyDirs; } }
        public bool CollectArtifats { get { return (steps & Steps.CollectArtifacts) == Steps.CollectArtifacts; } }
        public string CopyPath { get; private set; }

        public StepSettings(string [] args)
        {
            if (args != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (string.Compare(args[i], GET_PROJECTS, true) == 0)
                        steps |= Steps.GetProjectsFromDXVCS;
                    else if (string.Compare(args[i], BUILD_PROJECTS, true) == 0)
                        steps |= Steps.Build;
                    else if (string.Compare(args[i], TEST_PROJECTS, true) == 0)
                        steps |= Steps.RunTests;
                    else if (string.Compare(args[i], ENV_INIT, true) == 0)
                        steps |= Steps.EnvironmentInitialization;
                    else if (string.Compare(args[i], REMOVE_PROJECTS, true) == 0)
                        steps |= Steps.RemoveProjectsDirectories;
                    else if (string.Compare(args[i], COPY_PROJECTS, true) == 0 && i < args.Length - 1)
                    {
                        CopyPath = args[i + 1];
                        steps |= Steps.CopyDirs;
                    }
                    else if (string.Compare(args[i], COLLECT_ARTIFATCS, true) == 0)
                        steps |= Steps.CollectArtifacts;
                }
            }
            if ((steps & AllSteps) == Steps.NoSteps || (steps & AllSteps) == Steps.CollectArtifacts)
                steps |= defaultSteps;
        }
    }
}
