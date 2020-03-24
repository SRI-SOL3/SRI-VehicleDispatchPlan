using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/**
 * 教習コースマスタ
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
    /// 教習コースクラス
    /// </summary>
    [Table("M_TrainingCourse")]
    public class M_TrainingCourse
    {
        /// <summary>教習コースコード</summary>
        [Key]
        public string TrainingCourseCd { get; set; }

        /// <summary>教習コース名</summary>
        [Required]
        public string TrainingCourseName { get; set; }

        /// <summary>実車コマ数</summary>
        [Required]
        public int PracticeClassQty { get; set; }
    }
}