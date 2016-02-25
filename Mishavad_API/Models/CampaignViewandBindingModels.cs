using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mishavad_API.Models
{
    
    public class Campaign_CreateBM //Add other validations if needed //Keep Display Tags so that you write frontend code with ease
    {
        [Required]
        [MaxLength(100)]
        [Display(Name = "_ui_key_CampaignTitle")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "_ui_key_TargetFund")]
        public long? TargetFund { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        [Display(Name = "_ui_key_CampaignTagline")]
        public string Tagline { get; set; }

    }

    //TODO: Add Max length validations to prevent attacks
    public class Campaign_UpdateBM
    {

        [MaxLength(100)]
        [Required]
        public string Title { get; set; }
        //TODO: add max length
        [Required]
        public string Tagline { get; set; }
        //TODO: add max length
        public string Story { get; set; }
        /*NOTE: request to validate the campaign is done by changing Status to waiting!*/
        public string Status { get; set; }

        public int? CategoryId { get; set; }
        public int? ProjectStageId { get; set; }
        //NOTE we check target fund range in review
        public long? TargetFund { get; set; }
        [Range(30,90)]
        public int? TotalDays { get; set; }
        public bool? VerifiedByOrg { get; set; }
        public string VerificationDescription { get; set; }
        //for Location
        public int? CityId { get; set; }
        //for Thumbnail
        public string ThumbnailPath { get; set; }
        public int? ThumbnailServerId { get; set; }
        public string Thumbnail { get; set; }
        public string Base64Thumbnail { get; set; }

        //for CampaignTags
        /*NOTE: Limit on the length of a tag string is placed in the Controller; This can be 
        imporoved in FUTURE by making use of custom annotation attributes*/
        [MaxLength(5)]
        public string[] Tags { get; set; }

        public bool RemovedFlag { get; set; }
    }


    /*The following Binding models only accept base64 image. This is a design we made 
    and perhaps we do not change this in future*/

    public class CampaignImage_AddBM {
        public string Description { get; set; }
        public bool? ShownInGallery { get; set; }
        public string Base64Image { get; set; }
        public bool AppendedToStory { get; set; }
    }

    public class CampaignImage_UpdateBM {
        public string Description { get; set; }
        public bool? ShownInGallery { get; set; }
        public string Base64Image { get; set; }
    }

    public class Reward_AddBM
    {
        [Required]
        public int Amount { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        public int NAvailable { get; set; }
        public int? DeliveryDays { get; set; }
        public bool AddressRequired { get; set; }
        public string Base64Image { get; set; }    
    }

    public class Reward_UpdateBM
    {
        public bool? AddressRequired { get; set; }
        public int? Amount { get; set; }
        public string Base64Image { get; set; }
        public int? DeliveryDays { get; set; }
        public string Description { get; set; }
        public int? NAvailable { get; set; }
        public string Title { get; set; }
    }

  

    public class Campaign_WaitingValidationModel
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Tagline { get; set; }
        [Required]
        public string Story { get; set; }
        [Required]
        public int CampaignCategoryId { get; set; }
        [Required]
        public int ProjectStageId { get; set; }
        [Required]
        public long TargetFund { get; set; }
        [Required]
        public int TotalDays { get; set; }
        [Required]
        public int LocationId { get; set; }
        [Required]
        public int ThumbnailFileServerId { get; set; }
        [Required]
        public string ThumbnailFilePath { get; set; }
    }

    /// <summary>
    /// View model that will be shown when the agent uses GET API for Campaign
    /// All VM properties are nullable because we do not want to get default values 
    /// when properties are transfered from data models to view models
    /// </summary>
    public class CampaignVM
    {
        public int Id { get; set; }
        public CampaignStatus Status { get; set;}
        public string Title { get; set; }
        public string Tagline { get; set; }
        public string[] StoryElements { get; set; }
        public int? CategoryId { get; set; }
        public string Category { get; set; } //We need name of category and projectstage in view too
        public int? ProjectStageId { get; set; }
        public string ProjectStage { get; set; }
        public long? TargetFund { get; set; }
        public int? TotalDays { get; set; }
        public bool? VerifiedByOrg { get; set; }
        public string VerificationDescription { get; set; }
        //for Location
        public int? CityId { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        //for Thumbnail
        public string Thumbnail { get; set; } //full path of thumbnail
        //for CampaignTags
        public IList<string> Tags { get; set; }
        public int? NBacked { get; set; }
        public long? CollectedFund { get; set; }
        public int?  CollectedFundPercent { get; set; }
        public int? TotalSecondsLeft { get; set; }

        public int? CreatedById { get; set; }
    }



    
    
    
    
    public class ContributeVM
    {
        [Display(Name = "_ui_key_FirstName")]
        public string FirstName { get; set; }

        [Display(Name = "_ui_key_LastName")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "_ui_key_Email")]
        public string Email { get; set; }

        [Display(Name = "_ui_key_GiftAmount")]
        public int Amount { get; set; }
    
    
    }
      

}