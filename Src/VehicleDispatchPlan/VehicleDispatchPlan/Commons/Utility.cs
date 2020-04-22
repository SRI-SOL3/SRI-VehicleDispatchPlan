using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.DataVisualization.Charting;
using VehicleDispatchPlan.Constants;
using VehicleDispatchPlan.Models;

/**
 * 共通処理
 *
 * @author t-murayama
 * @version 1.0
 * ----------------------------------
 * 2020/03/01 t-murayama 新規作成
 *
 */
namespace VehicleDispatchPlan.Commons
{
    /// <summary>
    /// 共通処理クラス
    /// </summary>
    public class Utility
    {
        /// <summary>
        /// グラフデータ取得
        /// </summary>
        /// <param name="db">DBコンテキスト</param>
        /// <param name="dateFrom">日付From</param>
        /// <param name="dateTo">日付To</param>
        /// <param name="targetTraineeLodging">合宿教習生情報(登録/更新時)</param>
        /// <param name="targetTraineeCommuting">通学教習生情報(登録/更新時)</param>
        /// <returns></returns>
        public List<V_ChartData> getChartData(MyDatabaseContext db, DateTime dateFrom, DateTime dateTo
            , List<T_TraineeLodging> targetTraineeLodging, List<T_TraineeCommuting> targetTraineeCommuting)
        {
            List<V_ChartData> chartData = new List<V_ChartData>();

            // 対象期間の日別コマ数データを取得
            List<T_DailyClasses> dailyClassesList = db.DailyClasses.Where(x => dateFrom <= x.Date && x.Date <= dateTo).ToList();
            // 対象期間の指導員データを取得
            List<T_DailyClassesByTrainer> trainerList = db.DailyClassesByTrainer.Where(x => x.Date >= dateFrom && x.Date <= dateTo).ToList();

            // 合宿教習生の取得
            List<T_TraineeLodging> traineeLodging;
            // 引数の教習生がnullの場合（受入予測管理）
            if (targetTraineeLodging == null)
            {
                // 対象期間の教習生データを全て取得
                traineeLodging = db.TraineeLodging.Where(
                    x => dateFrom <= x.EntrancePlanDate && x.EntrancePlanDate <= dateTo
                    || dateFrom <= x.GraduatePlanDate && x.GraduatePlanDate <= dateTo
                    || x.EntrancePlanDate < dateFrom && dateTo < x.GraduatePlanDate).ToList();
            }
            // 引数の教習生がnullでない場合（教習生管理(登録/更新)）
            else
            {
                // 0以外の教習生IDを取得
                List<int> traineeIdList = targetTraineeLodging.Where(x => !x.TraineeId.Equals(0)).Select(x => x.TraineeId).ToList();
                // 対象教習生ID以外を取得
                traineeLodging = db.TraineeLodging.Where(
                    x => !traineeIdList.Contains(x.TraineeId)
                    && (dateFrom <= x.EntrancePlanDate && x.EntrancePlanDate <= dateTo
                    || dateFrom <= x.GraduatePlanDate && x.GraduatePlanDate <= dateTo
                    || x.EntrancePlanDate < dateFrom && dateTo < x.GraduatePlanDate)).ToList();
            }

            // 通学教習生の取得
            List<T_TraineeCommuting> traineeCommuting;
            // 引数の教習生がnullの場合（受入予測管理）
            if (targetTraineeCommuting == null)
            {
                // 対象期間の教習生データを全て取得
                traineeCommuting = db.TraineeCommuting.Where(
                    x => dateFrom <= x.EntrancePlanDate && x.EntrancePlanDate <= dateTo
                    || dateFrom <= x.GraduatePlanDate && x.GraduatePlanDate <= dateTo
                    || x.EntrancePlanDate < dateFrom && dateTo < x.GraduatePlanDate).ToList();
            }
            // 引数の教習生がnullでない場合（教習生管理(登録/更新)）
            else
            {
                // 0以外の教習生IDを取得
                List<int> traineeIdList = targetTraineeCommuting.Where(x => !x.TraineeId.Equals(0)).Select(x => x.TraineeId).ToList();
                // 対象教習生ID以外を取得
                traineeCommuting = db.TraineeCommuting.Where(
                    x => !traineeIdList.Contains(x.TraineeId)
                    && (dateFrom <= x.EntrancePlanDate && x.EntrancePlanDate <= dateTo
                    || dateFrom <= x.GraduatePlanDate && x.GraduatePlanDate <= dateTo
                    || x.EntrancePlanDate < dateFrom && dateTo < x.GraduatePlanDate)).ToList();
            }

            // 受入累積数
            int acceptLodgingSumAmt = 0;
            int acceptCommutingSumAmt = 0;
            // 受入可能人数/期間
            double acceptLodgingMaxAmt = 0;
            double acceptCommutingMaxAmt = 0;

            for (DateTime day = dateFrom; day.CompareTo(dateTo) <= 0; day = day.AddDays(1))
            {
                // 対象の日でインスタンスを生成
                V_ChartData data = new V_ChartData { Date = day };

                // 日別予測条件
                T_DailyClasses dailyClasses = dailyClassesList.Where(x => ((DateTime)x.Date).Equals(day)).FirstOrDefault();
                if (dailyClasses == null)
                {
                    dailyClasses = new T_DailyClasses() { Date = day };
                }
                // 教習総コマ数/日
                double dailySumClasses = trainerList.Where(x => x.Date.Equals(day)).Select(x => x.Classes).Sum();

                // --------------------
                // １．受入可能数/日
                // --------------------
                // ①-1.【合宿】教習生一人が卒業までに必要なコマ数
                //     ＝ (合宿生のAT一段階コマ数 ＋ 合宿生のAT二段階コマ数) × (合宿生のAT一段階比率[%] ＋ 合宿生のAT二段階比率[%]) ÷ 100
                //       ＋ (合宿生のMT一段階コマ数 ＋ 合宿生のMT二段階コマ数) × (合宿生のMT一段階比率[%] ＋ 合宿生のMT二段階比率[%]) ÷ 100
                double ldgTraineeReqClasses = (dailyClasses.LdgAtFstClass + dailyClasses.LdgAtSndClass) * (dailyClasses.LdgAtFstRatio + dailyClasses.LdgAtSndRatio) / 100
                    + (dailyClasses.LdgMtFstClass + dailyClasses.LdgMtSndClass) * (dailyClasses.LdgMtFstRatio + dailyClasses.LdgMtSndRatio) / 100;
                // ①-2.【通学】教習生一人が卒業までに必要なコマ数
                //     ＝ (通学生のAT一段階コマ数 ＋ 通学生のAT二段階コマ数) × (通学生のAT一段階比率[%] ＋ 通学生のAT二段階比率[%]) ÷ 100
                //       ＋ (通学生のMT一段階コマ数 ＋ 通学生のMT二段階コマ数) × (通学生のMT一段階比率[%] ＋ 通学生のMT二段階比率[%]) ÷ 100
                double cmtTraineeReqClasses = (dailyClasses.CmtAtFstClass + dailyClasses.CmtAtSndClass) * (dailyClasses.CmtAtFstRatio + dailyClasses.CmtAtSndRatio) / 100
                    + (dailyClasses.CmtMtFstClass + dailyClasses.CmtMtSndClass) * (dailyClasses.CmtMtFstRatio + dailyClasses.CmtMtSndRatio) / 100;

                // ②-1.【合宿】受入可能人数/日を加算  ※①-1が0の場合は加算しない
                if (ldgTraineeReqClasses != 0)
                {
                    acceptLodgingMaxAmt = acceptLodgingMaxAmt + ((dailySumClasses / ldgTraineeReqClasses) * (dailyClasses.LodgingRatio / 100));
                }
                // ②-2.【通学】受入可能人数/日を加算  ※①-2が0の場合は加算しない
                if (cmtTraineeReqClasses != 0)
                {
                    acceptCommutingMaxAmt = acceptCommutingMaxAmt + ((dailySumClasses / cmtTraineeReqClasses) * (dailyClasses.CommutingRatio / 100));
                }

                // --------------------
                // ２．在籍可能数/日
                // --------------------
                // ①-1.【合宿】教習生一人が実車教習に必要なコマ数/日
                //     ＝ 合宿生のAT一段階コマ数/日 × 合宿生のAT一段階比率[%] ÷ 100 ＋ 合宿生のAT二段階コマ数/日 × 合宿生のAT二段階比率[%] ÷ 100
                //       ＋ 合宿生のMT一段階コマ数/日 × 合宿生のMT一段階比率[%] ÷ 100 ＋ 合宿生のMT二段階コマ数/日 × 合宿生のMT二段階比率[%] ÷ 100
                double ldgDailyReqClasses = dailyClasses.LdgAtFstClassDay * dailyClasses.LdgAtFstRatio / 100 + dailyClasses.LdgAtSndClassDay * dailyClasses.LdgAtSndRatio / 100
                    + dailyClasses.LdgMtFstClassDay * dailyClasses.LdgMtFstRatio / 100 + dailyClasses.LdgMtSndClassDay * dailyClasses.LdgMtSndRatio / 100;
                // ①-2.【通学】教習生一人が実車教習に必要なコマ数/日
                //     ＝ 通学生のAT一段階コマ数/日 × 通学生のAT一段階比率[%] ÷ 100 ＋ 通学生のAT二段階コマ数/日 × 通学生のAT二段階比率[%] ÷ 100
                //       ＋ 通学生のMT一段階コマ数/日 × 通学生のMT一段階比率[%] ÷ 100 ＋ 通学生のMT二段階コマ数/日 × 通学生のMT二段階比率[%] ÷ 100
                double cmtDailyReqClasses = dailyClasses.CmtAtFstClassDay * dailyClasses.CmtAtFstRatio / 100 + dailyClasses.CmtAtSndClassDay * dailyClasses.CmtAtSndRatio / 100
                    + dailyClasses.CmtMtFstClassDay * dailyClasses.CmtMtFstRatio / 100 + dailyClasses.CmtMtSndClassDay * dailyClasses.CmtMtSndRatio / 100;

                // ②-1.【合宿】在籍可能人数/日  ※①-1が0の場合は算出しない
                if (ldgDailyReqClasses != 0)
                {
                    data.DailyLodgingMaxAmt = Math.Round((dailySumClasses / ldgDailyReqClasses) * (dailyClasses.LodgingRatio / 100), 1);
                }
                // ②-2.【通学】在籍可能人数/日  ※①-2が0の場合は算出しない
                if (cmtDailyReqClasses != 0)
                {
                    data.DailyCommutingMaxAmt = Math.Round((dailySumClasses / cmtDailyReqClasses) * (dailyClasses.CommutingRatio / 100), 1);
                }

                // --------------------
                // ３．受入累積数
                // --------------------
                // 受入累積数を加算
                acceptLodgingSumAmt = acceptLodgingSumAmt + traineeLodging.Where(x => x.EntrancePlanDate.Equals(day)).Count();
                acceptCommutingSumAmt = acceptCommutingSumAmt + traineeCommuting.Where(x => x.EntrancePlanDate.Equals(day)).Count();
                // グラフデータに設定
                data.AcceptLodgingSumAmt = acceptLodgingSumAmt;
                data.AcceptCommutingSumAmt = acceptCommutingSumAmt;

                // --------------------
                // 在籍見込数
                // --------------------
                // 合宿在籍見込数(MT-一段階)（教習がMTかつ、入校予定日が対象日以上かつ、仮免予定日が対象日未満）
                data.LodgingMtFstRegAmt = traineeLodging.Where(
                    x => x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_MT)
                    && x.EntrancePlanDate <= day && day < x.TmpLicencePlanDate).Count();
                // 合宿在籍見込数(MT-二段階)（教習がMTかつ、仮免予定日が対象日以上かつ、卒業予定日が対象日以下）
                data.LodgingMtSndRegAmt = traineeLodging.Where(
                    x => x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_MT)
                    && x.TmpLicencePlanDate <= day && day <= x.GraduatePlanDate).Count();
                // 合宿在籍見込数(AT-一段階)（教習がATかつ、入校予定日が対象日以上かつ、仮免予定日が対象日未満）
                data.LodgingAtFstRegAmt = traineeLodging.Where(
                    x => x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_AT)
                    && x.EntrancePlanDate <= day && day < x.TmpLicencePlanDate).Count();
                // 合宿在籍見込数(AT-二段階)（教習がATかつ、仮免予定日が対象日以上かつ、卒業予定日が対象日以下）
                data.LodgingAtSndRegAmt = traineeLodging.Where(
                    x => x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_AT)
                    && x.TmpLicencePlanDate <= day && day <= x.GraduatePlanDate).Count();

                // 通学在籍見込数(MT-一段階)（教習がMTかつ、入校予定日が対象日以上かつ、仮免予定日が対象日未満）
                data.CommutingMtFstRegAmt = traineeCommuting.Where(
                    x => x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_MT)
                    && x.EntrancePlanDate <= day && day < x.TmpLicencePlanDate).Count();
                // 通学在籍見込数(MT-二段階)（教習がMTかつ、仮免予定日が対象日以上かつ、卒業予定日が対象日以下）
                data.CommutingMtFstRegAmt = traineeCommuting.Where(
                    x => x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_MT)
                    && x.TmpLicencePlanDate <= day && day <= x.GraduatePlanDate).Count();
                // 通学在籍見込数(AT-一段階)（教習がATかつ、入校予定日が対象日以上かつ、仮免予定日が対象日未満）
                data.CommutingAtFstRegAmt = traineeCommuting.Where(
                    x => x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_AT)
                    && x.EntrancePlanDate <= day && day < x.TmpLicencePlanDate).Count();
                // 通学在籍見込数(AT-二段階)（教習がATかつ、仮免予定日が対象日以上かつ、卒業予定日が対象日以下）
                data.CommutingAtSndRegAmt = traineeCommuting.Where(
                    x => x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_AT)
                    && x.TmpLicencePlanDate <= day && day <= x.GraduatePlanDate).Count();

                // 引数の合宿教習生情報が設定されている場合はデータに追加
                if (targetTraineeLodging != null)
                {
                    // 合宿在籍数(MT-一段階)
                    data.LodgingMtFstRegAmt = data.LodgingMtFstRegAmt
                        + targetTraineeLodging.Where(
                            x => x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_MT)
                            && x.EntrancePlanDate <= day && day < x.TmpLicencePlanDate).Count();
                    // 合宿在籍数(MT-二段階)
                    data.LodgingMtSndRegAmt = data.LodgingMtSndRegAmt
                        + targetTraineeLodging.Where(
                            x => x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_MT)
                            && x.TmpLicencePlanDate <= day && day <= x.GraduatePlanDate).Count();
                    // 合宿在籍数(AT-一段階)
                    data.LodgingAtFstRegAmt = data.LodgingAtFstRegAmt
                        + targetTraineeLodging.Where(
                            x => x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_AT)
                            && x.EntrancePlanDate <= day && day < x.TmpLicencePlanDate).Count();
                    // 合宿在籍数(AT-二段階)
                    data.LodgingAtSndRegAmt = data.LodgingAtSndRegAmt
                        + targetTraineeLodging.Where(
                            x => x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_AT)
                            && x.TmpLicencePlanDate <= day && day <= x.GraduatePlanDate).Count();
                }

                // 引数の通学教習生情報が設定されている場合はデータに追加
                if (targetTraineeCommuting != null)
                {
                    // 通学在籍数(MT-一段階)
                    data.CommutingMtFstRegAmt = data.CommutingMtFstRegAmt
                        + targetTraineeCommuting.Where(
                            x => x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_MT)
                            && x.EntrancePlanDate <= day && day < x.TmpLicencePlanDate).Count();
                    // 通学在籍数(MT-二段階)
                    data.CommutingMtSndRegAmt = data.CommutingMtSndRegAmt
                        + targetTraineeCommuting.Where(
                            x => x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_MT)
                            && x.TmpLicencePlanDate <= day && day <= x.GraduatePlanDate).Count();
                    // 通学在籍数(AT-一段階)
                    data.CommutingAtFstRegAmt = data.CommutingAtFstRegAmt
                        + targetTraineeCommuting.Where(
                            x => x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_AT)
                            && x.EntrancePlanDate <= day && day < x.TmpLicencePlanDate).Count();
                    // 通学在籍数(AT-二段階)
                    data.CommutingAtSndRegAmt = data.CommutingAtSndRegAmt
                        + targetTraineeCommuting.Where(
                            x => x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_AT)
                            && x.TmpLicencePlanDate <= day && day <= x.GraduatePlanDate).Count();
                }

                chartData.Add(data);
            }

            // 受入可能人数/期間を全てのデータに設定
            chartData.ForEach(x => {
                x.AcceptLodgingMaxAmt = acceptLodgingMaxAmt;
                x.AcceptCommutingMaxAmt = acceptCommutingMaxAmt;
            });

            return chartData;
        }

        /// <summary>
        /// グラフ画像パス取得
        /// </summary>
        /// <param name="year">年</param>
        /// <param name="month">月</param>
        /// <param name="chartData">グラフデータ</param>
        /// <param name="totalRemAmt">総受入残数</param>
        /// <param name="lodgingRemAmt">合宿受入残数</param>
        /// <param name="commutingRemAmt">通学受入残数</param>
        /// <param name="totalMaxAmt">総在籍可能数</param>
        /// <param name="lodgingMaxAmt">合宿在籍可能数</param>
        /// <param name="commutingMaxAmt">通学在籍可能数</param>
        /// <param name="totalRegAmt">総在籍見込数</param>
        /// <param name="lodgingRegAmt">合宿在籍見込数</param>
        /// <param name="commutingRegAmt">通学在籍見込数</param>
        /// <returns>画像パス</returns>
        public string getChartPath(string year, string month, List<V_ChartData> chartData, 
            bool totalRemAmt, bool lodgingRemAmt, bool commutingRemAmt, bool totalMaxAmt, bool lodgingMaxAmt, bool commutingMaxAmt, bool totalRegAmt, bool lodgingRegAmt, bool commutingRegAmt)
        {
            // グラフを作成
            Chart chart = new Chart()
            {
                Height = 500,
                Width = 2000,
                ImageType = ChartImageType.Png,
                // ＴＯＤＯ：ユーザーＩＤとかにする？
                ImageLocation = @"ChartImages\" + year + "-" + month + ".png",
                ChartAreas =
                {
                    new ChartArea
                    {
                        Name = "Default",
                        AxisY = new Axis
                        {
                            IsStartedFromZero = true,
                            Interval = 10
                        },
                        AxisX = new Axis
                        {
                            Interval = 5
                        }
                    }
                }
            };

            chart.Series.Clear();

            // 総受入残数
            if (totalRemAmt == true)
            {
                chart.Series.Add(AppConstant.SERIES_TOTAL_REM_AMT);
                chart.Series[AppConstant.SERIES_TOTAL_REM_AMT].ChartType = SeriesChartType.Line;
                chart.Series[AppConstant.SERIES_TOTAL_REM_AMT].Color = Color.FromArgb(255, 0, 0);
                chart.Series[AppConstant.SERIES_TOTAL_REM_AMT].MarkerStyle = MarkerStyle.None;
                chart.Series[AppConstant.SERIES_TOTAL_REM_AMT].MarkerColor = Color.FromArgb(255, 0, 0);
                chart.Series[AppConstant.SERIES_TOTAL_REM_AMT].BorderWidth = 3;
                chart.Series[AppConstant.SERIES_TOTAL_REM_AMT].BorderColor = Color.FromArgb(255, 0, 0);
            }

            // 合宿受入残数
            if (lodgingRemAmt == true)
            {
                chart.Series.Add(AppConstant.SERIES_LODGING_REM_AMT);
                chart.Series[AppConstant.SERIES_LODGING_REM_AMT].ChartType = SeriesChartType.Line;
                chart.Series[AppConstant.SERIES_LODGING_REM_AMT].Color = Color.FromArgb(0, 0, 255);
                chart.Series[AppConstant.SERIES_LODGING_REM_AMT].MarkerStyle = MarkerStyle.None;
                chart.Series[AppConstant.SERIES_LODGING_REM_AMT].MarkerColor = Color.FromArgb(0, 0, 255);
                chart.Series[AppConstant.SERIES_LODGING_REM_AMT].BorderWidth = 3;
                chart.Series[AppConstant.SERIES_LODGING_REM_AMT].BorderColor = Color.FromArgb(0, 0, 255);
            }

            // 通学受入残数
            if (commutingRemAmt == true)
            {
                chart.Series.Add(AppConstant.SERIES_COMMUTING_REM_AMT);
                chart.Series[AppConstant.SERIES_COMMUTING_REM_AMT].ChartType = SeriesChartType.Line;
                chart.Series[AppConstant.SERIES_COMMUTING_REM_AMT].Color = Color.FromArgb(0, 200, 0);
                chart.Series[AppConstant.SERIES_COMMUTING_REM_AMT].MarkerStyle = MarkerStyle.None;
                chart.Series[AppConstant.SERIES_COMMUTING_REM_AMT].MarkerColor = Color.FromArgb(0, 200, 0);
                chart.Series[AppConstant.SERIES_COMMUTING_REM_AMT].BorderWidth = 3;
                chart.Series[AppConstant.SERIES_COMMUTING_REM_AMT].BorderColor = Color.FromArgb(0, 200, 0);
            }

            // 総在籍可能数
            if (totalMaxAmt == true)
            {
                chart.Series.Add(AppConstant.SERIES_TOTAL_MAX_AMT);
                chart.Series[AppConstant.SERIES_TOTAL_MAX_AMT].ChartType = SeriesChartType.Line;
                chart.Series[AppConstant.SERIES_TOTAL_MAX_AMT].Color = Color.FromArgb(200, 60, 60);
                chart.Series[AppConstant.SERIES_TOTAL_MAX_AMT].MarkerStyle = MarkerStyle.None;
                chart.Series[AppConstant.SERIES_TOTAL_MAX_AMT].MarkerColor = Color.FromArgb(200, 60, 60);
                chart.Series[AppConstant.SERIES_TOTAL_MAX_AMT].BorderWidth = 2;
                chart.Series[AppConstant.SERIES_TOTAL_MAX_AMT].BorderDashStyle = ChartDashStyle.Dash;
                chart.Series[AppConstant.SERIES_TOTAL_MAX_AMT].BorderColor = Color.FromArgb(200, 60, 60);
            }

            // 合宿在籍可能数
            if (lodgingMaxAmt == true)
            {
                chart.Series.Add(AppConstant.SERIES_LODGING_MAX_AMT);
                chart.Series[AppConstant.SERIES_LODGING_MAX_AMT].ChartType = SeriesChartType.Line;
                chart.Series[AppConstant.SERIES_LODGING_MAX_AMT].Color = Color.FromArgb(30, 100, 160);
                chart.Series[AppConstant.SERIES_LODGING_MAX_AMT].MarkerStyle = MarkerStyle.None;
                chart.Series[AppConstant.SERIES_LODGING_MAX_AMT].MarkerColor = Color.FromArgb(30, 100, 160);
                chart.Series[AppConstant.SERIES_LODGING_MAX_AMT].BorderWidth = 2;
                chart.Series[AppConstant.SERIES_LODGING_MAX_AMT].BorderDashStyle = ChartDashStyle.Dash;
                chart.Series[AppConstant.SERIES_LODGING_MAX_AMT].BorderColor = Color.FromArgb(30, 100, 160);
            }

            // 通学在籍可能数
            if (commutingMaxAmt == true)
            {
                chart.Series.Add(AppConstant.SERIES_COMMUTING_MAX_AMT);
                chart.Series[AppConstant.SERIES_COMMUTING_MAX_AMT].ChartType = SeriesChartType.Line;
                chart.Series[AppConstant.SERIES_COMMUTING_MAX_AMT].Color = Color.FromArgb(30, 160, 60);
                chart.Series[AppConstant.SERIES_COMMUTING_MAX_AMT].MarkerStyle = MarkerStyle.None;
                chart.Series[AppConstant.SERIES_COMMUTING_MAX_AMT].MarkerColor = Color.FromArgb(30, 160, 60);
                chart.Series[AppConstant.SERIES_COMMUTING_MAX_AMT].BorderWidth = 2;
                chart.Series[AppConstant.SERIES_COMMUTING_MAX_AMT].BorderDashStyle = ChartDashStyle.Dash;
                chart.Series[AppConstant.SERIES_COMMUTING_MAX_AMT].BorderColor = Color.FromArgb(30, 160, 60);
            }

            // 総在籍見込数
            if (totalRegAmt == true)
            {
                chart.Series.Add(AppConstant.SERIES_TOTAL_REG_AMT);
                chart.Series[AppConstant.SERIES_TOTAL_REG_AMT].ChartType = SeriesChartType.Area;
                chart.Series[AppConstant.SERIES_TOTAL_REG_AMT].Color = Color.FromArgb(50, 200, 60, 60);
                chart.Series[AppConstant.SERIES_TOTAL_REG_AMT].MarkerStyle = MarkerStyle.Circle;
                chart.Series[AppConstant.SERIES_TOTAL_REG_AMT].MarkerColor = Color.FromArgb(200, 60, 60);
                chart.Series[AppConstant.SERIES_TOTAL_REG_AMT].BorderWidth = 2;
                chart.Series[AppConstant.SERIES_TOTAL_REG_AMT].BorderColor = Color.FromArgb(200, 60, 60);
            }

            // 合宿在籍見込数
            if (lodgingRegAmt == true)
            {
                chart.Series.Add(AppConstant.SERIES_LODGING_REG_AMT);
                chart.Series[AppConstant.SERIES_LODGING_REG_AMT].ChartType = SeriesChartType.Area;
                chart.Series[AppConstant.SERIES_LODGING_REG_AMT].Color = Color.FromArgb(50, 30, 100, 160);
                chart.Series[AppConstant.SERIES_LODGING_REG_AMT].MarkerStyle = MarkerStyle.Circle;
                chart.Series[AppConstant.SERIES_LODGING_REG_AMT].MarkerColor = Color.FromArgb(30, 100, 160);
                chart.Series[AppConstant.SERIES_LODGING_REG_AMT].BorderWidth = 2;
                chart.Series[AppConstant.SERIES_LODGING_REG_AMT].BorderColor = Color.FromArgb(30, 100, 160);
            }

            // 通学在籍見込数
            if (commutingRegAmt == true)
            {
                chart.Series.Add(AppConstant.SERIES_COMMUTING_REG_AMT);
                chart.Series[AppConstant.SERIES_COMMUTING_REG_AMT].ChartType = SeriesChartType.Area;
                chart.Series[AppConstant.SERIES_COMMUTING_REG_AMT].Color = Color.FromArgb(50, 30, 160, 60);
                chart.Series[AppConstant.SERIES_COMMUTING_REG_AMT].MarkerStyle = MarkerStyle.Circle;
                chart.Series[AppConstant.SERIES_COMMUTING_REG_AMT].MarkerColor = Color.FromArgb(30, 160, 60);
                chart.Series[AppConstant.SERIES_COMMUTING_REG_AMT].BorderWidth = 2;
                chart.Series[AppConstant.SERIES_COMMUTING_REG_AMT].BorderColor = Color.FromArgb(30, 160, 60);
            }

            // 日単位でプロット
            foreach (V_ChartData data in chartData)
            {
                // 総受入残数
                if (totalRemAmt == true)
                {
                    chart.Series[AppConstant.SERIES_TOTAL_REM_AMT].Points.AddXY(data.Date.ToString("M/d"), data.AcceptTotalRemAmt);
                }

                // 合宿受入残数
                if (lodgingRemAmt == true)
                {
                    chart.Series[AppConstant.SERIES_LODGING_REM_AMT].Points.AddXY(data.Date.ToString("M/d"), data.AcceptLodgingRemAmt);
                }

                // 通学受入残数
                if (commutingRemAmt == true)
                {
                    chart.Series[AppConstant.SERIES_COMMUTING_REM_AMT].Points.AddXY(data.Date.ToString("M/d"), data.AcceptCommutingRemAmt);
                }

                // 総在籍最大数
                if (totalMaxAmt == true)
                {
                    chart.Series[AppConstant.SERIES_TOTAL_MAX_AMT].Points.AddXY(data.Date.ToString("M/d"), data.DailyTotalMaxAmt);
                }

                // 合宿在籍最大数
                if (lodgingMaxAmt == true)
                {
                    chart.Series[AppConstant.SERIES_LODGING_MAX_AMT].Points.AddXY(data.Date.ToString("M/d"), data.DailyLodgingMaxAmt);
                }

                // 通学在籍最大数
                if (commutingMaxAmt == true)
                {
                    chart.Series[AppConstant.SERIES_COMMUTING_MAX_AMT].Points.AddXY(data.Date.ToString("M/d"), data.DailyCommutingMaxAmt);
                }

                // 総在籍数
                if (totalRegAmt == true)
                {
                    chart.Series[AppConstant.SERIES_TOTAL_REG_AMT].Points.AddXY(data.Date.ToString("M/d")
                        , data.LodgingMtFstRegAmt + data.LodgingMtSndRegAmt + data.LodgingAtFstRegAmt + data.LodgingMtSndRegAmt 
                        + data.CommutingMtFstRegAmt + data.CommutingMtSndRegAmt + data.CommutingAtFstRegAmt + data.CommutingAtSndRegAmt);
                }
                
                // 合宿受入可能残数
                if (lodgingRegAmt == true)
                {
                    chart.Series[AppConstant.SERIES_LODGING_REG_AMT].Points.AddXY(data.Date.ToString("M/d")
                        , data.LodgingMtFstRegAmt + data.LodgingMtSndRegAmt + data.LodgingAtFstRegAmt + data.LodgingMtSndRegAmt);
                }

                // 通学受入可能残数
                if (commutingRegAmt == true)
                {
                    chart.Series[AppConstant.SERIES_COMMUTING_REG_AMT].Points.AddXY(data.Date.ToString("M/d")
                        , data.CommutingMtFstRegAmt + data.CommutingMtSndRegAmt + data.CommutingAtFstRegAmt + data.CommutingAtSndRegAmt);
                }
            }

            // グラフを画像で出力
            string savePath = AppDomain.CurrentDomain.BaseDirectory + chart.ImageLocation;
            string saveDir = Path.GetDirectoryName(savePath);
            if (!Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
            }
            chart.SaveImage(savePath);

            return "/" + chart.ImageLocation.Replace(@"\", "/");
        }

        /// <summary>
        /// エラーメッセージ生成
        /// </summary>
        /// <param name="modelState">ステータス</param>
        /// <returns>エラーメッセージ</returns>
        public string getErrorMessage(ModelStateDictionary modelState)
        {
            // 改行タグ
            const string TAG_BR = "<br>";

            // エラーメッセージを連結
            string errorMessage = "";
            foreach (ModelState state in modelState.Values)
            {
                foreach (ModelError error in state.Errors)
                {
                    errorMessage = errorMessage + error.ErrorMessage + TAG_BR;
                }
            }

            // 末尾の改行タグを削除
            if (errorMessage.Length > TAG_BR.Length)
            {
                errorMessage = errorMessage.Substring(0, errorMessage.Length - TAG_BR.Length);
            }

            return errorMessage;
        }
    }
}