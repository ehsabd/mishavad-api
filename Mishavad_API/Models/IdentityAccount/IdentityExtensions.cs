using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Microsoft.AspNet.Identity;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Threading;
using System.Text.RegularExpressions;

using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.ComponentModel.DataAnnotations;

namespace Mishavad_API.Models
{
    public static class GenericPrincipalExtensions
    {
        public static string FullName(this System.Security.Principal.IPrincipal user)
        {
            if (user.Identity.IsAuthenticated)
            {
                ClaimsIdentity claimsIdentity = user.Identity as ClaimsIdentity;
                foreach (var claim in claimsIdentity.Claims)
                {
                    if (claim.Type == "FullName")
                        return claim.Value;
                }
                return "";
            }
            else
                return "";
        }
    }

    
    public  class CustomizePasswordValidation : IIdentityValidator<string>
    {

        public static int LengthRequired { get; set; }
        public CustomizePasswordValidation()
        {
        }

        public Task<IdentityResult> ValidateAsync(string Item)
        {

            //TODO refine validation error messages
            if (string.IsNullOrEmpty(Item) || Item.Length < LengthRequired)
            {
                return Task.FromResult(IdentityResult.Failed(string.Format(
                    "PasswordValidation_ShortLength,{0}",LengthRequired)));
            }
                                  
            if (!Regex.IsMatch(Item, @".*[0-9a-zA-Z!@#$%^&*0-9]"))
                return Task.FromResult(IdentityResult.Failed(
                    "PasswordValidation_InvalidChars"));
            else if (!Regex.IsMatch(Item, @".*[0-9]"))
                return Task.FromResult(IdentityResult.Failed(
                    "PasswordValidation_NoNumbers"));
            else if (!Regex.IsMatch(Item, @".*[a-zA-Z]"))
                return Task.FromResult(IdentityResult.Failed(
                    "PasswordValidation_SmallCapitalLetters"));
            else if (Regex.IsMatch(Item, @"(.)\1{1,}"))
                return Task.FromResult(IdentityResult.Failed(
                    "PasswordValidation_ConsecutivelyRepeatedChars"));
            else if (Regex.IsMatch(Item, @"13[0-9]{2,}"))
                return Task.FromResult(IdentityResult.Failed(
                    "PasswordValidation_ShamsiDate"));

            return Task.FromResult(IdentityResult.Success);
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class CustomizedPasswordValidationAttribute : ValidationAttribute
    {
        public CustomizedPasswordValidationAttribute()
            : base("Error")
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string Item = (string)value;

            var validator = new CustomizePasswordValidation();
            var result = validator.ValidateAsync(Item).Result; 
            if (result.Succeeded)
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult( string.Join("", result.Errors));
            }
           
            
        }
    }

    public class AuthResource
    {
        public const string Users = "USER";
        public const string Campaigns = "CAMP";
        public const string UserDocuments = "USER_DOC";
        public const string CampaignDocuments = "CAMP_DOC";
        

    }

    public class AuthAction
    {
        public const string List = "LIST";
        public const string GetFullDetails = "GET_FULL_DETAILS";
    }

    /// <summary>
    /// This subclassing is used to implement claim-based access control 
    /// refer to Pro ASP.net Web API Security Pages 92-93
    /// </summary>
    public class AuthorizationManager : ClaimsAuthorizationManager
    {
        public override bool CheckAccess(AuthorizationContext context)
        {
            var resource = context.Resource.First().Value;
            var action = context.Action.First().Value;

        if (
                (action == AuthAction.List && resource == AuthResource.Users)
                || 
                (action== AuthAction.List && resource== AuthResource.Campaigns)
                ||
                (action == AuthAction.GetFullDetails && resource == AuthResource.Campaigns)
                ||
                (action == AuthAction.List && resource == AuthResource.UserDocuments)
                ||
                (action == AuthAction.List && resource == AuthResource.CampaignDocuments)
                )
            {
                ClaimsIdentity identity = (context.Principal.Identity as ClaimsIdentity);
                if (identity.Claims.Any(c => c.Type == ClaimTypes.Role &&
                c.Value.Equals("TopLevelAdmin")))
                        return true;
            }
            return false;
        }
    }

    /// <summary>
    /// You may use this attribute like this:
    /// [ClaimsAuthorize (Action=AuthAction.GetFullDetails, Resource=AuthResource.Campaigns)]
    /// </summary>
    public class ClaimsAuthorize : AuthorizeAttribute
    {
        public string Resource { get; set; }
        public string Action { get; set; }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            
            var auth = new AuthorizationManager();
            var haveAccess =
                auth.CheckAccess(new AuthorizationContext(ClaimsPrincipal.Current, Resource.ToString(), Action.ToString()));

            if (!haveAccess) {
                return false;
            }

            //Continue with the regular Authorize check
            return base.IsAuthorized(actionContext);
        }
    }





    }