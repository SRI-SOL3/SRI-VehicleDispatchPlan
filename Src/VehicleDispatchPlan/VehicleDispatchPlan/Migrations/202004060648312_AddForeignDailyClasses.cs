namespace VehicleDispatchPlan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddForeignDailyClasses : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.T_DailyClassesByTrainer", "Date");
            AddForeignKey("dbo.T_DailyClassesByTrainer", "Date", "dbo.T_DailyClasses", "Date", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.T_DailyClassesByTrainer", "Date", "dbo.T_DailyClasses");
            DropIndex("dbo.T_DailyClassesByTrainer", new[] { "Date" });
        }
    }
}
