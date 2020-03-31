namespace VehicleDispatchPlan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDailyClasses : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.T_DailyClasses",
                c => new
                    {
                        Date = c.DateTime(nullable: false),
                        DefaultClasses = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Date);
            
            CreateTable(
                "dbo.T_DailyClassesByTrainer",
                c => new
                    {
                        Date = c.DateTime(nullable: false),
                        No = c.Int(nullable: false),
                        TrainerName = c.String(nullable: false),
                        Classes = c.Double(nullable: false),
                    })
                .PrimaryKey(t => new { t.Date, t.No });
            
        }
        
        public override void Down()
        {
            DropTable("dbo.T_DailyClassesByTrainer");
            DropTable("dbo.T_DailyClasses");
        }
    }
}
