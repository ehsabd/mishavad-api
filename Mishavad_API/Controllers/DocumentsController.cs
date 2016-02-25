using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Mishavad_API.Models;

using Microsoft.AspNet.Identity;
using System.Security.Claims;

namespace Mishavad_API.Controllers
{
    [RoutePrefix("api/Documents")]
    [Authorize]
    public class DocumentsController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Documents/Campaign/1
        [Route("Campaign/{campaignId}")]
        [ResponseType(typeof(IQueryable<Document>))]
        public async Task <IHttpActionResult> GetCampaignDocuments(int campaignId)
        {
            var camp = await db.SafelyGetCampaignById(campaignId);
            var userId = User.Identity.GetUserId();
            var docs = camp.CampaignDocumentMaps.Select(m => m.Document);
            var auth = new AuthorizationManager();
            var haveAccess =
                auth.CheckAccess(new AuthorizationContext(ClaimsPrincipal.Current, AuthResource.CampaignDocuments, AuthAction.List));
            if (haveAccess)
            {
                return Ok(await GetDocumentsWithDownloadLinks(docs));
            }
            else {
                
                if (camp.CreatedById.ToString() != userId) return Unauthorized();

                return Ok(docs);
            }
        }

        // GET: api/Documents/Campaign/1/5
        [Route("Campaign/{campaignId}/{id}", Name = "GetCampaignDocument")]
        [ResponseType(typeof(Document))]
        public async Task<IHttpActionResult> GetCampaignDocument(int campaignId, int id)
        {
            Document doc;
            var admin = false;
            if (admin)
            {
                var map = await db.CampaignDocumentMaps.FindAsync(id);
                if (map == null) return NotFound();
                doc=map.Document;
            }
            else {
                var user = GetThisUser();
                var camp = user.CreatedCampaigns.Where(c => c.Id == campaignId).FirstOrDefault();
                if (camp==null) return BadRequest("Invalid campaign or Unauthorized user");
                var map = camp.CampaignDocumentMaps.Where(m => m.DocumentId == id).FirstOrDefault();
                if (map == null) return NotFound();
                doc=map.Document;
            }
            return Ok(doc);
   
        }

        // POST (Update): api/Documents/5
        
