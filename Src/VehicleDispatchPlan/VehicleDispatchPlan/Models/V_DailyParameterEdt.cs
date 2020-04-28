using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

/**
 * 日別予測条件編集
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
    /// 日別予測条件編集クラス
    /// </summary>
    public class V_DailyParameterEdt
    {
        /// <summary>検索日付</summary>
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? SearchDate { get; set; }

        /// <summary>更新日付To</summary>
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? UpdateTo { get; set; }

        /// <summary>日別予測条件</summary>
        public T_DailyClasses DailyClasses { get; set; }

        /// <summary>日別指導員コマ数</summary>
        public List<T_DailyClassesByTrainer> TrainerList { get; set; }
    }
}