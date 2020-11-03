using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace DncZeus.Api
{
    /// <summary>
    /// 
    /// </summary>
    public class Program
    {
        /// <summary>
        /// 应用程序启动入口方法(Main)
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            //CreateWebHostBuilder(args).Build().Run();
            TryLoadAssembly();
            var host = CreateWebHostBuilder(args).Build();
            host.Run();
        }

        public static void TryLoadAssembly()
        {
            Assembly entry = Assembly.GetEntryAssembly();
            //找到当前执行文件所在路径
            string dir = Path.GetDirectoryName(entry.Location);
            string entryName = entry.GetName().Name;
            //获取执行文件同一目录下的其他dll
            foreach (string dll in Directory.GetFiles(dir, "*.dll"))
            {
                if (entryName.Equals(Path.GetFileNameWithoutExtension(dll))) { continue; }
                //非程序集类型的关联load时会报错
                try
                {
                    AssemblyLoadContext.Default.LoadFromAssemblyPath(dll);
                }
                catch (Exception ex)
                {
                }


            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel(c => c.AddServerHeader = false)
                .UseStartup<Startup>();
    }
}
