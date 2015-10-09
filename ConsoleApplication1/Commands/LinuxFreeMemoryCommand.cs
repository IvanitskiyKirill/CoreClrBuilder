using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreClrBuilder.Commands
{
    class LinuxFreeMemoryStartCommand : Command
    {
        protected override void PrepareCommand()
        {
            Init("sync", "", "sync", "");
        }
    }
    class LinuxFreeMemoryCommand : Command
    {
        protected override void PrepareCommand()
        {
            Init("echo", "3 > /proc/sys/vm/drop_caches", "drop caches", "");
        }
    }
}
