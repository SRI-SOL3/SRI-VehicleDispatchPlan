using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using VehicleDispatchPlan.Models;

namespace VehicleDispatchPlan.Controllers
{
    public class InstractorController : Controller
    {
        private MyDatabaseContext db = new MyDatabaseContext();

        /// <summary>
        /// 指定日付の指導員を登録
        /// </summary>
        /// <param name="dailyClassesByTrainer">指導員別コマ数クラス</param>
        /// <returns>指導員一覧画面</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Date,No,TrainerName,Classes")] T_DailyClassesByTrainer dailyClassesByTrainer)
        {
            ///nullチェック
            if (dailyClassesByTrainer == null && dailyClassesByTrainer.Date == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            ///データ追加のため対象日のNoを取得してインクリメント
            var nextNum = db.DailyClassesByTrainer.Where(item => ((DateTime)item.Date).Equals((DateTime)dailyClassesByTrainer.Date)).Select(s => s.No).Max() + 1;
            if (nextNum == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ///コマ数入力チェック
            if (dailyClassesByTrainer.Classes <= 0)
            {
                ViewBag.ErrorMessage = "コマ数を入力してください。";
                return View(dailyClassesByTrainer);
            }

            if (ModelState.IsValid)
            {
                ///追加するデータ（No）
                dailyClassesByTrainer.No = nextNum;

                ///データ追加
                db.DailyClassesByTrainer.Add(dailyClassesByTrainer);
                db.SaveChanges();

            }
            else
            {
                ViewBag.ErrorMessage = "項目に値を入力してください。";
                return View(dailyClassesByTrainer);
            }

            ///指導員一覧画面へ遷移
            return BackToList(dailyClassesByTrainer.Date);
        }



        /// <summary>
        /// 指導員登録用の設定を行う
        /// </summary>
        /// <param name="Date">指定日付</param>
        /// <returns>登録画面へ遷移</returns>
        public ActionResult Create(DateTime? Date)
        {

            //指導員登録用インスタンス
            var dailyClassesByTrainer = new T_DailyClassesByTrainer();

            //引数が空であればエラー
            if (Date == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            }

            ///日付を設定
            dailyClassesByTrainer.Date = Date;

            return View(dailyClassesByTrainer);
        }


        /// <summary>
        /// 指導員データの保存処理を行う。（複数レコード）
        /// </summary>
        /// <param name="model">指導員用Model</param>
        /// <returns>一覧画面</returns>
        public ActionResult Edit([Bind(Include = "Date,t_DailyClassesByTrainer")] V_SearchInstractorViewModel model)
        {

            ///一覧画面で編集されたデータを登録
            foreach(var Instructor in model.t_DailyClassesByTrainer)
            {
                Instructor.Date = Instructor.DailyClasses.Date;
                Instructor.DailyClasses = null;
                
                db.Entry(Instructor).State = EntityState.Modified;
                db.SaveChanges();
                
            }

            ///更新メッセージ表示

            TempData["更新"] = "指導員のデータを更新しました。　更新日時:" + DateTime.Now.ToString("yyyy年MM月dd日 HH時mm分ss秒");

            return BackToList(model.Date);

          
        }


        /// <summary>
        /// 削除画面表示
        /// </summary>
        /// <param name="Date">指定日付</param>
        /// <param name="No">指導員別コマ数クラスのNO</param>
        /// <returns></returns>
        // GET: T_DailyClassesByTrainer/Delete/5
        public ActionResult Delete(DateTime? Date,int? No)
        {

            ///nullチェック
            if (No == null || Date == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ///削除対象のデータを取得
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
        /// <param name="No"></param>
        /// <returns>指導員一覧画面</returns>
        // POST: T_DailyClassesByTrainer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(DateTime? Date, int? No)
        {
            T_DailyClassesByTrainer t_DailyClassesByTrainer = db.DailyClassesByTrainer.Find(Date, No);
            db.DailyClassesByTrainer.Remove(t_DailyClassesByTrainer);
            db.SaveChanges();
            return BackToList(Date);
        }

        /// <summary>
        /// 指導員一覧
        /// </summary>
        /// <param name="model">指導員用Model</param>
        /// <returns>一覧画面</returns>
        public ActionResult List([Bind(Include = "Date")] V_SearchInstractorViewModel model)
        {
            

            if (model.Date != null)
            {
                // ----- 追加 -----
                ModelState.Clear();
                // ----------------
                ///画面で指定した日付を設定
                var list = db.DailyClassesByTrainer.Where(item => ((DateTime)item.Date).Equals((DateTime)model.Date)).ToList();
                model.t_DailyClassesByTrainer = list.ToList();
                ViewBag.DataExistsflg = true;
                
            }
            else
            {
                ///空のリストを設定
                model.t_DailyClassesByTrainer = new List<T_DailyClassesByTrainer>();

            }

            return (View(model));
        }

        /// <summary>
        /// 指導員一覧へのリダイレクト
        /// </summary>
        /// <param name="Date">指定日付</param>
        /// <returns>指導員一覧へのリダイレクト</returns>
        public ActionResult BackToList(DateTime? Date)
        {
            V_SearchInstractorViewModel model = new V_SearchInstractorViewModel();
            model.Date = Date;
            model.t_DailyClassesByTrainer = new List<T_DailyClassesByTrainer>();

            
            return RedirectToAction("List", model);
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
