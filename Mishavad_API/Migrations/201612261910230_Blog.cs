namespace Mishavad_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Blog : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BlogPostCategories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.BlogPosts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Slug = c.String(),
                        CreatedById = c.Int(),
                        Content = c.String(),
                        Tags = c.String(),
                        ImageFileServerId = c.Int(),
                        ImageFilePath = c.String(),
                        BlogPostCategoryId = c.Int(),
                        MembersOnly = c.Boolean(nullable: false),
                        CreatedDateUtc = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BlogPostCategories", t => t.BlogPostCategoryId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedById)
                .ForeignKey("dbo.FileServers", t => t.ImageFileServerId)
                .Index(t => t.CreatedById)
                .Index(t => t.ImageFileServerId)
                .Index(t => t.BlogPostCategoryId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BlogPosts", "ImageFileServerId", "dbo.FileServers");
            DropForeignKey("dbo.BlogPosts", "CreatedById", "dbo.AspNetUsers");
            DropForeignKey("dbo.BlogPosts", "BlogPostCategoryId", "dbo.BlogPostCategories");
            DropIndex("dbo.BlogPosts", new[] { "BlogPostCategoryId" });
            DropIndex("dbo.BlogPosts", new[] { "ImageFileServerId" });
            DropIndex("dbo.BlogPosts", new[] { "CreatedById" });
            DropTable("dbo.BlogPosts");
            DropTable("dbo.BlogPostCategories");
        }
    }
}
