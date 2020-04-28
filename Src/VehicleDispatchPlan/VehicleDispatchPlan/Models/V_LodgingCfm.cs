using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

/**
 * 宿泊施設確認
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
    /// 宿泊施設確認クラス
    /// </summary>
    public class V_LodgingCfm
    {
        /// <summary>日付From</summary>
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DateFrom { get; set; }

        /// <summary>日付To</summary>
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DateTo { get; set; }

        /// <summary>宿泊施設コード</summary>
        public string LodgingCd { get; set; }

        /// <summary>宿泊施設選択肢</summary>
        public SelectList SelectLodging { get; set; }

        /// <summary>教習生</summary>
        public List<T_TraineeLodging> Trainee { get; set; }
    }
}