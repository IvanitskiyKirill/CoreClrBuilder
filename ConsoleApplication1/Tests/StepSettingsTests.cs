using NUnit.Framework;

namespace CoreClrBuilder.Tests
{
    [TestFixture]
    class StepSettingsTests
    {
        [Test]
        public void DefaultSettings()
        {
            StepSettings settings = new StepSettings(null);
            Assert.AreEqual(settings.DefaultSteps, settings.Steps);
        }

        [Test]
        public void EnvironmentInitialization()
        {
            StepSettings settings = new StepSettings(new string[] { StepSettings.ENV_INIT });
            Assert.AreEqual(Steps.EnvironmentInitialization, settings.Steps);
        }

        [Test]
        public void BuildProjects()
        {
            StepSettings settings = new StepSettings(new string[] { StepSettings.BUILD_PROJECTS });
            Assert.AreEqual(Steps.Build, settings.Steps);
        }

        [Test]
        public void GetProjects()
        {
            StepSettings settings = new StepSettings(new string[] { StepSettings.GET_PROJECTS });
            Assert.AreEqual(Steps.GetProjectsFromDXVCS, settings.Steps);
        }

        [Test]
        public void RemoveProjects()
        {
            StepSettings settings = new StepSettings(new string[] { StepSettings.REMOVE_PROJECTS });
            Assert.AreEqual(Steps.RemoveProjectsDirectories, settings.Steps);
        }

        [Test]
        public void RunTests()
        {
            StepSettings settings = new StepSettings(new string[] { StepSettings.TEST_PROJECTS });
            Assert.AreEqual(Steps.RunTests, settings.Steps);
        }

        [Test]
        public void CopyProjects()
        {
            StepSettings settings = new StepSettings(new string[] { StepSettings.COPY_PROJECTS });
            Assert.AreEqual(settings.DefaultSteps, settings.Steps);

            settings = new StepSettings(new string[] { StepSettings.COPY_PROJECTS, "destination path" });
            Assert.AreEqual(Steps.CopyDirs, settings.Steps);
        }

        [Test]
        public void CollectArtifacts()
        {
            StepSettings settings = new StepSettings(new string[] { StepSettings.COLLECT_ARTIFATCS });
            Assert.AreEqual(settings.DefaultSteps | Steps.CollectArtifacts, settings.Steps);
        }

        [Test]
        public void CompatibleSteps() {

            string[] compatibleSteps = new string[] {
                StepSettings.ENV_INIT,
                StepSettings.REMOVE_PROJECTS,
                StepSettings.GET_PROJECTS,
                StepSettings.BUILD_PROJECTS,
                StepSettings.TEST_PROJECTS,
                StepSettings.COLLECT_ARTIFATCS,
                StepSettings.COPY_PROJECTS, "destination path"
            };

            StepSettings settings = new StepSettings(compatibleSteps);
            Assert.AreEqual(settings.AllSteps, settings.Steps);
        }
    }
}
