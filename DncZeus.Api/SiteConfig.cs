using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DncZeus.Api
{
    public class SiteConfig
    {
        private static IConfigurationSection _appSection = null;

        /// <summary>
        /// api配置信息
        /// </summary>
        public static string AppSetting(string key)
        {
            string str = string.Empty;
            if (_appSection.GetSection(key) != null)
            {
                str = _appSection.GetSection(key).Value;
            }
            return str;
        }

        public static void SetAppSetting(IConfigurationSection section)
        {
            _appSection = section;
        }

        public static string GetSite(string apiName)
        {
            return AppSetting(apiName);
        }
    }
}
