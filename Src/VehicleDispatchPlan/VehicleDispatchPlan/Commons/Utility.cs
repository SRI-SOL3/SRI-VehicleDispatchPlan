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
        /// <param name="forecastList">受入予測情報</param>
        /// <param name="forecastByWorkList">勤務属性別受入予測情報</param>
        /// <param name="targetTrainee">教習生情報</param>
        /// <returns>グラフデータ</returns>
        public List<V_ChartData> getChartData(MyDatabaseContext db, DateTime dateFrom, DateTime dateTo,
            List<T_Forecast> forecastList, List<T_ForecastByWork> forecastByWorkList, List<T_Trainee> targetTrainee)
        {
            // 対象日付Fromの月の初日
            dateFrom = new DateTime(dateFrom.Year, dateFrom.Month, 1);
            // 対象日付Toの月の最終日
            dateTo = new DateTime(dateTo.Year, dateTo.Month + 1, 1).AddDays(-1);

            // --------------------
            // 受入予測データの取得
            // --------------------
            // 受入予測が空の場合、DBから取得（教習生管理）
            if (forecastList == null)
            {
                forecastList = new List<T_Forecast>();
                for (int year = dateFrom.Year; year.CompareTo(dateTo.Year) <= 0; year++)
                {
                    for (int month = dateFrom.Month; month.CompareTo(dateTo.Month) <= 0; month++)
                    {
                        T_Forecast forecastDb = db.Forecast.Where(x => x.Year.Equals(year.ToString()) && x.Month.Equals(month.ToString())).FirstOrDefault();
                        if (forecastDb != null)
                        {
                            forecastList.Add(forecastDb);
                        }
                    }
                }
            }

            // 勤務属性別受入予測が空の場合、DBから取得（教習生管理）
            if (forecastByWorkList == null)
            {
                forecastByWorkList = new List<T_ForecastByWork>();
                for (int year = dateFrom.Year; year.CompareTo(dateTo.Year) <= 0; year++)
                {
                    for (int month = dateFrom.Month; month.CompareTo(dateTo.Month) <= 0; month++)
                    {
                        List<T_ForecastByWork> forecastByWorkListDb = db.ForecastByWork.Where(x => x.Year.Equals(year.ToString()) && x.Month.Equals(month.ToString())).ToList();
                        if (forecastByWorkListDb.Count() != 0)
                        {
                            forecastByWorkList.AddRange(forecastByWorkListDb);
                        }
                    }
                }
            }

            // 教習生データが空の場合（売上予測管理）
            List<T_Trainee> traineeList;
            if (targetTrainee == null)
            {
                // 対象期間の全データを取得
                //traineeList = db.Trainee.Where(
                //    x => Math.Abs(x.EntrancePlanDate.CompareTo(dateFrom) + x.EntrancePlanDate.CompareTo(dateTo)) < 2
                //    || Math.Abs(x.GraduatePlanDate.CompareTo(dateFrom) + x.GraduatePlanDate.CompareTo(dateTo)) < 2).ToList();
                traineeList = db.Trainee.Where(
                    x => x.EntrancePlanDate >= dateFrom && x.EntrancePlanDate <= dateTo
                    || x.GraduatePlanDate >= dateFrom && x.GraduatePlanDate <= dateTo).ToList();
            }
            // 教習生データが設定されている場合（教習生管理）
            else
            {
                // 0以外の教習生IDを取得
                List<int> traineeIdList = targetTrainee.Where(x => !x.TraineeId.Equals(0)).Select(x => x.TraineeId).ToList();
                // 対象教習生ID以外を取得
                //traineeList = db.Trainee.Where(
                //    x => !traineeIdList.Contains(x.TraineeId)
                //    && (Math.Abs(x.EntrancePlanDate.CompareTo(dateFrom) + x.EntrancePlanDate.CompareTo(dateTo)) < 2
                //    || Math.Abs(x.GraduatePlanDate.CompareTo(dateFrom) + x.GraduatePlanDate.CompareTo(dateTo)) < 2)).ToList();
                traineeList = db.Trainee.Where(
                    x => !traineeIdList.Contains(x.TraineeId)
                    && (x.EntrancePlanDate >= dateFrom && x.EntrancePlanDate <= dateTo
                    || x.GraduatePlanDate >= dateFrom && x.GraduatePlanDate <= dateTo)).ToList();
            }

            // --------------------
            // 当月受入最大数を算出
            // --------------------
            // 月別受入最大数
            Dictionary<string, double> acceptTotalMaxAmt = new Dictionary<string, double>();
            // 月別合宿受入最大数
            Dictionary<string, double> acceptLodgingMaxAmt = new Dictionary<string, double>();
            // 月別通学受入最大数
            Dictionary<string, double> acceptCommutingMaxAmt = new Dictionary<string, double>();
            // 教習コースマスタからATとMTの実車コマ数を取得
            List<M_TrainingCourse> trainingCourse = db.TrainingCourse.ToList();
            int mtClassQty = trainingCourse.Where(x => x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_MT)).Select(x => x.PracticeClassQty).FirstOrDefault();
            int atClassQty = trainingCourse.Where(x => x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_AT)).Select(x => x.PracticeClassQty).FirstOrDefault();

            for (int year = dateFrom.Year; year.CompareTo(dateTo.Year) <= 0; year++)
            {
                for (int month = dateFrom.Month; month.CompareTo(dateTo.Month) <= 0; month++)
                {
                    // 対象年月の受入予測情報を取得
                    T_Forecast forecast = forecastList.Where(x => x.Year.Equals(year.ToString()) && x.Month.Equals(month.ToString())).FirstOrDefault();
                    // 対象年月の勤務属性別受入予測情報を取得
                    List<T_ForecastByWork> forecastByWork = forecastByWorkList.Where(x => x.Year.Equals(year.ToString()) && x.Month.Equals(month.ToString())).ToList();

                    if (forecast != null && forecastByWork.Count() != 0)
                    {
                        // 1.実車総コマ数/月（時限数/日 * 指導員数/日 * 出勤日数/月 * (100 - 教習外業務比率) / 100）
                        double totalClassQty = forecastByWork.Select(x => x.ClassQty * x.InstructorAmt * x.WorkDays * (100 - x.NotDrivingRatio) / 100).Sum();
                        // 2.教習生一人当たりの実車コマ数
                        double traineeClassQty = mtClassQty * forecast.MtRatio / 100 + atClassQty * (100 - forecast.MtRatio) / 100;
                        // 3.月別受入最大数/月（合計・合宿・通学）を設定
                        acceptTotalMaxAmt.Add(year + "-" + month, totalClassQty / traineeClassQty);
                        acceptLodgingMaxAmt.Add(year + "-" + month, (totalClassQty / traineeClassQty) * forecast.LodgingRatio / 100);
                        acceptCommutingMaxAmt.Add(year + "-" + month, (totalClassQty / traineeClassQty) * (100 - forecast.LodgingRatio) / 100);
                    }
                    else
                    {
                        acceptTotalMaxAmt.Add(year + "-" + month, 0);
                        acceptLodgingMaxAmt.Add(year + "-" + month, 0);
                        acceptCommutingMaxAmt.Add(year + "-" + month, 0);
                    }
                }
            }

            // --------------------
            // 日別の在籍数・受入累積を算出
            // --------------------
            // 受入累積数
            int acceptTotalSumAmt = 0;
            int acceptLodgingSumAmt = 0;
            int acceptCommutingSumAmt = 0;
            // 対象月
            int targetMonth = 0;
            // 対象期間の日付分繰り返し
            List<V_ChartData> chartData = new List<V_ChartData>();
            for (int day = 0; dateFrom.AddDays(day) <= dateTo; day++)
            {
                V_ChartData data = new V_ChartData();
                // 日付を設定
                data.Date = dateFrom.AddDays(day);

                // 受入最大人数を設定
                data.AcceptTotalMaxAmt = Math.Round(acceptTotalMaxAmt[data.Date.Year + "-" + data.Date.Month], 1);
                data.AcceptLodgingMaxAmt = Math.Round(acceptLodgingMaxAmt[data.Date.Year + "-" + data.Date.Month], 1);
                data.AcceptCommutingMaxAmt = Math.Round(acceptCommutingMaxAmt[data.Date.Year + "-" + data.Date.Month], 1);

                // 月が変わった場合
                if (targetMonth != data.Date.Month)
                {
                    // 受入累積数を初期化
                    acceptTotalSumAmt = 0;
                    acceptLodgingSumAmt = 0;
                    acceptCommutingSumAmt = 0;
                    targetMonth = data.Date.Month;
                }
                // 受入累積数を加算
                acceptTotalSumAmt = acceptTotalSumAmt + traineeList.Where(x => x.EntrancePlanDate.Equals(data.Date)).Count();
                acceptLodgingSumAmt = acceptLodgingSumAmt + traineeList.Where(x => x.EntrancePlanDate.Equals(data.Date) 
                    && x.AttendTypeCd.Equals(AppConstant.CD_ATTEND_TYPE_LODGING)).Count();
                acceptCommutingSumAmt = acceptCommutingSumAmt + traineeList.Where(x => x.EntrancePlanDate.Equals(data.Date)
                    && x.AttendTypeCd.Equals(AppConstant.CD_ATTEND_TYPE_COMMUTING)).Count();
                data.AcceptTotalSumAmt = acceptTotalSumAmt;
                data.AcceptLodgingSumAmt = acceptLodgingSumAmt;
                data.AcceptCommutingSumAmt = acceptCommutingSumAmt;

                // 合宿在籍数(MT-一段階)（合宿かつ、MTかつ、入校予定日が対象日以上かつ、仮免予定日が対象日未満）
                //data.LodgingMtFstRegAmt = traineeList.Where(x => x.AttendTypeCd.Equals(AppConstant.CD_ATTEND_TYPE_LODGING)
                //    && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_MT)
                //    && x.EntrancePlanDate.CompareTo(data.Date) <= 0 && x.TmpLicencePlanDate.CompareTo(data.Date) > 0).Count();
                data.LodgingMtFstRegAmt = traineeList.Where(x => x.AttendTypeCd.Equals(AppConstant.CD_ATTEND_TYPE_LODGING)
                    && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_MT)
                    && x.EntrancePlanDate <= data.Date && x.TmpLicencePlanDate > data.Date).Count();

                // 合宿在籍数(MT-二段階)（合宿かつ、MTかつ、仮免予定日が対象日以上かつ、卒業予定日予定日が対象日以下）
                //data.LodgingMtSndRegAmt = traineeList.Where(x => x.AttendTypeCd.Equals(AppConstant.CD_ATTEND_TYPE_LODGING)
                //    && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_MT)
                //    && x.TmpLicencePlanDate.CompareTo(data.Date) <= 0 && x.GraduatePlanDate.CompareTo(data.Date) >= 0).Count();
                data.LodgingMtSndRegAmt = traineeList.Where(x => x.AttendTypeCd.Equals(AppConstant.CD_ATTEND_TYPE_LODGING)
                    && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_MT)
                    && x.TmpLicencePlanDate <= data.Date && x.GraduatePlanDate >= data.Date).Count();

                // 合宿在籍数(AT-一段階)（合宿かつ、ATかつ、入校予定日が対象日以上かつ、仮免予定日が対象日未満）
                //data.LodgingAtFstRegAmt = traineeList.Where(x => x.AttendTypeCd.Equals(AppConstant.CD_ATTEND_TYPE_LODGING)
                //    && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_AT)
                //    && x.EntrancePlanDate.CompareTo(data.Date) <= 0 && x.TmpLicencePlanDate.CompareTo(data.Date) > 0).Count();
                data.LodgingAtFstRegAmt = traineeList.Where(x => x.AttendTypeCd.Equals(AppConstant.CD_ATTEND_TYPE_LODGING)
                    && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_AT)
                    && x.EntrancePlanDate <= data.Date && x.TmpLicencePlanDate > data.Date).Count();

                // 合宿在籍数(AT-二段階)（合宿かつ、ATかつ、仮免予定日が対象日以上かつ、卒業予定日予定日が対象日以下）
                //data.LodgingAtSndRegAmt = traineeList.Where(x => x.AttendTypeCd.Equals(AppConstant.CD_ATTEND_TYPE_LODGING)
                //    && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_AT)
                //    && x.TmpLicencePlanDate.CompareTo(data.Date) <= 0 && x.GraduatePlanDate.CompareTo(data.Date) >= 0).Count();
                data.LodgingAtSndRegAmt = traineeList.Where(x => x.AttendTypeCd.Equals(AppConstant.CD_ATTEND_TYPE_LODGING)
                    && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_AT)
                    && x.TmpLicencePlanDate <= data.Date && x.GraduatePlanDate >= data.Date).Count();

                // 通学在籍数(MT)（通学かつ、MTかつ、入校予定日が対象日以上かつ、卒業予定日予定日が対象日以下）
                //data.CommutingMtRegAmt = traineeList.Where(x => x.AttendTypeCd.Equals(AppConstant.CD_ATTEND_TYPE_COMMUTING)
                //    && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_MT)
                //    && x.EntrancePlanDate.CompareTo(data.Date) <= 0 && x.GraduatePlanDate.CompareTo(data.Date) >= 0).Count();
                data.CommutingMtRegAmt = traineeList.Where(x => x.AttendTypeCd.Equals(AppConstant.CD_ATTEND_TYPE_COMMUTING)
                    && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_MT)
                    && x.EntrancePlanDate <= data.Date && x.GraduatePlanDate >= data.Date).Count();

                // 通学在籍数(AT)（通学かつ、ATかつ、入校予定日が対象日以上かつ、卒業予定日予定日が対象日以下）
                //data.CommutingAtRegAmt = traineeList.Where(x => x.AttendTypeCd.Equals(AppConstant.CD_ATTEND_TYPE_COMMUTING)
                //    && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_AT)
                //    && x.EntrancePlanDate.CompareTo(data.Date) <= 0 && x.GraduatePlanDate.CompareTo(data.Date) >= 0).Count();
                data.CommutingAtRegAmt = traineeList.Where(x => x.AttendTypeCd.Equals(AppConstant.CD_ATTEND_TYPE_COMMUTING)
                    && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_AT)
                    && x.EntrancePlanDate <= data.Date && x.GraduatePlanDate >= data.Date).Count();

                // 引数の教習生情報が設定されている場合はデータに追加
                if (targetTrainee != null)
                {
                    // 合宿在籍数(MT-一段階)
                    //data.LodgingMtFstRegAmt = data.LodgingMtFstRegAmt
                    //    + targetTrainee.Where(x => x.AttendTypeCd.Equals(AppConstant.CD_ATTEND_TYPE_LODGING)
                    //        && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_MT)
                    //        && x.EntrancePlanDate.CompareTo(data.Date) <= 0 && x.TmpLicencePlanDate.CompareTo(data.Date) > 0).Count();
                    data.LodgingMtFstRegAmt = data.LodgingMtFstRegAmt
                        + targetTrainee.Where(x => x.AttendTypeCd.Equals(AppConstant.CD_ATTEND_TYPE_LODGING)
                            && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_MT)
                            && x.EntrancePlanDate <= data.Date && x.TmpLicencePlanDate > data.Date).Count();
                    // 合宿在籍数(MT-二段階)
                    //data.LodgingMtSndRegAmt = data.LodgingMtSndRegAmt
                    //    + targetTrainee.Where(x => x.AttendTypeCd.Equals(AppConstant.CD_ATTEND_TYPE_LODGING)
                    //        && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_MT)
                    //        && x.TmpLicencePlanDate.CompareTo(data.Date) <= 0 && x.GraduatePlanDate.CompareTo(data.Date) >= 0).Count();
                    data.LodgingMtSndRegAmt = data.LodgingMtSndRegAmt
                        + targetTrainee.Where(x => x.AttendTypeCd.Equals(AppConstant.CD_ATTEND_TYPE_LODGING)
                            && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_MT)
                            && x.TmpLicencePlanDate <= data.Date && x.GraduatePlanDate >= data.Date).Count();
                    // 合宿在籍数(AT-一段階)
                    //data.LodgingAtFstRegAmt = data.LodgingAtFstRegAmt
                    //    + targetTrainee.Where(x => x.AttendTypeCd.Equals(AppConstant.CD_ATTEND_TYPE_LODGING)
                    //        && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_AT)
                    //        && x.EntrancePlanDate.CompareTo(data.Date) <= 0 && x.TmpLicencePlanDate.CompareTo(data.Date) > 0).Count();
                    data.LodgingAtFstRegAmt = data.LodgingAtFstRegAmt
                        + targetTrainee.Where(x => x.AttendTypeCd.Equals(AppConstant.CD_ATTEND_TYPE_LODGING)
                            && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_AT)
                            && x.EntrancePlanDate <= data.Date && x.TmpLicencePlanDate > data.Date).Count();
                    // 合宿在籍数(AT-二段階)
                    //data.LodgingAtSndRegAmt = data.LodgingAtSndRegAmt
                    //    + targetTrainee.Where(x => x.AttendTypeCd.Equals(AppConstant.CD_ATTEND_TYPE_LODGING)
                    //        && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_AT)
                    //        && x.TmpLicencePlanDate.CompareTo(data.Date) <= 0 && x.GraduatePlanDate.CompareTo(data.Date) >= 0).Count();
                    data.LodgingAtSndRegAmt = data.LodgingAtSndRegAmt
                        + targetTrainee.Where(x => x.AttendTypeCd.Equals(AppConstant.CD_ATTEND_TYPE_LODGING)
                            && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_AT)
                            && x.TmpLicencePlanDate <= data.Date && x.GraduatePlanDate >= data.Date).Count();
                    // 通学在籍数(MT)
                    //data.CommutingMtRegAmt = data.CommutingMtRegAmt
                    //    + targetTrainee.Where(x => x.AttendTypeCd.Equals(AppConstant.CD_ATTEND_TYPE_COMMUTING)
                    //        && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_MT)
                    //        && x.EntrancePlanDate.CompareTo(data.Date) <= 0 && x.GraduatePlanDate.CompareTo(data.Date) >= 0).Count();
                    data.CommutingMtRegAmt = data.CommutingMtRegAmt
                        + targetTrainee.Where(x => x.AttendTypeCd.Equals(AppConstant.CD_ATTEND_TYPE_COMMUTING)
                            && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_MT)
                            && x.EntrancePlanDate <= data.Date && x.GraduatePlanDate >= data.Date).Count();
                    // 通学在籍数(AT)
                    //data.CommutingAtRegAmt = data.CommutingAtRegAmt
                    //    + targetTrainee.Where(x => x.AttendTypeCd.Equals(AppConstant.CD_ATTEND_TYPE_COMMUTING)
                    //        && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_AT)
                    //        && x.EntrancePlanDate.CompareTo(data.Date) <= 0 && x.GraduatePlanDate.CompareTo(data.Date) >= 0).Count();
                    data.CommutingAtRegAmt = data.CommutingAtRegAmt
                        + targetTrainee.Where(x => x.AttendTypeCd.Equals(AppConstant.CD_ATTEND_TYPE_COMMUTING)
                            && x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_AT)
                            && x.EntrancePlanDate <= data.Date && x.GraduatePlanDate >= data.Date).Count();
                }

                chartData.Add(data);
            }

            return chartData;
        }

        /// <summary>
        /// グラフ画像パス取得
        /// </summary>
        /// <param name="year">年</param>
        /// <param name="month">月</param>
        /// <param name="chartData">グラフデータ</param>
        /// <param name="chartTotalRem">グラフ表示_総受入残数</param>
        /// <param name="chartLodgingRem">グラフ表示_合宿受入残数</param>
        /// <param name="chartCommutingRem">グラフ表示_通学受入残数</param>
        /// <param name="chartTotalReg">グラフ表示_総在籍数</param>
        /// <param name="chartLodgingReg">グラフ表示_合宿在籍数</param>
        /// <param name="chartCommutingReg">グラフ表示_通学在籍数</param>
        /// <returns>画像パス</returns>
        public string getChartPath(string year, string month, List<V_ChartData> chartData, 
            bool chartTotalRem, bool chartLodgingRem, bool chartCommutingRem, bool chartTotalReg, bool chartLodgingReg, bool chartCommutingReg)
        {
            // グラフを作成
            Chart chart = new Chart()
            {
                Height = 500,
                Width = 2000,
                ImageType = ChartImageType.Png,
                // TODO:ユーザーＩＤとかにする？
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
                },
                Legends =
                    {
                        new Legend
                        {
                            Title = "凡例"
                        }
                    }
            };

            chart.Series.Clear();

            // 総受入残数
            if (chartTotalRem == true)
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
            if (chartLodgingRem == true)
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
            if (chartCommutingRem == true)
            {
                chart.Series.Add(AppConstant.SERIES_COMMUTING_REM_AMT);
                chart.Series[AppConstant.SERIES_COMMUTING_REM_AMT].ChartType = SeriesChartType.Line;
                chart.Series[AppConstant.SERIES_COMMUTING_REM_AMT].Color = Color.FromArgb(0, 200, 0);
                chart.Series[AppConstant.SERIES_COMMUTING_REM_AMT].MarkerStyle = MarkerStyle.None;
                chart.Series[AppConstant.SERIES_COMMUTING_REM_AMT].MarkerColor = Color.FromArgb(0, 200, 0);
                chart.Series[AppConstant.SERIES_COMMUTING_REM_AMT].BorderWidth = 3;
                chart.Series[AppConstant.SERIES_COMMUTING_REM_AMT].BorderColor = Color.FromArgb(0, 200, 0);
            }

            // 総在籍数
            if (chartTotalReg == true)
            {
                chart.Series.Add(AppConstant.SERIES_TOTAL_REG_AMT);
                chart.Series[AppConstant.SERIES_TOTAL_REG_AMT].ChartType = SeriesChartType.Area;
                chart.Series[AppConstant.SERIES_TOTAL_REG_AMT].Color = Color.FromArgb(50, 200, 60, 60);
                chart.Series[AppConstant.SERIES_TOTAL_REG_AMT].MarkerStyle = MarkerStyle.Circle;
                chart.Series[AppConstant.SERIES_TOTAL_REG_AMT].MarkerColor = Color.FromArgb(200, 60, 60);
                chart.Series[AppConstant.SERIES_TOTAL_REG_AMT].BorderWidth = 2;
                chart.Series[AppConstant.SERIES_TOTAL_REG_AMT].BorderColor = Color.FromArgb(200, 60, 60);
            }

            // 合宿在籍数
            if (chartLodgingReg == true)
            {
                chart.Series.Add(AppConstant.SERIES_LODGING_REG_AMT);
                chart.Series[AppConstant.SERIES_LODGING_REG_AMT].ChartType = SeriesChartType.Area;
                chart.Series[AppConstant.SERIES_LODGING_REG_AMT].Color = Color.FromArgb(50, 30, 100, 160);
                chart.Series[AppConstant.SERIES_LODGING_REG_AMT].MarkerStyle = MarkerStyle.Circle;
                chart.Series[AppConstant.SERIES_LODGING_REG_AMT].MarkerColor = Color.FromArgb(30, 100, 160);
                chart.Series[AppConstant.SERIES_LODGING_REG_AMT].BorderWidth = 3;
                chart.Series[AppConstant.SERIES_LODGING_REG_AMT].BorderColor = Color.FromArgb(30, 100, 160);
            }

            // 通学在籍数
            if (chartCommutingReg == true)
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
                if (chartTotalRem == true)
                {
                    chart.Series[AppConstant.SERIES_TOTAL_REM_AMT].Points.AddXY(data.Date.ToString("M/d"), data.AcceptTotalRemAmt);
                }

                // 合宿受入残数
                if (chartLodgingRem == true)
                {
                    chart.Series[AppConstant.SERIES_LODGING_REM_AMT].Points.AddXY(data.Date.ToString("M/d"), data.AcceptLodgingRemAmt);
                }

                // 通学受入残数
                if (chartCommutingRem == true)
                {
                    chart.Series[AppConstant.SERIES_COMMUTING_REM_AMT].Points.AddXY(data.Date.ToString("M/d"), data.AcceptCommutingRemAmt);
                }

                // 合宿在籍数＆通学在籍数
                int lodgingRegAmt = data.LodgingMtFstRegAmt + data.LodgingMtSndRegAmt + data.LodgingAtFstRegAmt + data.LodgingMtSndRegAmt;
                int commutingRegAmt = data.CommutingMtRegAmt + data.CommutingAtRegAmt;

                // 総在籍数
                if (chartTotalReg == true)
                {
                    chart.Series[AppConstant.SERIES_TOTAL_REG_AMT].Points.AddXY(data.Date.ToString("M/d"), lodgingRegAmt + commutingRegAmt);
                }
                
                // 合宿受入可能残数
                if (chartLodgingReg == true)
                {
                    chart.Series[AppConstant.SERIES_LODGING_REG_AMT].Points.AddXY(data.Date.ToString("M/d"), lodgingRegAmt);
                }

                // 通学受入可能残数
                if (chartCommutingReg == true)
                {
                    chart.Series[AppConstant.SERIES_COMMUTING_REG_AMT].Points.AddXY(data.Date.ToString("M/d"), commutingRegAmt);
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