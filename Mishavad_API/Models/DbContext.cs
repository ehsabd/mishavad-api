using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

using System.Web.Http;
using System.Net.Http;
using System.Net;
using System.Data.Entity;
using System.Threading.Tasks;
using EntityFramework.DynamicFilters;

namespace Mishavad_API.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, CustomRole,
    int, CustomUserLogin, CustomUserRole, CustomUserClaim>
    {
       
        public ApplicationDbContext()
            : base("DefaultConnection")
        {
            //Added for cryptography by @ehsabd
            ((System.Data.Entity.Infrastructure.IObjectContextAdapter)this).ObjectContext.ObjectMaterialized += 
                new System.Data.Entity.Core.Objects.ObjectMaterializedEventHandler(ObjectMaterialized);
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Filter("RemovedFlagUtc", (ISoftDelete d) => d.RemovedFlagUtc, null);
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<UserInfo> UserInfos { get; set; }
        public DbSet<UserDocumentMap> UserDocumentMaps { get; set; }

        // Campaign Models
        public DbSet<CampaignImage> CampaignImages { get; set; }
        public DbSet<CampaignCategory> CampaignCategories { get; set; }
        public DbSet<ProjectStage> ProjectStages { get; set; }
        public DbSet<CampaignTagMap> CampaignTagMaps { get; set; }
        public DbSet<CampaignDocumentMap> CampaignDocumentMaps { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Reward> Rewards { get; set; }
        
        public DbSet<Document> Documents { get; set; }
        public DbSet<Campaign> Campaigns { get; set; }
        
        //Financial Models
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<JournalEntry> JournalEntries { get; set; }
        public DbSet<DepositInvoice> DepositInvoices { get; set; }
        public DbSet<TransactionDepositInvoiceMap> TransactionDepositInvoiceMaps { get; set; }
        public DbSet<TransactionRewardMap> TransactionRewardMaps { get; set; }
        public DbSet<TransactionChargebackMap> TransactionChargebackMap { get; set; }

        //FileServer Models
        public DbSet<FileServerToken> FileServerTokens { get; set; }
        public DbSet<FileServer> FileServers { get; set; }
        //Misc Models
        public DbSet<City> Cities { get; set; }
        public DbSet<CampaignTag> CampaignTags { get; set; }
        //DualAuthModels
        public DbSet<Operation> Operations { get; set; }
        public DbSet<OperationAuth> OperationAuths { get; set; }
        // Blog Models
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<BlogPostCategory> BlogPostCategories { get; set; }
        // Visit Models
        public DbSet<VisitReferrer> VisitReferrers { get; set; }

        public DbSet<CampaignComment> CampaignComments { get; set; }

        public DbSet<CampaignUpdate> CampaignUpdates { get; set; }

        public DbSet<PublicSetting> PublicSettings { get; set; }

        public DbSet<PrivateSetting> PrivateSettings { get; set; }

        public class GlobalSettings
        {
            public static int SecurityDoSMaxCampaignsPerUserPerDay { get; set; }
            public static int CampaignMinFund { get; set; }
        }

        public async Task<Campaign> SafelyGetCampaignById(int id, bool CheckReadOnly = false)
        {
            var c = await Campaigns.FindAsync(id);
            if (c == null)
                Helpers.CustomHttpExceptions.CustomBadRequest("Invalid Campaign Id");
            if (CheckReadOnly && c.Status.HasFlag(CampaignStatus.ReadOnly))
                Helpers.CustomHttpExceptions.CustomHttpException(HttpStatusCode.Forbidden, "Campaign is ReadOnly");
            return c;
        }

        //Post this method and the next to code review!
        public override int SaveChanges()
        {
            /*NOTE: perhaps we could fix this just for Seed:
            http://stackoverflow.com/questions/3095696/how-do-i-get-the-calling-method-name-and-type-using-reflection
            */
            System.Diagnostics.StackFrame frame = new System.Diagnostics.StackFrame(1);
            var method = frame.GetMethod();
            var type = method.DeclaringType;
            var name = method.Name;
            if (name.Contains("Seed"))
            {
                return SaveChangesAsync().Result;
            }
            else {
                throw (new Exception("Use only SaveChangesAsync, Caller:"+name));
            }
        }

        public override async System.Threading.Tasks.Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken)
        {
            AddBF_IdxToNewEncryptedEntries(this);
            var pendingEntriesForEncryption = EncryptPendingEntries(this);
           
            //TODO: Get Campaigns Need CF Update By Changed Transactions (to Completed Status)
            //TODO: Get Rewards Need Update By Added ClaimedRewards 
         //   var campaignsNeedUpdate = GetCampaignsNeedCFUpdate();
          //  var rewardsNeedUpdate = GetRewardsNeedUpdate();
            /*
            NOTE: We should first save changes to ensure that gift funds are added for campaigns 
                  and then calculate CollectedFunds and NClaimeds
            */
            var result = await base.SaveChangesAsync(cancellationToken);
            //TODO: Shouldn't we check this result var?
            //After Changes Saved
          //  UpdateCollectedFunds(campaignsNeedUpdate);
        //    UpdateNClaimeds(rewardsNeedUpdate);
            //We need another save for updated collected funds and NClaimeds
            result = await base.SaveChangesAsync(cancellationToken);
            DecryptPendingEntries(pendingEntriesForEncryption);
            return result;

        /*  catch (System.Data.Entity.Validation.DbEntityValidationException e)
            //Catches validation error so that we can debug what goes wrong
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    System.Diagnostics.Debug.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        System.Diagnostics.Debug.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }*/
    }


    void ObjectMaterialized(object sender, System.Data.Entity.Core.Objects.ObjectMaterializedEventArgs e)
        {
            if (IsEncrypted(e.Entity)) 
                DecryptEntity(e.Entity);
        }

        #region EncryptionMethods

        private void AddBF_IdxToNewEncryptedEntries(ApplicationDbContext ctx) {

            var contextAdapter = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)ctx);
            contextAdapter.ObjectContext.DetectChanges(); //force this. Sometimes entity state needs a handle jiggle

             //Get all pending entities that are not relationships and have Encrypted attribute
            var query = contextAdapter.ObjectContext.ObjectStateManager
                .GetObjectStateEntries(System.Data.Entity.EntityState.Added)
                .Where(en => !en.IsRelationship && IsEncrypted(en)
                );           

            foreach (var entry in query.ToList())
            {
                 var BF_Idx_Attr = GetEntityType(entry).GetProperties().SelectMany(p=>p.GetCustomAttributes(typeof(BF_Idx), true)).First() as BF_Idx;
                 if (BF_Idx_Attr.ContextGenerated == ContextGeneratedOption.Random)
                 {
                     SetBF_IdxProp(entry.Entity, Helpers.EncryptionService.NewBF_Idx());
                 }
            }
        }

        private List<System.Data.Entity.Core.Objects.ObjectStateEntry>
                EncryptPendingEntries(ApplicationDbContext ctx)
        {
            /*NOTE We do not have to add Html Encoding for XSS prevention with the relevant attributes because this should be done in 
            controllers*/

            var pendingEntries = GetCryptoCandidateEntries(ctx);
            foreach (var entry in pendingEntries) //Encrypt all pending changes for candidates
                EncryptEntity(entry.Entity);

            return pendingEntries;
        }

        /// <summary>
        /// Decrypt updated entities for continued use
        /// </summary>
        /// <param name="pendingEntries"></param>
        private void DecryptPendingEntries(List<System.Data.Entity.Core.Objects.ObjectStateEntry> pendingEntries)
        {
            foreach (var entry in pendingEntries)
                DecryptEntity(entry.Entity);

        }


        private List<System.Data.Entity.Core.Objects.ObjectStateEntry> GetCryptoCandidateEntries(ApplicationDbContext ctx)
        {
            var contextAdapter = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)ctx);
            contextAdapter.ObjectContext.DetectChanges(); //force this. Sometimes entity state needs a handle jiggle
            //Get all pending entities that are not relationships and have Encrypted attribute 
            var query = contextAdapter.ObjectContext.ObjectStateManager
                .GetObjectStateEntries(System.Data.Entity.EntityState.Added | System.Data.Entity.EntityState.Modified)
                .Where(en => !en.IsRelationship && IsEncrypted(en));
            
            var pendingEntities = query.ToList();
                
            return pendingEntities;
        }

        
        /// <summary>
        /// Gets all the properties that are encryptable and encrypt them
        /// </summary>
        /// <param name="entity"></param>
        private void EncryptEntity(object entity)
        {
            var BF_Idx = GetBF_IdxProp(entity);
         
            var encryptedProperties = entity.GetType().GetProperties()
                .Where(p => p.GetCustomAttributes(typeof(Encrypted), true).Any(a => p.PropertyType == typeof(String)));
            foreach (var property in encryptedProperties)
            {
                string value = property.GetValue(entity) as string;
                if (!String.IsNullOrEmpty(value))
                {
                    string encryptedValue = Mishavad_API.Helpers.EncryptionService.Encrypt(value, BF_Idx);
                    property.SetValue(entity, encryptedValue);
                }
            }
        }

        /// <summary>
        /// Gets all the properties that are encryptable and decyrpt them
        /// </summary>
        /// <param name="entity"></param>
        private void DecryptEntity(object entity)
        {
            
            var BF_Idx = GetBF_IdxProp(entity);
                      
            var encryptedProperties = entity.GetType().GetProperties()
                .Where(p => p.GetCustomAttributes(typeof(Encrypted), true).Any(a => p.PropertyType == typeof(String)));

            foreach (var property in encryptedProperties)
            {
                string encryptedValue = property.GetValue(entity) as string;
                if (!String.IsNullOrEmpty(encryptedValue))
                {
                    string value = Mishavad_API.Helpers.EncryptionService.Decrypt(encryptedValue, BF_Idx);
                    this.Entry(entity).Property(property.Name).OriginalValue = value;
                    this.Entry(entity).Property(property.Name).IsModified = false;
                }
            }
        }

        private bool IsEncrypted(System.Data.Entity.Core.Objects.ObjectStateEntry entry)
        {
            return GetEntityType(entry).GetCustomAttributes(typeof(Encrypted), true).Any();
        }

        private bool IsEncrypted( object entity)
        {
            return entity.GetType().GetCustomAttributes(typeof(Encrypted), true).Any();
        }

        private void SetBF_IdxProp(object entity, int BF_Idx)
        {
            //Find the BF_Idx property
            var BF_Idx_Property = entity.GetType().GetProperties()
                                .Where(p => p.GetCustomAttributes(typeof(BF_Idx), true).Any())
                                .First();
            BF_Idx_Property.SetValue(entity, BF_Idx);
        }

        private int GetBF_IdxProp(object entity)
        {
            //Find the BF_Idx property
            var BF_Idx_Property = entity.GetType().GetProperties()
                                .Where(p => p.GetCustomAttributes(typeof(BF_Idx), true).Any())
                                .First();
            var BF_Idx = (int)this.Entry(entity).Property(BF_Idx_Property.Name).CurrentValue;
            return BF_Idx;
        }


        #endregion EncryptionMethods

        #region CollectedFund_and_NClaimedCalculations

     /*
     TODO Fix these things private IEnumerable<Campaign> GetCampaignsNeedCFUpdate()
        {   /*
            NOTE: We can not use lazy loading because the new GiftFunds have not been saved yet.
            Also note that we will update collected funds after changes are saved successfully!
            
            var contextAdapter = (System.Data.Entity.Infrastructure.IObjectContextAdapter)this;
            var newGiftFundsCampaignIds = contextAdapter.ObjectContext.ObjectStateManager
                .GetObjectStateEntries(System.Data.Entity.EntityState.Added)
                .Where(en => !en.IsRelationship && (en.Entity.GetType().Name == "GiftFund"))
                .Select(en => ((GiftFund)en.Entity).CampaignId);
                      

            var campaigns = from id in newGiftFundsCampaignIds
            join camp in this.Campaigns on id equals camp.Id
            select camp;

            return campaigns;
        }
    */

      /*TODO: Fix this
      private IEnumerable<Reward> GetRewardsNeedUpdate()
        {   
            //NOTE: We can not use lazy loading because the new GiftFunds have not been saved yet.
           
            var contextAdapter = (System.Data.Entity.Infrastructure.IObjectContextAdapter)this;
            var newGiftFundsRewardIds = contextAdapter.ObjectContext.ObjectStateManager
                .GetObjectStateEntries(System.Data.Entity.EntityState.Added)
                .Where(en => !en.IsRelationship && (en.Entity.GetType() == typeof(GiftFund)))
                .Select(en => ((GiftFund)en.Entity).RewardId);
            
            var rewards = from id in newGiftFundsRewardIds
                            join reward in this.Rewards on id equals reward.Id
                            select reward;
            return rewards;
        }*/


     /*   Fix these private void UpdateCollectedFunds(IEnumerable<Campaign> campaignsNeedUpdate)
        {
            foreach (var camp in campaignsNeedUpdate) {
                this.Entry(camp).Entity.CollectedFund = this.GiftFunds.Where(g => (g.CampaignId == camp.Id)).Select(g => g.Amount).Sum();
                this.Entry(camp).Entity.NBacked =  this.GiftFunds.Where(g => (g.CampaignId == camp.Id)).Distinct().Count();

                this.Entry(camp).State = System.Data.Entity.EntityState.Modified;
            }
        }
        
        private void UpdateNClaimeds(IEnumerable<Reward> rewardsNeedingUpdate)
        {
            foreach (var r in rewardsNeedingUpdate)
            {
                this.Entry(r).Entity.NClaimed = this.GiftFunds.Where(g => (g.RewardId == r.Id)).Count();
                this.Entry(r).State = System.Data.Entity.EntityState.Modified;
            }
        }*/
        #endregion

       

        private List<Campaign> GetAddedOrUpdatedCampaigns (ApplicationDbContext ctx)
        {
            //TODO: Fix GetEntryType and Make Unit test for this
            var contextAdapter = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)ctx);
            var addedOrChangedCampaigns = contextAdapter.ObjectContext.ObjectStateManager
                .GetObjectStateEntries(System.Data.Entity.EntityState.Added | System.Data.Entity.EntityState.Modified)
                .Where(en => !en.IsRelationship && (GetEntityType(en) == typeof(Campaign)))
                .Select(en => (Campaign)en.Entity).ToList();
              return addedOrChangedCampaigns;
        }
        /// <summary>
        /// This function gets the type of the entity inside a entry. Note that entr.GetType() returns ObjectStateEntry as type!
        /// Also entry.Entity.GetType returns the type of proxy object
        /// </summary>
        /// <param name="en"></param>
        /// <returns></returns>
        private Type GetEntityType(System.Data.Entity.Core.Objects.ObjectStateEntry en)
        {
            return System.Data.Entity.Core.Objects.ObjectContext.GetObjectType(en.Entity.GetType());
        }

       

        private void CustomBadRequest(string reason)
        {
            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
            { ReasonPhrase = "Bad Request - " + reason });
        }

        public DbSet<Mishavad_API.Models.ExtraInfoJSON> ExtraInfoJSONs { get; set; }

    }
}