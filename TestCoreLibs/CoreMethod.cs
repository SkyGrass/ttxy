using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace TestCoreLibs
{
    /// <summary>
    /// 核心类
    /// 入参不限制
    /// 出参统一调用returnResult
    /// </summary>
    public class CoreMethod : BaseMethod
    {
        public static readonly CoreMethod instance = new CoreMethod();

        /// <summary>
        /// 连接串
        /// </summary>
        private string connStr { get; set; }
        public object v_str_FID { get; private set; }

        public CoreMethod() { }

        public CoreMethod(string _connStr)
        {
            this.connStr = _connStr;
        }

        /// <summary>
        /// 获取传递进来的数据库连接
        /// </summary>
        /// <returns></returns>
        public string getConnStr()
        {
            return returnResult("success", "success", this.connStr);
        }

        /// <summary>
        /// U8 Password Encrypt
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string U8Encrypt(string input)
        {
            string rethash = "";
            try
            {
                System.Security.Cryptography.SHA1 hash = System.Security.Cryptography.SHA1.Create();
                System.Text.ASCIIEncoding encoder = new System.Text.ASCIIEncoding();
                byte[] combined = encoder.GetBytes(input);
                hash.ComputeHash(combined);
                rethash = Convert.ToBase64String(hash.Hash);
            }
            catch (Exception ex)
            {
                return returnResult("", ex.Message, null);
            }
            //用友密码最后一位补位 
            return returnResult("success", "success", rethash + (char)3);
        }


        /// <summary>
        /// 根据工单号获取工序计划
        /// </summary>
        /// <param name="v_str_Json">传入工单号、登录用户</param>
        /// <returns></returns>
        public string GetMORounting(string v_str_Json)
        {
            try
            {
                string v_str_Result = "";

                //解析
                DAPI.DAPI_MO _DAPI_MO = JsonConvert.DeserializeObject<DAPI.DAPI_MO>(v_str_Json);

                ZYSoft.DB.Common.Configuration.ConnectionString = connStr;

                string v_str_sql = "";
                //根据工单分录ID，取出工单及产品信息
                v_str_sql = string.Format(@"select b.moid, b.modid, a.mocode,b.sortseq,b.invcode,c.cinvname,c.cinvstd,b.mdeptcode,b.define30 ,b.Qty  
                from mom_order a with(nolock) inner join mom_orderdetail b  with(nolock) on a.moid = b.moid
                inner join inventory c on b.invcode = c.cinvcode where b.status = 3 and b.modid = {0}
                ", _DAPI_MO.FModID);

                DataTable dt_MO = ZYSoft.DB.BLL.Common.ExecuteDataTable(v_str_sql);

                if (dt_MO == null)
                {
                    return returnResult("", "未获取到生产订单", null);
                }
                else if (dt_MO.Rows.Count == 0)
                {
                    return returnResult("", "未获取到生产订单记录", null);
                }

                //取出工单产品的图片（转存至临时表）

                v_str_sql = string.Format(@"select b.Picture,b.cPicturetype from inventory a 
                                inner join AA_Picture b on a.PictureGUID=b.cGUID  where a.cinvcode='{0}' AND b.Picture IS NOT NULL ",
                            dt_MO.Rows[0]["invcode"].ToString());

                DataTable dt_Pic = ZYSoft.DB.BLL.Common.ExecuteDataTable(v_str_sql);

                string v_str_FileName = "";
                if (dt_Pic == null)
                {
                    return returnResult("", "未获取到产品图片", null);
                }
                else if (dt_Pic.Rows.Count > 0)
                {
                    byte[] MyData = new byte[0];

                    MyData = (byte[])dt_Pic.Rows[0]["Picture"];//读取第一个图片的位流
                    int ArraySize = MyData.GetUpperBound(0);//获得数据库中存储的位流数组的维度上限，用作读取流的上限

                    string v_str_FilePath = "";
                    if (HttpContext.Current == null)
                    {
                        v_str_FilePath = Application.StartupPath + "\\Img\\";
                    }
                    else
                    {
                        v_str_FilePath = HttpContext.Current.Server.MapPath("~/Img/");
                    }

                    if (!Directory.Exists(v_str_FilePath))
                    {
                        Directory.CreateDirectory(v_str_FilePath);
                    }

                    v_str_FileName = v_str_FilePath + dt_MO.Rows[0]["invcode"].ToString() + "." + dt_Pic.Rows[0]["cPicturetype"].ToString();
                    FileStream fs = new FileStream(v_str_FileName, FileMode.OpenOrCreate, FileAccess.Write);
                    fs.Write(MyData, 0, ArraySize);
                    fs.Close();
                }

                //取出工序  根据人员对应的工序判断是否可操作
                v_str_sql = string.Format(@"SELECT row_number() over (order by opseq) as ino,  MoDId,MoRoutingId,t1.MoRoutingDId,OpSeq,t1.OperationId,Description,WcId,Remark,  FirstFlag,LastFlag,BFFlag, 
                    BalMachiningQty iPlanQty, BalQualifiedQty iGoodQty ,BalScrapQty iBadQty ,
                    ISNULL(T3.FRptQty,0) FRptQty, ISNULL(T3.FUnConfirmQty,0) FUnConfirmQty ,ISnull(T3.FUnVerifyQty,0) FUnVerifyQty,   t2.QtMethod ,
                    CASE WHEN t4.OperationId >0 THEN 1 ELSE 0 END FIsPermitRpt
                    FROM sfc_moroutingdetail t1 
                    LEFT OUTER JOIN sfc_moroutinginsp t2 ON t1.MoRoutingInspId=t2.MoRoutingInspId AND t1.MoRoutingDId=t2.MoRoutingDId
                    LEFT OUTER JOIN ZYSoft_LYYL_2019.dbo.t_MOReportQty T3 ON T1.MoRoutingDId=T3.FMoRoutingDId AND ISNULL(FStepCode,'')='' 
                    LEFT OUTER JOIN (select operationid,cpsncode,cpersonname from HM_LY_PsnRoutingLView
                    where ivouchstate=2 and cstepcode is null and cpsncode='{1}'
                    ) t4 on t1.operationid=t4.operationid  
                    where T1.MoDId= {0} ORDER BY opseq  ", _DAPI_MO.FModID, _DAPI_MO.FEmpCode);
                DataTable dt_Process = ZYSoft.DB.BLL.Common.ExecuteDataTable(v_str_sql);

                if (dt_Process == null)
                {
                    return returnResult("", "未获取到工序计划", null);
                }
                else if (dt_Process.Rows.Count == 0)
                {
                    return returnResult("", "未获取到工序计划记录", null);
                }

                //取出工步 根据人员对应的工序判断是否可操作
                v_str_sql = string.Format(@"SELECT MoRoutingDId,T1.OperationId,T2.cStepCode,cStepName, 
                    ISNULL(T3.FPlanQty,0) FPlanQty,ISNULL(T3.FGoodQty,0) FGoodQty,ISNULL(T3.FBadQty,0) FBadQty,
                    ISNULL(T3.FRptQty,0) FRptQty, ISNULL(T3.FUnConfirmQty,0) FUnConfirmQty ,ISnull(T3.FUnVerifyQty,0) FUnVerifyQty,
                    CASE WHEN t4.OperationId >0 AND T4.cstepcode IS NOT NULL  THEN 1 ELSE 0 END FIsPermitRpt
                    FROM [HM_LY_MoRountingStepMain] T1 JOIN [HM_LY_MoRountingStepSub] T2 ON T2.ID = T1.ID
                    LEFT OUTER JOIN ZYSoft_LYYL_2019.dbo.t_MOReportQty T3 ON T1.MoRoutingDId=T3.FMoRoutingDId AND ISNULL(FStepCode,'')=cStepCode
                    LEFT OUTER JOIN (select operationid,cstepcode from HM_LY_PsnRoutingLView
                    where ivouchstate=2 and cstepcode is NOT null and cpsncode='{1}'
                    ) t4 on t1.operationid=t4.operationid  AND T2.cstepcode=T4.cstepcode
                    WHERE T1.MoDId={0} ORDER BY T1.OperationId,t2.cStepCode       ", _DAPI_MO.FModID, _DAPI_MO.FEmpCode);
                DataTable dt_Setp = ZYSoft.DB.BLL.Common.ExecuteDataTable(v_str_sql);

                if (dt_Setp == null)
                {
                    return returnResult("", "未获取到工步", null);
                }
                else if (dt_Setp.Rows.Count == 0)
                {
                    return returnResult("", "未获取到工序计划记录", null);
                }

                DAPI.DAPI_MORouting _MORouting = new DAPI.DAPI_MORouting();

                _MORouting.MoID = dt_MO.Rows[0]["moid"].ToString();
                _MORouting.ModID = dt_MO.Rows[0]["modid"].ToString();
                _MORouting.MoCode = dt_MO.Rows[0]["mocode"].ToString();
                _MORouting.SortSeq = dt_MO.Rows[0]["sortseq"].ToString();

                //根据部门判定是 11机加工[有序报工]，还是 12装配调试[无序报工]
                _MORouting.mDeptCode = dt_MO.Rows[0]["mdeptcode"].ToString();

                _MORouting.cInvCode = dt_MO.Rows[0]["invcode"].ToString();
                _MORouting.cInvName = dt_MO.Rows[0]["cinvname"].ToString();
                _MORouting.cInvStd = dt_MO.Rows[0]["cinvstd"].ToString();

                _MORouting.cInvImgPath = v_str_FileName;

                _MORouting.iQty = decimal.Parse(dt_MO.Rows[0]["Qty"].ToString());

                List<DAPI.DAPI_MORoutingEntry> ls_MORoutingEntry = new List<DAPI.DAPI_MORoutingEntry>();

                foreach (DataRow dr in dt_Process.Rows)
                {
                    _MORouting.MoRoutingId = dr["MoRoutingId"].ToString();

                    DAPI.DAPI_MORoutingEntry _MORoutingEntry = new DAPI.DAPI_MORoutingEntry();

                    DataRow[] dr_Find = dt_Process.Select("ino = '" + (int.Parse(dr["ino"].ToString()) - 1).ToString() + "'");
                    if(dr_Find.Length ==0) //首道直接就是自己
                        _MORoutingEntry.OutMoRoutingDId = dr["MoRoutingDId"].ToString();
                    else
                        _MORoutingEntry.OutMoRoutingDId = dr_Find[0]["MoRoutingDId"].ToString();

                    _MORoutingEntry.MoRoutingDId = dr["MoRoutingDId"].ToString();
                    _MORoutingEntry.SortSeq = dr["OpSeq"].ToString();
                    _MORoutingEntry.WorkCenterID = dr["WcId"].ToString();
                    _MORoutingEntry.ProcessID = dr["OperationId"].ToString();
                    _MORoutingEntry.ProcessName = dr["Description"].ToString();
                    _MORoutingEntry.ProcessDesc = dr["Remark"].ToString();

                    _MORoutingEntry.IsFirstFlag = bool.Parse(dr["FirstFlag"].ToString());
                    _MORoutingEntry.IsLastFlag = bool.Parse(dr["LastFlag"].ToString());
                    _MORoutingEntry.IsBFFlag = bool.Parse(dr["BFFlag"].ToString());

                    //工序是否可超额
                    if (_MORoutingEntry.ProcessDesc.Equals("CNC") |
                        _MORoutingEntry.ProcessDesc.Equals("车床"))
                    {
                        _MORoutingEntry.IsExceedFlag = true;
                    }
                    else
                        _MORoutingEntry.IsExceedFlag = false;

                    //用户是否可操作
                    if (dr["FIsPermitRpt"].ToString().Equals("1"))
                    {
                        _MORoutingEntry.IsPermitRpt = true;
                    }
                    else
                        _MORoutingEntry.IsPermitRpt = false;

                    //如果工序有工步，则按工步最小的合格数量作为此工序的可汇报数量
                    DataRow[] dr_SetpS = dt_Setp.Select("OperationId= '" + _MORoutingEntry.ProcessID + "'");

                    if (dr_SetpS.Length > 0)
                    {
                        _MORoutingEntry.IsExistSetp = true;

                        //最出工步中最小的合格数量
                        _MORoutingEntry.iPlanQty = decimal.Parse(dt_Setp.Compute("Min(FGoodQty)", "OperationId = '" + _MORoutingEntry.ProcessID + "'").ToString());
                    }
                    else
                    {
                        _MORoutingEntry.IsExistSetp = false;
                        //这里可能存在BUG，需要取上道合格数量才行,
                        if (_MORoutingEntry.IsFirstFlag)
                        {
                            //首道取产品数量
                            _MORoutingEntry.iPlanQty = _MORouting.iQty;
                        }
                        else
                        {
                            //取上道工序的合格数量
                            _MORoutingEntry.iPlanQty = decimal.Parse(dr_Find[0]["iGoodQty"].ToString());
                            //_MORoutingEntry.iPlanQty = decimal.Parse(dr["iPlanQty"].ToString());
                        }
                    }

                    //如果是装配，则可汇报数量=订单数量
                    if (_MORouting.mDeptCode == "12")
                    {
                        _MORoutingEntry.iPlanQty = _MORouting.iQty;
                    }

                    _MORoutingEntry.iGoodQty = decimal.Parse(dr["iGoodQty"].ToString());
                    _MORoutingEntry.iBadQty = decimal.Parse(dr["iBadQty"].ToString());

                    _MORoutingEntry.iUnConfirmQty = decimal.Parse(dr["FUnConfirmQty"].ToString());
                    _MORoutingEntry.iUnVerifyQty = decimal.Parse(dr["FUnVerifyQty"].ToString());

                    _MORoutingEntry.iRptQty = decimal.Parse(dr["FRptQty"].ToString());

                    _MORoutingEntry.iUnRptQty = _MORoutingEntry.iPlanQty - _MORoutingEntry.iRptQty - _MORoutingEntry.iUnConfirmQty - _MORoutingEntry.iUnVerifyQty;

                    //处理工序对应的工步
                    List<DAPI.DAPI_MORoutingSetpEntry> ls_MORoutingSetpEntry = new List<DAPI.DAPI_MORoutingSetpEntry>();
                    foreach (DataRow dr_Setp in dr_SetpS)
                    {
                        DAPI.DAPI_MORoutingSetpEntry _MORoutingSetpEntry = new DAPI.DAPI_MORoutingSetpEntry();

                        _MORoutingSetpEntry.MoRoutingDId = _MORoutingEntry.MoRoutingDId;
                        _MORoutingSetpEntry.cStepCode = dr_Setp["cStepCode"].ToString();
                        _MORoutingSetpEntry.cStepName = dr_Setp["cStepName"].ToString();
                        _MORoutingSetpEntry.IsExceedFlag = false;
                        //用户是否可操作
                        if (dr_Setp["FIsPermitRpt"].ToString().Equals("1"))
                        {
                            _MORoutingSetpEntry.IsPermitRpt = true;
                        }
                        else
                            _MORoutingSetpEntry.IsPermitRpt = false;

                        _MORoutingSetpEntry.iPlanQty = decimal.Parse(dr_Setp["FPlanQty"].ToString());

                        //如果是装配，则可汇报数量=订单数量
                        if (_MORouting.mDeptCode.StartsWith("12"))
                        {
                            _MORoutingSetpEntry.iPlanQty = _MORouting.iQty;
                        }

                        _MORoutingSetpEntry.iGoodQty = decimal.Parse(dr_Setp["FGoodQty"].ToString());
                        _MORoutingSetpEntry.iBadQty = decimal.Parse(dr_Setp["FBadQty"].ToString());

                        _MORoutingSetpEntry.iUnConfirmQty = decimal.Parse(dr_Setp["FUnConfirmQty"].ToString());
                        _MORoutingSetpEntry.iUnVerifyQty = decimal.Parse(dr_Setp["FUnVerifyQty"].ToString());

                        _MORoutingSetpEntry.iRptQty = decimal.Parse(dr_Setp["FRptQty"].ToString());

                        _MORoutingSetpEntry.iUnRptQty = _MORoutingSetpEntry.iPlanQty - _MORoutingSetpEntry.iRptQty - _MORoutingSetpEntry.iUnConfirmQty - _MORoutingSetpEntry.iUnVerifyQty;

                        ls_MORoutingSetpEntry.Add(_MORoutingSetpEntry);
                    }
                    _MORoutingEntry.Setp = ls_MORoutingSetpEntry;

                    ls_MORoutingEntry.Add(_MORoutingEntry);
                }

                _MORouting.MORoutingEntry = ls_MORoutingEntry;

                //记录是否可汇报需要根据  IsPermitRpt=true  and iUnRptQty >0 
                v_str_Result = JsonConvert.SerializeObject(_MORouting);
                return returnResult("success", "success", v_str_Result);
            }
            catch (Exception ex)
            {
                return returnResult("", ex.Message, null);
            }

        }


        /// <summary>
        /// 保存报工记录
        /// </summary>
        /// <param name="v_str_Json"></param>
        /// <returns></returns>
        public string SaveMORpt(string v_str_Json)
        {
            try
            {            
                //先保存进记录表，再生成U8报工单

                string v_str_Result = "", v_str_ErrMsg="";

                //解析
                DAPI.DAPI_MORpt _DAPI_MORpt = JsonConvert.DeserializeObject<DAPI.DAPI_MORpt>(v_str_Json);

                ZYSoft.DB.Common.Configuration.ConnectionString = connStr;

                string v_str_sql = "", v_str_Guid = Guid.NewGuid().ToString();
                List<string> ls_Sql = new List<string>();

                v_str_sql = string.Format(@"INSERT INTO [ZYSoft_LYYL_2019].[dbo].[t_MOReportList]
                ([FGuid],[FMOID],[FMoDId],[FMoRoutingId],[FMoRoutingDId]
                ,[FOperationId],[FOpSeq],[FOpName],[FStepCode],[FStepName]
                ,[FDate],[FIsFirstFlag],[FIsLastFlag],[FIsExceed],[FMaker]
                ,[FPlanQty],[FRptQty],[FGoodQty],[FBadQty],[FUnVerifyQty]
                ,[FReason],[FIsNeedConfirm],[FIsNeedVerify],[FPositionCode]
                ,[FOutMoRoutingDid],[FWorkCenterID],[FEmpCode],[FIsBFFlag],[FIsVerifyFlag]) 
                SELECT '{0}','{1}','{2}','{3}','{4}',
                '{5}','{6}','{7}','{8}','{9}',
                GETDATE(),'{10}','{11}','{12}','{13}',
                '{14}','{15}','{16}','{17}','{18}',
                '{19}','{20}','{21}','{22}',
                '{23}','{24}','{25}','{26}','{27}'",
               v_str_Guid, _DAPI_MORpt.FMoID, _DAPI_MORpt.FModID, _DAPI_MORpt.FMoRoutingId, _DAPI_MORpt.FMoRoutingDId,
               _DAPI_MORpt.FProcessID, _DAPI_MORpt.FOpSeq, _DAPI_MORpt.FOpName, _DAPI_MORpt.FStepCode, _DAPI_MORpt.FStepName,
               _DAPI_MORpt.FIsFirstFlag, _DAPI_MORpt.FIsLastFlag, _DAPI_MORpt.FIsExceed, _DAPI_MORpt.FUserID,
               _DAPI_MORpt.FPlanQty, _DAPI_MORpt.FRptQty, _DAPI_MORpt.FGoodQty, _DAPI_MORpt.FBadQty, _DAPI_MORpt.FUnVerifyQty,
               _DAPI_MORpt.FReason, (_DAPI_MORpt.FIsExceed ? 1 : 0), (_DAPI_MORpt.FUnVerifyQty > 0 ? 1 : 0), _DAPI_MORpt.FPositionCode,
               _DAPI_MORpt.FOutMoRoutingDid, _DAPI_MORpt.FWorkCenterID,_DAPI_MORpt.FEmpCode, _DAPI_MORpt.FIsBFFlag, _DAPI_MORpt.FIsVerifyFlag);

                ls_Sql.Add(v_str_sql);
                //保存记录至数量汇总表

                v_str_sql = string.Format(@"
                IF NOT EXISTS(SELECT 1 FROM [ZYSoft_LYYL_2019].[dbo].[t_MOReportQty]
	                WHERE FMoID={0} AND FMoDId={1} AND FMoRoutingDId={3} AND FOperationId='{4}' AND FOpSeq='{5}' AND FStepCode='{6}')
                BEGIN
	                INSERT INTO [ZYSoft_LYYL_2019].[dbo].[t_MOReportQty]
	                ([FMOID],[FMoDId],[FMoRoutingId],[FMoRoutingDId],[FOperationId]
	                ,[FOpSeq],[FStepCode])
                    SELECT '{0}','{1}','{2}','{3}','{4}',
                     '{5}','{6}'
                END ", _DAPI_MORpt.FMoID, _DAPI_MORpt.FModID, _DAPI_MORpt.FMoRoutingId, _DAPI_MORpt.FMoRoutingDId,
               _DAPI_MORpt.FProcessID, _DAPI_MORpt.FOpSeq, _DAPI_MORpt.FStepCode);

                ls_Sql.Add(v_str_sql);

                if (ZYSoft.DB.BLL.Common.ExecuteSQLTran(ls_Sql, ref v_str_ErrMsg) !=-1)
                {
                    ls_Sql.Clear();

                    //调接口送入汇报单
                    //如果是工步则不需要调接口生单
                    //如果是超额则不需要调接口生单，后续将重先进行写入记录并生单

                    if (_DAPI_MORpt.FStepCode == "" && _DAPI_MORpt.FIsNeedConfirm == false)
                    {
                        //调用接口文件，生成U8单据
                        ZYSoft_LYYL_U8API_V125.clsFC _U8Interface = new ZYSoft_LYYL_U8API_V125.clsFC();


                        string v_str_U8BillID = "", v_str_U8BillNo = "";

                        if (_U8Interface.BuildU8Bills("574B7C58-3ED8-4D54-93B4-BDB5222174C2",
                                                _DAPI_MORpt.FAccountNo,
                                                DateTime.Now.Year.ToString(), //ConfigurationManager.AppSettings["AccountYear"],
                                                _DAPI_MORpt.FUserID,
                                                _DAPI_MORpt.FUserPwd,
                                                v_str_Guid, "FC91", 1, DateTime.Now.ToString("yyyy-MM-dd"), "ZYSoft_LYYL_2019",
                                                 ref v_str_U8BillID, ref v_str_U8BillNo, ref v_str_ErrMsg))
                        {
                            return returnResult("", "生成报工单失败," + v_str_ErrMsg, null);
                        }

                        //生单成功后将数量 写入 t_MOReportQty 表中

                        //取报工单记录
                        v_str_sql = string.Format(@"SELECT MID,cVouchCode FROM fc_MoRoutingBill WHERE define14 = '{0}' ", v_str_Guid);

                        DataTable dt_FC = new DataTable();
                        dt_FC = ZYSoft.DB.BLL.Common.ExecuteDataTable(v_str_sql);

                        if (dt_FC == null)
                        {
                            return returnResult("", "未获取到报工单", null);
                        }
                        else if (dt_FC.Rows.Count == 0)
                        {
                            return returnResult("", "未获取到生成的报工单记录", null);
                        }

                        //更新生单成功标记
                        v_str_sql = string.Format(@"UPDATE [ZYSoft_LYYL_2019].[dbo].[t_MOReportList] SET FIsBuildFC=1,
                        FFCBillID = '{0}', FFCBillNo = '{1}' WHERE FGuid = '{2}'",
                        dt_FC.Rows[0]["MID"].ToString(), dt_FC.Rows[0]["cVouchCode"].ToString(), v_str_Guid);
                        ls_Sql.Add(v_str_sql);
                    }

                    //List<string> ls_Sql = new List<string>();
     

                    //将报工数量、合格、不合格、待审核 数量 更新进汇总表
                    v_str_sql = string.Format(@"UPDATE T1 SET T1.FRptQty = ISNULL(T1.FRptQty, 0) + T2.FRptQty,
                    T1.FGoodQty = ISNULL(T1.FGoodQty, 0) + T2.FGoodQty,
                    T1.FBadQty = ISNULL(T1.FBadQty, 0) + T2.FBadQty,
                    T1.FUnVerifyQty = ISNULL(T1.FUnVerifyQty, 0) + T2.FUnVerifyQty
                    FROM ZYSoft_LYYL_2019.dbo.t_MOReportQty T1 JOIN ZYSoft_LYYL_2019.dbo.t_MOReportList t2 on t1.FMOID = T2.FMOID
                    AND T1.FMoDId = T2.FMoDId AND T1.FMoRoutingDId = T2.FMoRoutingDId AND T1.FOpSeq = T2.FOpSeq AND T1.FStepCode=T2.FStepCode
                    WHERE T2.FGuid = '{0}' AND FIsNeedConfirm=0 ", v_str_Guid);
                    ls_Sql.Add(v_str_sql);

                    //将待确认数量 更新进汇总表
                    v_str_sql = string.Format(@"UPDATE T1 SET T1.FUnConfirmQty = ISNULL(T1.FUnConfirmQty, 0) + T2.FRptQty
                    FROM ZYSoft_LYYL_2019.dbo.t_MOReportQty T1 JOIN ZYSoft_LYYL_2019.dbo.t_MOReportList t2 on t1.FMOID = T2.FMOID
                    AND T1.FMoDId = T2.FMoDId AND T1.FMoRoutingDId = T2.FMoRoutingDId AND T1.FOpSeq = T2.FOpSeq AND T1.FStepCode=T2.FStepCode
                    WHERE T2.FGuid = '{0}' AND FIsNeedConfirm=1 ", v_str_Guid);
                    ls_Sql.Add(v_str_sql);


                    if (ZYSoft.DB.BLL.Common.ExecuteSQLTran(ls_Sql, ref v_str_ErrMsg) > 0)
                    {
                        return returnResult("success", "success", v_str_Result);
                    }
                    else
                    {
                        return returnResult("", "更新报工数量失败," + v_str_ErrMsg, v_str_Result);
                    }
                }
                else
                {
                    return returnResult("", "保存报工记录失败," + v_str_ErrMsg, v_str_Result);
                }
            }
            catch (Exception ex)
            {
                return returnResult("", ex.Message, null);
            }
        }


        /// <summary>
        /// 获取需要班组长确认的记录
        /// </summary>
        /// <param name="v_str_Json"></param>
        /// <returns></returns>
        public string GetNeedConfirm(string v_str_Json)
        {

            try
            {
                //先保存进记录表，再生成U8报工单

                string v_str_Result = "";

                //解析
                DAPI.DAPI_MO _DAPI_MO = JsonConvert.DeserializeObject<DAPI.DAPI_MO>(v_str_Json);

                ZYSoft.DB.Common.Configuration.ConnectionString = connStr;

                //取出当前班组长需要确认的工序
                string v_str_sql = string.Format(@"SELECT T4.MoCode,T2.SortSeq, T3.cInvCode,T3.cInvName,t3.cInvStd,T2.MDeptCode,T2.Qty,T1.*
                                        FROM ZYSoft_LYYL_2019.dbo.t_MOReportList T1 JOIN mom_orderdetail T2 ON T1.FMoDId = T2.MoDId AND T1.FMOID = T2.MoId
                                        LEFT JOIN dbo.Inventory T3 ON T2.InvCode = T3.cInvCode
                                        LEFT JOIN mom_order T4 ON T4.MoId = T2.MoId
                                        LEFT JOIN (select operationid, cstepcode, cpsncode, cpersonname from HM_LY_PsnRoutingLView
                                                           where ivouchstate= 2 and cstepcode is null and cpsncode = '{1}'
                                                            ) t5 on t1.FOperationId = t5.operationid
                                         WHERE FIsNeedConfirm = 1 AND FIsFinishConfirm = 0 ", _DAPI_MO.FUserID);

                DataTable dt_Record = ZYSoft.DB.BLL.Common.ExecuteDataTable(v_str_sql);

                if (dt_Record == null)
                {
                    return returnResult("", "未获取到要班组长需要确认的记录", null);
                }
                else if (dt_Record.Rows.Count > 0)
                {
                    v_str_Result = JsonConvert.SerializeObject(dt_Record);
                }

                return returnResult("success", "success", v_str_Result);
            }
            catch (Exception ex)
            {
                return returnResult("", ex.Message, null);
            }
        }


        /// <summary>
        /// 获取需要质量检验的记录
        /// </summary>
        /// <param name="v_str_Json"></param>
        /// <returns></returns>
        public string GetNeedVerify(string v_str_Json)
        {

            try
            {
                //先保存进记录表，再生成U8报工单

                string v_str_Result = "";

                //解析
                DAPI.DAPI_MO _DAPI_MO = JsonConvert.DeserializeObject<DAPI.DAPI_MO>(v_str_Json);

                ZYSoft.DB.Common.Configuration.ConnectionString = connStr;

                //取出当前班组长需要确认的工序
                string v_str_sql = string.Format(@"SELECT T4.MoCode,T2.SortSeq, T3.cInvCode,T3.cInvName,t3.cInvStd,T2.MDeptCode,T2.Qty,T1.*
                                        FROM ZYSoft_LYYL_2019.dbo.t_MOReportList T1 JOIN mom_orderdetail T2 ON T1.FMoDId = T2.MoDId AND T1.FMOID = T2.MoId
                                        LEFT JOIN dbo.Inventory T3 ON T2.InvCode = T3.cInvCode
                                        LEFT JOIN mom_order T4 ON T4.MoId = T2.MoId
                                        LEFT JOIN (select operationid, cstepcode, cpsncode, cpersonname from HM_LY_PsnRoutingLView
                                                           where ivouchstate= 2 and cstepcode is null and cpsncode = '{1}'
                                                            ) t5 on t1.FOperationId = t5.operationid
                                         WHERE FIsNeedVerify=1 AND FIsFinishVerify=0  ", _DAPI_MO.FUserID);

                DataTable dt_Record = ZYSoft.DB.BLL.Common.ExecuteDataTable(v_str_sql);

                if (dt_Record == null)
                {
                    return returnResult("", "未获取到要质量检验的记录", null);
                }
                else if (dt_Record.Rows.Count > 0)
                {
                    v_str_Result = JsonConvert.SerializeObject(dt_Record);
                }

                return returnResult("success", "success", v_str_Result);
            }
            catch (Exception ex)
            {
                return returnResult("", ex.Message, null);
            }
        }


        /// <summary>
        /// 查询记录
        /// </summary>
        /// <param name="v_str_Json"></param>
        /// <returns></returns>
        public string GetMORpt(string v_str_Json)
        {

            try
            {
                //查询报工记录

                string v_str_Result = "";

                //解析
                DAPI.DAPI_MORptFilter _DAPI_MORptFilter = JsonConvert.DeserializeObject<DAPI.DAPI_MORptFilter>(v_str_Json);

                ZYSoft.DB.Common.Configuration.ConnectionString = connStr;

                //取出当前班组长需要确认的工序
                string v_str_sql = string.Format(@"SELECT T4.MoCode,T2.SortSeq, T3.cInvCode,T3.cInvName,t3.cInvStd,T2.MDeptCode,T2.Qty,T1.*
                                        FROM ZYSoft_LYYL_2019.dbo.t_MOReportList T1 JOIN mom_orderdetail T2 ON T1.FMoDId = T2.MoDId AND T1.FMOID = T2.MoId
                                        LEFT JOIN dbo.Inventory T3 ON T2.InvCode = T3.cInvCode
                                        LEFT JOIN mom_order T4 ON T4.MoId = T2.MoId
                                         WHERE {0}  ", _DAPI_MORptFilter.FFilter);

                DataTable dt_Record = ZYSoft.DB.BLL.Common.ExecuteDataTable(v_str_sql);

                if (dt_Record == null)
                {
                    return returnResult("", "未获取到报工的记录", null);
                }
                else if (dt_Record.Rows.Count > 0)
                {
                    v_str_Result = JsonConvert.SerializeObject(dt_Record);
                }

                return returnResult("success", "success", v_str_Result);
            }
            catch (Exception ex)
            {
                return returnResult("", ex.Message, null);
            }
        }

        /// <summary>
        /// 保存班组长确认记录
        /// </summary>
        /// <param name="v_str_Json"></param>
        /// <returns></returns>
        public string SaveMOConfirmRpt(string v_str_Json)
        {
            try
            {
                //先保存进记录表，更新原记录标记，再生成U8报工单

                string v_str_Result = "", v_str_ErrMsg = "";

                //解析
                DAPI.DAPI_MORpt _DAPI_MORpt = JsonConvert.DeserializeObject<DAPI.DAPI_MORpt>(v_str_Json);

                ZYSoft.DB.Common.Configuration.ConnectionString = connStr;

                string v_str_sql = "", v_str_Guid = Guid.NewGuid().ToString();



                v_str_sql = string.Format(@"INSERT INTO [ZYSoft_LYYL_2019].[dbo].[t_MOReportList]
                ([FGuid],[FMOID],[FMoDId],[FMoRoutingId],[FMoRoutingDId]
                ,[FOperationId],[FOpSeq],[FOpName],[FStepCode],[FStepName]
                ,[FDate],[FIsFirstFlag],[FIsLastFlag],[FIsExceed],[FMaker]
                ,[FPlanQty],[FRptQty],[FGoodQty],[FBadQty],[FUnVerifyQty]
                ,[FReason],[FIsNeedConfirm],[FIsNeedVerify],[FPositionCode]
                ,[FOutMoRoutingDid],[FWorkCenterID],[FEmpCode],[FIsBFFlag],[FIsVerifyFlag]) 
                SELECT '{0}','{1}','{2}','{3}','{4}',
                '{5}','{6}','{7}','{8}','{9}',
                GETDATE(),'{10}','{11}','{12}','{13}',
                '{14}','{15}','{16}','{17}','{18}',
                '{19}','{20}','{21}','{22}',
                '{23}','{24}','{25}','{26}','{27}'",
               v_str_Guid, _DAPI_MORpt.FMoID, _DAPI_MORpt.FModID, _DAPI_MORpt.FMoRoutingId, _DAPI_MORpt.FMoRoutingDId,
               _DAPI_MORpt.FProcessID, _DAPI_MORpt.FOpSeq, _DAPI_MORpt.FOpName, _DAPI_MORpt.FStepCode, _DAPI_MORpt.FStepName,
               _DAPI_MORpt.FIsFirstFlag, _DAPI_MORpt.FIsLastFlag, _DAPI_MORpt.FIsExceed, _DAPI_MORpt.FUserID,
               _DAPI_MORpt.FPlanQty, _DAPI_MORpt.FRptQty, _DAPI_MORpt.FGoodQty, _DAPI_MORpt.FBadQty, _DAPI_MORpt.FUnVerifyQty,
               _DAPI_MORpt.FReason, (_DAPI_MORpt.FIsExceed ? 1 : 0), (_DAPI_MORpt.FUnVerifyQty > 0 ? 1 : 0), _DAPI_MORpt.FPositionCode,
                _DAPI_MORpt.FOutMoRoutingDid, _DAPI_MORpt.FWorkCenterID, _DAPI_MORpt.FEmpCode, _DAPI_MORpt.FIsBFFlag, _DAPI_MORpt.FIsVerifyFlag);

                if (ZYSoft.DB.BLL.Common.ExecuteNonQuery(v_str_sql,ref v_str_ErrMsg) > 0)
                {
                    //调接口送入汇报单
                    //如果是工步则不需要调接口生单
                    //如果是超额则不需要调接口生单，后续将重先进行写入记录并生单
                    //调用接口文件，生成U8单据
                    ZYSoft_LYYL_U8API_V125.clsFC _U8Interface = new ZYSoft_LYYL_U8API_V125.clsFC();


                    string v_str_U8BillID = "", v_str_U8BillNo = "";

                    if (_U8Interface.BuildU8Bills("574B7C58-3ED8-4D54-93B4-BDB5222174C2",
                                            _DAPI_MORpt.FAccountNo,
                                            DateTime.Now.Year.ToString(), //ConfigurationManager.AppSettings["AccountYear"],
                                            _DAPI_MORpt.FUserID,
                                            _DAPI_MORpt.FUserPwd,
                                            v_str_Guid, "FC91", 1, DateTime.Now.ToString("yyyy-MM-dd"), "ZYSoft_LYYL_2019",
                                             ref v_str_U8BillID, ref v_str_U8BillNo, ref v_str_ErrMsg))
                    {
                        return returnResult("", "生成报工单失败," + v_str_ErrMsg, null);
                    }


                    //生单成功后将数量 写入 t_MOReportQty 表中

                    //取报工单记录
                    v_str_sql = string.Format(@"SELECT MID,cVouchCode FROM fc_MoRoutingBill WHERE define14 = '{0}' ", v_str_Guid);

                    DataTable dt_FC = new DataTable();
                    dt_FC = ZYSoft.DB.BLL.Common.ExecuteDataTable(v_str_sql);

                    if (dt_FC == null)
                    {
                        return returnResult("", "未获取到报工单", null);
                    }
                    else if (dt_FC.Rows.Count == 0)
                    {
                        return returnResult("", "未获取到生成的报工单记录", null);
                    }


                    List<string> ls_Sql = new List<string>();

                    //更新记录标记
                    v_str_sql = string.Format(@"UPDATE [ZYSoft_LYYL_2019].[dbo].[t_MOReportList] SET FIsFinishConfirm=1,
                    FConfirmer = '{0}', FConfirmDate = GETDATE() WHERE FID = {1}",
                    _DAPI_MORpt.FUserID, _DAPI_MORpt.FID);
                    ls_Sql.Add(v_str_sql);
                    //更新生单成功标记
                    v_str_sql = string.Format(@"UPDATE [ZYSoft_LYYL_2019].[dbo].[t_MOReportList] SET FIsBuildFC=1,
                    FFCBillID = '{0}', FFCBillNo = '{1}' WHERE FGuid = '{2}'",
                    dt_FC.Rows[0]["MID"].ToString(), dt_FC.Rows[0]["cVouchCode"].ToString(), v_str_Guid);
                    ls_Sql.Add(v_str_sql);

                    //更新数量，先将未确认的减去
                    v_str_sql = string.Format(@"UPDATE T1 SET T1.FUnConfirmQty = ISNULL(T1.FUnConfirmQty, 0) - T2.FRptQty
                    FROM ZYSoft_LYYL_2019.dbo.t_MOReportQty T1 JOIN ZYSoft_LYYL_2019.dbo.t_MOReportList t2 on t1.FMOID = T2.FMOID
                    AND T1.FMoDId = T2.FMoDId AND T1.FMoRoutingDId = T2.FMoRoutingDId AND T1.FOpSeq = T2.FOpSeq
                    WHERE T2.FID = {0} ",_DAPI_MORpt.FID);
                    ls_Sql.Add(v_str_sql);

                    //将报工数量、合格、不合格 数量 更新进汇总表
                    v_str_sql = string.Format(@"UPDATE T1 SET T1.FRptQty = ISNULL(T1.FRptQty, 0) + T2.FRptQty,
                    T1.FGoodQty = ISNULL(T1.FGoodQty, 0) + T2.FGoodQty,
                    T1.FBadQty = ISNULL(T1.FBadQty, 0) + T2.FBadQty
                    FROM ZYSoft_LYYL_2019.dbo.t_MOReportQty T1 JOIN ZYSoft_LYYL_2019.dbo.t_MOReportList t2 on t1.FMOID = T2.FMOID
                    AND T1.FMoDId = T2.FMoDId AND T1.FMoRoutingDId = T2.FMoRoutingDId AND T1.FOpSeq = T2.FOpSeq
                    WHERE T2.FGuid = '{0}'", v_str_Guid);
                    ls_Sql.Add(v_str_sql);


                    if (ZYSoft.DB.BLL.Common.ExecuteSQLTran(ls_Sql,ref v_str_ErrMsg) > 0)
                    {
                        return returnResult("success", "success", v_str_Result);
                    }
                    else
                    {
                        return returnResult("", "更新报工数量失败," + v_str_ErrMsg, v_str_Result);
                    }
                }
                else
                {
                    return returnResult("", "保存报工记录失败," + v_str_ErrMsg, v_str_Result);
                }                    
            }
            catch (Exception ex)
            {
                return returnResult("", ex.Message, null);
            }
        }


        /// <summary>
        /// 保存质量检验的记录
        /// </summary>
        /// <param name="v_str_Json"></param>
        /// <returns></returns>
        public string SaveMOVerifyRpt(string v_str_Json)
        {
            try
            {
                //先保存进记录表，更新原记录标记，再生成U8报工单

                string v_str_Result = "", v_str_ErrMsg = "";

                //解析
                DAPI.DAPI_MORpt _DAPI_MORpt = JsonConvert.DeserializeObject<DAPI.DAPI_MORpt>(v_str_Json);

                ZYSoft.DB.Common.Configuration.ConnectionString = connStr;

                string v_str_sql = "", v_str_Guid = Guid.NewGuid().ToString();

                v_str_sql = string.Format(@"INSERT INTO [ZYSoft_LYYL_2019].[dbo].[t_MOReportList]
                ([FGuid],[FMOID],[FMoDId],[FMoRoutingId],[FMoRoutingDId]
                ,[FOperationId],[FOpSeq],[FOpName],[FStepCode],[FStepName]
                ,[FDate],[FIsFirstFlag],[FIsLastFlag],[FIsExceed],[FMaker]
                ,[FPlanQty],[FRptQty],[FGoodQty],[FBadQty],[FUnVerifyQty]
                ,[FReason],[FIsNeedConfirm],[FIsNeedVerify],[FPositionCode]
                ,[FOutMoRoutingDid],[FWorkCenterID],[FEmpCode],[FIsBFFlag],[FIsVerifyFlag]) 
                SELECT '{0}','{1}','{2}','{3}','{4}',
                '{5}','{6}','{7}','{8}','{9}',
                GETDATE(),'{10}','{11}','{12}','{13}',
                '{14}','{15}','{16}','{17}','{18}',
                '{19}','{20}','{21}','{22}',
                '{23}','{24}','{25}','{26}','{27}'",
               v_str_Guid, _DAPI_MORpt.FMoID, _DAPI_MORpt.FModID, _DAPI_MORpt.FMoRoutingId, _DAPI_MORpt.FMoRoutingDId,
               _DAPI_MORpt.FProcessID, _DAPI_MORpt.FOpSeq, _DAPI_MORpt.FOpName, _DAPI_MORpt.FStepCode, _DAPI_MORpt.FStepName,
               _DAPI_MORpt.FIsFirstFlag, _DAPI_MORpt.FIsLastFlag, _DAPI_MORpt.FIsExceed, _DAPI_MORpt.FUserID,
               _DAPI_MORpt.FPlanQty, _DAPI_MORpt.FRptQty, _DAPI_MORpt.FGoodQty, _DAPI_MORpt.FBadQty, _DAPI_MORpt.FUnVerifyQty,
               _DAPI_MORpt.FReason, (_DAPI_MORpt.FIsExceed ? 1 : 0), (_DAPI_MORpt.FUnVerifyQty > 0 ? 1 : 0), _DAPI_MORpt.FPositionCode,
               _DAPI_MORpt.FOutMoRoutingDid, _DAPI_MORpt.FWorkCenterID,_DAPI_MORpt.FEmpCode, _DAPI_MORpt.FIsBFFlag, _DAPI_MORpt.FIsVerifyFlag);

                if (ZYSoft.DB.BLL.Common.ExecuteNonQuery(v_str_sql, ref v_str_ErrMsg) > 0)
                {
                    //调接口送入汇报单
                    //如果是工步则不需要调接口生单
                    //如果是超额则不需要调接口生单，后续将重先进行写入记录并生单

                    //调用接口文件，生成U8单据
                    ZYSoft_LYYL_U8API_V125.clsFC _U8Interface = new ZYSoft_LYYL_U8API_V125.clsFC();


                    string v_str_U8BillID = "", v_str_U8BillNo = "";

                    if (_U8Interface.BuildU8Bills("574B7C58-3ED8-4D54-93B4-BDB5222174C2",
                                            _DAPI_MORpt.FAccountNo,
                                            DateTime.Now.Year.ToString(), //ConfigurationManager.AppSettings["AccountYear"],
                                            _DAPI_MORpt.FUserID,
                                            _DAPI_MORpt.FUserPwd,
                                            v_str_Guid, "FC91", 1, DateTime.Now.ToString("yyyy-MM-dd"), "ZYSoft_LYYL_2019",
                                             ref v_str_U8BillID, ref v_str_U8BillNo, ref v_str_ErrMsg))
                    {
                        return returnResult("", "生成报工单失败," + v_str_ErrMsg, null);
                    }

                    //生单成功后将数量 写入 t_MOReportQty 表中

                    //取报工单记录
                    v_str_sql = string.Format(@"SELECT MID,cVouchCode FROM fc_MoRoutingBill WHERE define14 = '{0}' ", v_str_Guid);

                    DataTable dt_FC = new DataTable();
                    dt_FC = ZYSoft.DB.BLL.Common.ExecuteDataTable(v_str_sql);

                    if (dt_FC == null)
                    {
                        return returnResult("", "未获取到报工单", null);
                    }
                    else if (dt_FC.Rows.Count == 0)
                    {
                        return returnResult("", "未获取到生成的报工单记录", null);
                    }


                    List<string> ls_Sql = new List<string>();

                    //更新记录标记
                    v_str_sql = string.Format(@"UPDATE [ZYSoft_LYYL_2019].[dbo].[t_MOReportList] SET FIsFinishVerify=1,
                    FVerifyer = '{0}', FVerifyDate = GETDATE() WHERE FID = {1}",
                    _DAPI_MORpt.FUserID, _DAPI_MORpt.FID);
                    ls_Sql.Add(v_str_sql);
                    //更新生单成功标记
                    v_str_sql = string.Format(@"UPDATE [ZYSoft_LYYL_2019].[dbo].[t_MOReportList] SET FIsBuildFC=1,
                    FFCBillID = '{0}', FFCBillNo = '{1}' WHERE FGuid = '{2}'",
                    dt_FC.Rows[0]["MID"].ToString(), dt_FC.Rows[0]["cVouchCode"].ToString(), v_str_Guid);
                    ls_Sql.Add(v_str_sql);

                    //更新数量，先将未审核的减去
                    v_str_sql = string.Format(@"UPDATE T1 SET T1.FUnVerifyQty = ISNULL(T1.FUnVerifyQty, 0) - T2.FRptQty
                    FROM ZYSoft_LYYL_2019.dbo.t_MOReportQty T1 JOIN ZYSoft_LYYL_2019.dbo.t_MOReportList t2 on t1.FMOID = T2.FMOID
                    AND T1.FMoDId = T2.FMoDId AND T1.FMoRoutingDId = T2.FMoRoutingDId AND T1.FOpSeq = T2.FOpSeq
                    WHERE T2.FID = {0} ", _DAPI_MORpt.FID);
                    ls_Sql.Add(v_str_sql);

                    //将报工数量、合格、不合格 数量 更新进汇总表
                    v_str_sql = string.Format(@"UPDATE T1 SET T1.FRptQty = ISNULL(T1.FRptQty, 0) + T2.FRptQty,
                    T1.FGoodQty = ISNULL(T1.FGoodQty, 0) + T2.FGoodQty,
                    T1.FBadQty = ISNULL(T1.FBadQty, 0) + T2.FBadQty
                    FROM ZYSoft_LYYL_2019.dbo.t_MOReportQty T1 JOIN ZYSoft_LYYL_2019.dbo.t_MOReportList t2 on t1.FMOID = T2.FMOID
                    AND T1.FMoDId = T2.FMoDId AND T1.FMoRoutingDId = T2.FMoRoutingDId AND T1.FOpSeq = T2.FOpSeq
                    WHERE T2.FGuid = '{0}'", v_str_Guid);
                    ls_Sql.Add(v_str_sql);


                    if (ZYSoft.DB.BLL.Common.ExecuteSQLTran(ls_Sql, ref v_str_ErrMsg) > 0)
                    {
                        return returnResult("success", "success", v_str_Result);
                    }
                    else
                    {
                        return returnResult("", "更新报工数量失败," + v_str_ErrMsg, v_str_Result);
                    }
                }
                else
                {
                    return returnResult("", "保存报工记录失败," + v_str_ErrMsg, v_str_Result);
                }
            }
            catch (Exception ex)
            {
                return returnResult("", ex.Message, null);
            }
        }



        /// <summary>
        /// 删除报工记录
        /// </summary>
        /// <param name="v_str_Json"></param>
        /// <returns></returns>
        public string DelMORpt(string v_str_Json)
        {
            try
            {
                //解析
                DAPI.DAPI_DelMORpt _DAPI_DelMORpt = JsonConvert.DeserializeObject<DAPI.DAPI_DelMORpt>(v_str_Json);


                //检查记录是否是最后一道，必须从后往前删除，如果是工序还需要 删除报工单记录

                string v_str_Result = "", v_str_ErrMsg = "";


                ZYSoft.DB.Common.Configuration.ConnectionString = connStr;

                string v_str_sql = "", v_str_Guid = Guid.NewGuid().ToString();

                v_str_sql = string.Format(@"SELECT * FROM [ZYSoft_LYYL_2019].[dbo].[t_MOReportList] WHERE FID ='{0}' ", _DAPI_DelMORpt.FID);

                DataTable dt_Record = ZYSoft.DB.BLL.Common.ExecuteDataTable(v_str_sql);

                if (dt_Record == null)
                {
                    return returnResult("", "未获取到报工的记录", null);
                }
                else if (dt_Record.Rows.Count == 0)
                {
                    return returnResult("", "未获取到报工的记录2", null);
                }

                //检查当前记录对应的工序是否有后道

                v_str_sql = string.Format(@"SELECT 1 FROM  [ZYSoft_LYYL_2019].[dbo].[t_MOReportList] WHERE FID > {0} AND FMoDId = '{1}'",
                        _DAPI_DelMORpt.FID, dt_Record.Rows[0]["FMoDId"].ToString());

                if (ZYSoft.DB.BLL.Common.Exist(v_str_sql))
                {
                    return returnResult("", "当前工单存在后续汇报记录，必须从后往前删除", null);
                }
                else
                {
                    //调接口删除 汇报单
                    //更新报工数量，
                    //如果是超额则不需要调接口生单，后续将重先进行写入记录并生单

                    ZYSoft_LYYL_U8API_V125.clsFC _U8Interface = new ZYSoft_LYYL_U8API_V125.clsFC();

                    if (_U8Interface.DeleteU8Bills("574B7C58-3ED8-4D54-93B4-BDB5222174C2",
                                            _DAPI_DelMORpt.FAccountNo,
                                            DateTime.Now.Year.ToString(), //ConfigurationManager.AppSettings["AccountYear"],
                                            _DAPI_DelMORpt.FUserID,
                                            _DAPI_DelMORpt.FUserPwd,
                                            _DAPI_DelMORpt.FID, "FC91", DateTime.Now.ToString("yyyy-MM-dd"), "ZYSoft_LYYL_2019",
                                            ref v_str_ErrMsg))
                    {
                        return returnResult("", "生成报工单失败," + v_str_ErrMsg, null);
                    }



                    List<string> ls_Sql = new List<string>();

                    //将报工数量、合格、不合格、待审核 数量 更新进汇总表
                    v_str_sql = string.Format(@"UPDATE T1 SET T1.FRptQty = ISNULL(T1.FRptQty, 0) - T2.FRptQty,
                    T1.FGoodQty = ISNULL(T1.FGoodQty, 0) - T2.FGoodQty,
                    T1.FBadQty = ISNULL(T1.FBadQty, 0) - T2.FBadQty,
                    T1.FUnVerifyQty = ISNULL(T1.FUnVerifyQty, 0) - T2.FUnVerifyQty
                    FROM ZYSoft_LYYL_2019.dbo.t_MOReportQty T1 JOIN ZYSoft_LYYL_2019.dbo.t_MOReportList t2 on t1.FMOID = T2.FMOID
                    AND T1.FMoDId = T2.FMoDId AND T1.FMoRoutingDId = T2.FMoRoutingDId AND T1.FOpSeq = T2.FOpSeq AND T1.FStepCode=T2.FStepCode
                    WHERE T2.FID = '{0}' AND FIsNeedConfirm=0 ", v_str_FID);
                    ls_Sql.Add(v_str_sql);

                    //将待确认数量 更新进汇总表
                    v_str_sql = string.Format(@"UPDATE T1 SET T1.FUnConfirmQty = ISNULL(T1.FUnConfirmQty, 0) - T2.FRptQty
                    FROM ZYSoft_LYYL_2019.dbo.t_MOReportQty T1 JOIN ZYSoft_LYYL_2019.dbo.t_MOReportList t2 on t1.FMOID = T2.FMOID
                    AND T1.FMoDId = T2.FMoDId AND T1.FMoRoutingDId = T2.FMoRoutingDId AND T1.FOpSeq = T2.FOpSeq AND T1.FStepCode=T2.FStepCode
                    WHERE T2.FID = '{0}' AND FIsNeedConfirm=1 ", v_str_FID);
                    ls_Sql.Add(v_str_sql);


                    //删除报工记录
                    v_str_sql = string.Format(@"DELETE FROM ZYSoft_LYYL_2019.dbo.t_MOReportQty 
                    WHERE FID = {0} ", v_str_FID);
                    ls_Sql.Add(v_str_sql);


                    if (ZYSoft.DB.BLL.Common.ExecuteSQLTran(ls_Sql, ref v_str_ErrMsg) > 0)
                    {
                        return returnResult("success", "success", v_str_Result);
                    }
                    else
                    {
                        return returnResult("", "删除报工记录失败," + v_str_ErrMsg, v_str_Result);
                    }
                }
            }
            catch (Exception ex)
            {
                return returnResult("", ex.Message, null);
            }
        }
    }
}
