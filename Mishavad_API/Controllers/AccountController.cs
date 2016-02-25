using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using Mishavad_API.Models;
using Mishavad_API.Providers;
using Mishavad_API.Results;

using System.Net;
using System.Web.Http.Description;
using System.Linq;
using System.Data.Entity;
using System.Text.RegularExpressions;
namespace Mishavad_API.Controllers
{
    [Authorize]
    [RoutePrefix("api/Account")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AccountController : ApiController
    {
        private const string LocalLoginProvider = "Local";
        private ApplicationUserManager _userManager;
        private static Random _random = new Random();

        private ApplicationDbContext db = new ApplicationDbContext();

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

        /// <summary>
        /// This method is used to check whether a user is already registered or not (you can call it both with local/external authentication)
        /// We added to this method other information we need. In case of a non-registered user, they will be null.
        /// </summary>
        /// <returns></returns>
        // GET api/Account/UserInfo
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("UserInfo")]
        public UserInfoViewModel GetUserInfo()
        {
            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            var info = new UserInfoViewModel
            {
                Email = User.Identity.GetUserName(),
                HasRegistered = externalLogin == null,
                LoginProvider = externalLogin != null ? externalLogin.LoginProvider : null
            };

            if (info.HasRegistered) {
                var user_id = int.Parse(User.Identity.GetUserId());
                var user = db.Users.Find(user_id);
                if (user == null)
                     CustomInternalServerError("User can not be found by Id");
                else {
                    info.FirstName = user.UserInfo.FirstName;
                    info.LastName = user.UserInfo.LastName;

                    info.EmailConfirmed = user.EmailConfirmed;
                   
                }
            }

            return info;   
        }


        // GET api/Account/Campaigns
        [Route("Campaigns")]
        public IHttpActionResult GetUserCampaigns() {
            var user_id = int.Parse(User.Identity.GetUserId());
            var userCampaigns = db.Campaigns.Where(c => c.CreatedById == user_id)
                .Include(c=>c.Category)
                .Include(c=>c.ProjectStage)
                .Include(c=>c.ThumbnailFileServer);
           // CustomInternalServerError("",userCampaigns.ToString());
            return Ok(userCampaigns);
        }
       
        // POST api/Account/Logout
        [Route("Logout")]
        public IHttpActionResult Logout()
        {
            Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return Ok();
        }

        // GET api/Account/ManageInfo?returnUrl=%2F&generateState=true
        [Route("ManageInfo")]
        public async Task<ManageInfoViewModel> GetManageInfo(string returnUrl, bool generateState = false)
        {
            ApplicationUser user = await UserManager.FindByIdAsync(int.Parse(User.Identity.GetUserId()));

            if (user == null)
            {
                return null;
            }

            List<UserLoginInfoViewModel> logins = new List<UserLoginInfoViewModel>();

            foreach (CustomUserLogin linkedAccount in user.Logins)
            {
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = linkedAccount.LoginProvider,
                    ProviderKey = linkedAccount.ProviderKey
                });
            }

