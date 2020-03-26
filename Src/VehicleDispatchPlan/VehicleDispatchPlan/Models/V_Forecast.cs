using System.Collections.Generic;
using System.ComponentModel;
using VehicleDispatchPlan.Constants;

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
        public List<T_Forecast> Forecast { get; set; }

        /// <summary>勤務属性別受入予測</summary>
        public List<T_ForecastByWork> ForecastByWork { get; set; }

        /// <summary>勤務属性</summary>
        public List<M_WorkType> WorkType { get; set; }

        /// <summary>グラフデータ</summary>
        public List<V_ChartData> ChartData { get; set; }

        /// <summary>更新不可</summary>
        public bool DisableUpdate { get; set; }

        /// <summary>グラフ表示_総受入残数</summary>
        [DisplayName(AppConstant.SERIES_TOTAL_REM_AMT)]
        public bool ChartTotalRem { get; set; }

        /// <summary>グラフ表示_合宿受入残数</summary>
        [DisplayName(AppConstant.SERIES_LODGING_REM_AMT)]
        public bool ChartLodgingRem { get; set; }

        /// <summary>グラフ表示_通学受入残数</summary>
        [DisplayName(AppConstant.SERIES_COMMUTING_REM_AMT)]
        public bool ChartCommutingRem { get; set; }

        /// <summary>グラフ表示_総在籍数</summary>
        [DisplayName(AppConstant.SERIES_TOTAL_REG_AMT)]
        public bool ChartTotalReg { get; set; }

        /// <summary>グラフ表示_合宿在籍数</summary>
        [DisplayName(AppConstant.SERIES_LODGING_REG_AMT)]
        public bool ChartLodgingReg { get; set; }

        /// <summary>グラフ表示_通学在籍数</summary>
        [DisplayName(AppConstant.SERIES_COMMUTING_REG_AMT)]
        public bool ChartCommutingReg { get; set; }
    }
}