namespace VehicleDispatchPlan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.M_CodeMaster",
                c => new
                    {
                        Div = c.String(nullable: false, maxLength: 128),
                        Cd = c.String(nullable: false, maxLength: 128),
                        Value = c.String(),
                    })
                .PrimaryKey(t => new { t.Div, t.Cd });
            
            CreateTable(
                "dbo.M_EntGrdCalendar",
                c => new
                    {
                        TrainingCourseCd = c.String(nullable: false, maxLength: 128),
                        EntrancePlanDate = c.DateTime(nullable: false),
                        TmpLicencePlanDate = c.DateTime(nullable: false),
                        GraduatePlanDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.TrainingCourseCd, t.EntrancePlanDate });
            
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
                "dbo.M_LodgingFacility",
                c => new
                    {
                        LodgingCd = c.String(nullable: false, maxLength: 128),
                        LodgingName = c.String(nullable: false),
                        TelNo = c.String(),
                        PostalNo = c.String(),
                        Address = c.String(),
                    })
                .PrimaryKey(t => t.LodgingCd);
            
            CreateTable(
                "dbo.T_Trainee",
                c => new
                    {
                        TraineeId = c.Int(nullable: false, identity: true),
                        GroupId = c.Int(nullable: false),
                        TraineeName = c.String(nullable: false),
                        AttendTypeCd = c.String(nullable: false),
                        TrainingCourseCd = c.String(nullable: false),
                        EntrancePlanDate = c.DateTime(nullable: false),
                        TmpLicencePlanDate = c.DateTime(nullable: false),
                        GraduatePlanDate = c.DateTime(nullable: false),
                        LodgingCd = c.String(),
                        AgentName = c.String(),
                    })
                .PrimaryKey(t => t.TraineeId);
            
            CreateTable(
                "dbo.M_TrainingCourse",
                c => new
                    {
                        TrainingCourseCd = c.String(nullable: false, maxLength: 128),
                        TrainingCourseName = c.String(nullable: false),
                        PracticeClassQty = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.TrainingCourseCd);
            
            CreateTable(
                "dbo.M_WorkType",
                c => new
                    {
                        WorkTypeCd = c.String(nullable: false, maxLength: 128),
                        WorkTypeName = c.String(),
                    })
                .PrimaryKey(t => t.WorkTypeCd);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.M_WorkType");
            DropTable("dbo.M_TrainingCourse");
            DropTable("dbo.T_Trainee");
            DropTable("dbo.M_LodgingFacility");
            DropTable("dbo.T_ForecastByWork");
            DropTable("dbo.T_Forecast");
            DropTable("dbo.M_EntGrdCalendar");
            DropTable("dbo.M_CodeMaster");
        }
    }
}
