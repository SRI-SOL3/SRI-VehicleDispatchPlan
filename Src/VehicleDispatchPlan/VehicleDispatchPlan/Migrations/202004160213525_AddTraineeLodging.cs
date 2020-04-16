namespace VehicleDispatchPlan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTraineeLodging : DbMigration
    {
        public override void Up()
        {
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
            DropIndex("dbo.T_TraineeLodging", new[] { "LodgingCd" });
            DropIndex("dbo.T_TraineeLodging", new[] { "TrainingCourseCd" });
            DropTable("dbo.T_TraineeLodging");
        }
    }
}
