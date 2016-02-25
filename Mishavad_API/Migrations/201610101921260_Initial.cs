namespace Mishavad_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CampaignCategories",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                        IconImagePath = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.CampaignComments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CampaignId = c.Int(nullable: false),
                        AuthorId = c.String(),
                        AuthorEmail = c.String(),
                        AuthorIP = c.String(),
                        Content = c.String(),
                        Agent = c.String(),
                        ParentId = c.Int(),
                        Status = c.Int(nullable: false),
                        CreatedDateUtc = c.DateTime(nullable: false),
                        Author_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Campaigns", t => t.CampaignId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.Author_Id)
                .Index(t => t.CampaignId)
                .Index(t => t.Author_Id);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AccNumSalt = c.String(nullable: false, maxLength: 2),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Campaigns",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Status = c.Int(nullable: false),
                        Title = c.String(nullable: false),
                        Tagline = c.String(nullable: false),
                        Story = c.String(),
                        CampaignCategoryId = c.Int(),
                        ProjectStageId = c.Int(),
                        TargetFund = c.Long(),
                        StartDateUtc = c.DateTime(),
                        TotalDays = c.Int(),
                        VerifiedByOrg = c.Boolean(),
                        VerificationDescription = c.String(),
                        LocationId = c.Int(),
                        CollectedFund = c.Long(nullable: false),
                        NBacked = c.Int(nullable: false),
                        Slug = c.String(),
                        CreatedById = c.Int(nullable: false),
                        ThumbnailFileServerId = c.Int(),
                        ThumbnailFilePath = c.String(),
                        CreatedDateUtc = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CampaignCategories", t => t.CampaignCategoryId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedById, cascadeDelete: true)
                .ForeignKey("dbo.Locations", t => t.LocationId)
                .ForeignKey("dbo.ProjectStages", t => t.ProjectStageId)
                .ForeignKey("dbo.FileServers", t => t.ThumbnailFileServerId)
                .Index(t => t.CampaignCategoryId)
                .Index(t => t.ProjectStageId)
                .Index(t => t.LocationId)
                .Index(t => t.CreatedById)
                .Index(t => t.ThumbnailFileServerId);
            
            CreateTable(
                "dbo.Documents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CampaignId = c.Int(nullable: false),
                        DocCategoryId = c.Int(nullable: false),
                        Subject = c.String(),
                        Note = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Campaigns", t => t.CampaignId, cascadeDelete: true)
                .Index(t => t.CampaignId);
            
            CreateTable(
                "dbo.DocFiles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FilePath = c.String(),
                        FileServerId = c.Int(),
                        Document_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FileServers", t => t.FileServerId)
                .ForeignKey("dbo.Documents", t => t.Document_Id)
                .Index(t => t.FileServerId)
                .Index(t => t.Document_Id);
            
            CreateTable(
                "dbo.FileServers",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        ServerIP = c.String(),
                        ServerUri = c.String(),
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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Campaigns", t => t.CampaignId)
                .ForeignKey("dbo.Rewards", t => t.RewardId)
                .Index(t => t.CampaignId)
                .Index(t => t.RewardId);
            
            CreateTable(
                "dbo.Rewards",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Amount = c.Int(nullable: false),
                        Title = c.String(),
                        Description = c.String(),
                        NAvailable = c.Int(nullable: false),
                        DeliveryDays = c.Int(),
                        AddressRequired = c.Boolean(nullable: false),
                        NClaimed = c.Int(nullable: false),
                        RemovedFlag = c.Boolean(),
                        CampaignId = c.Int(nullable: false),
                        ImageFilePath = c.String(),
                        ImageFileServerId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Campaigns", t => t.CampaignId, cascadeDelete: true)
                .ForeignKey("dbo.FileServers", t => t.ImageFileServerId)
                .Index(t => t.CampaignId)
                .Index(t => t.ImageFileServerId);
            
            CreateTable(
                "dbo.CampaignImages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CampaignId = c.Int(nullable: false),
                        ShownInGallery = c.Boolean(),
                        Description = c.String(),
                        FilePath = c.String(),
                        FileServerId = c.Int(),
                        RemovedFlag = c.Boolean(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Campaigns", t => t.CampaignId, cascadeDelete: true)
                .ForeignKey("dbo.FileServers", t => t.FileServerId)
                .Index(t => t.CampaignId)
                .Index(t => t.FileServerId);
            
            CreateTable(
                "dbo.Locations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CityId = c.Int(nullable: false),
                        Address1 = c.String(),
                        Address2 = c.String(),
                        PostalCode = c.String(maxLength: 10),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Cities", t => t.CityId, cascadeDelete: true)
                .Index(t => t.CityId);
            
            CreateTable(
                "dbo.Cities",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                        ProvinceName = c.String(),
                        ProvinceCode = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ProjectStages",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                        IconImagePath = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.CampaignTagMaps",
                c => new
                    {
                        CampaignTagName = c.String(nullable: false, maxLength: 128),
                        CampaignId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.CampaignTagName, t.CampaignId })
                .ForeignKey("dbo.Campaigns", t => t.CampaignId, cascadeDelete: true)
                .ForeignKey("dbo.CampaignTags", t => t.CampaignTagName, cascadeDelete: true)
                .Index(t => t.CampaignTagName)
                .Index(t => t.CampaignId);
            
            CreateTable(
                "dbo.CampaignTags",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 128),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.Name);
            
            CreateTable(
                "dbo.CampaignUpdates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CampaignId = c.Int(nullable: false),
                        Story = c.String(),
                        Status = c.Int(nullable: false),
                        CreatedDateUtc = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Campaigns", t => t.CampaignId, cascadeDelete: true)
                .Index(t => t.CampaignId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.OutgoingFundOrders",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        CreatedDateUtc = c.DateTime(nullable: false),
                        DateTime = c.DateTime(),
                        Status = c.Int(nullable: false),
                        ApplicationUserId = c.Int(nullable: false),
                        Amount = c.Int(nullable: false),
                        ReferenceNumber = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUserId, cascadeDelete: true)
                .Index(t => t.ApplicationUserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        RoleId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.UserInfoes",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        FirstName = c.String(),
                        LastName = c.String(),
                        NationalID = c.String(maxLength: 10),
                        BirthCertNo = c.String(),
                        DateOfBirth = c.DateTime(),
                        YearsOfEducation = c.Int(),
                        MobileNumber = c.String(),
                        PhoneNumber = c.String(),
                        BF_Idx = c.Int(nullable: false),
                        CityOfBirth_Id = c.Int(),
                        HomeLocation_Id = c.Int(),
                        WorkLocation_Id = c.Int(),
                    })
                .PrimaryKey(t => t.UserId)
                .ForeignKey("dbo.Cities", t => t.CityOfBirth_Id)
                .ForeignKey("dbo.Locations", t => t.HomeLocation_Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .ForeignKey("dbo.Locations", t => t.WorkLocation_Id)
                .Index(t => t.UserId)
                .Index(t => t.CityOfBirth_Id)
                .Index(t => t.HomeLocation_Id)
                .Index(t => t.WorkLocation_Id);
            
            CreateTable(
                "dbo.FileServerTokens",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        UserId = c.String(),
                        FileTokenType = c.Int(nullable: false),
                        TokenExpDateUtc = c.DateTime(nullable: false),
                        TokenHash = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
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
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.CampaignComments", "Author_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.UserInfoes", "WorkLocation_Id", "dbo.Locations");
            DropForeignKey("dbo.UserInfoes", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.UserInfoes", "HomeLocation_Id", "dbo.Locations");
            DropForeignKey("dbo.UserInfoes", "CityOfBirth_Id", "dbo.Cities");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.OutgoingFundOrders", "ApplicationUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.CampaignUpdates", "CampaignId", "dbo.Campaigns");
            DropForeignKey("dbo.Campaigns", "ThumbnailFileServerId", "dbo.FileServers");
            DropForeignKey("dbo.CampaignTagMaps", "CampaignTagName", "dbo.CampaignTags");
            DropForeignKey("dbo.CampaignTagMaps", "CampaignId", "dbo.Campaigns");
            DropForeignKey("dbo.Campaigns", "ProjectStageId", "dbo.ProjectStages");
            DropForeignKey("dbo.Campaigns", "LocationId", "dbo.Locations");
            DropForeignKey("dbo.Locations", "CityId", "dbo.Cities");
            DropForeignKey("dbo.CampaignImages", "FileServerId", "dbo.FileServers");
            DropForeignKey("dbo.CampaignImages", "CampaignId", "dbo.Campaigns");
            DropForeignKey("dbo.Rewards", "ImageFileServerId", "dbo.FileServers");
            DropForeignKey("dbo.GiftFunds", "RewardId", "dbo.Rewards");
            DropForeignKey("dbo.Rewards", "CampaignId", "dbo.Campaigns");
            DropForeignKey("dbo.GiftFunds", "CampaignId", "dbo.Campaigns");
            DropForeignKey("dbo.Documents", "CampaignId", "dbo.Campaigns");
            DropForeignKey("dbo.DocFiles", "Document_Id", "dbo.Documents");
            DropForeignKey("dbo.DocFiles", "FileServerId", "dbo.FileServers");
            DropForeignKey("dbo.Campaigns", "CreatedById", "dbo.AspNetUsers");
            DropForeignKey("dbo.CampaignComments", "CampaignId", "dbo.Campaigns");
            DropForeignKey("dbo.Campaigns", "CampaignCategoryId", "dbo.CampaignCategories");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.UserInfoes", new[] { "WorkLocation_Id" });
            DropIndex("dbo.UserInfoes", new[] { "HomeLocation_Id" });
            DropIndex("dbo.UserInfoes", new[] { "CityOfBirth_Id" });
            DropIndex("dbo.UserInfoes", new[] { "UserId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.OutgoingFundOrders", new[] { "ApplicationUserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.CampaignUpdates", new[] { "CampaignId" });
            DropIndex("dbo.CampaignTagMaps", new[] { "CampaignId" });
            DropIndex("dbo.CampaignTagMaps", new[] { "CampaignTagName" });
            DropIndex("dbo.Locations", new[] { "CityId" });
            DropIndex("dbo.CampaignImages", new[] { "FileServerId" });
            DropIndex("dbo.CampaignImages", new[] { "CampaignId" });
            DropIndex("dbo.Rewards", new[] { "ImageFileServerId" });
            DropIndex("dbo.Rewards", new[] { "CampaignId" });
            DropIndex("dbo.GiftFunds", new[] { "RewardId" });
            DropIndex("dbo.GiftFunds", new[] { "CampaignId" });
            DropIndex("dbo.DocFiles", new[] { "Document_Id" });
            DropIndex("dbo.DocFiles", new[] { "FileServerId" });
            DropIndex("dbo.Documents", new[] { "CampaignId" });
            DropIndex("dbo.Campaigns", new[] { "ThumbnailFileServerId" });
            DropIndex("dbo.Campaigns", new[] { "CreatedById" });
            DropIndex("dbo.Campaigns", new[] { "LocationId" });
            DropIndex("dbo.Campaigns", new[] { "ProjectStageId" });
            DropIndex("dbo.Campaigns", new[] { "CampaignCategoryId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.CampaignComments", new[] { "Author_Id" });
            DropIndex("dbo.CampaignComments", new[] { "CampaignId" });
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.IncomingFundOrders");
            DropTable("dbo.FileServerTokens");
            DropTable("dbo.UserInfoes");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.OutgoingFundOrders");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.CampaignUpdates");
            DropTable("dbo.CampaignTags");
            DropTable("dbo.CampaignTagMaps");
            DropTable("dbo.ProjectStages");
            DropTable("dbo.Cities");
            DropTable("dbo.Locations");
            DropTable("dbo.CampaignImages");
            DropTable("dbo.Rewards");
            DropTable("dbo.GiftFunds");
            DropTable("dbo.FileServers");
            DropTable("dbo.DocFiles");
            DropTable("dbo.Documents");
            DropTable("dbo.Campaigns");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.CampaignComments");
            DropTable("dbo.CampaignCategories");
        }
    }
}
