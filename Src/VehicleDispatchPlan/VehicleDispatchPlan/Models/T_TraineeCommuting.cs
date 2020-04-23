using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

/**
 * 通学教習生
 *
 * @author 土井勇紀
 * @version 1.0
 * ----------------------------------
 * 2020/04/16 土井勇紀　複製
 *
 */
namespace VehicleDispatchPlan.Models
{
    /// <summary>
    /// 通学教習生クラス
    /// </summary>
    [Table("T_TraineeCommuting")]
    public class T_TraineeCommuting
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public T_TraineeCommuting() { }

        /// <summary>
        /// コンストラクタ（コピー）
        /// </summary>
        /// <param name="trainee">コピー元</param>
        public T_TraineeCommuting(T_TraineeCommuting trainee)
        {
            // 各項目の値をコピー
            this.TraineeId = trainee.TraineeId;
            this.TraineeName = trainee.TraineeName;
            this.Gender = trainee.Gender;
            this.SelectGender = trainee.SelectGender;
            this.TrainingCourseCd = trainee.TrainingCourseCd;
            this.SelectTrainingCourse = trainee.SelectTrainingCourse;
            this.ReserveDate = trainee.ReserveDate;
            this.EntrancePlanDate = trainee.EntrancePlanDate;
            this.TmpLicencePlanDate = trainee.TmpLicencePlanDate;
            this.GraduatePlanDate = trainee.GraduatePlanDate;
            this.AgentName = trainee.AgentName;
            this.SchoolName = trainee.SchoolName;
            this.MiddleSchoolDistrict = trainee.MiddleSchoolDistrict;
            this.FormOfAttractingCustomers = trainee.FormOfAttractingCustomers;
            this.CancelFlg = trainee.CancelFlg;
        }

        /// <summary>教習生ID<summary>
        [Key]
        [Required]
        [DisplayName("教習生ID")]
        public int TraineeId { get; set; }

        /// <summary>名前<summary>
        [Required]
        [DisplayName("教習者名")]
        public string TraineeName { get; set; }

        /// <summary>性別（M:男 / F:女）<summary>
        [Required]
        [DisplayName("性別")]
        public string Gender { get; set; }

        /// <summary>[非DB項目]性別選択肢<summary>
        [NotMapped]
        public SelectList SelectGender { get; set; }

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

        /// <summary>エージェント<summary>
        [DisplayName("エージェント")]
        public string AgentName { get; set; }

        /// <summary>高校名<summary>
        [DisplayName("高校名")]
        public string SchoolName { get; set; }

        /// <summary>中学区<summary>
        [DisplayName("中学区")]
        public string MiddleSchoolDistrict { get; set; }

        /// <summary>集客形態<summary>
        [DisplayName("集客形態")]
        public string FormOfAttractingCustomers { get; set; }

        /// <summary>キャンセルフラグ</summary>
        [DisplayName("キャンセル")]
        public bool CancelFlg { get; set; }

        public static implicit operator T_TraineeCommuting(List<T_TraineeCommuting> v)
        {
            throw new NotImplementedException();
        }

        public static implicit operator T_TraineeCommuting(List<V_TraineeCommutingEdt> v)
        {
            throw new NotImplementedException();
        }
    }
}