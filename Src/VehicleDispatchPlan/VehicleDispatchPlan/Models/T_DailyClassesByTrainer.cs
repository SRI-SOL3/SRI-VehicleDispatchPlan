﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

/**
 * 指導員別コマ数
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
    /// 指導員別コマ数クラス
    /// </summary>
    [Table("T_DailyClassesByTrainer")]
    public class T_DailyClassesByTrainer
    {
        /// <summary>対象日</summary>
        [Key]
        [Required]
        [Column(Order = 1)]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DisplayName("対象日")]
        public DateTime? Date { get; set; }

        /// <summary>No</summary>
        [Key]
        [Required]
        [Column(Order = 2)]
        [DisplayName("No")]
        public int No { get; set; }

        /// <summary>指導員名</summary>
        [Required]
        public string TrainerName { get; set; }

        /// <summary>コマ数</summary>
        [Required]
        [DisplayName("コマ数")]
        public double Classes { get; set; }
    }
}