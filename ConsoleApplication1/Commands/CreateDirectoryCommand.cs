using System.IO;

namespace CoreClrBuilder.Commands {
    class CreateDirectoryCommand : ActionCommand {
        public CreateDirectoryCommand(string path) : base("create directory " + path, () => {
            if(Directory.Exists(path)) {
                Directory.Delete(path, true);
            }
            Directory.CreateDirectory(path);
        }) {}
    }
}