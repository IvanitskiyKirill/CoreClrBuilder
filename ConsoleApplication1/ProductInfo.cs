using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CoreClrBuilder
{
    class ProductInfo
    {

        List<CoreClrProject> projects = new List<CoreClrProject>();
        public List<CoreClrProject> Projects { get { return projects; } }

        public ProductInfo(string fileName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            string releaseVersion = doc.SelectSingleNode("/ProductInfo/ProductInformation/Version").InnerText;
            XmlNode vssLocations = doc.SelectSingleNode("/ProductInfo/VSSLocations");
            foreach (XmlNode location in vssLocations.ChildNodes)
            {
                string buildConf = location.Attributes["BuildConfiguration"] == null ? "Debug" : location.Attributes["BuildConfiguration"].InnerText;
                projects.Add(new CoreClrProject(location.Attributes["VSSPath"].InnerText, location.Attributes["ReferenceName"].InnerText, releaseVersion, buildConf));
            }
        }
    }
}
