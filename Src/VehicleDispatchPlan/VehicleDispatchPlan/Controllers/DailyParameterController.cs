using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using VehicleDispatchPlan.Commons;
using VehicleDispatchPlan.Constants;
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
        /// <param name="dailyParameterEdt">日別予測条件編集情報</param>
        /// <returns></returns>
        public ActionResult Edit([Bind(Include = "SearchDate")] V_DailyParameterEdt dailyParameterEdt)
        {
            Trace.WriteLine("GET /DailyParameter/Edit/" + dailyParameterEdt.SearchDate);

            // ステータスをクリア
            ModelState.Clear();

            // インスタンスを生成
            dailyParameterEdt.DailyClasses = new T_DailyClasses();
            dailyParameterEdt.TrainerList = new List<T_DailyClassesByTrainer>();

            if (dailyParameterEdt.SearchDate != null)
            {
                // 日別予測条件を取得
                T_DailyClasses dailyClasses = db.DailyClasses.Find(dailyParameterEdt.SearchDate);
                // データが存在しない場合はインスタンスを生成
                if (dailyClasses == null)
                {
                    dailyClasses = new T_DailyClasses() { Date = dailyParameterEdt.SearchDate };
                }
                dailyParameterEdt.DailyClasses = dailyClasses;
                // 更新日付Toを設定
                dailyParameterEdt.UpdateTo = dailyClasses.Date;

                // 指導員コマ数を取得
                dailyParameterEdt.TrainerList = db.DailyClassesByTrainer.Where(x => ((DateTime)x.Date).Equals((DateTime)dailyParameterEdt.SearchDate)).OrderBy(x => x.No).ToList();
            }
            else
            {
                ViewBag.SearchErrorMessage = "検索条件を指定してください。";
            }

            // 指導員コマ管理画面から遷移するための検索日付をTempDataに設定
            TempData[AppConstant.TEMP_KEY_SEARCH_DATE] = dailyParameterEdt.SearchDate;

            return View(dailyParameterEdt);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="cmd">コマンド</param>
        /// <param name="dailyParameterEdt">日別予測条件編集情報</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string cmd, [Bind(Include = "DailyClasses,UpdateTo")] V_DailyParameterEdt dailyParameterEdt)
        {
            Trace.WriteLine("POST /DailyParameter/Edit/" + dailyParameterEdt.DailyClasses.Date + "&" + dailyParameterEdt.UpdateTo);

            // 更新ボタンが押下された場合
            if (AppConstant.CMD_UPDATE.Equals(cmd))
            {
                // 日付を設定
                dailyParameterEdt.SearchDate = dailyParameterEdt.DailyClasses.Date;

                // 入力チェック
                bool validation = true;
                if (ModelState.IsValid)
                {
                    // 更新日付Toの必須チェック
                    if (dailyParameterEdt.UpdateTo == null)
                    {
                        ViewBag.ErrorMessage = "更新範囲の日付を設定してください。";
                        validation = false;
                    }
                    // 日付の前後チェック
                    if (validation == true &&
                        dailyParameterEdt.UpdateTo < dailyParameterEdt.DailyClasses.Date)
                    {
                        ViewBag.ErrorMessage = "更新範囲の日付の前後関係が不正です。";
                        validation = false;
                    }
                    // 合宿比率[%]と通学比率[%]のチェック
                    if (validation == true &&
                        dailyParameterEdt.DailyClasses.LodgingRatio + dailyParameterEdt.DailyClasses.CommutingRatio != 100)
                    {
                        ViewBag.ErrorMessage = "合宿比率[%]と通学比率[%]は合わせて100になるように設定してください。";
                        validation = false;
                    }
                    // 合宿の在籍比率[%]（AT一段階/二段階、MT一段階/二段階）のチェック
                    if (validation == true &&
                        dailyParameterEdt.DailyClasses.LdgAtFstRatio + dailyParameterEdt.DailyClasses.LdgAtSndRatio + dailyParameterEdt.DailyClasses.LdgMtFstRatio + dailyParameterEdt.DailyClasses.LdgMtSndRatio != 100)
                    {
                        ViewBag.ErrorMessage = "合宿の在籍比率[%]（AT一段階/二段階、MT一段階/二段階）は合わせて100になるように設定してください。";
                        validation = false;
                    }
                    // 通学の在籍比率[%]（AT一段階/二段階、MT一段階/二段階）のチェック
                    if (validation == true &&
                        dailyParameterEdt.DailyClasses.CmtAtFstRatio + dailyParameterEdt.DailyClasses.CmtAtSndRatio + dailyParameterEdt.DailyClasses.CmtMtFstRatio + dailyParameterEdt.DailyClasses.CmtMtSndRatio != 100)
                    {
                        ViewBag.ErrorMessage = "通学の在籍比率[%]（AT一段階/二段階、MT一段階/二段階）は合わせて100になるように設定してください。";
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
                    // 指定した範囲で登録/更新処理を行う
                    for (DateTime date = (DateTime)dailyParameterEdt.DailyClasses.Date;
                        date.CompareTo((DateTime)dailyParameterEdt.UpdateTo) <= 0; date = date.AddDays(1))
                    {
                        // 登録/更新対象を設定
                        T_DailyClasses dailyClasses = new T_DailyClasses() { Date = date };
                        this.SetUpdateInfo(dailyClasses, dailyParameterEdt.DailyClasses);
                        // 存在チェック
                        if (db.DailyClasses.Where(x => ((DateTime)x.Date).Equals(date)).Count() == 0)
                        {
                            // 登録処理
                            db.DailyClasses.Add(dailyClasses);
                        }
                        else
                        {
                            // 更新処理
                            db.Entry(dailyClasses).State = EntityState.Modified;
                        }
                    }
                    db.SaveChanges();

                    // 完了メッセージ
                    ViewBag.CompMessage = "データを更新しました。";
                }

                // 指導員コマ数を取得
                dailyParameterEdt.TrainerList = db.DailyClassesByTrainer.Where(x => ((DateTime)x.Date).Equals((DateTime)dailyParameterEdt.SearchDate)).OrderBy(x => x.No).ToList();
            }

            // その他
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            return View(dailyParameterEdt);
        }

        /// <summary>
        /// 更新情報の設定
        /// </summary>
        /// <param name="target">設定対象</param>
        /// <param name="source">設定元</param>
        private void SetUpdateInfo(T_DailyClasses target, T_DailyClasses source)
        {
            // publicのメンバー変数を取得
            PropertyInfo[] propertyArray = source.GetType().GetProperties();
            foreach (PropertyInfo property in propertyArray)
            {
                // 日付はスキップ
                if ("Date".Equals(property.Name)) continue;
                // 対象項目を設定
                property.SetValue(target, property.GetValue(source));
            }
        }

        /// <summary>
        /// インポート表示
        /// </summary>
        /// <returns></returns>
        public ActionResult Import()
        {
            Trace.WriteLine("GET /DailyParameter/Import");

            return View(new List<T_DailyClasses>());
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
        public ActionResult Import(string cmd, HttpPostedFileBase postedFile, [Bind(Include = "")] List<T_DailyClasses> dailyClassesList)
        {
            Trace.WriteLine("POST /DailyParameter/Import");

            // 読込ボタンが押下された場合
            if (AppConstant.CMD_READ.Equals(cmd))
            {
                // ステータスをクリア
                ModelState.Clear();
                // 日別条件を初期化
                dailyClassesList = new List<T_DailyClasses>();

                if (postedFile != null)
                {
                    // 拡張子チェック
                    string extension = Path.GetExtension(postedFile.FileName);
                    if (!".csv".Equals(extension) && !".CSV".Equals(extension))
                    {
                        // エラーメッセージ
                        ViewBag.ErrorMessage = "ファイルはcsv形式を指定してください。";
                        return View(dailyClassesList);
                    }

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

                    // 項目数
                    int itemCnt = 0;
                    // テキストを全行読み込み
                    using (StreamReader sr = new StreamReader(filepath, Encoding.GetEncoding("shift_jis")))
                    {
                        int row = 0;
                        while (!sr.EndOfStream)
                        {
                            row++;
                            // CSVファイルの一行を読み込む
                            string line = sr.ReadLine();
                            // 読み込んだ一行をカンマ毎に分けて配列に格納
                            string[] values = line.Split(',');

                            // ヘッダ行
                            if (row == 1)
                            {
                                // 項目数を取得
                                itemCnt = values.Count();
                                // スキップ
                                continue;
                            }

                            // 空行チェック（全ての項目が空）
                            if (values.Where(x => string.IsNullOrEmpty(x)).Count() == values.Count())
                            {
                                break;
                            }
                            // CSV項目数チェック
                            if (values.Count() != itemCnt)
                            {
                                ViewBag.ErrorMessage = "csvの項目数に誤りがあるため、読み込みを途中で終了しました。 " + row + "行目";
                                break;
                            }

                            T_DailyClasses dailyClasses = new T_DailyClasses();
                            // 日付項目
                            DateTime dateItem;
                            // 数値項目
                            double doubleItem;

                            // ----- 対象日 -----
                            if (!this.ItemCheck(values[0], out dateItem, "対象日", row)) break;
                            dailyClasses.Date = dateItem;

                            // ----- 合宿比率[%] -----
                            if (!this.ItemCheck(values[1], out doubleItem, "合宿比率[%]", row)) break;
                            dailyClasses.LodgingRatio = doubleItem;

                            // ----- 通学比率[%] -----
                            if (!this.ItemCheck(values[2], out doubleItem, "通学比率[%]", row)) break;
                            dailyClasses.CommutingRatio = doubleItem;

                            // ----- 【合宿】MT一段階比率[%] -----
                            if (!this.ItemCheck(values[3], out doubleItem, "【合宿】MT一段階比率[%]", row)) break;
                            dailyClasses.LdgMtFstRatio = doubleItem;

                            // ----- 【合宿】MT二段階比率[%] -----
                            if (!this.ItemCheck(values[4], out doubleItem, "【合宿】MT二段階比率[%]", row)) break;
                            dailyClasses.LdgMtSndRatio = doubleItem;

                            // ----- 【合宿】AT一段階比率[%] -----
                            if (!this.ItemCheck(values[5], out doubleItem, "【合宿】AT一段階比率[%] ", row)) break;
                            dailyClasses.LdgAtFstRatio = doubleItem;

                            // ----- 【合宿】AT二段階比率[%] -----
                            if (!this.ItemCheck(values[6], out doubleItem, "【合宿】AT二段階比率[%]", row)) break;
                            dailyClasses.LdgAtSndRatio = doubleItem;

                            // ----- 【合宿】MT一段階コマ数 -----
                            if (!this.ItemCheck(values[7], out doubleItem, "【合宿】MT一段階コマ数", row)) break;
                            dailyClasses.LdgMtFstClass = doubleItem;

                            // ----- 【合宿】MT二段階コマ数 -----
                            if (!this.ItemCheck(values[8], out doubleItem, "【合宿】MT二段階コマ数", row)) break;
                            dailyClasses.LdgMtSndClass = doubleItem;

                            // ----- 【合宿】AT一段階コマ数 -----
                            if (!this.ItemCheck(values[9], out doubleItem, "【合宿】AT一段階コマ数", row)) break;
                            dailyClasses.LdgAtFstClass = doubleItem;

                            // ----- 【合宿】AT二段階コマ数 -----
                            if (!this.ItemCheck(values[10], out doubleItem, "【合宿】AT二段階コマ数", row)) break;
                            dailyClasses.LdgAtSndClass = doubleItem;

                            // ----- 【合宿】MT一段階コマ数/日 -----
                            if (!this.ItemCheck(values[11], out doubleItem, "【合宿】MT一段階コマ数/日", row)) break;
                            dailyClasses.LdgMtFstClassDay = doubleItem;

                            // ----- 【合宿】MT二段階コマ数/日 -----
                            if (!this.ItemCheck(values[12], out doubleItem, "【合宿】MT二段階コマ数/日", row)) break;
                            dailyClasses.LdgMtSndClassDay = doubleItem;

                            // ----- 【合宿】AT一段階コマ数/日 -----
                            if (!this.ItemCheck(values[13], out doubleItem, "【合宿】AT一段階コマ数/日", row)) break;
                            dailyClasses.LdgAtFstClassDay = doubleItem;

                            // ----- 【合宿】AT二段階コマ数/日 -----
                            if (!this.ItemCheck(values[14], out doubleItem, "【合宿】AT二段階コマ数/日", row)) break;
                            dailyClasses.LdgAtSndClassDay = doubleItem;

                            // ----- 【通学】MT一段階比率[%] -----
                            if (!this.ItemCheck(values[15], out doubleItem, "【通学】MT一段階比率[%]", row)) break;
                            dailyClasses.CmtMtFstRatio = doubleItem;

                            // ----- 【通学】MT二段階比率[%] -----
                            if (!this.ItemCheck(values[16], out doubleItem, "【通学】MT二段階比率[%]", row)) break;
                            dailyClasses.CmtMtSndRatio = doubleItem;

                            // ----- 【通学】AT一段階比率[%] -----
                            if (!this.ItemCheck(values[17], out doubleItem, "【通学】AT一段階比率[%]", row)) break;
                            dailyClasses.CmtAtFstRatio = doubleItem;

                            // ----- 【通学】AT二段階比率[%] -----
                            if (!this.ItemCheck(values[18], out doubleItem, "【通学】AT二段階比率[%]", row)) break;
                            dailyClasses.CmtAtSndRatio = doubleItem;

                            // ----- 【通学】MT一段階コマ数 -----
                            if (!this.ItemCheck(values[19], out doubleItem, "【通学】MT一段階コマ数", row)) break;
                            dailyClasses.CmtMtFstClass = doubleItem;

                            // ----- 【通学】MT二段階コマ数 -----
                            if (!this.ItemCheck(values[20], out doubleItem, "【通学】MT二段階コマ数", row)) break;
                            dailyClasses.CmtMtSndClass = doubleItem;

                            // ----- 【通学】AT一段階コマ数 -----
                            if (!this.ItemCheck(values[21], out doubleItem, "【通学】AT一段階コマ数", row)) break;
                            dailyClasses.CmtAtFstClass = doubleItem;

                            // ----- 【通学】AT二段階コマ数 -----
                            if (!this.ItemCheck(values[22], out doubleItem, "【通学】AT二段階コマ数", row)) break;
                            dailyClasses.CmtAtSndClass = doubleItem;

                            // ----- 【通学】MT一段階コマ数/日 -----
                            if (!this.ItemCheck(values[23], out doubleItem, "【通学】MT一段階コマ数/日", row)) break;
                            dailyClasses.CmtMtFstClassDay = doubleItem;

                            // ----- 【通学】MT二段階コマ数/日 -----
                            if (!this.ItemCheck(values[24], out doubleItem, "【通学】MT二段階コマ数/日", row)) break;
                            dailyClasses.CmtMtSndClassDay = doubleItem;

                            // ----- 【通学】AT一段階コマ数/日 -----
                            if (!this.ItemCheck(values[25], out doubleItem, "【通学】AT一段階コマ数/日", row)) break;
                            dailyClasses.CmtAtFstClassDay = doubleItem;

                            // ----- 【通学】AT二段階コマ数/日 -----
                            if (!this.ItemCheck(values[26], out doubleItem, "【通学】AT二段階コマ数/日", row)) break;
                            dailyClasses.CmtAtSndClassDay = doubleItem;

                            dailyClassesList.Add(dailyClasses);
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
                // 入力チェック
                bool validation = true;
                if (ModelState.IsValid)
                {
                    // 重複チェック
                    int repeatedNum = dailyClassesList.GroupBy(x => x.Date)
                        .Select(x => new { Count = x.Count() }).Where(x => x.Count != 1).Count();
                    if (repeatedNum > 0)
                    {
                        ViewBag.ErrorMessage = "対象日の重複データがあります。（同じ日のデータを複数登録することはできません。）";
                        validation = false;
                    }
                    // 各行の不整合チェック
                    if (validation == true)
                    {
                        foreach (T_DailyClasses dailyClasses in dailyClassesList)
                        {
                            // 合宿比率と通学比率のチェック
                            if (dailyClasses.LodgingRatio + dailyClasses.CommutingRatio != 100)
                            {
                                ViewBag.ErrorMessage = "合宿比率[%]と通学比率[%]は合わせて100になるように設定してください。";
                                validation = false;
                                break;
                            }
                            // 合宿の在籍比率[%]（AT一段階/二段階、MT一段階/二段階）のチェック
                            if (dailyClasses.LdgAtFstRatio + dailyClasses.LdgAtSndRatio + dailyClasses.LdgMtFstRatio + dailyClasses.LdgMtSndRatio != 100)
                            {
                                ViewBag.ErrorMessage = "合宿の在籍比率[%]（AT一段階/二段階、MT一段階/二段階）は合わせて100になるように設定してください。";
                                validation = false;
                                break;
                            }
                            // 通学の在籍比率[%]（AT一段階/二段階、MT一段階/二段階）のチェック
                            if (dailyClasses.CmtAtFstRatio + dailyClasses.CmtAtSndRatio + dailyClasses.CmtMtFstRatio + dailyClasses.CmtMtSndRatio != 100)
                            {
                                ViewBag.ErrorMessage = "通学の在籍比率[%]（AT一段階/二段階、MT一段階/二段階）は合わせて100になるように設定してください。";
                                validation = false;
                                break;
                            }
                        }
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
                    // データの登録/更新
                    foreach (T_DailyClasses dailyClasses in dailyClassesList)
                    {
                        // 存在チェック
                        if (db.DailyClasses.Where(x => ((DateTime)x.Date).Equals((DateTime)dailyClasses.Date)).Count() == 0)
                        {
                            // 登録処理
                            db.DailyClasses.Add(dailyClasses);
                        }
                        else
                        {
                            // 更新処理
                            db.Entry(dailyClasses).State = EntityState.Modified;
                        }
                    }
                    db.SaveChanges();
                    // 完了メッセージ
                    ViewBag.CompMessage = "インポートが完了しました。";
                    // 表示データを初期化
                    dailyClassesList = new List<T_DailyClasses>();
                }
            }

            // その他
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            return View(dailyClassesList);
        }

        /// <summary>
        /// DateTime型項目チェック
        /// </summary>
        /// <param name="value">評価対象</param>
        /// <param name="item">DateTime変換値</param>
        /// <param name="itemName">項目名</param>
        /// <param name="row">行数</param>
        /// <returns>評価結果(true/false)</returns>
        private bool ItemCheck(string value, out DateTime item, string itemName, int row)
        {
            item = new DateTime();
            // 必須チェック
            if (string.IsNullOrEmpty(value))
            {
                ViewBag.ErrorMessage = itemName + "が未設定のため、読み込みを途中で終了しました。 " + row + "行目";
                return false;
            }
            // 型整合性チェック
            if (!DateTime.TryParse(value, out item))
            {
                ViewBag.ErrorMessage = itemName + "の設定が不正のため、読み込みを途中で終了しました。 " + row + "行目";
                return false;
            }
            return true;
        }

        /// <summary>
        /// double型項目チェック
        /// </summary>
        /// <param name="value">評価対象</param>
        /// <param name="item">double変換値</param>
        /// <param name="itemName">項目名</param>
        /// <param name="row">行数</param>
        /// <returns>評価結果(true/false)</returns>
        private bool ItemCheck(string value, out double item, string itemName, int row)
        {
            item = 0;
            // 必須チェック
            if (string.IsNullOrEmpty(value))
            {
                ViewBag.ErrorMessage = itemName + "が未設定のため、読み込みを途中で終了しました。 " + row + "行目";
                return false;
            }
            // 型整合性チェック
            if (!double.TryParse(value, out item))
            {
                ViewBag.ErrorMessage = itemName + "の設定が不正のため、読み込みを途中で終了しました。 " + row + "行目";
                return false;
            }
            return true;
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
