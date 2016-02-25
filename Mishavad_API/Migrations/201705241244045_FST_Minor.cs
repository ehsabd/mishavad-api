namespace Mishavad_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FST_Minor : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FileServerTokens", "Resource", c => c.String());
            AddColumn("dbo.FileServerTokens", "EntryId", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FileServerTokens", "EntryId");
            DropColumn("dbo.FileServerTokens", "Resource");
        }
    }
}
