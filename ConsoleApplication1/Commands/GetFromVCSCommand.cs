using System;

namespace CoreClrBuilder.Commands
{
    class GetFromVCSCommand : Command
    {
        EnvironmentSettings settings;
        string remotePath;
        string localPath;
        string comment;
        string workingDir;

        public GetFromVCSCommand(EnvironmentSettings settings, string remotePath, string workingDir) :
            this(settings, remotePath, string.Empty, string.Empty, workingDir)
        { }
        public GetFromVCSCommand(EnvironmentSettings settings, string remotePath, string localPath, string comment, string workingDir)
        {
            this.remotePath = remotePath;
            this.localPath = localPath;
            this.comment = comment;
            this.workingDir = workingDir;
            this.settings = settings;
        }

        protected override void PrepareCommand()
        {
            if (string.IsNullOrEmpty(remotePath))
                throw new ArgumentNullException("remote path cannot be null");
            Init(settings.DXVCSGet, string.Format("vcsservice.devexpress.devx {0} {1}", remotePath, localPath), comment, workingDir);
        }
    }

}
