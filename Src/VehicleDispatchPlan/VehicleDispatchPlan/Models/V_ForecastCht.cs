using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

/**
 * 受入予測管理図表
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
    /// 受入予測管理図表クラス
    /// </summary>
    public class V_ForecastCht
    {
        /// <summary>対象期間From</summary>
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? PlanDateFrom { get; set; }

        /// <summary>対象期間To</summary>
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? PlanDateTo { get; set; }

        /// <summary>グラフデータ</summary>
        public List<V_ChartData> ChartData { get; set; }
    }
}