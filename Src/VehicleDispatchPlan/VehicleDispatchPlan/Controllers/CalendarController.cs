using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using VehicleDispatchPlan.Models;

/**
 * 入卒カレンダーコントローラ
 *
 * @author t-murayama
 * @version 1.0
 * ----------------------------------
 * 2020/03/01 t-murayama 新規作成
 *
 */
namespace VehicleDispatchPlan_Dev.Controllers
{
    public class CalendarController : Controller
    {
        // データベースコンテキスト
        private MyDatabaseContext db = new MyDatabaseContext();

        // GET: Calendar
        public ActionResult List(string SelectYear, string SelectMonth)
        {
            List<M_EntGrdCalendar> calendarList = new List<M_EntGrdCalendar>();
            // 年、月を指定して取得
            if (!string.IsNullOrEmpty(SelectYear) && !string.IsNullOrEmpty(SelectMonth))
            {
                calendarList = db.EntGrdCalendar.Where(
                    x => ((DateTime)x.EntrancePlanDate).Year.ToString().Equals(SelectYear)
                    && ((DateTime)x.EntrancePlanDate).Month.ToString().Equals(SelectMonth)).ToList();
            }

            // ドロップダウンリストの選択肢を設定
            this.SetSelectItem(calendarList);
            this.SetDisplayItem();

            return View(calendarList);
        }

        [HttpPost]
        public ActionResult List(string cmd, [Bind(Include = "TrainingCourseCd,EntrancePlanDate,TmpLicencePlanDate,GraduatePlanDate")] List<M_EntGrdCalendar> calendarList)
        {
            Trace.WriteLine("POST /Trainee/Edit");

            if ("更新".Equals(cmd))
            {
                if (ModelState.IsValid)
                {
                    // 重複チェック
                    int repeatedNum = calendarList.GroupBy(x => new { x.TrainingCourseCd, x.EntrancePlanDate })
                        .Select(x => new { Count = x.Count() }).Where(x => x.Count != 1).Count();
                    if (repeatedNum > 0)
                    {
                        TempData["errorMessage"] = "教習コース、入校予定日の重複データがあります。";
                    }
                    // TODO:データの登録/更新
                }
                else
                {
                    TempData["errorMessage"] = "必須項目（教習コース、入校予定日、仮免予定日、卒業予定日）を設定してください。";
                }
            }

            // ドロップダウンリストの選択肢を設定
            this.SetSelectItem(calendarList);
            this.SetDisplayItem();

            return View(calendarList);
        }

        // GET: Calendar
        public ActionResult Import()
        {
            return View(new List<M_EntGrdCalendar>());
        }

