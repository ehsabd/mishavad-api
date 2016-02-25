namespace Mishavad_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FinancialModels2 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.DepositInvoices", "Id", "dbo.Transactions");
            DropForeignKey("dbo.Transactions", "Account_Id", "dbo.Accounts");
            DropIndex("dbo.Transactions", new[] { "Account_Id" });
            DropIndex("dbo.DepositInvoices", new[] { "Id" });
            DropPrimaryKey("dbo.DepositInvoices");
            CreateTable(
                "dbo.TransactionRewardMaps",
                c => new
                    {
                        TransactionId = c.Long(nullable: false),
                        RewardId = c.Int(),
                    })
                .PrimaryKey(t => t.TransactionId)
                .ForeignKey("dbo.Rewards", t => t.RewardId)
                .ForeignKey("dbo.Transactions", t => t.TransactionId)
                .Index(t => t.TransactionId)
                .Index(t => t.RewardId);
            
            CreateTable(
                "dbo.ExtraInfoJSONs",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        InfoJSON = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TransactionChargebackMaps",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        OriginalTransactionId = c.Long(),
                        ChargebackTransactionId = c.Long(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Transactions", t => t.ChargebackTransactionId)
                .ForeignKey("dbo.Transactions", t => t.OriginalTransactionId)
                .Index(t => t.OriginalTransactionId, unique: true)
                .Index(t => t.ChargebackTransactionId, unique: true);
            
            CreateTable(
                "dbo.TransactionDepositInvoiceMaps",
                c => new
                    {
                        TransactionId = c.Long(nullable: false),
                        DepositInvoiceId = c.Long(),
                    })
                .PrimaryKey(t => t.TransactionId)
                .ForeignKey("dbo.DepositInvoices", t => t.DepositInvoiceId)
                .ForeignKey("dbo.Transactions", t => t.TransactionId)
                .Index(t => t.TransactionId)
                .Index(t => t.DepositInvoiceId);
            
            AddColumn("dbo.JournalEntries", "TransactionId", c => c.Long(nullable: false));
            AddColumn("dbo.Transactions", "CreatedById", c => c.Int(nullable: false));
            AddColumn("dbo.Transactions", "ExtraInfoJSONId", c => c.Long());
            AddColumn("dbo.DepositInvoices", "AccountName", c => c.String());
            AddColumn("dbo.DepositInvoices", "ReceiverCampaignId", c => c.Int());
            AddColumn("dbo.DepositInvoices", "ExtraInfoJSONId", c => c.Long());
            AddColumn("dbo.AspNetUsers", "AccountId", c => c.Int());
            AddColumn("dbo.Campaigns", "AccountId", c => c.Int());
            AlterColumn("dbo.Accounts", "AccountName", c => c.String(maxLength: 300));
            AlterColumn("dbo.DepositInvoices", "Id", c => c.Long(nullable: false, identity: true));
            AddPrimaryKey("dbo.DepositInvoices", "Id");
            CreateIndex("dbo.Accounts", "AccountName", unique: true);
            CreateIndex("dbo.JournalEntries", "TransactionId");
            CreateIndex("dbo.Transactions", "CreatedById");
            CreateIndex("dbo.Transactions", "ExtraInfoJSONId");
            CreateIndex("dbo.AspNetUsers", "AccountId");
            CreateIndex("dbo.Campaigns", "AccountId");
            CreateIndex("dbo.DepositInvoices", "ReceiverCampaignId");
            CreateIndex("dbo.DepositInvoices", "ExtraInfoJSONId");
            AddForeignKey("dbo.AspNetUsers", "AccountId", "dbo.Accounts", "Id");
            AddForeignKey("dbo.Campaigns", "AccountId", "dbo.Accounts", "Id");
            AddForeignKey("dbo.Transactions", "CreatedById", "dbo.AspNetUsers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Transactions", "ExtraInfoJSONId", "dbo.ExtraInfoJSONs", "Id");
            AddForeignKey("dbo.JournalEntries", "TransactionId", "dbo.Transactions", "Id", cascadeDelete: true);
            AddForeignKey("dbo.DepositInvoices", "ExtraInfoJSONId", "dbo.ExtraInfoJSONs", "Id");
            AddForeignKey("dbo.DepositInvoices", "ReceiverCampaignId", "dbo.Campaigns", "Id");
            DropColumn("dbo.Accounts", "Balance");
            DropColumn("dbo.JournalEntries", "Balance");
            DropColumn("dbo.Transactions", "Amount");
            DropColumn("dbo.Transactions", "DebitAccountId");
            DropColumn("dbo.Transactions", "CreditAccountId");
            DropColumn("dbo.Transactions", "ExtraInfoJSON");
            DropColumn("dbo.Transactions", "Account_Id");
            DropColumn("dbo.AspNetUsers", "AccNumSalt");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "AccNumSalt", c => c.String(nullable: false, maxLength: 2));
            AddColumn("dbo.Transactions", "Account_Id", c => c.Int());
            AddColumn("dbo.Transactions", "ExtraInfoJSON", c => c.String());
            AddColumn("dbo.Transactions", "CreditAccountId", c => c.Int(nullable: false));
            AddColumn("dbo.Transactions", "DebitAccountId", c => c.Int(nullable: false));
            AddColumn("dbo.Transactions", "Amount", c => c.Int(nullable: false));
            AddColumn("dbo.JournalEntries", "Balance", c => c.Long(nullable: false));
            AddColumn("dbo.Accounts", "Balance", c => c.Long(nullable: false));
            DropForeignKey("dbo.TransactionDepositInvoiceMaps", "TransactionId", "dbo.Transactions");
            DropForeignKey("dbo.TransactionDepositInvoiceMaps", "DepositInvoiceId", "dbo.DepositInvoices");
            DropForeignKey("dbo.TransactionChargebackMaps", "OriginalTransactionId", "dbo.Transactions");
            DropForeignKey("dbo.TransactionChargebackMaps", "ChargebackTransactionId", "dbo.Transactions");
            DropForeignKey("dbo.DepositInvoices", "ReceiverCampaignId", "dbo.Campaigns");
            DropForeignKey("dbo.DepositInvoices", "ExtraInfoJSONId", "dbo.ExtraInfoJSONs");
            DropForeignKey("dbo.JournalEntries", "TransactionId", "dbo.Transactions");
            DropForeignKey("dbo.Transactions", "ExtraInfoJSONId", "dbo.ExtraInfoJSONs");
            DropForeignKey("dbo.Transactions", "CreatedById", "dbo.AspNetUsers");
            DropForeignKey("dbo.TransactionRewardMaps", "TransactionId", "dbo.Transactions");
            DropForeignKey("dbo.TransactionRewardMaps", "RewardId", "dbo.Rewards");
            DropForeignKey("dbo.Campaigns", "AccountId", "dbo.Accounts");
            DropForeignKey("dbo.AspNetUsers", "AccountId", "dbo.Accounts");
            DropIndex("dbo.TransactionDepositInvoiceMaps", new[] { "DepositInvoiceId" });
            DropIndex("dbo.TransactionDepositInvoiceMaps", new[] { "TransactionId" });
            DropIndex("dbo.TransactionChargebackMaps", new[] { "ChargebackTransactionId" });
            DropIndex("dbo.TransactionChargebackMaps", new[] { "OriginalTransactionId" });
            DropIndex("dbo.DepositInvoices", new[] { "ExtraInfoJSONId" });
            DropIndex("dbo.DepositInvoices", new[] { "ReceiverCampaignId" });
            DropIndex("dbo.TransactionRewardMaps", new[] { "RewardId" });
            DropIndex("dbo.TransactionRewardMaps", new[] { "TransactionId" });
            DropIndex("dbo.Campaigns", new[] { "AccountId" });
            DropIndex("dbo.AspNetUsers", new[] { "AccountId" });
            DropIndex("dbo.Transactions", new[] { "ExtraInfoJSONId" });
            DropIndex("dbo.Transactions", new[] { "CreatedById" });
            DropIndex("dbo.JournalEntries", new[] { "TransactionId" });
            DropIndex("dbo.Accounts", new[] { "AccountName" });
            DropPrimaryKey("dbo.DepositInvoices");
            AlterColumn("dbo.DepositInvoices", "Id", c => c.Long(nullable: false));
            AlterColumn("dbo.Accounts", "AccountName", c => c.String());
            DropColumn("dbo.Campaigns", "AccountId");
            DropColumn("dbo.AspNetUsers", "AccountId");
            DropColumn("dbo.DepositInvoices", "ExtraInfoJSONId");
            DropColumn("dbo.DepositInvoices", "ReceiverCampaignId");
            DropColumn("dbo.DepositInvoices", "AccountName");
            DropColumn("dbo.Transactions", "ExtraInfoJSONId");
            DropColumn("dbo.Transactions", "CreatedById");
            DropColumn("dbo.JournalEntries", "TransactionId");
            DropTable("dbo.TransactionDepositInvoiceMaps");
            DropTable("dbo.TransactionChargebackMaps");
            DropTable("dbo.ExtraInfoJSONs");
            DropTable("dbo.TransactionRewardMaps");
            AddPrimaryKey("dbo.DepositInvoices", "Id");
            CreateIndex("dbo.DepositInvoices", "Id");
            CreateIndex("dbo.Transactions", "Account_Id");
            AddForeignKey("dbo.Transactions", "Account_Id", "dbo.Accounts", "Id");
            AddForeignKey("dbo.DepositInvoices", "Id", "dbo.Transactions", "Id");
        }
    }
}
