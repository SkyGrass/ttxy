using static DncZeus.Api.Entities.Enums.CommonEnum;

namespace DncZeus.Api.RequestPayload.Base.Bed
{
    /// <summary>
    /// 图标请求参数实体
    /// </summary>
    public class  BedRequestPayload : RequestPayload
    {
        /// <summary>45
        /// 是否已被删除
        /// </summary>
        public IsDeleted IsDeleted { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public Status Status { get; set; }
    }
}
