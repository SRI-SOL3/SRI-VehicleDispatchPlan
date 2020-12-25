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
        /// <summary>通学種別コード_合宿</summary>
        public const string ATTEND_TYPE_CD_LODGING = "01";
        /// <summary>通学種別コード_通い</summary>
        public const string ATTEND_TYPE_CD_COMMUTING = "02";

        /// <summary>教習コースコード_MT</summary>
        public const string TRAINING_COURSE_CD_MT = "01";
        /// <summary>教習コースコード_AT</summary>
        public const string TRAINING_COURSE_CD_AT = "02";

        /// <summary>性別_男性</summary>
        public const string GENDER_MALE = "M";
        /// <summary>性別_女性</summary>
        public const string GENDER_FEMALE = "F";

        /// <summary>コマンド_確認</summary>
        public const string CMD_CONFIRM = "確認";
        /// <summary>コマンド_更新</summary>
        public const string CMD_UPDATE = "更新";
        /// <summary>コマンド_登録</summary>
        public const string CMD_REGIST = "登録";
        /// <summary>コマンド_戻る</summary>
        public const string CMD_RETURN = "戻る";
        /// <summary>コマンド_追加</summary>
        public const string CMD_ADD = "追加";
        /// <summary>コマンド_削除</summary>
        public const string CMD_REMOVE = "削除";
        /// <summary>コマンド_仮免・卒業日設定</summary>
        public const string CMD_SET_TMP_GRD = "仮免・卒業日設定";
        /// <summary>コマンド_読込</summary>
        public const string CMD_READ = "読込";
        /// <summary>コマンド_検索</summary>
        public const string CMD_SEARCH = "検索";
        /// <summary>コマンド_再表示</summary>
        public const string CMD_REDISPLAY = "再表示";

        /// <summary>系統_総受入残人数</summary>
        public const string SERIES_TOTAL_REM_AMT = "総受入残人数";
        /// <summary>系統_合宿受入残人数</summary>
        public const string SERIES_LODGING_REM_AMT = "合宿受入残人数";
        /// <summary>系統_通学受入残人数</summary>
        public const string SERIES_COMMUTING_REM_AMT = "通学受入残人数";
        /// <summary>系統_受入可能数</summary>
        public const string SERIES_TOTAL_MAX_AMT = "総在籍可能数";
        /// <summary>系統_合宿受入可能数</summary>
        public const string SERIES_LODGING_MAX_AMT = "合宿在籍可能数";
        /// <summary>系統_通学受入可能数</summary>
        public const string SERIES_COMMUTING_MAX_AMT = "通学在籍可能数";
        /// <summary>系統_総在籍数</summary>
        public const string SERIES_TOTAL_REG_AMT = "総在籍数";
        /// <summary>系統_合宿在籍数</summary>
        public const string SERIES_LODGING_REG_AMT = "合宿在籍数";
        /// <summary>系統_通学在籍数</summary>
        public const string SERIES_COMMUTING_REG_AMT = "通学在籍数";

        /// <summary>TempDataキー_日別設定遷移有無</summary>
        public const string TEMP_KEY_IS_LINK = "IsLink";
        /// <summary>TempDataキー_日付From</summary>
        public const string TEMP_KEY_DATE_FROM = "DateFrom";
        /// <summary>TempDataキー_日付To</summary>
        public const string TEMP_KEY_DATE_TO = "DateTo";
        /// <summary>TempDataキー_検索日付</summary>
        public const string TEMP_KEY_SEARCH_DATE = "SearchDate";

        /// <summary>編集モード</summary>
        public enum EditMode
        {
            Edit = 1,
            Confirm = 2
        }
    }
}