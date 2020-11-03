using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static DncZeus.Api.Entities.Enums.CommonEnum;

namespace DncZeus.Api.Entities
{
    /// <summary>
    /// 费用配置实体类
    /// </summary>
    public class DncHosManageFee
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
        public string LinkSymbol_1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal Ratio_1 { get; set; }
    }
}