        [HttpPost]
        public ActionResult Import(string cmd, HttpPostedFileBase postedFile, [Bind(Include = "TrainingCourseCd,EntrancePlanDate,TmpLicencePlanDate,GraduatePlanDate")] List<M_EntGrdCalendar> calendarList)
        {
            if ("読込".Equals(cmd))
            {
                // カレンダーを初期化
                calendarList = new List<M_EntGrdCalendar>();

                if (postedFile != null)
                {
                    // 拡張子チェック
                    string extension = Path.GetExtension(postedFile.FileName);
                    if (!".csv".Equals(extension) && !".CSV".Equals(extension))
                    {
                        // エラーメッセージ
                        TempData["errorMessage"] = "ファイルはcsv形式を指定してください。";
                        return View(calendarList);
                    }

                    // 教習コースマスタ取得
                    List<M_TrainingCourse> trainingCourse = db.TrainingCourse.ToList();

                    // アップロード先ディレクトリ
                    string uploadDir = AppDomain.CurrentDomain.BaseDirectory + @"Uploads\";
                    // ディレクトリが存在しない場合は作成
                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }

                    // ファイルをサーバーに保存
                    string filepath = uploadDir + Path.GetFileName(postedFile.FileName);
                    postedFile.SaveAs(filepath);
                    // テキストを全行読み込み
                    using (StreamReader sr = new StreamReader(filepath, Encoding.UTF8))
                    {
                        while (!sr.EndOfStream)
                        {
                            // CSVファイルの一行を読み込む
                            string line = sr.ReadLine();
                            // 読み込んだ一行をカンマ毎に分けて配列に格納
                            string[] values = line.Split(',');

                            // 教習コース名
                            string trainingCourseName;
                            // 入校予定日
                            DateTime entrancePlanDate;
                            // 仮免予定日
                            DateTime tmpLicencePlanDate;
                            // 卒業予定日
                            DateTime graduatePlanDate;

                            // CSV項目数チェック
                            if (values.Count() != 4)
                            {
                                TempData["errorMessage"] = "csvの項目数に誤りがあるため、読み込みを途中で終了しました。";
                                break;
                            }

                            // ----- 教習コース -----
                            // 必須チェック
                            if (string.IsNullOrEmpty(values[0]))
                            {
                                TempData["errorMessage"] = "教習コースが未設定のため、読み込みを途中で終了しました。";
                                break;
                            }
                            // マスタ存在チェック
                            trainingCourseName = trainingCourse.Where(
                                x => x.TrainingCourseCd.Equals(values[0])).Select(x => x.TrainingCourseName).FirstOrDefault();
                            if (string.IsNullOrEmpty(trainingCourseName))
                            {
                                TempData["errorMessage"] = "教習コースの設定が不正のため、読み込みを途中で終了しました。";
                                break;
                            }

                            // ----- 入校予定日 -----
                            // 必須チェック
                            if (string.IsNullOrEmpty(values[1]))
                            {
                                TempData["errorMessage"] = "入校予定日が未設定のため、読み込みを途中で終了しました。";
                                break;
                            }
                            // 日付整合性チェック
                            if (!DateTime.TryParse(values[1], out entrancePlanDate))
                            {
                                TempData["errorMessage"] = "入校予定日の設定が不正のため、読み込みを途中で終了しました。";
                                break;
                            }

                            // ----- 仮免予定日 -----
                            // 必須チェック
                            if (string.IsNullOrEmpty(values[2]))
                            {
                                TempData["errorMessage"] = "仮免予定日が未設定のため、読み込みを途中で終了しました。";
                                break;
                            }
                            // 日付整合性チェック
                            if (!DateTime.TryParse(values[2], out tmpLicencePlanDate))
                            {
                                TempData["errorMessage"] = "仮免予定日の設定が不正のため、読み込みを途中で終了しました。";
                                break;
                            }

                            // ----- 卒業予定日 -----
                            // 必須チェック
                            if (string.IsNullOrEmpty(values[3]))
                            {
                                TempData["errorMessage"] = "卒業予定日が未設定のため、読み込みを途中で終了しました。";
                                break;
                            }
                            // 日付整合性チェック
                            if (!DateTime.TryParse(values[3], out graduatePlanDate))
                            {
                                TempData["errorMessage"] = "卒業予定日の設定が不正のため、読み込みを途中で終了しました。";
                                break;
                            }

                            // 各項目を設定
                            M_EntGrdCalendar calendar = new M_EntGrdCalendar();
                            calendar.TrainingCourseCd = values[0];
                            calendar.TrainingCourseName = trainingCourseName;
                            calendar.EntrancePlanDate = entrancePlanDate;
                            calendar.TmpLicencePlanDate = tmpLicencePlanDate;
                            calendar.GraduatePlanDate = graduatePlanDate;
                            // リストに追加
                            calendarList.Add(calendar);
                        }
                    }
                }
                else
                {
                    TempData["errorMessage"] = "ファイルを選択してください。";
                }
            }

