using System;
using static DncZeus.Api.Entities.Enums.CommonEnum;

namespace DncZeus.Api.ViewModels.Base.DncBed
{ 
    /// <summary>
    /// 
    /// </summary>
    public class BedSelectJsonModel
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        public int Value
        {
            get
            {
                return Id;
            }
        }

        public string text
        {
            get
            {
                return Name;
            }
        }
    }
}
