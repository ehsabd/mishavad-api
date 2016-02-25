namespace Mishavad_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DocumentModels : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.DocFiles", "FileServerId", "dbo.FileServers");
            DropForeignKey("dbo.DocFiles", "Document_Id", "dbo.Documents");
            DropForeignKey("dbo.Documents", "CampaignId", "dbo.Campaigns");
            DropIndex("dbo.Documents", new[] { "CampaignId" });
            DropIndex("dbo.DocFiles", new[] { "FileServerId" });
            DropIndex("dbo.DocFiles", new[] { "Document_Id" });
            CreateTable(
                "dbo.CampaignDocumentMaps",
                c => new
                    {
                        DocumentId = c.Int(nullable: false),
                        CampaignId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.DocumentId)
                .ForeignKey("dbo.Campaigns", t => t.CampaignId, cascadeDelete: true)
                .ForeignKey("dbo.Documents", t => t.DocumentId)
                .Index(t => t.DocumentId)
                .Index(t => t.CampaignId);
            
            CreateTable(
                "dbo.DocumentTypes",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserDocumentMaps",
                c => new
                    {
                        DocumentId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.DocumentId)
                .ForeignKey("dbo.Documents", t => t.DocumentId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.DocumentId)
                .Index(t => t.UserId);
            
            AddColumn("dbo.Documents", "DocumentTypeId", c => c.Int(nullable: false));
            AddColumn("dbo.Documents", "FilePath", c => c.String(nullable: false));
            AddColumn("dbo.Documents", "FileServerId", c => c.Int(nullable: false));
            AddColumn("dbo.Documents", "Description", c => c.String());
            AddColumn("dbo.Documents", "BF_Idx", c => c.Int(nullable: false));
            CreateIndex("dbo.Documents", "DocumentTypeId");
            CreateIndex("dbo.Documents", "FileServerId");
            AddForeignKey("dbo.Documents", "FileServerId", "dbo.FileServers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Documents", "DocumentTypeId", "dbo.DocumentTypes", "Id", cascadeDelete: true);
            DropColumn("dbo.Documents", "CampaignId");
            DropColumn("dbo.Documents", "DocCategoryId");
            DropColumn("dbo.Documents", "Subject");
            DropColumn("dbo.Documents", "Note");
            DropTable("dbo.DocFiles");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.DocFiles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FilePath = c.String(),
                        FileServerId = c.Int(),
                        Document_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Documents", "Note", c => c.String());
            AddColumn("dbo.Documents", "Subject", c => c.String());
            AddColumn("dbo.Documents", "DocCategoryId", c => c.Int(nullable: false));
            AddColumn("dbo.Documents", "CampaignId", c => c.Int(nullable: false));
            DropForeignKey("dbo.UserDocumentMaps", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.UserDocumentMaps", "DocumentId", "dbo.Documents");
            DropForeignKey("dbo.CampaignDocumentMaps", "DocumentId", "dbo.Documents");
            DropForeignKey("dbo.Documents", "DocumentTypeId", "dbo.DocumentTypes");
            DropForeignKey("dbo.Documents", "FileServerId", "dbo.FileServers");
            DropForeignKey("dbo.CampaignDocumentMaps", "CampaignId", "dbo.Campaigns");
            DropIndex("dbo.UserDocumentMaps", new[] { "UserId" });
            DropIndex("dbo.UserDocumentMaps", new[] { "DocumentId" });
            DropIndex("dbo.Documents", new[] { "FileServerId" });
            DropIndex("dbo.Documents", new[] { "DocumentTypeId" });
            DropIndex("dbo.CampaignDocumentMaps", new[] { "CampaignId" });
            DropIndex("dbo.CampaignDocumentMaps", new[] { "DocumentId" });
            DropColumn("dbo.Documents", "BF_Idx");
            DropColumn("dbo.Documents", "Description");
            DropColumn("dbo.Documents", "FileServerId");
            DropColumn("dbo.Documents", "FilePath");
            DropColumn("dbo.Documents", "DocumentTypeId");
            DropTable("dbo.UserDocumentMaps");
            DropTable("dbo.DocumentTypes");
            DropTable("dbo.CampaignDocumentMaps");
            CreateIndex("dbo.DocFiles", "Document_Id");
            CreateIndex("dbo.DocFiles", "FileServerId");
            CreateIndex("dbo.Documents", "CampaignId");
            AddForeignKey("dbo.Documents", "CampaignId", "dbo.Campaigns", "Id", cascadeDelete: true);
            AddForeignKey("dbo.DocFiles", "Document_Id", "dbo.Documents", "Id");
            AddForeignKey("dbo.DocFiles", "FileServerId", "dbo.FileServers", "Id");
        }
    }
}
