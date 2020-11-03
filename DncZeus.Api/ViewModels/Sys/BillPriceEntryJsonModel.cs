using DncZeus.Api.Entities;
using System;
using System.Collections.Generic;
using static DncZeus.Api.Entities.Enums.CommonEnum;

namespace DncZeus.Api.ViewModels.Sys
{
    /// <summary>
    /// 
    /// </summary>
    public class BillPriceEntryJsonModel
    {
        public long FEntryID { get; set; }
        public int FID { get; set; }
        public string FDate { get; set; }
        public double FPrice { get; set; }
        public string FBeginDate { get; set; }
        public string FEndDate { get; set; }
        public int FCareWayID { get; set; }
    }
}
