using static DncZeus.Api.Entities.Enums.CommonEnum;

namespace DncZeus.Api.RequestPayload.Base.Area
{
    /// <summary>
    /// 图标请求参数实体
    /// </summary>
    public class AreaRequestPayload : RequestPayload
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
    }
}
