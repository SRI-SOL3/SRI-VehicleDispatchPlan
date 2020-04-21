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
 *
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

        #region 『段階別』による教習生の在籍比率
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
        #endregion

        #region 教習生一人が『卒業までに必要な』実車教習のコマ数
        /// <summary>【合宿】AT一段階コマ数</summary>
        [Required]
        [DisplayName("AT一段階コマ数")]
        public double LdgAtFstClass { get; set; }

        /// <summary>【合宿】AT二段階コマ数</summary>
        [Required]
        [DisplayName("AT二段階コマ数")]
        public double LdgAtSndClass { get; set; }

        /// <summary>【合宿】MT一段階コマ数</summary>
        [Required]
        [DisplayName("MT一段階コマ数")]
        public double LdgMtFstClass { get; set; }

        /// <summary>【合宿】MT二段階コマ数</summary>
        [Required]
        [DisplayName("MT二段階コマ数")]
        public double LdgMtSndClass { get; set; }

        /// <summary>【通学】AT一段階コマ数</summary>
        [Required]
        [DisplayName("AT一段階コマ数")]
        public double CmtAtFstClass { get; set; }

        /// <summary>【通学】AT二段階コマ数</summary>
        [Required]
        [DisplayName("AT二段階コマ数")]
        public double CmtAtSndClass { get; set; }

        /// <summary>【通学】MT一段階コマ数</summary>
        [Required]
        [DisplayName("MT一段階コマ数")]
        public double CmtMtFstClass { get; set; }

        /// <summary>【通学】MT二段階コマ数</summary>
        [Required]
        [DisplayName("MT二段階コマ数")]
        public double CmtMtSndClass { get; set; }
        #endregion

        #region 教習生一人が『一日に受ける』実車教習のコマ数
        /// <summary>【合宿】AT一段階コマ数/日</summary>
        [Required]
        [DisplayName("AT一段階コマ数/日")]
        public double LdgAtFstClassDay { get; set; }

        /// <summary>【合宿】AT二段階コマ数/日</summary>
        [Required]
        [DisplayName("AT二段階コマ数/日")]
        public double LdgAtSndClassDay { get; set; }

        /// <summary>【合宿】MT一段階コマ数/日</summary>
        [Required]
        [DisplayName("MT一段階コマ数/日")]
        public double LdgMtFstClassDay { get; set; }

        /// <summary>【合宿】MT二段階コマ数/日</summary>
        [Required]
        [DisplayName("MT二段階コマ数/日")]
        public double LdgMtSndClassDay { get; set; }

        /// <summary>【通学】AT一段階コマ数/日</summary>
        [Required]
        [DisplayName("AT一段階コマ数/日")]
        public double CmtAtFstClassDay { get; set; }

        /// <summary>【通学】AT二段階コマ数/日</summary>
        [Required]
        [DisplayName("AT二段階コマ数/日")]
        public double CmtAtSndClassDay { get; set; }

        /// <summary>【通学】MT一段階コマ数/日</summary>
        [Required]
        [DisplayName("MT一段階コマ数/日")]
        public double CmtMtFstClassDay { get; set; }

        /// <summary>【通学】MT二段階コマ数/日</summary>
        [Required]
        [DisplayName("MT二段階コマ数/日")]
        public double CmtMtSndClassDay { get; set; }
        #endregion
    }
}