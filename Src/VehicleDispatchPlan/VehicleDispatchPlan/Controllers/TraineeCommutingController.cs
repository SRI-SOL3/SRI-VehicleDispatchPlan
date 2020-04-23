using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using VehicleDispatchPlan.Commons;
using VehicleDispatchPlan.Constants;
using VehicleDispatchPlan.Models;

/**
 * 通学教習生管理コントローラ
 *
 * @author t-murayama
 * @version 1.0
 * ----------------------------------
 * 2020/04/17 土井勇紀　複製
 *
 */
namespace VehicleDispatchPlan.Controllers
{
    public class TraineeCommutingController : Controller
    {
        // データベースコンテキスト
        private MyDatabaseContext db = new MyDatabaseContext();

        /// <summary>
        /// 一覧表示
        /// </summary>
        /// <param name="planDateFrom">入校予定日From</param>
        /// <param name="planDateTo">入校予定日To</param>
        /// <param name="page">ページ数</param>
        /// <returns></returns>
        public ActionResult List(DateTime? planDateFrom, DateTime? planDateTo, int? page)
        {
            Trace.WriteLine("GET /TraineeCommuting/List/" + page);

            // 日付型を"yyyy-MM-dd"形式の文字列に変換
            ViewBag.PlanDateFrom = planDateFrom != null ? ((DateTime)planDateFrom).ToString("yyyy-MM-dd") : null;
            ViewBag.PlanDateTo = planDateTo != null ? ((DateTime)planDateTo).ToString("yyyy-MM-dd") : null;

            // nullの場合は0001/01/01～9999/12/31を設定
            DateTime dateFrom = planDateFrom ?? new DateTime(0001, 01, 01);
            DateTime dateTo = planDateTo ?? new DateTime(9999, 12, 31);

            // 教習生一覧情報を取得
            List<T_TraineeCommuting> tarineeList = db.TraineeCommuting.Where(x => dateFrom <= x.EntrancePlanDate && x.EntrancePlanDate <= dateTo).ToList();
            
            int pageSize = 20;
            int pageNumber = page ?? 1;

            return View(tarineeList.ToPagedList(pageNumber, pageSize));
        }

        /// <summary>
        /// 詳細表示
        /// </summary>
        /// <param name="id">教習生ID</param>
        /// <returns></returns>
        public ActionResult Details(int? id)
        {
            Trace.WriteLine("GET /TraineeCommuting/Details/" + id);

            // IDが空の場合、エラー
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // 教習生情報を取得
            T_TraineeCommuting trainee = db.TraineeCommuting.Find(id);
            if (trainee == null)
            {
                return HttpNotFound();
            }

            return View(trainee);
        }

        /// <summary>
        /// 登録表示
        /// </summary>
        /// <returns></returns>
        public ActionResult Regist()
        {
            Trace.WriteLine("GET /TraineeCommuting/Regist");

            V_TraineeCommutingEdt traineeEdt = new V_TraineeCommutingEdt();

            // 教習生のインスタンスを生成
            traineeEdt.Trainee = new T_TraineeCommuting() { ReserveDate = DateTime.Today };

            // 編集モードを設定
            traineeEdt.EditMode = AppConstant.EditMode.Edit;

            // ドロップダウンリストの選択肢を設定
            this.SetSelectItem(traineeEdt);

            return View(traineeEdt);
        }

