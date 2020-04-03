using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/**
 * 宿泊施設マスタ
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
    /// 宿泊施設クラス
    /// </summary>
    [Table("M_LodgingFacility")]
    public class M_LodgingFacility
    {
        /// <summary>宿泊施設コード</summary>
        [Key]
        [Required]
        [DisplayName("宿泊施設コード")]
        public string LodgingCd { get; set; }

        /// <summary>宿泊施設名</summary>
        [Required]
        [DisplayName("宿泊施設")]
        public string LodgingName { get; set; }

        /// <summary>電話番号</summary>
        [DisplayName("電話番号")]
        public string TelNo { get; set; }

        /// <summary>郵便番号</summary>
        [DisplayName("郵便番号")]
        public string PostalNo { get; set; }

        /// <summary>住所</summary>
        [DisplayName("住所")]
        public string Address { get; set; }
    }
}