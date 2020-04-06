namespace VehicleDispatchPlan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DropForecast : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.T_Forecast");
            DropTable("dbo.T_ForecastByWork");
            DropTable("dbo.M_WorkType");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.M_WorkType",
                c => new
                    {
                        WorkTypeCd = c.String(nullable: false, maxLength: 128),
                        WorkTypeName = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.WorkTypeCd);
            
            CreateTable(
                "dbo.T_ForecastByWork",
                c => new
                    {
                        Year = c.String(nullable: false, maxLength: 128),
                        Month = c.String(nullable: false, maxLength: 128),
                        WorkTypeCd = c.String(nullable: false, maxLength: 128),
                        ClassQty = c.Int(nullable: false),
                        InstructorAmt = c.Int(nullable: false),
                        WorkDays = c.Int(nullable: false),
                        NotDrivingRatio = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Year, t.Month, t.WorkTypeCd });
            
            CreateTable(
                "dbo.T_Forecast",
                c => new
                    {
                        Year = c.String(nullable: false, maxLength: 128),
                        Month = c.String(nullable: false, maxLength: 128),
                        LectureClassQty = c.Int(nullable: false),
                        LodgingRatio = c.Double(nullable: false),
                        MtRatio = c.Double(nullable: false),
                    })
                .PrimaryKey(t => new { t.Year, t.Month });
            
        }
    }
}
