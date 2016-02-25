namespace Mishavad_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BlogPostAltSecTitle : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BlogPostAltTitleMaps",
                c => new
                    {
                        BlogPostTitleName = c.String(nullable: false, maxLength: 128),
                        BlogPostId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.BlogPostTitleName, t.BlogPostId })
                .ForeignKey("dbo.BlogPosts", t => t.BlogPostId, cascadeDelete: true)
                .ForeignKey("dbo.BlogPostTitles", t => t.BlogPostTitleName, cascadeDelete: true)
                .Index(t => t.BlogPostTitleName)
                .Index(t => t.BlogPostId);
            
            CreateTable(
                "dbo.BlogPostTitles",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Name);
            
            AddColumn("dbo.BlogPosts", "SecondaryTitle", c => c.String());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BlogPostAltTitleMaps", "BlogPostTitleName", "dbo.BlogPostTitles");
            DropForeignKey("dbo.BlogPostAltTitleMaps", "BlogPostId", "dbo.BlogPosts");
            DropIndex("dbo.BlogPostAltTitleMaps", new[] { "BlogPostId" });
            DropIndex("dbo.BlogPostAltTitleMaps", new[] { "BlogPostTitleName" });
            DropColumn("dbo.BlogPosts", "SecondaryTitle");
            DropTable("dbo.BlogPostTitles");
            DropTable("dbo.BlogPostAltTitleMaps");
        }
    }
}
