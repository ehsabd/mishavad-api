using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.Web.Http.Description;
using System.Threading;
using System.Threading.Tasks;
using Mishavad_API.Models;
using System.Data.Entity;
using System.IdentityModel.Services;
using System.Security.Permissions;
using System.Web.Http.OData;
using System.Web.Http.OData.Query;
using System.Web.Http.OData.Extensions;
using Microsoft.AspNet.Identity;
namespace Mishavad_API.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize]
    public class UserInfoesController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/UserInfoes
        // TODO (B): implement error handling this throws error when check fails
        // Although this is not neccessary it make error more meaningful. 
        // links to implement
        // http://www.codeproject.com/Articles/686802/Exception-Handling-Using-Custom-Attributes-and-Sta
        // http://www.asp.net/web-api/overview/error-handling/exception-handling
        // NOTE: We cannot search users by names because this
        // requires decryption of all names and this is memory/cpu-intensive
        // Thus we used ApplicationUser for ODataQueryOptions
        [ClaimsPrincipalPermission(SecurityAction.Demand, Operation = "ListUsers", Resource =
"User")]
        public PageResult<object> Get(ODataQueryOptions<ApplicationUser> options)
        {
            //TODO: Limit kinds of Query that could be done. 
            //look at :http://www.asp.net/web-api/overview/odata-support-in-aspnet-web-api/supporting-odata-query-options
            ODataQuerySettings settings = new ODataQuerySettings()
            {
                PageSize = 20
            };
            var query = db.Users;
         

            var userResults = options.ApplyTo(query, settings) as IQueryable<ApplicationUser>;

            var userInfoResults = userResults.Select(user => new
            {
                FirstName = user.UserInfo.FirstName,
                LastName = user.UserInfo.LastName,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed
            }
                            );

            return new PageResult<object>(
                userInfoResults as IEnumerable<object>,
                Request.ODataProperties().NextLink,
                Request.ODataProperties().TotalCount);
        }

        // GET: api/UserInfoes/5
        public async Task<IHttpActionResult> Get(int id)
        {
                //TODO: Think about how Admin and the user him/herself should be able to get/change their account data (user info)
                //      This is not used to show user info. Currently we use /api/Account/UserInfo instead
                var user = await db.UserInfos.FindAsync(id);
                if (user == null)
                    return NotFound();
                else
                    return Ok<UserInfo>(user);
        }

        // POST (Update): api/UserInfoes
        public async Task Post(int id, [FromBody]string value)
        {
        }

    }
}
