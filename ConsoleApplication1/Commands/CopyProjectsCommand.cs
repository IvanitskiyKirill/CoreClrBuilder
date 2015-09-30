using System;
using System.IO;

namespace CoreClrBuilder.Commands
{
    class CopyProjectsCommand : ICommand
    {
        ProjectsInfo productInfo;
        string copyPath;
        bool copySubDirs;
        public CopyProjectsCommand(ProjectsInfo productInfo, string copyPath, bool copySubDirs)
        {
            this.productInfo = productInfo;
            this.copyPath = copyPath;
            this.copySubDirs = copySubDirs;
        }

        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                Console.WriteLine(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
                return;
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location. 
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        public void Execute()
        {
            foreach (var item in productInfo.Projects)
            {
                if (Directory.Exists(item.LocalPath))
                {
                    Console.WriteLine("Begin copy dir {0} ", item.LocalPath);
                    DirectoryCopy(item.LocalPath, Path.Combine(copyPath, item.LocalPath), true);
                }
                else
                {
                    Console.WriteLine("dir {0} doesn't exist", item.LocalPath);
                }
            }
        }
    }

}
