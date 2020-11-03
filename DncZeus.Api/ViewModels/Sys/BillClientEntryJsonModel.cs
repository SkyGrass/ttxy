using DncZeus.Api.Entities;
using System;
using System.Collections.Generic;
using static DncZeus.Api.Entities.Enums.CommonEnum;

namespace DncZeus.Api.ViewModels.Sys
{
    /// <summary>
    /// 
    /// </summary>
    public class BillClientEntryJsonModel
    {
        public long FEntryID { get; set; }
        public int FID { get; set; }
        public int FHospitalID { get; set; }
        public int FAreaID { get; set; }
        public int FBedID { get; set; }
        public string FClient { get; set; }
        public string FDate { get; set; }
    }
}
