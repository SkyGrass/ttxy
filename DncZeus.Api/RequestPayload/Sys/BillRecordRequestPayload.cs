using System;
using static DncZeus.Api.Entities.Enums.CommonEnum;

namespace DncZeus.Api.RequestPayload.Sys
{
    /// <summary>
    /// 
    /// </summary>
    public class BillRecordRequestPayload : RequestPayload
    {
        public string KeyWord { get; set; }
        public string FBeginDate { get; set; }
        public string FEndDate { get; set; }
        public int FHospitalID { get; set; }
        public int FAreaID { get; set; }
        public int FBedID { get; set; }
        public int FManagerID { get; set; }
        /// <summary> 
        ///  
        /// </summary>
        public YesOrNo IsCancelManageCost { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public YesOrNo IsClosed { get; set; }
    }
}
