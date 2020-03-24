using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

/**
 * 教習生参照
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
    /// 教習生参照クラス
    /// </summary>
    public class V_TraineeRef
    {
        /// <summary>教習生ID<summary>
        [DisplayName("ID")]
        public int TraineeId { get; set; }

        /// <summary>名前<summary>
        [DisplayName("教習者名")]
        public string TraineeName { get; set; }

        /// <summary>通学種別ｺｰﾄﾞ<summary>
        public string AttendTypeCd { get; set; }

        /// <summary>通学種別<summary>
        [DisplayName("通学種別")]
        public string AttendTypeName { get; set; }

        /// <summary>教習コース<summary>
        [DisplayName("教習コース")]
        public string TrainingCourseName { get; set; }

        /// <summary>入校予定日<summary>
        [DisplayName("入校予定日")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EntrancePlanDate { get; set; }

        /// <summary>仮免予定日<summary>
        [DisplayName("仮免予定日")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime TmpLicencePlanDate { get; set; }

        /// <summary>卒業予定日<summary>
        [DisplayName("卒業予定日")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime GraduatePlanDate { get; set; }

        /// <summary>宿泊施設<summary>
        [DisplayName("宿泊施設名")]
        public string LodgingName { get; set; }

        /// <summary>紹介エージェント<summary>
        [DisplayName("エージェント")]
        public string AgentName { get; set; }
    }
}