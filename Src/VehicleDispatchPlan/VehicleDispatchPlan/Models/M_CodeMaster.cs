using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/**
 * コードマスタ
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
    /// コードマスタクラス
    /// </summary>
    [Table("M_CodeMaster")]
    public class M_CodeMaster
    {
        /// <summary>区分</summary>
        [Key]
        [Required]
        [Column(Order = 1)]
        public string Div { get; set; }

        /// <summary>コード</summary>
        [Key]
        [Required]
        [Column(Order = 2)]
        public string Cd { get; set; }

        /// <summary>値</summary>
        public string Value { get; set; }
    }
}