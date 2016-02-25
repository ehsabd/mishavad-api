namespace Mishavad_API.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations;
    
    public partial class ImproveRemovedFlag : DbMigration
    {
        public override void Up()
        {
            AlterTableAnnotations(
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
                        Slug = c.String(maxLength: 300),
                        CreatedById = c.Int(nullable: false),
                        ThumbnailFileServerId = c.Int(),
                        ThumbnailFilePath = c.String(),
                        AccountId = c.Int(),
                        CreatedDateUtc = c.DateTime(nullable: false),
                        RemovedFlagUtc = c.DateTime(),
                    },
                annotations: new Dictionary<string, AnnotationValues>
                {
                    { 
                        "DynamicFilter_Campaign_RemovedFlagUtc",
                        new AnnotationValues(oldValue: null, newValue: "EntityFramework.DynamicFilters.DynamicFilterDefinition")
                    },
                });
            
            AlterTableAnnotations(
                "dbo.Documents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        FilePath = c.String(nullable: false),
                        FileServerId = c.Int(nullable: false),
                        RemovedFlagUtc = c.DateTime(),
                        BF_Idx = c.Int(nullable: false),
                    },
                annotations: new Dictionary<string, AnnotationValues>
                {
                    { 
                        "DynamicFilter_Document_RemovedFlagUtc",
                        new AnnotationValues(oldValue: null, newValue: "EntityFramework.DynamicFilters.DynamicFilterDefinition")
                    },
                });
            
            AlterTableAnnotations(
                "dbo.CampaignImages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CampaignId = c.Int(nullable: false),
                        ShownInGallery = c.Boolean(),
                        Description = c.String(),
                        FilePath = c.String(),
                        FileServerId = c.Int(),
                        RemovedFlagUtc = c.DateTime(),
                    },
                annotations: new Dictionary<string, AnnotationValues>
                {
                    { 
                        "DynamicFilter_CampaignImage_RemovedFlagUtc",
                        new AnnotationValues(oldValue: null, newValue: "EntityFramework.DynamicFilters.DynamicFilterDefinition")
                    },
                });
            
            AlterTableAnnotations(
                "dbo.Rewards",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Amount = c.Int(nullable: false),
                        Title = c.String(nullable: false),
                        Description = c.String(nullable: false),
                        NAvailable = c.Int(nullable: false),
                        DeliveryDays = c.Int(),
                        AddressRequired = c.Boolean(nullable: false),
                        ImageFilePath = c.String(),
                        ImageFileServerId = c.Int(),
                        NClaimed = c.Int(nullable: false),
                        RemovedFlagUtc = c.DateTime(),
                        CampaignId = c.Int(nullable: false),
                    },
                annotations: new Dictionary<string, AnnotationValues>
                {
                    { 
                        "DynamicFilter_Reward_RemovedFlagUtc",
                        new AnnotationValues(oldValue: null, newValue: "EntityFramework.DynamicFilters.DynamicFilterDefinition")
                    },
                });
            
            AddColumn("dbo.Campaigns", "RemovedFlagUtc", c => c.DateTime());
            AddColumn("dbo.Documents", "RemovedFlagUtc", c => c.DateTime());
            AddColumn("dbo.CampaignImages", "RemovedFlagUtc", c => c.DateTime());
            AddColumn("dbo.Rewards", "RemovedFlagUtc", c => c.DateTime());
            DropColumn("dbo.Campaigns", "RemovedFlag");
            DropColumn("dbo.CampaignImages", "RemovedFlag");
            DropColumn("dbo.Rewards", "RemovedFlag");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Rewards", "RemovedFlag", c => c.Boolean());
            AddColumn("dbo.CampaignImages", "RemovedFlag", c => c.Boolean());
            AddColumn("dbo.Campaigns", "RemovedFlag", c => c.Boolean());
            DropColumn("dbo.Rewards", "RemovedFlagUtc");
            DropColumn("dbo.CampaignImages", "RemovedFlagUtc");
            DropColumn("dbo.Documents", "RemovedFlagUtc");
            DropColumn("dbo.Campaigns", "RemovedFlagUtc");
            AlterTableAnnotations(
                "dbo.Rewards",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Amount = c.Int(nullable: false),
                        Title = c.String(nullable: false),
                        Description = c.String(nullable: false),
                        NAvailable = c.Int(nullable: false),
                        DeliveryDays = c.Int(),
                        AddressRequired = c.Boolean(nullable: false),
                        ImageFilePath = c.String(),
                        ImageFileServerId = c.Int(),
                        NClaimed = c.Int(nullable: false),
                        RemovedFlagUtc = c.DateTime(),
                        CampaignId = c.Int(nullable: false),
                    },
                annotations: new Dictionary<string, AnnotationValues>
                {
                    { 
                        "DynamicFilter_Reward_RemovedFlagUtc",
                        new AnnotationValues(oldValue: "EntityFramework.DynamicFilters.DynamicFilterDefinition", newValue: null)
                    },
                });
            
            AlterTableAnnotations(
                "dbo.CampaignImages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CampaignId = c.Int(nullable: false),
                        ShownInGallery = c.Boolean(),
                        Description = c.String(),
                        FilePath = c.String(),
                        FileServerId = c.Int(),
                        RemovedFlagUtc = c.DateTime(),
                    },
                annotations: new Dictionary<string, AnnotationValues>
                {
                    { 
                        "DynamicFilter_CampaignImage_RemovedFlagUtc",
                        new AnnotationValues(oldValue: "EntityFramework.DynamicFilters.DynamicFilterDefinition", newValue: null)
                    },
                });
            
            AlterTableAnnotations(
                "dbo.Documents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        FilePath = c.String(nullable: false),
                        FileServerId = c.Int(nullable: false),
                        RemovedFlagUtc = c.DateTime(),
                        BF_Idx = c.Int(nullable: false),
                    },
                annotations: new Dictionary<string, AnnotationValues>
                {
                    { 
                        "DynamicFilter_Document_RemovedFlagUtc",
                        new AnnotationValues(oldValue: "EntityFramework.DynamicFilters.DynamicFilterDefinition", newValue: null)
                    },
                });
            
            AlterTableAnnotations(
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
                        Slug = c.String(maxLength: 300),
                        CreatedById = c.Int(nullable: false),
                        ThumbnailFileServerId = c.Int(),
                        ThumbnailFilePath = c.String(),
                        AccountId = c.Int(),
                        CreatedDateUtc = c.DateTime(nullable: false),
                        RemovedFlagUtc = c.DateTime(),
                    },
                annotations: new Dictionary<string, AnnotationValues>
                {
                    { 
                        "DynamicFilter_Campaign_RemovedFlagUtc",
                        new AnnotationValues(oldValue: "EntityFramework.DynamicFilters.DynamicFilterDefinition", newValue: null)
                    },
                });
            
        }
    }
}
