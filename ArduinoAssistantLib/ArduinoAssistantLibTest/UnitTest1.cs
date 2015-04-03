using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ArduinoAssistantLib;

namespace ArduinoAssistantLibTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestCreateProject()
        {
            Project testProject = new Project("Test", @"D:\", "Test description");
            Project newProject = Project.createNewProject("Test", @"D:\", "Test description");
            Assert.AreEqual(testProject, newProject);
        }
    }
}
