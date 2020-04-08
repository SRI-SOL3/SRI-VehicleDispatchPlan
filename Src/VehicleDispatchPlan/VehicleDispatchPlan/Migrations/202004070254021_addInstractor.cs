namespace VehicleDispatchPlan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addInstractor : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.T_DailyClasses", "InstractorViewModel_Id", "dbo.InstractorViewModels");
            DropForeignKey("dbo.T_DailyClassesByTrainer", "InstractorViewModel_Id", "dbo.InstractorViewModels");
            DropIndex("dbo.T_DailyClasses", new[] { "InstractorViewModel_Id" });
            DropIndex("dbo.T_DailyClassesByTrainer", new[] { "InstractorViewModel_Id" });
            DropColumn("dbo.T_DailyClasses", "InstractorViewModel_Id");
            DropColumn("dbo.T_DailyClassesByTrainer", "InstractorViewModel_Id");
            DropTable("dbo.InstractorViewModels");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.InstractorViewModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.T_DailyClassesByTrainer", "InstractorViewModel_Id", c => c.Int());
            AddColumn("dbo.T_DailyClasses", "InstractorViewModel_Id", c => c.Int());
            CreateIndex("dbo.T_DailyClassesByTrainer", "InstractorViewModel_Id");
            CreateIndex("dbo.T_DailyClasses", "InstractorViewModel_Id");
            AddForeignKey("dbo.T_DailyClassesByTrainer", "InstractorViewModel_Id", "dbo.InstractorViewModels", "Id");
            AddForeignKey("dbo.T_DailyClasses", "InstractorViewModel_Id", "dbo.InstractorViewModels", "Id");
        }
    }
}
