namespace Mishavad_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Campaign_RemovedFlag : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Campaigns", "RemovedFlag", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Campaigns", "RemovedFlag");
        }
    }
}
