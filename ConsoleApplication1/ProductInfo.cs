using System.Collections.Generic;
using System.Xml;

namespace CoreClrBuilder
{
    class ProductInfo
    {
        public string ReleaseVersion { get; private set; }
        List<CoreClrProject> projects = new List<CoreClrProject>();
        public List<CoreClrProject> Projects { get { return projects; } }

        public ProductInfo(string fileName, string framework)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            ReleaseVersion = doc.SelectSingleNode("/ProductInfo/ProductInformation/Version").InnerText;
            XmlNode vssLocations = doc.SelectSingleNode("/ProductInfo/VSSLocations");
            foreach (XmlNode location in vssLocations.ChildNodes)
            {
                string buildConf = location.Attributes["BuildConfiguration"] == null ? "Debug" : location.Attributes["BuildConfiguration"].InnerText;
                projects.Add(new CoreClrProject(location.Attributes["VSSPath"].InnerText, location.Attributes["ReferenceName"].InnerText, ReleaseVersion, buildConf, framework));
            }
        }
    }
}
