using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreClrBuilder
{
    public class WrongExitCodeException : Exception
    {
        public WrongExitCodeException(string fileName, string arguments, int exitCode, List<string> output)
            :
            base(String.Format("Process \"{0}\" has finished with error code {1}\nArguments :{2}\nOutput :\n{3}", fileName, exitCode, arguments, String.Join("\n", output.ToArray())))
        {
        }
    }
}
