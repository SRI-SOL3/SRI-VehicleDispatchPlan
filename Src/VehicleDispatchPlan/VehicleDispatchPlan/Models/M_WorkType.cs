using System.ComponentModel.DataAnnotations;

/**
 * 勤務属性マスタ
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
    /// 勤務属性クラス
    /// </summary>
    public class M_WorkType
    {
        /// <summary>勤務属性コード</summary>
        [Key]
        [Required]
        public string WorkTypeCd { get; set; }

        /// <summary>勤務属性名</summary>
        [Required]
        public string WorkTypeName { get; set; }
    }
}