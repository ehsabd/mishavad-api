namespace Mishavad_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BlogTitle : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BlogPosts", "Title", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.BlogPosts", "Title");
        }
    }
}
