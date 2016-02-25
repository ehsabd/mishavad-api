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
using System.Web.Http.OData;
using System.Web.Http.OData.Query;
using System.Web.Http.OData.Extensions;
using System.Security.Claims;
using System.Dynamic;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Mishavad_API.Helpers;
namespace Mishavad_API.Controllers
{
   
    [Authorize]
    [RoutePrefix("api/Campaigns")]
    public class CampaignsController : ApiController
    {
        private Ganss.XSS.HtmlSanitizer sanitizer = new Ganss.XSS.HtmlSanitizer();
            
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Campaigns
        /// <summary>
        /// Gets the list of campaigns
        /// </summary>

        /* [RequireHttps]*/ //TODO: add https to all
                            //TODO: Restrict Odata through code is only available for Admin right now for this controller
                            /*
                            NOTE: If you use orderby in your odata queries be aware that this may cause performance hit check this link:
                                http://stackoverflow.com/questions/36228379/bad-odata-performance-with-entity-framework-and-top-and-orderby
                            */
        [AllowAnonymous]
        [Route("")]
        [ResponseType(typeof(PageResult<object>))]
        public IHttpActionResult GetCampaigns(ODataQueryOptions<Campaign> options)
        {
            IQueryable<object> results;
            ODataQuerySettings settings = new ODataQuerySettings();
           
            IQueryable<Campaign> campResults;
            var auth = new AuthorizationManager();
            var haveAccess =
                auth.CheckAccess(new AuthorizationContext(ClaimsPrincipal.Current, AuthResource.Campaigns, AuthAction.List));
              
            /*NOTE:
            Admin access should be different from home page access to campaigns
            Only approved campaigns should come in home page, but Admin should have access to all campaigns
            NOTE II: Access to removed campaign is open to question for admin!*/
            if (haveAccess) //Only For Admin
            {
                Console.WriteLine( options.Filter.RawValue);
                settings.PageSize = 20;
                var adminquery = db.Campaigns;
                campResults = options.ApplyTo(adminquery, settings) as IQueryable<Campaign>;
                           
                results = campResults.Select(c => new
                {
                    //TODO: Add other needed properties for Admin
                    Id = c.Id,
                    Title = c.Title,
                    TargetFund = c.TargetFund,
                    CollectedFund = c.CollectedFund,
                    CategoryId = c.CampaignCategoryId,
                    ProjectStageId = c.ProjectStageId,
                    Story = c.Story,
                    Tagline = c.Tagline,
                    Thumbnail = c.ThumbnailFullPath,
                    Category = c.Category==null ? null : c.Category.Name,
                    ProjectStage = c.ProjectStage == null ? null : c.ProjectStage.Name
                });
                Console.WriteLine(results.ToString());
              
                
            }   
            else {
                //TODO: Cache the first page. It's the same for all! and it is loaded more!
                var query = db.Campaigns.Where(c=>
                            (c.TargetFund ?? -1) > 0 &&
                             c.Status.HasFlag(CampaignStatus.Approved) //It could be Running/Fulfilled/Unfulfilled
                             );
                            

                //Restrict Options Here!
                System.Diagnostics.Debug.WriteLine(query.ToString());

                settings.PageSize = 4;
                results = options.ApplyTo(query, settings) as IQueryable<Campaign>;
                results.Load(); //TODO: improve model and remove this to prevent multiple lazy loadings
            }
            return Ok(new PageResult<object>(
               results as IEnumerable<object>,
               Request.ODataProperties().NextLink,
               Request.ODataProperties().TotalCount));
        }

        [Route("", Name = "GetUserCampaigns")]
        public IHttpActionResult GetUserCampaigns(bool user_campaigns)
        {
            var user_id = int.Parse(User.Identity.GetUserId());
            var userCampaigns = db.Campaigns.Where(c => c.CreatedById == user_id)
                .Include(c => c.Category)
                .Include(c => c.ProjectStage)
                .Include(c => c.ThumbnailFileServer);
            // CustomInternalServerError("",userCampaigns.ToString());
            return Ok(userCampaigns);
        }

