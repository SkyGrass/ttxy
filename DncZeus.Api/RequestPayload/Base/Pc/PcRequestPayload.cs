using static DncZeus.Api.Entities.Enums.CommonEnum;

namespace DncZeus.Api.RequestPayload.Base.Pc
{
    /// <summary>
    /// 图标请求参数实体
    /// </summary>
    public class PcRequestPayload : RequestPayload
    {
        /// <summary>45
        /// 是否已被删除
        /// </summary>
        public IsDeleted IsDeleted { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public Status Status { get; set; }

        public int HospitalId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int AreaId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int ManagerId { get; set; }
    }
}
