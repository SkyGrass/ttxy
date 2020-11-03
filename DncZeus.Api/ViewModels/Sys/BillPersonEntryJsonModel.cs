using DncZeus.Api.Entities;
using System;
using System.Collections.Generic;
using static DncZeus.Api.Entities.Enums.CommonEnum;

namespace DncZeus.Api.ViewModels.Sys
{
    /// <summary>
    /// 
    /// </summary>
    public class BillPersonEntryJsonModel
    {
        public long FEntryID { get; set; }
        public int FID { get; set; }
        public string FDate { get; set; }
        public int FPersonID { get; set; }
        public double FPrice { get; set; }
        public string FBeginDate { get; set; }
        public string FEndDate { get; set; }
        public string FBeginPeriod { get; set; }
        public string FEndPeriod { get; set; }
        public int FCareWayID { get; set; }
        public int FCareTypeID { get; set; }
    }
}
