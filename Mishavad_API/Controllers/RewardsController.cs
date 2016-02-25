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
using Mishavad_API.Models;

using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
namespace Mishavad_API.Controllers
{
   
    [RoutePrefix("api/Campaigns/{campaignId}/Rewards")]
    public class RewardsController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET (Query)
        [Route("", Name = "GetRewards")]
        public IQueryable<object> GetRewards(int? campaignId)
        {
            var Rewards =
            db.Rewards
            .Where(r => ((r.CampaignId == campaignId)))
            .Include(r=>r.Campaign)
            .Include(r=>r.ImageFileServer);
            
            return Rewards;
        }

        // GET
        [Route("{id?}", Name = "GetReward")]
        [ResponseType(typeof(Reward))]
        public IHttpActionResult GetReward(int? campaignId, int id)
        {
            Reward Reward = db.Rewards.Find(id);
            if (Reward == null)
            {
                return NotFound();
            }

            return Ok(Reward);
        }


        // POST
        [Authorize]
        [Route("", Name = "PostNewReward")]
        [ResponseType(typeof(Reward))]
        public async Task<IHttpActionResult> PostNewReward(int campaignId, Reward_AddBM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
          
            
            var campaign = await db.SafelyGetCampaignById(campaignId,true);
           
            //Only the one who created the campaign can add images
            //TODO: Add this if you encountered an error db.Entry(Reward).Reference(p => p.Campaign).Load();
            //TODO what about admins?
            if (campaign.CreatedById.ToString() != User.Identity.GetUserId())
            {
                return Unauthorized();
            }

            var reward = new Reward { CampaignId = (int)campaignId
            ,AddressRequired=model.AddressRequired,
            Amount=model.Amount,
            DeliveryDays=model.DeliveryDays,
            NAvailable=model.NAvailable,
            Title=Helpers.MySanitizer.StrictSanitize(model.Title),
            Description=Helpers.MySanitizer.StrictSanitize(model.Description)};

            if (!string.IsNullOrEmpty(model.Base64Image))
                {
                    var uploaderResponse = await Helpers.UploadHelper.UploadBase64ImageAsync(db, User.Identity.GetUserId(), model.Base64Image, FileServerTokenType.RewardImageUpload);
                    if (uploaderResponse.StatusCode == HttpStatusCode.OK || uploaderResponse.StatusCode == HttpStatusCode.Created)
                    {
                        reward.ImageFilePath = uploaderResponse.FilePath;
                        reward.ImageFileServerId = uploaderResponse.FileServerId;
                    }
                    else {
                        return InternalServerError(new Exception(uploaderResponse.Message));
                    }
                }
            
            db.Rewards.Add(reward);
            await db.SaveChangesAsync();
            return Created("PostNewReward",  reward);
        }
        
        // POST
        [ResponseType(typeof(void))]
        [Authorize]
        [Route("{id?}", Name = "PostReward")]
        public async Task<IHttpActionResult> PostReward(int campaignId, int id, Reward_UpdateBM model, bool soft_delete = false)
        {           
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (model==null)
            {
                return BadRequest();
            }
            var reward = db.Rewards.Find(id);

            if (reward == null)
                return NotFound();

            if (reward.CampaignId != campaignId)
                return BadRequest("CampaignId mismatch");

            var campaign = await db.SafelyGetCampaignById(campaignId,true);
           
            //Only the one who created the campaign can edit rewards
            //TODO what about admins?
            if (campaign.CreatedById.ToString() != User.Identity.GetUserId())
            {
                return new System.Web.Http.Results.ResponseMessageResult(
                    Request.CreateResponse(HttpStatusCode.Unauthorized,
                    "The authorized user is not identical with the user who created the campaign"));   
            }

            if (soft_delete)
            {
                reward.RemovedFlagUtc = DateTime.UtcNow;
                db.Entry(reward).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return StatusCode(HttpStatusCode.NoContent);
            }
            /*NOTE: Always check the number of properties you are updating with that of binding model and go through them
            alphabetically
            Reward_UpdateBM has 7 props
            */

            if (model.AddressRequired != null)
                reward.AddressRequired = (bool)model.AddressRequired;
            if (model.Amount != null)
                reward.Amount = (int)model.Amount;
            reward.DeliveryDays = model.DeliveryDays; // Nullable
            if (model.Description != null)
                reward.Description = Helpers.MySanitizer.StrictSanitize(model.Description);
            if (model.NAvailable != null)
                reward.NAvailable = (int)model.NAvailable;
            if (model.Title != null)
                reward.Title = Helpers.MySanitizer.StrictSanitize(model.Title);
            
            
            


            if (!string.IsNullOrEmpty(model.Base64Image))
            {
                var uploaderResponse = await Helpers.UploadHelper.UploadBase64ImageAsync(db, User.Identity.GetUserId(), model.Base64Image, FileServerTokenType.RewardImageUpload);
                if (uploaderResponse.StatusCode == HttpStatusCode.OK || uploaderResponse.StatusCode == HttpStatusCode.Created)
                {
                    reward.ImageFilePath = uploaderResponse.FilePath;
                    reward.ImageFileServerId = uploaderResponse.FileServerId;
                }
                else {
                    return InternalServerError(new Exception(uploaderResponse.Message));
                }
            }
            
            db.Entry(reward).State = EntityState.Modified;

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

        private bool RewardExists(int id)
        {
            return db.Rewards.Count(e => e.Id == id) > 0;
        }
    }
}