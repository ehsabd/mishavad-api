using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization; //for [Data Contract]
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Mishavad_API.Models
{
   

    /*ENUMS*/
    /*NOTE
    Flags make it possible to have multiple status for a campaign
    this makes coding easier in some conditions.
    Particularly, Approved & ReadOnly statuses are used for first page and edit page to indicate
    which campagins are going to be shown and which ones are editable
    see this answer for order of flags in ToString : http://stackoverflow.com/a/26107341/2571422
    */
    [JsonConverter(typeof(StringEnumConverter))]
    [Flags]
    public enum CampaignStatus {
        Unknown=0, 
        PreliminaryRegistered = 1,  
        CompletelyRegistered = 2,
        Waiting = 4,
        Approved = 8,
        NotApproved = 16,
        Running =32,
        Fulfilled = 64,
        Closed = 128,
        Unfulfilled =256,
        ReadOnly =512
    }

    public enum CommentStatus {
        Waiting = 0,
        Approved = 1,
        Spam = -1
    }

    public enum UpdateStatus {
        Waiting = 0,
        Approved = 1,
        NotApproved = -1
    }

   

    /*NOTE: Data Members are intended for use by OData queries, the properties that should be exposed to
    the user should be moderated in the controller and by view models. For example CreatedById is attributed as 
    DataMember though we would not expose it to the ordinary user!*/

    [DataContract]
    public class Campaign :ISoftDelete
    {
        //TODO: Add model validations using usual and customized validation attributes to prevent cheating on API!
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public CampaignStatus Status { get; set; }

        [DataMember]
        [Required]
        
        public string Title { get; set; }

        [DataMember]
        [Required]
        [DataType(DataType.MultilineText)]
        
        public string Tagline { get; set; }

        [DataMember]
        [DataType(DataType.MultilineText)]
        
        public string Story { get; set; }

        [DataMember]
        [JsonProperty(PropertyName = "categoryId")]
        
        public int? CampaignCategoryId { get; set; }

        [DataMember]
        [NotMapped]
        [JsonProperty(PropertyName = "category")]
        public string CategoryName
        {
            get
            {
                return Category == null ? null : Category.Name;
            }
        }

        [DataMember]
        [Display(Name = "_ui_key_ProjectStage")]
        
        public int? ProjectStageId { get; set; }

        [DataMember]
        [NotMapped]
        [JsonProperty(PropertyName = "projectStage")]
        public string ProjectStageName
        {
            get
            {
                return ProjectStage == null ? null : ProjectStage.Name;

            }
        }

        [DataMember]
        [Display(Name = "_ui_key_TargetFund")]
        public long? TargetFund { get; set; }

        [DataMember]
        [Display(Name = "_ui_key_StartDate")]
        public DateTime? StartDateUtc { get; set; }

        [DataMember]
        public int? TotalDays { get; set; }

        [DataMember]
        public bool? VerifiedByOrg { get; set; }

        [DataMember]
        public string VerificationDescription { get; set; }

        [DataMember]
        
        public int? LocationId { get; set; }

        [DataMember]
        [NotMapped]
        public int? TotalSecondsLeft
        {
            get
            {
                if (StartDateUtc == null || TotalDays == null)
                    return null;

                var timeLeft = new TimeSpan((int)TotalDays, 0, 0, 0) - ((TimeSpan)(DateTime.UtcNow - StartDateUtc));
                var totalSeconds = timeLeft.TotalSeconds;
                return totalSeconds < 0 ? 0 : (int)totalSeconds;
                /*if (totalSeconds<0)
                    return (new { days = 0, hours = 0, mins = 0, secs = 0 });
                else
                    return (new { days = timeLeft.Days, hours = timeLeft.Hours, mins = timeLeft.Minutes, secs = timeLeft.Seconds });
                */
            
            }
        }

        [NotMapped]
        [DataMember]
        public DateTime? ComputedEndDateUtc
        {
            get
            {
                if (TotalDays == null || StartDateUtc == null)
                    return null;
                return ((DateTime)StartDateUtc).AddDays((int)TotalDays);
            }
        }

        /// <summary>
        /// It is computed this way = CollectedFund*100/TargetFund (Returns an integer but does not rounds)
        /// </summary>
        [DataMember]
        [NotMapped]
        public int? CollectedFundPercent
        {
            get
            {
                return TargetFund.HasValue ? ((int)(CollectedFund * 100 / TargetFund.Value)) : (int?)null;
            }
        }
        
        

        //Computes Fullpath of Thumbnail for ease of use in controllers
        [DataMember]
        [NotMapped]
        [JsonProperty(PropertyName = "thumbnail")]
        public string ThumbnailFullPath
        {
            get
            {
                return Helpers.FileServerTokenManager.GetFullPath (ThumbnailFileServer, ThumbnailFilePath);
            }
        }

        //Externally Updated Properties
        [DataMember]
        public long CollectedFund { get; set; }

        [DataMember]
        public int NBacked { get; set; }

        [DataMember]
        [Index(IsUnique = true)]
        [StringLength(300)]//need this for index
        public string Slug { get; set; }

        //Non-DataMembers

        /*NOTE: CreatedById should be a Non-DataMember by default,
        Only certain controllers may allow client to access this info*/
        public int CreatedById { get; set; }

        
        public int? ThumbnailFileServerId { get; set; }
        public string ThumbnailFilePath { get; set; }

        /*NOTE: Contrary to the users', campaigns' AccountIds should be Nullable, because we only need to 
        add AccountId when a campaign is accepted and there could be payments to it. We do not want to pollute our database with accounts of
        not-accepted campaigns.
        Also note that for the same reason the order of AccountIds depend on the order of acceptance, not creation of campaigns*/
        public int? AccountId { get; set; }
        public virtual Account Account { get; set; }

        [Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedDateUtc { get; set; }

        public DateTime? RemovedFlagUtc { get; set; }

        public virtual FileServer ThumbnailFileServer { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual CampaignCategory Category { get; set; }
        public virtual ProjectStage ProjectStage { get; set; }
        public virtual IList<Reward> Rewards { get; set; }
        public virtual IList<CampaignImage> Images { get; set; }
        public virtual IList<CampaignComment> Comments { get; set; }
        public virtual IList<CampaignUpdate> Updates { get; set; }
        public virtual IList<CampaignTagMap> TagMaps { get; set; }
        public virtual IList<CampaignDocumentMap> CampaignDocumentMaps { get; set; }
        public virtual Location Location{get; set;}
    }

    [DataContract]
    public class CampaignImage :ISoftDelete
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public int CampaignId { get; set; }
        [DataMember]
        public bool? ShownInGallery { get; set; }

        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string FilePath { get; set; }
        [DataMember]
        public int? FileServerId { get; set; }
        [DataMember]
        public DateTime? RemovedFlagUtc { get; set; }

        public virtual FileServer FileServer { get; set; }

        [NotMapped]
        [DataMember]
        public string FullPath
        {
            get
            {
                return Helpers.FileServerTokenManager.GetFullPath (FileServer, FilePath);
            }
        }
        public virtual Campaign Campaign { get; set; }
    }

    public class CampaignCategory
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        [Display(Name = "_ui_key_CampaignCategory")]
        public string Name { get; set; }
        public string IconImagePath { get; set; }
    }

    public class CampaignComment
    {
        public int Id { get; set; }
        public int CampaignId { get; set; }
        public string AuthorId { get; set; }
        public string AuthorEmail { get; set; }
        public string AuthorIP { get; set; }
        public string Content { get; set; }
        public string Agent { get; set; }
        public int? ParentId { get; set; }
        public CommentStatus Status { get; set; }

        public virtual ApplicationUser Author { get; set; }

        public virtual Campaign Campaign { get; set; }

        [Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedDateUtc { get; set; }
    }


    public class CampaignTag
    {
        [Key]
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class CampaignTagMap
    {
        // [ForeignKey(CampaignTag)]
        [Key]
        [Column(Order = 1)]
        public string CampaignTagName { get; set; }
        [Key]
        [Column(Order = 2)]
        public int CampaignId { get; set; }
        public virtual CampaignTag CampaignTag { get; set; }
        public virtual Campaign Campaign { get; set; }
    }

    public class CampaignUpdate
    {
        public int Id { get; set; }
        public int CampaignId { get; set; }
        public string Story { get; set; }
        public UpdateStatus Status { get; set; }

        public virtual Campaign Campaign { get; set; }

        [Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedDateUtc { get; set; }
    }

    [DataContract]
    public class City
    {
        [DataMember]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string ProvinceName { get; set; }
        [DataMember]
        public string ProvinceCode { get; set; }
    }

    
 

    /*NOTE:
        We can have multiple campaigns for one location (e.g. staged or extended campaigns or one who have multiple projects)  
        Locations are also used for users (work and home locations) so there are no virtual properties pertaining to campaigns (foreign key problems)     
    */

    [DataContract]
    public class Location
    {
        public int Id { get; set; }

        [DataMember]
        public int CityId { get; set; }
        [DataMember]
        public string Address1 { get; set; }
        [DataMember]
        public string Address2 { get; set; }

        [MaxLength(10)]
        [DataMember]
        public string PostalCode { get; set; }

        [DataMember]
        public virtual City City { get; set; }

    }

    public class ProjectStage
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        [Display(Name = "_ui_key_ProjectStage")]
        public string Name { get; set; }
        public string IconImagePath { get; set; }
    }

    [DataContract]
    public class Reward:ISoftDelete
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        [Required]
        public int Amount { get; set; } 
        [DataMember]
        [Required]
        public string Title { get; set; }
        [DataMember]
        [Required]
        public string Description { get; set; }
        [DataMember]
        public int NAvailable { get; set; }
        //Nullable because we don't want to force the creater to give a fixed time
        //maybe we want ?!
        //This is also a data member because its needed for editing. Not needed in other cases, but no harm in that! (not secret!)
        [DataMember]
        public int? DeliveryDays { get; set; }
        [DataMember]
        public bool AddressRequired { get; set; }

   
        public string ImageFilePath { get; set; }
        public int? ImageFileServerId { get; set; }

        [DataMember]
        [NotMapped]
        public string ImagePath { get {
                return Helpers.FileServerTokenManager.GetFullPath (ImageFileServer, ImageFilePath);
            }
        }

        /// <summary>
        /// This property stores the number of rewards that have been claimed
        /// This property is updated externally when a new gift fund added
        /// </summary>
        [DataMember]
        public int NClaimed { get; set; }

        [DataMember]
        [NotMapped]
        public DateTime? EstimatedDeliveryDateUtc
        {
            get
            {
                if (Campaign == null ||
                    Campaign.ComputedEndDateUtc == null ||
                    DeliveryDays == null)
                    return null;
                return ((DateTime)Campaign.ComputedEndDateUtc).AddDays((int)DeliveryDays);
            }
        }

        [DataMember]
        [NotMapped]
        public bool IsAvailable { get
            {
                return NAvailable > NClaimed;
            }
        }

        //Non-DataMembers
        public DateTime? RemovedFlagUtc { get; set; }
        public int CampaignId { get; set; }
        public virtual Campaign Campaign { get; set; }
        //TODO: Add something like this using our new accounting system! like Qty in http://stackoverflow.com/questions/4074737/accounting-database-storing-a-transaction
        public virtual IList<TransactionRewardMap> TransactionRewardMaps { get; set; }
        public virtual FileServer ImageFileServer { get; set; }
        
    }

    public class ClaimedReward
    {
        public int Id { get; set; }
        public int RewardId { get; set; }
        public int TransactionId { get; set; }
        public virtual Reward Reward { get; set; }
        public virtual Transaction Transaction { get; set; }
    }

   
    


   
  

    
}