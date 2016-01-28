using System;
using System.IO;

namespace CoreClrBuilder.Commands
{
    class CopyProjectsCommand : BatchCommand {
        public CopyProjectsCommand(ProjectsInfo productInfo, string copyPath, bool copySubDirs) {
            productInfo.Projects.ForEach(project => {
                Add(new CopyDirectoryCommand(project.LocalPath, Path.Combine(copyPath, project.LocalPath), copySubDirs));
            });
        }
    }

    class CopyDirectoryCommand : ICommand {
        #region static

        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs) {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if(!dir.Exists) {
                Console.WriteLine(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
                return;
            }

            // If the destination directory doesn't exist, create it. 
            if(!Directory.Exists(destDirName)) {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach(FileInfo file in files) {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location. 
            if(copySubDirs) {
                foreach(DirectoryInfo subdir in dirs) {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        #endregion

        readonly string sourcePath;
        readonly string destPath;
        readonly bool copySubDirs;

        public CopyDirectoryCommand(string sourcePath, string destPath, bool copySubDirs) {
            this.sourcePath = sourcePath;
            this.destPath = destPath;
            this.copySubDirs = copySubDirs;
        }

        public void Execute() {
            if(Directory.Exists(sourcePath)) {
                Console.WriteLine("Begin copy dir {0} ", destPath);
                DirectoryCopy(sourcePath, destPath, copySubDirs);
            } else {
                Console.WriteLine("dir {0} doesn't exist", sourcePath);
            }
        }
    }
}
