namespace Mishavad_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PublicPrivateSettings : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Settings", newName: "PrivateSettings");
            CreateTable(
                "dbo.PublicSettings",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 128),
                        Value = c.String(),
                    })
                .PrimaryKey(t => t.Name);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.PublicSettings");
            RenameTable(name: "dbo.PrivateSettings", newName: "Settings");
        }
    }
}
