using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using VehicleDispatchPlan.Constants;
using VehicleDispatchPlan.Models;

/**
 * 宿泊施設管理コントローラ
 *
 * @author t-murayama
 * @version 1.0
 * ----------------------------------
 * 2020/03/01 t-murayama 新規作成
 *
 */
namespace VehicleDispatchPlan.Controllers
{
    public class LodgingController : Controller
    {
        // データベースコンテキスト
        private MyDatabaseContext db = new MyDatabaseContext();

        /// <summary>
        /// 一覧表示
        /// </summary>
        /// <returns></returns>
        public ActionResult List()
        {
            Trace.WriteLine("GET /Lodging/List");

            // 宿泊施設情報を取得
            List<M_LodgingFacility> lodgingFacilityList = db.LodgingFacility.OrderBy(x => x.LodgingCd).ToList();

            return View(lodgingFacilityList);
        }

        /// <summary>
        /// 登録表示
        /// </summary>
        /// <returns></returns>
        public ActionResult Regist()
        {
            Trace.WriteLine("GET /Lodging/Regist");

            // 宿泊施設のインスタンスを生成
            M_LodgingFacility lodgingFacility = new M_LodgingFacility();

            return View(lodgingFacility);
        }

        /// <summary>
        /// 登録処理
        /// </summary>
        /// <param name="cmd">コマンド</param>
        /// <param name="lodgingFacility">宿泊施設情報</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Regist(string cmd, [Bind(Include = "LodgingCd,LodgingName,TelNo,PostalNo,Address")] M_LodgingFacility lodgingFacility)
        {
            Trace.WriteLine("POST /Lodging/Regist");

            // 登録ボタンが押下された場合
            if (AppConstant.CMD_REGIST.Equals(cmd))
            {
                // 入力チェック
                bool validation = true;
                if (ModelState.IsValid)
                {
                    // 重複チェック
                    if (db.LodgingFacility.Find(lodgingFacility.LodgingCd) != null)
                    {
                        ViewBag.ErrorMessage = "設定された宿泊施設コードはすでに存在します。";
                        validation = false;
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "必須項目（宿泊施設コード、宿泊施設名）を設定してください。";
                    validation = false;
                }

                if (validation == true)
                {
                    // 登録処理
                    db.LodgingFacility.Add(lodgingFacility);
                    db.SaveChanges();
                    // 一覧へリダイレクト
                    return RedirectToAction("List");
                }
            }

            // その他
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            return View(lodgingFacility);
        }

        /// <summary>
        /// 更新表示
        /// </summary>
        /// <param name="cd">宿泊施設コード</param>
        /// <returns></returns>
        public ActionResult Edit(string cd)
        {
            Trace.WriteLine("GET /Lodging/Edit/" + cd);

            // DBから宿泊施設情報を取得
            M_LodgingFacility lodgingFacility = db.LodgingFacility.Find(cd);
            if (lodgingFacility == null)
            {
                return HttpNotFound();
            }

            return View(lodgingFacility);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="cmd">コマンド</param>
        /// <param name="lodgingFacility">宿泊施設情報</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string cmd, [Bind(Include = "LodgingCd,LodgingName,TelNo,PostalNo,Address")] M_LodgingFacility lodgingFacility)
        {
            Trace.WriteLine("POST /Lodging/Edit/" + lodgingFacility.LodgingCd);

            // 更新ボタンが押下された場合
            if (AppConstant.CMD_UPDATE.Equals(cmd))
            {
                if (ModelState.IsValid)
                {
                    // 更新処理
                    db.Entry(lodgingFacility).State = EntityState.Modified;
                    db.SaveChanges();
                    // 一覧へリダイレクト
                    return RedirectToAction("List");
                }
                else
                {
                    ViewBag.ErrorMessage = "必須項目（宿泊施設名）を設定してください。";
                }
            }

            // その他
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            return View(lodgingFacility);
        }

        /// <summary>
        /// 削除処理
        /// </summary>
        /// <param name="cd">宿泊施設コード</param>
        /// <returns></returns>
        public ActionResult Delete(string cd)
        {
            Trace.WriteLine("GET /Lodging/Delete/" + cd);

            // コードが空の場合、エラー
            if (string.IsNullOrEmpty(cd))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // 宿泊施設情報を取得
            M_LodgingFacility lodgingFacility = db.LodgingFacility.Find(cd);
            if (lodgingFacility == null)
            {
                return HttpNotFound();
            }

            return View(lodgingFacility);
        }

        /// <summary>
        /// 削除処理
        /// </summary>
        /// <param name="cd">宿泊施設コード</param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed([Bind(Include = "LodgingCd")] M_LodgingFacility lodgingFacility)
        {
            Trace.WriteLine("POST /Lodging/Delete/" + lodgingFacility.LodgingCd);

            // 宿泊施設情報を取得
            M_LodgingFacility target = db.LodgingFacility.Find(lodgingFacility.LodgingCd);
            if (target != null)
            {
                // 宿泊施設が設定されている教習生情報を取得
                List<T_TraineeLodging> traineeList = db.TraineeLodging.Where(x => x.LodgingCd.Equals(target.LodgingCd)).ToList();
                foreach (T_TraineeLodging trainee in traineeList)
                {
                    // 宿泊施設コードを空にして更新
                    trainee.LodgingCd = null;
                    // マスタを空に設定
                    trainee.TrainingCourse = null;
                    trainee.LodgingFacility = null;
                    db.Entry(trainee).State = EntityState.Modified;
                }
                // 宿泊施設情報を削除
                db.LodgingFacility.Remove(target);
            }
            db.SaveChanges();

            // 一覧へリダイレクト
            return RedirectToAction("List");
        }

        /// <summary>
        /// 宿泊状況確認
        /// </summary>
        /// <param name="cmd">コマンド</param>
        /// <param name="lodgingCfm">宿泊状況確認情報</param>
        /// <returns></returns>
        public ActionResult Confirm(string cmd, [Bind(Include = "DateFrom,DateTo,LodgingCd")] V_LodgingCfm lodgingCfm)
        {
            Trace.WriteLine("GET /Lodging/Confirm");

            // 教習生情報を初期化
            lodgingCfm.Trainee = new List<T_TraineeLodging>();

            // コマンドが空（初回表示）でない場合
            if (!string.IsNullOrEmpty(cmd))
            {
                // 検索ボタンが押下された場合
                if (AppConstant.CMD_SEARCH.Equals(cmd))
                {
                    bool validation = true;

                    // 入力チェック
                    if (lodgingCfm.DateFrom == null || lodgingCfm.DateTo == null || string.IsNullOrEmpty(lodgingCfm.LodgingCd))
                    {
                        ViewBag.ErrorMessage = "検索条件を指定してください。";
                        validation = false;
                    }
                    // 前後チェック
                    if (validation == true && (lodgingCfm.DateFrom > lodgingCfm.DateTo))
                    {
                        ViewBag.ErrorMessage = "日付の前後関係が不正です。";
                        validation = false;
                    }

                    if (validation == true)
                    {
                        // 宿泊施設が一致かつ、対象期間に在籍する合宿教習生を取得
                        lodgingCfm.Trainee = db.TraineeLodging.Where(
                            x => x.LodgingCd.Equals(lodgingCfm.LodgingCd)
                            && (lodgingCfm.DateFrom <= x.EntrancePlanDate && x.EntrancePlanDate <= lodgingCfm.DateTo
                            || lodgingCfm.DateFrom <= x.GraduatePlanDate && x.GraduatePlanDate <= lodgingCfm.DateTo
                            || x.EntrancePlanDate < lodgingCfm.DateFrom && lodgingCfm.DateTo < x.GraduatePlanDate)).ToList();
                    }
                }

                // その他
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }

            // ドロップダウンリストの選択肢を設定
            lodgingCfm.SelectLodging = new SelectList(db.LodgingFacility.OrderBy(x => x.LodgingCd).ToList(), "LodgingCd", "LodgingName", lodgingCfm.LodgingCd);

            return View(lodgingCfm);
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