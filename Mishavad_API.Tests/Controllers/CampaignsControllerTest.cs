using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mishavad_API.Controllers;
using System.Linq;

namespace Mishavad_API.Tests.Controllers
{
    [TestClass]
    public class CampaignsControllerTest
    {
        [TestMethod]
        public void GetCampaigns()
        {
            // Arrange
            var controller = new CampaignsController();

            // Act
            IQueryable<object> result = controller.GetCampaigns();

            // Assert
            Assert.IsNotNull(result);
            
        }
    }
}
