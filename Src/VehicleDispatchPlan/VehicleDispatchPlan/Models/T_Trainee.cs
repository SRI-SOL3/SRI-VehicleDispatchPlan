using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

/**
 * 教習生
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
    /// 教習生クラス
    /// </summary>
    [Table("T_Trainee")]
    public class T_Trainee
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public T_Trainee() { }

        /// <summary>
        /// コンストラクタ（コピー）
        /// </summary>
        /// <param name="trainee">コピー元</param>
        public T_Trainee(T_Trainee trainee)
        {
            // 各項目の値をコピー
            this.TraineeId = trainee.TraineeId;
            this.GroupId = trainee.GroupId;
            this.TraineeName = trainee.TraineeName;
            this.AttendTypeCd = trainee.AttendTypeCd;
            this.AttendTypeName = trainee.AttendTypeName;
            this.SelectAttendType = trainee.SelectAttendType;
            this.TrainingCourseCd = trainee.TrainingCourseCd;
            this.TrainingCourseName = trainee.TrainingCourseName;
            this.SelectTrainingCourse = trainee.SelectTrainingCourse;
            this.EntrancePlanDate = trainee.EntrancePlanDate;
            this.TmpLicencePlanDate = trainee.TmpLicencePlanDate;
            this.GraduatePlanDate = trainee.GraduatePlanDate;
            this.LodgingCd = trainee.LodgingCd;
            this.LodgingName = trainee.LodgingName;
            this.SelectLodging = trainee.SelectLodging;
            this.AgentName = trainee.AgentName;
        }

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

        /// <summary>通学種別<summary>
        [Required]
        [DisplayName("通学種別")]
        public string AttendTypeCd { get; set; }

        /// <summary>[非DB項目]通学種別名<summary>
        [NotMapped]
        [DisplayName("通学種別")]
        public string AttendTypeName { get; set; }

        /// <summary>[非DB項目]通学種別選択肢<summary>
        [NotMapped]
        public SelectList SelectAttendType { get; set; }

        /// <summary>教習コース<summary>
        [Required]
        [DisplayName("教習コース")]
        public string TrainingCourseCd { get; set;}

        /// <summary>[非DB項目]教習コース名<summary>
        [NotMapped]
        [DisplayName("教習コース")]
        public string TrainingCourseName { get; set; }

        /// <summary>[非DB項目]教習コース選択肢<summary>
        [NotMapped]
        public SelectList SelectTrainingCourse { get; set; }

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

        /// <summary>[非DB項目]宿泊施設名<summary>
        [NotMapped]
        [DisplayName("宿泊施設")]
        public string LodgingName { get; set; }

        /// <summary>[非DB項目]宿泊施設選択肢<summary>
        [NotMapped]
        public SelectList SelectLodging { get; set; }

        /// <summary>エージェント<summary>
        [DisplayName("エージェント")]
        public string AgentName { get; set; }
    }
}