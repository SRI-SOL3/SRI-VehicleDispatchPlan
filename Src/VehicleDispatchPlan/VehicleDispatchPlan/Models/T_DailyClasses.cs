using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/**
 * 日別予測条件
 *
 * @author t-murayama
 * @version 1.0
 * ----------------------------------
 * 2020/03/01 t-murayama 新規作成
 * 2021/04/12 t-murayama 20210416リリース対応(ver.1.2)
 */
namespace VehicleDispatchPlan.Models
{
    /// <summary>
    /// 日別予測条件クラス
    /// </summary>
    [Table("T_DailyClasses")]
    public class T_DailyClasses
    {
        /// <summary>対象日</summary>
        [Key]
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DisplayName("対象日")]
        public DateTime? Date { get; set; }

        // [20210416リリース対応] Add Start 教習外コマ数比率の追加
        #region 教習外コマ数比率
        /// <summary>学科・検定比率[%]</summary>
        [Required]
        [Range(0, 100)]
        [DisplayName("学科・検定比率[%]")]
        public double DepartExamRatio { get; set; }

        /// <summary>他車種比率[%]</summary>
        [Required]
        [Range(0, 100)]
        [DisplayName("他車種比率[%]")]
        public double OtherVehicleRatio { get; set; }

        /// <summary>講習比率[%]</summary>
        [Required]
        [Range(0, 100)]
        [DisplayName("講習比率[%]")]
        public double SeminarRatio { get; set; }

        /// <summary>その他[%]</summary>
        [Required]
        [Range(0, 100)]
        [DisplayName("その他[%]")]
        public double OtherRatio { get; set; }
        #endregion
        // [20210416リリース対応] Add End

        #region 合宿／通学比率
        /// <summary>合宿比率[%]</summary>
        [Required]
        [Range(0, 100)]
        [DisplayName("合宿比率[%]")]
        public double LodgingRatio { get; set; }

        /// <summary>通学比率[%]</summary>
        [Required]
        [Range(0, 100)]
        [DisplayName("通学比率[%]")]
        public double CommutingRatio { get; set; }
        #endregion

        #region 『段階別』による教習生の在籍比率
        /// <summary>【合宿】MT一段階比率[%]</summary>
        [Required]
        [Range(0, 100)]
        [DisplayName("MT一段階比率[%]")]
        public double LdgMtFstRatio { get; set; }

        /// <summary>【合宿】MT二段階比率[%]</summary>
        [Required]
        [Range(0, 100)]
        [DisplayName("MT二段階比率[%]")]
        public double LdgMtSndRatio { get; set; }

        /// <summary>【合宿】AT一段階比率[%]</summary>
        [Required]
        [Range(0, 100)]
        [DisplayName("AT一段階比率[%]")]
        public double LdgAtFstRatio { get; set; }

        /// <summary>【合宿】AT二段階比率[%]</summary>
        [Required]
        [Range(0, 100)]
        [DisplayName("AT二段階比率[%]")]
        public double LdgAtSndRatio { get; set; }

        /// <summary>【通学】MT一段階比率[%]</summary>
        [Required]
        [Range(0, 100)]
        [DisplayName("MT一段階比率[%]")]
        public double CmtMtFstRatio { get; set; }

        /// <summary>【通学】MT二段階比率[%]</summary>
        [Required]
        [Range(0, 100)]
        [DisplayName("MT二段階比率[%]")]
        public double CmtMtSndRatio { get; set; }

        /// <summary>【通学】AT一段階比率[%]</summary>
        [Required]
        [Range(0, 100)]
        [DisplayName("AT一段階比率[%]")]
        public double CmtAtFstRatio { get; set; }

        /// <summary>【通学】AT二段階比率[%]</summary>
        [Required]
        [Range(0, 100)]
        [DisplayName("AT二段階比率[%]")]
        public double CmtAtSndRatio { get; set; }
        #endregion

        #region 教習生一人が『卒業までに必要な』実車教習のコマ数
        /// <summary>【合宿】MT一段階コマ数</summary>
        [Required]
        [DisplayName("MT一段階コマ数")]
        public double LdgMtFstClass { get; set; }

        /// <summary>【合宿】MT二段階コマ数</summary>
        [Required]
        [DisplayName("MT二段階コマ数")]
        public double LdgMtSndClass { get; set; }

        /// <summary>【合宿】AT一段階コマ数</summary>
        [Required]
        [DisplayName("AT一段階コマ数")]
        public double LdgAtFstClass { get; set; }

        /// <summary>【合宿】AT二段階コマ数</summary>
        [Required]
        [DisplayName("AT二段階コマ数")]
        public double LdgAtSndClass { get; set; }

        /// <summary>【通学】MT一段階コマ数</summary>
        [Required]
        [DisplayName("MT一段階コマ数")]
        public double CmtMtFstClass { get; set; }

        /// <summary>【通学】MT二段階コマ数</summary>
        [Required]
        [DisplayName("MT二段階コマ数")]
        public double CmtMtSndClass { get; set; }

        /// <summary>【通学】AT一段階コマ数</summary>
        [Required]
        [DisplayName("AT一段階コマ数")]
        public double CmtAtFstClass { get; set; }

        /// <summary>【通学】AT二段階コマ数</summary>
        [Required]
        [DisplayName("AT二段階コマ数")]
        public double CmtAtSndClass { get; set; }
        #endregion

        #region 教習生一人が『一日に受ける』実車教習のコマ数
        /// <summary>【合宿】MT一段階コマ数/日</summary>
        [Required]
        [DisplayName("MT一段階コマ数/日")]
        public double LdgMtFstClassDay { get; set; }

        /// <summary>【合宿】MT二段階コマ数/日</summary>
        [Required]
        [DisplayName("MT二段階コマ数/日")]
        public double LdgMtSndClassDay { get; set; }

        /// <summary>【合宿】AT一段階コマ数/日</summary>
        [Required]
        [DisplayName("AT一段階コマ数/日")]
        public double LdgAtFstClassDay { get; set; }

        /// <summary>【合宿】AT二段階コマ数/日</summary>
        [Required]
        [DisplayName("AT二段階コマ数/日")]
        public double LdgAtSndClassDay { get; set; }

        /// <summary>【通学】MT一段階コマ数/日</summary>
        [Required]
        [DisplayName("MT一段階コマ数/日")]
        public double CmtMtFstClassDay { get; set; }

        /// <summary>【通学】MT二段階コマ数/日</summary>
        [Required]
        [DisplayName("MT二段階コマ数/日")]
        public double CmtMtSndClassDay { get; set; }

        /// <summary>【通学】AT一段階コマ数/日</summary>
        [Required]
        [DisplayName("AT一段階コマ数/日")]
        public double CmtAtFstClassDay { get; set; }

        /// <summary>【通学】AT二段階コマ数/日</summary>
        [Required]
        [DisplayName("AT二段階コマ数/日")]
        public double CmtAtSndClassDay { get; set; }
        #endregion
    }
}