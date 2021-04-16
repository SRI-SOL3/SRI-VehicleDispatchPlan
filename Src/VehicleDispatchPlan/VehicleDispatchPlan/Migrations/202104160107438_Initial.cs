namespace VehicleDispatchPlan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.T_DailyClasses",
                c => new
                    {
                        Date = c.DateTime(nullable: false),
                        DepartExamRatio = c.Double(nullable: false),
                        OtherVehicleRatio = c.Double(nullable: false),
                        SeminarRatio = c.Double(nullable: false),
                        OtherRatio = c.Double(nullable: false),
                        LodgingRatio = c.Double(nullable: false),
                        CommutingRatio = c.Double(nullable: false),
                        LdgMtFstRatio = c.Double(nullable: false),
                        LdgMtSndRatio = c.Double(nullable: false),
                        LdgAtFstRatio = c.Double(nullable: false),
                        LdgAtSndRatio = c.Double(nullable: false),
                        CmtMtFstRatio = c.Double(nullable: false),
                        CmtMtSndRatio = c.Double(nullable: false),
                        CmtAtFstRatio = c.Double(nullable: false),
                        CmtAtSndRatio = c.Double(nullable: false),
                        LdgMtFstClass = c.Double(nullable: false),
                        LdgMtSndClass = c.Double(nullable: false),
                        LdgAtFstClass = c.Double(nullable: false),
                        LdgAtSndClass = c.Double(nullable: false),
                        CmtMtFstClass = c.Double(nullable: false),
                        CmtMtSndClass = c.Double(nullable: false),
                        CmtAtFstClass = c.Double(nullable: false),
                        CmtAtSndClass = c.Double(nullable: false),
                        LdgMtFstClassDay = c.Double(nullable: false),
                        LdgMtSndClassDay = c.Double(nullable: false),
                        LdgAtFstClassDay = c.Double(nullable: false),
                        LdgAtSndClassDay = c.Double(nullable: false),
                        CmtMtFstClassDay = c.Double(nullable: false),
                        CmtMtSndClassDay = c.Double(nullable: false),
                        CmtAtFstClassDay = c.Double(nullable: false),
                        CmtAtSndClassDay = c.Double(nullable: false),
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
                .PrimaryKey(t => new { t.Date, t.No })
                .ForeignKey("dbo.T_DailyClasses", t => t.Date, cascadeDelete: true)
                .Index(t => t.Date);
            
            CreateTable(
                "dbo.M_EntGrdCalendar",
                c => new
                    {
                        TrainingCourseCd = c.String(nullable: false, maxLength: 128),
                        EntrancePlanDate = c.DateTime(nullable: false),
                        TmpLicencePlanDate = c.DateTime(nullable: false),
                        GraduatePlanDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.TrainingCourseCd, t.EntrancePlanDate })
                .ForeignKey("dbo.M_TrainingCourse", t => t.TrainingCourseCd, cascadeDelete: true)
                .Index(t => t.TrainingCourseCd);
            
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
                "dbo.T_TraineeCommuting",
                c => new
                    {
                        TraineeId = c.Int(nullable: false, identity: true),
                        TraineeName = c.String(nullable: false),
                        Gender = c.String(nullable: false),
                        TrainingCourseCd = c.String(nullable: false, maxLength: 128),
                        ReserveDate = c.DateTime(nullable: false),
                        EntrancePlanDate = c.DateTime(nullable: false),
                        TmpLicencePlanDate = c.DateTime(nullable: false),
                        GraduatePlanDate = c.DateTime(nullable: false),
                        SchoolName = c.String(),
                        MiddleSchoolDistrict = c.String(),
                        FormOfAttractingCustomers = c.String(),
                        CancelFlg = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.TraineeId)
                .ForeignKey("dbo.M_TrainingCourse", t => t.TrainingCourseCd, cascadeDelete: true)
                .Index(t => t.TrainingCourseCd);
            
            CreateTable(
                "dbo.T_TraineeLodging",
                c => new
                    {
                        TraineeId = c.Int(nullable: false, identity: true),
                        GroupId = c.Int(nullable: false),
                        TraineeName = c.String(nullable: false),
                        Gender = c.String(nullable: false),
                        TrainingCourseCd = c.String(nullable: false, maxLength: 128),
                        ReserveDate = c.DateTime(nullable: false),
                        EntrancePlanDate = c.DateTime(nullable: false),
                        TmpLicencePlanDate = c.DateTime(nullable: false),
                        GraduatePlanDate = c.DateTime(nullable: false),
                        LodgingCd = c.String(maxLength: 128),
                        AgentName = c.String(),
                        SchoolName = c.String(),
                        CancelFlg = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.TraineeId)
                .ForeignKey("dbo.M_LodgingFacility", t => t.LodgingCd)
                .ForeignKey("dbo.M_TrainingCourse", t => t.TrainingCourseCd, cascadeDelete: true)
                .Index(t => t.TrainingCourseCd)
                .Index(t => t.LodgingCd);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.T_TraineeLodging", "TrainingCourseCd", "dbo.M_TrainingCourse");
            DropForeignKey("dbo.T_TraineeLodging", "LodgingCd", "dbo.M_LodgingFacility");
            DropForeignKey("dbo.T_TraineeCommuting", "TrainingCourseCd", "dbo.M_TrainingCourse");
            DropForeignKey("dbo.M_EntGrdCalendar", "TrainingCourseCd", "dbo.M_TrainingCourse");
            DropForeignKey("dbo.T_DailyClassesByTrainer", "Date", "dbo.T_DailyClasses");
            DropIndex("dbo.T_TraineeLodging", new[] { "LodgingCd" });
            DropIndex("dbo.T_TraineeLodging", new[] { "TrainingCourseCd" });
            DropIndex("dbo.T_TraineeCommuting", new[] { "TrainingCourseCd" });
            DropIndex("dbo.M_EntGrdCalendar", new[] { "TrainingCourseCd" });
            DropIndex("dbo.T_DailyClassesByTrainer", new[] { "Date" });
            DropTable("dbo.T_TraineeLodging");
            DropTable("dbo.T_TraineeCommuting");
            DropTable("dbo.M_LodgingFacility");
            DropTable("dbo.M_TrainingCourse");
            DropTable("dbo.M_EntGrdCalendar");
            DropTable("dbo.T_DailyClassesByTrainer");
            DropTable("dbo.T_DailyClasses");
        }
    }
}
