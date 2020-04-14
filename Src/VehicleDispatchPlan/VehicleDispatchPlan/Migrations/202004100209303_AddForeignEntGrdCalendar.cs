﻿namespace VehicleDispatchPlan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddForeignEntGrdCalendar : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.M_EntGrdCalendar", "TrainingCourseCd");
            AddForeignKey("dbo.M_EntGrdCalendar", "TrainingCourseCd", "dbo.M_TrainingCourse", "TrainingCourseCd", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.M_EntGrdCalendar", "TrainingCourseCd", "dbo.M_TrainingCourse");
            DropIndex("dbo.M_EntGrdCalendar", new[] { "TrainingCourseCd" });
        }
    }
}
