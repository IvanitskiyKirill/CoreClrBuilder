using System;

namespace CoreClrBuilder.Commands
{
    class UnixGrantAccessCommand : Command
    {
        string path;
        string workingDir;
        public UnixGrantAccessCommand(string path, string workingDir)
        {
            this.path = path;
            this.workingDir = workingDir;
        }

        protected override void PrepareCommand()
        {
            Init("chmod", "-R 777 " + path, "grant access to folder " + path, workingDir);
        }
    }

}
