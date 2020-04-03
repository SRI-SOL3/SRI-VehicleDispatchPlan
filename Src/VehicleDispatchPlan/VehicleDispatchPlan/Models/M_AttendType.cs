using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
/**
* 通学種別マスタ
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
    /// 通学種別クラス
    /// </summary>
    [Table("M_AttendType")]
    public class M_AttendType
    {
        /// <summary>通学種別コード</summary>
        [Key]
        [Required]
        [DisplayName("通学種別コード")]
        public string AttendTypeCd { get; set; }

        /// <summary>通学種別名</summary>
        [Required]
        [DisplayName("通学種別")]
        public string AttendTypeName { get; set; }
    }
}