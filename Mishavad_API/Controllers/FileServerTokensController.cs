using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Mishavad_API.Models;

using System.Security.Cryptography;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
namespace Mishavad_API.Controllers
{
    [RoutePrefix("api/FST")]
    [Authorize]
    public class FileServerTokensController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        
        // GET: api/FST/Check/userId/token
        [Route("Check/{userId?}/{token?}")]
        [AllowAnonymous]
        [ResponseType(typeof(int))]
        [HttpGet]
        public IHttpActionResult Check(string accountNumber, string token)
        {

            if (accountNumber == null || token == null)
            {
                return BadRequest();
            }

            System.Diagnostics.Debug.WriteLine("Token recieved by FST Checker:" + token);

            var hash = Helpers.FileServerTokenManager.GenerateHash(token);
            
            var query = from t in db.FileServerTokens
                        orderby t.TokenExpDateUtc descending
                        where (t.AccountNumber==accountNumber  && t.TokenHash == hash)
                        select t;
            if (query.Count() > 0)
            {
                var tokenobject = query.First();
                var result = DateTime.Compare(tokenobject.TokenExpDateUtc, DateTime.UtcNow) > 0;
              //TODO: uncomment these for in-production
              //  db.FileServerTokens.Remove(tokenobject);
              //  await db.SaveChangesAsync();
                if (result)
                {
                    return Ok(tokenobject.FileTokenType);
                }
                else {
                    return Unauthorized();//Add token expired message
                }
            }
            return Unauthorized();
         
        }

        // POST: api/FST/Avatar
        //       api/FST/CampaignImage
        //       api/FST/Document
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokenType"> Avatar, CampaignImage, or Document</param>
        /// <returns></returns>
        [Route("{tokenType?}")]
        [ResponseType(typeof(object))]
        [HttpPost]
        public async Task<IHttpActionResult> PostImageToken(string tokenType)
        {
            var userId = User.Identity.GetUserId();
            var user = db.Users.Find(userId);
            var token = Helpers.FileServerTokenManager.GenerateRandomToken();
            var hash = Helpers.FileServerTokenManager.GenerateHash(token);

            var fst = new FileServerToken
            {
                TokenHash = hash,
                TokenExpDateUtc = DateTime.UtcNow.Add(Helpers.FileServerTokenManager.TokenTimeSpan),
                AccountNumber = userId
            };

            fst.FileTokenType = (FileServerTokenType)Enum.Parse(typeof(FileServerTokenType), tokenType);


            db.FileServerTokens.Add(fst);
           await db.SaveChangesAsync();

            return Created<object>("DefaultApi", new { userId = userId, token = System.Web.HttpUtility.UrlEncode(token) });
        }
    }
}
