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

        // GET: T_DailyClassesByTrainer
        public ActionResult Index(T_DailyClasses t_DailyClasses)
        {

            var dailyClassesByTrainer = db.DailyClassesByTrainer.Include(t => t.DailyClasses);

            if (t_DailyClasses != null) 
            { 
                db.DailyClasses.FirstOrDefault().Date = DateTime.Today;
                return View(dailyClassesByTrainer.ToList());
            }
            else
            
            
            return View(dailyClassesByTrainer.ToList());
        }

        // GET: T_DailyClassesByTrainer/Details/5
        public ActionResult Details(DateTime id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            T_DailyClassesByTrainer t_DailyClassesByTrainer = db.DailyClassesByTrainer.Find(id);
            if (t_DailyClassesByTrainer == null)
            {
                return HttpNotFound();
            }
            return View(t_DailyClassesByTrainer);
        }

        // GET: T_DailyClassesByTrainer/Create
        public ActionResult Create()
        {
            ViewBag.Date = new SelectList(db.DailyClasses, "Date", "Date");
            return View();
        }

        // POST: T_DailyClassesByTrainer/Create
        // 過多ポスティング攻撃を防止するには、バインド先とする特定のプロパティを有効にしてください。
        // 詳細については、https://go.microsoft.com/fwlink/?LinkId=317598 を参照してください。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Date,No,TrainerName,Classes")] T_DailyClassesByTrainer t_DailyClassesByTrainer)
        {
            if (ModelState.IsValid)
            {
                db.DailyClassesByTrainer.Add(t_DailyClassesByTrainer);
                db.SaveChanges();
                return RedirectToAction("List");
            }

            ViewBag.Date = new SelectList(db.DailyClasses, "Date", "Date", t_DailyClassesByTrainer.Date);
            return View(t_DailyClassesByTrainer);
        }
        /// <summary>
        /// 指導員データの保存処理を行う。（複数レコード）
        /// </summary>
        /// <param name="model">指導員用Model</param>
        /// <returns>一覧画面</returns>
        public ActionResult Edit([Bind(Include = "Date,t_DailyClassesByTrainer")] V_SearchInstractorViewModel model)
        {

            foreach(var trainer in model.t_DailyClassesByTrainer)
            {
                trainer.Date = trainer.DailyClasses.Date;
                trainer.DailyClasses = null;
                
                db.Entry(trainer).State = EntityState.Modified;
                db.SaveChanges();
            }

        
            return RedirectToAction("List",model);

          
        }

        // GET: T_DailyClassesByTrainer/Edit/5
        //public ActionResult Edit(DateTime id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    T_DailyClassesByTrainer t_DailyClassesByTrainer = db.DailyClassesByTrainer.Find(id);
        //    if (t_DailyClassesByTrainer == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.Date = new SelectList(db.DailyClasses, "Date", "Date", t_DailyClassesByTrainer.Date);
        //    return View(t_DailyClassesByTrainer);
        //}

        // POST: T_DailyClassesByTrainer/Edit/5
        // 過多ポスティング攻撃を防止するには、バインド先とする特定のプロパティを有効にしてください。
        // 詳細については、https://go.microsoft.com/fwlink/?LinkId=317598 を参照してください。
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "Date,No,TrainerName,Classes")] T_DailyClassesByTrainer t_DailyClassesByTrainer)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(t_DailyClassesByTrainer).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Seach");
        //    }
        //    ViewBag.Date = new SelectList(db.DailyClasses, "Date", "Date", t_DailyClassesByTrainer.Date);
        //    return View(t_DailyClassesByTrainer);
        //}

        // GET: T_DailyClassesByTrainer/Delete/5
        public ActionResult Delete(DateTime id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            T_DailyClassesByTrainer t_DailyClassesByTrainer = db.DailyClassesByTrainer.Find(id);
            if (t_DailyClassesByTrainer == null)
            {
                return HttpNotFound();
            }
            return View(t_DailyClassesByTrainer);
        }

        // POST: T_DailyClassesByTrainer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(DateTime id)
        {
            T_DailyClassesByTrainer t_DailyClassesByTrainer = db.DailyClassesByTrainer.Find(id);
            db.DailyClassesByTrainer.Remove(t_DailyClassesByTrainer);
            db.SaveChanges();
            return RedirectToAction("List");
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

                ///画面で指定した日付を設定
                var list = db.DailyClassesByTrainer.Where(item => ((DateTime)item.Date).Equals((DateTime)model.Date)).ToList();
                model.t_DailyClassesByTrainer = list.ToList();
            }
            else
            {
                ///空のリストを設定
                model.t_DailyClassesByTrainer = new List<T_DailyClassesByTrainer>();

            }

            return (View(model));
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
