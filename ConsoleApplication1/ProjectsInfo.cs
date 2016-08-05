using System;
using System.Collections.Generic;
using System.Xml;

namespace CoreClrBuilder
{
    class ProjectsInfo
    {
        public string ReleaseVersion { get; private set; }
        List<CoreClrProject> projects = new List<CoreClrProject>();
        public List<CoreClrProject> Projects { get { return projects; } }

        public string Framework { get; private set; }
        public ProjectsInfo(XmlDocument doc, string framework) {
            Parse(doc, framework);
        }

        public ProjectsInfo(string fileName, string framework)
        {
            if (fileName == null)
                return;
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            Parse(doc, framework);
        }
        void Parse(XmlDocument doc, string framework) {
            Framework = framework;
            ReleaseVersion = doc.SelectSingleNode("/ProductInfo/ProductInformation/Version").InnerText;
            XmlNode vssLocations = doc.SelectSingleNode("/ProductInfo/VSSLocations");
            foreach (XmlNode location in vssLocations.ChildNodes)
            {
                bool isTestProject = location.Attributes["TestProject"] == null ? false : Convert.ToBoolean(location.Attributes["TestProject"].InnerText);
                string buildConf = location.Attributes["BuildConfiguration"] == null ? (isTestProject ? "DebugTest" : "DebugTest") : location.Attributes["BuildConfiguration"].InnerText;
                string vssPath = location.Attributes["VSSPath"].InnerText;
                string localPath = location.Attributes["ReferenceName"].InnerText;
                projects.Add(new CoreClrProject(vssPath, localPath, ReleaseVersion, buildConf, framework, isTestProject));
            }
        }
    }
}
