using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DncZeus.Api.Entities
{
    public class T_BillPrice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long FEntryID { get; set; }
        public int FID { get; set; }
        public DateTime FDate { get; set; }
        public double FPrice { get; set; }
        public DateTime? FEndDate { get; set; }
        public DateTime? FBeginDate { get; set; }
        public int FCareWayID { get; set; }

    }
}
