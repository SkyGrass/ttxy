using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZYSoft.LYYL.Core.DAPI
{

    /// <summary>
    /// 工序报工 
    /// </summary>
    public class DAPI_MORpt
    {
        /// <summary>
        /// u8 帐套号 如001
        /// </summary>
        public string FAccountNo { get; set; }
        /// <summary>
        /// 登陆的用户名
        /// </summary>
        public string FUserID { get; set; }

        /// <summary>
        /// 用户密码
        /// </summary>
        public String FUserPwd { get; set; }

        /// <summary>
        ///  用户对应的员工工号
        /// </summary>
        public String FEmpCode { get; set; }
        
        /// <summary>
        /// 生产订单ID
        /// </summary>
        public string FMoID { get; set; }
        /// <summary>
        /// 生产订单分录ID
        /// </summary>
        public string FModID { get; set; }

        /// <summary>
        /// 工序计划ID
        /// </summary>
        public string FMoRoutingId { get; set; }

        /// <summary>
        /// 工序计划分录ID
        /// </summary>
        public string FMoRoutingDId { get; set; }

        /// <summary>
        /// 移出工序分录ID  上道工序ID
        /// </summary>
        public string FOutMoRoutingDid { get; set; }
        /// <summary>
        /// 工作中心ID
        /// </summary>
        public string FWorkCenterID { get; set; }
        /// <summary>
        /// 工序ID
        /// </summary>
        public string FProcessID { get; set; }


        /// <summary>
        /// 工序行号
        /// </summary>
        public string FOpSeq { get; set; }
        /// <summary>
        /// 工序名称
        /// </summary>
        public string FOpName { get; set; }

        /// <summary>
        /// 工步行号
        /// </summary>
        public string FStepCode { get; set; }

        /// <summary>
        /// 工步名称
        /// </summary>
        public string FStepName { get; set; }


        /// <summary>
        /// 是否首道工序
        /// </summary>
        public bool FIsFirstFlag { get; set; }

        /// <summary>
        /// 是否末道工序
        /// </summary>
        public bool FIsLastFlag { get; set; }

        /// <summary>
        /// 工序是否要进行工序检验的标记
        /// </summary>
        public bool FIsVerifyFlag { get; set; }

        /// <summary>
        /// 是否倒冲工序
        /// </summary>
        public bool FIsBFFlag { get; set; }


        /// <summary>
        /// 是否存在工步
        /// </summary>
        public bool FIsExistSetp { get; set; }

        /// <summary>
        /// 本次汇报超计划标记
        /// </summary>
        public bool FIsExceed { get; set; }

        /// <summary>
        /// 可汇报数量
        /// </summary>
        public decimal FPlanQty { get; set; }
        /// <summary>
        /// 本次汇报数量
        /// </summary>
        public decimal FRptQty { get; set; }

        /// <summary>
        /// 合格数量
        /// </summary>
        public decimal FGoodQty { get; set; }

        /// <summary>
        /// 不合格数量
        /// </summary>
        public decimal FBadQty { get; set; }

        /// <summary>
        /// 是否需要 班组长审核的标记
        /// </summary>
        public bool FIsNeedConfirm { get; set; }

        /// <summary>
        /// 待评审数量 质量未进行判定处理的数量
        /// </summary>
        public decimal FUnVerifyQty { get; set; }

        /// <summary>
        /// 超额原因
        /// </summary>
        public string FReason { get; set; }

        /// <summary>
        /// 末道的位置编码
        /// </summary>
        public string FPositionCode { get; set; }


        /// <summary>
        /// 班组长或质量要审核的记录ID
        /// </summary>
        public string FID { get; set; }

    }
}
