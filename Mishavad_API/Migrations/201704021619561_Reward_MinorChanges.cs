namespace Mishavad_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Reward_MinorChanges : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Rewards", "Title", c => c.String(nullable: false));
            AlterColumn("dbo.Rewards", "Description", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Rewards", "Description", c => c.String());
            AlterColumn("dbo.Rewards", "Title", c => c.String());
        }
    }
}
