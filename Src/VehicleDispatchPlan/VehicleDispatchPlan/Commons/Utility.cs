using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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
        /// <param name="targetTrainee">教習生情報(登録/更新時)</param>
        /// <returns></returns>
        public List<V_ChartData> getChartData(MyDatabaseContext db, DateTime dateFrom, DateTime dateTo, List<T_Trainee> targetTrainee)
        {
            List<V_ChartData> chartData = new List<V_ChartData>();

            // 対象期間の指導員データを取得
            List<T_DailyClassesByTrainer> trainerList = db.DailyClassesByTrainer.Where(x => x.Date >= dateFrom && x.Date <= dateTo).ToList();

            List<T_Trainee> traineeList;
            // 引数の教習生がnullの場合（受入予測管理）
            if (targetTrainee == null)
            {
                // 対象期間の教習生データを全て取得
                traineeList = db.Trainee.Where(
                    x => x.EntrancePlanDate >= dateFrom && x.EntrancePlanDate <= dateTo
                    || x.GraduatePlanDate >= dateFrom && x.GraduatePlanDate <= dateTo
                    || x.EntrancePlanDate < dateFrom && dateTo < x.GraduatePlanDate).ToList();
            }
            // 引数の教習生がnullでない場合（教習生管理(登録/更新)）
            else
            {
                // 0以外の教習生IDを取得
                List<int> traineeIdList = targetTrainee.Where(x => !x.TraineeId.Equals(0)).Select(x => x.TraineeId).ToList();
                // 対象教習生ID以外を取得
                traineeList = db.Trainee.Where(
                    x => !traineeIdList.Contains(x.TraineeId)
                    && (x.EntrancePlanDate >= dateFrom && x.EntrancePlanDate <= dateTo
                    || x.GraduatePlanDate >= dateFrom && x.GraduatePlanDate <= dateTo
                    || x.EntrancePlanDate < dateFrom && dateTo < x.GraduatePlanDate)).ToList();
            }

            // 教習コースマスタから、MT/ATの実車コマ数を取得
            List<M_TrainingCourse> trainingCourseList = db.TrainingCourse.ToList();
            int mtClassQty = trainingCourseList.Where(x => x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_MT)).Select(x => x.PracticeClassQty).FirstOrDefault();
            int atClassQty = trainingCourseList.Where(x => x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_AT)).Select(x => x.PracticeClassQty).FirstOrDefault();

            // 対象期間の日別コマ数データを取得
            List<T_DailyClasses> dailyClassesList = db.DailyClasses.Where(x => dateFrom <= x.Date && x.Date <= dateTo).ToList();

            // 受入累積数
            int acceptTotalSumAmt = 0;
            int acceptLodgingSumAmt = 0;
            int acceptCommutingSumAmt = 0;

            double acceptLodgingMaxAmt = 0;
            double acceptCommutingMaxAmt = 0;

            for (DateTime day = dateFrom; day.CompareTo(dateTo) <= 0; day = day.AddDays(1))
            {
                // 対象の日でインスタンスを生成
                V_ChartData data = new V_ChartData { Date = day };

                // 教習総コマ数/日
                double dailySumClasses = trainerList.Where(x => x.Date.Equals(day)).Select(x => x.Classes).Sum();

                // --------------------
                // 受入可能数/日
                // --------------------
                // 教習生一人が卒業までに必要なコマ数（ATの実車教習コマ数×ATの比率[%]÷100 ＋ MTの実車教習コマ数×MTの比率[%]÷100）
                // ＴＯＤＯ：ATの比率とMTの比率は日次で管理？
                double atRatio = dailyClassesList.Where(x => ((DateTime)x.Date).Equals(day)).Select(x => x.AtRatio).FirstOrDefault();
                double traineeReqClasses = (atClassQty * atRatio / 100) + (mtClassQty * (100 - atRatio) / 100);
                // 受入可能人数/日を加算
                // ＴＯＤＯ：合宿比率、通学比率は日次で管理？
                acceptLodgingMaxAmt = acceptLodgingMaxAmt + ((dailySumClasses / traineeReqClasses) * (90.0 / 100));
                acceptCommutingMaxAmt = acceptCommutingMaxAmt + ((dailySumClasses / traineeReqClasses) * (10.0 / 100));

                // --------------------
                // 在籍可能数/日
                // --------------------
                // 教習生一人が実車教習に必要なコマ数/日（2×一段階の比率[%]÷100 ＋ 3×二段階の比率[%]÷100）
                // ＴＯＤＯ：一段階の比率、二段階の比率は日次で管理？
                double firstRatio = dailyClassesList.Where(x => ((DateTime)x.Date).Equals(day)).Select(x => x.FirstRatio).FirstOrDefault();
                double dailyReqClasses = 2 * firstRatio / 100 + 3 * (100 - firstRatio) / 100;
                // 在籍可能人数
                // ＴＯＤＯ：合宿比率、通学比率は日次で管理？
                double lodgingRatio = dailyClassesList.Where(x => ((DateTime)x.Date).Equals(day)).Select(x => x.LodgingRatio).FirstOrDefault();
                data.DailyLodgingMaxAmt = Math.Round((dailySumClasses / dailyReqClasses) * (lodgingRatio / 100), 1);
                data.DailyCommutingMaxAmt = Math.Round((dailySumClasses / dailyReqClasses) * ((100 - lodgingRatio) / 100), 1);
                data.DailyTotalMaxAmt = Math.Round(data.DailyLodgingMaxAmt + data.DailyCommutingMaxAmt, 1);

                // --------------------
                // 受入累積数
                // --------------------
                // 受入累積数を加算
                acceptTotalSumAmt = acceptTotalSumAmt + traineeList.Where(x => x.EntrancePlanDate.Equals(day)).Count();
                acceptLodgingSumAmt = acceptLodgingSumAmt + traineeList.Where(x => x.EntrancePlanDate.Equals(day)
                    && x.AttendTypeCd.Equals(AppConstant.ATTEND_TYPE_CD_LODGING)).Count();
                acceptCommutingSumAmt = acceptCommutingSumAmt + traineeList.Where(x => x.EntrancePlanDate.Equals(day)
                    && x.AttendTypeCd.Equals(AppConstant.ATTEND_TYPE_CD_COMMUTING)).Count();
                // グラフデータに設定
                data.AcceptTotalSumAmt = acceptTotalSumAmt;
                data.AcceptLodgingSumAmt = acceptLodgingSumAmt;
                data.AcceptCommutingSumAmt = acceptCommutingSumAmt;

                // --------------------
                // 在籍見込数
                // --------------------
                // 合宿在籍見込数(MT-一段階)（合宿かつ、MTかつ、入校予定日が対象日以上かつ、仮免予定日が対象日未満）
                data.LodgingMtFstRegAmt = traineeList.Where(x => x.AttendTypeCd.Equals(AppConstant.ATTEND_TYPE_CD_LODGING)
                    && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_MT)
                    && x.EntrancePlanDate <= day && x.TmpLicencePlanDate > day).Count();

                // 合宿在籍見込数(MT-二段階)（合宿かつ、MTかつ、仮免予定日が対象日以上かつ、卒業予定日予定日が対象日以下）
                data.LodgingMtSndRegAmt = traineeList.Where(x => x.AttendTypeCd.Equals(AppConstant.ATTEND_TYPE_CD_LODGING)
                    && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_MT)
                    && x.TmpLicencePlanDate <= day && x.GraduatePlanDate >= day).Count();

                // 合宿在籍見込数(AT-一段階)（合宿かつ、ATかつ、入校予定日が対象日以上かつ、仮免予定日が対象日未満）
                data.LodgingAtFstRegAmt = traineeList.Where(x => x.AttendTypeCd.Equals(AppConstant.ATTEND_TYPE_CD_LODGING)
                    && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_AT)
                    && x.EntrancePlanDate <= day && x.TmpLicencePlanDate > day).Count();

                // 合宿在籍見込数(AT-二段階)（合宿かつ、ATかつ、仮免予定日が対象日以上かつ、卒業予定日予定日が対象日以下）
                data.LodgingAtSndRegAmt = traineeList.Where(x => x.AttendTypeCd.Equals(AppConstant.ATTEND_TYPE_CD_LODGING)
                    && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_AT)
                    && x.TmpLicencePlanDate <= day && x.GraduatePlanDate >= day).Count();

                // 通学在籍見込数(MT)（通学かつ、MTかつ、入校予定日が対象日以上かつ、卒業予定日予定日が対象日以下）
                data.CommutingMtRegAmt = traineeList.Where(x => x.AttendTypeCd.Equals(AppConstant.ATTEND_TYPE_CD_COMMUTING)
                    && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_MT)
                    && x.EntrancePlanDate <= day && x.GraduatePlanDate >= day).Count();

                // 通学在籍見込数(AT)（通学かつ、ATかつ、入校予定日が対象日以上かつ、卒業予定日予定日が対象日以下）
                data.CommutingAtRegAmt = traineeList.Where(x => x.AttendTypeCd.Equals(AppConstant.ATTEND_TYPE_CD_COMMUTING)
                    && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_AT)
                    && x.EntrancePlanDate <= day && x.GraduatePlanDate >= day).Count();

                // 引数の教習生情報が設定されている場合はデータに追加
                if (targetTrainee != null)
                {
                    // 合宿在籍数(MT-一段階)
                    data.LodgingMtFstRegAmt = data.LodgingMtFstRegAmt
                        + targetTrainee.Where(x => x.AttendTypeCd.Equals(AppConstant.ATTEND_TYPE_CD_LODGING)
                            && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_MT)
                            && x.EntrancePlanDate <= day && x.TmpLicencePlanDate > day).Count();
                    // 合宿在籍数(MT-二段階)
                    data.LodgingMtSndRegAmt = data.LodgingMtSndRegAmt
                        + targetTrainee.Where(x => x.AttendTypeCd.Equals(AppConstant.ATTEND_TYPE_CD_LODGING)
                            && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_MT)
                            && x.TmpLicencePlanDate <= day && x.GraduatePlanDate >= day).Count();
                    // 合宿在籍数(AT-一段階)
                    data.LodgingAtFstRegAmt = data.LodgingAtFstRegAmt
                        + targetTrainee.Where(x => x.AttendTypeCd.Equals(AppConstant.ATTEND_TYPE_CD_LODGING)
                            && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_AT)
                            && x.EntrancePlanDate <= day && x.TmpLicencePlanDate > day).Count();
                    // 合宿在籍数(AT-二段階)
                    data.LodgingAtSndRegAmt = data.LodgingAtSndRegAmt
                        + targetTrainee.Where(x => x.AttendTypeCd.Equals(AppConstant.ATTEND_TYPE_CD_LODGING)
                            && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_AT)
                            && x.TmpLicencePlanDate <= day && x.GraduatePlanDate >= day).Count();
                    // 通学在籍数(MT)
                    data.CommutingMtRegAmt = data.CommutingMtRegAmt
                        + targetTrainee.Where(x => x.AttendTypeCd.Equals(AppConstant.ATTEND_TYPE_CD_COMMUTING)
                            && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_MT)
                            && x.EntrancePlanDate <= day && x.GraduatePlanDate >= day).Count();
                    // 通学在籍数(AT)
                    data.CommutingAtRegAmt = data.CommutingAtRegAmt
                        + targetTrainee.Where(x => x.AttendTypeCd.Equals(AppConstant.ATTEND_TYPE_CD_COMMUTING)
                            && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_AT)
                            && x.EntrancePlanDate <= day && x.GraduatePlanDate >= day).Count();
                }

                chartData.Add(data);
            }

            // 受入可能人数/期間を全てのデータに設定
            for (int i = 0; i < chartData.Count(); i++)
            {
                chartData[i].AcceptLodgingMaxAmt = acceptLodgingMaxAmt;
                chartData[i].AcceptCommutingMaxAmt = acceptCommutingMaxAmt;
                chartData[i].AcceptTotalMaxAmt = acceptLodgingMaxAmt + acceptCommutingMaxAmt;
            }

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
                        , data.LodgingMtFstRegAmt + data.LodgingMtSndRegAmt + data.LodgingAtFstRegAmt + data.LodgingMtSndRegAmt + data.CommutingMtRegAmt + data.CommutingAtRegAmt);
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
                        , data.CommutingMtRegAmt + data.CommutingAtRegAmt);
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
    }
}