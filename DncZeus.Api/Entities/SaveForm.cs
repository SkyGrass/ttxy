using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DncZeus.Api.Entities
{
    public class SaveForm
    { /// <summary>
      /// 
      /// </summary>
        public int configId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int hosId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<DncHosManageFee> managerFee { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public WorkerFee workerFee { get; set; }

        public class WorkerFee
        {
            /// <summary>
            /// 
            /// </summary>
            public List<DncWorkerFeeByCareProject> feeByProjectDataSource { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public List<DncWorkerFeeByCarePrice> feeByPriceDataSource { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public List<DncWorkerFeeByCareWay> feeByWayDataSource { get; set; }
        }
    }
}
