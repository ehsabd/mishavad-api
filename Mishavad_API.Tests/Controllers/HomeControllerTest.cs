using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mishavad_API;
using Mishavad_API.Controllers;

namespace Mishavad_API.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        [TestMethod]
        public void Index()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Home Page", result.ViewBag.Title);
        }
    }
}
