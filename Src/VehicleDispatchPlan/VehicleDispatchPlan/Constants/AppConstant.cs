/**
 * 共通定数
 *
 * @author t-murayama
 * @version 1.0
 * ----------------------------------
 * 2020/03/01 t-murayama 新規作成
 *
 */
namespace VehicleDispatchPlan.Constants
{
    /// <summary>
    /// 共通定数クラス
    /// </summary>
    public class AppConstant
    {
        /// <summary>区分_通学種別</summary>
        public const string DIV_ATTEND_TYPE = "01";
        /// <summary>ｺｰﾄﾞ_合宿</summary>
        public const string CD_ATTEND_TYPE_LODGING = "01";
        /// <summary>ｺｰﾄﾞ_通い</summary>
        public const string CD_ATTEND_TYPE_COMMUTING = "02";

        /// <summary>教習コースコード_MT</summary>
        public const string TRAINING_COURSE_CD_MT = "01";
        /// <summary>教習コースコード_AT</summary>
        public const string TRAINING_COURSE_CD_AT = "02";

        /// <summary>系統_総受入残数</summary>
        public const string SERIES_TOTAL_REM_AMT = "総受入残数";
        /// <summary>系統_合宿受入残数</summary>
        public const string SERIES_LODGING_REM_AMT = "合宿受入残数";
        /// <summary>系統_通学受入残数</summary>
        public const string SERIES_COMMUTING_REM_AMT = "通学受入残数";
        /// <summary>系統_総在籍数</summary>
        public const string SERIES_TOTAL_REG_AMT = "総在籍数";
        /// <summary>系統_合宿在籍数</summary>
        public const string SERIES_LODGING_REG_AMT = "合宿在籍数";
        /// <summary>系統_通学在籍数</summary>
        public const string SERIES_COMMUTING_REG_AMT = "通学在籍数";

        /// <summary>編集モード</summary>
        public enum EditMode
        {
            Edit = 1,
            Confirm = 2
        }
    }
}