using System;
using System.Collections.Generic;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using System.Reflection;
using System.Configuration;

namespace CZDJ.XHTY
{
    public class Configuration
    {
        public static string mConnectionString = string.Empty;
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public static string ConnectionString
        {
            get
            {
                if (mConnectionString == string.Empty)
                {
                    //无法动态读取信息
                    string AppStr = System.Configuration.ConfigurationManager.AppSettings["ConStr"];
                    if (string.IsNullOrEmpty(AppStr))
                    {
                        AppStr = "ConnectionString";
                    }

                    //ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

                    // 强制重新载入配置文件的ConnectionStrings配置节
                    ConfigurationManager.RefreshSection("connectionStrings");
                    mConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings[AppStr].ConnectionString;
                }
                return mConnectionString;
            }
            set
            {
                mConnectionString = value;
            }
        }
    }

    public class DBHelper
    {

        /// <summary>
        /// 执行SQL查询语句，用SqlDataAdapter对象填充数据集
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>返回DataTable对象</returns>
        public static DataTable ExecuteDataTable(string SQLString)
        {
            SqlConnection conn = null;
            SqlDataAdapter sqlAda = null;
            DataTable dt = null;

            try
            {
                conn = new SqlConnection(Configuration.ConnectionString);

                sqlAda = new SqlDataAdapter(SQLString, conn);
                dt = new DataTable();
                sqlAda.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {
                Log.SaveLog("ExecuteDataTable - " + SQLString + ";Err:" + ex.Message.ToString());
                return null;
            }
            finally
            {
                dt.Dispose();
                sqlAda.Dispose();
                conn.Dispose();
            }
        }

        /// <summary>
        /// 执行SQL查询语句，用SqlDataAdapter对象填充数据集
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>返回DataTable对象</returns>
        public static DataTable ExecuteDataTable(string SQLString,int Times)
        {
            SqlConnection conn = null;
            SqlDataAdapter sqlAda = null;
            DataTable dt = null;

            try
            {
                conn = new SqlConnection(Configuration.ConnectionString);

                sqlAda = new SqlDataAdapter(SQLString, conn);
                sqlAda.SelectCommand.CommandTimeout = Times;
                
                dt = new DataTable();
                sqlAda.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {
                Log.SaveLog("ExecuteDataTable - " + SQLString + ";Err:"  + ex.Message.ToString());
                return null;
            }
            finally
            {
                dt.Dispose();
                sqlAda.Dispose();
                conn.Dispose();
            }
        }

        /// <summary>
        /// 返回第一行第一列的值
        /// 可以用来判断某个值是否存在
        /// 也可以用来得到一个值
        /// </summary>
        /// <param name="SQL">SQL查询语句</param>
        public static string ExecuteScalar(string SQLString)
        {
            SqlConnection conn = null;
            SqlCommand cmd = null;
            object returnValue = null;

            try
            {
                using (conn = new SqlConnection(Configuration.ConnectionString))
                {
                    using (cmd = new SqlCommand(SQLString, conn))
                    {
                        conn.Open();

                        returnValue = cmd.ExecuteScalar();

                        if (returnValue != null)
                            return returnValue.ToString();
                        else
                            return "";
                    }
                }
            }
            catch
            {
                return "";
            }
            finally
            {
                returnValue = null;
                cmd.Dispose();
                conn.Dispose();
            }
        }


        /// <summary>
        /// 执行无返回查询语句
        /// </summary>
        /// <param name="SQLString">查询语句字符串</param>
        public static int ExecuteNonQuery(string SQLString)
        {
            SqlConnection conn = null;
            SqlCommand cmd = null;
            int iReturn = 0;

            try
            {
                using (conn = new SqlConnection(Configuration.ConnectionString))
                {
                    using (cmd = new SqlCommand(SQLString, conn))
                    {
                        conn.Open();

                        iReturn = cmd.ExecuteNonQuery();

                        return iReturn;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.SaveLog("ExecuteNonQuery  - " + SQLString + ";Err:"+ ex.Message.ToString());             
                return -1;
            }
            finally
            {
                cmd.Dispose();
                conn.Dispose();
            }
        }


        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">多条SQL语句</param>		
        public static int ExecuteSqlTran(List<String> SQLStringList)
        {
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlTransaction tx = null;
            string strsql = "";
            try
            {
                using (conn = new SqlConnection(Configuration.ConnectionString))
                {
                    conn.Open();
                    cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandTimeout = 360;
                    tx = conn.BeginTransaction();
                    cmd.Transaction = tx;
                    try
                    {
                        int count = 0;
                        for (int n = 0; n < SQLStringList.Count; n++)
                        {
                            strsql = SQLStringList[n];
                            if (strsql.Trim().Length > 1)
                            {
                                cmd.CommandText = strsql;
                                count += cmd.ExecuteNonQuery();
                            }
                        }
                        tx.Commit();
                        //WriteLog.WriteErrLog(1, "Done Success.DataConnect:[" + connectionString + "].SqlString:[" + strsql + "].");
                        return count;
                    }
                    catch (System.Data.SqlClient.SqlException ex)
                    {
                        tx.Rollback();
                        Log.SaveLog("ExecuteSqlTran "+  ex.Message.ToString());

                        for (int n = 0; n < SQLStringList.Count; n++)
                        {
                            Log.SaveLog("ExecuteSqlTran SQL:" + SQLStringList[n]);
                        }

                        return -1;
                    }
                }
            }
            catch
            {
                return -1;
            }
            finally
            {
                tx.Dispose();
                cmd.Dispose();
                conn.Dispose();
                //conn = null;
                //cmd = null;
                //tx = null;
            }
        }



        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <param name="Times">超时时间</param>
        /// <returns>超时时间</returns>
        public static DataSet ExecuteQuery(string SQLString, int Times)
        {
            DataSet ds = null;
            SqlDataAdapter command = null;

            using (SqlConnection connection = new SqlConnection(Configuration.ConnectionString))
            {
                ds = new DataSet();
                try
                {
                    connection.Open();
                    command = new SqlDataAdapter(SQLString, connection);
                    command.SelectCommand.CommandTimeout = Times;
                    command.Fill(ds, "ds");
                    return ds;
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    ds = null;
                    command.Dispose();
                    connection.Dispose();
                }
            }


        }

        /// <summary>
        /// 执行一条计算查询结果语句
        /// </summary>
        /// <param name="strSql"></param>
        /// <returns></returns>
        public static bool Exists(string strSql)
        {
            object obj = null;
            int cmdresult;
            try
            {
                obj = GetSingle(strSql);
                if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                {
                    cmdresult = 0;
                }
                else
                {
                    cmdresult = int.Parse(obj.ToString());
                }
                if (cmdresult == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch(Exception ei)
            {
                Log.SaveLog("Exists  - " + strSql + ";Err:" + ei.Message); 
                return false;
            }
            finally
            {
                obj = null;
            }
        }


        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public static object GetSingle(string SQLString)
        {
            object obj = null;

            using (SqlConnection connection = new SqlConnection(Configuration.ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (System.Data.SqlClient.SqlException e)
                    {
                        connection.Close();
                        throw new Exception(e.Message);
                    }
                    finally
                    {
                        cmd.Dispose();
                        obj = null;
                    }
                }
            }
        }


    }
}

