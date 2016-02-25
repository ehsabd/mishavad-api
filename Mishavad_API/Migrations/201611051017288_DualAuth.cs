namespace Mishavad_API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DualAuth : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.OutgoingFundOrders", name: "ApplicationUserId", newName: "UserId");
            RenameIndex(table: "dbo.OutgoingFundOrders", name: "IX_ApplicationUserId", newName: "IX_UserId");
            CreateTable(
                "dbo.OperationAuths",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OperationId = c.Int(nullable: false),
                        AuthUserId = c.Int(nullable: false),
                        ReasonsForAction = c.String(),
                        Status = c.Int(nullable: false),
                        CreatedDateUtc = c.DateTime(nullable: false),
                        IsDone = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.AuthUserId, cascadeDelete: true)
                .ForeignKey("dbo.Operations", t => t.OperationId, cascadeDelete: true)
                .Index(t => t.OperationId)
                .Index(t => t.AuthUserId);
            
            CreateTable(
                "dbo.Operations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CreatedById = c.Int(nullable: false),
                        Entity = c.String(),
                        Property = c.String(),
                        OldValue = c.String(),
                        NewValue = c.String(),
                        CreatedDateUtc = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedById, cascadeDelete: false)
                .Index(t => t.CreatedById);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.OperationAuths", "OperationId", "dbo.Operations");
            DropForeignKey("dbo.Operations", "CreatedById", "dbo.AspNetUsers");
            DropForeignKey("dbo.OperationAuths", "AuthUserId", "dbo.AspNetUsers");
            DropIndex("dbo.Operations", new[] { "CreatedById" });
            DropIndex("dbo.OperationAuths", new[] { "AuthUserId" });
            DropIndex("dbo.OperationAuths", new[] { "OperationId" });
            DropTable("dbo.Operations");
            DropTable("dbo.OperationAuths");
            RenameIndex(table: "dbo.OutgoingFundOrders", name: "IX_UserId", newName: "IX_ApplicationUserId");
            RenameColumn(table: "dbo.OutgoingFundOrders", name: "UserId", newName: "ApplicationUserId");
        }
    }
}
