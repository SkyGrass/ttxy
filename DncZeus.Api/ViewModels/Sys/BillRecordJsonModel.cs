using DncZeus.Api.Entities;
using System;
using System.Collections.Generic;
using static DncZeus.Api.Entities.Enums.CommonEnum;

namespace DncZeus.Api.ViewModels.Sys
{
    /// <summary>
    /// 
    /// </summary>
    public class BillRecordJsonModel
    {
        public long FID { get; set; }
        public string FOrderBillNo { get; set; }
        public string FDate { get; set; }
        public string FBeginDate { get; set; }
        public string FBeginPeriod { get; set; }
        public string FEndDate { get; set; }
        public string FEndPeriod { get; set; }
        public double FPlanDay { get; set; }
        public string FClient { get; set; }
        public string FClientTel { get; set; }
        public int FHospitalID { get; set; }
        public int FAreaID { get; set; }
        public int FBedID { get; set; }
        public int FManagerID { get; set; }
        public string FRemark { get; set; }
        public bool FIsCancelManageCost { get; set; }
        public bool FIsClosed { get; set; }
        public double FDay { get; set; }
        public double FRecSum { get; set; }
        public bool? FIsStop { get; set; }
        public DateTime? FStopDate { get; set; }
        public Guid? FStopBillerID { get; set; }
    }
}
