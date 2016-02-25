namespace Mishavad_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BlogPostFootNote : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BlogPosts", "FootNote", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.BlogPosts", "FootNote");
        }
    }
}