        /// <summary>
        /// 登録処理
        /// </summary>
        /// <param name="cmd">コマンド</param>
        /// <param name="index">インデックス</param>
        /// <param name="traineeEdt">教習生登録情報</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Regist(string cmd, int? index, [Bind(Include = "Trainee")] V_TraineeCommutingEdt traineeEdt)
        {
            Trace.WriteLine("POST /TraineeCommuting/Regist");

            // 確認ボタンが押下された場合（TODO:本来はDBに一時登録データを持つべき？）
            if (AppConstant.CMD_CONFIRM.Equals(cmd))
            {
                // 入力チェック
                bool validation = true;
                if (ModelState.IsValid)
                {
                    // 入校予定日、仮免予定日の比較
                    if (traineeEdt.Trainee.EntrancePlanDate >= traineeEdt.Trainee.TmpLicencePlanDate)
                    {
                        ViewBag.ErrorMessage = "仮免予定日は入校予定日より後に設定してください。";
                        validation = false;
                    }

                    // 仮免予定日、卒業予定日の比較
                    if (validation == true && traineeEdt.Trainee.TmpLicencePlanDate >= traineeEdt.Trainee.GraduatePlanDate)
                    {
                        ViewBag.ErrorMessage = "卒業予定日は仮免予定日より後に設定してください。";
                        validation = false;
                    }
                }
                else
                {
                    // エラーメッセージを生成
                    ViewBag.ErrorMessage = new Utility().GetErrorMessage(ModelState);
                    validation = false;
                }

                if (validation == true)
                {
                    // 確認モードに変更
                    traineeEdt.EditMode = AppConstant.EditMode.Confirm;
                    // 入校予定日－10日（ＴＯＤＯ：いったん－10日としている）
                    DateTime dateFrom = ((DateTime)traineeEdt.Trainee.EntrancePlanDate).AddDays(-10);
                    // 卒業予定日＋10日（ＴＯＤＯ：いったん＋10日としている）
                    DateTime dateTo = ((DateTime)traineeEdt.Trainee.GraduatePlanDate).AddDays(10);
                    // 表データを作成
                    traineeEdt.ChartData = new Utility().GetChartData(db, dateFrom, dateTo, null, traineeEdt.Trainee);
                }
                else
                {
                    // 編集モードに変更
                    traineeEdt.EditMode = AppConstant.EditMode.Edit;
                }
            }

            // 戻るボタンが押下された場合
            else if (AppConstant.CMD_RETURN.Equals(cmd))
            {
                // 編集モードに変更
                traineeEdt.EditMode = AppConstant.EditMode.Edit;
            }

            // 登録ボタンが押下された場合
            else if (AppConstant.CMD_REGIST.Equals(cmd))
            {
                // 外部キーマスタのリセット
                traineeEdt.Trainee.TrainingCourse = null;
                // 登録処理
                db.TraineeCommuting.Add(traineeEdt.Trainee);
                db.SaveChanges();

                // 一覧へリダイレクト
                return RedirectToAction("List");
            }

            // その他
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // ドロップダウンリストの選択肢を設定
            this.SetSelectItem(traineeEdt);

            return View(traineeEdt);
        }

