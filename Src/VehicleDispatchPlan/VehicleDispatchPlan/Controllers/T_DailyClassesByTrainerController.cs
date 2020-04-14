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
    public class T_DailyClassesByTrainerController : Controller
    {
        private MyDatabaseContext db = new MyDatabaseContext();

        // GET: T_DailyClassesByTrainer
        public ActionResult Index()
        {
            var dailyClassesByTrainer = db.DailyClassesByTrainer.Include(t => t.DailyClasses);
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
                return RedirectToAction("Index");
            }

            ViewBag.Date = new SelectList(db.DailyClasses, "Date", "Date", t_DailyClassesByTrainer.Date);
            return View(t_DailyClassesByTrainer);
        }

        // GET: T_DailyClassesByTrainer/Edit/5
        public ActionResult Edit(DateTime id)
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
            ViewBag.Date = new SelectList(db.DailyClasses, "Date", "Date", t_DailyClassesByTrainer.Date);
            return View(t_DailyClassesByTrainer);
        }

        // POST: T_DailyClassesByTrainer/Edit/5
        // 過多ポスティング攻撃を防止するには、バインド先とする特定のプロパティを有効にしてください。
        // 詳細については、https://go.microsoft.com/fwlink/?LinkId=317598 を参照してください。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Date,No,TrainerName,Classes")] T_DailyClassesByTrainer t_DailyClassesByTrainer)
        {
            if (ModelState.IsValid)
            {
                db.Entry(t_DailyClassesByTrainer).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Date = new SelectList(db.DailyClasses, "Date", "Date", t_DailyClassesByTrainer.Date);
            return View(t_DailyClassesByTrainer);
        }

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
            return RedirectToAction("Index");
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
