using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static DncZeus.Api.Entities.Enums.CommonEnum;

namespace DncZeus.Api.Entities
{
    /// <summary>
    /// 费用配置实体类
    /// </summary>
    public class DncWorkerFeeByCarePrice
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
        public int StartPoint { get; set; }
        public int EndPoint { get; set; }
        public decimal Fee { get; set; }
    }
}
