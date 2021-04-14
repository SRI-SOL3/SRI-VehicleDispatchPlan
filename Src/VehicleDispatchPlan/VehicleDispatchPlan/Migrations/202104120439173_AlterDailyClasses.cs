namespace VehicleDispatchPlan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterDailyClasses : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.T_DailyClasses", "DepartExamRatio", c => c.Double(nullable: false));
            AddColumn("dbo.T_DailyClasses", "OtherVehicleRatio", c => c.Double(nullable: false));
            AddColumn("dbo.T_DailyClasses", "SeminarRatio", c => c.Double(nullable: false));
            AddColumn("dbo.T_DailyClasses", "OtherRatio", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.T_DailyClasses", "OtherRatio");
            DropColumn("dbo.T_DailyClasses", "SeminarRatio");
            DropColumn("dbo.T_DailyClasses", "OtherVehicleRatio");
            DropColumn("dbo.T_DailyClasses", "DepartExamRatio");
        }
    }
}
