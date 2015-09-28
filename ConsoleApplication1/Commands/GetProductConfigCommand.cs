using System.IO;

namespace CoreClrBuilder.Commands
{
    class GetProductConfigCommand : GetFromVCSCommand
    {
        string productConfig;
        public GetProductConfigCommand(EnvironmentSettings settings) :
            base(settings, 
                Path.Combine(settings.RemoteSettingsPath,"Product.xml"), 
                settings.WorkingDir, 
                "Get Product.xml", 
                settings.WorkingDir)
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
