using NUnit.Engine.Addins;
using System.IO;
using System.Xml;

namespace CoreClrBuilder.Commands {
    public class Nunit3To2Coverter : ICommand {
        string nunit3ResultPath;
        string nunit2ResultPath;
        public Nunit3To2Coverter(string nunit3ResultPath, string nunit2ResultPath) {
            this.nunit3ResultPath = nunit3ResultPath;
            this.nunit2ResultPath = nunit2ResultPath;
        }
        public void Execute() {
            XmlDocument xmldoc = new XmlDocument();
            FileStream fs = new FileStream(nunit3ResultPath, FileMode.Open, FileAccess.Read);
            xmldoc.Load(fs);
            
            var writer = new MyNUnit2XmlResultWriter();
            writer.WriteResultFile(xmldoc.GetElementsByTagName("test-run").Item(0), nunit2ResultPath);
        }
    }
}
    