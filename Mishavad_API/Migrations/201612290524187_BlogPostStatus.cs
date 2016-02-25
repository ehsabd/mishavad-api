namespace Mishavad_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BlogPostStatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BlogPosts", "Status", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.BlogPosts", "Status");
        }
    }
}
