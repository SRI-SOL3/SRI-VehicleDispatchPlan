using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using VehicleDispatchPlan.Constants;
using VehicleDispatchPlan.Models;

/**
 * 共通処理
 *
 * @author t-murayama
 * @version 1.0
 * ----------------------------------
 * 2020/03/01 t-murayama 新規作成
 * 2021/01/26 t-murayama 20210205リリース対応(ver.1.1)
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
        /// グラフデータ取得（予測値算出ロジック）
        /// </summary>
        /// <param name="db">DBコンテキスト</param>
        /// <param name="dateFrom">日付From</param>
        /// <param name="dateTo">日付To</param>
        /// <param name="targetTraineeLodging">合宿教習生情報(登録/更新時)</param>
        /// <param name="targetTraineeCommuting">通学教習生情報(登録/更新時)</param>
        /// <returns>グラフデータ</returns>
        public List<V_ChartData> GetChartData(MyDatabaseContext db, DateTime dateFrom, DateTime dateTo
            , List<T_TraineeLodging> targetTraineeLodging, T_TraineeCommuting targetTraineeCommuting)
        {
            List<V_ChartData> chartData = new List<V_ChartData>();

            /*
             * [処理説明]
             * 繰り返し処理の開始を日曜、終了を土曜にするため、
             * 「日付Fromの直近の日曜」と「日付Toの直後の土曜」を取得する。
             * DayOfWeek：日曜=0～土曜=6
             * [目的]
             * 週平均の在籍可能数を算出するため、検索範囲外の日曜～土曜の範囲で算出する必要がある。
             */
            // 日付Fromの直近の日曜を取得
            DateTime sunFrom = dateFrom.AddDays(-1 * (int)dateFrom.DayOfWeek);
            // 日付Toの直後の土曜を取得
            DateTime satTo = dateTo.AddDays((int)DayOfWeek.Saturday - (int)dateTo.DayOfWeek);

            // 合宿教習生の取得
            List<T_TraineeLodging> traineeLodging;
            // 引数の教習生がnullの場合（受入予測管理）
            if (targetTraineeLodging == null)
            {
                // 対象期間の教習生データを全て取得
                traineeLodging = db.TraineeLodging.Where(
                    x => x.CancelFlg == false
                    && (sunFrom <= x.EntrancePlanDate && x.EntrancePlanDate <= satTo
                    || sunFrom <= x.GraduatePlanDate && x.GraduatePlanDate <= satTo
                    || x.EntrancePlanDate < sunFrom && satTo < x.GraduatePlanDate)).ToList();
            }
            // 引数の教習生がnullでない場合（教習生管理(登録/更新)）
            else
            {
                // 0以外の教習生IDを取得
                List<int> traineeIdList = targetTraineeLodging.Where(x => !x.TraineeId.Equals(0)).Select(x => x.TraineeId).ToList();
                // 対象教習生ID以外を取得
                traineeLodging = db.TraineeLodging.Where(
                    x => !traineeIdList.Contains(x.TraineeId)
                    && x.CancelFlg == false
                    && (sunFrom <= x.EntrancePlanDate && x.EntrancePlanDate <= satTo
                    || sunFrom <= x.GraduatePlanDate && x.GraduatePlanDate <= satTo
                    || x.EntrancePlanDate < sunFrom && satTo < x.GraduatePlanDate)).ToList();
                // 対象教習生を追加
                traineeLodging.AddRange(targetTraineeLodging.Where(x => x.CancelFlg == false));
            }

            // 通学教習生の取得
            List<T_TraineeCommuting> traineeCommuting;
            // 引数の教習生がnullの場合（受入予測管理）
            if (targetTraineeCommuting == null)
            {
                // 対象期間の教習生データを全て取得
                traineeCommuting = db.TraineeCommuting.Where(
                    x => x.CancelFlg == false
                    && sunFrom <= x.EntrancePlanDate && x.EntrancePlanDate <= satTo
                    || sunFrom <= x.GraduatePlanDate && x.GraduatePlanDate <= satTo
                    || x.EntrancePlanDate < sunFrom && satTo < x.GraduatePlanDate).ToList();
            }
            // 引数の教習生がnullでない場合（教習生管理(登録/更新)）
            else
            {
                // 対象教習生IDを取得
                int traineeId = targetTraineeCommuting.TraineeId;
                // 対象教習生ID以外を取得
                traineeCommuting = db.TraineeCommuting.Where(
                    x => !x.TraineeId.Equals(traineeId)
                    && x.CancelFlg == false
                    && (sunFrom <= x.EntrancePlanDate && x.EntrancePlanDate <= satTo
                    || sunFrom <= x.GraduatePlanDate && x.GraduatePlanDate <= satTo
                    || x.EntrancePlanDate < sunFrom && satTo < x.GraduatePlanDate)).ToList();
                // 対象教習生を追加
                if (targetTraineeCommuting.CancelFlg == false)
                {
                    traineeCommuting.Add(targetTraineeCommuting);
                }
            }

            // 対象期間の日別コマ数データを取得
            List<T_DailyClasses> dailyClassesList = db.DailyClasses.Where(x => sunFrom <= x.Date && x.Date <= satTo).ToList();
            // 対象期間の指導員データを取得
            List<T_DailyClassesByTrainer> trainerList = db.DailyClassesByTrainer.Where(x => x.Date >= sunFrom && x.Date <= satTo).ToList();

            // 受入可能人数/期間
            double acceptLodgingMaxAmt = 0;
            double acceptCommutingMaxAmt = 0;
            // 受入累積数
            Dictionary<DateTime, int> acceptLodgingTotalAmtDic = new Dictionary<DateTime, int>();
            Dictionary<DateTime, int> acceptCommutingTotalAmtDic = new Dictionary<DateTime, int>();
            // 在籍可能数/週
            double weeklyLodgingSumAmt = 0;
            double weeklyCommutingSumAmt = 0;
            // 在籍可能数/日（週平均）
            Dictionary<DateTime, double> dailyLodgingMaxAmtDic = new Dictionary<DateTime, double>();
            Dictionary<DateTime, double> dailyCommutingMaxAmtDic = new Dictionary<DateTime, double>();
            // 残コマ数/日
            double dailyRemClasses = 0;
            // 残コマ数/週
            double weeklyRemClasses = 0;
            Dictionary<DateTime, double> weeklyRemClassesDic = new Dictionary<DateTime, double>();

            for (DateTime day = sunFrom; day <= satTo; day = day.AddDays(1))
            {
                // 日別予測条件
                T_DailyClasses dailyClasses = dailyClassesList.Where(x => ((DateTime)x.Date).Equals(day)).FirstOrDefault();
                if (dailyClasses == null)
                {
                    dailyClasses = new T_DailyClasses() { Date = day };
                }
                // 教習総コマ数/日
                double dailySumClasses = trainerList.Where(x => x.Date.Equals(day)).Select(x => x.Classes).Sum();

                // --------------------
                // 在籍可能数/日
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
                    // 週合計に加算
                    weeklyLodgingSumAmt += (dailySumClasses / ldgDailyReqClasses) * (dailyClasses.LodgingRatio / 100);
                }
                // ②-2.【通学】在籍可能人数/日  ※①-2が0の場合は算出しない
                if (cmtDailyReqClasses != 0)
                {
                    // 週合計に加算
                    weeklyCommutingSumAmt += (dailySumClasses / cmtDailyReqClasses) * (dailyClasses.CommutingRatio / 100);
                }

                // --------------------
                // 在籍数
                // --------------------
                // 合宿在籍数(MT-一段階)（教習がMTかつ、入校予定日が対象日以上かつ、仮免予定日が対象日未満）
                int lodgingMtFstRegAmt =
                    traineeLodging.Where(
                        x => x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_MT)
                        && x.EntrancePlanDate <= day && day < x.TmpLicencePlanDate).Count();
                // [20210205リリース対応] Mod Start 卒業日以下を卒業日未満に修正
                //// 合宿在籍数(MT-二段階)（教習がMTかつ、仮免予定日が対象日以上かつ、卒業予定日が対象日以下）
                //int lodgingMtSndRegAmt =
                //    traineeLodging.Where(
                //        x => x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_MT)
                //        && x.TmpLicencePlanDate <= day && day <= x.GraduatePlanDate).Count();
                // 合宿在籍数(MT-二段階)（教習がMTかつ、仮免予定日が対象日以上かつ、卒業予定日が対象日未満）
                int lodgingMtSndRegAmt =
                    traineeLodging.Where(
                        x => x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_MT)
                        && x.TmpLicencePlanDate <= day && day < x.GraduatePlanDate).Count();
                // [20210205リリース対応] Mod End
                // [20210205リリース対応] Del Start
                //// 合宿在籍数(MT-二段階)（教習がMTかつ、仮免予定日が対象日以上かつ、卒業予定日が対象日未満）※卒業予定者を含まない
                //int lodgingMtRegAmtExceptGraduate =
                //    traineeLodging.Where(
                //        x => x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_MT)
                //        && x.TmpLicencePlanDate <= day && day < x.GraduatePlanDate).Count();
                // [20210205リリース対応] Del End
                // 合宿在籍数(AT-一段階)（教習がATかつ、入校予定日が対象日以上かつ、仮免予定日が対象日未満）
                int lodgingAtFstRegAmt =
                    traineeLodging.Where(
                        x => x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_AT)
                        && x.EntrancePlanDate <= day && day < x.TmpLicencePlanDate).Count();
                // [20210205リリース対応] Mod Start 卒業日以下を卒業日未満に修正
                //// 合宿在籍数(AT-二段階)（教習がATかつ、仮免予定日が対象日以上かつ、卒業予定日が対象日以下）
                //int lodgingAtSndRegAmt =
                //    traineeLodging.Where(
                //        x => x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_AT)
                //        && x.TmpLicencePlanDate <= day && day <= x.GraduatePlanDate).Count();
                // 合宿在籍数(AT-二段階)（教習がATかつ、仮免予定日が対象日以上かつ、卒業予定日が対象日未満）
                int lodgingAtSndRegAmt =
                    traineeLodging.Where(
                        x => x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_AT)
                        && x.TmpLicencePlanDate <= day && day < x.GraduatePlanDate).Count();
                // [20210205リリース対応] Mod End
                // [20210205リリース対応] Del Start
                //// 合宿在籍数(AT-二段階)（教習がATかつ、仮免予定日が対象日以上かつ、卒業予定日が対象日未満）※卒業予定者を含まない
                //int lodgingAtRegAmtExceptGraduate =
                //    traineeLodging.Where(
                //        x => x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_AT)
                //        && x.TmpLicencePlanDate <= day && day < x.GraduatePlanDate).Count();
                // [20210205リリース対応] Del End
                // 通学在籍数(MT-一段階)（教習がMTかつ、入校予定日が対象日以上かつ、仮免予定日が対象日未満）
                int commutingMtFstRegAmt =
                    traineeCommuting.Where(
                        x => x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_MT)
                        && x.EntrancePlanDate <= day && day < x.TmpLicencePlanDate).Count();
                // [20210205リリース対応] Mod Start 卒業日以下を卒業日未満に修正
                //// 通学在籍数(MT-二段階)（教習がMTかつ、仮免予定日が対象日以上かつ、卒業予定日が対象日以下）
                //int commutingMtSndRegAmt =
                //    traineeCommuting.Where(
                //        x => x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_MT)
                //        && x.TmpLicencePlanDate <= day && day <= x.GraduatePlanDate).Count();
                // 通学在籍数(MT-二段階)（教習がMTかつ、仮免予定日が対象日以上かつ、卒業予定日が対象日未満）
                int commutingMtSndRegAmt =
                    traineeCommuting.Where(
                        x => x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_MT)
                        && x.TmpLicencePlanDate <= day && day < x.GraduatePlanDate).Count();
                // [20210205リリース対応] Mod End
                // 通学在籍数(AT-一段階)（教習がATかつ、入校予定日が対象日以上かつ、仮免予定日が対象日未満）
                int commutingAtFstRegAmt =
                    traineeCommuting.Where(
                        x => x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_AT)
                        && x.EntrancePlanDate <= day && day < x.TmpLicencePlanDate).Count();
                // [20210205リリース対応] Mod Start 卒業日以下を卒業日未満に修正
                //// 通学在籍数(AT-二段階)（教習がATかつ、仮免予定日が対象日以上かつ、卒業予定日が対象日以下）
                //int commutingAtSndRegAmt =
                //    traineeCommuting.Where(
                //        x => x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_AT)
                //        && x.TmpLicencePlanDate <= day && day <= x.GraduatePlanDate).Count();
                // 通学在籍数(AT-二段階)（教習がATかつ、仮免予定日が対象日以上かつ、卒業予定日が対象日未満）
                int commutingAtSndRegAmt =
                    traineeCommuting.Where(
                        x => x.TrainingCourseCd.Equals(AppConstant.TRAINING_COURSE_CD_AT)
                        && x.TmpLicencePlanDate <= day && day < x.GraduatePlanDate).Count();
                // [20210205リリース対応] Mod End

                // --------------------
                // 当日の合宿生消化コマ数
                // --------------------
                // [20210205リリース対応] Mod Start 卒業日以下をlodgingMtSndRegAmt/lodgingAtSndRegAmtに統一
                //// 合宿生のMT一段階コマ数/日 × 合宿生のMT一段階在籍数
                //// ＋ 合宿生のMT二段階コマ数/日 × 合宿生のMT二段階在籍数（卒業予定者を含まない）
                //// ＋ 合宿生のAT一段階コマ数/日 × 合宿生のAT一段階在籍数
                //// ＋ 合宿生のAT二段階コマ数/日 × 合宿生のAT二段階在籍数（卒業予定者を含まない）
                //double sumClasses =
                //    dailyClasses.LdgMtFstClassDay * lodgingMtFstRegAmt
                //    + dailyClasses.LdgMtSndClassDay * lodgingMtRegAmtExceptGraduate
                //    + dailyClasses.LdgAtFstClassDay * lodgingAtFstRegAmt
                //    + dailyClasses.LdgAtSndClassDay * lodgingAtRegAmtExceptGraduate;

                // 合宿生のMT一段階コマ数/日 × 合宿生のMT一段階在籍数
                // ＋ 合宿生のMT二段階コマ数/日 × 合宿生のMT二段階在籍数
                // ＋ 合宿生のAT一段階コマ数/日 × 合宿生のAT一段階在籍数
                // ＋ 合宿生のAT二段階コマ数/日 × 合宿生のAT二段階在籍数
                double sumClasses =
                    dailyClasses.LdgMtFstClassDay * lodgingMtFstRegAmt
                    + dailyClasses.LdgMtSndClassDay * lodgingMtSndRegAmt
                    + dailyClasses.LdgAtFstClassDay * lodgingAtFstRegAmt
                    + dailyClasses.LdgAtSndClassDay * lodgingAtSndRegAmt;
                // [20210205リリース対応] Mod End
                // 残コマ数/日
                dailyRemClasses = Math.Round(dailySumClasses - sumClasses, 1);
                // 残コマ数/週
                weeklyRemClasses += dailyRemClasses;

                // 土曜の場合
                if (day.DayOfWeek.Equals(DayOfWeek.Saturday))
                {
                    // 過去7日分を繰り返し
                    for (DateTime tmpDay = day.AddDays(-6); tmpDay <= day; tmpDay = tmpDay.AddDays(1))
                    {
                        // 週平均の在籍可能数を設定（在籍可能数/週 ÷ 7）
                        dailyLodgingMaxAmtDic.Add(tmpDay, weeklyLodgingSumAmt / 7);
                        dailyCommutingMaxAmtDic.Add(tmpDay, weeklyCommutingSumAmt / 7);
                        // 対象の日付に残コマ数/週を設定
                        weeklyRemClassesDic.Add(tmpDay, weeklyRemClasses);
                    }

                    // 各変数をリセット
                    weeklyRemClasses = 0; // 残コマ数/週
                    weeklyLodgingSumAmt = 0;  // 合宿在籍可能数/週
                    weeklyCommutingSumAmt = 0;  // 通学在籍可能数/週
                }

                // 日付が検索範囲内の場合、グラフデータを生成
                if (dateFrom <= day && day <= dateTo)
                {
                    // --------------------
                    // 受入可能数/期間
                    // --------------------
                    // ①-1.【合宿】教習生一人が卒業までに必要なコマ数/日
                    //     ＝ (合宿生のAT一段階コマ数 ＋ 合宿生のAT二段階コマ数) × (合宿生のAT一段階比率[%] ＋ 合宿生のAT二段階比率[%]) ÷ 100
                    //       ＋ (合宿生のMT一段階コマ数 ＋ 合宿生のMT二段階コマ数) × (合宿生のMT一段階比率[%] ＋ 合宿生のMT二段階比率[%]) ÷ 100
                    double ldgTraineeReqClasses = (dailyClasses.LdgAtFstClass + dailyClasses.LdgAtSndClass) * (dailyClasses.LdgAtFstRatio + dailyClasses.LdgAtSndRatio) / 100
                        + (dailyClasses.LdgMtFstClass + dailyClasses.LdgMtSndClass) * (dailyClasses.LdgMtFstRatio + dailyClasses.LdgMtSndRatio) / 100;
                    // ①-2.【通学】教習生一人が卒業までに必要なコマ数/日
                    //     ＝ (通学生のAT一段階コマ数 ＋ 通学生のAT二段階コマ数) × (通学生のAT一段階比率[%] ＋ 通学生のAT二段階比率[%]) ÷ 100
                    //       ＋ (通学生のMT一段階コマ数 ＋ 通学生のMT二段階コマ数) × (通学生のMT一段階比率[%] ＋ 通学生のMT二段階比率[%]) ÷ 100
                    double cmtTraineeReqClasses = (dailyClasses.CmtAtFstClass + dailyClasses.CmtAtSndClass) * (dailyClasses.CmtAtFstRatio + dailyClasses.CmtAtSndRatio) / 100
                        + (dailyClasses.CmtMtFstClass + dailyClasses.CmtMtSndClass) * (dailyClasses.CmtMtFstRatio + dailyClasses.CmtMtSndRatio) / 100;

                    // ②-1.【合宿】受入可能人数/日を加算  ※①-1が0の場合は加算しない
                    if (ldgTraineeReqClasses != 0)
                    {
                        acceptLodgingMaxAmt += (dailySumClasses / ldgTraineeReqClasses) * (dailyClasses.LodgingRatio / 100);
                    }
                    // ②-2.【通学】受入可能人数/日を加算  ※①-2が0の場合は加算しない
                    if (cmtTraineeReqClasses != 0)
                    {
                        acceptCommutingMaxAmt += (dailySumClasses / cmtTraineeReqClasses) * (dailyClasses.CommutingRatio / 100);
                    }

                    // --------------------
                    // 受入累積数
                    // --------------------
                    // 前日までの受入累計数
                    int beforeLodgingTotalAmt = acceptLodgingTotalAmtDic.ContainsKey(day.AddDays(-1)) ? acceptLodgingTotalAmtDic[day.AddDays(-1)] : 0;
                    int beforeCommutingTotalAmt = acceptCommutingTotalAmtDic.ContainsKey(day.AddDays(-1)) ? acceptCommutingTotalAmtDic[day.AddDays(-1)] : 0;
                    // 受入累積数を加算
                    acceptLodgingTotalAmtDic.Add(day, beforeLodgingTotalAmt + traineeLodging.Where(x => x.EntrancePlanDate.Equals(day)).Count());
                    acceptCommutingTotalAmtDic.Add(day, beforeCommutingTotalAmt + traineeCommuting.Where(x => x.EntrancePlanDate.Equals(day)).Count());

                    // グラフデータのインスタンスを生成
                    V_ChartData data = new V_ChartData { Date = day };

                    // --------------------
                    // 在籍数
                    // --------------------
                    // 合宿在籍数(MT-一段階)
                    data.LodgingMtFstRegAmt = lodgingMtFstRegAmt;
                    // 合宿在籍数(MT-二段階)
                    data.LodgingMtSndRegAmt = lodgingMtSndRegAmt;
                    // 合宿在籍数(AT-一段階)
                    data.LodgingAtFstRegAmt = lodgingAtFstRegAmt;
                    // 合宿在籍数(AT-二段階)
                    data.LodgingAtSndRegAmt = lodgingAtSndRegAmt;
                    // 通学在籍数(MT-一段階)
                    data.CommutingMtFstRegAmt = commutingMtFstRegAmt;
                    // 通学在籍数(MT-二段階)
                    data.CommutingMtSndRegAmt = commutingMtSndRegAmt;
                    // 通学在籍数(AT-一段階)
                    data.CommutingAtFstRegAmt = commutingAtFstRegAmt;
                    // 通学在籍数(AT-二段階)
                    data.CommutingAtSndRegAmt = commutingAtSndRegAmt;

                    // [20210205リリース対応] Add Start 総コマ数/日を追加
                    // --------------------
                    // 総コマ数/日
                    // --------------------
                    // 教習総コマ数/日
                    data.DailySumClasses = dailySumClasses;
                    // [20210205リリース対応] Add End
                    // --------------------
                    // 残コマ数/日
                    // --------------------
                    // 教習総コマ数/日 - 消化コマ数/日
                    data.DailyRemClasses = dailyRemClasses;

                    chartData.Add(data);
                }
            }

            chartData.ForEach(x => {
                // 受入残数を全てのデータに設定
                x.AcceptLodgingRemAmt = Math.Round(acceptLodgingMaxAmt - acceptLodgingTotalAmtDic[x.Date], 1);
                x.AcceptCommutingRemAmt = Math.Round(acceptCommutingMaxAmt - acceptCommutingTotalAmtDic[x.Date], 1);
                // 在籍可能数/日の週平均を全てのデータに設定
                x.DailyLodgingMaxAmt = Math.Round(dailyLodgingMaxAmtDic[x.Date], 1);
                x.DailyCommutingMaxAmt = Math.Round(dailyCommutingMaxAmtDic[x.Date], 1);
                // 残コマ数/週を全てのデータに設定
                x.WeeklyRemClasses = Math.Round(weeklyRemClassesDic[x.Date], 1);
            });

            return chartData;
        }

        /// <summary>
        /// エラーメッセージ生成
        /// </summary>
        /// <param name="modelState">ステータス</param>
        /// <returns>エラーメッセージ</returns>
        public string GetErrorMessage(ModelStateDictionary modelState)
        {
            // メッセージリスト
            List<string> messageList = new List<string>();

            // エラーメッセージを追加
            foreach (ModelState state in modelState.Values)
            {
                foreach (ModelError error in state.Errors)
                {
                    messageList.Add(error.ErrorMessage);
                }
            }
            // 重複を削除
            messageList = messageList.Distinct().ToList();

            // brタグで文字列に連結
            return string.Join("<br>", messageList);
        }
    }
}