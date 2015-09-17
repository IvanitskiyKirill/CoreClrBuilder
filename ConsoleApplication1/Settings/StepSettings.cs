using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreClrBuilder
{
    class StepSettings
    {
        public bool EnvironmentInitialization { get; set; }
        public bool Build { get; private set; }
        public bool RunTests { get; private set; }
        public bool RestorePackages { get; private set; }
        public bool GetProjectsFromDXVCS { get; private set; }

        public bool RemoveProjectsDirectories { get; private set; }
        public bool CopyDirs { get; private set; }
        public string CopyPath { get; private set; }
        public StepSettings(string [] args)
        {
            Build = true;
            RunTests = true;
            RestorePackages = true;
            GetProjectsFromDXVCS = true;
            EnvironmentInitialization = true;

            RemoveProjectsDirectories = false;

            for (int i = 0; i < args.Length; i++)
            {
                if ((string.Compare(args[i], "exclude_steps:", true) == 0 || string.Compare(args[i], "ex:", true) == 0) && i < args.Length - 1)
                {
                    while (i + 1 < args.Length)
                    {
                        if (string.Compare(args[i + 1], "get", true) == 0)
                            GetProjectsFromDXVCS = false;
                        else if (string.Compare(args[i + 1], "restore", true) == 0)
                            RestorePackages = false;
                        else if (string.Compare(args[i + 1], "build", true) == 0)
                            Build = false;
                        else if (string.Compare(args[i + 1], "test", true) == 0)
                            RunTests = false;
                        else if (string.Compare(args[i + 1], "env_init", true) == 0)
                            EnvironmentInitialization = false;
                        else if (string.Compare(args[i + 1], "all", true) == 0)
                        {
                            DisableAllSteps();
                        }
                        else
                            break;
                        i++;
                    }
                }
                else if (string.Compare(args[i], "-remove_projects", true) == 0 || string.Compare(args[i], "-rm", true) == 0) {
                    DisableAllSteps();
                    RemoveProjectsDirectories = true;
                }
                else if (string.Compare(args[i], "-copy", true) == 0 && i < args.Length - 1)
                {
                    DisableAllSteps();
                    CopyPath = args[i + 1];
                    CopyDirs = true;
                }
                else if (string.Compare(args[i], "-get", true) == 0)
                {
                    DisableAllSteps();
                    GetProjectsFromDXVCS = true;
                }
            }
        }
        void DisableAllSteps() {
            Build = false;
            RunTests = false;
            RestorePackages = false;
            GetProjectsFromDXVCS = false;
            EnvironmentInitialization = false;
        }
    }
}
