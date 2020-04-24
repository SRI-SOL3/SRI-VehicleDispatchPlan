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


        /// <summary>
        /// インポート表示
        /// </summary>
        /// <returns></returns>
        public ActionResult Import([Bind(Include = "Date,No,TrainerName,Classes")] List<T_DailyClassesByTrainer> importList)
        {
            if (importList != null)
            {
                return View(importList);
            }

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

                // 新規採番用変数
                var numbered = new List<int>();

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
                            int? dailyClassesNo;
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
                            //null許容
                            dailyClassesNo = int.TryParse(values[1], out var i) ? (int?)i : null;




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

                // 重複チェック
                int repeatedNum = importList.GroupBy(x => new { x.Date, x.No })
                    .Select(x => new { Count = x.Count() }).Where(x => x.Count != 1).Count();
                if (repeatedNum > 0)
                {
                    ViewBag.ErrorMessage = "日付、No.の重複データがあります。（同じ日に同じNoのデータを複数登録することはできません。）ファイルを修正して再度読み込みを行ってください。";
                    return View(new List<T_DailyClassesByTrainer>());
                }



                ////ソート用リスト
                List<T_DailyClassesByTrainer> sorted = new List<T_DailyClassesByTrainer>();

                ////親テーブルに日付データがない場合は親データに挿入
                //foreach (T_DailyClassesByTrainer dailyClassesByTrainer in importList)
                //{ 
                //    if ((int)db.DailyClasses.Where(item => ((DateTime)item.Date).Equals((DateTime)dailyClassesByTrainer.Date)).Count() == 0)
                //    {
                //        //親テーブル
                //        T_DailyClasses t_DailyClasses = new T_DailyClasses();
                //        t_DailyClasses.Date = dailyClassesByTrainer.Date;
                //        db.DailyClasses.Add(t_DailyClasses);

                //        db.SaveChanges();

                //    }
                //}

                //新規採番(Noに空文字が存在することを想定)
                if (!ModelState.IsValid)
                {

                    //----変数-----

                    //Noに空文字が存在するか
                    bool ExistNullFlg = false;

                    // データの登録/更新
                    //新規採番No（DB内にNoが一つ以上存在した場合）
                    int nextNum = 1;

                    //新規採番No（DB内にNoが存在しなかった場合）
                    int MaxIntOfTheDay = 1;
                    
                    //日付格納用
                    DateTime? aDay = new DateTime();



                


                    //ソート（日付、No）
                    sorted = importList.OrderBy(x => x.Date).ThenBy(x => x.No).ToList();

                    //ループ
                    foreach (T_DailyClassesByTrainer dailyClassesByTrainer in importList)
                    {

                        //Noが存在するか
                        if(db.DailyClassesByTrainer.Where(item => ((DateTime)item.Date).Equals((DateTime)dailyClassesByTrainer.Date)).Select(s => s.No).Count() > 0)
                        {
                            //新規採番
                            nextNum = (int)db.DailyClassesByTrainer.Where(item => ((DateTime)item.Date).Equals((DateTime)dailyClassesByTrainer.Date)).Select(s => s.No).Max() + 1;
                        }

                        //日付が異なる                        
                        if (aDay != dailyClassesByTrainer.Date)
                        {
                            //初期値に戻す
                            MaxIntOfTheDay = 1;
                        }

                        //Nullが存在するか
                        if (dailyClassesByTrainer.No == null)
                        {
                            
                            if (nextNum > MaxIntOfTheDay)
                            {
                                //採番済のデータが存在すれば、次の番号から割り当てる
                                dailyClassesByTrainer.No = nextNum;
                                MaxIntOfTheDay = nextNum;
                                MaxIntOfTheDay++;
                                ExistNullFlg = true;
                                //クリア
                                ModelState.Clear();

                            }
                            else
                            {
                                //採番済の番号がなければ、新規に採番を行う。
                                dailyClassesByTrainer.No = MaxIntOfTheDay;
                                MaxIntOfTheDay++;
                            }

                        }

                        aDay = dailyClassesByTrainer.Date;
                    }

                    

                    if (ExistNullFlg)
                    {
                        ViewBag.CompMessage = "Noの新規採番が完了しました。再度登録ボタンを押してください。";
                        return View(sorted);
                    }

                    
                }



                if (ModelState.IsValid)
                {


                    //親テーブルに日付データがない場合は親データに挿入
                    foreach (T_DailyClassesByTrainer dailyClassesByTrainer in importList)
                    {
                        if ((int)db.DailyClasses.Where(item => ((DateTime)item.Date).Equals((DateTime)dailyClassesByTrainer.Date)).Count() == 0)
                        {
                            //親テーブル
                            T_DailyClasses t_DailyClasses = new T_DailyClasses();
                            t_DailyClasses.Date = dailyClassesByTrainer.Date;
                            db.DailyClasses.Add(t_DailyClasses);

                            db.SaveChanges();

                        }
                    }

                    // データの登録/更新
                    foreach (T_DailyClassesByTrainer dailyClassesByTrainer in importList)
                        {

                            if (dailyClassesByTrainer.No == null)
                            {
                                // 登録処理(Noがnull)
                                db.DailyClassesByTrainer.Add(dailyClassesByTrainer);

                            }
                            else if ((db.DailyClassesByTrainer.Where(x => ((DateTime)x.Date).Equals((DateTime)dailyClassesByTrainer.Date)
                                        && ((int)x.No).Equals((int)dailyClassesByTrainer.No)).Count() == 0))
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
                else
                {
                    //var errormsgs = ModelState.SelectMany(x => x.Value.Errors.Select(z => z.ErrorMessage));
                    //ViewBag.ErrorMessage = errormsgs.ToString();
                    ViewBag.ErrorMessage = "必須項目(指導員、コマ数)を設定してください。";
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
