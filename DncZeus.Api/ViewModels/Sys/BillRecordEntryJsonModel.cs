using DncZeus.Api.Entities;
using System;
using System.Collections.Generic;
using static DncZeus.Api.Entities.Enums.CommonEnum;

namespace DncZeus.Api.ViewModels.Sys
{
    /// <summary>
    /// 
    /// </summary>
    public class BillRecordEntryJsonModel
    {
        public long FEntryID { get; set; }
        public int FID { get; set; }
        public string FDate { get; set; }
        public string FRecType { get; set; }
        public int FRecWayID { get; set; }
        public double FTotalRecSum { get; set; }
        public double FPlanRecSum { get; set; }
        public double FRecSum { get; set; }
        public int FPersonID { get; set; }
        public double FPrice { get; set; }
        public string FBeginDate { get; set; }
        public string FBeginPeriod { get; set; }
        public string FEndDate { get; set; }
        public string FEndPeriod { get; set; }
        public int FCareWayID { get; set; }
        public int FCareTypeID { get; set; }
        public double FDay { get; set; }
        public bool FIsEnd { get; set; }
        public double FLastDay { get; set; }
    }
}
