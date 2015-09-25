using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreClrBuilder.Commands
{
    class RemoveProjectsCommand : ICommand
    {
        readonly ProductInfo productInfo;
        public RemoveProjectsCommand(ProductInfo productInfo)
        {
            this.productInfo = productInfo;
        }

        void SetAttributesNormal(DirectoryInfo dir)
        {
            foreach (DirectoryInfo subDir in dir.GetDirectories())
                SetAttributesNormal(subDir);
            foreach (FileInfo fileInfo in dir.GetFiles())
                fileInfo.Attributes = FileAttributes.Normal;

        }
        public void Execute()
        {
            Console.WriteLine("Remove projects");
            foreach (var item in productInfo.Projects)
            {
                if (Directory.Exists(item.LocalPath))
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(item.LocalPath);
                    SetAttributesNormal(dirInfo);
                    Console.WriteLine("Remove dir {0}", item.LocalPath);
                    Directory.Delete(item.LocalPath, true);
                }
                else
                {
                    Console.WriteLine("dir {0} doesn't exist", item.LocalPath);
                }
            }
        }
    }
}
