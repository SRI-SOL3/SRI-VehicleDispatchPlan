﻿using System.Data.Entity;

/**
 * データベースコンテキスト
 *
 * @author t-murayama
 * @version 1.0
 * ----------------------------------
 * 2020/03/01 t-murayama 新規作成
 *
 */
namespace VehicleDispatchPlan.Models
{
    public class MyDatabaseContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx

        public MyDatabaseContext() : base("name=MyDbConnection")
        {
        }

        // 教習生
        public System.Data.Entity.DbSet<Models.T_Trainee> Trainee { get; set; }
        // 受入予測
        public System.Data.Entity.DbSet<Models.T_Forecast> Forecast { get; set; }
        // 勤務属性別受入予測
        public System.Data.Entity.DbSet<Models.T_ForecastByWork> ForecastByWork { get; set; }
        // 日別コマ数
        public System.Data.Entity.DbSet<Models.T_DailyClasses> DailyClasses { get; set; }
        // 指導員別コマ数
        public System.Data.Entity.DbSet<Models.T_DailyClassesByTrainer> DailyClassesByTrainer { get; set; }
        // コードマスタ
        public System.Data.Entity.DbSet<Models.M_AttendType> AttendType { get; set; }
        // 勤務属性
        public System.Data.Entity.DbSet<Models.M_WorkType> WorkType { get; set; }
        // 教習コース
        public System.Data.Entity.DbSet<Models.M_TrainingCourse> TrainingCourse { get; set; }
        // 宿泊施設
        public System.Data.Entity.DbSet<Models.M_LodgingFacility> LodgingFacility { get; set; }
        // 入卒カレンダー
        public System.Data.Entity.DbSet<Models.M_EntGrdCalendar> EntGrdCalendar { get; set; }
    }
}