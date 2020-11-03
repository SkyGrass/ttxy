using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DncZeus.Api.Models.Excel
{
    /// <summary>
    /// 导出Excel请求数据包
    /// </summary>
    public class ExcelMo
    {
        /// <summary>
        /// 网格配置
        /// </summary>
        public List<Column> columns { get; set; }
        /// <summary>
        /// 请求类型
        /// </summary>
        public string tyename { get; set; }
        /// <summary>
        /// 请求条件
        /// </summary>
        public string querstr { get; set; }
        public class Column
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
