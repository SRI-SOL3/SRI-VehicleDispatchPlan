using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/**
 * 受入予測
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
    /// 受入予測クラス
    /// </summary>
    public class T_Forecast
    {
        /// <summary>年</summary>
        [Key]
        [Column(Order = 1)]
        public string Year { get; set; }

        /// <summary>月</summary>
        [Key]
        [Column(Order = 2)]
        public string Month { get; set; }

        /// <summary>学科総時限数数/月</summary>
        [DisplayName("学科総時限数/月")]
        public int LectureClassQty { get; set; }

        /// <summary>合宿比率</summary>
        [Range(0,100)]
        [DisplayName("合宿比率[%]")]
        public double LodgingRatio { get; set; }

        /// <summary>MT比率</summary>
        [Range(0, 100)]
        [DisplayName("MT比率[%]")]
        public double MtRatio { get; set; }
    }
}