namespace VehicleDispatchPlan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DropTrainee : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.T_Trainee", "AttendTypeCd", "dbo.M_AttendType");
            DropForeignKey("dbo.T_Trainee", "LodgingCd", "dbo.M_LodgingFacility");
            DropForeignKey("dbo.T_Trainee", "TrainingCourseCd", "dbo.M_TrainingCourse");
            DropIndex("dbo.T_Trainee", new[] { "AttendTypeCd" });
            DropIndex("dbo.T_Trainee", new[] { "TrainingCourseCd" });
            DropIndex("dbo.T_Trainee", new[] { "LodgingCd" });
            DropTable("dbo.M_AttendType");
            DropTable("dbo.T_Trainee");
        }
        
        public override void Down()
        {
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
                .PrimaryKey(t => t.TraineeId);
            
            CreateTable(
                "dbo.M_AttendType",
                c => new
                    {
                        AttendTypeCd = c.String(nullable: false, maxLength: 128),
                        AttendTypeName = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.AttendTypeCd);
            
            CreateIndex("dbo.T_Trainee", "LodgingCd");
            CreateIndex("dbo.T_Trainee", "TrainingCourseCd");
            CreateIndex("dbo.T_Trainee", "AttendTypeCd");
            AddForeignKey("dbo.T_Trainee", "TrainingCourseCd", "dbo.M_TrainingCourse", "TrainingCourseCd", cascadeDelete: true);
            AddForeignKey("dbo.T_Trainee", "LodgingCd", "dbo.M_LodgingFacility", "LodgingCd");
            AddForeignKey("dbo.T_Trainee", "AttendTypeCd", "dbo.M_AttendType", "AttendTypeCd", cascadeDelete: true);
        }
    }
}
