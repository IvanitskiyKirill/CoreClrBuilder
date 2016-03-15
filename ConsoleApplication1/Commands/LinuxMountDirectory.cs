using System.IO;

namespace CoreClrBuilder.Commands {
    class LinuxMountDirectory : Command {
        readonly string sourcePath;
        readonly string targetPath;
        readonly string workingDir;

        public LinuxMountDirectory(string sourcePath, string targetPath, string workingDir) {
            this.workingDir = workingDir;
            this.targetPath = targetPath;
            this.sourcePath = sourcePath;
        }

        string Args {
            get {
                return string.Format(
                    @"-c ""cat Linux/rootpwd | sudo -S mount -t cifs -o credentials={0},uid=1000,gid=1000,rw --source {1} --target {2}""", //yes, it's awful
                    PlatformPathsCorrector.Inst.Correct(Path.Combine(workingDir, "Linux", "credentials"), Platform.Windows),
                    sourcePath,
                    targetPath);
            }
        }

        protected override void PrepareCommand() {
            Init("bash", Args, string.Format("mount remote {0} to local {1}", sourcePath, targetPath), workingDir);
        }
    }
}