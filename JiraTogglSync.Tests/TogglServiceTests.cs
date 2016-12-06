using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JiraTogglSync.Services;

namespace JiraTogglSync.Tests
{
    [TestClass]
    public class TogglServiceTests
    {
        [TestMethod]
        public void CanBeCreated()
        {
            new TogglService("1fa287625aecd5691a041986168d7af2");
        }
    }
}
