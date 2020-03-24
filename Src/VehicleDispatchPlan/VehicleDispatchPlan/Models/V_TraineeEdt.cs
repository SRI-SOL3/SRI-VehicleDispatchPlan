using System.Collections.Generic;
using VehicleDispatchPlan.Constants;

/**
 * 教習生編集
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
    /// 教習生編集クラス
    /// </summary>
    public class V_TraineeEdt
    {
        /// <summary>教習生</summary>
        public T_Trainee Trainee { get; set; }

        /// <summary>編集モード</summary>
        public AppConstant.EditMode EditMode { get; set; }

        /// <summary>グラフデータ</summary>
        public List<V_ChartData> ChartData { get; set; }
    }
}