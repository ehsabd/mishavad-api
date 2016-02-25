namespace Mishavad_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FinancialModelsMajor : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.GiftFunds", "CampaignId", "dbo.Campaigns");
            DropForeignKey("dbo.GiftFunds", "RewardId", "dbo.Rewards");
            DropForeignKey("dbo.OutgoingFundOrders", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.GiftFunds", new[] { "CampaignId" });
            DropIndex("dbo.GiftFunds", new[] { "RewardId" });
            DropIndex("dbo.OutgoingFundOrders", new[] { "UserId" });
            CreateTable(
                "dbo.Accounts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AccountName = c.String(),
                        AccountType = c.Int(nullable: false),
                        Balance = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.JournalEntries",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        CreatedDateUtc = c.DateTime(nullable: false),
                        AccountId = c.Int(nullable: false),
                        Amount = c.Int(nullable: false),
                        IsCredit = c.Boolean(nullable: false),
                        Balance = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Accounts", t => t.AccountId, cascadeDelete: true)
                .Index(t => t.AccountId);
            
            CreateTable(
                "dbo.Transactions",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        CreatedDateUtc = c.DateTime(nullable: false),
                        Amount = c.Int(nullable: false),
                        DebitAccountId = c.Int(nullable: false),
                        CreditAccountId = c.Int(nullable: false),
                        ExtraInfoJSON = c.String(),
                        Account_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Accounts", t => t.Account_Id)
                .Index(t => t.Account_Id);
            
            CreateTable(
                "dbo.DepositInvoices",
                c => new
                    {
                        Id = c.Long(nullable: false),
                        Amount = c.Int(nullable: false),
                        CreatedDateUtc = c.DateTime(nullable: false),
                        PaymentGateway = c.Int(nullable: false),
                        GatewayTransactionReferenceID = c.String(),
                        Status = c.Int(nullable: false),
                        ReferenceNumber = c.String(),
                        GatewayTraceNumber = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Transactions", t => t.Id)
                .Index(t => t.Id);
            
            DropTable("dbo.GiftFunds");
            DropTable("dbo.OutgoingFundOrders");
            DropTable("dbo.IncomingFundOrders");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.IncomingFundOrders",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        InvNumSalt = c.String(nullable: false, maxLength: 2),
                        CreatedDateUtc = c.DateTime(nullable: false),
                        TransactionReferenceID = c.String(),
                        Status = c.Int(nullable: false),
                        FirstName = c.String(),
                        LastName = c.String(),
                        MobileNumber = c.String(maxLength: 10),
                        Email = c.String(),
                        Amount = c.Int(nullable: false),
                        ReferenceNumber = c.String(),
                        GiftFundCampaignId = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.OutgoingFundOrders",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        CreatedDateUtc = c.DateTime(nullable: false),
                        DateTime = c.DateTime(),
                        Status = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        Amount = c.Int(nullable: false),
                        ReferenceNumber = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.GiftFunds",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        CreatedDateUtc = c.DateTime(nullable: false),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Email = c.String(nullable: false),
                        Amount = c.Int(nullable: false),
                        CampaignId = c.Int(),
                        RewardId = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            DropForeignKey("dbo.Transactions", "Account_Id", "dbo.Accounts");
            DropForeignKey("dbo.DepositInvoices", "Id", "dbo.Transactions");
            DropForeignKey("dbo.JournalEntries", "AccountId", "dbo.Accounts");
            DropIndex("dbo.DepositInvoices", new[] { "Id" });
            DropIndex("dbo.Transactions", new[] { "Account_Id" });
            DropIndex("dbo.JournalEntries", new[] { "AccountId" });
            DropTable("dbo.DepositInvoices");
            DropTable("dbo.Transactions");
            DropTable("dbo.JournalEntries");
            DropTable("dbo.Accounts");
            CreateIndex("dbo.OutgoingFundOrders", "UserId");
            CreateIndex("dbo.GiftFunds", "RewardId");
            CreateIndex("dbo.GiftFunds", "CampaignId");
            AddForeignKey("dbo.OutgoingFundOrders", "UserId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.GiftFunds", "RewardId", "dbo.Rewards", "Id");
            AddForeignKey("dbo.GiftFunds", "CampaignId", "dbo.Campaigns", "Id");
        }
    }
}
