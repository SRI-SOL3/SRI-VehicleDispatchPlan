using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

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
        [Column(Order = 1)]
        [DisplayName("教習コース")]
        public string TrainingCourseCd { get; set; }

        /// <summary>入学予定日</summary>
        [Key]
        [Column(Order = 2)]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DisplayName("入学予定日")]
        public DateTime EntrancePlanDate { get; set; }

        /// <summary>仮免予定日</summary>
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DisplayName("仮免予定日")]
        public DateTime TmpLicencePlanDate { get; set; }

        /// <summary>卒業予定日</summary>
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DisplayName("卒業予定日")]
        public DateTime GraduatePlanDate { get; set; }
    }
}