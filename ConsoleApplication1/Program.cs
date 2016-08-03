﻿using CoreClrBuilder.Commands;
using System;
using System.IO;

namespace CoreClrBuilder
{
    class Program
    {
        private static int StartMain(string[] args)
        {
            if (args.Length == 1 && (args[0] == "-h" || args[0] == "-help"))
            {
                Console.Write(@"
CoreClrBuilder.exe [-config:<name>] [env_init] [remove] [get] [build] [test]
    env_init [-r <runtime>] [-arch <name>] [-v <version>]
    build [dnx451] [dotnet] [dnxcore50] 

-config:<config_name> - configuration file with projects and project settings (Not Supported yet)

env_init - dotnet core installation, getting nuget.config, product.xml
    -r - runtime clr or coreclr
    -arch - x64 or x86
    -v - version of dnx (Example: 1.0.0-beta4-11566)

remove - remove direcories with projects (only for Windows)

get - get projects from DXVCS    

build - restore packages and build projects
    dnx451 or dotnet or dnxcore50 - target framework

test - run tests
");
                return 0;
            }
            StepSettings stepSettings = new StepSettings(args);
            DNXSettings dnxSettings = new DNXSettings(args);
            EnvironmentSettings envSettings = new EnvironmentSettings();
            Console.WriteLine("Init Settings");

            if (!File.Exists(envSettings.ProductConfig) && string.IsNullOrEmpty(envSettings.BranchVersion))
            {
                Console.Write("Please place Product.xml near CoreClrBuilder.exe");
                return 1;
            }
            Executor executor = new Executor();
            return executor.ExecuteTasks(dnxSettings, stepSettings, envSettings);
        }

        static int Main(string[] args)
        {
            try
            {
                AppDomain.CurrentDomain.UnhandledException += (sender, e)
                => FatalExceptionObject(e.ExceptionObject);

                return StartMain(args);
            }
            catch (Exception e)
            {
                FatalExceptionObject(e);
                return 1;
            }
        }

        static void FatalExceptionObject(object exceptionObject)
        {
            var e = exceptionObject as Exception;
            if (e == null)
                Console.WriteLine("Unhandled exception doesn't derive from System.Exception: " + exceptionObject.ToString());
            else {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
