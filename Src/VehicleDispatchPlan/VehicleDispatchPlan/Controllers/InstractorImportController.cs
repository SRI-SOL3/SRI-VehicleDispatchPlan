using System;
using System.Collections.Generic;
using System.Data;
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
 * 指導員日次情報取込コントローラ
 *
 * @author 土井勇紀
 * @version 1.0
 * ----------------------------------
 * 2020/04/21 土井勇紀　複製
 * 2021/02/05 t-murayama 20210205リリース対応(ver.1.1)
 *
 */
namespace VehicleDispatchPlan_Dev.Controllers
{
    public class InstractorImportController : Controller
    {
        // データベースコンテキスト
        private MyDatabaseContext db = new MyDatabaseContext();

        /// <summary>
        /// インポート表示
        /// </summary>
        /// <returns></returns>
        public ActionResult Import()
        {
            Trace.WriteLine("GET /InstractorImport/Import");

            return View(new List<T_DailyClassesByTrainer>());
        }

        /// <summary>
        /// インポート実行
        /// </summary>
        /// <param name="cmd">コマンド</param>
        /// <param name="postedFile">CSVファイル</param>
        /// <param name="importList">指導員日次情報</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Import(string cmd, HttpPostedFileBase postedFile, [Bind(Include = "Date,No,TrainerName,Classes")] List<T_DailyClassesByTrainer> importList)
        {
            Trace.WriteLine("POST /InstractorImport/Import");

            // 読込ボタンが押下された場合
            if (AppConstant.CMD_READ.Equals(cmd))
            {
                // ステータスをクリア
                ModelState.Clear();
                // 指導員コマ数クラスを初期化
                importList = new List<T_DailyClassesByTrainer>();

                if (postedFile != null)
                {
                    // 拡張子チェック
                    string extension = Path.GetExtension(postedFile.FileName);
                    if (!".csv".Equals(extension) && !".CSV".Equals(extension))
                    {
                        // エラーメッセージ
                        ViewBag.ErrorMessage = "ファイルはcsv形式を指定してください。";
                        return View(importList);
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

                            // 対象日
                            DateTime dailyClassesDate;
                            // No
                            // [20210205リリース対応] Mod Start int?→intに変更
                            //int? dailyClassesNo;
                            //int dailyClassesNoInt;
                            int dailyClassesNo;
                            // [20210205リリース対応] Mod End
                            // 指導員名
                            string dailyClassesTrainerName;
                            // コマ数
                            double dailyClassesNum;

                            // ----- 対象日 -----
                            // 必須チェック
                            if (string.IsNullOrEmpty(values[0]))
                            {
                                ViewBag.ErrorMessage = "対象日が未設定のため、読み込みを途中で終了しました。 " + row + "行目";
                                break;
                            }
                            // 日付整合性チェック
                            if (!DateTime.TryParse(values[0], out dailyClassesDate))
                            {
                                ViewBag.ErrorMessage = "対象日の設定が不正のため、読み込みを途中で終了しました。 " + row + "行目";
                                break;
                            }

                            // ----- No -----
                            // null許容
                            if (string.IsNullOrEmpty(values[1]))
                            {
                                // [20210205リリース対応] Mod Start nullを許容しない
                                //dailyClassesNo = null;
                                ViewBag.ErrorMessage = "Noが未設定のため、読み込みを途中で終了しました。 " + row + "行目";
                                break;
                                // [20210205リリース対応] Mod End
                            }
                            else
                            {
                                // 数値整合性チェック
                                // [20210205リリース対応] Mod Start 変数の変更
                                //if (!int.TryParse(values[1], out dailyClassesNoInt))
                                //{
                                //    ViewBag.ErrorMessage = "Noの設定が不正のため、読み込みを途中で終了しました。 " + row + "行目";
                                //    break;
                                //}
                                //if (dailyClassesNoInt <= 0)
                                //{
                                //    ViewBag.ErrorMessage = "Noの設定が不正のため、読み込みを途中で終了しました。 " + row + "行目";
                                //    break;
                                //}
                                if (!int.TryParse(values[1], out dailyClassesNo))
                                {
                                    ViewBag.ErrorMessage = "Noの設定が不正のため、読み込みを途中で終了しました。 " + row + "行目";
                                    break;
                                }
                                if (dailyClassesNo <= 0)
                                {
                                    ViewBag.ErrorMessage = "Noの設定が不正のため、読み込みを途中で終了しました。 " + row + "行目";
                                    break;
                                }
                                // [20210205リリース対応] Mod End
                                // [20210205リリース対応] Del Start
                                //if (db.DailyClassesByTrainer.Where(x => ((DateTime)x.Date).Equals(dailyClassesDate) && ((int)x.No).Equals(dailyClassesNoInt)).Count() == 0)
                                //{
                                //    ViewBag.ErrorMessage = "Noの設定が不正のため、読み込みを途中で終了しました。Noはすでに登録済の値のみ指定できます。 " + row + "行目";
                                //    break;
                                //}
                                //dailyClassesNo = dailyClassesNoInt;
                                // [20210205リリース対応] Del End
                            }

                            // ----- 指導員名 -----
                            // 必須チェック
                            if (string.IsNullOrEmpty(values[2]))
                            {
                                ViewBag.ErrorMessage = "指導員名が未設定のため、読み込みを途中で終了しました。 " + row + "行目";
                                break;
                            }
                            dailyClassesTrainerName = values[2];

                            // ----- コマ数 -----
                            // 必須チェック
                            if (string.IsNullOrEmpty(values[3]))
                            {
                                ViewBag.ErrorMessage = "コマ数が未設定のため、読み込みを途中で終了しました。 " + row + "行目";
                                break;
                            }
                            // 数値整合性チェック
                            if (!double.TryParse(values[3], out dailyClassesNum))
                            {
                                ViewBag.ErrorMessage = "コマ数の設定が不正のため、読み込みを途中で終了しました。 " + row + "行目";
                                break;
                            }

                            T_DailyClassesByTrainer t_DailyClassesByTrainer = new T_DailyClassesByTrainer()
                            {
                                Date = dailyClassesDate,
                                No = dailyClassesNo,
                                TrainerName = dailyClassesTrainerName,
                                Classes = dailyClassesNum
                            };
                            // リストに追加
                            importList.Add(t_DailyClassesByTrainer);
                        }
                    }

                    // 重複チェック
                    // [20210205リリース対応] Mod Start Noをint?型からint型に変更したため、Nullは存在しない
                    //int repeatedNum = importList.Where(x => x.No != null).GroupBy(x => new { x.Date, x.No })
                    //    .Select(x => new { Count = x.Count() }).Where(x => x.Count != 1).Count();
                    int repeatedNum = importList.GroupBy(x => new { x.Date, x.No })
                        .Select(x => new { Count = x.Count() }).Where(x => x.Count != 1).Count();
                    // [20210205リリース対応] Mod End
                    if (repeatedNum > 0)
                    {
                        ViewBag.ErrorMessage = "日付、No.の重複データがあります。（同じ日に同じNoのデータを複数登録することはできません。）ファイルを修正して再度読み込みを行ってください。";
                        return View(new List<T_DailyClassesByTrainer>());
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
                // [20210205リリース対応] Del Start 採番処理の削除
                //// 未採番のデータがある場合（採番処理）
                //if (importList.Where(x => x.No == null).Count() > 0)
                //{
                //// ステータスをクリア
                //ModelState.Clear();
                //// ソート（日付、No(設定済)、No(未設定)）
                //importList = importList.OrderBy(x => x.Date).ThenBy(x => x.No == null ? 1 : 0).ThenBy(x => x.No).ToList();
                //// 前レコードの日付
                //DateTime? beforeDate = null;
                //// 新規採番No
                //int nextNum = 0;
                //// 未設定のNoを採番
                //foreach (T_DailyClassesByTrainer dailyClassesByTrainer in importList.Where(x => x.No == null))
                //{
                //    // 前レコードの日付がnullもしくは対象レコードと異なる場合
                //    if (beforeDate == null || beforeDate != dailyClassesByTrainer.Date)
                //    {
                //        // 最大値を取得
                //        int? maxNum = db.DailyClassesByTrainer.Where(x => ((DateTime)x.Date).Equals((DateTime)dailyClassesByTrainer.Date)).Select(x => x.No).Max();
                //        // 最大値を加算（最大値がnullの場合は1）
                //        nextNum = maxNum == null ? 1 : (int)maxNum + 1;
                //    }
                //    else
                //    {
                //        // Noを加算
                //        nextNum++;
                //    }

                //    // 採番された番号を設定
                //    dailyClassesByTrainer.No = nextNum;
                //    // 日付を保持
                //    beforeDate = dailyClassesByTrainer.Date;
                //}

                //ViewBag.CompMessage = "Noの新規採番が完了しました。再度登録ボタンを押してください。";
                //}

                //// 未採番のデータがない場合（登録・更新処理）
                //else
                //{
                // [20210205リリース対応] Del End
                // 入力チェック
                bool validation = true;
                if (ModelState.IsValid)
                {
                    // コマ数チェック
                    // [20210205リリース対応] Mod Start コマ数の0許容
                    //if (importList.Where(x => x.Classes <= 0).Count() > 0)
                    //{
                    //    ViewBag.ErrorMessage = "コマ数に0以下は設定できません。";
                    //    validation = false;
                    //}
                    if (importList.Where(x => x.Classes < 0).Count() > 0)
                    {
                        ViewBag.ErrorMessage = "コマ数に0未満は設定できません。";
                        validation = false;
                    }
                    // [20210205リリース対応] Mod End
                }
                else
                {
                    // エラーメッセージ生成
                    ViewBag.ErrorMessage = new Utility().GetErrorMessage(ModelState);
                    validation = false;
                }

                if (validation == true)
                {
                    // [20210205リリース対応] Add Start 既存データ削除
                    // 削除対象の日付を取得
                    List<DateTime?> dateList = importList.Select(x => x.Date).Distinct().ToList();
                    // 対象データを全て削除
                    db.DailyClassesByTrainer.RemoveRange(
                        db.DailyClassesByTrainer.Where(x => dateList.Contains(x.Date)));
                    // [20210205リリース対応] Add End

                    // 登録済み日別予測条件（親データ）を取得
                    List<T_DailyClasses> addedDailyClasses = db.DailyClasses.Where(x => dateList.Contains(x.Date)).ToList();

                    foreach (T_DailyClassesByTrainer dailyClassesByTrainer in importList)
                    {
                        // 日別予測条件（親データ）の存在チェック
                        if (addedDailyClasses.Where(x => ((DateTime)x.Date).Equals((DateTime)dailyClassesByTrainer.Date)).Count() == 0)
                        {
                            // 日別予測条件を生成
                            T_DailyClasses addDailyClasses = new T_DailyClasses() { Date = dailyClassesByTrainer.Date };
                            // 日付を指定してデータを登録
                            db.DailyClasses.Add(addDailyClasses);
                            // 追加済みとする
                            addedDailyClasses.Add(addDailyClasses);
                        }

                        // [20210205リリース対応] Mod Start 存在チェックを削除
                        //// 存在チェック
                        //if (db.DailyClassesByTrainer.Where(x => ((DateTime)x.Date).Equals((DateTime)dailyClassesByTrainer.Date)
                        //    && ((int)x.No).Equals((int)dailyClassesByTrainer.No)).Count() == 0)
                        //{
                        //    // 登録処理
                        //    db.DailyClassesByTrainer.Add(dailyClassesByTrainer);
                        //}
                        //else
                        //{
                        //    // 更新処理
                        //    db.Entry(dailyClassesByTrainer).State = EntityState.Modified;
                        //}
                        // 登録処理
                        db.DailyClassesByTrainer.Add(dailyClassesByTrainer);
                        // [20210205リリース対応] Mod End
                    }
                    db.SaveChanges();
                    // 完了メッセージ
                    ViewBag.CompMessage = "インポートが完了しました。";
                    // 表示データを初期化
                    importList = new List<T_DailyClassesByTrainer>();
                }
                // [20210205リリース対応] Del Start
                //}
                // [20210205リリース対応] Del End
            }

            // その他
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            return View(importList);
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
