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
using VehicleDispatchPlan.Constants;
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

       /// <summary>
       /// 一覧表示
       /// </summary>
       /// <param name="selectYear">対象年</param>
       /// <param name="selectMonth">対象月</param>
       /// <returns></returns>
        public ActionResult List(string selectYear, string selectMonth)
        {
            List<M_EntGrdCalendar> calendarList = new List<M_EntGrdCalendar>();
            // 年、月を指定して取得
            if (!string.IsNullOrEmpty(selectYear) && !string.IsNullOrEmpty(selectMonth))
            {
                calendarList = db.EntGrdCalendar.Where(
                    x => ((DateTime)x.EntrancePlanDate).Year.ToString().Equals(selectYear)
                    && ((DateTime)x.EntrancePlanDate).Month.ToString().Equals(selectMonth)).ToList();
            }

            // ドロップダウンリストの選択肢を設定
            this.SetSelectItem(calendarList);
            this.SetDisplayItem();

            return View(calendarList);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="cmd">コマンド</param>
        /// <param name="calendarList">カレンダー情報</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
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
                        ViewBag.ErrorMessage = "教習コース、入校予定日の重複データがあります。";
                    }
                    // ＴＯＤＯ：データの登録/更新
                }
                else
                {
                    ViewBag.ErrorMessage = "必須項目（教習コース、入校予定日、仮免予定日、卒業予定日）を設定してください。";
                }
            }

            // ドロップダウンリストの選択肢を設定
            this.SetSelectItem(calendarList);
            this.SetDisplayItem();

            return View(calendarList);
        }

        /// <summary>
        /// インポート表示
        /// </summary>
        /// <returns></returns>
        public ActionResult Import()
        {
            return View(new List<M_EntGrdCalendar>());
        }

        /// <summary>
        /// インポート処理
        /// </summary>
        /// <param name="cmd">コマンド</param>
        /// <param name="postedFile">CSVファイル</param>
        /// <param name="calendarList">カレンダー情報</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Import(string cmd, HttpPostedFileBase postedFile, [Bind(Include = "TrainingCourseCd,EntrancePlanDate,TmpLicencePlanDate,GraduatePlanDate")] List<M_EntGrdCalendar> calendarList)
        {
            // 読込ボタンが押下された場合
            if (AppConstant.CMD_READ.Equals(cmd))
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
                        ViewBag.ErrorMessage = "ファイルはcsv形式を指定してください。";
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
                                ViewBag.ErrorMessage = "csvの項目数に誤りがあるため、読み込みを途中で終了しました。";
                                break;
                            }

                            // ----- 教習コース -----
                            // 必須チェック
                            if (string.IsNullOrEmpty(values[0]))
                            {
                                ViewBag.ErrorMessage = "教習コースが未設定のため、読み込みを途中で終了しました。";
                                break;
                            }
                            // マスタ存在チェック
                            trainingCourseName = trainingCourse.Where(
                                x => x.TrainingCourseCd.Equals(values[0])).Select(x => x.TrainingCourseName).FirstOrDefault();
                            if (string.IsNullOrEmpty(trainingCourseName))
                            {
                                ViewBag.ErrorMessage = "教習コースの設定が不正のため、読み込みを途中で終了しました。";
                                break;
                            }

                            // ----- 入校予定日 -----
                            // 必須チェック
                            if (string.IsNullOrEmpty(values[1]))
                            {
                                ViewBag.ErrorMessage = "入校予定日が未設定のため、読み込みを途中で終了しました。";
                                break;
                            }
                            // 日付整合性チェック
                            if (!DateTime.TryParse(values[1], out entrancePlanDate))
                            {
                                ViewBag.ErrorMessage = "入校予定日の設定が不正のため、読み込みを途中で終了しました。";
                                break;
                            }

                            // ----- 仮免予定日 -----
                            // 必須チェック
                            if (string.IsNullOrEmpty(values[2]))
                            {
                                ViewBag.ErrorMessage = "仮免予定日が未設定のため、読み込みを途中で終了しました。";
                                break;
                            }
                            // 日付整合性チェック
                            if (!DateTime.TryParse(values[2], out tmpLicencePlanDate))
                            {
                                ViewBag.ErrorMessage = "仮免予定日の設定が不正のため、読み込みを途中で終了しました。";
                                break;
                            }

                            // ----- 卒業予定日 -----
                            // 必須チェック
                            if (string.IsNullOrEmpty(values[3]))
                            {
                                ViewBag.ErrorMessage = "卒業予定日が未設定のため、読み込みを途中で終了しました。";
                                break;
                            }
                            // 日付整合性チェック
                            if (!DateTime.TryParse(values[3], out graduatePlanDate))
                            {
                                ViewBag.ErrorMessage = "卒業予定日の設定が不正のため、読み込みを途中で終了しました。";
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
                    ViewBag.ErrorMessage = "ファイルを選択してください。";
                }
            }

            // 登録ボタンが押下された場合
            else if (AppConstant.CMD_REGIST.Equals(cmd))
            {
                if (ModelState.IsValid)
                {
                    // 重複チェック
                    int repeatedNum = calendarList.GroupBy(x => new { x.TrainingCourseCd, x.EntrancePlanDate })
                        .Select(x => new { Count = x.Count() }).Where(x => x.Count != 1).Count();
                    if (repeatedNum > 0)
                    {
                        ViewBag.ErrorMessage = "教習コース、入校予定日の重複データがあります。";
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
                                ViewBag.CompMessage = "インポートが完了しました。";
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
                    ViewBag.ErrorMessage = "必須項目（教習コース、入校予定日、仮免予定日、卒業予定日）を設定してください。";
                }
            }

            // その他
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
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
                new SelectListItem() { Text = (nowYear - 5).ToString(), Value=(nowYear - 5).ToString() }
                , new SelectListItem() { Text = (nowYear - 4).ToString(), Value=(nowYear - 4).ToString() }
                , new SelectListItem() { Text = (nowYear - 3).ToString(), Value=(nowYear - 3).ToString() }
                , new SelectListItem() { Text = (nowYear - 2).ToString(), Value=(nowYear - 2).ToString() }
                , new SelectListItem() { Text = (nowYear - 1).ToString(), Value=(nowYear - 1).ToString() }
                , new SelectListItem() { Text = nowYear.ToString(), Value=nowYear.ToString() }
                , new SelectListItem() { Text = (nowYear + 1).ToString(), Value=(nowYear + 1).ToString() }
                , new SelectListItem() { Text = (nowYear + 2).ToString(), Value=(nowYear + 2).ToString() }
                , new SelectListItem() { Text = (nowYear + 3).ToString(), Value=(nowYear + 3).ToString() }
                , new SelectListItem() { Text = (nowYear + 4).ToString(), Value=(nowYear + 4).ToString() }
                , new SelectListItem() { Text = (nowYear + 5).ToString(), Value=(nowYear + 5).ToString() }
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
