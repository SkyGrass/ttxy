using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZYSoft.LYYL.Core.DAPI
{
    /// <summary>
    /// 生产订单 入参 实体类
    /// </summary>
    public class DAPI_MO
    {
        /// <summary>
        /// 生产订单分录ID【扫描的条码值】
        /// </summary>
        public string FModID { get; set; }

        /// <summary>
        /// 登陆的用户名
        /// </summary>
        public string FUserID { get; set; }

        /// <summary>
        ///  用户对应的员工工号
        /// </summary>
        public String FEmpCode { get; set; }

        /// <summary>
        ///  图片存储路径 
        /// </summary>
        public String FPath { get; set; }


        /// <summary>
        ///  图片网络URL
        /// </summary>
        public String FUrl { get; set; }
    }

    /// <summary>
    /// 报工记录查询
    /// </summary>
    public class DAPI_MORptFilter
    {
        /// <summary>
        /// 查询条件
        /// </summary>
        public string FFilter { get; set; }

        /// <summary>
        /// 登陆的用户名
        /// </summary>
        public string FUserID { get; set; }

        /// <summary>
        /// 当前页
        /// </summary>
        public int FCurrentPage { get; set; }
        /// <summary>
        /// 页码
        /// </summary>
        public int FPageSize { get; set; }
    }

    /// <summary>
    /// 报工记录查询
    /// </summary>
    public class DAPI_DelMORpt
    {
        /// <summary>
        /// 记录ID
        /// </summary>
        public string FID { get; set; }

        /// <summary>
        /// 登陆的用户名
        /// </summary>
        public string FUserID { get; set; }

        /// <summary>
        /// 用户密码
        /// </summary>
        public String FUserPwd { get; set; }

        /// <summary>
        /// u8 帐套号 如001
        /// </summary>
        public string FAccountNo { get; set; }

    }

    /// <summary>
    /// 生产订单 工序计划 实体类
    /// </summary>
    public class DAPI_MORouting
    {
        /// <summary>
        /// 生产订单ID
        /// </summary>
        public string MoID { get; set; }
        /// <summary>
        /// 生产订单分录ID
        /// </summary>
        public string ModID { get; set; }
        /// <summary>
        /// 生产订单号
        /// </summary>
        public string MoCode { get; set; }
        /// <summary>
        /// 生产部门编码  【通过此编码判断：机加工车间 和 装配车间 】
        /// </summary>
        public string mDeptCode { get; set; }

        /// <summary>
        /// 行号
        /// </summary>
        public string SortSeq { get; set; }

        /// <summary>
        /// 产品编码
        /// </summary>
        public string cInvCode { get; set; }
        /// <summary>
        /// 产品名称
        /// </summary>
        public string cInvName { get; set; }
        /// <summary>
        /// 产品规格 
        /// </summary>
        public string cInvStd { get; set; }

        /// <summary>
        ///  计量单位
        /// </summary>
        public string cComUnitName { get; set; }

        /// <summary>
        /// 项目
        /// </summary>
        public string cProject { get; set; }
        /// <summary>
        /// 版本号
        /// </summary>
        public string cVersion { get; set; }
        /// <summary>
        /// 批号
        /// </summary>
        public string MoLotCode { get; set; }
        /// <summary>
        /// 开工时间
        /// </summary>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// 完工时间
        /// </summary>
        public DateTime DueDate { get; set; }
        /// <summary>
        /// 产品图片路径 【相对路径】
        /// </summary>
        public string cInvImgPath { get; set; }
        /// <summary>
        /// 生产订单数量
        /// </summary>
        public decimal iQty { get; set; }
        /// <summary>
        /// 工序计划ID
        /// </summary>
        public string MoRoutingId { get; set; }

        /// <summary>
        /// 明细
        /// </summary>
        public List<DAPI_MORoutingEntry> MORoutingEntry { get; set; }
    }

    /// <summary>
    /// 工序计划 工序分录 
    /// </summary>
    public class DAPI_MORoutingEntry
    {
        /// <summary>
        /// 工序行号
        /// </summary>
        public string SortSeq { get; set; }
        /// <summary>
        /// 工序计划分录ID
        /// </summary>
        public string MoRoutingDId { get; set; }
        /// <summary>
        /// 上道工序分录ID
        /// </summary>
        public string OutMoRoutingDId { get; set; }
        /// <summary>
        /// 工作中心ID
        /// </summary>
        public string WorkCenterID { get; set; }
        /// <summary>
        /// 工序ID
        /// </summary>
        public string ProcessID { get; set; }
        /// <summary>
        /// 工序名称
        /// </summary>
        public string ProcessName { get; set; }

        /// <summary>
        /// 工序描述
        /// </summary>
        public string ProcessDesc { get; set; }

        /// <summary>
        /// 工序是否要进行工序检验的标记
        /// </summary>
        public bool IsVerifyFlag { get; set; }

        /// <summary>
        /// 是否存在工步
        /// </summary>
        public bool IsExistSetp { get; set; }

        /// <summary>
        /// 是否首道工序
        /// </summary>
        public bool IsFirstFlag { get; set; }

        /// <summary>
        /// 是否末道工序
        /// </summary>
        public bool IsLastFlag { get; set; }

        /// <summary>
        /// 是否可超计划数量汇报
        /// </summary>
        public bool IsExceedFlag { get; set; }

        /// <summary>
        /// 是否倒冲工序
        /// </summary>
        public bool IsBFFlag { get; set; }


        /// <summary>
        /// 是否委外工序
        /// </summary>
        public bool IsSubFlag { get; set; }

        /// <summary>
        /// 委外工序已发料标记
        /// </summary>
        public bool IsSubIssueFlag { get; set; }

        /// <summary>
        /// 当前用户是否可操作的标记  【根据用户可操作的工序、工步来判断】
        /// </summary>
        public bool IsPermitRpt { get; set; }

        /// <summary>
        /// 可汇报数量
        /// </summary>
        public decimal iPlanQty { get; set; }
        /// <summary>
        /// 已汇报数量
        /// </summary>
        public decimal iRptQty { get; set; }

        /// <summary>
        /// 合格数量
        /// </summary>
        public decimal iGoodQty { get; set; }

        /// <summary>
        /// 不合格数量
        /// </summary>
        public decimal iBadQty { get; set; }

        /// <summary>
        /// 待确认数量 班组长未审核的数量(不作为已报工数量)
        /// </summary>
        public decimal iUnConfirmQty { get; set; }

        /// <summary>
        /// 待评审数量 质量未进行判定处理的数量(不作为已报工数量)
        /// </summary>
        public decimal iUnVerifyQty { get; set; }
        /// <summary>
        /// 未汇报数量   可报工数量-已报工数量-待审核数量-待评审数量
        /// </summary>
        public decimal iUnRptQty { get; set; }

        /// <summary>
        ///工序对应的工步
        /// </summary>
        public List<DAPI_MORoutingSetpEntry> Setp { get; set; }

    }


    /// <summary>
    /// 工序计划 工步分录 
    /// </summary>
    public class DAPI_MORoutingSetpEntry
    {

        /// <summary>
        /// 工序计划分录ID
        /// </summary>
        public string MoRoutingDId { get; set; }

        /// <summary>
        /// 工序行号
        /// </summary>
        public string cStepProcessSortSeq { get; set; }
        /// <summary>
        /// 工序ID
        /// </summary>
        public string cStepProcessID { get; set; }
        /// <summary>
        /// 工序名称
        /// </summary>
        public string cStepProcessName { get; set; }
        /// <summary>
        /// 工步行号
        /// </summary>
        public string cStepCode { get; set; }
        /// <summary>
        /// 工步名称
        /// </summary>
        public string cStepName { get; set; }

        /// <summary>
        /// 是否可超计划数量汇报
        /// </summary>
        public bool IsExceedFlag { get; set; }

    
        /// <summary>
        /// 当前用户是否可操作的标记  【根据用户可操作的工序、工步来判断】
        /// </summary>
        public bool IsPermitRpt { get; set; }

        /// <summary>
        /// 可汇报数量
        /// </summary>
        public decimal iPlanQty { get; set; }
        /// <summary>
        /// 已汇报数量
        /// </summary>
        public decimal iRptQty { get; set; }

        /// <summary>
        /// 合格数量
        /// </summary>
        public decimal iGoodQty { get; set; }

        /// <summary>
        /// 不合格数量
        /// </summary>
        public decimal iBadQty { get; set; }

        /// <summary>
        /// 待确认数量 班组长未审核的数量(不作为已报工数量)
        /// </summary>
        public decimal iUnConfirmQty { get; set; }

        /// <summary>
        /// 待评审数量 质量未进行判定处理的数量(不作为已报工数量)
        /// </summary>
        public decimal iUnVerifyQty { get; set; }
        /// <summary>
        /// 未汇报数量   可报工数量-已报工数量-待审核数量-待评审数量
        /// </summary>
        public decimal iUnRptQty { get; set; }


    }
}
