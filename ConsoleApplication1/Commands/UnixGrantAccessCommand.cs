namespace CoreClrBuilder.Commands
{
    class UnixGrantAccessCommand : Command
    {
        public UnixGrantAccessCommand(string path, string workingDir) : base("chmod", "-R 777 " + path, "grant access to folder " + path, workingDir)
        {
        }
    }

}
