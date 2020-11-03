using static DncZeus.Api.Entities.Enums.CommonEnum;

namespace DncZeus.Api.ViewModels.Base.DncManager
{
    /// <summary>
    /// 
    /// </summary>
    public class ManagerCreateViewModel
    {
        /// <summary>
        /// 
        /// </summary>
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
        public string TelNo { get; set; }
        /// <summary>
        ///  
        /// </summary>
        public string AppPwd { get; set; }
        /// <summary>
        ///  
        /// </summary>
        public int HospitalId { get; set; }
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