            else if ("登録".Equals(cmd))
            {
                if (ModelState.IsValid)
                {
                    // 重複チェック
                    int repeatedNum = calendarList.GroupBy(x => new { x.TrainingCourseCd, x.EntrancePlanDate })
                        .Select(x => new { Count = x.Count() }).Where(x => x.Count != 1).Count();
                    if (repeatedNum > 0)
                    {
                        TempData["errorMessage"] = "教習コース、入校予定日の重複データがあります。";
                    }
                    else
                    {
                        // トランザクション作成
                        using (DbContextTransaction tran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                // データの登録/更新
                                foreach (M_EntGrdCalendar calendar in calendarList)
                                {
                                    // 存在チェック
                                    if (db.EntGrdCalendar.Where(x => x.TrainingCourseCd.Equals(calendar.TrainingCourseCd) 
                                        && ((DateTime)x.EntrancePlanDate).Equals((DateTime)calendar.EntrancePlanDate)).Count() == 0)
                                    {
                                        // 登録処理
                                        db.EntGrdCalendar.Add(calendar);
                                    }
                                    else
                                    {
                                        // 更新処理
                                        db.Entry(calendar).State = EntityState.Modified;
                                    }
                                }
                                db.SaveChanges();
                                // コミット
                                tran.Commit();
                                // 完了メッセージ
                                TempData["compMessage"] = "インポートが完了しました。";
                                // 表示データを初期化
                                calendarList = new List<M_EntGrdCalendar>();
                            }
                            catch (Exception e)
                            {
                                // ロールバック
                                tran.Rollback();
                                throw e;
                            }
                        }
                    }
                }
                else
                {
                    TempData["errorMessage"] = "必須項目（教習コース、入校予定日、仮免予定日、卒業予定日）を設定してください。";
                }
            }

            // ドロップダウンリストの選択肢を設定
            this.SetSelectItem(calendarList);

            return View(calendarList);
        }

        /// <summary>
        /// 画面項目を設定
        /// </summary>
        private void SetDisplayItem()
        {
            // 年の選択肢を設定
            int nowYear = DateTime.Now.Year;
            List<SelectListItem> selectYear = new List<SelectListItem>()
            {
                new SelectListItem() { Text = (nowYear - 5).ToString(), Value=(nowYear - 2).ToString() }
                , new SelectListItem() { Text = (nowYear - 4).ToString(), Value=(nowYear - 2).ToString() }
                , new SelectListItem() { Text = (nowYear - 3).ToString(), Value=(nowYear - 2).ToString() }
                , new SelectListItem() { Text = (nowYear - 2).ToString(), Value=(nowYear - 2).ToString() }
                , new SelectListItem() { Text = (nowYear - 1).ToString(), Value=(nowYear - 1).ToString() }
                , new SelectListItem() { Text = nowYear.ToString(), Value=nowYear.ToString() }
                , new SelectListItem() { Text = (nowYear + 1).ToString(), Value=(nowYear + 1).ToString() }
                , new SelectListItem() { Text = (nowYear + 2).ToString(), Value=(nowYear + 2).ToString() }
                , new SelectListItem() { Text = (nowYear + 3).ToString(), Value=(nowYear + 2).ToString() }
                , new SelectListItem() { Text = (nowYear + 4).ToString(), Value=(nowYear + 2).ToString() }
                , new SelectListItem() { Text = (nowYear + 5).ToString(), Value=(nowYear + 2).ToString() }
            };
            ViewBag.SelectYear = selectYear;
        }

        /// <summary>
        /// ドロップダウンリストの選択肢を設定
        /// </summary>
        private void SetSelectItem(List<M_EntGrdCalendar> calendarList)
        {
            // 教習コースマスタ取得
            List<M_TrainingCourse> trainingCourse = db.TrainingCourse.ToList();

            for (int i = 0; i < calendarList.Count(); i++)
            {
                // 教習コースの選択肢設定
                calendarList[i].SelectTrainingCourse = new SelectList(trainingCourse, "TrainingCourseCd", "TrainingCourseName", calendarList[i].TrainingCourseCd);
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
