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
 * 教習生管理コントローラ
 *
 * @author t-murayama
 * @version 1.0
 * ----------------------------------
 * 2020/03/01 t-murayama 新規作成
 *
 */
namespace VehicleDispatchPlan.Controllers
{
    public class TraineeController : Controller
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
            Trace.WriteLine("GET /Trainee/List");

            ViewBag.PlanDateFrom = planDateFrom;
            ViewBag.PlanDateTo = planDateTo;

            DateTime dateFrom = planDateFrom ?? new DateTime(0001, 01, 01);
            DateTime dateTo = planDateTo ?? new DateTime(9999, 12, 31);

            // 教習生一覧情報を取得
            List<T_Trainee> tarineeList = db.Trainee.Where(x => dateFrom <= x.EntrancePlanDate && x.EntrancePlanDate <= dateTo).ToList();

            int pageSize = 20;
            int pageNumber = (page ?? 1);

            return View(tarineeList.ToPagedList(pageNumber, pageSize));
        }

        /// <summary>
        /// 詳細表示
        /// </summary>
        /// <param name="id">教習生ID</param>
        /// <returns></returns>
        public ActionResult Details(int? id)
        {
            Trace.WriteLine("GET /Trainee/Details/" + id);

            // IDが空の場合、エラー
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // 教習生情報を取得
            T_Trainee trainee = db.Trainee.Find(id);
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
            Trace.WriteLine("GET /Trainee/Regist");

            V_TraineeReg traineeReg = new V_TraineeReg();

            // 教習生のインスタンスを生成
            traineeReg.TraineeList = new List<T_Trainee>();
            traineeReg.TraineeList.Add(new T_Trainee());

            // 編集モードを設定
            traineeReg.EditMode = AppConstant.EditMode.Edit;

            // ドロップダウンリストの選択肢を設定
            this.SetSelectItem(traineeReg);

            return View(traineeReg);
        }

        /// <summary>
        /// 登録処理
        /// </summary>
        /// <param name="trainee">教習生情報</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Regist(string cmd, string index, [Bind(Include = "TraineeList")] V_TraineeReg traineeReg)
        {
            Trace.WriteLine("POST /Trainee/Regist");

            // 確認ボタンが押下された場合（TODO:本来はDBに一時登録データを持つべき？）
            if ("確認".Equals(cmd))
            {
                // 入力チェック
                bool validation = true;
                if (ModelState.IsValid)
                {
                    // 日付比較
                    foreach (T_Trainee trainee in traineeReg.TraineeList)
                    {
                        // 入校予定日、仮免予定日の比較
                        if (trainee.EntrancePlanDate >= trainee.TmpLicencePlanDate)
                        {
                            TempData["errorMessage"] = "仮免予定日は入校予定日より後に設定してください。";
                            validation = false;
                            break;
                        }

                        // 仮免予定日、卒業予定日の比較
                        if (trainee.TmpLicencePlanDate >= trainee.GraduatePlanDate)
                        {
                            TempData["errorMessage"] = "卒業予定日は仮免予定日より後に設定してください。";
                            validation = false;
                            break;
                        }
                    }
                }
                else
                {
                    TempData["errorMessage"] = "必須項目（教習者名、通学種別、教習コース、入校予定日、仮免予定日、卒業予定日）を設定してください。";
                    validation = false;
                }

                if (validation == true)
                {
                    // 確認モードに変更
                    traineeReg.EditMode = AppConstant.EditMode.Confirm;
                    DateTime dateFrom = (DateTime)traineeReg.TraineeList.Select(x => x.EntrancePlanDate).Min();
                    DateTime dateTo = (DateTime)traineeReg.TraineeList.Select(x => x.GraduatePlanDate).Max();
                    // 表データを作成
                    Utility utility = new Utility();
                    traineeReg.ChartData = utility.getChartData(db, dateFrom, dateTo, null, null, traineeReg.TraineeList);
                }
                else
                {
                    // 編集モードに変更
                    traineeReg.EditMode = AppConstant.EditMode.Edit;
                }
            }

            // 戻るボタンが押下された場合
            else if ("戻る".Equals(cmd))
            {
                // 編集モードに変更
                traineeReg.EditMode = AppConstant.EditMode.Edit;
            }

            // 登録ボタンが押下された場合
            else if ("登録".Equals(cmd))
            {
                // トランザクション作成
                using (DbContextTransaction tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        // グループIDを加算
                        int groupId = db.Trainee.Select(x => x.GroupId).Max() + 1;

                        foreach (T_Trainee trainee in traineeReg.TraineeList)
                        {
                            // グループIDを設定
                            trainee.GroupId = groupId;
                            // 登録処理
                            db.Trainee.Add(trainee);
                        }
                        db.SaveChanges();
                        // コミット
                        tran.Commit();
                    }
                    catch (Exception e)
                    {
                        // ロールバック
                        tran.Rollback();
                        throw e;
                    }
                }
                // 一覧へリダイレクト
                return RedirectToAction("List");
            }

            // 仮免・卒業日設定ボタンが押下された場合
            else if ("仮免・卒業日設定".Equals(cmd))
            {
                // ステータスをクリア
                ModelState.Clear();
                // 編集モードを設定
                traineeReg.EditMode = AppConstant.EditMode.Edit;
                // インデックスを元に教習生を取得
                int i = Convert.ToInt32(index);
                T_Trainee trainee = traineeReg.TraineeList[i];

                // 合宿の場合
                if (AppConstant.CD_ATTEND_TYPE_LODGING.Equals(trainee.AttendTypeCd))
                {
                    // 教習コース、入校予定日の必須チェック
                    if (!string.IsNullOrEmpty(trainee.TrainingCourseCd) && trainee.EntrancePlanDate != null)
                    {
                        // カレンダーテーブルから取得
                       M_EntGrdCalendar calendar = db.EntGrdCalendar.Where(
                           x => x.TrainingCourseCd.Equals(trainee.TrainingCourseCd) && ((DateTime)x.EntrancePlanDate).Equals((DateTime)trainee.EntrancePlanDate)).FirstOrDefault();
                        if (calendar != null)
                        {
                            // 仮免予定日
                            traineeReg.TraineeList[i].TmpLicencePlanDate = calendar.TmpLicencePlanDate;
                            // 卒業予定日
                            traineeReg.TraineeList[i].GraduatePlanDate = calendar.GraduatePlanDate;
                        }
                        else
                        {
                            // エラー
                            TempData["errorMessage"] = "入校予定日:" + ((DateTime)trainee.EntrancePlanDate).ToString("yyyy/MM/dd") + " のデータが入卒カレンダーに登録されていません。";
                        }
                    }
                    else
                    {
                        // エラー
                        TempData["errorMessage"] = "教習コース、入校予定日を設定してください。";
                    }
                }
                // 合宿以外(通学)の場合
                else
                {
                    // エラー
                    TempData["errorMessage"] = "通学種別が合宿ではない場合は入卒カレンダーからの設定はできません。";
                }
            }

            // 追加ボタンが押下された場合
            else if ("追加".Equals(cmd))
            {
                // ステータスをクリア
                ModelState.Clear();
                // 編集モードを設定
                traineeReg.EditMode = AppConstant.EditMode.Edit;
                // インデックスを元に教習生を追加
                int i = Convert.ToInt32(index);
                traineeReg.TraineeList.Insert(i + 1, new T_Trainee(traineeReg.TraineeList[i]));
                // 教習者名をクリア
                traineeReg.TraineeList[i + 1].TraineeName = "";
            }

            // 削除ボタンが押下された場合
            else if ("削除".Equals(cmd))
            {
                // ステータスをクリア
                ModelState.Clear();
                // 編集モードを設定
                traineeReg.EditMode = AppConstant.EditMode.Edit;
                // インデックスを元に教習生を削除
                int i = Convert.ToInt32(index);
                traineeReg.TraineeList.RemoveAt(i);
            }

            // ドロップダウンリストの選択肢を設定
            this.SetSelectItem(traineeReg);

            return View(traineeReg);
        }

        /// <summary>
        /// 更新表示
        /// </summary>
        /// <returns></returns>
        public ActionResult Edit(int id)
        {
            Trace.WriteLine("GET /Trainee/Edit");

            V_TraineeEdt traineeEdt = new V_TraineeEdt();

            // DBから教習生情報を取得
            traineeEdt.Trainee = db.Trainee.Find(id);
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
        /// <param name="trainee">教習生情報</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string cmd, [Bind(Include = "Trainee")] V_TraineeEdt traineeEdt)
        {
            Trace.WriteLine("POST /Trainee/Edit/" + traineeEdt.Trainee.TraineeId);

            // 確認ボタンが押下された場合
            if ("確認".Equals(cmd))
            {
                // 入力チェック
                bool validation = true;
                if (ModelState.IsValid)
                {
                    // 入校予定日、仮免予定日の比較
                    if (traineeEdt.Trainee.EntrancePlanDate >= traineeEdt.Trainee.TmpLicencePlanDate)
                    {
                        TempData["errorMessage"] = "仮免予定日は入校予定日より後に設定してください。";
                        validation = false;
                    }

                    // 仮免予定日、卒業予定日の比較
                    if (traineeEdt.Trainee.TmpLicencePlanDate >= traineeEdt.Trainee.GraduatePlanDate)
                    {
                        TempData["errorMessage"] = "卒業予定日は仮免予定日より後に設定してください。";
                        validation = false;
                    }
                }
                else
                {
                    TempData["errorMessage"] = "必須項目（教習者名、通学種別、教習コース、入校予定日、仮免予定日、卒業予定日）を設定してください。";
                    validation = false;
                }

                if (validation == true)
                {
                    // 確認モードに設定
                    traineeEdt.EditMode = AppConstant.EditMode.Confirm;
                    // 表データを作成
                    Utility utility = new Utility();
                    traineeEdt.ChartData = utility.getChartData(db, (DateTime)traineeEdt.Trainee.EntrancePlanDate, (DateTime)traineeEdt.Trainee.GraduatePlanDate, null, null, new List<T_Trainee> { traineeEdt.Trainee });
                }
                else
                {
                    // 編集モードに変更
                    traineeEdt.EditMode = AppConstant.EditMode.Edit;
                }
            }

            // 戻るボタンが押下された場合
            else if ("戻る".Equals(cmd))
            {
                // 編集モードに変更
                traineeEdt.EditMode = AppConstant.EditMode.Edit;
            }

            // 更新ボタンが押下された場合
            else if ("更新".Equals(cmd))
            {
                // 更新処理
                db.Entry(traineeEdt.Trainee).State = EntityState.Modified;
                db.SaveChanges();
                // 一覧へリダイレクト
                return RedirectToAction("List");
            }

            // 仮免・卒業日設定ボタンが押下された場合
            else if ("仮免・卒業日設定".Equals(cmd))
            {
                // ステータスをクリア
                ModelState.Clear();
                // 編集モードを設定
                traineeEdt.EditMode = AppConstant.EditMode.Edit;

                // 合宿の場合
                if (AppConstant.CD_ATTEND_TYPE_LODGING.Equals(traineeEdt.Trainee.AttendTypeCd))
                {
                    // 教習コース、入校予定日の必須チェック
                    if (!string.IsNullOrEmpty(traineeEdt.Trainee.TrainingCourseCd) && traineeEdt.Trainee.EntrancePlanDate != null)
                    {
                        // カレンダーテーブルから取得
                        M_EntGrdCalendar calendar = db.EntGrdCalendar.Where(
                            x => x.TrainingCourseCd.Equals(traineeEdt.Trainee.TrainingCourseCd) && ((DateTime)x.EntrancePlanDate).Equals((DateTime)traineeEdt.Trainee.EntrancePlanDate)).FirstOrDefault();
                        if (calendar != null)
                        {
                            // 仮免予定日
                            traineeEdt.Trainee.TmpLicencePlanDate = calendar.TmpLicencePlanDate;
                            // 卒業予定日
                            traineeEdt.Trainee.GraduatePlanDate = calendar.GraduatePlanDate;
                        }
                        else
                        {
                            // エラー
                            TempData["errorMessage"] = "入校予定日：のデータが入卒カレンダーに登録されていません。";
                        }
                    }
                    else
                    {
                        // エラー
                        TempData["errorMessage"] = "教習コース、入校予定日を設定してください。";
                    }
                }
                // 合宿以外(通学)の場合
                else
                {
                    // エラー
                    TempData["errorMessage"] = "通学の場合は入卒カレンダーからの設定はできません。";
                }
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
            Trace.WriteLine("GET /Trainee/Delete/" + id);

            // IDが空の場合、エラー
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // 教習生情報を取得
            T_Trainee trainee = db.Trainee.Find(id);
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
            Trace.WriteLine("POST /Trainee/Delete/" + id);

            // 教習生データを取得
            T_Trainee trainee = db.Trainee.Find(id);
            // 削除
            db.Trainee.Remove(trainee);
            db.SaveChanges();

            // 一覧へリダイレクト
            return RedirectToAction("List");
        }

        /// <summary>
        /// ドロップダウンリストの選択肢を設定
        /// </summary>
        private void SetSelectItem(V_TraineeEdt traineeEdt)
        {
            // 編集モードの場合
            if (AppConstant.EditMode.Edit.Equals(traineeEdt.EditMode))
            {
                // 通学種別の選択肢設定
                traineeEdt.Trainee.SelectAttendType = new SelectList(db.AttendType.ToList(), "AttendTypeCd", "AttendTypeName", traineeEdt.Trainee.AttendTypeCd);
                // 教習コースの選択肢設定
                traineeEdt.Trainee.SelectTrainingCourse = new SelectList(db.TrainingCourse.ToList(), "TrainingCourseCd", "TrainingCourseName", traineeEdt.Trainee.TrainingCourseCd);
                // 宿泊施設の選択肢設定
                traineeEdt.Trainee.SelectLodging = new SelectList(db.LodgingFacility.ToList(), "LodgingCd", "LodgingName", traineeEdt.Trainee.LodgingCd);
            }
            // 確認モードの場合
            else
            {
                // 通学種別を設定
                traineeEdt.Trainee.AttendType = db.AttendType.Find(traineeEdt.Trainee.AttendTypeCd);
                // 教習コースを設定
                traineeEdt.Trainee.TrainingCourse = db.TrainingCourse.Find(traineeEdt.Trainee.TrainingCourseCd);
                // 宿泊施設を設定
                traineeEdt.Trainee.LodgingFacility = db.LodgingFacility.Find(traineeEdt.Trainee.LodgingCd);
            }
        }

        /// <summary>
        /// ドロップダウンリストの選択肢を設定
        /// </summary>
        private void SetSelectItem(V_TraineeReg traineeReg)
        {
            // 編集モードの場合
            if (AppConstant.EditMode.Edit.Equals(traineeReg.EditMode))
            {
                for (int i = 0; i < traineeReg.TraineeList.Count(); i++)
                {
                    // 通学種別の選択肢設定
                    traineeReg.TraineeList[i].SelectAttendType = new SelectList(db.AttendType.ToList(), "AttendTypeCd", "AttendTypeName", traineeReg.TraineeList[i].AttendTypeCd);
                    // 教習コースの選択肢設定
                    traineeReg.TraineeList[i].SelectTrainingCourse = new SelectList(db.TrainingCourse.ToList(), "TrainingCourseCd", "TrainingCourseName", traineeReg.TraineeList[i].TrainingCourseCd);
                    // 宿泊施設の選択肢設定
                    traineeReg.TraineeList[i].SelectLodging = new SelectList(db.LodgingFacility.ToList(), "LodgingCd", "LodgingName", traineeReg.TraineeList[i].LodgingCd);
                }
            }
            // 確認モードの場合
            else
            {
                for (int i = 0; i < traineeReg.TraineeList.Count(); i++)
                {
                    // 通学種別名を設定
                    traineeReg.TraineeList[i].AttendType = db.AttendType.Find(traineeReg.TraineeList[i].AttendTypeCd);
                    // 教習コース名を設定
                    traineeReg.TraineeList[i].TrainingCourse = db.TrainingCourse.Find(traineeReg.TraineeList[i].TrainingCourseCd);
                    // 宿泊施設名を設定
                    traineeReg.TraineeList[i].LodgingFacility = db.LodgingFacility.Find(traineeReg.TraineeList[i].LodgingCd);
                }
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