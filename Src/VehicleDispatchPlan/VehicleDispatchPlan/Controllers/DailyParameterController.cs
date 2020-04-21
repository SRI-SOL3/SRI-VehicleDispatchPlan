using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
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
        /// <param name="date">日付</param>
        /// <returns></returns>
        public ActionResult Edit(DateTime? date)
        {
            Trace.WriteLine("GET /DailyParameter/Edit/" + date);

            if (date == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            T_DailyClasses dailyClasses = db.DailyClasses.Find(date);
            if (dailyClasses == null)
            {
                return HttpNotFound();
            }
            return View(dailyClasses);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="dailyClasses"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Date,LodgingRatio,CommutingRatio,LdgAtFstRatio,LdgAtSndRatio,LdgMtFstRatio,LdgMtSndRatio,CmtAtFstRatio,CmtAtSndRatio,CmtMtFstRatio,CmtMtSndRatio,LdgAtFstClass,LdgAtSndClass,LdgMtFstClass,LdgMtSndClass,CmtAtFstClass,CmtAtSndClass,CmtMtFstClass,CmtMtSndClass,LdgAtFstClassDay,LdgAtSndClassDay,LdgMtFstClassDay,LdgMtSndClassDay,CmtAtFstClassDay,CmtAtSndClassDay,CmtMtFstClassDay,CmtMtSndClassDay")] T_DailyClasses dailyClasses)
        {
            Trace.WriteLine("POST /DailyParameter/Edit/" + dailyClasses.Date);

            if (ModelState.IsValid)
            {
                //db.Entry(dailyClasses).State = EntityState.Modified;
                //db.SaveChanges();
                //return RedirectToAction("Index");
            }
            return View(dailyClasses);
        }

        // GET: DailyParameter/Delete/5
        public ActionResult Delete(DateTime id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            T_DailyClasses t_DailyClasses = db.DailyClasses.Find(id);
            if (t_DailyClasses == null)
            {
                return HttpNotFound();
            }
            return View(t_DailyClasses);
        }

        // POST: DailyParameter/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(DateTime id)
        {
            T_DailyClasses t_DailyClasses = db.DailyClasses.Find(id);
            db.DailyClasses.Remove(t_DailyClasses);
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
