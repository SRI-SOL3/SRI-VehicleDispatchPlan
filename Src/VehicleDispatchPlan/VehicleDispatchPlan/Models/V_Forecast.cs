using System.Collections.Generic;
using System.ComponentModel;

/**
 * 受入予測表示
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
    /// 受入予測表示クラス
    /// </summary>
    public class V_Forecast
    {
        /// <summary>受入予測</summary>
        public T_Forecast Forecast { get; set; }

        /// <summary>勤務属性別受入予測</summary>
        public List<T_ForecastByWork> ForecastByWork { get; set; }

        /// <summary>勤務属性</summary>
        public List<M_WorkType> WorkType { get; set; }

        /// <summary>グラフデータ</summary>
        public List<V_ChartData> ChartData { get; set; }

        /// <summary>更新不可</summary>
        public bool DisableUpdate { get; set; }

        /// <summary>グラフ表示_総受入残数</summary>
        [DisplayName("総受入残数")]
        public bool ChartTotalRem { get; set; }

        /// <summary>グラフ表示_合宿受入残数</summary>
        [DisplayName("合宿受入残数")]
        public bool ChartLodgingRem { get; set; }

        /// <summary>グラフ表示_通学受入残数</summary>
        [DisplayName("通学受入残数")]
        public bool ChartCommutingRem { get; set; }

        /// <summary>グラフ表示_総在籍数</summary>
        [DisplayName("総在籍数")]
        public bool ChartTotalReg { get; set; }

        /// <summary>グラフ表示_合宿在籍数</summary>
        [DisplayName("合宿在籍数")]
        public bool ChartLodgingReg { get; set; }

        /// <summary>グラフ表示_通学在籍数</summary>
        [DisplayName("通学在籍数")]
        public bool ChartCommutingReg { get; set; }
    }
}