namespace Mishavad_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VisitModels1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.VisitReferrers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IP = c.Binary(nullable: false, maxLength: 16),
                        Referrer = c.String(),
                        Url = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.VisitReferrers");
        }
    }
}
