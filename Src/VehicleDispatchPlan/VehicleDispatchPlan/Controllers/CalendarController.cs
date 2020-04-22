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
using VehicleDispatchPlan.Commons;
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
        /// <param name="cmd">コマンド</param>
        /// <param name="entGrdCalendarEdt">入卒カレンダー編集情報</param>
        /// <returns></returns>
        public ActionResult List(string cmd, [Bind(Include = "Year,Month")] V_EntGrdCalendarEdt entGrdCalendarEdt)
        {
            Trace.WriteLine("GET /Calendar/List");

            // カレンダー情報を初期化
            entGrdCalendarEdt.CalendarList = new List<M_EntGrdCalendar>();

            if (!string.IsNullOrEmpty(cmd))
            {
                // 検索ボタンが押下された場合
                if (AppConstant.CMD_SEARCH.Equals(cmd))
                {
                    // 年、月を指定して取得
                    if (!string.IsNullOrEmpty(entGrdCalendarEdt.Year) && !string.IsNullOrEmpty(entGrdCalendarEdt.Month))
                    {
                        // 年と月を指定してカレンダーを取得
                        entGrdCalendarEdt.CalendarList = db.EntGrdCalendar.Where(
                            x => ((DateTime)x.EntrancePlanDate).Year.ToString().Equals(entGrdCalendarEdt.Year)
                            && ((DateTime)x.EntrancePlanDate).Month.ToString().Equals(entGrdCalendarEdt.Month))
                            .OrderBy(x => x.EntrancePlanDate).ThenBy(x => x.TrainingCourseCd).ToList();
                        // 削除フラグをfalseに設定
                        entGrdCalendarEdt.CalendarList.ForEach(x => x.DeleteFlg = false);
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "検索条件を指定してください。";
                    }
                }

                // その他
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }


            return View(entGrdCalendarEdt);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="cmd">コマンド</param>
        /// <param name="calendarList">カレンダー情報</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult List(string cmd, int? index, [Bind(Include = "Year,Month,CalendarList")] V_EntGrdCalendarEdt entGrdCalendarEdt)
        {
            Trace.WriteLine("POST /Calendar/List");

            // 更新ボタンが押下された場合
            if (AppConstant.CMD_UPDATE.Equals(cmd))
            {
                if (ModelState.IsValid)
                {
                    foreach (M_EntGrdCalendar calendar in entGrdCalendarEdt.CalendarList)
                    {
                        if (calendar.DeleteFlg == false)
                        {
                            // 更新（ステータスを修正に設定）
                            db.Entry(calendar).State = EntityState.Modified;
                        }
                        else
                        {
                            // 削除対象を取得
                            M_EntGrdCalendar target = db.EntGrdCalendar.Find(calendar.TrainingCourseCd, calendar.EntrancePlanDate);
                            if (target != null)
                            {
                                // 削除処理
                                db.EntGrdCalendar.Remove(target);
                            }
                        }
                    }
                    db.SaveChanges();

                    // 年と月を指定してカレンダーを取得
                    entGrdCalendarEdt.CalendarList = db.EntGrdCalendar.Where(
                        x => ((DateTime)x.EntrancePlanDate).Year.ToString().Equals(entGrdCalendarEdt.Year)
                        && ((DateTime)x.EntrancePlanDate).Month.ToString().Equals(entGrdCalendarEdt.Month))
                        .OrderBy(x => x.EntrancePlanDate).ThenBy(x => x.TrainingCourseCd).ToList();
                    // 削除フラグをfalseに設定
                    entGrdCalendarEdt.CalendarList.ForEach(x => x.DeleteFlg = false);

                    // 完了メッセージ
                    ViewBag.CompMessage = "データを更新しました。";
                }
                else
                {
                    // エラーメッセージを生成
                    ViewBag.ErrorMessage = new Utility().getErrorMessage(ModelState);
                }
            }

            // 削除ボタンが押下された場合
            else if (AppConstant.CMD_REMOVE.Equals(cmd))
            {
                // ステータスをクリア
                ModelState.Clear();
                // インデックスを元にカレンダーを削除
                int i = (int)index;
                entGrdCalendarEdt.CalendarList[i].DeleteFlg = true;
            }

            // その他
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // 外部キーのマスタを設定
            this.SetForeignMaster(entGrdCalendarEdt.CalendarList);

            return View(entGrdCalendarEdt);
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
            Trace.WriteLine("POST /Calendar/Import");

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
                        int row = 0;
                        while (!sr.EndOfStream)
                        {
                            row++;
                            // CSVファイルの一行を読み込む
                            string line = sr.ReadLine();
                            // ヘッダ行はスキップ
                            if (row == 1)
                            {
                                continue;
                            }
                            // 読み込んだ一行をカンマ毎に分けて配列に格納
                            string[] values = line.Split(',');

                            // 入校予定日
                            DateTime entrancePlanDate;
                            // 教習コースコード
                            string trainingCourseCd;
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

                            // ----- 入校予定日 -----
                            // 必須チェック
                            if (string.IsNullOrEmpty(values[0]))
                            {
                                ViewBag.ErrorMessage = "入校予定日が未設定のため、読み込みを途中で終了しました。";
                                break;
                            }
                            // 日付整合性チェック
                            if (!DateTime.TryParse(values[0], out entrancePlanDate))
                            {
                                ViewBag.ErrorMessage = "入校予定日の設定が不正のため、読み込みを途中で終了しました。";
                                break;
                            }

                            // ----- 教習コース -----
                            // 必須チェック
                            if (string.IsNullOrEmpty(values[1]))
                            {
                                ViewBag.ErrorMessage = "教習コースが未設定のため、読み込みを途中で終了しました。";
                                break;
                            }
                            // マスタ存在チェック
                            trainingCourseCd = trainingCourse.Where(
                                x => x.TrainingCourseCd.Equals(values[1])).Select(x => x.TrainingCourseCd).FirstOrDefault();
                            if (string.IsNullOrEmpty(trainingCourseCd))
                            {
                                ViewBag.ErrorMessage = "教習コースの設定が不正のため、読み込みを途中で終了しました。";
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
                            M_EntGrdCalendar calendar = new M_EntGrdCalendar()
                            {
                                EntrancePlanDate = entrancePlanDate,
                                TrainingCourseCd = trainingCourseCd,
                                TmpLicencePlanDate = tmpLicencePlanDate,
                                GraduatePlanDate = graduatePlanDate
                            };
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
                        // 完了メッセージ
                        ViewBag.CompMessage = "インポートが完了しました。";
                        // 表示データを初期化
                        calendarList = new List<M_EntGrdCalendar>();
                    }
                }
                else
                {
                    // エラーメッセージを生成
                    ViewBag.ErrorMessage = new Utility().getErrorMessage(ModelState);
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
        /// 外部キーのマスターを設定
        /// </summary>
        private void SetForeignMaster(List<M_EntGrdCalendar> calendarList)
        {
            // 教習コースマスタ取得
            List<M_TrainingCourse> trainingCourse = db.TrainingCourse.ToList();

            for (int i = 0; i < calendarList.Count(); i++)
            {
                // 教習コースマスタを設定
                calendarList[i].TrainingCourse = trainingCourse.Where(x => x.TrainingCourseCd.Equals(calendarList[i].TrainingCourseCd)).FirstOrDefault();
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
