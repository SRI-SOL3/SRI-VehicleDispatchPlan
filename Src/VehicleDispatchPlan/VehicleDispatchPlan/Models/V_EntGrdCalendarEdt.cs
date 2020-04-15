using System.Collections.Generic;

/**
 * 入卒カレンダー編集
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
    /// 入卒カレンダー編集クラス
    /// </summary>
    public class V_EntGrdCalendarEdt
    {
        /// <summary>対象年</summary>
        public string Year { get; set; }

        /// <summary>対象月</summary>
        public string Month { get; set; }

        /// <summary>入卒カレンダー</summary>
        public List<M_EntGrdCalendar> CalendarList { get; set; }
    }
}