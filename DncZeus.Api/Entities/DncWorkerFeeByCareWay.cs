using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static DncZeus.Api.Entities.Enums.CommonEnum;

namespace DncZeus.Api.Entities
{
    /// <summary>
    /// 费用配置实体类
    /// </summary>
    public class DncWorkerFeeByCareWay
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int MainId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Column(TypeName = "int")]
        public int ItemId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ItemName { get; set; }
        public string SysParName_1 { get; set; }
        public string LinkSymbol_1 { get; set; }
        public string SysParName_2 { get; set; } 
        public string LinkSymbol_2 { get; set; }
        public decimal Ratio_1 { get; set; }
    }
}
