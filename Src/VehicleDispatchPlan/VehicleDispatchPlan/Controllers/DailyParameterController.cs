using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using VehicleDispatchPlan.Commons;
using VehicleDispatchPlan.Models;

/**
 * 日別予測条件管理コントローラ
 *
 * @author t-murayama
 * @version 1.0
 * ----------------------------------
 * 2020/03/01 t-murayama 新規作成
 *
 */
namespace VehicleDispatchPlan.Controllers
{
    public class DailyParameterController : Controller
    {
        // データベースコンテキスト
        private MyDatabaseContext db = new MyDatabaseContext();

        /// <summary>
        /// 更新表示
        /// </summary>
        /// <param name="dailyParameterEdt">日別予測条件編集情報</param>
        /// <returns></returns>
        public ActionResult Edit([Bind(Include = "Date")] V_DailyParameterEdt dailyParameterEdt)
        {
            Trace.WriteLine("GET /DailyParameter/Edit/" + dailyParameterEdt.Date);

            // ステータスをクリア
            ModelState.Clear();
            // 日付が空の場合、エラー
            if (dailyParameterEdt.Date == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // 日別予測条件を取得
            T_DailyClasses dailyClasses = db.DailyClasses.Find(dailyParameterEdt.Date);
            // データが存在しない場合はインスタンスを生成
            if (dailyClasses == null)
            {
                dailyClasses = new T_DailyClasses() { Date = dailyParameterEdt.Date };
                ViewBag.DisableTrainerEdit = true;
            }
            else
            {
                ViewBag.DisableTrainerEdit = false;
            }
            dailyParameterEdt.DailyClasses = dailyClasses;

            // 指導員コマ数を取得
            dailyParameterEdt.TrainerList = db.DailyClassesByTrainer.Where(x => ((DateTime)x.Date).Equals((DateTime)dailyParameterEdt.Date)).OrderBy(x => x.No).ToList();

            return View(dailyParameterEdt);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="dailyParameterEdt">日別予測条件編集情報</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string cmd, [Bind(Include = "DailyClasses")] V_DailyParameterEdt dailyParameterEdt)
        {
            Trace.WriteLine("POST /DailyParameter/Edit/" + dailyParameterEdt.DailyClasses.Date);

            // 指導員管理の非活性をtrueに設定
            ViewBag.DisableTrainerEdit = true;
            // 日付を設定
            dailyParameterEdt.Date = dailyParameterEdt.DailyClasses.Date;

            // 入力チェック
            bool validation = true;
            if (ModelState.IsValid)
            {
                // 合宿比率と通学比率のチェック
                if (dailyParameterEdt.DailyClasses.LodgingRatio + dailyParameterEdt.DailyClasses.CommutingRatio != 100)
                {
                    ViewBag.ErrorMessage = "合宿比率[%]と通学比率[%]は合わせて100になるように設定してください。";
                    validation = false;
                }
                // 合宿の在籍比率[%]（AT一段階/二段階、MT一段階/二段階）のチェック
                if (validation == true &&
                    dailyParameterEdt.DailyClasses.LdgAtFstRatio + dailyParameterEdt.DailyClasses.LdgAtSndRatio + dailyParameterEdt.DailyClasses.LdgMtFstRatio + dailyParameterEdt.DailyClasses.LdgMtSndRatio != 100)
                {
                    ViewBag.ErrorMessage = "合宿の在籍比率[%]（AT-一段階/二段階、MT-一段階/二段階）は合わせて100になるように設定してください。";
                    validation = false;
                }
                // 通学の在籍比率[%]（AT一段階/二段階、MT一段階/二段階）のチェック
                if (validation == true &&
                    dailyParameterEdt.DailyClasses.CmtAtFstRatio + dailyParameterEdt.DailyClasses.CmtAtSndRatio + dailyParameterEdt.DailyClasses.CmtMtFstRatio + dailyParameterEdt.DailyClasses.CmtMtSndRatio != 100)
                {
                    ViewBag.ErrorMessage = "通学の在籍比率[%]（AT-一段階/二段階、MT-一段階/二段階）は合わせて100になるように設定してください。";
                    validation = false;
                }
            }
            else
            {
                // エラーメッセージを生成
                ViewBag.ErrorMessage = new Utility().getErrorMessage(ModelState);
                validation = false;
            }

            if (validation == true)
            {
                // 存在チェック
                if (db.DailyClasses.Where(x => ((DateTime)x.Date).Equals((DateTime)dailyParameterEdt.DailyClasses.Date)).Count() == 0)
                {
                    // 登録処理
                    db.DailyClasses.Add(dailyParameterEdt.DailyClasses);
                }
                else
                {
                    // 更新処理
                    db.Entry(dailyParameterEdt.DailyClasses).State = EntityState.Modified;
                }
                db.SaveChanges();
                // 完了メッセージ
                ViewBag.CompMessage = "データを更新しました。";
                // 指導員管理の非活性をfalseに設定
                ViewBag.DisableTrainerEdit = false;
            }

            // 指導員コマ数を取得
            dailyParameterEdt.TrainerList = db.DailyClassesByTrainer.Where(x => ((DateTime)x.Date).Equals((DateTime)dailyParameterEdt.DailyClasses.Date)).OrderBy(x => x.No).ToList();

            return View(dailyParameterEdt);
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
