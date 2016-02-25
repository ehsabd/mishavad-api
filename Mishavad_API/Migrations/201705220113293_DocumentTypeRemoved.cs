namespace Mishavad_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DocumentTypeRemoved : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Documents", "DocumentTypeId", "dbo.DocumentTypes");
            DropIndex("dbo.Documents", new[] { "DocumentTypeId" });
            DropColumn("dbo.Documents", "DocumentTypeId");
            DropTable("dbo.DocumentTypes");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.DocumentTypes",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Documents", "DocumentTypeId", c => c.Int(nullable: false));
            CreateIndex("dbo.Documents", "DocumentTypeId");
            AddForeignKey("dbo.Documents", "DocumentTypeId", "dbo.DocumentTypes", "Id", cascadeDelete: true);
        }
    }
}
