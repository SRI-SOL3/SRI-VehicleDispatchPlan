﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/**
 * 日別コマ数
 *
 * @author t-murayama
 * @version 1.0
 * ----------------------------------
 * 2020/03/01 t-murayama 新規作成
 *
 */
namespace VehicleDispatchPlan.Models
{
    /// <summary>
    /// 日別コマ数クラス
    /// </summary>
    [Table("T_DailyClasses")]
    public class T_DailyClasses
    {
        /// <summary>対象日</summary>
        [Key]
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DisplayName("対象日")]
        public DateTime? Date { get; set; }

        /// <summary>標準コマ数</summary>
        [Required]
        [DisplayName("標準コマ数")]
        public double DefaultClasses { get; set; }
    }
}