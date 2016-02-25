namespace Mishavad_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IndexCampaginBlogSlug : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Campaigns", "Slug", c => c.String(maxLength: 300));
            AlterColumn("dbo.BlogPosts", "Slug", c => c.String(maxLength: 300));
            CreateIndex("dbo.Campaigns", "Slug", unique: true);
            CreateIndex("dbo.BlogPosts", "Slug", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.BlogPosts", new[] { "Slug" });
            DropIndex("dbo.Campaigns", new[] { "Slug" });
            AlterColumn("dbo.BlogPosts", "Slug", c => c.String());
            AlterColumn("dbo.Campaigns", "Slug", c => c.String());
        }
    }
}
