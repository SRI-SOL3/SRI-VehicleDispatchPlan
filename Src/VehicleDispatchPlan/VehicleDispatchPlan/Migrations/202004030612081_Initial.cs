namespace VehicleDispatchPlan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.M_AttendType",
                c => new
                    {
                        AttendTypeCd = c.String(nullable: false, maxLength: 128),
                        AttendTypeName = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.AttendTypeCd);
            
            CreateTable(
                "dbo.T_DailyClasses",
                c => new
                    {
                        Date = c.DateTime(nullable: false),
                        DefaultClasses = c.Double(nullable: false),
                        AtRatio = c.Double(nullable: false),
                        FirstRatio = c.Double(nullable: false),
                        LodgingRatio = c.Double(nullable: false),
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
                        AttendTypeCd = c.String(nullable: false, maxLength: 128),
                        TrainingCourseCd = c.String(nullable: false, maxLength: 128),
                        EntrancePlanDate = c.DateTime(nullable: false),
                        TmpLicencePlanDate = c.DateTime(nullable: false),
                        GraduatePlanDate = c.DateTime(nullable: false),
                        LodgingCd = c.String(maxLength: 128),
                        AgentName = c.String(),
                    })
                .PrimaryKey(t => t.TraineeId)
                .ForeignKey("dbo.M_AttendType", t => t.AttendTypeCd, cascadeDelete: true)
                .ForeignKey("dbo.M_LodgingFacility", t => t.LodgingCd)
                .ForeignKey("dbo.M_TrainingCourse", t => t.TrainingCourseCd, cascadeDelete: true)
                .Index(t => t.AttendTypeCd)
                .Index(t => t.TrainingCourseCd)
                .Index(t => t.LodgingCd);
            
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
                        WorkTypeName = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.WorkTypeCd);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.T_Trainee", "TrainingCourseCd", "dbo.M_TrainingCourse");
            DropForeignKey("dbo.T_Trainee", "LodgingCd", "dbo.M_LodgingFacility");
            DropForeignKey("dbo.T_Trainee", "AttendTypeCd", "dbo.M_AttendType");
            DropIndex("dbo.T_Trainee", new[] { "LodgingCd" });
            DropIndex("dbo.T_Trainee", new[] { "TrainingCourseCd" });
            DropIndex("dbo.T_Trainee", new[] { "AttendTypeCd" });
            DropTable("dbo.M_WorkType");
            DropTable("dbo.M_TrainingCourse");
            DropTable("dbo.T_Trainee");
            DropTable("dbo.M_LodgingFacility");
            DropTable("dbo.T_ForecastByWork");
            DropTable("dbo.T_Forecast");
            DropTable("dbo.M_EntGrdCalendar");
            DropTable("dbo.T_DailyClassesByTrainer");
            DropTable("dbo.T_DailyClasses");
            DropTable("dbo.M_AttendType");
        }
    }
}
