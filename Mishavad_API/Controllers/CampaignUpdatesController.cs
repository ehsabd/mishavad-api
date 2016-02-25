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
    [ApiExplorerSettings(IgnoreApi = true)]
    public class CampaignUpdatesController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/CampaignUpdates
        public IQueryable<CampaignUpdate> GetCampaignUpdates()
        {
            return db.CampaignUpdates;
        }

        // GET: api/CampaignUpdates/5
        [ResponseType(typeof(CampaignUpdate))]
        public IHttpActionResult GetCampaignUpdate(int id)
        {
            CampaignUpdate campaignUpdate = db.CampaignUpdates.Find(id);
            if (campaignUpdate == null)
            {
                return NotFound();
            }

            return Ok(campaignUpdate);
        }

        // POST: api/CampaignUpdates/5
        [ResponseType(typeof(void))]
        [Authorize]
        public async Task<IHttpActionResult> PostCampaignUpdate(int id, CampaignUpdate campaignUpdate)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != campaignUpdate.Id)
            {
                return BadRequest();
            }

            //Only the one who created the campaign can edit the updates
            //TODO: What about admins?
            var campaign = campaignUpdate.Campaign;
            if (campaign.CreatedById.ToString() != User.Identity.GetUserId())
            {
                return Unauthorized();
            }
            else if (campaign.Status != CampaignStatus.Approved)
            {
                return BadRequest("Updates can only be edited for approved campaigns");
            }
            else
                if (campaignUpdate.Status == Models.UpdateStatus.Approved ||
                    campaignUpdate.Status == Models.UpdateStatus.Waiting)
            {
                return BadRequest("No update change for validated or waiting updates");
            }


            db.Entry(campaignUpdate).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CampaignUpdateExists(id))
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

        // POST: api/CampaignUpdates
        [ResponseType(typeof(CampaignUpdate))]
        [Authorize]
        public async Task<IHttpActionResult> PostCampaignUpdate(CampaignUpdate campaignUpdate)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Only the one who created the campaign can add the updates
            //TODO: What about admins?
            var campaign = campaignUpdate.Campaign;
            if (campaign.CreatedById.ToString() != User.Identity.GetUserId())
            {
                return Unauthorized();
            }
            else if (campaign.Status != CampaignStatus.Approved)
            {
                return BadRequest("Updates can only be added for approved campaigns");
            }
           

            db.CampaignUpdates.Add(campaignUpdate);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = campaignUpdate.Id }, campaignUpdate);
        }

   
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool CampaignUpdateExists(int id)
        {
            return db.CampaignUpdates.Count(e => e.Id == id) > 0;
        }
    }
}