        //TODO: Make GET compatibale with the UPDATE POST
        /*NOTE: We make use of CampaignVM because we need more details like city and province 
        when we get individual campaign data. DO NOT remove this implementation */

        // GET: api/Campaigns/5
        [AllowAnonymous]
        [ResponseType(typeof(object))]
        [Route("{id_or_slug?}", Name = "GetCampaign")]
        public IHttpActionResult GetCampaign(string id_or_slug)
        {
            
            Campaign c = GetCampaignByIdOrSlug(id_or_slug);
           

            if (c.Status.HasFlag(CampaignStatus.Approved)) {
                var vm = CampaignToCampaignVM(c);
                return Ok(vm);
            }
            else if (User.Identity.IsAuthenticated) { 

              if (User.Identity.GetUserId() != c.CreatedById.ToString())
                {
                    var auth = new AuthorizationManager();
                    var haveAccess =
                        auth.CheckAccess(new AuthorizationContext(ClaimsPrincipal.Current, AuthResource.Campaigns, AuthAction.GetFullDetails));
                    if (!haveAccess) return Unauthorized();
                }

                var vm = CampaignToCampaignVM(c);
                //Add full details properties
                vm.CreatedById = c.CreatedById;
                return Ok(vm);


            } 
            else{
                return Unauthorized();
            }

                
        }


