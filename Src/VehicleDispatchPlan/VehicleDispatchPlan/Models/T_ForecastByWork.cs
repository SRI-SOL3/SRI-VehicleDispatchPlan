using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/**
 * 勤務属性別受入予測
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
    /// 勤務属性別受入予測クラス
    /// </summary>
    public class T_ForecastByWork
    {
        /// <summary>年</summary>
        [Key]
        [Column(Order = 1)]
        public string Year { get; set; }

        /// <summary>月</summary>
        [Key]
        [Column(Order = 2)]
        public string Month { get; set; }

        /// <summary>勤務属性コード</summary>
        [Key]
        [Column(Order = 3)]
        [DisplayName("勤務属性")]
        public string WorkTypeCd { get; set; }

        /// <summary>時限数/日</summary>
        [DisplayName("時限数/日")]
        public int ClassQty { get; set; }

        /// <summary>指導員数/日</summary>
        [DisplayName("指導員数/日")]
        public int InstructorAmt { get; set; }

        /// <summary>出勤日数/月</summary>
        [DisplayName("出勤日数/月")]
        public int WorkDays { get; set; }

        /// <summary>教習外業務比率</summary>
        [Range(0, 100)]
        [DisplayName("教習外業務比率")]
        public int NotDrivingRatio { get; set; }
    }
}