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

        /// <summary>日付(文字列) ※グラフ横軸用</summary>
        public string DateMd
        {
            get
            {
                return this.Date.ToString("M/d");
            }
        }

        // --------------------
        // 受入可能残数
        // --------------------
        /// <summary>当期受入可能残数</summary>
        public double AcceptTotalRemAmt
        {
            get
            {
                return Math.Round(this.AcceptLodgingRemAmt + this.AcceptCommutingRemAmt, 1);
            }
        }

        /// <summary>当期合宿受入可能残数</summary>
        public double AcceptLodgingRemAmt { get; set; }

        /// <summary>当期通学受入可能残数</summary>
        public double AcceptCommutingRemAmt { get; set; }

        // --------------------
        // 在籍最大数
        // --------------------
        /// <summary>当日在籍最大数</summary>
        public double DailyTotalMaxAmt
        {
            get
            {
                return Math.Round(this.DailyLodgingMaxAmt + this.DailyCommutingMaxAmt, 1);
            }
        }

        /// <summary>当日合宿在籍最大数</summary>
        public double DailyLodgingMaxAmt { get; set; }

        /// <summary>当日通学在籍最大数</summary>
        public double DailyCommutingMaxAmt { get; set; }

        // --------------------
        // 在籍数
        // --------------------
        /// <summary>総在籍数</summary>
        public int TotalRegAmt
        {
            get
            {
                return this.LodgingMtFstRegAmt + this.LodgingMtSndRegAmt
                    + this.LodgingAtFstRegAmt + this.LodgingAtSndRegAmt
                    + this.CommutingMtFstRegAmt + this.CommutingMtSndRegAmt
                    + this.CommutingAtFstRegAmt + this.CommutingAtSndRegAmt;
            }
        }

        /// <summary>合宿在籍数</summary>
        public int LodgingRegAmt
        {
            get
            {
                return this.LodgingMtFstRegAmt + this.LodgingMtSndRegAmt
                    + this.LodgingAtFstRegAmt + this.LodgingAtSndRegAmt;
            }
        }

        /// <summary>通学在籍数</summary>
        public int CommutingRegAmt
        {
            get
            {
                return this.CommutingMtFstRegAmt + this.CommutingMtSndRegAmt
                    + this.CommutingAtFstRegAmt + this.CommutingAtSndRegAmt;
            }
        }

        /// <summary>合宿在籍数(MT-一段階)</summary>
        public int LodgingMtFstRegAmt { get; set; }

        /// <summary>合宿在籍数(MT-二段階)</summary>
        public int LodgingMtSndRegAmt { get; set; }

        /// <summary>合宿在籍数(AT-一段階)</summary>
        public int LodgingAtFstRegAmt { get; set; }

        /// <summary>合宿在籍数(AT-二段階)</summary>
        public int LodgingAtSndRegAmt { get; set; }

        /// <summary>通学在籍数(MT-一段階)</summary>
        public int CommutingMtFstRegAmt { get; set; }

        /// <summary>通学在籍数(MT-二段階)</summary>
        public int CommutingMtSndRegAmt { get; set; }

        /// <summary>通学在籍数(AT-一段階)</summary>
        public int CommutingAtFstRegAmt { get; set; }

        /// <summary>通学在籍数(AT-二段階)</summary>
        public int CommutingAtSndRegAmt { get; set; }

        /// <summary>残コマ数/週</summary>
        public double WeeklyRemClasses { get; set; }
    }
}