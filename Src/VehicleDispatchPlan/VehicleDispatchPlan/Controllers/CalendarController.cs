using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using VehicleDispatchPlan.Models;

namespace VehicleDispatchPlan_Dev.Controllers
{
    public class CalendarController : Controller
    {
        private MyDatabaseContext db = new MyDatabaseContext();

        // GET: Calendar
        public ActionResult Index()
        {
            return View(db.EntGrdCalendar.ToList());
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
