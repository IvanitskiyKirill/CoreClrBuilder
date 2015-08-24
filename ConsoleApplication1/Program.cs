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
            if (args.Length > 1)
            {
                throw new ArgumentException("Incorrect number of arguments");
            }
            string framework = args.Length == 1 ? args[0] : string.Empty;

            Executor executor = new Executor();
            return executor.ExecuteTasks(framework);
        }
    }
}
