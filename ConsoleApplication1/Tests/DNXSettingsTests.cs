using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreClrBuilder.Tests
{
    [TestFixture]
    class DNXSettingsTests
    {
        DNXSettings settings;
        [Test]
        public void DefaultSettings() {
            settings = new DNXSettings(null);
            AssertSettings("clr", "x64", false);
            Assert.AreEqual("upgrade -r clr -arch x64", settings.CreateArgsForDNX());
        }
        [Test]
        public void Runtime()
        {
            settings = new DNXSettings(parse("-r coreclr"));
            AssertSettings("coreclr", "x64", false);
            Assert.AreEqual("upgrade -r coreclr -arch x64", settings.CreateArgsForDNX());

            settings = new DNXSettings(parse("-r clr"));
            AssertSettings("clr", "x64", false);
            Assert.AreEqual("upgrade -r clr -arch x64", settings.CreateArgsForDNX());
        }
        [Test]
        public void DefaultRuntime() {
            settings = new DNXSettings(parse("dotnet"));
            Assert.AreEqual("coreclr", settings.Runtime);

            settings = new DNXSettings(parse("dnxcore50"));
            Assert.AreEqual("coreclr", settings.Runtime);

            settings = new DNXSettings(parse("dnx451"));
            Assert.AreEqual("clr", settings.Runtime);
        }
        [Test]
        public void Architecture() {
            settings = new DNXSettings(parse("-arch"));
            Assert.AreEqual("x64", settings.Architecture);

            settings = new DNXSettings(parse("-arch x64"));
            Assert.AreEqual("x64", settings.Architecture);

            settings = new DNXSettings(parse("-arch x86"));
            Assert.AreEqual("x86", settings.Architecture);

        }
        [Test]
        public void Unstable()
        {
            settings = new DNXSettings(parse("-u"));
            Assert.IsTrue(settings.UnstableChannel);
            Assert.AreEqual("upgrade -r clr -arch x64 -u", settings.CreateArgsForDNX());
        }
        [Test]
        public void DNXVersion()
        {
            settings = new DNXSettings(parse("-v"));
            Assert.IsNull(settings.DNXVersion);
            string dnxVersion = "1.0.0-beta8-15616";
            settings = new DNXSettings(parse("-v " + dnxVersion));
            Assert.AreEqual(dnxVersion, settings.DNXVersion);
            Assert.AreEqual("install 1.0.0-beta8-15616 -Persist -r clr -arch x64", settings.CreateArgsForDNX());
        }
        [Test]
        public void Frameworks() {
            settings = new DNXSettings(parse("dotnet"));
            Assert.AreEqual("dotnet", settings.Framework);

            settings = new DNXSettings(parse("dnxcore50"));
            Assert.AreEqual("dnxcore50", settings.Framework);

            settings = new DNXSettings(parse("dnx451"));
            Assert.AreEqual("dnx451", settings.Framework);
        }

        void AssertSettings(string runtime, string arch, bool unstable) {
            AssertSettings(runtime, arch, unstable, null);
        }
        void AssertSettings(string runtime, string arch, bool unstable, string framework)
        {
            AssertSettings(runtime, arch, unstable, framework, null);
        }
        void AssertSettings(string runtime, string arch, bool unstable, string framework, string dnxVersion)
        {
            Assert.AreEqual(unstable, settings.UnstableChannel);
            Assert.AreEqual(runtime, settings.Runtime);
            Assert.AreEqual(arch, settings.Architecture);
            Assert.AreEqual(dnxVersion, settings.DNXVersion);
            Assert.AreEqual(framework, settings.Framework);
        }
        string[] parse(string args) {
            return args.Trim().Split();
        }
    }
}
