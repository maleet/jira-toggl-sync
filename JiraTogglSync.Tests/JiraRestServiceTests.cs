using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JiraTogglSync.Services;

namespace JiraTogglSync.Tests
{
	[TestClass]
    public class JiraRestServiceTests
    {
        [TestMethod]
        public void CanBeCreated()
        {
            new JiraRestService("https://atlassian.net", "maleet@gmail.com", "65D3TZVDsPVxmGe88RKT", "RetainRemainingEstimate");
        }
    }
}
