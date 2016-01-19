using CoreClrBuilder.Commands;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreClrBuilder.Tests
{
    [TestFixture]
    class CommandFactoryTests
    {
        EnvironmentSettings envSettings;
        DNXSettings dnxSettings = new DNXSettings(null);
        CommandFactory factory;
        [SetUp]
        public void Setup() {
            envSettings = new EnvironmentSettings();
            ProjectsInfo productInfo = new ProjectsInfo(new System.Xml.XmlDocument(), null);
            factory = new CommandFactory(envSettings, productInfo);
        }
        [Test]
        public void InstallEnvironment() {
            BatchCommand command = factory.InstallEnvironment(dnxSettings) as BatchCommand;
            Assert.IsNotNull(command);

            Assert.AreEqual(3, command.Commands.Count);
            Assert.IsInstanceOf<DownloadDNVMCommand>(command.Commands[0]);
            Assert.IsInstanceOf<InstallDNXCommand>(command.Commands[1]);
            Assert.IsInstanceOf<GetNugetConfigCommand>(command.Commands[2]);
        }
        [Test]
        public void GetProjectsFromVCS() {

        }
        [Test]
        public void BuildProjects() { }
        [Test]
        public void RunTests() { }
        [Test]
        public void CopyProjects() { }
        [Test]
        public void RemoveProjects() { }
        [Test]
        public void CollectArtifacts() { }
    }
}
