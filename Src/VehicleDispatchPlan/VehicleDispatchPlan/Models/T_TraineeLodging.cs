using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

/**
 * 合宿教習生
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
    /// 合宿教習生クラス
    /// </summary>
    public class T_TraineeLodging
    {
        /// <summary>教習生ID<summary>
        [Key]
        [Required]
        [DisplayName("教習生ID")]
        public int TraineeId { get; set; }

        /// <summary>グループID<summary>
        [Required]
        [DisplayName("グループID")]
        public int GroupId { get; set; }

        /// <summary>名前<summary>
        [Required]
        [DisplayName("教習者名")]
        public string TraineeName { get; set; }

        /// <summary>性別（M:男 / F:女）<summary>
        [Required]
        [DisplayName("性別")]
        public string Gender { get; set; }

        /// <summary>教習コース<summary>
        [Required]
        [DisplayName("教習コース")]
        public string TrainingCourseCd { get; set; }

        /// <summary>[外部キー]教習コースマスタ</summary>
        [ForeignKey("TrainingCourseCd")]
        public virtual M_TrainingCourse TrainingCourse { get; set; }

        /// <summary>[非DB項目]教習コース選択肢<summary>
        [NotMapped]
        public SelectList SelectTrainingCourse { get; set; }

        /// <summary>申込日<summary>
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DisplayName("申込日")]
        public DateTime? ReserveDate { get; set; }

        /// <summary>入校予定日<summary>
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DisplayName("入校予定日")]
        public DateTime? EntrancePlanDate { get; set; }

        /// <summary>仮免予定日<summary>
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DisplayName("仮免予定日")]
        public DateTime? TmpLicencePlanDate { get; set; }

        /// <summary>卒業予定日<summary>
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DisplayName("卒業予定日")]
        public DateTime? GraduatePlanDate { get; set; }

        /// <summary>宿泊施設<summary>
        [DisplayName("宿泊施設")]
        public string LodgingCd { get; set; }

        /// <summary>[外部キー]宿泊施設マスタ</summary>
        [ForeignKey("LodgingCd")]
        public virtual M_LodgingFacility LodgingFacility { get; set; }

        /// <summary>[非DB項目]宿泊施設選択肢<summary>
        [NotMapped]
        public SelectList SelectLodging { get; set; }

        /// <summary>エージェント<summary>
        [DisplayName("エージェント")]
        public string AgentName { get; set; }

        /// <summary>学校名<summary>
        [DisplayName("学校名")]
        public string SchoolName { get; set; }

        /// <summary>キャンセルフラグ</summary>
        [DisplayName("キャンセル")]
        public bool CancelFlg { get; set; }
    }
}