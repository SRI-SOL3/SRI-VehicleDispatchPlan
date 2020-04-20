using System.Collections.Generic;
using VehicleDispatchPlan.Constants;

/**
 * 教習生登録
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
    /// 通教習生登録クラス
    /// </summary>
    public class V_TraineeCommutingReg
    {
        /// <summary>教習生</summary>
        public List<T_TraineeCommuting> TraineeList { get; set; }

        /// <summary>編集モード</summary>
        public AppConstant.EditMode EditMode { get; set; }

        /// <summary>グラフデータ</summary>
        public List<V_ChartData> ChartData { get; set; }
    }
}