using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZYSoft.LYYL.Core.DAPI
{

    /// <summary>
    /// 导出Excel请求数据包
    /// </summary>
    public class DAPI_Export
    {
        /// <summary>
        /// 网格配置
        /// </summary>
        public List<ExportColumn> column { get; set; }
        /// <summary>
        /// 请求类型
        /// </summary>
        public string reqType { get; set; }
        /// <summary>
        /// 请求条件
        /// </summary>
        public string reqStr { get; set; }

        public class ExportColumn
        {
            /// <summary>
            /// 列名
            /// </summary>
            public string label { get; set; }
            /// <summary>
            /// 字段名
            /// </summary>
            public string field { get; set; }
        }
    }
}
