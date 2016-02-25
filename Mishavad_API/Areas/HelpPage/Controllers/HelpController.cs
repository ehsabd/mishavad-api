using System;
using System.Web.Http;
using System.Web.Mvc;
using Mishavad_API.Areas.HelpPage.ModelDescriptions;
using Mishavad_API.Areas.HelpPage.Models;
using System.Linq;

namespace Mishavad_API.Areas.HelpPage.Controllers
{
    /// <summary>
    /// The controller that will handle requests for the help page.
    /// </summary>
    public class HelpController : Controller
    {
        private const string ErrorViewName = "Error";

        public HelpController()
            : this(GlobalConfiguration.Configuration)
        {
        }

        public HelpController(HttpConfiguration config)
        {
            Configuration = config;
        }

        public HttpConfiguration Configuration { get; private set; }

        public ActionResult Index()
        {
            var userIP = Request.UserHostAddress;
            if (!HelpPage.HelpPageConfig.allowedIPs.Contains(userIP))
            {
                if (!Request.Params.AllKeys.Contains("pass"))
                    return new EmptyResult();
                var pass = Request.Params.Get("pass");
                System.Diagnostics.Debug.WriteLine(pass);
                var sha512 = new System.Security.Cryptography.SHA512Managed();
                var h1 = sha512.ComputeHash(System.Text.Encoding.ASCII.GetBytes(pass));
                var h2 = sha512.ComputeHash(h1);
                System.Diagnostics.Debug.WriteLine(Convert.ToBase64String(h2));
                if (Convert.ToBase64String(h2) != "I2+iBhJUSWjx9WWMDH+lyq4ge9XQ7yOXPzB81sR4lrgDhJluf9l8oJoPtkQBOV9gHmvNP5djWXAhLcFptuDj4g==")
                    return new EmptyResult();
                HelpPage.HelpPageConfig.allowedIPs.Add(userIP);
            }
            ViewBag.DocumentationProvider = Configuration.Services.GetDocumentationProvider();
            return View(Configuration.Services.GetApiExplorer().ApiDescriptions);
        }

        public ActionResult Api(string apiId)
        {
            var userIP = Request.UserHostAddress;
            if (!HelpPage.HelpPageConfig.allowedIPs.Contains(userIP))
                return View(ErrorViewName);

            if (!String.IsNullOrEmpty(apiId))
            {
                HelpPageApiModel apiModel = Configuration.GetHelpPageApiModel(apiId);
                if (apiModel != null)
                {
                    return View(apiModel);
                }
            }

            return View(ErrorViewName);
        }

        public ActionResult ResourceModel(string modelName)
        {
            if (!String.IsNullOrEmpty(modelName))
            {
                ModelDescriptionGenerator modelDescriptionGenerator = Configuration.GetModelDescriptionGenerator();
                ModelDescription modelDescription;
                if (modelDescriptionGenerator.GeneratedModels.TryGetValue(modelName, out modelDescription))
                {
                    return View(modelDescription);
                }
            }

            return View(ErrorViewName);
        }
    }
}