using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/**
 * エージェントモデル
 *
 * @author t-murayama
 * @version 1.0
 * ----------------------------------
 * 2020/03/01 t-murayama 新規作成
 *
 */
namespace VehicleDispatchPlan.Models
{
    /// <summary>
    /// エージェントモデル
    /// </summary>
    [Table("M_Agent")]
    public class M_Agent
    {
        /// <summary>エージェントコード</summary>
        [Key]
        [Required]
        public string AgentCd { get; set; }

        /// <summary>名前</summary>
        [Required]
        public string AgentName { get; set; }
    }
}