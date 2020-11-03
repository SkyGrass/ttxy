using static DncZeus.Api.Entities.Enums.CommonEnum;

namespace DncZeus.Api.RequestPayload.Base.Bed
{
    /// <summary>
    /// 图标请求参数实体
    /// </summary>
    public class BedSelectRequestPayload
    {
        public int HospitalId { get; set; }
        public int AreaId { get; set; }
    }
}
