using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using VehicleDispatchPlan.Commons;
using VehicleDispatchPlan.Models;

/**
 * 指導員管理コントローラ
 *
 * @author t-murayama
 * @version 1.0
 * ----------------------------------
 * 2020/03/01 t-murayama 新規作成
 * 2021/02/05 t-murayama 20210205リリース対応(ver.1.1)
 *
 */
namespace VehicleDispatchPlan.Controllers
{
    public class InstractorController : Controller
    {
        private MyDatabaseContext db = new MyDatabaseContext();

        /// <summary>
        /// 指導員一覧
        /// </summary>
        /// <param name="model">指導員用Model</param>
        /// <returns>一覧画面</returns>
        public ActionResult List([Bind(Include = "Date")] V_SearchInstractorViewModel model)
        {
            if (model.Date != null)
            {
                // ステータスをクリア
                ModelState.Clear();
                // 画面で指定した日付を設定
                var list = db.DailyClassesByTrainer.Where(item => ((DateTime)item.Date).Equals((DateTime)model.Date)).ToList();
                model.t_DailyClassesByTrainer = list.ToList();
            }
            else
            {
                // 空のリストを設定
                model.t_DailyClassesByTrainer = new List<T_DailyClassesByTrainer>();
            }

            return View(model);
        }

        /// <summary>
        /// 指導員データの保存処理を行う。（複数レコード）
        /// </summary>
        /// <param name="model">指導員用Model</param>
        /// <returns>一覧画面</returns>
        [HttpPost, ActionName("List")]
        [ValidateAntiForgeryToken]
        public ActionResult ListEdit([Bind(Include = "t_DailyClassesByTrainer")] V_SearchInstractorViewModel model)
        {
            // 日付を設定
            model.Date = model.t_DailyClassesByTrainer.FirstOrDefault().Date;
            // 入力チェック
            bool validation = true;
            if (ModelState.IsValid)
            {
                foreach (T_DailyClassesByTrainer dailyClassesByTrainer in model.t_DailyClassesByTrainer)
                {
                    // コマ数のチェック
                    // [20210205リリース対応] Mod Start コマ数の0許容
                    //if (dailyClassesByTrainer.Classes <= 0)
                    //{
                    //    ViewBag.ErrorMessage = "コマ数に0以下は設定できません。";
                    //    validation = false;
                    //    break;
                    //}
                    if (dailyClassesByTrainer.Classes < 0)
                    {
                        ViewBag.ErrorMessage = "コマ数に0未満は設定できません。";
                        validation = false;
                        break;
                    }
                    // [20210205リリース対応] Mod End
                }
            }
            else
            {
                // エラーメッセージ生成
                ViewBag.ErrorMessage = new Utility().GetErrorMessage(ModelState);
                validation = false;
            }

            if (validation == true)
            {
                // 一覧画面で編集されたデータを登録
                foreach (var Instructor in model.t_DailyClassesByTrainer)
                {
                    Instructor.DailyClasses = null;
                    db.Entry(Instructor).State = EntityState.Modified;
                }
                db.SaveChanges();

                // 更新メッセージ表示
                ViewBag.CompMessage = "指導員のデータを更新しました。　更新日時:" + DateTime.Now.ToString("yyyy年MM月dd日 HH時mm分ss秒");
            }

            return View(model);
        }

        /// <summary>
        /// 指導員登録用の設定を行う
        /// </summary>
        /// <param name="Date">指定日付</param>
        /// <returns>登録画面へ遷移</returns>
        public ActionResult Create(DateTime? Date)
        {
            // 引数が空であればエラー
            if (Date == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // 指導員登録用インスタンス
            T_DailyClassesByTrainer dailyClassesByTrainer = new T_DailyClassesByTrainer() { Date = Date, No = 0 };

            return View(dailyClassesByTrainer);
        }

        /// <summary>
        /// 指定日付の指導員を登録
        /// </summary>
        /// <param name="dailyClassesByTrainer">指導員別コマ数クラス</param>
        /// <returns>指導員一覧画面</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Date,No,TrainerName,Classes")] T_DailyClassesByTrainer dailyClassesByTrainer)
        {
            // nullチェック
            if (dailyClassesByTrainer.Date == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // 入力チェック
            bool validation = true;
            if (ModelState.IsValid)
            {
                // コマ数入力チェック
                // [20210205リリース対応] Mod Start コマ数の0許容
                //if (dailyClassesByTrainer.Classes <= 0)
                //{
                //    ViewBag.ErrorMessage = "コマ数に0以下は設定できません。";
                //    validation = false;
                //}
                if (dailyClassesByTrainer.Classes < 0)
                {
                    ViewBag.ErrorMessage = "コマ数に0未満は設定できません。";
                    validation = false;
                }
                // [20210205リリース対応] Mod End
            }
            else
            {
                // エラーメッセージ生成
                ViewBag.ErrorMessage = new Utility().GetErrorMessage(ModelState);
                validation = false;
            }

            if (validation == true)
            {
                // 日別予測条件（親データ）の存在チェック
                if (db.DailyClasses.Where(x => ((DateTime)x.Date).Equals((DateTime)dailyClassesByTrainer.Date)).Count() == 0)
                {
                    // 日付を指定してデータを登録
                    db.DailyClasses.Add(new T_DailyClasses() { Date = dailyClassesByTrainer.Date });
                }
                // データ追加のため対象日のNoを取得してインクリメント（データが0件の場合は１を設定）
                List<T_DailyClassesByTrainer> trainerList = db.DailyClassesByTrainer.Where(item => ((DateTime)item.Date).Equals((DateTime)dailyClassesByTrainer.Date)).ToList();
                int nextNum = trainerList.Count() > 0 ? trainerList.Select(s => s.No).Max() + 1 : 1;
                // 追加するデータ（No）
                dailyClassesByTrainer.No = nextNum;

                // データ追加
                db.DailyClassesByTrainer.Add(dailyClassesByTrainer);
                db.SaveChanges();

                // 指導員一覧画面へ遷移
                return RedirectToAction("List", new { Date = dailyClassesByTrainer.Date });
            }

            return View(dailyClassesByTrainer);
        }

        /// <summary>
        /// 削除画面表示
        /// </summary>
        /// <param name="Date">指定日付</param>
        /// <param name="No">指導員別コマ数クラスのNo</param>
        /// <returns></returns>
        public ActionResult Delete(DateTime? Date, int? No)
        {
            // nullチェック
            if (No == null || Date == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // 削除対象のデータを取得
            T_DailyClassesByTrainer t_DailyClassesByTrainer = db.DailyClassesByTrainer.Find(Date, No);
            if (t_DailyClassesByTrainer == null)
            {
                return HttpNotFound();
            }

            return View(t_DailyClassesByTrainer);
        }

        /// <summary>
        /// 削除実施
        /// </summary>
        /// <param name="Date">指定日付</param>
        /// <param name="No">指導員別コマ数クラスのNo</param>
        /// <returns>指導員一覧画面</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(DateTime? Date, int? No)
        {
            T_DailyClassesByTrainer t_DailyClassesByTrainer = db.DailyClassesByTrainer.Find(Date, No);
            db.DailyClassesByTrainer.Remove(t_DailyClassesByTrainer);
            db.SaveChanges();
            return RedirectToAction("List", new { Date = Date });
        }

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
