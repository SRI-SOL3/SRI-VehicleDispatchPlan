using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

/**
 * 入卒カレンダーマスタ
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
    /// 入卒カレンダークラス
    /// </summary>
    [Table("M_EntGrdCalendar")]
    public class M_EntGrdCalendar
    {
        /// <summary>教習コース<summary>
        [Key]
        [Required]
        [Column(Order = 1)]
        [DisplayName("教習コース")]
        public string TrainingCourseCd { get; set; }

        /// <summary>[非DB項目]教習コース名<summary>
        [NotMapped]
        [DisplayName("教習コース")]
        public string TrainingCourseName { get; set; }

        /// <summary>[非DB項目]教習コース選択肢<summary>
        [NotMapped]
        public SelectList SelectTrainingCourse { get; set; }

        /// <summary>入校予定日</summary>
        [Key]
        [Required]
        [Column(Order = 2)]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DisplayName("入校予定日")]
        public DateTime? EntrancePlanDate { get; set; }

        /// <summary>仮免予定日</summary>
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DisplayName("仮免予定日")]
        public DateTime? TmpLicencePlanDate { get; set; }

        /// <summary>卒業予定日</summary>
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DisplayName("卒業予定日")]
        public DateTime? GraduatePlanDate { get; set; }
    }
}