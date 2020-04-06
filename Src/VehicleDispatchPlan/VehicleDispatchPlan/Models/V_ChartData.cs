using System;

/**
 * グラフデータ
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
    /// グラフデータクラス
    /// </summary>
    public class V_ChartData
    {
        /// <summary>日付</summary>
        public DateTime Date { get; set; }

        /// <summary>当日在籍最大数</summary>
        public double DailyTotalMaxAmt { get; set; }

        /// <summary>当日合宿在籍最大数</summary>
        public double DailyLodgingMaxAmt { get; set; }

        /// <summary>当日通学在籍最大数</summary>
        public double DailyCommutingMaxAmt { get; set; }

        /// <summary>当期受入最大数</summary>
        public double AcceptTotalMaxAmt { get; set; }

        /// <summary>当期合宿受入最大数</summary>
        public double AcceptLodgingMaxAmt { get; set; }

        /// <summary>当期通学受入最大数</summary>
        public double AcceptCommutingMaxAmt { get; set; }

        /// <summary>当期受入累積数</summary>
        public int AcceptTotalSumAmt { get; set; }

        /// <summary>当期合宿受入累積数</summary>
        public int AcceptLodgingSumAmt { get; set; }

        /// <summary>当期通学受入累積数</summary>
        public int AcceptCommutingSumAmt { get; set; }

        /// <summary>当期受入可能残数</summary>
        public double AcceptTotalRemAmt { get { return Math.Round(this.AcceptTotalMaxAmt - this.AcceptTotalSumAmt, 1); } }

        /// <summary>当期合宿受入可能残数</summary>
        public double AcceptLodgingRemAmt { get { return Math.Round(this.AcceptLodgingMaxAmt - this.AcceptLodgingSumAmt, 1); } }

        /// <summary>当期合宿受入可能残数</summary>
        public double AcceptCommutingRemAmt { get { return Math.Round(this.AcceptCommutingMaxAmt - this.AcceptCommutingSumAmt, 1); } }

        /// <summary>総在籍数</summary>
        public int TotalRegAmt { get { return this.LodgingMtFstRegAmt + this.LodgingMtSndRegAmt 
                    + this.LodgingAtFstRegAmt + this.LodgingAtSndRegAmt + this.CommutingMtRegAmt + this.CommutingAtRegAmt; } }

        /// <summary>合宿在籍数(MT-一段階)</summary>
        public int LodgingMtFstRegAmt { get; set; }

        /// <summary>合宿在籍数(MT-二段階)</summary>
        public int LodgingMtSndRegAmt { get; set; }

        /// <summary>合宿在籍数(AT-一段階)</summary>
        public int LodgingAtFstRegAmt { get; set; }

        /// <summary>合宿在籍数(AT-二段階)</summary>
        public int LodgingAtSndRegAmt { get; set; }

        /// <summary>通学在籍数(MT)</summary>
        public int CommutingMtRegAmt { get; set; }

        /// <summary>通学在籍数(AT)</summary>
        public int CommutingAtRegAmt { get; set; }
    }
}