        // POST (Update): api/Campaigns/5
        /*
        Do we restrict campaign adding/editing to eligible users or not?
         !TODO!: #peformance, check performance hit for sanitizing model when doing partial update.
         !TODO!: Check performance of RemoveRange-AddRange combination for camp tags
          DONE, NOTE: This does not affect performance because very few campaigns are added/edited 
                but you should take care of malicous behaviors
        */
        [ResponseType(typeof(void))]
        [Route("{id_or_slug?}", Name = "PostCampaign")]
        public async Task<IHttpActionResult> PostCampaign(string id_or_slug, Campaign_UpdateBM model, bool soft_delete =false)
        {

            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var campaign = GetCampaignByIdOrSlug(id_or_slug);
            

            //Only the one who created the campaign can edit it
            //TODO: What about admins?
            var userId = User.Identity.GetUserId();
            var user = db.Users.Find(int.Parse(userId));
            
            if (campaign.CreatedById.ToString() != userId)
            {
                CustomHttpExceptions.CustomHttpException(HttpStatusCode.Unauthorized,
                    string.Format(
                    "Unauthorized: The user (Id = {0}) who has requested the update is not the creator of the campaign!",
                    userId) 
                );
            }

            /*TODO: think about these conditions and code business logic accordingly:
            1) The user decides to cancel campaign in 'Waiting' status
            2) The user decides to interrupt 'Waiting' status and do some changes
            3) The user decides to remove an 'Approved' or 'Waiting' campaign
            */
            if (campaign.Status.HasFlag(CampaignStatus.ReadOnly)) 
            {
                CustomHttpExceptions.CustomHttpException(HttpStatusCode.Forbidden,"Campaign can not be modified because of its current status");
            }

            if (soft_delete)
            {
                campaign.RemovedFlagUtc = DateTime.UtcNow;
                db.Entry(campaign).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return StatusCode(HttpStatusCode.NoContent);
            }

            UpdateCampaignByUpdateCampaignVM(ref campaign, model);

            AddOrUpdateSlug(ref campaign);

            //Checks whether there is a base64 thumbnail 
            if (model.Base64Thumbnail != null)
            {
                var uploaderResponse = await Helpers.UploadHelper.UploadBase64ImageAsync(db, userId,model.Base64Thumbnail,FileServerTokenType.CampaignImageUpload);

                if (uploaderResponse.StatusCode == HttpStatusCode.OK || uploaderResponse.StatusCode == HttpStatusCode.Created)
                {
                    model.ThumbnailPath = uploaderResponse.FilePath;
                    model.ThumbnailServerId = uploaderResponse.FileServerId;
                    campaign.ThumbnailFileServerId = model.ThumbnailServerId;
                    campaign.ThumbnailFilePath = model.ThumbnailPath;
                    Console.WriteLine("Thumbnail Uploaded. Thumbnail Path:" + campaign.ThumbnailFilePath) ;
                }

                else {
                    Console.WriteLine("Thumbnail Upload Error Code:" + uploaderResponse.StatusCode);
                    Console.WriteLine(uploaderResponse.Message );
                }

            }
            
          
            if (model.CityId != null)
            {
                if (campaign.Location!=null)
                {
                    var location = campaign.Location;
                    location.CityId = (int)model.CityId;
                    db.Entry(location).State = EntityState.Modified;
                }
                else
                {
                    campaign.Location = new Location { CityId = (int)model.CityId };
                }

            }

           
            var waitingStatus = CheckandUpdateWaitingStatus(campaign, model.Status);
            if (waitingStatus)
            {
                campaign.Status = CampaignStatus.Waiting | CampaignStatus.ReadOnly;
                if (campaign.Account == null)
                {
                    campaign.Account = new Account { AccountName = "cmp_" + campaign.Id.ToString(), AccountType = AccountType.CampaignAccount };
                }
            }

            if (model.Tags != null)
            {
                AddTags( model.Tags, campaign);
            }

            db.Entry(campaign).State = EntityState.Modified;
       
            await db.SaveChangesAsync();

            return  StatusCode(HttpStatusCode.NoContent);
           
        }

       
        // POST (New): api/Campaigns
        [ResponseType(typeof(Campaign))]
        [HttpPost]
        [Route("", Name = "PostNewCampaign")]
        public async Task<IHttpActionResult> PostNewCampaign([FromBody] Campaign_CreateBM model)
        {
            if (db.Campaigns.Where(c => c.Title == model.Title).Count() > 0)
            {
                return Conflict();
            }

            var thisUserId = int.Parse(User.Identity.GetUserId());

            if (db.Campaigns.Where(c => c.CreatedById == thisUserId &&
                                        (c.Status == CampaignStatus.PreliminaryRegistered
                                        || c.Status == CampaignStatus.CompletelyRegistered
                                        || c.Status == CampaignStatus.Waiting)).Count() >= 2) {
                CustomHttpExceptions.CustomHttpException(HttpStatusCode.Conflict,"The user cannot create a campaign because they already have maximum two 'Not-Accepted' campaigns");
            }

            var todayUtc = DateTime.UtcNow.Date;
            if (
                db.Campaigns.Where(c => c.CreatedById == thisUserId && c.CreatedDateUtc>=todayUtc).Count() 
                >= ApplicationDbContext.GlobalSettings.SecurityDoSMaxCampaignsPerUserPerDay
                )
            {
                CustomHttpExceptions.CustomHttpException(HttpStatusCode.Conflict, "The user must wait up to one day to create a new campaign");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //NOTE: We use Ganss sanitizer for HTML (perhaps only Story) and our own MySanitizer for the rest
            model.Title = Helpers.MySanitizer.StrictSanitize(model.Title);
            model.Tagline = Helpers.MySanitizer.StrictSanitize(model.Tagline);

            var campaign = new Campaign
            {
                Status = CampaignStatus.PreliminaryRegistered,
                CreatedById = thisUserId,
                TargetFund = model.TargetFund,
                Title = model.Title,
                Tagline = model.Tagline
            };

            AddOrUpdateSlug(ref campaign);

            db.Campaigns.Add(campaign);
            await db.SaveChangesAsync();

           // return CreatedAtRoute("DefaultApi", new { id = campaign.Id }, campaign);
            return Created<Campaign>("DefaultApi", campaign);
        }


        
     
           
        // POST : api/Campaigns/5/EasyPay
        [ResponseType(typeof(int))]
        [HttpPost]
        [Route("{id_or_slug?}/EasyPay", Name = "EasyPayCampaign")]
        public IHttpActionResult EasyPayCampaign(string id_or_slug ,[FromUri] int amount, [FromUri] string creditType)
        {
            Campaign c = GetCampaignByIdOrSlug(id_or_slug);
                        
            var userId = User.Identity.GetUserId();
            var user = db.Users.Find(userId);
            if (user.AccountBalance >= amount)
            {
                //TODO: Add EasyPay Code here.
            }
            
            return Ok();
        }
       

        public CampaignVM CampaignToCampaignVM(Campaign c) {
            var vm=  new CampaignVM
            {
                Id = c.Id,
                Status=c.Status,
                CategoryId = c.CampaignCategoryId,
                Category = c.Category == null ? null : c.Category.Name,
                TotalDays = c.TotalDays,
                Tags = c.TagMaps.Select(tm => tm.CampaignTagName).ToList(),
                CityId = (c.Location == null) ? (int?)null : c.Location.CityId,
                City = (c.Location == null || c.Location.City == null) ? null : c.Location.City.Name,
                Province = (c.Location == null || c.Location.City == null) ? null : c.Location.City.ProvinceName,
                ProjectStageId = c.ProjectStageId,
                ProjectStage = c.ProjectStage == null ? null : c.ProjectStage.Name,
                Tagline = c.Tagline,
                TargetFund = c.TargetFund,
                Thumbnail = c.ThumbnailFullPath,
                Title = c.Title,
                VerificationDescription = c.VerificationDescription,
                VerifiedByOrg = c.VerifiedByOrg,
                NBacked = c.NBacked,
                CollectedFund = c.CollectedFund,
                CollectedFundPercent = c.CollectedFundPercent,
                TotalSecondsLeft = c.TotalSecondsLeft
            };

            try
            {
                vm.StoryElements = JsonConvert.DeserializeObject<string[]>(c.Story);
            }
            catch
            {
                vm.StoryElements = new string[] { c.Story ?? "" };
            }

            return vm;
        }

        public bool CheckandUpdateWaitingStatus(Campaign campaign, string status)
        {
            if (string.Equals(status,
                CampaignStatus.Waiting.ToString(),
                StringComparison.OrdinalIgnoreCase))
            {
                /*
                NOTE: This is not neccessary and in some cases against RESTful implementation
                although not practical from client-side point of view there are no problems with changing
                some campaign props and request a waiting status at the same request!
                //counts non-null values of model
                var nonNullCount = typeof(UpdateCampaignVM).GetProperties()
                    .Where(p => p.GetValue(model) != null)
                    .Count();
                if (nonNullCount > 1) {
                    CustomBadRequest(string.Format(
                        "Model can not have Status value and other values at the same time: {0} non-null values in model"
                        ,nonNullCount));
                }
            */

                ModelState.Clear();

                var validationProps = typeof(Campaign_WaitingValidationModel).GetProperties().Select(p => p.Name).ToList();
                //Check Null or Empty values of campaign
                var nullOrWhiteSpaceProps =
                    typeof(Campaign).GetProperties()
                    .Where(p => validationProps.Contains(p.Name))
                    .Where(p => string.IsNullOrWhiteSpace((p.GetValue(campaign) ?? "").ToString()))
                    .Select(p => p.Name);

                if (nullOrWhiteSpaceProps.Count() > 0)
                {
                    CustomHttpExceptions.CustomBadRequest("The following properties of campaign have Null or Empty values:"
                        + string.Join(",", nullOrWhiteSpaceProps.ToArray()));
                }
                return true;
            }
            return false;
        }

        public void UpdateCampaignByUpdateCampaignVM(ref Campaign campaign, Campaign_UpdateBM model) {
            if (model.Story != null)
            {
                try
                {
                    var storyElements = JsonConvert.DeserializeObject<string[]>(model.Story);
                    campaign.Story = JsonConvert.SerializeObject(
                        storyElements.Select(elm => sanitizer.Sanitize(elm))
                        );
                }
                catch
                {
                    throw new Exception("Error in sanitizing story elements. Story might not be a deserializable Json string");
                }
            }
            
            if (model.Title != null)
                campaign.Title = Helpers.MySanitizer.StrictSanitize(model.Title);
            if (model.Tagline != null)
                campaign.Tagline = Helpers.MySanitizer.StrictSanitize(model.Tagline);
            if (model.CategoryId != null)
                campaign.CampaignCategoryId = model.CategoryId;
            if (model.TotalDays != null)
                campaign.TotalDays = model.TotalDays;
            if (model.ProjectStageId != null)
                campaign.ProjectStageId = model.ProjectStageId;
            if (model.VerifiedByOrg != null)
                campaign.VerifiedByOrg = model.VerifiedByOrg;
            if (model.VerificationDescription != null)
                campaign.VerificationDescription = model.VerificationDescription;
            if (model.TargetFund != null)
                campaign.TargetFund = model.TargetFund;
            
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

       
        /*replaced by checking null!
        private bool LocationExists(int id)
        {
            return db.Locations.Count(e => e.Id == id) > 0;
        }*/

        private bool TagExist(CampaignTag campaignTag)
        {
            return db.CampaignTags.Count(t => t.Name == campaignTag.Name) > 0;
        }

        private void AddTags(string[] tags, Campaign campaign) {
            var lengthyTags = tags.Where(t => t.Length > 20);
            if (lengthyTags.Count() > 0)
            {
                CustomHttpExceptions.CustomBadRequest("Some tags are lengthy:" + string.Join(",", lengthyTags));
            }
            //Sanitize tags
            tags = tags.Select(t => Helpers.MySanitizer.StrictSanitize(t)).ToArray();
            /*NOTE:AddRange is a no-op if the entity is already in the context in the Added state, but the tags added
            previously are in Unchanged state so we have to add only new entities*/
            var oldTags = campaign.TagMaps.ToList();
            var newTags = tags.Select(t => new CampaignTag { Name = t }).ToList();
            //Remove only old tags that are not in new tags
            foreach (var t in oldTags)
            {
                if (!tags.Contains(t.CampaignTagName))
                {
                    db.CampaignTagMaps.Remove(t);
                }
            }

            //add only new tags that are not in old tags
            var oldTagNames = oldTags.Select(ot => ot.CampaignTagName).ToArray();
            foreach (var nt in newTags)
            {
                if (!TagExist(nt))
                {
                    db.CampaignTags.Add(nt);
                }
                if (!oldTagNames.Contains(nt.Name))
                {
                    db.CampaignTagMaps.Add(new CampaignTagMap { CampaignTagName = nt.Name, CampaignId = campaign.Id });
                }
            }
        }
       
      
        /// <summary>
        /// Returns a non-null campaign based on Id or Slug. If there was no campaign with 
        /// the Id/slud it throws a Not Found web exception
        /// </summary>
        /// <param name="id_or_slug"></param>
        /// <returns></returns>
        private Campaign GetCampaignByIdOrSlug(string id_or_slug) {
            Campaign camp;
            int id;
            if (int.TryParse(id_or_slug, out id)){
                camp = (db.Campaigns.Find(id));
            }
            else {
                var query = db.Campaigns.Where(c => c.Slug == id_or_slug).Select(c=>c);
                camp = (query.Count() == 1) ? query.First() : null; 
            }

            if (camp == null) {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    ReasonPhrase = "Not Found - Wrong Id or Slug"
                });
            }
            return camp;
        }

        private void AddOrUpdateSlug(ref Campaign camp)
        {
            var slug = Helpers.SlugHelper.GenerateSlug(camp.Title);
            var campId = camp.Id;
            if (db.Campaigns.Where(c => (c.Slug == slug && c.Id!=campId)).Count() > 0)
            {
                Helpers.CustomHttpExceptions.CustomHttpException(HttpStatusCode.Conflict, "SlugCollision");
            }
            camp.Slug = slug;

        }

    }
}