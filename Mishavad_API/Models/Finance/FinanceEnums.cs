using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mishavad_API.Models
{
    


    public enum OrderStatus { 
        Null = 0,
        BankInProcess = 1,  
        BankApproved = 2,
        WaitingReview = 3 //Usually for Outgoing funds
    }

    public enum AccountType
    {
        UserAccount = 10,
        CampaignAccount = 20,
        PaymentGatewayAccount = 30,
        PlatformFeesAccount = 40,
        AnonymousContributorsAccount = 50,
        GameSponsorAccount = 60
    }

    public enum TransactionStatus
    {
        Pending = 10,
        Completed = 20
    }
    
    public enum PaymentGateway
    {
        Pasargad = 10,
        ZarinPal = 20,
        Fake = -10
    }

   



}