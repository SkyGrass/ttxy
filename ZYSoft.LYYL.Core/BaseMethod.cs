using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ZYSoft.LYYL.Core
{
    /// <summary>
    /// 基类 定义统一返回方法
    /// </summary>
    public class BaseMethod
    {
        /// <summary>
        /// 统一返回数据
        /// </summary>
        /// <param name="_state">success ; error ; exception</param>
        /// <param name="_msg"></param>
        /// <param name="_obj"></param>
        /// <returns></returns>
        public static string returnResult(string _state = "", string _msg = "", object _obj = null)
        {
            try
            {
                return JsonConvert.SerializeObject(new MethodResult()
                {
                    data = _obj,
                    msg = _msg,
                    state = _state
                });
            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(new MethodResult()
                {
                    data = null,
                    msg = e.Message,
                    state = "exception"
                });
            }

        }
    }
}
