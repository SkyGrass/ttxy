using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZYSoft.LYYL.Core.DAPI
{

  
    /// <summary>
    /// 报工记录查询  实体类
    /// </summary>
    public class DAPI_QueryMO
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
        /// 明细 工序\工步分录 
        /// </summary>
        public List<DAPI_QueryMOProcess> MOProcess { get; set; }
    }

    /// <summary>
    /// 工序计划 工序\工步分录 
    /// </summary>
    public class DAPI_QueryMOProcess
    {
        /// <summary>
        /// 工序行号
        /// </summary>
        public string SortSeq { get; set; }
        /// <summary>
        /// 工序名称
        /// </summary>
        public string ProcessName { get; set; }

        /// <summary>
        /// 工序描述
        /// </summary>
        public string ProcessDesc { get; set; }

        /// <summary>
        /// 是否工步
        /// </summary>
        public bool IsSetp { get; set; }



        /// <summary>
        /// 是否委外工序
        /// </summary>
        public bool IsSubFlag { get; set; }

        /// <summary>
        /// 委外工序已发料标记
        /// </summary>
        public bool IsSubIssueFlag { get; set; }

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
        ///工序汇报明细分录
        /// </summary>
        public List<DAPI_QueryMORptDetail> MORptDetail { get; set; }

    }


    /// <summary>
    /// 工序汇报明细分录 
    /// </summary>
    public class DAPI_QueryMORptDetail
    {

        /// <summary>
        /// 汇报时间
        /// </summary>
        public string RptDate { get; set; }

        /// <summary>
        /// 汇报人
        /// </summary>
        public string Maker { get; set; }
       
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
       

    }
}
