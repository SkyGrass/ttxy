using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZYSoft.LYYL.Core.DAPI
{
    /// <summary>
    /// 工艺查看 入参 实体类
    /// </summary>
    public class DAPI_MOProcess
    {
        /// <summary>
        /// 过滤条件 按工单0，按存货1
        /// </summary>
        public string FType { get; set; }
        /// <summary>
        /// 过滤值 
        /// </summary>
        public string FValue { get; set; }

        /// <summary>
        /// 登陆的用户名
        /// </summary>
        public string FUserID { get; set; }


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
    /// 工艺查看 出参 实体类
    /// </summary>
    public class DAPI_MOQueryProcess
    {
     
        /// <summary>
        /// 生产订单号
        /// </summary>
        public string MoCode { get; set; }
        
        /// <summary>
        /// 行号
        /// </summary>
        public string SortSeq { get; set; }
        /// <summary>
        /// 项目
        /// </summary>
        public string cProject { get; set; }

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
        /// 产品图片路径 【相对路径】
        /// </summary>
        public string cInvImgPath { get; set; }


        /// <summary>
        /// 明细 标准工艺路线
        /// </summary>
        public List<DAPI_MOQueryProcessRouting> MOQueryProcessRouting { get; set; }


        /// <summary>
        /// 明细 工艺文件
        /// </summary>
        public List<DAPI_MOQueryProcessFile> MOQueryProcessFile { get; set; }
    }

    /// <summary>
    /// 工艺查看 标准工艺路线
    /// </summary>
    public class DAPI_MOQueryProcessRouting
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



    }


    /// <summary>
    ///  工艺查看 工艺文件
    /// </summary>
    public class DAPI_MOQueryProcessFile
    {

        /// <summary>
        /// 工艺文件名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 工艺路径
        /// </summary>
        public string FilePath { get; set; }
    }

}
