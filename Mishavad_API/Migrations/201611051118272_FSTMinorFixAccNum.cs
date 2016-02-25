namespace Mishavad_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FSTMinorFixAccNum : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FileServerTokens", "AccountNumber", c => c.String());
            DropColumn("dbo.FileServerTokens", "UserId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.FileServerTokens", "UserId", c => c.String());
            DropColumn("dbo.FileServerTokens", "AccountNumber");
        }
    }
}
