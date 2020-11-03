using static DncZeus.Api.Entities.Enums.CommonEnum;

namespace DncZeus.Api.ViewModels.Base.DncPc
{
    /// <summary>
    /// 
    /// </summary>
    public class PcCreateViewModel
    {
        public int Id { get; set; }
        /// <summary>
        /// 图标名称
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        ///
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string IDCardNo { get; set; }
        /// <summary>
        ///  
        /// </summary>
        public int AreaId { get; set; }
        /// <summary>
        ///  
        /// </summary>
        public int HospitalId { get; set; }
        /// <summary>
        ///  
        /// </summary>
        public int ManagerId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Status Status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IsDeleted IsDeleted { get; set; }
    }
}
