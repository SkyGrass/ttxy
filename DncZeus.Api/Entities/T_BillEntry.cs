using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DncZeus.Api.Entities
{
    public class T_BillEntry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long FEntryID { get; set; }
        public int FID { get; set; }
        public DateTime FDate { get; set; }
        public string FRecType { get; set; }
        public int FRecWayID { get; set; }
        public double FTotalRecSum { get; set; }
        public double FPlanRecSum { get; set; }
        public double FRecSum { get; set; }
        public int FPersonID { get; set; }
        public double FPrice { get; set; }
        public DateTime FBeginDate { get; set; }
        public string FBeginPeriod { get; set; }
        public DateTime? FEndDate { get; set; }
        public string FEndPeriod { get; set; }
        public int FCareWayID { get; set; }
        public int FCareTypeID { get; set; }
        public double FDay { get; set; }
        public bool FIsEnd { get; set; }
        public double FLastDay { get; set; }
    }
}
