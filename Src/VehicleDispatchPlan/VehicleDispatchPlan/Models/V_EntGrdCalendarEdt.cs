using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VehicleDispatchPlan.Models
{
    public class V_EntGrdCalendarEdt
    {
        /// <summary>年</summary>
        public string Year { get; set; }

        /// <summary>月</summary>
        public string Month { get; set; }

        /// <summary>カレンダー</summary>
        public List<M_EntGrdCalendar> CalendarList { get; set; }
    }
}