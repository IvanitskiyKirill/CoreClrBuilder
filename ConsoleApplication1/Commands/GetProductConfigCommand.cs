using System.IO;

namespace CoreClrBuilder.Commands
{
    class GetProductConfigCommand : GetFromVCSCommand
    {
        string productConfig;
        public GetProductConfigCommand(EnvironmentSettings settings) :
            base(settings, string.Format("{0}/Product.xml", settings.RemoteSettingsPath), string.Empty, "Get Product.xml", settings.WorkingDir)
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
