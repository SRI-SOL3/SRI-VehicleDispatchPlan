using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
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
        /// <param name="forecastCht">受入予測図表</param>
        /// <returns></returns>
        public ActionResult Chart(string cmd
            , [Bind(Include = "PlanDateFrom,PlanDateTo,TotalRemFlg,TotalMaxFlg,TotalRegFlg,LodgingRemFlg,LodgingMaxFlg,LodgingRegFlg,CommutingRemFlg,CommutingMaxFlg,CommutingRegFlg,ChartData")] V_ForecastCht forecastCht)
        {
            Trace.WriteLine("GET /Forecast/Chart");

            // ステータスをクリア
            ModelState.Clear();
            // グラフデータを初期化
            forecastCht.ChartData = new List<V_ChartData>();

            // 入力チェック
            bool validation = true;

            // コマンドが空（初回表示）の場合
            if (string.IsNullOrEmpty(cmd))
            {
                // 各グラフ表示フラグをtrueに設定
                forecastCht.TotalRemFlg = true;
                forecastCht.TotalMaxFlg = true;
                forecastCht.TotalRegFlg = true;
                forecastCht.LodgingRemFlg = true;
                forecastCht.LodgingMaxFlg = true;
                forecastCht.LodgingRegFlg = true;
                forecastCht.CommutingRemFlg = true;
                forecastCht.CommutingMaxFlg = true;
                forecastCht.CommutingRegFlg = true;

                // 入力チェック
                if (forecastCht.PlanDateFrom == null || forecastCht.PlanDateTo == null)
                {
                    validation = false;
                }
                // 前後チェック
                if (validation == true && (forecastCht.PlanDateFrom > forecastCht.PlanDateTo))
                {
                    validation = false;
                }
            }

            // コマンドが設定されている場合
            else
            {
                // 検索ボタンもしくは再表示ボタンが押下された場合
                if (AppConstant.CMD_SEARCH.Equals(cmd) || AppConstant.CMD_REDISPLAY.Equals(cmd))
                {
                    // 入力チェック
                    if (forecastCht.PlanDateFrom == null || forecastCht.PlanDateTo == null)
                    {
                        ViewBag.ErrorMessage = "検索条件を指定してください。";
                        validation = false;
                    }
                    // 前後チェック
                    if (validation == true && (forecastCht.PlanDateFrom > forecastCht.PlanDateTo))
                    {
                        ViewBag.ErrorMessage = "日付の前後関係が不正です。";
                        validation = false;
                    }
                }

                // その他
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }

            if (validation == true)
            {
                // 日別設定画面から遷移するための日付From/ToをTempDataに設定
                TempData[AppConstant.TEMP_KEY_DATE_FROM] = forecastCht.PlanDateFrom;
                TempData[AppConstant.TEMP_KEY_DATE_TO] = forecastCht.PlanDateTo;
                Utility utility = new Utility();
                // グラフデータを作成
                forecastCht.ChartData = utility.GetChartData(db, (DateTime)forecastCht.PlanDateFrom, (DateTime)forecastCht.PlanDateTo, null, null);
                // グラフを生成（各表示フラグはnullの場合、trueとする）
                ViewBag.ChartPath = utility.GetChartPath(
                    ((DateTime)forecastCht.PlanDateFrom).Year.ToString(), ((DateTime)forecastCht.PlanDateTo).Month.ToString(), forecastCht.ChartData
                    , forecastCht.TotalRemFlg, forecastCht.LodgingRemFlg, forecastCht.CommutingRemFlg
                    , forecastCht.TotalMaxFlg, forecastCht.LodgingMaxFlg, forecastCht.CommutingMaxFlg
                    , forecastCht.TotalRegFlg, forecastCht.LodgingRegFlg, forecastCht.CommutingRegFlg);
            }

            return View(forecastCht);
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