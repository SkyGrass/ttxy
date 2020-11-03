using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DncZeus.Api.Utils
{
    public class HttpHelper
    {
        public static bool Post(string url, HttpContent postData, ref string res_data)
        {
            res_data = string.Empty;
            try
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage response = client.PostAsync(url, postData).Result;
                res_data = response.Content.ReadAsStringAsync().Result;
                return response.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
