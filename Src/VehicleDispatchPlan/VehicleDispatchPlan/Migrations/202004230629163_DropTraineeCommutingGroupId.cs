namespace VehicleDispatchPlan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DropTraineeCommutingGroupId : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.T_TraineeCommuting", "GroupId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.T_TraineeCommuting", "GroupId", c => c.Int(nullable: false));
        }
    }
}
