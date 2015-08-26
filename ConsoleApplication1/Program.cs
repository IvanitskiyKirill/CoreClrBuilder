using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;

namespace CoreClrBuilder
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 1 && (args[0] == "-h" || args[0] == "-help"))
            {
                Console.Write(@"
""-u"" - use unstable version of dnx
""-r"" - runtime clr or coreclr
""-arch"" - x64 or x86
""-v"" - version of dnx (Example: 1.0.0-beta4-11566)
""dnx451"" or ""dotnet"" or ""dnxcore50"" - target framework
""-branch"" - release branch (Example: -branch 15.2)
""exclude_steps: <steps>""  - (Sample: exclude_steps: get restore test)
    Availible steps to exclude:
    ""get"" - get projects from DXVCS    
    ""restore"" - restore packages for projects
    ""build"" - get from DXVCS, restore packages and build projects
    ""test"" - run tests
");
                return 0;
            }
            StepSettings stepSettings = new StepSettings(args);
            DNXSettings dnxSettings = new DNXSettings(args);
            EnvironmentSettings envSettings = new EnvironmentSettings(args);
            if (!File.Exists(envSettings.ProductConfig) && string.IsNullOrEmpty(envSettings.BranchVersion))
            {
                Console.Write("Please specify branch version (Example: -branch 15.2) or put Product.xml near CoreClrBuilder.exe");
                return 0;
            }
            Executor executor = new Executor();
            return executor.ExecuteTasks(dnxSettings, stepSettings, envSettings);
        }
    }
}
