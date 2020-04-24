using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace VehicleDispatchPlan.Models
{
    public class V_SearchInstractorViewModel
    {
        /// <summary>
        /// 対象日
        /// </summary>
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DisplayName("対象日")]
        public DateTime? Date { get; set; }

        /// <summary>
        /// 指導員別コマ数クラス
        /// </summary>
        public List<T_DailyClassesByTrainer> t_DailyClassesByTrainer { get; set; }
    }
}