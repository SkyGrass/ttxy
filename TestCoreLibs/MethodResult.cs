using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCoreLibs
{
    public class MethodResult
    {
        /// <summary>
        /// success ; error ; exception
        /// </summary>
        public string state { get; set; }
        public string msg { get; set; }
        public object data { get; set; }
    }
}
