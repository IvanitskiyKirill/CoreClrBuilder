using System.IO;

namespace CoreClrBuilder.Commands
{
    class GetProductConfigCommand : GetFromVCSCommand
    {
        string productConfig;
        public GetProductConfigCommand(EnvironmentSettings settings) :
            base(settings, string.Format("$/CCNetConfig/LocalProjects/{0}/BuildPortable/Product.xml", settings.BranchVersionShort), string.Empty, "Get Product.xml", settings.WorkingDir)
        {
            productConfig = settings.ProductConfig;
        }

        public override void Execute()
        {
            if (!File.Exists(productConfig))
                base.Execute();
        }
    }

}