            if (user.PasswordHash != null)
            {
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = LocalLoginProvider,
                    ProviderKey = user.UserName,
                });
            }

            return new ManageInfoViewModel
            {
                LocalLoginProvider = LocalLoginProvider,
                Email = user.UserName,
                Logins = logins,
                ExternalLoginProviders = GetExternalLogins(returnUrl, generateState)
            };
        }

        // POST api/Account/ChangePassword
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await UserManager.ChangePasswordAsync(int.Parse(User.Identity.GetUserId()), model.OldPassword,
                model.Password //i.e. NewPassword
                );
            
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/SetPassword
        [Route("SetPassword")]
        public async Task<IHttpActionResult> SetPassword(SetPasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await UserManager.AddPasswordAsync(int.Parse(User.Identity.GetUserId()), model.NewPassword);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/AddExternalLogin
        [Route("AddExternalLogin")]
        public async Task<IHttpActionResult> AddExternalLogin(AddExternalLoginBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

            AuthenticationTicket ticket = AccessTokenFormat.Unprotect(model.ExternalAccessToken);

            if (ticket == null || ticket.Identity == null || (ticket.Properties != null
                && ticket.Properties.ExpiresUtc.HasValue
                && ticket.Properties.ExpiresUtc.Value < DateTimeOffset.UtcNow))
            {
                return BadRequest("External login failure.");
            }

            ExternalLoginData externalData = ExternalLoginData.FromIdentity(ticket.Identity);

            if (externalData == null)
            {
                return BadRequest("The external login is already associated with an account.");
            }

            IdentityResult result = await UserManager.AddLoginAsync(int.Parse(User.Identity.GetUserId()),
                new UserLoginInfo(externalData.LoginProvider, externalData.ProviderKey));

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/RemoveLogin
        [Route("RemoveLogin")]
        public async Task<IHttpActionResult> RemoveLogin(RemoveLoginBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result;

            if (model.LoginProvider == LocalLoginProvider)
            {
                result = await UserManager.RemovePasswordAsync(int.Parse(User.Identity.GetUserId()));
            }
            else
            {
                result = await UserManager.RemoveLoginAsync(int.Parse(User.Identity.GetUserId()),
                    new UserLoginInfo(model.LoginProvider, model.ProviderKey));
            }

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // GET api/Account/ExternalLogin
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
        [AllowAnonymous]
        [Route("ExternalLogin", Name = "ExternalLogin")]
        public async Task<IHttpActionResult> GetExternalLogin(string provider, string error = null)
        {
            if (error != null)
            {
                return Redirect(Url.Content("~/") + "#error=" + Uri.EscapeDataString(error));
            }

            if (!User.Identity.IsAuthenticated)
            {
                return new ChallengeResult(provider, this);
            }

            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            if (externalLogin == null)
            {
                return InternalServerError();
            }

            if (externalLogin.LoginProvider != provider)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                return new ChallengeResult(provider, this);
            }

            ApplicationUser user = await UserManager.FindAsync(new UserLoginInfo(externalLogin.LoginProvider,
                externalLogin.ProviderKey));

            bool hasRegistered = user != null;

            if (hasRegistered)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                
                 ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(UserManager,
                    OAuthDefaults.AuthenticationType);
                ClaimsIdentity cookieIdentity = await user.GenerateUserIdentityAsync(UserManager,
                    CookieAuthenticationDefaults.AuthenticationType);

                AuthenticationProperties properties = ApplicationOAuthProvider.CreateProperties(user.UserName);
                Authentication.SignIn(properties, oAuthIdentity, cookieIdentity);
            }
            else
            {
                IEnumerable<Claim> claims = externalLogin.GetClaims();
                ClaimsIdentity identity = new ClaimsIdentity(claims, OAuthDefaults.AuthenticationType);
                Authentication.SignIn(identity);
            }

            return Ok();
        }

        // GET api/Account/ExternalLogins?returnUrl=%2F&generateState=true
        [AllowAnonymous]
        [Route("ExternalLogins")]
        public IEnumerable<ExternalLoginViewModel> GetExternalLogins(string returnUrl, bool generateState = false)
        {
            IEnumerable<AuthenticationDescription> descriptions = Authentication.GetExternalAuthenticationTypes();
            List<ExternalLoginViewModel> logins = new List<ExternalLoginViewModel>();

            string state;

            if (generateState)
            {
                const int strengthInBits = 256;
                state = RandomOAuthStateGenerator.Generate(strengthInBits);
            }
            else
            {
                state = null;
            }

            foreach (AuthenticationDescription description in descriptions)
            {
                ExternalLoginViewModel login = new ExternalLoginViewModel
                {
                    Name = description.Caption,
                    Url = Url.Route("ExternalLogin", new
                    {
                        provider = description.AuthenticationType,
                        response_type = "token",
                        client_id = Startup.PublicClientId,
                        redirect_uri = new Uri(Request.RequestUri, returnUrl).AbsoluteUri,
                        state = state
                    }),
                    State = state
                };
                logins.Add(login);
            }

            return logins;
        }

        // POST api/Account/Register
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(RegisterBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            /*TODO: What if the user wants to login only with mobile phone number
                Decide on this case and consult our design doccumentations*/
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                UserInfo = new UserInfo { FirstName = model.FirstName, LastName = model.LastName }
            };

            IdentityResult result = await UserManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                //ignore Name Taken error if exist
                var re = new Regex(@"Name (.*) is already taken.");
                var errors = string.Join("|", from e in result.Errors
                                               let matches = re.Matches(e)
                                               where matches.Count == 0
                                               select e);
                //change Email Taken error to front-end format;
                errors = Regex.Replace(errors, @"Email '(.*)' is already taken.", "EmailTaken,$1");

                return BadRequest(errors);
            }

            var confirmationCode = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
            var uriBuilder = new UriBuilder(Url.Content("~/api/Account/ConfirmEmail"));
            var nvc = HttpUtility.ParseQueryString(uriBuilder.Query);
            nvc.Add("userId", user.Id.ToString());
            nvc.Add("code", confirmationCode);
            nvc.Add("redirectUrl", model.redirectUrl);
            uriBuilder.Query = nvc.ToString();
            var confirmUrl = uriBuilder.ToString();

           
            // Construct the message body as HTML.
            string body = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">";
            body += "<HTML><HEAD><META http-equiv=Content-Type content=\"text/html; charset=UTF-8\">";
            body += "</HEAD><BODY><DIV><FONT face=Tahoma color=#ff0000 size=2>";
            body += "لطفا اکانت خود را با این لینک فعال کنید:";
            body += "<a href=\"" + confirmUrl + "\">لینک</a>";
            body += "</FONT></DIV></BODY></HTML>";

            try
            {
                await UserManager.SendEmailAsync(user.Id,
                   "(اکانت خود را فعال کنید) Confirm your account",
                   body);
            }
            catch (Exception ex) {
                Console.WriteLine("SendEmail Error:" + ex.Message);
                //TODO (B): Add code here to catch if anything is wrong with Confirmation Emails and inform Admin
            }

            return Ok("Registered");
        }



        // POST api/Account/RegisterExternal
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("RegisterExternal")]
        public async Task<IHttpActionResult> RegisterExternal(RegisterExternalBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var info = await Authentication.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return InternalServerError();
            }

            var user = new ApplicationUser() { UserName = model.Email, Email = model.Email };

            IdentityResult result = await UserManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            result = await UserManager.AddLoginAsync(user.Id, info.Login);
            if (!result.Succeeded)
            {
                return GetErrorResult(result); 
            }
            return Ok();
        }
        /*
        //POST api/Account/RegisterEasy
        public async Task<IHttpActionResult> RegisterEasy(RegisterEasyBindingModel model) {

        }
        */
        // GET api/Account/ConfirmEmail
        [HttpGet]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> ConfirmEmail(int userId, string code, string redirectUrl) {
            var response = new HttpResponseMessage();

            var responseTemplate = "<html><head>";
            responseTemplate+= "<style>div {{direction:rtl; width: 350px;margin: auto;height: 100px;    position: relative;    top: 200px;    background: orange;    text-align: center; font-family: Tahoma, serif; padding:20px; line-height: 30px;    color: white;    border-radius: 5px;    border: darkorange solid 1px;}}</style>";
            responseTemplate+= "<style>body {{background:url(\"data: image / png; base64,iVBORw0KGgoAAAANSUhEUgAAAAQAAAAECAYAAACp8Z5 + AAAAGklEQVQIW2P4//8/AxI2RuGAaBQOTADOAWEAjAwwnWPnCWYAAAAASUVORK5CYII=\");background-color: darkgray;}}</style>";
            responseTemplate +="<meta charset=\"UTF-8\"></head><body><div><p>{0}</p></div></body></html>";

            if (userId == 0 || code == null)
            {
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
            }
            else {
                try {
                    var result = await UserManager.ConfirmEmailAsync(userId, code);
                    if (result.Succeeded)
                    {
                        response.StatusCode = System.Net.HttpStatusCode.OK;
                        var confirmedMessage = "ایمیل شما تأیید گردید";
                        confirmedMessage += "برای بازگشت به *می شود*<a href=\"" + redirectUrl + "\">اینجا کلیک کنید</a>";
                        response.Content = new StringContent(
                            string.Format(responseTemplate, confirmedMessage));
                        response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/html");
                    }
                    else
                    {
                        return EmailNotConfirmed(responseTemplate);
                    }
                } catch (System.InvalidOperationException)
                {
                    return EmailNotConfirmed(responseTemplate);
                }
            }
            return response;

        }

        private HttpResponseMessage EmailNotConfirmed(string responseTemplate) {
            var response = new HttpResponseMessage();
            response.StatusCode = System.Net.HttpStatusCode.BadRequest;
            response.Content = new StringContent(
                string.Format(responseTemplate, "خطا در تأیید ایمیل! احتمالا لینک معتبر نیست. دوباره برای تأیید ایمیل درخواست دهید."));
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/html");
            return response;
        }

        // GET api/Account/FullName
        [Route("FullName")]
        public string GetFullName()
        {
            return User.FullName();
        }

       
        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Helpers

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        private class ExternalLoginData
        {
            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string UserName { get; set; }

            public IList<Claim> GetClaims()
            {
                IList<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider));

                if (UserName != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));
                }

                return claims;
            }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                {
                    return null;
                }

                Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer)
                    || String.IsNullOrEmpty(providerKeyClaim.Value))
                {
                    return null;
                }

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                {
                    return null;
                }

                return new ExternalLoginData
                {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    UserName = identity.FindFirstValue(ClaimTypes.Name)
                };
            }
        }

        private static class RandomOAuthStateGenerator
        {
            private static RandomNumberGenerator _random = new RNGCryptoServiceProvider();

            public static string Generate(int strengthInBits)
            {
                const int bitsPerByte = 8;

                if (strengthInBits % bitsPerByte != 0)
                {
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");
                }

                int strengthInBytes = strengthInBits / bitsPerByte;

                byte[] data = new byte[strengthInBytes];
                _random.GetBytes(data);
                return HttpServerUtility.UrlTokenEncode(data);
            }
        }

        private void CustomInternalServerError(string reason,  string content="")
        {

            throw new HttpResponseException(
                new HttpResponseMessage(HttpStatusCode.InternalServerError)
            { ReasonPhrase = "Internal Server Error - " + reason, Content =new StringContent(content)});
        }

        #endregion
    }
}
