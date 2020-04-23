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
 * 指導員日次情報取得コントローラ
 *
 * @author 土井勇紀
 * @version 1.0
 * ----------------------------------
 * 2020/04/21 土井勇紀　複製
 *
 */
namespace VehicleDispatchPlan_Dev.Controllers
{
    public class InstractorImportController : Controller
    {
        // データベースコンテキスト
        private MyDatabaseContext db = new MyDatabaseContext();


        public ActionResult List()
        {
            var list = new List<T_DailyClassesByTrainer>();
            list = db.DailyClassesByTrainer.ToList();
            return View(list);
        }


        /// <summary>
        /// インポート表示
        /// </summary>
        /// <returns></returns>
        public ActionResult Import()
        {
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
            Trace.WriteLine("POST /Calendar/Import");

            // 読込ボタンが押下された場合
            if (AppConstant.CMD_READ.Equals(cmd))
            {
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

                    // 日別予測条件クラスデータ取得
                    List<T_DailyClasses> dailyClasses = db.DailyClasses.ToList();

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


                            // 対象日
                            DateTime dailyClassesDate;
                            // No
                            int dailyClassesNo;
                            // 指導員名
                            String dailyClassesTrainerName;
                            // コマ数
                            double dailyClassesNum;

                            // CSV項目数チェック
                            if (values.Count() != 4)
                            {
                                ViewBag.ErrorMessage = "csvの項目数に誤りがあるため、読み込みを途中で終了しました。 " + row.ToString() + "行目";
                                break;
                            }

                            // ----- 対象日 -----
                            // 必須チェック
                            if (string.IsNullOrEmpty(values[0]))
                            {
                                ViewBag.ErrorMessage = "対象日が未設定のため、読み込みを途中で終了しました。 " + row.ToString() + "行目";
                                break;
                            }
                            // 日付整合性チェック
                            if (!DateTime.TryParse(values[0], out dailyClassesDate))
                            {
                                ViewBag.ErrorMessage = "対象日の設定が不正のため、読み込みを途中で終了しました。 " + row.ToString() + "行目";
                                break;
                            }
                            // 対象日存在チェック
                            dailyClassesDate = dailyClasses
                                .Where(x => ((DateTime)x.Date).Equals(DateTime.Parse(values[0])))
                                .Select(x => (DateTime)x.Date).FirstOrDefault();

                            if (string.IsNullOrEmpty(dailyClassesDate.ToString()))
                            {
                                ViewBag.ErrorMessage = "対象日の設定が不正のため、読み込みを途中で終了しました。 " + row.ToString() + "行目";
                                break;
                            }

                            // ----- No -----
                            // 必須チェック
                            if (string.IsNullOrEmpty(values[1]))
                            {
                                ViewBag.ErrorMessage = "Noが未設定のため、読み込みを途中で終了しました。 " + row.ToString() + "行目";
                                break;
                            }

                            //数値整合性チェック
                            if (!int.TryParse(values[1], out dailyClassesNo))
                            {
                                ViewBag.ErrorMessage = "Noの設定が不正のため、読み込みを途中で終了しました。 " + row.ToString() + "行目";
                                break;
                            }


                            // ----- 指導員名 -----
                            // 必須チェック
                            if (string.IsNullOrEmpty(values[2]))
                            {
                                ViewBag.ErrorMessage = "指導員名が未設定のため、読み込みを途中で終了しました。 " + row.ToString() + "行目";
                                break;
                            }
                            dailyClassesTrainerName = values[2].ToString();


                            // ----- コマ数 -----
                            // 必須チェック
                            if (string.IsNullOrEmpty(values[3]))
                            {
                                ViewBag.ErrorMessage = "コマ数が未設定のため、読み込みを途中で終了しました。 " + row.ToString() + "行目";
                                break;
                            }
                            //数値整合性チェック
                            if (!double.TryParse(values[3], out dailyClassesNum))
                            {
                                ViewBag.ErrorMessage = "コマ数の整合性が不正のため、読み込みを途中で終了しました。 " + row.ToString() + "行目";
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
                    int repeatedNum = importList.GroupBy(x => new { x.Date, x.No })
                        .Select(x => new { Count = x.Count() }).Where(x => x.Count != 1).Count();
                    if (repeatedNum > 0)
                    {
                        ViewBag.ErrorMessage = "日付、No.の重複データがあります。（同じ日に同じNoのデータを登録することはできません。）";
                    }
                    else
                    {
                        // データの登録/更新
                        foreach (T_DailyClassesByTrainer dailyClassesByTrainer in importList)
                        {

                            db.DailyClassesByTrainer.Add(dailyClassesByTrainer);

                            // 存在チェック
                            //int ch = db.DailyClassesByTrainer
                            //   .Where(x => ((DateTime)x.Date).Equals((DateTime)dailyClassesByTrainer.Date)).Count();

                            //int nm = db.DailyClassesByTrainer.Where((x.No).Equals(dailyClassesByTrainer.No));
                            if (db.DailyClassesByTrainer.Where(x => ((DateTime)x.Date).Equals((DateTime)dailyClassesByTrainer.Date)
                                    && (x.No).Equals(dailyClassesByTrainer.No)).Count() == 0)
                            {
                                // 登録処理
                                db.DailyClassesByTrainer.Add(dailyClassesByTrainer);
                            }
                            else
                            {
                                // 更新処理
                                db.Entry(dailyClassesByTrainer).State = EntityState.Modified;
                            }
                        }
                        db.SaveChanges();
                        // 完了メッセージ
                        ViewBag.CompMessage = "インポートが完了しました。";
                        // 表示データを初期化
                        importList = new List<T_DailyClassesByTrainer>();
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "必須項目を設定してください。";
                }
            }


            ///その他
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
