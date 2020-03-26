using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using VehicleDispatchPlan.Commons;
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
        /// 編集表示
        /// </summary>
        /// <returns></returns>
        public ActionResult Edit(string YearFrom, string MonthFrom, string YearTo, string MonthTo)
        {
            // 初期化処理
            V_Forecast vForecast = new V_Forecast();
            // 受入予測を初期化
            vForecast.Forecast = new List<T_Forecast>();
            // 勤務属性別受入予測を初期化
            vForecast.ForecastByWork = new List<T_ForecastByWork>();
            // 勤務属性を取得
            vForecast.WorkType = db.WorkType.OrderBy(x => x.WorkTypeCd).ToList();

            bool validation = true; 
            // 入力チェック
            if (string.IsNullOrEmpty(YearFrom) || string.IsNullOrEmpty(MonthFrom) || string.IsNullOrEmpty(YearTo) || string.IsNullOrEmpty(MonthTo))
            {
                TempData["errorMessage"] = "検索条件を指定してください。";
                validation = false;
            }
            // 前後チェック
            if (Convert.ToInt32(YearFrom) > Convert.ToInt32(YearTo) 
                || (Convert.ToInt32(YearFrom) == Convert.ToInt32(YearTo)) && Convert.ToInt32(MonthFrom) > Convert.ToInt32(MonthTo))
            {
                TempData["errorMessage"] = "日付の前後関係が不正です。";
                validation = false;
            }

            if (validation == true)
            {
                // 対象日付Fromの月の初日
                DateTime dateFrom = new DateTime(Convert.ToInt32(YearFrom), Convert.ToInt32(MonthFrom), 1);
                // 対象日付Toの月の最終日
                DateTime dateTo = new DateTime(Convert.ToInt32(YearTo), Convert.ToInt32(MonthTo) + 1, 1).AddDays(-1);

                for (DateTime day = dateFrom; day.CompareTo(dateTo) <= 0; day = day.AddMonths(1))
                {
                    string year = day.Year.ToString();
                    string month = day.Month.ToString();
                    // 受入予測を取得
                    T_Forecast forecast = db.Forecast.Where(x => x.Year.Equals(year) && x.Month.Equals(month)).FirstOrDefault();
                    if (forecast == null)
                    {
                        forecast = new T_Forecast();
                    }
                    vForecast.Forecast.Add(forecast);

                    // 勤務属性別受入予測を取得
                    List<T_ForecastByWork> forecastByWorkList = db.ForecastByWork.Where(x => x.Year.Equals(year) && x.Month.Equals(month)).ToList();
                    foreach (M_WorkType workType in vForecast.WorkType)
                    {
                        T_ForecastByWork forecastByWork = forecastByWorkList.Where(x => x.WorkTypeCd.Equals(workType.WorkTypeCd)).FirstOrDefault();
                        if (forecastByWork == null)
                        {
                            forecastByWork = new T_ForecastByWork
                            {
                                Year = year,
                                Month = month,
                                WorkTypeCd = workType.WorkTypeCd
                            };
                        }
                        vForecast.ForecastByWork.Add(forecastByWork);
                    }
                }

                // グラフの表示を全て有に設定
                vForecast.ChartTotalRem = true;
                vForecast.ChartLodgingRem = true;
                vForecast.ChartCommutingRem = true;
                vForecast.ChartTotalReg = true;
                vForecast.ChartLodgingReg = true;
                vForecast.ChartCommutingReg = true;

                // グラフと表を作成
                Utility utility = new Utility();
                vForecast.ChartData = utility.getChartData(db, dateFrom, dateTo, vForecast.Forecast, vForecast.ForecastByWork, null);
                ViewBag.ChartPath = utility.getChartPath(dateFrom.Year.ToString(), dateFrom.Month.ToString(), vForecast.ChartData
                    , vForecast.ChartTotalRem, vForecast.ChartLodgingRem, vForecast.ChartCommutingRem, vForecast.ChartTotalReg, vForecast.ChartLodgingReg, vForecast.ChartCommutingReg);

                // 更新不可をfalseに設定
                vForecast.DisableUpdate = false;
            }
            else
            {
                // 更新不可をtrueに設定
                vForecast.DisableUpdate = true;
            }

            // 画面項目の設定
            this.SetDisplayItem();

            return View(vForecast);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="search"></param>
        /// <param name="update"></param>
        /// <param name="vForecast"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string cmd, [Bind(Include = "Forecast,ForecastByWork,WorkType,ChartTotalRem,ChartLodgingRem,ChartCommutingRem,ChartTotalReg,ChartLodgingReg,ChartCommutingReg")] V_Forecast vForecast)
        {
            Trace.WriteLine("POST /Trainee/Edit/");

            //// 検索ボタンが押された場合
            //if ("検索".Equals(cmd))
            //{
            //    // ステータスをクリア（画面表示を反映させる）
            //    ModelState.Clear();

            //    if (!string.IsNullOrEmpty(year) && !string.IsNullOrEmpty(month))
            //    {
            //        // 年と期間を指定して予測条件を取得
            //        T_Forecast forecast = db.Forecast.Where(x => x.Year.Equals(year) && x.Month.Equals(month)).FirstOrDefault();
            //        // 取得された場合、バインドモデルに反映
            //        vForecast.Forecast = forecast != null ? forecast : new T_Forecast { Year = year, Month = month };

            //        // 勤務属性別受入予測を取得
            //        List<T_ForecastByWork> forecastByWorkList = db.ForecastByWork.Where(x => x.Year.Equals(year) && x.Month.Equals(month)).ToList();
            //        // 勤務属性別受入予測をクリア
            //        vForecast.ForecastByWork = new List<T_ForecastByWork>();
            //        // 勤務属性毎に繰り返し
            //        foreach (M_WorkType workType in vForecast.WorkType)
            //        {
            //            // 対象の勤務属性別受入予測を取得
            //            T_ForecastByWork forecastByWork = forecastByWorkList.Where(x => x.WorkTypeCd.Equals(workType.WorkTypeCd)).FirstOrDefault();
            //            if (forecastByWork != null)
            //            {
            //                // 存在する場合は追加
            //                vForecast.ForecastByWork.Add(forecastByWork);
            //            }
            //            else
            //            {
            //                // 存在しない場合はインスタンスを生成して追加
            //                vForecast.ForecastByWork.Add(
            //                    new T_ForecastByWork
            //                    {
            //                        Year = year,
            //                        Month = month,
            //                        WorkTypeCd = workType.WorkTypeCd
            //                    });
            //            }
            //        }

            //        // グラフの表示を全て有に設定
            //        vForecast.ChartTotalRem = true;
            //        vForecast.ChartLodgingRem = true;
            //        vForecast.ChartCommutingRem = true;
            //        vForecast.ChartTotalReg = true;
            //        vForecast.ChartLodgingReg = true;
            //        vForecast.ChartCommutingReg = true;

            //        // 日付From/Toを設定
            //        DateTime dateFrom = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), 1);
            //        DateTime dateTo = dateFrom.AddMonths(1).AddDays(-1);

            //        // グラフと表を作成
            //        Utility utility = new Utility();
            //        vForecast.ChartData = utility.getChartData(db, dateFrom, dateTo, new List<T_Forecast> { vForecast.Forecast}, vForecast.ForecastByWork, null);
            //        ViewBag.ChartPath = utility.getChartPath(year, month, vForecast.ChartData
            //            , vForecast.ChartTotalRem, vForecast.ChartLodgingRem, vForecast.ChartCommutingRem, vForecast.ChartTotalReg, vForecast.ChartLodgingReg, vForecast.ChartCommutingReg);

            //        // 更新不可をfalseに設定
            //        vForecast.DisableUpdate = false;
            //    }
            //    else
            //    {
            //        // 受入予測を初期化
            //        vForecast.Forecast = new T_Forecast { Year = year, Month = month };
            //        // 勤務属性別受入予測を初期化
            //        vForecast.ForecastByWork = new List<T_ForecastByWork>();
            //        // 勤務属性別受入予測を勤務属性毎に追加
            //        foreach (M_WorkType workType in vForecast.WorkType)
            //        {
            //            vForecast.ForecastByWork.Add(
            //                new T_ForecastByWork
            //                {
            //                    Year = year,
            //                    Month = month,
            //                    WorkTypeCd = workType.WorkTypeCd
            //                });
            //        }
            //        // 更新不可をtrueに設定
            //        vForecast.DisableUpdate = true;
            //    }
            //}

            //// 更新ボタンが押された場合
            //if ("更新".Equals(cmd))
            //{
            //    if (ModelState.IsValid)
            //    {
            //        // トランザクション作成
            //        using (DbContextTransaction tran = db.Database.BeginTransaction())
            //        {
            //            try
            //            {
            //                // 受入予測の登録／更新
            //                // 存在チェック
            //                if (db.Forecast.Where(x => x.Year.Equals(year) && x.Month.Equals(month)).Count() == 0)
            //                {
            //                    // 登録処理
            //                    db.Forecast.Add(vForecast.Forecast);
            //                    db.SaveChanges();
            //                }
            //                else
            //                {
            //                    // 更新処理
            //                    db.Entry(vForecast.Forecast).State = EntityState.Modified;
            //                    db.SaveChanges();
            //                }

            //                // 勤務属性別受入予測の登録／更新
            //                foreach (T_ForecastByWork forecastByWork in vForecast.ForecastByWork)
            //                {
            //                    // 存在チェック
            //                    if (db.ForecastByWork.Where(x => x.Year.Equals(year) && x.Month.Equals(month)
            //                        && x.WorkTypeCd.Equals(forecastByWork.WorkTypeCd)).Count() == 0)
            //                    {
            //                        // 登録処理
            //                        db.ForecastByWork.Add(forecastByWork);
            //                        db.SaveChanges();
            //                    }
            //                    else
            //                    {
            //                        // 更新処理
            //                        db.Entry(forecastByWork).State = EntityState.Modified;
            //                        db.SaveChanges();
            //                    }
            //                }

            //                // コミット
            //                tran.Commit();
            //            }
            //            catch (Exception e)
            //            {
            //                // ロールバック
            //                tran.Rollback();
            //                throw e;
            //            }
            //        }

            //        // 日付From/Toを設定
            //        DateTime dateFrom = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), 1);
            //        DateTime dateTo = dateFrom.AddMonths(1).AddDays(-1);

            //        // グラフと表を作成
            //        Utility utility = new Utility();
            //        vForecast.ChartData = utility.getChartData(db, dateFrom, dateTo, new List<T_Forecast> { vForecast.Forecast }, vForecast.ForecastByWork, null);
            //        ViewBag.ChartPath = utility.getChartPath(year, month, vForecast.ChartData
            //            , vForecast.ChartTotalRem, vForecast.ChartLodgingRem, vForecast.ChartCommutingRem, vForecast.ChartTotalReg, vForecast.ChartLodgingReg, vForecast.ChartCommutingReg);
            //    }
            //}

            //// 再表示ボタンが押された場合
            //else if ("再表示".Equals(cmd))
            //{
            //    // 日付From/Toを設定
            //    DateTime dateFrom = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), 1);
            //    DateTime dateTo = dateFrom.AddMonths(1).AddDays(-1);

            //    // グラフと表を作成
            //    Utility utility = new Utility();
            //    vForecast.ChartData = utility.getChartData(db, dateFrom, dateTo, new List<T_Forecast> { vForecast.Forecast }, vForecast.ForecastByWork, null);
            //    ViewBag.ChartPath = utility.getChartPath(year, month, vForecast.ChartData
            //        , vForecast.ChartTotalRem, vForecast.ChartLodgingRem, vForecast.ChartCommutingRem, vForecast.ChartTotalReg, vForecast.ChartLodgingReg, vForecast.ChartCommutingReg);
            //}

            // 画面項目の設定
            this.SetDisplayItem();

            return View(vForecast);
        }

        /// <summary>
        /// 画面項目を設定
        /// </summary>
        private void SetDisplayItem()
        {
            // 年の選択肢を設定
            int nowYear = DateTime.Now.Year;
            List<SelectListItem> selectYear = new List<SelectListItem>()
            {
                new SelectListItem() { Text = (nowYear - 5).ToString(), Value=(nowYear - 2).ToString() }
                , new SelectListItem() { Text = (nowYear - 4).ToString(), Value=(nowYear - 2).ToString() }
                , new SelectListItem() { Text = (nowYear - 3).ToString(), Value=(nowYear - 2).ToString() }
                , new SelectListItem() { Text = (nowYear - 2).ToString(), Value=(nowYear - 2).ToString() }
                , new SelectListItem() { Text = (nowYear - 1).ToString(), Value=(nowYear - 1).ToString() }
                , new SelectListItem() { Text = nowYear.ToString(), Value=nowYear.ToString() }
                , new SelectListItem() { Text = (nowYear + 1).ToString(), Value=(nowYear + 1).ToString() }
                , new SelectListItem() { Text = (nowYear + 2).ToString(), Value=(nowYear + 2).ToString() }
                , new SelectListItem() { Text = (nowYear + 3).ToString(), Value=(nowYear + 2).ToString() }
                , new SelectListItem() { Text = (nowYear + 4).ToString(), Value=(nowYear + 2).ToString() }
                , new SelectListItem() { Text = (nowYear + 5).ToString(), Value=(nowYear + 2).ToString() }
            };
            ViewBag.SelectYear = selectYear;
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