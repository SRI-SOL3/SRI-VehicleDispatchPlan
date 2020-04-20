namespace VehicleDispatchPlan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTraineeCommuting : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.T_TraineeCommuting",
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
                        AgentName = c.String(),
                        SchoolName = c.String(),
                        MiddleSchoolDistrict = c.String(),
                        FormOfAttractingCustomers = c.String(),
                        CancelFlg = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.TraineeId)
                .ForeignKey("dbo.M_TrainingCourse", t => t.TrainingCourseCd, cascadeDelete: true)
                .Index(t => t.TrainingCourseCd);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.T_TraineeCommuting", "TrainingCourseCd", "dbo.M_TrainingCourse");
            DropIndex("dbo.T_TraineeCommuting", new[] { "TrainingCourseCd" });
            DropTable("dbo.T_TraineeCommuting");
        }
    }
}