        /// <summary>
        /// 更新表示
        /// </summary>
        /// <param name="id">教習生ID</param>
        /// <returns></returns>
        public ActionResult Edit(int id)
        {
            Trace.WriteLine("GET /TraineeCommuting/Edit/" + id);

            V_TraineeCommutingEdt traineeEdt = new V_TraineeCommutingEdt();

            // DBから教習生情報を取得
            traineeEdt.Trainee = db.TraineeCommuting.Find(id);
            if (traineeEdt.Trainee == null)
            {
                return HttpNotFound();
            }

            // 編集モードを設定
            traineeEdt.EditMode = AppConstant.EditMode.Edit;

            // ドロップダウンリストの選択肢を設定
            this.SetSelectItem(traineeEdt);

            return View(traineeEdt);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="cmd">コマンド</param>
        /// <param name="traineeEdt">教習生編集情報</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string cmd, [Bind(Include = "Trainee")] V_TraineeCommutingEdt traineeEdt)
        {
            Trace.WriteLine("POST /TraineeCommuting/Edit/" + traineeEdt.Trainee.TraineeId);

            // 確認ボタンが押下された場合
            if (AppConstant.CMD_CONFIRM.Equals(cmd))
            {
                // 入力チェック
                bool validation = true;
                if (ModelState.IsValid)
                {
                    // 入校予定日、仮免予定日の比較
                    if (traineeEdt.Trainee.EntrancePlanDate >= traineeEdt.Trainee.TmpLicencePlanDate)
                    {
                        ViewBag.ErrorMessage = "仮免予定日は入校予定日より後に設定してください。";
                        validation = false;
                    }

                    // 仮免予定日、卒業予定日の比較
                    if (validation == true && traineeEdt.Trainee.TmpLicencePlanDate >= traineeEdt.Trainee.GraduatePlanDate)
                    {
                        ViewBag.ErrorMessage = "卒業予定日は仮免予定日より後に設定してください。";
                        validation = false;
                    }
                }
                else
                {
                    // エラーメッセージを生成
                    ViewBag.ErrorMessage = new Utility().GetErrorMessage(ModelState);
                    validation = false;
                }

                if (validation == true)
                {
                    // 確認モードに設定
                    traineeEdt.EditMode = AppConstant.EditMode.Confirm;
                    // 入校予定日－10日（ＴＯＤＯ：いったん－10日としている）
                    DateTime dateFrom = ((DateTime)traineeEdt.Trainee.EntrancePlanDate).AddDays(-10);
                    // 卒業予定日＋10日（ＴＯＤＯ：いったん＋10日としている）
                    DateTime dateTo = ((DateTime)traineeEdt.Trainee.GraduatePlanDate).AddDays(10);
                    // 表データを作成
                    traineeEdt.ChartData = new Utility().GetChartData(db, dateFrom, dateTo, null, traineeEdt.Trainee);
                }
                else
                {
                    // 編集モードに変更
                    traineeEdt.EditMode = AppConstant.EditMode.Edit;
                }
            }

            // 戻るボタンが押下された場合
            else if (AppConstant.CMD_RETURN.Equals(cmd))
            {
                // 編集モードに変更
                traineeEdt.EditMode = AppConstant.EditMode.Edit;
            }

            // 更新ボタンが押下された場合
            else if (AppConstant.CMD_UPDATE.Equals(cmd))
            {
                // マスタは更新しないため、データをクリア（ＴＯＤＯ：他に良い方法があるか…？）
                traineeEdt.Trainee.TrainingCourse = null;
                // 更新処理
                db.Entry(traineeEdt.Trainee).State = EntityState.Modified;
                db.SaveChanges();
                // 一覧へリダイレクト
                return RedirectToAction("List");
            }

            // その他
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // ドロップダウンリストの選択肢を設定
            this.SetSelectItem(traineeEdt);

            return View(traineeEdt);
        }

        /// <summary>
        /// 削除処理
        /// </summary>
        /// <param name="id">教習生ID</param>
        /// <returns></returns>
        public ActionResult Delete(int? id)
        {
            Trace.WriteLine("GET /TraineeCommuting/Delete/" + id);

            // IDが空の場合、エラー
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // 教習生情報を取得
            T_TraineeCommuting trainee = db.TraineeCommuting.Find(id);
            if (trainee == null)
            {
                return HttpNotFound();
            }

            return View(trainee);
        }

        /// <summary>
        /// 削除処理
        /// </summary>
        /// <param name="id">教習生ID</param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Trace.WriteLine("POST /TraineeCommuting/Delete/" + id);

            // 教習生データを取得
            T_TraineeCommuting trainee = db.TraineeCommuting.Find(id);
            if (trainee != null)
            {
                // 削除
                db.TraineeCommuting.Remove(trainee);
                db.SaveChanges();
            }

            // 一覧へリダイレクト
            return RedirectToAction("List");
        }

        /// <summary>
        /// ドロップダウンリストの選択肢を設定
        /// </summary>
        /// <param name="traineeEdt">教習生編集情報</param>
        private void SetSelectItem(V_TraineeCommutingEdt traineeEdt)
        {
            // 編集モードの場合
            if (AppConstant.EditMode.Edit.Equals(traineeEdt.EditMode))
            {
                // 性別の選択肢設定
                traineeEdt.Trainee.SelectGender = new SelectList(new List<SelectListItem> {
                        new SelectListItem() { Text = AppConstant.GENDER_MALE, Value=AppConstant.GENDER_MALE },
                        new SelectListItem() { Text = AppConstant.GENDER_FEMALE, Value=AppConstant.GENDER_FEMALE }
                    }, "Value", "Text", traineeEdt.Trainee.Gender);
                // 教習コースの選択肢設定
                traineeEdt.Trainee.SelectTrainingCourse = new SelectList(db.TrainingCourse.OrderBy(x => x.TrainingCourseCd).ToList(), "TrainingCourseCd", "TrainingCourseName", traineeEdt.Trainee.TrainingCourseCd);
            }
            // 確認モードの場合
            else
            {
                // 教習コースを設定
                traineeEdt.Trainee.TrainingCourse = db.TrainingCourse.Find(traineeEdt.Trainee.TrainingCourseCd);
            }
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