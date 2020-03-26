using System;
using System.Diagnostics;
using System.Web.Mvc;

/**
 * Homeコントローラ
 *
 * @author t-murayama
 * @version 1.0
 * ----------------------------------
 * 2020/03/01 t-murayama 新規作成
 *
 */
namespace VehicleDispatchPlan.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// メニュー
        /// </summary>
        /// <returns></returns>
        public ActionResult Menu()
        {
            Trace.WriteLine("GET /Home/Menu");

            return View();
        }

        /// <summary>
        /// 受入可能人数管理
        /// </summary>
        /// <returns></returns>
        public ActionResult AcceptPeopleKanri()
        {
            return View();
        }
    }
}