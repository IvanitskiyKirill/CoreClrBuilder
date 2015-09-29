using System;

namespace CoreClrBuilder.Commands
{
    class GetFromVCSCommand : Command
    {
        public GetFromVCSCommand(EnvironmentSettings settings, string remotePath, string workingDir) :
            this(settings, remotePath, string.Empty, string.Empty, workingDir)
        { }
        public GetFromVCSCommand(EnvironmentSettings settings, string remotePath, string localPath, string comment, string workingDir)
        {

            if (string.IsNullOrEmpty(remotePath))
                throw new ArgumentNullException("remote path cannot be null");
            Init(settings.DXVCSGet, string.Format("vcsservice.devexpress.devx {0} {1}", remotePath, localPath), comment, workingDir);
        }
    }

}