        [ResponseType(typeof(void))]
        [HttpPost]
        public async Task<IHttpActionResult> PostDocument(int id, Document document)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != document.Id)
            {
                return BadRequest();
            }

            db.Entry(document).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DocumentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Documents
        [ResponseType(typeof(Document))]
        [Route("Campaign/{campaignId}")]
        public async Task<IHttpActionResult> PostNewCampaignDocument(int campaignId, Document_AddBM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var camp = await db.SafelyGetCampaignById(campaignId,true);
            if (camp.CreatedById.ToString() != User.Identity.GetUserId()) return Unauthorized();
            if (camp.Status.HasFlag(CampaignStatus.ReadOnly))
                Helpers.CustomHttpExceptions.CustomHttpException(HttpStatusCode.Forbidden, "Campaign is ReadOnly");
            if (model.Base64Image != null)
            {
                var doc = await UploadDocument(model);
               
                var map = new CampaignDocumentMap
                {
                    CampaignId = campaignId,
                    Document = doc
                };
                db.CampaignDocumentMaps.Add(map);
                await db.SaveChangesAsync();

                return CreatedAtRoute("GetCampaignDocument", new { campaignId = campaignId, id = map.DocumentId }, map.Document);
            }

            else {
                 return BadRequest("No base64 image");
            }

        }

        // POST: api/Documents/User/ThisUser
        /// <summary>
        /// NOTE: Only a user, theriselves can post a document, so we do not have two POST methods for User, ThisUser!
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ResponseType(typeof(Document))]
        [Route("User/ThisUser")]
        public async Task<IHttpActionResult> PostNewUserDocument(Document_AddBM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = int.Parse(User.Identity.GetUserId());

            if (model.Base64Image != null)
            {
                var doc = await UploadDocument(model);
            
                var map = new UserDocumentMap
                {
                    UserId = userId,
                    Document = doc
                };
                db.UserDocumentMaps.Add(map);
                await db.SaveChangesAsync();

                return CreatedAtRoute("GetUserDocument", new { userId=userId, id = map.DocumentId }, map.Document);
            }

            else {
                return BadRequest("No base64 image");
            }
        }

        // POST (Update): api/Documents/User/ThisUser
        /// <summary>
        /// NOTE: Only a user, theriselves can update (remove) a document, so we do not have two POST methods for User, ThisUser!
        /// currently we do not update documents, we only can remove them
        /// </summary>
        [ResponseType(typeof(Document))]
        [Route("User/ThisUser/{id}")]
        public async Task<IHttpActionResult> PostUserDocument(int id, bool soft_delete=false)
        {
         /*   if (!ModelState.IsValid) //add when you added an update model
            {
                return BadRequest(ModelState);
            }*/

            var userId = int.Parse(User.Identity.GetUserId());

            var user = db.Users.Find(userId);
            var map = user.UserDocumentMaps.Where(m => m.DocumentId == id).FirstOrDefault();
            if (map == null) return NotFound();

            map.Document.RemovedFlagUtc = DateTime.UtcNow;
            db.Entry(map).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent); ;

        }

        // GET: api/Documents/User/ThisUser
        [Route("User/ThisUser")]
        [ResponseType(typeof(IQueryable<Document>))]
        public async Task<IHttpActionResult> GetThisUserDocuments()
        {
            var userId = int.Parse(User.Identity.GetUserId());
            return await GetUserDocuments(userId);
        }

        // GET: api/Documents/User/3
        [Route("User/{userId}")]
        [ResponseType(typeof(IQueryable<object>))]
        public async Task<IHttpActionResult> GetUserDocuments (int userId)
        {
            var user = db.Users.Find(userId);
            if (user == null) return BadRequest("Invalid userId");
            var docs = user.UserDocumentMaps.Select(m => m.Document);
            var auth = new AuthorizationManager();
            var haveAccess =
                auth.CheckAccess(new AuthorizationContext(ClaimsPrincipal.Current, AuthResource.UserDocuments, AuthAction.List));
            if (haveAccess)
            {
                return Ok(await GetDocumentsWithDownloadLinks(docs));
            }
            else {
                if (User.Identity.GetUserId() != userId.ToString()) return Unauthorized();
                return Ok(docs);
            }
           
        }

        // GET: api/Documents/User/ThisUser/5
        [Route("User/ThisUser/{id}")]
        [ResponseType(typeof(Document))]
        public IHttpActionResult GetThisUserDocument(int id)
        {
            var userId = int.Parse(User.Identity.GetUserId());
            return  GetUserDocument(userId, id);
        }

        /*NOTE: Name of Route is only used to create links, e.g, for CreatedAtRoute. It has nothing to do with the Method name,
        though often they are identical*/
        // GET: api/Documents/User/3/5
        [Route("User/{userId}/{id}", Name = "GetUserDocument")]
        [ResponseType(typeof(object))]
        public IHttpActionResult GetUserDocument(int userId, int id)
        {
            var user = db.Users.Find(userId);
            if (user == null) return BadRequest("Invalid userId");
            if (userId.ToString() != User.Identity.GetUserId()) return Unauthorized();
            var map = user.UserDocumentMaps.Where(m => m.DocumentId == id).FirstOrDefault();
            if (map == null) return NotFound();

           
                return Ok(map.Document);
        }


        //TODO Do encrypt decrypt stream for byte chunks, a better memory wise approach:https://stackoverflow.com/questions/5596747/download-stream-file-from-url-asp-net
        [Route("Download/{id}")]
        [ResponseType(typeof(object))]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> GetDownloadDocument(int id, string token)
        {
            HttpResponseMessage response = Request.CreateResponse();
            var doc = await db.Documents.FindAsync(id);
            if (doc == null) return new HttpResponseMessage(HttpStatusCode.BadRequest);
            
                //Create a WebRequest to get the file
                HttpWebRequest fileReq = (HttpWebRequest)HttpWebRequest.Create(
                    Helpers.FileServerTokenManager.GetFullPath(doc.FileServer,doc.FilePath)
                    );
               
                //Create a response for this request
                HttpWebResponse fileResp =  (HttpWebResponse) await fileReq.GetResponseAsync();

            var length = (int)fileResp.ContentLength;
            
            /*TODO: Currently we load all the response content into buffer (server memory).
            In future, We may do this only partially for a bytes block and stream continously to the response*/
            byte[] buffer = new byte[length];
           
            using (System.IO.Stream stream = fileResp.GetResponseStream())
            {
                int remaining = length;
                int pos = 0;
                while (remaining != 0)
                {
                    int add = await stream.ReadAsync(buffer, pos, remaining);
                    pos += add;
                    remaining -= add;
                }
            }
         
             var outbuffer = Helpers.EncryptionService.DecryptBytes(buffer, doc.BF_Idx);

             var outStream = new System.IO.MemoryStream(outbuffer);
            response.Content = new StreamContent(outStream);
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("render");
            response.Content.Headers.ContentDisposition.FileName = string.Format(
                "doc_{0}_{1}.jpg",
                doc.Id,
                System.IO.Path.GetFileNameWithoutExtension(doc.FilePath)
                );
            response.Content.Headers.ContentLength = outbuffer.Count();
            return response;
        }

       
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool DocumentExists(int id)
        {
            return db.Documents.Count(e => e.Id == id) > 0;
        }

        private ApplicationUser GetThisUser()
        {
            return db.Users.Find(int.Parse(User.Identity.GetUserId()));
        }

        private async Task<Document> UploadDocument(Document_AddBM model)
        {
            Console.WriteLine("Doc Image Upload Started.");

            var uploaderResponse = await Helpers.UploadHelper.UploadBase64ImageAsync(db, User.Identity.GetUserId(), model.Base64Image, FileServerTokenType.DocumentUpload, true);

            if (uploaderResponse.StatusCode == HttpStatusCode.OK || uploaderResponse.StatusCode == HttpStatusCode.Created)
            {
                Console.WriteLine("Doc Image Uploaded, Path:" + uploaderResponse.FilePath);
                return
                    new Document
                    {
                        FileServerId = uploaderResponse.FileServerId,
                        FilePath = uploaderResponse.FilePath,
                        BF_Idx = (int)uploaderResponse.BF_Idx,
                        Description = model.Description
                    };
            }
            else
            {
                var err = string.Format("Doc Image Upload Error, Code:{0}, Message:{1}", uploaderResponse.StatusCode, uploaderResponse.Message);
                Helpers.CustomHttpExceptions.CustomHttpException(HttpStatusCode.InternalServerError, err);
                return null;
            }
        }

        private async Task<IList<object>> GetDocumentsWithDownloadLinks(IEnumerable<Document> docs)
        {
            var docsWithDl = new List<object>();
            foreach (var doc in docs)
            {
                var token = Helpers.FileServerTokenManager.GenerateRandomToken();
                var hash = Helpers.FileServerTokenManager.GenerateSaltedHash(token);
                db.FileServerTokens.Add(new FileServerToken
                {
                    FileTokenType = FileServerTokenType.DocumentDownload,
                    TokenExpDateUtc = DateTime.UtcNow.Add(Helpers.FileServerTokenManager.TokenTimeSpan),
                    TokenHash = hash,
                    Resource = "Documents",
                    EntryId = doc.Id
                });

                docsWithDl.Add(new
                {
                    Id = doc.Id,
                    Description=doc.Description,
                    ImagePath = Url.Content("~/api/Documents/Download/"+doc.Id+"?token="+ System.Web.HttpUtility.UrlEncode(token))
                });
            }
            await db.SaveChangesAsync();
            return docsWithDl;
        }
    }
}