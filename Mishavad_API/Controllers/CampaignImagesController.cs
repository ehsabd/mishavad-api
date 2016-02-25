using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

using System.Threading.Tasks;
using Mishavad_API.Models;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
namespace Mishavad_API.Controllers
{
   
    [RoutePrefix("api/Campaigns/{campaignId}/Images")]
    public class CampaignImagesController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET (Query)
        [Route("", Name = "GetCampaignImages")]
        public IQueryable<object> GetCampaignImages(int campaignId)
        {
            var campaignImages =
            db.CampaignImages
            .Where(i => (i.CampaignId == campaignId))
            .Select(i => new
                {
                    Id=i.Id,
                    CampaignId = i.CampaignId,
                    Description=i.Description,
                    FilePath=i.FullPath,
                    ShownInGallery=i.ShownInGallery
                
            });

            return campaignImages;
        }

        // GET
        [Route("{id?}", Name = "GetCampaignImage")]
        [ResponseType(typeof(CampaignImage))]
        public IHttpActionResult GetCampaignImage(int campaignId, int id)
        {
            CampaignImage campaignImage = db.CampaignImages.Find(id);
            if (campaignImage == null)
            {
                return NotFound();
            }

            return Ok(campaignImage);
        }

     

        // POST
        [Authorize]
        [Route("", Name = "PostNewCampaignImage")]
        [ResponseType(typeof(CampaignImage))]
        public async Task<IHttpActionResult> PostNewCampaignImage(int campaignId, CampaignImage_AddBM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
          

           
            var campaign = await db.SafelyGetCampaignById(campaignId,true);
            
            //Only the one who created the campaign can add images
            //TODO: Add this if you encountered an error db.Entry(campaignImage).Reference(p => p.Campaign).Load();
            /*NOTE: Admins can not add images to a campaign, they can only remove it sometimes!*/
            var userId = User.Identity.GetUserId();
            

            if (campaign.CreatedById.ToString() != userId)
            {
                return Unauthorized();
            }
            var campaignImage = new CampaignImage
            {
                CampaignId = campaignId,
                Description = model.Description,
                ShownInGallery = model.ShownInGallery
            };
            
                if (string.IsNullOrEmpty(model.Base64Image)){
                    return BadRequest("No base64 image");
                }
                else {
                    var uploaderResponse = await Helpers.UploadHelper.UploadBase64ImageAsync(db, userId, model.Base64Image,FileServerTokenType.CampaignImageUpload);
                    if (uploaderResponse.StatusCode == HttpStatusCode.OK || uploaderResponse.StatusCode == HttpStatusCode.Created)
                    {
                        campaignImage.FilePath = uploaderResponse.FilePath;
                        campaignImage.FileServerId = uploaderResponse.FileServerId;
                    }
                    else {
                       return InternalServerError(new Exception(uploaderResponse.Message));
                    }
                }
                
            
            db.CampaignImages.Add(campaignImage);

           await db.SaveChangesAsync();

            if (model.AppendedToStory)
            {
                var storyElements = new List<string>();
                try {
                    storyElements= JsonConvert.DeserializeObject<List<string>>(campaign.Story);
                }
                catch (JsonReaderException){
                    //This ensures compatibility with story property that contained plain text of story
                    storyElements.Add(campaign.Story);
                }
                storyElements.Add(string.Format("[campaignImageId:{0}]", campaignImage.Id));
                campaign.Story = JsonConvert.SerializeObject(storyElements);
                db.Entry(campaign).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
 
            return CreatedAtRoute("PostNewCampaignImage", new { id = campaignImage.Id }, campaignImage);
        }

        // POST (Update)
        [ResponseType(typeof(void))]
        [Authorize]
        [Route("{id}", Name = "PostCampaignImage")]
        public async Task<IHttpActionResult> PostCampaignImage(int campaignId, int id, CampaignImage_UpdateBM model, bool soft_delete=false)
        {
          
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            if (model==null)
            {
                return BadRequest();
            }

            var campaignImage = await db.CampaignImages.FindAsync(id);
            if (campaignImage== null){
                return NotFound();
            }
            
           
            if (campaignId != campaignImage.CampaignId) {
                return BadRequest("campaign id mismatch");
            }
            var campaign = await db.SafelyGetCampaignById(campaignId,true);
           

            //Only the one who created the campaign can edit images
            //TODO: Add this if you encountered an error db.Entry(campaignImage).Reference(p => p.Campaign).Load();
            /*NOTE: Admins can not add/update images to a campaign, they can only remove it sometimes!*/
            var userId = User.Identity.GetUserId();
          

            if (campaign.CreatedById.ToString() != userId)
            {
                return new System.Web.Http.Results.ResponseMessageResult(
                    Request.CreateResponse(HttpStatusCode.Unauthorized,
                    "The user who wants to edit the images is not identical with user who created the campaign"));
            }

            if (soft_delete)
            {
                campaignImage.RemovedFlagUtc = DateTime.UtcNow;
                db.Entry(campaignImage).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return StatusCode(HttpStatusCode.NoContent);
            }


            campaignImage.Description = model.Description;
            campaignImage.ShownInGallery = model.ShownInGallery;
           
          
                if (string.IsNullOrEmpty(model.Base64Image))
                {
                    return BadRequest("No base64 image");
                }
                else {
                    var uploaderResponse = await Helpers.UploadHelper.UploadBase64ImageAsync(db, userId, model.Base64Image,FileServerTokenType.CampaignImageUpload);
                    if (uploaderResponse.StatusCode == HttpStatusCode.OK || uploaderResponse.StatusCode == HttpStatusCode.Created)
                    {
                        campaignImage.FilePath = uploaderResponse.FilePath;
                        campaignImage.FileServerId = uploaderResponse.FileServerId;
                    }
                    else {
                        return InternalServerError(new Exception(uploaderResponse.Message));
                    }
                }
            
            db.Entry(campaignImage).State = EntityState.Modified;

            await db.SaveChangesAsync();
            
            return StatusCode(HttpStatusCode.NoContent);
        }
       
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool CampaignImageExists(int id)
        {
            return db.CampaignImages.Count(e => e.Id == id) > 0;
        }
    }
}