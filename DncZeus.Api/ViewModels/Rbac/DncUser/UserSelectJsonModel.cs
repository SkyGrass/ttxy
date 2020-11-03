using System;
using static DncZeus.Api.Entities.Enums.CommonEnum;

namespace DncZeus.Api.ViewModels.Rbac.DncUser
{
    /// <summary>
    /// 
    /// </summary>
    public class UserSelectJsonModel
    {
        /// <summary>
        /// 
        /// </summary>
        public Guid FGuid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string DisplayName { get; set; }

        public Guid Value
        {
            get
            {
                return FGuid;
            }
        }

        public string text
        {
            get
            {
                return DisplayName;
            }
        }
    }
}
