using System;
using System.Collections.Generic;
using System.Web.Mvc;
using VehicleDispatchPlan.Commons;
using VehicleDispatchPlan.Constants;
using VehicleDispatchPlan.Models;

/**
 * 受入予測コントローラ
 *
 * @author t-murayama
 * @version 1.0
 * ----------------------------------
 * 2020/03/01 t-murayama 新規作成
 *
 */
namespace VehicleDispatchPlan.Controllers
{
    public class ForecastController : Controller
    {
        // データベースコンテキスト
        private MyDatabaseContext db = new MyDatabaseContext();

        /// <summary>
        /// 受入予測確認
        /// </summary>
        /// <param name="cmd">コマンド</param>
        /// <param name="dateFrom">日付From</param>
        /// <param name="dateTo">日付To</param>
        /// <param name="totalRemAmt">総受入残数</param>
        /// <param name="lodgingRemAmt">総在籍可能数</param>
        /// <param name="commutingRemAmt">総在籍見込数</param>
        /// <param name="totalMaxAmt">合宿受入残数</param>
        /// <param name="lodgingMaxAmt">合宿在籍可能数</param>
        /// <param name="commutingMaxAmt">合宿在籍見込数</param>
        /// <param name="totalRegAmt">通学受入残数</param>
        /// <param name="lodgingRegAmt">通学在籍可能数</param>
        /// <param name=cCommutingRegAmt">通学在籍見込数</param>
        /// <returns></returns>
        public ActionResult Edit(string cmd, DateTime? dateFrom, DateTime? dateTo
            , bool? totalRemAmt, bool? lodgingRemAmt, bool? commutingRemAmt
            , bool? totalMaxAmt, bool? lodgingMaxAmt, bool? commutingMaxAmt
            , bool? totalRegAmt, bool? lodgingRegAmt, bool? commutingRegAmt)
        {
            // グラフデータ
            List<V_ChartData> chartData = new List<V_ChartData>();

            bool validation = true;

            // 検索ボタンもしくは再表示ボタンが押下された場合
            if (AppConstant.CMD_SEARCH.Equals(cmd) 
                || AppConstant.CMD_REDISPLAY.Equals(cmd))
            {
                // 入力チェック
                if (dateFrom == null || dateTo == null)
                {
                    ViewBag.ErrorMessage = "検索条件を指定してください。";
                    validation = false;
                }
                // 前後チェック
                if (validation == true && (dateFrom > dateTo))
                {
                    ViewBag.ErrorMessage = "日付の前後関係が不正です。";
                    validation = false;
                }

                if (validation == true)
                {
                    // グラフと表を作成
                    Utility utility = new Utility();
                    chartData = utility.getChartData(db, (DateTime)dateFrom, (DateTime)dateTo, null);
                    // グラフを生成（各表示フラグはnullの場合、trueとする）
                    ViewBag.ChartPath = utility.getChartPath(((DateTime)dateFrom).Year.ToString(), ((DateTime)dateFrom).Month.ToString(), chartData
                        , totalRemAmt ?? true, lodgingRemAmt ?? true, commutingRemAmt ?? true
                        , totalMaxAmt ?? true, lodgingMaxAmt ?? true, commutingMaxAmt ?? true
                        , totalRegAmt ?? true, lodgingRegAmt ?? true, commutingRegAmt ?? true);
                }
            }

            return View(chartData);
        }

        /// <summary>
        /// データベース接続の破棄
        /// </summary>
        /// <param name="disposing">破棄有無</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}