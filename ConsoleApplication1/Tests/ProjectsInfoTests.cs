using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CoreClrBuilder.Tests
{
    [TestFixture]
    class ProjectsInfoTests
    {
        string productConfig = @"<?xml version=""1.0""?>
<ProductInfo xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
InternalName=""Portable.v15.2.2005"" Team=""CoreClr Team"">
	<ProductInformation>
		<PublicName>Portable</PublicName>
		<Version>15.2.0</Version>
		<Suite>true</Suite>
		<Comments>Developer Express components NET</Comments>
		<MenuName>NET</MenuName>
		<ProductVersionSuffix>.v15.2.2005</ProductVersionSuffix>
	</ProductInformation>
	<VSSLocations>
		<VSSLocation VSSPath=""$/2015.2/Win/DevExpress.Data/"" ReferenceName=""Win\DevExpress.Data"" Permanent=""true"" />
		<VSSLocation VSSPath=""$/2015.2/Win/DevExpress.Pdf/DevExpress.Pdf.Core/"" ReferenceName=""Win\DevExpress.Pdf\DevExpress.Pdf.Core"" BuildConfiguration=""DebugTest"" Permanent=""true"" />
	</VSSLocations>
	</ProductInfo>
";

        [Test]
        public void ParseConfig() {
            ProjectsInfo info;
            using (MemoryStream stream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(stream)) {
                    writer.Write(productConfig);
                    writer.Flush();
                    stream.Position = 0;

                    XmlDocument doc = new XmlDocument();
                    doc.Load(stream);
                    info = new ProjectsInfo(doc, "dotnet");
                }
            }
            Assert.AreEqual("dotnet", info.Framework);
            Assert.AreEqual("15.2.0", info.ReleaseVersion);
            Assert.AreEqual(2, info.Projects.Count);

            AssertProject(info.Projects[0], @"Win\DevExpress.Data", "$/2015.2/Win/DevExpress.Data/", "Debug");
            AssertProject(info.Projects[1], @"Win\DevExpress.Pdf\DevExpress.Pdf.Core", "$/2015.2/Win/DevExpress.Pdf/DevExpress.Pdf.Core/", "DebugTest");
        }
        void AssertProject(CoreClrProject project, string localPath, string vssPath, string buildConfiguration) {
            Assert.AreEqual(buildConfiguration, project.BuildConfiguration);
            Assert.AreEqual(localPath, project.LocalPath);
            Assert.AreEqual(vssPath, project.VSSPath);
        }
    }
}
