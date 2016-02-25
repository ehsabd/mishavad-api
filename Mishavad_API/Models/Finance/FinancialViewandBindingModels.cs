using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mishavad_API.Models
{
    public enum OrderStatusVM
    {
        Null = 0,
        BankInProcess = 1,
        BankApproved = 2,
        WaitingReview = 3 //Usually for Outgoing funds
    }

    public class NewDepositInvoiceBM
    {
        public string Description { get; set; }
        public string Email { get; set; }
        [Required]
        public int Amount { get; set; }
        public string ExtraInfoJSON { get; set; }
        public int? ReceiverCampaignId { get; set; }
    }

    public class GiftFundVM
    {
        [Display(Name = "_ui_key_GiftFundDate")]
        public DateTime CreatedDateTime { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Required]

        [Display(Name = "_ui_key_Email")]
        public string Email { get; set; }

        [Display(Name = "_ui_key_GiftAmount")]
        public int Amount { get; set; }
        public int? CampaignId { get; set; }
    }
}