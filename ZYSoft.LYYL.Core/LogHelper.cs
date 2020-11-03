using System;
using System.IO;
using System.Text;
using System.Web;
using System.Configuration;
//using System.Windows.Forms;

namespace ZYSoft.LYYL.Core
{
    public static class LogHelper
    {
        /// <summary>
        /// 是否保存日志
        /// </summary>
        public static bool v_bol_SaveLog = true;

        private static string LogName = "";
        /// <summary>
        /// 获取或设置日志文件的名称
        /// </summary>
        public static string LogFileName
        {
            get
            {
                return LogName;
            }
            set
            {
                LogName = value;
            }
        }

        public static int FileSize
        {
            get { return 2048 * 1024; }
        }


        /// <summary>
        /// 获取或设置日志文件的路径
        /// </summary>
        public static string FileLogPath
        {
            get
            {
                return "E:/ZYSoft/Logs/";
               // return HttpContext.Current.Server.MapPath("~/Logs/");
            }
        }

        /// <summary>
        /// 向指定目录中的文件中追加日志文件,日志文件的名称将由传递的参数决定.
        /// </summary>
        /// <param name="LogType">0：错误日志 1：操作完成日志</param>
        /// <param name="LogFileName">日志文件的名称,如:mylog.txt ,如果没有自动创建,如果存在将追加写入日志</param>
        /// <param name="Message">要写入的内容</param>
        public static void WriteErrLog(int LogType, string Message)
        {
            if (!v_bol_SaveLog) return;

            string LogFileName = GetLogName(LogType);
            if (LogFileName.Equals("")) return;
            //DirectoryInfo path=new DirectoryInfo(LogFileName);
            //如果日志文件目录不存在,则创建
            if (!Directory.Exists(FileLogPath))
            {
                Directory.CreateDirectory(FileLogPath);
            }

            FileInfo finfo = new FileInfo(FileLogPath + LogFileName);
            if (finfo.Exists && finfo.Length > FileSize)
            {
                finfo.Delete();
            }
            try
            {
                FileStream fs = new FileStream(FileLogPath + LogFileName, FileMode.Append);
                StreamWriter strwriter = new StreamWriter(fs);
                try
                {

                    DateTime d = DateTime.Now;
                    strwriter.WriteLine("Date:" + d.ToString() + "  Msg:" + Message);
                    strwriter.Flush();
                }
                catch (Exception ee)
                {
                    Console.WriteLine("日志文件写入失败信息:" + ee.ToString());
                }
                finally
                {
                    strwriter.Close();
                    strwriter = null;
                    fs.Close();
                    fs = null;
                }
            }
            catch (Exception ee)
            {
                Console.WriteLine("日志文件没有打开,详细信息如下:");
            }

        }

        private static string GetLogName(int LogType)
        {
            switch (LogType)
            {
                case 0:
                    return "LogError" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                case 1:
                    return "LogSuccess" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                default:
                    return "";
            }
        }
    }
}