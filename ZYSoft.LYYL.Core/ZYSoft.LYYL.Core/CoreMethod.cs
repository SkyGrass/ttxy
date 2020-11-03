using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace ZYSoft.LYYL.Core
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
                return "";
            }
            //用友密码最后一位补位 
            return  rethash + (char)3;


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
                //LogHelper.WriteErrLog(0, v_str_Json); 
                string v_str_Result = "";

                //解析
                DAPI.DAPI_MO _DAPI_MO = JsonConvert.DeserializeObject<DAPI.DAPI_MO>(v_str_Json);

                ZYSoft.DB.Common.Configuration.ConnectionString = connStr;

                string v_str_sql = "";
                //根据工单分录ID，取出工单及产品信息
                Regex rx = new Regex("^[0-9]*$");
                if (!rx.IsMatch(_DAPI_MO.FModID))
                {
                    if (!_DAPI_MO.FModID.Contains("-"))
                    {
                        return returnResult("", "生产订单录入有误", null);
                    }
                }


                if (_DAPI_MO.FModID.Contains("-"))
                {
                    v_str_sql = string.Format(@"select b.moid, b.modid, a.mocode,b.sortseq,b.invcode,c.cinvname,c.cinvstd,d.cComUnitName,b.Free1 cVersion, 
                     b.mdeptcode,f.cDepName,F.cDepMemo,b.define30 ,b.Qty,bb.StartDate,bb.DueDate
                    ,b.MoLotCode ,b.CostItemCode  ,B.Define32 cProject
                    from mom_order a with(nolock) inner join mom_orderdetail b  with(nolock) on a.moid = b.moid
                    LEFT JOIN mom_morder bb ON b.MoDId=bb.MoDId AND b.moid=bb.MoId
                    inner join inventory c on b.invcode = c.cinvcode 
                    LEFT JOIN ComputationUnit d ON c.cComUnitCode= d.cComunitCode
                    JOIN dbo.Department f  ON b.MDeptCode=f.cDepCode  AND f.cDepMemo IN ('前道','后道')
                    where b.status = 3 and a.mocode = '{0}' and b.sortseq ='{1}'
                    ", _DAPI_MO.FModID.Split('-')[0], _DAPI_MO.FModID.Split('-')[1]);
                }
                else
                {
                    v_str_sql = string.Format(@"select b.moid, b.modid, a.mocode,b.sortseq,b.invcode,c.cinvname,c.cinvstd,d.cComUnitName,b.Free1 cVersion, 
                 b.mdeptcode,f.cDepName,F.cDepMemo,b.define30 ,b.Qty,bb.StartDate,bb.DueDate
                ,b.MoLotCode ,b.CostItemCode  ,B.Define32 cProject
                from mom_order a with(nolock) inner join mom_orderdetail b  with(nolock) on a.moid = b.moid
                LEFT JOIN mom_morder bb ON b.MoDId=bb.MoDId AND b.moid=bb.MoId
                inner join inventory c on b.invcode = c.cinvcode 
                LEFT JOIN ComputationUnit d ON c.cComUnitCode= d.cComunitCode
                JOIN dbo.Department f ON b.MDeptCode=f.cDepCode  AND f.cDepMemo IN ('前道','后道')
                where b.status = 3 and b.modid = '{0}'
                ", _DAPI_MO.FModID);
                }
                DataTable dt_MO = ZYSoft.DB.BLL.Common.ExecuteDataTable(v_str_sql);

                if (dt_MO == null)
                {
                    return returnResult("", "未获取到生产订单", null);
                }
                else if (dt_MO.Rows.Count == 0)
                {
                    return returnResult("", "未获取到生产订单记录(检查工单部门)", null);
                }

                _DAPI_MO.FModID = dt_MO.Rows[0]["modid"].ToString();

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
                    //if (HttpContext.Current == null)
                    //{
                    //    v_str_FilePath = Application.StartupPath + "\\Img\\";
                    //}
                    //else
                    //{
                    //    v_str_FilePath = HttpContext.Current.Server.MapPath("~/Img/");
                    //}

                    v_str_FilePath = _DAPI_MO.FPath;

                    if (!Directory.Exists(v_str_FilePath))
                    {
                        Directory.CreateDirectory(v_str_FilePath);
                    }

                    v_str_FileName = v_str_FilePath + dt_MO.Rows[0]["invcode"].ToString() + "." + dt_Pic.Rows[0]["cPicturetype"].ToString();
                    //LogHelper.WriteErrLog(0, v_str_FileName);

                    FileStream fs = new FileStream(v_str_FileName, FileMode.OpenOrCreate, FileAccess.Write);
                    fs.Write(MyData, 0, ArraySize);
                    fs.Close();

                    v_str_FileName = string.Format(@"{0}/{1}", _DAPI_MO.FUrl, dt_MO.Rows[0]["invcode"].ToString() + "." + dt_Pic.Rows[0]["cPicturetype"].ToString());
                    //LogHelper.WriteErrLog(0, v_str_FileName);
                }

                //预置数量汇总空行
                v_str_sql = string.Format(@"
                 INSERT INTO [ZYSoft_LYYL_2019].[dbo].[t_MOReportQty]
	                ([FMOID],[FMoDId],[FMoRoutingId],[FMoRoutingDId],[FOperationId]
	                ,[FOpSeq],[FStepCode])
                    SELECT T1.MOID,T1.MoDId,T1.MoRoutingId,T1.MoRoutingDId,T1.OperationId,T1.OpSeq,'' 
				 FROM sfc_moroutingdetail t1    
				WHERE  T1.MoDId= {0}   and (t1.SubFlag=1 or T1.Define25='是')
				AND   MoRoutingDId NOT IN (SELECT [FMoRoutingDId] FROM ZYSoft_LYYL_2019.dbo.t_MOReportQty WHERE FMoDId= {0})
            ", _DAPI_MO.FModID);
                ZYSoft.DB.BLL.Common.ExecuteNonQuery(v_str_sql);

                //更新检验合格数量  BalQualifiedQty 改为 CompleteQty 完成数量 
                v_str_sql = string.Format(@" UPDATE T1 SET T1.FGoodQty=T2.CompleteQty,T1.FBadQty=T1.FRptQty - T2.CompleteQty  
                     FROM ZYSoft_LYYL_2019..t_MOReportQty T1 JOIN sfc_moroutingdetail T2 ON T1.FMOID=T2.MOID AND T1.FMoDId=T2.modid AND T1.FMoRoutingId =T2.MoRoutingId AND T1.FMoRoutingDId= T2.MoRoutingDId
                     WHERE  T2.Define25='是' AND T2.MoDId={0} ", _DAPI_MO.FModID);
                ZYSoft.DB.BLL.Common.ExecuteNonQuery(v_str_sql);

                //委外合格 数量 (报工数量默认为合格数量)
                v_str_sql = string.Format(@" UPDATE T1 SET T1.FRptQty=T2.CompleteQty, T1.FGoodQty=T2.CompleteQty,T1.FBadQty=0
                     FROM ZYSoft_LYYL_2019..t_MOReportQty T1 JOIN sfc_moroutingdetail T2 ON T1.FMOID=T2.MOID AND T1.FMoDId=T2.modid AND T1.FMoRoutingId =T2.MoRoutingId AND T1.FMoRoutingDId= T2.MoRoutingDId
                     WHERE  T2.SubFlag=1 AND T2.MoDId={0} ", _DAPI_MO.FModID);
                ZYSoft.DB.BLL.Common.ExecuteNonQuery(v_str_sql);


                //取出工序  根据人员对应的工序判断是否可操作
                //检验取合格数量进行流转
                //委外工序无需报工，待收料回来后继续流转

                v_str_sql = string.Format(@"SELECT row_number() over (order by opseq) as ino,  MoDId,MoRoutingId,t1.MoRoutingDId,OpSeq,t1.OperationId,Description,WcId,Remark,  FirstFlag,LastFlag,BFFlag, SubFlag,
                    BalMachiningQty iPlanQty,
                    case when T1.Define25='是' OR t1.SubFlag=1  then CompleteQty else  isnull(t3.FGoodQty,0) end iGoodQty ,BalScrapQty iBadQty ,
                    ISNULL(T3.FRptQty,0) FRptQty, ISNULL(T3.FUnConfirmQty,0) FUnConfirmQty ,ISnull(T3.FUnVerifyQty,0) FUnVerifyQty,   t2.QtMethod ,
                    CASE WHEN t4.OperationId >0 THEN 1 ELSE 0 END FIsPermitRpt ,T1.Define25 FIsVerify
                    FROM sfc_moroutingdetail t1 
                    LEFT OUTER JOIN sfc_moroutinginsp t2 ON t1.MoRoutingInspId=t2.MoRoutingInspId AND t1.MoRoutingDId=t2.MoRoutingDId
                    LEFT OUTER JOIN ZYSoft_LYYL_2019.dbo.t_MOReportQty T3 ON T1.MoRoutingDId=T3.FMoRoutingDId AND ISNULL(FStepCode,'')=''   AND FRptQty >0 
                    AND T3.FMoDId=T1.MoDId AND T3.FOperationId= T1.OperationId
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
                    AND T3.FMoDId=T1.MoDId AND T3.FOperationId= T1.OperationId
                    LEFT OUTER JOIN (select operationid,cstepcode from HM_LY_PsnRoutingLView
                    where ivouchstate=2 and cstepcode is NOT null and cpsncode='{1}'
                    ) t4 on t1.operationid=t4.operationid  AND T2.cstepcode=T4.cstepcode
                    WHERE T1.MoDId={0} ORDER BY   T1.OperationId,t1.MoRoutingDId, t2.cStepCode     ", _DAPI_MO.FModID, _DAPI_MO.FEmpCode);
                DataTable dt_Setp = ZYSoft.DB.BLL.Common.ExecuteDataTable(v_str_sql);

                if (dt_Setp == null)
                {
                    return returnResult("", "未获取到工步", null);
                }
                //else if (dt_Setp.Rows.Count == 0)
                //{
                //    return returnResult("", "未获取到工序计划记录", null);
                //}

                string v_str_DeptMemo = "";

                DAPI.DAPI_MORouting _MORouting = new DAPI.DAPI_MORouting();

                _MORouting.MoID = dt_MO.Rows[0]["moid"].ToString();
                _MORouting.ModID = dt_MO.Rows[0]["modid"].ToString();
                _MORouting.MoCode = dt_MO.Rows[0]["mocode"].ToString();
                _MORouting.SortSeq = dt_MO.Rows[0]["sortseq"].ToString();

                //根据部门判定是 11机加工[有序报工]，还是 12装配调试[无序报工]
                _MORouting.mDeptCode = dt_MO.Rows[0]["mdeptcode"].ToString();
                v_str_DeptMemo = dt_MO.Rows[0]["cDepMemo"].ToString();

                _MORouting.cInvCode = dt_MO.Rows[0]["invcode"].ToString();
                _MORouting.cInvName = dt_MO.Rows[0]["cinvname"].ToString();
                _MORouting.cInvStd = dt_MO.Rows[0]["cinvstd"].ToString();

                _MORouting.MoLotCode = dt_MO.Rows[0]["MoLotCode"].ToString();
                _MORouting.cProject = dt_MO.Rows[0]["cProject"].ToString();
                _MORouting.cVersion = dt_MO.Rows[0]["cVersion"].ToString();
                _MORouting.cComUnitName = dt_MO.Rows[0]["cComUnitName"].ToString();
                _MORouting.StartDate = DateTime.Parse(dt_MO.Rows[0]["StartDate"].ToString());
                _MORouting.DueDate = DateTime.Parse(dt_MO.Rows[0]["DueDate"].ToString());


                _MORouting.cInvImgPath = v_str_FileName;
                //_MORouting.cInvImgPath = string.Format(@"{0}/{1}", _DAPI_MO.FUrl, dt_MO.Rows[0]["invcode"].ToString() + "." + dt_Pic.Rows[0]["cPicturetype"].ToString());

                _MORouting.iQty = decimal.Parse(dt_MO.Rows[0]["Qty"].ToString());

                List<DAPI.DAPI_MORoutingEntry> ls_MORoutingEntry = new List<DAPI.DAPI_MORoutingEntry>();

                foreach (DataRow dr in dt_Process.Rows)
                {
                    //LogHelper.WriteErrLog(0, "工序");

                    _MORouting.MoRoutingId = dr["MoRoutingId"].ToString();

                    DAPI.DAPI_MORoutingEntry _MORoutingEntry = new DAPI.DAPI_MORoutingEntry();

                    DataRow[] dr_Find = dt_Process.Select("ino = '" + (int.Parse(dr["ino"].ToString()) - 1).ToString() + "'");
                    if (dr_Find.Length == 0) //首道直接就是自己
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

                    //工序是否检验
                    if (dr["FIsVerify"].ToString().Equals("是"))
                        _MORoutingEntry.IsVerifyFlag = true;
                    else
                        _MORoutingEntry.IsVerifyFlag = false;

                    //工序是否可超额
                    if (_MORoutingEntry.ProcessName.Contains("CNC") |
                        _MORoutingEntry.ProcessName.Equals("车床"))
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
                    DataRow[] dr_SetpS = dt_Setp.Select("OperationId= '" + _MORoutingEntry.ProcessID + "' and MoRoutingDId = '"+ _MORoutingEntry.MoRoutingDId+"'  ");

                    if (dr_SetpS.Length > 0)
                    {
                        _MORoutingEntry.IsExistSetp = true;

                        //最出工步中最小的合格数量
                        _MORoutingEntry.iPlanQty = decimal.Parse(dt_Setp.Compute("Min(FGoodQty)", "OperationId = '" + _MORoutingEntry.ProcessID + "' and MoRoutingDId = '" + _MORoutingEntry.MoRoutingDId + "' ").ToString());
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
                    //LogHelper.WriteErrLog(0, "工序2");


                    //装配车间
                    //20180808    CT机械装配
                    //20180809    XR机械装配
                    //20180810    装配调试
                    //20180811    MR调试
                    //20180812    RT调试


                    //如果是装配，则可汇报数量=订单数量
                    //if ((_MORouting.mDeptCode.Equals("20180808") |
                    //    _MORouting.mDeptCode.Equals("20180809") |
                    //    _MORouting.mDeptCode.Equals("20180810") |
                    //    _MORouting.mDeptCode.Equals("20180811") |
                    //    _MORouting.mDeptCode.Equals("20180812") )
                    //    && _MORoutingEntry.IsExistSetp ==false && _MORoutingEntry.IsLastFlag==false)   //打头的部门是 装配调试 是无序加工
                    //{
                    //    _MORoutingEntry.iPlanQty = _MORouting.iQty;
                    //}
                    if (v_str_DeptMemo.Equals("后道")
                      && _MORoutingEntry.IsExistSetp == false
                      && _MORoutingEntry.IsLastFlag == false)   //打头的部门是 装配调试 是无序加工
                    {
                        _MORoutingEntry.iPlanQty = _MORouting.iQty;
                    }


                    //LogHelper.WriteErrLog(0, "11");

                    //if ((_MORouting.mDeptCode.Equals("20180808") |
                    //    _MORouting.mDeptCode.Equals("20180809") |
                    //    _MORouting.mDeptCode.Equals("20180810") |
                    //    _MORouting.mDeptCode.Equals("20180811") |
                    //    _MORouting.mDeptCode.Equals("20180812")) && _MORoutingEntry.IsExistSetp == false && _MORoutingEntry.IsLastFlag == true)   //12打头的部门是 装配调试 是无序加工
                    //{
                    //    //取出所有工序的最小合格数量 作为末道的可汇报数量
                    //    _MORoutingEntry.iPlanQty = decimal.Parse(dt_Process.Compute("Min(iGoodQty)", "LastFlag=0").ToString());
                    //}

                    if (v_str_DeptMemo.Equals("后道")
                        && _MORoutingEntry.IsExistSetp == false
                        && _MORoutingEntry.IsLastFlag == true)   //12打头的部门是 装配调试 是无序加工
                    {
                        //取出所有工序的最小合格数量 作为末道的可汇报数量
                        _MORoutingEntry.iPlanQty = decimal.Parse(dt_Process.Compute("Min(iGoodQty)", "LastFlag=0").ToString());
                    }


                    _MORoutingEntry.iGoodQty = decimal.Parse(dr["iGoodQty"].ToString());
                    //_MORoutingEntry.iBadQty = decimal.Parse(dr["iBadQty"].ToString());

                    _MORoutingEntry.iUnConfirmQty = decimal.Parse(dr["FUnConfirmQty"].ToString());
                    _MORoutingEntry.iUnVerifyQty = decimal.Parse(dr["FUnVerifyQty"].ToString());

                    _MORoutingEntry.iRptQty = decimal.Parse(dr["FRptQty"].ToString());


                    //委外工序无序汇报
                    if (bool.Parse(dr["SubFlag"].ToString()))
                    {
                        _MORoutingEntry.iGoodQty = decimal.Parse(dr["iGoodQty"].ToString());
                        _MORoutingEntry.iRptQty = decimal.Parse(dr["iGoodQty"].ToString());
                        _MORoutingEntry.IsPermitRpt = false;
                    }


                    _MORoutingEntry.iUnRptQty = _MORoutingEntry.iPlanQty - _MORoutingEntry.iRptQty - _MORoutingEntry.iUnConfirmQty - _MORoutingEntry.iUnVerifyQty;

                    //防止工序是检验的情况下，数量记录未回写 记录表中
                    _MORoutingEntry.iBadQty = _MORoutingEntry.iRptQty - _MORoutingEntry.iGoodQty;

                    //LogHelper.WriteErrLog(0, "22");

                    //处理工序对应的工步
                    List<DAPI.DAPI_MORoutingSetpEntry> ls_MORoutingSetpEntry = new List<DAPI.DAPI_MORoutingSetpEntry>();
                    foreach (DataRow dr_Setp in dr_SetpS)
                    {
                        DAPI.DAPI_MORoutingSetpEntry _MORoutingSetpEntry = new DAPI.DAPI_MORoutingSetpEntry();

                        _MORoutingSetpEntry.cStepProcessSortSeq = _MORoutingEntry.SortSeq;
                        _MORoutingSetpEntry.cStepProcessID = _MORoutingEntry.ProcessID;
                        _MORoutingSetpEntry.cStepProcessName = _MORoutingEntry.ProcessName;

                        _MORoutingSetpEntry.MoRoutingDId = _MORoutingEntry.MoRoutingDId;
                        _MORoutingSetpEntry.cStepCode = dr_Setp["cStepCode"].ToString();
                        _MORoutingSetpEntry.cStepName = dr_Setp["cStepName"].ToString();
                        //根据工序来判断工步是否可超
                        _MORoutingSetpEntry.IsExceedFlag = _MORoutingEntry.IsExceedFlag;

                        //用户是否可操作
                        if (dr_Setp["FIsPermitRpt"].ToString().Equals("1"))
                        {
                            _MORoutingSetpEntry.IsPermitRpt = true;
                        }
                        else
                            _MORoutingSetpEntry.IsPermitRpt = false;

                        //所有工步默认取上道合格数量
                        _MORoutingSetpEntry.iPlanQty = decimal.Parse(dr_Find[0]["iGoodQty"].ToString()); // decimal.Parse(dr_Setp["FPlanQty"].ToString());

                        //如果是装配，则可汇报数量=订单数量
                        //if (_MORouting.mDeptCode.Equals("20180808") |
                        //    _MORouting.mDeptCode.Equals("20180809") |
                        //    _MORouting.mDeptCode.Equals("20180810") |
                        //    _MORouting.mDeptCode.Equals("20180811") |
                        //    _MORouting.mDeptCode.Equals("20180812"))
                        //{
                        //    _MORoutingSetpEntry.iPlanQty = _MORouting.iQty;
                        //}

                        if (v_str_DeptMemo.Equals("后道"))
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

                //LogHelper.WriteErrLog(0, "准备序列化");
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

                //检查报工数量是否超额  (超额的不检查)
                if (!_DAPI_MORpt.FIsExceed)
                {
                    v_str_sql = string.Format(@"SELECT 1 FROM sfc_morouting t1 JOIN  sfc_moroutingdetail t2 ON t1.MoRoutingId=t2.MoRoutingId 
                    WHERE  t2.MoRoutingDId = {0} AND t1.Qty < ISNULL(t2.ReportQty, 0)
                     ", _DAPI_MORpt.FMoRoutingDId);
                    if (ZYSoft.DB.BLL.Common.Exist(v_str_sql))
                    {
                        return returnResult("", "超计划报工数量，禁止重复报工", null);
                    }
                }

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
               _DAPI_MORpt.FReason, (_DAPI_MORpt.FIsNeedConfirm ? 1 : 0), (_DAPI_MORpt.FUnVerifyQty > 0 ? 1 : 0), _DAPI_MORpt.FPositionCode,
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

                LogHelper.WriteErrLog(0, "提交语句");

                if (ZYSoft.DB.BLL.Common.ExecuteSQLTran(ls_Sql, ref v_str_ErrMsg) !=-1)
                {
                    ls_Sql.Clear();

                    //调接口送入汇报单
                    //如果是工步则不需要调接口生单
                    //如果是超额则不需要调接口生单，后续将重先进行写入记录并生单

                    string v_str_U8BillID = "", v_str_U8BillNo = "";

                    if (_DAPI_MORpt.FStepCode == "" && _DAPI_MORpt.FIsNeedConfirm == false)

                    {
                        LogHelper.WriteErrLog(0, "开始生成汇报单");

                        v_str_sql = "EXEC [ZYSoft_LYYL_2019].[dbo].P_ZYSoft_CallU8Service  'Build_MORpt', '" + v_str_Guid + "' ";
                        DataSet ds = ZYSoft.DB.BLL.Common.GetDataSetByTime(v_str_sql, 60 * 1000);

                        if (ds == null)
                        {
                            return returnResult("", "生成报工单失败", null);
                        }

                        string v_str_U8Result = ds.Tables[0].Rows[0]["FResult"].ToString();

                        //string v_str_U8Result = ZYSoft.DB.BLL.Common.ExecuteScalar(v_str_sql); 

                        LogHelper.WriteErrLog(0, "汇报单结果:" + v_str_Result);

                        if (v_str_U8Result != "Y")
                        {
                            return returnResult("", "生成报工单失败," + v_str_U8Result, null);
                        }

                        ////调用接口文件，生成U8单据
                        //ZYSoft_LYYL_U8API_V125.clsFC _U8Interface = new ZYSoft_LYYL_U8API_V125.clsFC();

                        //if (_U8Interface.BuildU8Bills("574B7C58-3ED8-4D54-93B4-BDB5222174C2",
                        //                        _DAPI_MORpt.FAccountNo,
                        //                        DateTime.Now.Year.ToString(), //ConfigurationManager.AppSettings["AccountYear"],
                        //                        _DAPI_MORpt.FUserID,
                        //                        _DAPI_MORpt.FUserPwd,
                        //                        v_str_Guid, "FC91", 1, DateTime.Now.ToString("yyyy-MM-dd"), "ZYSoft_LYYL_2019",
                        //                         ref v_str_U8BillID, ref v_str_U8BillNo, ref v_str_ErrMsg))
                        //{
                        //    return returnResult("", "生成报工单失败," + v_str_ErrMsg, null);
                        //}

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

                        v_str_U8BillID = dt_FC.Rows[0]["MID"].ToString();

                        //更新生单成功标记
                        v_str_sql = string.Format(@"UPDATE [ZYSoft_LYYL_2019].[dbo].[t_MOReportList] SET FIsBuildFC=1,
                        FFCBillID = '{0}', FFCBillNo = '{1}' WHERE FGuid = '{2}'",
                        dt_FC.Rows[0]["MID"].ToString(), dt_FC.Rows[0]["cVouchCode"].ToString(), v_str_Guid);
                        ls_Sql.Add(v_str_sql);
                    }

                    if (_DAPI_MORpt.FStepCode != "")
                    {
                        //更新生单成功标记
                        v_str_sql = string.Format(@"UPDATE [ZYSoft_LYYL_2019].[dbo].[t_MOReportList] SET FIsBuildFC=1,
                        FFCBillID = '{0}', FFCBillNo = '{1}' WHERE FGuid = '{2}'",
                        0, "工步报工", v_str_Guid);
                        ls_Sql.Add(v_str_sql);
                    }

                    //List<string> ls_Sql = new List<string>();


                    //将报工数量、合格、不合格、待审核 数量 更新进汇总表
                    if (_DAPI_MORpt.FIsVerifyFlag)
                    {
                        //如果本道需要U8中检验的，则不更新合格数量 不合格数量，
                        v_str_sql = string.Format(@"UPDATE T1 SET T1.FRptQty = ISNULL(T1.FRptQty, 0) + T2.FRptQty
                        FROM ZYSoft_LYYL_2019.dbo.t_MOReportQty T1 JOIN ZYSoft_LYYL_2019.dbo.t_MOReportList t2 on t1.FMOID = T2.FMOID
                        AND T1.FMoDId = T2.FMoDId AND T1.FMoRoutingDId = T2.FMoRoutingDId AND T1.FOpSeq = T2.FOpSeq AND T1.FStepCode=T2.FStepCode
                        WHERE T2.FGuid = '{0}' AND FIsNeedConfirm=0 ", v_str_Guid);
                        ls_Sql.Add(v_str_sql);
                    }
                    else
                    {
                        v_str_sql = string.Format(@"UPDATE T1 SET T1.FRptQty = ISNULL(T1.FRptQty, 0) + T2.FRptQty,
                        T1.FGoodQty = ISNULL(T1.FGoodQty, 0) + T2.FGoodQty,
                        T1.FBadQty = ISNULL(T1.FBadQty, 0) + T2.FBadQty
                        FROM ZYSoft_LYYL_2019.dbo.t_MOReportQty T1 JOIN ZYSoft_LYYL_2019.dbo.t_MOReportList t2 on t1.FMOID = T2.FMOID
                        AND T1.FMoDId = T2.FMoDId AND T1.FMoRoutingDId = T2.FMoRoutingDId AND T1.FOpSeq = T2.FOpSeq AND T1.FStepCode=T2.FStepCode
                        WHERE T2.FGuid = '{0}' AND FIsNeedConfirm=0 ", v_str_Guid);
                            ls_Sql.Add(v_str_sql);
                    }

                    //将待评审数量 更新进汇总表
                    v_str_sql = string.Format(@"UPDATE T1 SET T1.FUnVerifyQty = ISNULL(T1.FUnVerifyQty, 0) + T2.FUnVerifyQty
                    FROM ZYSoft_LYYL_2019.dbo.t_MOReportQty T1 JOIN ZYSoft_LYYL_2019.dbo.t_MOReportList t2 on t1.FMOID = T2.FMOID
                    AND T1.FMoDId = T2.FMoDId AND T1.FMoRoutingDId = T2.FMoRoutingDId AND T1.FOpSeq = T2.FOpSeq AND T1.FStepCode=T2.FStepCode
                    WHERE T2.FGuid = '{0}' AND FIsNeedVerify=1 ", v_str_Guid);
                    ls_Sql.Add(v_str_sql);

                    //将待确认数量 更新进汇总表
                    v_str_sql = string.Format(@"UPDATE T1 SET T1.FUnConfirmQty = ISNULL(T1.FUnConfirmQty, 0) + T2.FRptQty
                    FROM ZYSoft_LYYL_2019.dbo.t_MOReportQty T1 JOIN ZYSoft_LYYL_2019.dbo.t_MOReportList t2 on t1.FMOID = T2.FMOID
                    AND T1.FMoDId = T2.FMoDId AND T1.FMoRoutingDId = T2.FMoRoutingDId AND T1.FOpSeq = T2.FOpSeq AND T1.FStepCode=T2.FStepCode
                    WHERE T2.FGuid = '{0}' AND FIsNeedConfirm=1 ", v_str_Guid);
                    ls_Sql.Add(v_str_sql);

                    //将报工数量更新对工序计划表中
                    v_str_sql = string.Format(@"UPDATE T1 SET T1.ReportQty = ISNULL(T1.ReportQty, 0) + T2.FRptQty
                    FROM sfc_moroutingdetail T1 JOIN ZYSoft_LYYL_2019.dbo.t_MOReportList t2 on t1.MoRoutingId = T2.FMoRoutingId
                    AND T1.MoRoutingDId = T2.FMoRoutingDId AND T1.OperationId = T2.FMoRoutingId 
                    WHERE T2.FGuid = '{0}'  ", v_str_Guid);
                    ls_Sql.Add(v_str_sql);


                    LogHelper.WriteErrLog(0, "提交更新结果" );

                    if (ZYSoft.DB.BLL.Common.ExecuteSQLTran(ls_Sql, ref v_str_ErrMsg) > 0)
                    {

                        LogHelper.WriteErrLog(0, "提交结果成功");

                        //最后一道工序提交给WMS                        
                        if (_DAPI_MORpt.FIsLastFlag)
                        {
                            #region 末道[入序]工序则送入WMS
                            if (_DAPI_MORpt.FOpName.Equals("入库"))
                            {
                                LogHelper.WriteErrLog(0, "末道工序调WMS接口");

                                v_str_sql = "EXEC [ZYSoft_LYYL_2019].[dbo].P_ZYSoft_CallWMSService  'Build_StockIn10','" + v_str_U8BillID + "','' ";
                                ZYSoft.DB.BLL.Common.ExecuteNonQuery(v_str_sql);
                            }
                            #endregion

                            #region 末道是[车间流转] 焊接工序时生成产成品入库   现场周转库 126
                            if (_DAPI_MORpt.FOpName.Equals("车间流转"))
                            {
                                LogHelper.WriteErrLog(0, "进入车间流转");

                                //保存
                                v_str_sql = string.Format(@"INSERT INTO [{0}].[dbo].[t_PDARecord]
                                    ([FID],[FIdentifier],[FAccountID],[FYear],[FBillType]
                                    ,[FDate],[FDepCode],[FWHCode],[FMaker],[FROB]
                                    ,[FRemark],[FInvCode],[FComUnitCode],[FSTComUnitCode],[FChangRate]
                                    ,[FBatch],[FQty],[FNum],[FSourceQty],[FSourceNum]
                                    ,[FSourceBillID],[FSourceBillNo],[FSourceBillEntryID],[FSourceBillEntryNo]
                                    ,[FProInvCode],[FProBatch],[FProQty],[FSource],[FRdCode]
                                    ,[FcFree1]
                                     )
                                    SELECT NEWID(),'{1}','{2}','{3}','{4}',
                                    '{5}', T1.MDeptCode,'{6}','{7}','{8}',
                                    '{9}',T1.InvCode,t2.cComUnitCode ,t2.cSTComUnitCode,0,
                                    t1.MoLotCode,'{10}',0,ISNULL(T1.Qty,0) - ISNULL(T1.QualifiedInQty,0),  0 ,
                                    t1.moid,t0.MoCode,t1.MoDId,t1.SortSeq,
                                    t1.InvCode, t1.MoLotCode,t1.qty, '生产订单','102',  
                                    t1.Free1  
                                    FROM  mom_order t0 join  mom_orderdetail t1 on t0.moid=t1.MoId
                                    LEFT JOIN dbo.Inventory T2 ON T1.InvCode=T2.cInvCode                                    
                                    WHERE T1.MoDId= {11}",
                                       "ZYSoft_LYYL_2019",
                                       v_str_Guid, _DAPI_MORpt.FAccountNo, DateTime.Now.Year, "0411",
                                      DateTime.Now.ToString("yyyy-MM-dd"), "126", _DAPI_MORpt.FUserID, 1,
                                      "报工自动入库",
                                      _DAPI_MORpt.FGoodQty,
                                      _DAPI_MORpt.FModID );

                                if (ZYSoft.DB.BLL.Common.ExecuteNonQuery(v_str_sql) ==-1 )
                                {
                                    LogHelper.WriteErrLog(0, "报工自动入库 缓存失败");
                                }
                                else
                                {
                                    v_str_sql = "EXEC [ZYSoft_LYYL_2019].[dbo].P_ZYSoft_CallU8Service  'Build_MORdReocrd', '" + v_str_Guid + "' ";
                                    //string v_str_U8Result = ZYSoft.DB.BLL.Common.ExecuteScalar(v_str_sql);

                                    DataSet ds = ZYSoft.DB.BLL.Common.GetDataSetByTime(v_str_sql, 60 * 1000);

                                    if (ds == null)
                                    {
                                        return returnResult("", "生成产成品入库失败", null);
                                    }

                                    string v_str_U8Result = ds.Tables[0].Rows[0]["FResult"].ToString();

                                    if (v_str_U8Result != "Y")
                                    {
                                        return returnResult("", "生成产成品入库失败," + v_str_U8Result, null);
                                    }


                                    ////string v_str_U8BillID = "", v_str_U8BillNo = "";
                                    //ZYSoft_LYYL_U8API_V125.clsOtherInOut _U8Interface2 = new ZYSoft_LYYL_U8API_V125.clsOtherInOut();

                                    //if (_U8Interface2.BuildU8Bills("574B7C58-3ED8-4D54-93B4-BDB5222174C2",
                                    //           _DAPI_MORpt.FAccountNo,
                                    //           DateTime.Now.Year.ToString(), //ConfigurationManager.AppSettings["AccountYear"],
                                    //           _DAPI_MORpt.FUserID,
                                    //           _DAPI_MORpt.FUserPwd,
                                    //           v_str_Guid, "0411", 1, DateTime.Now.ToString("yyyy-MM-dd"), "ZYSoft_LYYL_2019",
                                    //            ref v_str_U8BillID, ref v_str_U8BillNo, ref v_str_ErrMsg))
                                    //{
                                    //    LogHelper.WriteErrLog(0, "生成产成品入库失败," + v_str_ErrMsg);
                                    //}
                                }
                            }
                            #endregion
                        }

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
                LogHelper.WriteErrLog(0, "出错了"+ex.Message);

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
                                                           where ivouchstate= 2 and cstepcode is null and cpsncode = '{0}'
                                                            ) t5 on t1.FOperationId = t5.operationid
                                         WHERE FIsNeedConfirm = 1 AND FIsFinishConfirm = 0   ", _DAPI_MO.FUserID);

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
                                                           where ivouchstate= 2 and cstepcode is null and cpsncode = '{0}'
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
        /// 查询可修改的报工记录 
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
                string v_str_sql = string.Format(@"SELECT T4.MoCode,T2.SortSeq, T3.cInvCode,T3.cInvName,t3.cInvStd,t31.cComUnitName, T2.MDeptCode,t5.cDepName,T2.Qty iQty,t2.Define32 cProject,t2.Free1 cVersion, t6.StartDate,t6.DueDate,t2.MoLotCode, T1.*,t7.FName FPostion
                                        FROM ZYSoft_LYYL_2019.dbo.t_MOReportList T1 JOIN mom_orderdetail T2 ON T1.FMoDId = T2.MoDId AND T1.FMOID = T2.MoId
                                        LEFT JOIN dbo.Inventory T3 ON T2.InvCode = T3.cInvCode
										LEFT JOIN ComputationUnit t31 ON t3.cComUnitCode=t31.cComunitCode
                                        LEFT JOIN mom_order T4 ON T4.MoId = T2.MoId
										left outer join department t5 on t2.MDeptCode= t5.cDepCode
										LEFT JOIN mom_morder t6 ON t2.MoDId=t6.MoDId AND t2.moid=t6.MoId
										left outer join ZYSoft_LYYL_2019..t_Place t7 on t1.FPositionCode=t7.FCode
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
        /// 按工单查询报工记录 
        /// </summary>
        /// <param name="v_str_MoCode">可录入工单号，或扫描工单条码</param>
        /// <returns></returns>
        public string GetSingleMORpt2(string v_str_MoCode)
        {
            try
            {
                //查询报工记录

                string v_str_Result = "";

                string v_str_Filter = "";
                //根据工单分录ID，取出工单及产品信息
                Regex rx = new Regex("^[0-9]*$");
                if (rx.IsMatch(v_str_MoCode))
                {
                    v_str_Filter = "T2.MoDId = " + v_str_MoCode;
                }
                else
                {
                    v_str_Filter = "T4.MoCode = '" + v_str_MoCode + "'";
                }

           
                ZYSoft.DB.Common.Configuration.ConnectionString = connStr;

                string v_str_sql = string.Format(@"SELECT T4.MoCode,T2.SortSeq, T3.cInvCode,T3.cInvName,t3.cInvStd,t31.cComUnitName, T2.MDeptCode,t5.cDepName,T2.Qty iQty,t2.Define32 cProject,t2.Free1 cVersion, t6.StartDate,t6.DueDate,t2.MoLotCode, T1.*,t7.FName FPostion
                                        FROM ZYSoft_LYYL_2019.dbo.t_MOReportList T1 JOIN mom_orderdetail T2 ON T1.FMoDId = T2.MoDId AND T1.FMOID = T2.MoId
                                        LEFT JOIN dbo.Inventory T3 ON T2.InvCode = T3.cInvCode
										LEFT JOIN ComputationUnit t31 ON t3.cComUnitCode=t31.cComunitCode
                                        LEFT JOIN mom_order T4 ON T4.MoId = T2.MoId
										left outer join department t5 on t2.MDeptCode= t5.cDepCode
										LEFT JOIN mom_morder t6 ON t2.MoDId=t6.MoDId AND t2.moid=t6.MoId
										left outer join ZYSoft_LYYL_2019..t_Place t7 on t1.FPositionCode=t7.FCode
										WHERE {0}  order by fopseq,FStepCode  ", v_str_Filter);

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
        /// 按工单查询报工记录 
        /// </summary>
        /// <param name="v_str_Json">传入工单号、登录用户</param>
        /// <returns></returns>
        public string GetSingleMORpt(string v_str_Json)
        {
            try
            {
                //LogHelper.WriteErrLog(0, v_str_Json); 
                string v_str_Result = "";

                //解析
                DAPI.DAPI_MO _DAPI_MO = JsonConvert.DeserializeObject<DAPI.DAPI_MO>(v_str_Json);

                ZYSoft.DB.Common.Configuration.ConnectionString = connStr;

                string v_str_sql = "";
                //根据工单分录ID，取出工单及产品信息
                Regex rx = new Regex("^[0-9]*$");
                if (!rx.IsMatch(_DAPI_MO.FModID))
                {
                    if (!_DAPI_MO.FModID.Contains("-"))
                    {
                        return returnResult("", "生产订单录入有误", null);
                    }
                }


                if (_DAPI_MO.FModID.Contains("-"))
                {
                    v_str_sql = string.Format(@"select b.moid, b.modid, a.mocode,b.sortseq,b.invcode,c.cinvname,c.cinvstd,d.cComUnitName,b.Free1 cVersion, 
                 b.mdeptcode,f.cDepName,F.cDepMemo,b.define30 ,b.Qty,bb.StartDate,bb.DueDate
                ,b.MoLotCode ,b.CostItemCode  ,B.Define32 cProject
                from mom_order a with(nolock) inner join mom_orderdetail b  with(nolock) on a.moid = b.moid
                LEFT JOIN mom_morder bb ON b.MoDId=bb.MoDId AND b.moid=bb.MoId
                inner join inventory c on b.invcode = c.cinvcode 
                LEFT JOIN ComputationUnit d ON c.cComUnitCode= d.cComunitCode
                JOIN dbo.Department f  ON b.MDeptCode=f.cDepCode  AND f.cDepMemo IN ('前道','后道')
                where b.status = 3 and a.mocode = '{0}' and b.sortseq ='{1}'
                ", _DAPI_MO.FModID.Split('-')[0], _DAPI_MO.FModID.Split('-')[1]);
                }
                else
                {
                    v_str_sql = string.Format(@"select b.moid, b.modid, a.mocode,b.sortseq,b.invcode,c.cinvname,c.cinvstd,d.cComUnitName,b.Free1 cVersion, 
                 b.mdeptcode,f.cDepName,F.cDepMemo,b.define30 ,b.Qty,bb.StartDate,bb.DueDate
                ,b.MoLotCode ,b.CostItemCode  ,B.Define32 cProject
                from mom_order a with(nolock) inner join mom_orderdetail b  with(nolock) on a.moid = b.moid
                LEFT JOIN mom_morder bb ON b.MoDId=bb.MoDId AND b.moid=bb.MoId
                inner join inventory c on b.invcode = c.cinvcode 
                LEFT JOIN ComputationUnit d ON c.cComUnitCode= d.cComunitCode
                JOIN dbo.Department f  ON b.MDeptCode=f.cDepCode  AND f.cDepMemo IN ('前道','后道')
                where b.status = 3 and b.modid = '{0}'
                ", _DAPI_MO.FModID);
                }
                DataTable dt_MO = ZYSoft.DB.BLL.Common.ExecuteDataTable(v_str_sql);

                if (dt_MO == null)
                {
                    return returnResult("", "未获取到生产订单", null);
                }
                else if (dt_MO.Rows.Count == 0)
                {
                    return returnResult("", "未获取到生产订单记录", null);
                }

                _DAPI_MO.FModID = dt_MO.Rows[0]["modid"].ToString();

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
                    //if (HttpContext.Current == null)
                    //{
                    //    v_str_FilePath = Application.StartupPath + "\\Img\\";
                    //}
                    //else
                    //{
                    //    v_str_FilePath = HttpContext.Current.Server.MapPath("~/Img/");
                    //}

                    v_str_FilePath = _DAPI_MO.FPath;

                    if (!Directory.Exists(v_str_FilePath))
                    {
                        Directory.CreateDirectory(v_str_FilePath);
                    }

                    v_str_FileName = v_str_FilePath + dt_MO.Rows[0]["invcode"].ToString() + "." + dt_Pic.Rows[0]["cPicturetype"].ToString();
                    //LogHelper.WriteErrLog(0, v_str_FileName);

                    FileStream fs = new FileStream(v_str_FileName, FileMode.OpenOrCreate, FileAccess.Write);
                    fs.Write(MyData, 0, ArraySize);
                    fs.Close();

                    v_str_FileName = string.Format(@"{0}/{1}", _DAPI_MO.FUrl, dt_MO.Rows[0]["invcode"].ToString() + "." + dt_Pic.Rows[0]["cPicturetype"].ToString());
                    //LogHelper.WriteErrLog(0, v_str_FileName);
                }

                //更新检验合格数量 
                v_str_sql = string.Format(@" UPDATE T1 SET T1.FGoodQty=T2.CompleteQty,T1.FBadQty=T1.FRptQty - T2.CompleteQty  
                     FROM ZYSoft_LYYL_2019..t_MOReportQty T1 JOIN sfc_moroutingdetail T2 ON T1.FMOID=T2.MOID AND T1.FMoDId=T2.modid AND T1.FMoRoutingId =T2.MoRoutingId AND T1.FMoRoutingDId= T2.MoRoutingDId
                     WHERE  T2.Define25='是' AND T2.MoDId={0} ", _DAPI_MO.FModID);
                ZYSoft.DB.BLL.Common.ExecuteNonQuery(v_str_sql);

                //委外合格 数量 (报工数量默认为合格数量)
                v_str_sql = string.Format(@" UPDATE T1 SET T1.FRptQty=T2.CompleteQty, T1.FGoodQty=T2.CompleteQty,T1.FBadQty=0
                     FROM ZYSoft_LYYL_2019..t_MOReportQty T1 JOIN sfc_moroutingdetail T2 ON T1.FMOID=T2.MOID AND T1.FMoDId=T2.modid AND T1.FMoRoutingId =T2.MoRoutingId AND T1.FMoRoutingDId= T2.MoRoutingDId
                     WHERE  T2.SubFlag=1 AND T2.MoDId={0} ", _DAPI_MO.FModID);
                ZYSoft.DB.BLL.Common.ExecuteNonQuery(v_str_sql);


                //取出工序 
                v_str_sql = string.Format(@"SELECT row_number() over (order by opseq) as ino,  MoDId,MoRoutingId,t1.MoRoutingDId,OpSeq,t1.OperationId,Description,WcId,Remark,  FirstFlag,LastFlag,BFFlag, 
                    BalMachiningQty FPlanQty, isnull(t3.FGoodQty,0) FGoodQty ,isnull(t3.FBadQty,0) FBadQty ,
                    ISNULL(T3.FRptQty,0) FRptQty, ISNULL(T3.FUnConfirmQty,0) FUnConfirmQty ,ISnull(T3.FUnVerifyQty,0) FUnVerifyQty              
                    FROM sfc_moroutingdetail t1 
                    LEFT OUTER JOIN sfc_moroutinginsp t2 ON t1.MoRoutingInspId=t2.MoRoutingInspId AND t1.MoRoutingDId=t2.MoRoutingDId
                    LEFT OUTER JOIN ZYSoft_LYYL_2019.dbo.t_MOReportQty T3 ON T1.MoRoutingDId=T3.FMoRoutingDId AND ISNULL(FStepCode,'')=''    AND FRptQty >0 
                    AND T3.FMoDId=T1.MoDId AND T3.FOperationId= T1.OperationId                       
                    where T1.MoDId= {0} ORDER BY opseq  ", _DAPI_MO.FModID);
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
                    ISNULL(T3.FRptQty,0) FRptQty, ISNULL(T3.FUnConfirmQty,0) FUnConfirmQty ,ISnull(T3.FUnVerifyQty,0) FUnVerifyQty                  
                    FROM [HM_LY_MoRountingStepMain] T1 JOIN [HM_LY_MoRountingStepSub] T2 ON T2.ID = T1.ID
                    LEFT OUTER JOIN ZYSoft_LYYL_2019.dbo.t_MOReportQty T3 ON T1.MoRoutingDId=T3.FMoRoutingDId AND ISNULL(FStepCode,'')=cStepCode
                    AND T3.FMoDId=T1.MoDId AND T3.FOperationId= T1.OperationId
                    WHERE T1.MoDId={0} ORDER BY T1.OperationId,t2.cStepCode       ", _DAPI_MO.FModID);
                DataTable dt_Setp = ZYSoft.DB.BLL.Common.ExecuteDataTable(v_str_sql);

                if (dt_Setp == null)
                {
                    return returnResult("", "未获取到工步", null);
                }
                //else if (dt_Setp.Rows.Count == 0)
                //{
                //    return returnResult("", "未获取到工序计划记录", null);
                //}

                //取出报工记录
                v_str_sql = string.Format(@"SELECT CONVERT(VARCHAR,FDATE,120) FRptDate, * FROM ZYSoft_LYYL_2019.dbo.t_MOReportList
                WHERE FMODID= {0}", _DAPI_MO.FModID);

                DataTable dt_RptList = ZYSoft.DB.BLL.Common.ExecuteDataTable(v_str_sql);
                
                if(dt_RptList ==null)
                {
                    return returnResult("", "未获取到报工记录", null);
                }

                string v_str_DeptMemo = "";
                DAPI.DAPI_QueryMO _MORouting = new DAPI.DAPI_QueryMO();

                _MORouting.MoID = dt_MO.Rows[0]["moid"].ToString();
                _MORouting.ModID = dt_MO.Rows[0]["modid"].ToString();
                _MORouting.MoCode = dt_MO.Rows[0]["mocode"].ToString();
                _MORouting.SortSeq = dt_MO.Rows[0]["sortseq"].ToString();

                //根据部门判定是 11机加工[有序报工]，还是 12装配调试[无序报工]
                _MORouting.mDeptCode = dt_MO.Rows[0]["mdeptcode"].ToString();
                v_str_DeptMemo = dt_MO.Rows[0]["cDepMemo"].ToString();

                _MORouting.cInvCode = dt_MO.Rows[0]["invcode"].ToString();
                _MORouting.cInvName = dt_MO.Rows[0]["cinvname"].ToString();
                _MORouting.cInvStd = dt_MO.Rows[0]["cinvstd"].ToString();

                _MORouting.MoLotCode = dt_MO.Rows[0]["MoLotCode"].ToString();
                _MORouting.cProject = dt_MO.Rows[0]["cProject"].ToString();
                _MORouting.cVersion = dt_MO.Rows[0]["cVersion"].ToString();
                _MORouting.cComUnitName = dt_MO.Rows[0]["cComUnitName"].ToString();
                _MORouting.StartDate = DateTime.Parse(dt_MO.Rows[0]["StartDate"].ToString());
                _MORouting.DueDate = DateTime.Parse(dt_MO.Rows[0]["DueDate"].ToString());


                _MORouting.cInvImgPath = v_str_FileName;
                //_MORouting.cInvImgPath = string.Format(@"{0}/{1}", _DAPI_MO.FUrl, dt_MO.Rows[0]["invcode"].ToString() + "." + dt_Pic.Rows[0]["cPicturetype"].ToString());

                _MORouting.iQty = decimal.Parse(dt_MO.Rows[0]["Qty"].ToString());

                List<DAPI.DAPI_QueryMOProcess> ls_MORoutingEntry = new List<DAPI.DAPI_QueryMOProcess>();

                foreach (DataRow dr in dt_Process.Rows)
                {
                    #region 工序
                    DAPI.DAPI_QueryMOProcess _MORoutingEntry = new DAPI.DAPI_QueryMOProcess();

                    DataRow[] dr_Find = dt_Process.Select("ino = '" + (int.Parse(dr["ino"].ToString()) - 1).ToString() + "'");

                    _MORoutingEntry.SortSeq = dr["OpSeq"].ToString();
                    _MORoutingEntry.ProcessName = dr["Description"].ToString();
                    _MORoutingEntry.ProcessDesc = dr["Remark"].ToString();


                    //如果工序有工步，则按工步最小的合格数量作为此工序的可汇报数量
                    DataRow[] dr_SetpS = dt_Setp.Select("OperationId= '" + dr["OperationId"].ToString() + "' and MoRoutingDId = '" + dr["MoRoutingDId"].ToString() + "'");

                    if (dr_SetpS.Length > 0)
                    {
                        //最出工步中最小的合格数量
                        _MORoutingEntry.iPlanQty = decimal.Parse(dt_Setp.Compute("Min(FGoodQty)", "OperationId = '" + dr["OperationId"].ToString() + "' and MoRoutingDId = '" + dr["MoRoutingDId"].ToString() + "'").ToString());
                    }
                    else
                    {

                        //这里可能存在BUG，需要取上道合格数量才行,
                        if (bool.Parse(dr["FirstFlag"].ToString()))
                        {
                            //首道取产品数量
                            _MORoutingEntry.iPlanQty = _MORouting.iQty;
                        }
                        else
                        {
                            //取上道工序的合格数量
                            _MORoutingEntry.iPlanQty = decimal.Parse(dr_Find[0]["FGoodQty"].ToString());
                        }
                    }
                    //LogHelper.WriteErrLog(0, "工序2");


                    //如果是装配，则可汇报数量=订单数量
                    //if (_MORouting.mDeptCode.Equals("20180808") |
                    //    _MORouting.mDeptCode.Equals("20180809") |
                    //    _MORouting.mDeptCode.Equals("20180810") |
                    //    _MORouting.mDeptCode.Equals("20180811") |
                    //    _MORouting.mDeptCode.Equals("20180812"))   //12打头的部门是 装配调试 是无序加工
                    //{
                    //    _MORoutingEntry.iPlanQty = _MORouting.iQty;
                    //}

                    if(v_str_DeptMemo.Equals("后道"))
                    {
                        _MORoutingEntry.iPlanQty = _MORouting.iQty;
                    }



                    _MORoutingEntry.iGoodQty = decimal.Parse(dr["FGoodQty"].ToString());
                    _MORoutingEntry.iBadQty = decimal.Parse(dr["FBadQty"].ToString());

                    _MORoutingEntry.iUnConfirmQty = decimal.Parse(dr["FUnConfirmQty"].ToString());
                    _MORoutingEntry.iUnVerifyQty = decimal.Parse(dr["FUnVerifyQty"].ToString());

                    _MORoutingEntry.iRptQty = decimal.Parse(dr["FRptQty"].ToString());

                    _MORoutingEntry.iUnRptQty = _MORoutingEntry.iPlanQty - _MORoutingEntry.iRptQty - _MORoutingEntry.iUnConfirmQty - _MORoutingEntry.iUnVerifyQty;

                    _MORoutingEntry.IsSetp = false;

                    #region 工序汇报记录
                    //提取工序对应的报工记录 FOpSeq,FStepCode
                    DataRow[] dr_RptList = dt_RptList.Select("FOpSeq= '" + dr["OpSeq"].ToString() + "' AND FStepCode =''  ");
                    List<DAPI.DAPI_QueryMORptDetail> ls_MORptDetail = new List<DAPI.DAPI_QueryMORptDetail>();

                    foreach (DataRow drr in dr_RptList)
                    {
                        DAPI.DAPI_QueryMORptDetail _QueryMORptDetail = new DAPI.DAPI_QueryMORptDetail();
                        _QueryMORptDetail.RptDate = drr["FRptDate"].ToString();
                        _QueryMORptDetail.Maker = drr["FMaker"].ToString();


                        if (bool.Parse(drr["FIsNeedConfirm"].ToString()))
                        {
                            _QueryMORptDetail.iUnConfirmQty = decimal.Parse(drr["FRptQty"].ToString());
                        }
                        else
                        {
                            _QueryMORptDetail.iRptQty = decimal.Parse(drr["FRptQty"].ToString());
                            _QueryMORptDetail.iGoodQty = decimal.Parse(drr["FGoodQty"].ToString());
                            _QueryMORptDetail.iBadQty = decimal.Parse(drr["FBadQty"].ToString());
                        }

                        if (bool.Parse(drr["FIsNeedVerify"].ToString()))
                        {
                            _QueryMORptDetail.iUnVerifyQty = decimal.Parse(drr["FUnVerifyQty"].ToString());
                        }

                        ls_MORptDetail.Add(_QueryMORptDetail);
                    }

                    #endregion

                    _MORoutingEntry.MORptDetail = ls_MORptDetail;

                    ls_MORoutingEntry.Add(_MORoutingEntry);

                    #endregion

                    //处理工序对应的工步
                    #region 工步
                    foreach (DataRow dr_Setp in dr_SetpS)
                    {
                        DAPI.DAPI_QueryMOProcess _MORoutingSetpEntry = new DAPI.DAPI_QueryMOProcess();

                        _MORoutingSetpEntry.SortSeq = dr_Setp["cStepCode"].ToString();
                        _MORoutingSetpEntry.ProcessName = dr_Setp["cStepName"].ToString();
                        _MORoutingSetpEntry.ProcessDesc = "";
                        //所有工步默认取上道合格数量
                        _MORoutingSetpEntry.iPlanQty = decimal.Parse(dr_Find[0]["FGoodQty"].ToString()); // decimal.Parse(dr_Setp["FPlanQty"].ToString());

                        //如果是装配，则可汇报数量=订单数量
                        //if (_MORouting.mDeptCode.Equals("20180808") |
                        //    _MORouting.mDeptCode.Equals("20180809") |
                        //    _MORouting.mDeptCode.Equals("20180810") |
                        //    _MORouting.mDeptCode.Equals("20180811") |
                        //    _MORouting.mDeptCode.Equals("20180812"))
                        //{
                        //    _MORoutingSetpEntry.iPlanQty = _MORouting.iQty;
                        //}

                        if (v_str_DeptMemo.Equals("后道"))
                        {
                            _MORoutingSetpEntry.iPlanQty = _MORouting.iQty;
                        }

                        _MORoutingSetpEntry.iGoodQty = decimal.Parse(dr_Setp["FGoodQty"].ToString());
                        _MORoutingSetpEntry.iBadQty = decimal.Parse(dr_Setp["FBadQty"].ToString());

                        _MORoutingSetpEntry.iUnConfirmQty = decimal.Parse(dr_Setp["FUnConfirmQty"].ToString());
                        _MORoutingSetpEntry.iUnVerifyQty = decimal.Parse(dr_Setp["FUnVerifyQty"].ToString());

                        _MORoutingSetpEntry.iRptQty = decimal.Parse(dr_Setp["FRptQty"].ToString());

                        _MORoutingSetpEntry.iUnRptQty = _MORoutingSetpEntry.iPlanQty - _MORoutingSetpEntry.iRptQty - _MORoutingSetpEntry.iUnConfirmQty - _MORoutingSetpEntry.iUnVerifyQty;


                        #region 工步报工记录
                        //提取工步对应的报工记录 FOpSeq,FStepCode
                        DataRow[] dr_RptList2 = dt_RptList.Select("FOpSeq= '" + dr["OpSeq"].ToString() + "' AND FStepCode ='"+ dr_Setp["cStepCode"] + "'  ");
                        List<DAPI.DAPI_QueryMORptDetail> ls_MORptDetail2 = new List<DAPI.DAPI_QueryMORptDetail>();

                        foreach (DataRow drr in dr_RptList2)
                        {
                            DAPI.DAPI_QueryMORptDetail _QueryMORptDetail = new DAPI.DAPI_QueryMORptDetail();
                            _QueryMORptDetail.RptDate = drr["FRptDate"].ToString();
                            _QueryMORptDetail.Maker = drr["FMaker"].ToString();

                            if (bool.Parse(drr["FIsNeedConfirm"].ToString()))
                            {
                                _QueryMORptDetail.iUnConfirmQty = decimal.Parse(drr["FRptQty"].ToString());
                            }
                            else
                            {
                                _QueryMORptDetail.iRptQty = decimal.Parse(drr["FRptQty"].ToString());
                                _QueryMORptDetail.iGoodQty = decimal.Parse(drr["FGoodQty"].ToString());
                                _QueryMORptDetail.iBadQty = decimal.Parse(drr["FBadQty"].ToString());
                            }

                            if (bool.Parse(drr["FIsNeedVerify"].ToString()))
                            {
                                _QueryMORptDetail.iUnVerifyQty = decimal.Parse(drr["FUnVerifyQty"].ToString());
                            }

                            ls_MORptDetail2.Add(_QueryMORptDetail);
                        }
                        #endregion

                        _MORoutingSetpEntry.MORptDetail = ls_MORptDetail2;


                        ls_MORoutingEntry.Add(_MORoutingSetpEntry);
                    }

                    #endregion
                }

                _MORouting.MOProcess = ls_MORoutingEntry;
                //LogHelper.WriteErrLog(0, "准备序列化");
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
                ,[FOutMoRoutingDid],[FWorkCenterID],[FEmpCode],[FIsBFFlag],[FIsVerifyFlag],FIsConfirmSign) 
                SELECT '{0}','{1}','{2}','{3}','{4}',
                '{5}','{6}','{7}','{8}','{9}',
                GETDATE(),'{10}','{11}','{12}','{13}',
                '{14}','{15}','{16}','{17}','{18}',
                '{19}','{20}','{21}','{22}',
                '{23}','{24}','{25}','{26}','{27}',1",
               v_str_Guid, _DAPI_MORpt.FMoID, _DAPI_MORpt.FModID, _DAPI_MORpt.FMoRoutingId, _DAPI_MORpt.FMoRoutingDId,
               _DAPI_MORpt.FProcessID, _DAPI_MORpt.FOpSeq, _DAPI_MORpt.FOpName, _DAPI_MORpt.FStepCode, _DAPI_MORpt.FStepName,
               _DAPI_MORpt.FIsFirstFlag, _DAPI_MORpt.FIsLastFlag, _DAPI_MORpt.FIsExceed, _DAPI_MORpt.FUserID,
               _DAPI_MORpt.FPlanQty, _DAPI_MORpt.FRptQty, _DAPI_MORpt.FGoodQty, _DAPI_MORpt.FBadQty, 0,
               _DAPI_MORpt.FReason, 0, 0, _DAPI_MORpt.FPositionCode,
                _DAPI_MORpt.FOutMoRoutingDid, _DAPI_MORpt.FWorkCenterID, _DAPI_MORpt.FEmpCode, _DAPI_MORpt.FIsBFFlag, _DAPI_MORpt.FIsVerifyFlag);

                if (ZYSoft.DB.BLL.Common.ExecuteNonQuery(v_str_sql,ref v_str_ErrMsg) > 0)
                {
                    //调接口送入汇报单
                    //如果是工步则不需要调接口生单
                    //如果是超额则不需要调接口生单，后续将重先进行写入记录并生单
                    //调用接口文件，生成U8单据

                    List<string> ls_Sql = new List<string>();

                    if (_DAPI_MORpt.FStepCode == "")
                    {
                        v_str_sql = "EXEC [ZYSoft_LYYL_2019].[dbo].P_ZYSoft_CallU8Service  'Build_MORpt', '" + v_str_Guid + "' ";
                        //string v_str_U8Result = ZYSoft.DB.BLL.Common.ExecuteScalar(v_str_sql);

                        DataSet ds = ZYSoft.DB.BLL.Common.GetDataSetByTime(v_str_sql, 60 * 1000);

                        if (ds == null)
                        {
                            return returnResult("", "生成报工单失败", null);
                        }

                        string v_str_U8Result = ds.Tables[0].Rows[0]["FResult"].ToString();

                        if (v_str_U8Result != "Y")
                        {
                            return returnResult("", "生成报工单失败," + v_str_U8Result, null);
                        }


                        //ZYSoft_LYYL_U8API_V125.clsFC _U8Interface = new ZYSoft_LYYL_U8API_V125.clsFC();


                        //string v_str_U8BillID = "", v_str_U8BillNo = "";

                        //if (_U8Interface.BuildU8Bills("574B7C58-3ED8-4D54-93B4-BDB5222174C2",
                        //                        _DAPI_MORpt.FAccountNo,
                        //                        DateTime.Now.Year.ToString(), //ConfigurationManager.AppSettings["AccountYear"],
                        //                        _DAPI_MORpt.FUserID,
                        //                        _DAPI_MORpt.FUserPwd,
                        //                        v_str_Guid, "FC91", 1, DateTime.Now.ToString("yyyy-MM-dd"), "ZYSoft_LYYL_2019",
                        //                         ref v_str_U8BillID, ref v_str_U8BillNo, ref v_str_ErrMsg))
                        //{
                        //    return returnResult("", "生成报工单失败," + v_str_ErrMsg, null);
                        //}


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
                   
                    if (_DAPI_MORpt.FStepCode != "")
                    {
                        //更新生单成功标记
                        v_str_sql = string.Format(@"UPDATE [ZYSoft_LYYL_2019].[dbo].[t_MOReportList] SET FIsBuildFC=1,
                        FFCBillID = '{0}', FFCBillNo = '{1}' WHERE FGuid = '{2}'",
                        0, "工步报工", v_str_Guid);
                        ls_Sql.Add(v_str_sql);
                    }


                    //更新记录标记
                    v_str_sql = string.Format(@"UPDATE [ZYSoft_LYYL_2019].[dbo].[t_MOReportList] SET FIsFinishConfirm=1,
                    FConfirmer = '{0}', FConfirmDate = GETDATE() ,FConfirmGuid ='{1}'  WHERE FID = {2}",
                    _DAPI_MORpt.FUserID, v_str_Guid, _DAPI_MORpt.FID);
                    ls_Sql.Add(v_str_sql);
                   

                    //更新数量，先将未确认的减去  原记录中的  一次确认完成 可能存在实际确认数量与计划确认数量不一致的情况
                    v_str_sql = string.Format(@"UPDATE T1 SET T1.FUnConfirmQty = ISNULL(T1.FUnConfirmQty, 0) - T2.FRptQty
                    FROM ZYSoft_LYYL_2019.dbo.t_MOReportQty T1 JOIN ZYSoft_LYYL_2019.dbo.t_MOReportList t2 on t1.FMOID = T2.FMOID
                    AND T1.FMoDId = T2.FMoDId AND T1.FMoRoutingDId = T2.FMoRoutingDId AND T1.FOpSeq = T2.FOpSeq AND T1.FStepCode=T2.FStepCode
                    WHERE T2.FID = '{0}'", _DAPI_MORpt.FID);
                    ls_Sql.Add(v_str_sql);

                    //将报工数量、合格、不合格 数量 更新进汇总表
                    v_str_sql = string.Format(@"UPDATE T1 SET T1.FRptQty = ISNULL(T1.FRptQty, 0) + T2.FRptQty,
                    T1.FGoodQty = ISNULL(T1.FGoodQty, 0) + T2.FGoodQty,
                    T1.FBadQty = ISNULL(T1.FBadQty, 0) + T2.FBadQty
                    FROM ZYSoft_LYYL_2019.dbo.t_MOReportQty T1 JOIN ZYSoft_LYYL_2019.dbo.t_MOReportList t2 on t1.FMOID = T2.FMOID
                    AND T1.FMoDId = T2.FMoDId AND T1.FMoRoutingDId = T2.FMoRoutingDId AND T1.FOpSeq = T2.FOpSeq AND T1.FStepCode=T2.FStepCode
                    WHERE T2.FGuid = '{0}'", v_str_Guid);
                    ls_Sql.Add(v_str_sql);

                    //将报工数量更新对工序计划表中
                    v_str_sql = string.Format(@"UPDATE T1 SET T1.ReportQty = ISNULL(T1.ReportQty, 0) + T2.FRptQty
                    FROM sfc_moroutingdetail T1 JOIN ZYSoft_LYYL_2019.dbo.t_MOReportList t2 on t1.MoRoutingId = T2.FMoRoutingId
                    AND T1.MoRoutingDId = T2.FMoRoutingDId AND T1.OperationId = T2.FMoRoutingId 
                    WHERE T2.FGuid = '{0}'  ", v_str_Guid);
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
                ,[FOutMoRoutingDid],[FWorkCenterID],[FEmpCode],[FIsBFFlag],[FIsVerifyFlag],FIsVerifySign) 
                SELECT '{0}','{1}','{2}','{3}','{4}',
                '{5}','{6}','{7}','{8}','{9}',
                GETDATE(),'{10}','{11}','{12}','{13}',
                '{14}','{15}','{16}','{17}','{18}',
                '{19}','{20}','{21}','{22}',
                '{23}','{24}','{25}','{26}','{27}',1",
               v_str_Guid, _DAPI_MORpt.FMoID, _DAPI_MORpt.FModID, _DAPI_MORpt.FMoRoutingId, _DAPI_MORpt.FMoRoutingDId,
               _DAPI_MORpt.FProcessID, _DAPI_MORpt.FOpSeq, _DAPI_MORpt.FOpName, _DAPI_MORpt.FStepCode, _DAPI_MORpt.FStepName,
               _DAPI_MORpt.FIsFirstFlag, _DAPI_MORpt.FIsLastFlag, _DAPI_MORpt.FIsExceed, _DAPI_MORpt.FUserID,
               _DAPI_MORpt.FPlanQty, _DAPI_MORpt.FRptQty, _DAPI_MORpt.FGoodQty, _DAPI_MORpt.FBadQty, 0,
               _DAPI_MORpt.FReason, 0, 0, _DAPI_MORpt.FPositionCode,
               _DAPI_MORpt.FOutMoRoutingDid, _DAPI_MORpt.FWorkCenterID,_DAPI_MORpt.FEmpCode, _DAPI_MORpt.FIsBFFlag, _DAPI_MORpt.FIsVerifyFlag);

                if (ZYSoft.DB.BLL.Common.ExecuteNonQuery(v_str_sql, ref v_str_ErrMsg) > 0)
                {
                    //调接口送入汇报单
                    //如果是工步则不需要调接口生单
                    //如果是超额则不需要调接口生单，后续将重先进行写入记录并生单

                    string v_str_U8BillID = "", v_str_U8BillNo = "";

                    List<string> ls_Sql = new List<string>();
                    if (_DAPI_MORpt.FStepCode == "")
                    {
                        v_str_sql = "EXEC [ZYSoft_LYYL_2019].[dbo].P_ZYSoft_CallU8Service  'Build_MORpt', '" + v_str_Guid + "' ";
                        //string v_str_U8Result = ZYSoft.DB.BLL.Common.ExecuteScalar(v_str_sql);
                        DataSet ds = ZYSoft.DB.BLL.Common.GetDataSetByTime(v_str_sql, 60 * 1000);

                        if (ds == null)
                        {
                            return returnResult("", "生成报工单失败", null);
                        }

                        string v_str_U8Result = ds.Tables[0].Rows[0]["FResult"].ToString();

                        if (v_str_U8Result != "Y")
                        {
                            return returnResult("", "生成报工单失败," + v_str_U8Result, null);
                        }


                        ////调用接口文件，生成U8单据
                        //ZYSoft_LYYL_U8API_V125.clsFC _U8Interface = new ZYSoft_LYYL_U8API_V125.clsFC();



                        //if (_U8Interface.BuildU8Bills("574B7C58-3ED8-4D54-93B4-BDB5222174C2",
                        //                        _DAPI_MORpt.FAccountNo,
                        //                        DateTime.Now.Year.ToString(), //ConfigurationManager.AppSettings["AccountYear"],
                        //                        _DAPI_MORpt.FUserID,
                        //                        _DAPI_MORpt.FUserPwd,
                        //                        v_str_Guid, "FC91", 1, DateTime.Now.ToString("yyyy-MM-dd"), "ZYSoft_LYYL_2019",
                        //                         ref v_str_U8BillID, ref v_str_U8BillNo, ref v_str_ErrMsg))
                        //{
                        //    return returnResult("", "生成报工单失败," + v_str_ErrMsg, null);
                        //}

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

                        v_str_U8BillID = dt_FC.Rows[0]["MID"].ToString();
                        //更新生单成功标记
                        v_str_sql = string.Format(@"UPDATE [ZYSoft_LYYL_2019].[dbo].[t_MOReportList] SET FIsBuildFC=1,
                    FFCBillID = '{0}', FFCBillNo = '{1}' WHERE FGuid = '{2}'",
                        dt_FC.Rows[0]["MID"].ToString(), dt_FC.Rows[0]["cVouchCode"].ToString(), v_str_Guid);
                        ls_Sql.Add(v_str_sql);
                    }

                    if (_DAPI_MORpt.FStepCode != "")
                    {
                        //更新生单成功标记
                        v_str_sql = string.Format(@"UPDATE [ZYSoft_LYYL_2019].[dbo].[t_MOReportList] SET FIsBuildFC=1,
                        FFCBillID = '{0}', FFCBillNo = '{1}' WHERE FGuid = '{2}'",
                        0, "工步报工", v_str_Guid);
                        ls_Sql.Add(v_str_sql);
                    }

                    //更新记录标记
                    v_str_sql = string.Format(@"UPDATE [ZYSoft_LYYL_2019].[dbo].[t_MOReportList] SET FIsFinishVerify=1,
                    FVerifyer = '{0}', FVerifyDate = GETDATE(),FVerifyGuid ='{1}' WHERE FID = {2}",
                    _DAPI_MORpt.FUserID, v_str_Guid, _DAPI_MORpt.FID);
                    ls_Sql.Add(v_str_sql);
                    

                    //更新数量，先将未评审的减去  原记录中的   一次评审完成
                    v_str_sql = string.Format(@"UPDATE T1 SET T1.FUnVerifyQty = ISNULL(T1.FUnVerifyQty, 0) - T2.FUnVerifyQty
                    FROM ZYSoft_LYYL_2019.dbo.t_MOReportQty T1 JOIN ZYSoft_LYYL_2019.dbo.t_MOReportList t2 on t1.FMOID = T2.FMOID
                    AND T1.FMoDId = T2.FMoDId AND T1.FMoRoutingDId = T2.FMoRoutingDId AND T1.FOpSeq = T2.FOpSeq AND T1.FStepCode=T2.FStepCode
                    WHERE T2.FID = '{0}' ", _DAPI_MORpt.FID);
                    ls_Sql.Add(v_str_sql);

                    //将报工数量、合格、不合格 数量 更新进汇总表
                    v_str_sql = string.Format(@"UPDATE T1 SET T1.FRptQty = ISNULL(T1.FRptQty, 0) + T2.FRptQty,
                    T1.FGoodQty = ISNULL(T1.FGoodQty, 0) + T2.FGoodQty,
                    T1.FBadQty = ISNULL(T1.FBadQty, 0) + T2.FBadQty
                    FROM ZYSoft_LYYL_2019.dbo.t_MOReportQty T1 JOIN ZYSoft_LYYL_2019.dbo.t_MOReportList t2 on t1.FMOID = T2.FMOID
                    AND T1.FMoDId = T2.FMoDId AND T1.FMoRoutingDId = T2.FMoRoutingDId AND T1.FOpSeq = T2.FOpSeq AND T1.FStepCode=T2.FStepCode
                    WHERE T2.FGuid = '{0}'", v_str_Guid);
                    ls_Sql.Add(v_str_sql);


                    //将报工数量更新对工序计划表中
                    v_str_sql = string.Format(@"UPDATE T1 SET T1.ReportQty = ISNULL(T1.ReportQty, 0) + T2.FRptQty
                    FROM sfc_moroutingdetail T1 JOIN ZYSoft_LYYL_2019.dbo.t_MOReportList t2 on t1.MoRoutingId = T2.FMoRoutingId
                    AND T1.MoRoutingDId = T2.FMoRoutingDId AND T1.OperationId = T2.FMoRoutingId 
                    WHERE T2.FGuid = '{0}'  ", v_str_Guid);
                    ls_Sql.Add(v_str_sql);

                    if (ZYSoft.DB.BLL.Common.ExecuteSQLTran(ls_Sql, ref v_str_ErrMsg) > 0)
                    {
                        //最后一道工序提交给WMS                        
                        if (_DAPI_MORpt.FIsLastFlag)
                        {
                            #region 末道[入序]工序则送入WMS
                            if (_DAPI_MORpt.FOpName.Equals("入库"))
                            {
                                v_str_sql = "EXEC [ZYSoft_LYYL_2019].[dbo].P_ZYSoft_CallWMSService  'Build_StockIn10','" + v_str_U8BillID + "','' ";
                                ZYSoft.DB.BLL.Common.ExecuteNonQuery(v_str_sql);
                            }
                            #endregion

                            #region 末道是[车间流转] 焊接工序时生成产成品入库   现场周转库 13
                            if (_DAPI_MORpt.FOpName.Equals("车间流转"))
                            {
                                //保存
                                v_str_sql = string.Format(@"INSERT INTO [{0}].[dbo].[t_PDARecord]
                                    ([FID],[FIdentifier],[FAccountID],[FYear],[FBillType]
                                    ,[FDate],[FDepCode],[FWHCode],[FMaker],[FROB]
                                    ,[FRemark],[FInvCode],[FComUnitCode],[FSTComUnitCode],[FChangRate]
                                    ,[FBatch],[FQty],[FNum],[FSourceQty],[FSourceNum]
                                    ,[FSourceBillID],[FSourceBillNo],[FSourceBillEntryID],[FSourceBillEntryNo]
                                    ,[FProInvCode],[FProBatch],[FProQty],[FSource],[FRdCode]
                                    ,[FcFree1]
                                     )
                                    SELECT NEWID(),'{1}','{2}','{3}','{4}',
                                    '{5}', T1.MDeptCode,'{6}','{7}','{8}',
                                    '{9}',T1.InvCode,t2.cComUnitCode ,t2.cSTComUnitCode,0,
                                    t1.MoLotCode,'{10}',0,ISNULL(T1.Qty,0) - ISNULL(T1.QualifiedInQty,0),  0 ,
                                    t1.moid,t0.mocode,t1.MoDId,t1.SortSeq,
                                    t1.InvCode, t1.MoLotCode,t1.qty, '生产订单','102',  
                                    t1.Free1  
                                    FROM mom_order t0 join  mom_orderdetail T1 on t0.moid=t1.moid
                                    LEFT JOIN dbo.Inventory T2 ON T1.InvCode=T2.cInvCode                                    
                                    WHERE T1.MoDId= {11}",
                                       "ZYSoft_LYYL_2019",
                                       v_str_Guid, _DAPI_MORpt.FAccountNo, DateTime.Now.Year, "0411",
                                      DateTime.Now.ToString("yyyy-MM-dd"), "13", _DAPI_MORpt.FUserID, 1,
                                      "报工自动入库",
                                      _DAPI_MORpt.FGoodQty,                                      
                                      _DAPI_MORpt.FModID);

                                if (ZYSoft.DB.BLL.Common.ExecuteNonQuery(v_str_sql) == -1)
                                {
                                    LogHelper.WriteErrLog(0, "报工自动入库 缓存失败");
                                }
                                else
                                {
                                    v_str_sql = "EXEC [ZYSoft_LYYL_2019].[dbo].P_ZYSoft_CallU8Service  'Build_MORdReocrd', '" + v_str_Guid + "' ";
                                    string v_str_U8Result2 = ZYSoft.DB.BLL.Common.ExecuteScalar(v_str_sql);

                                    if (v_str_U8Result2 != "Y")
                                    {
                                        return returnResult("", "生成产成品入库失败," + v_str_U8Result2, null);
                                    }


                                    //string v_str_U8BillID = "", v_str_U8BillNo = "";
                                    //ZYSoft_LYYL_U8API_V125.clsOtherInOut _U8Interface2 = new ZYSoft_LYYL_U8API_V125.clsOtherInOut();

                                    //if (_U8Interface2.BuildU8Bills("574B7C58-3ED8-4D54-93B4-BDB5222174C2",
                                    //           _DAPI_MORpt.FAccountNo,
                                    //           DateTime.Now.Year.ToString(), //ConfigurationManager.AppSettings["AccountYear"],
                                    //           _DAPI_MORpt.FUserID,
                                    //           _DAPI_MORpt.FUserPwd,
                                    //           v_str_Guid, "0411", 1, DateTime.Now.ToString("yyyy-MM-dd"), "ZYSoft_LYYL_2019",
                                    //            ref v_str_U8BillID, ref v_str_U8BillNo, ref v_str_ErrMsg))
                                    //{
                                    //    LogHelper.WriteErrLog(0, "生成产成品入库失败," + v_str_ErrMsg);
                                    //}
                                }
                            }
                            #endregion
                        }
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

                string v_str_sql = "", v_str_Guid = ""; //= Guid.NewGuid().ToString();

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

                    v_str_sql = "EXEC [ZYSoft_LYYL_2019].[dbo].P_ZYSoft_CallU8Service  'Del_MORpt', '" + dt_Record.Rows[0]["FFCBillID"].ToString() + "' ";
                    string v_str_U8Result = ZYSoft.DB.BLL.Common.ExecuteScalar(v_str_sql);

                    if (v_str_U8Result != "Y")
                    {
                        return returnResult("", "删除报工单失败," + v_str_U8Result, null);
                    }

                    //ZYSoft_LYYL_U8API_V125.clsFC _U8Interface = new ZYSoft_LYYL_U8API_V125.clsFC();

                    //if (_U8Interface.DeleteU8Bills("574B7C58-3ED8-4D54-93B4-BDB5222174C2",
                    //                        _DAPI_DelMORpt.FAccountNo,
                    //                        DateTime.Now.Year.ToString(), //ConfigurationManager.AppSettings["AccountYear"],
                    //                        _DAPI_DelMORpt.FUserID,
                    //                        _DAPI_DelMORpt.FUserPwd,
                    //                        _DAPI_DelMORpt.FID, "FC91", DateTime.Now.ToString("yyyy-MM-dd"), "ZYSoft_LYYL_2019",
                    //                        ref v_str_ErrMsg))
                    //{
                    //    return returnResult("", "生成报工单失败," + v_str_ErrMsg, null);
                    //}

                    v_str_sql = string.Format(@"SELECT FGuid FROM  [ZYSoft_LYYL_2019].[dbo].[t_MOReportList] WHERE FID = {0} ",
                     _DAPI_DelMORpt.FID );
                    v_str_Guid = ZYSoft.DB.BLL.Common.ExecuteScalar(v_str_sql);

                    List<string> ls_Sql = new List<string>();

                    //更新检验合格数量
                    v_str_sql = string.Format(@" UPDATE T1 SET 
                    T1.FGoodQty=T3.BalQualifiedQty,
                    T1.FBadQty=T1.FRptQty - T3.BalQualifiedQty  
                     FROM ZYSoft_LYYL_2019..t_MOReportQty T1 JOIN ZYSoft_LYYL_2019.dbo.t_MOReportList t2 on t1.FMOID = T2.FMOID
                    AND T1.FMoDId = T2.FMoDId AND T1.FMoRoutingDId = T2.FMoRoutingDId AND T1.FOpSeq = T2.FOpSeq AND T1.FStepCode=T2.FStepCode
                    JOIN sfc_moroutingdetail T3 ON T1.FMOID=T3.MOID AND T1.FMoDId=T3.modid AND T1.FMoRoutingId =T3.MoRoutingId AND T1.FMoRoutingDId= T3.MoRoutingDId
                     WHERE  T3.Define25='是' AND T2.FID={0} ", _DAPI_DelMORpt.FID);
                    ZYSoft.DB.BLL.Common.ExecuteNonQuery(v_str_sql);
                    ls_Sql.Add(v_str_sql);

                    //将报工数量、合格、不合格、待审核 数量 更新进汇总表
                    v_str_sql = string.Format(@"UPDATE T1 SET 
                    T1.FRptQty = ISNULL(T1.FRptQty, 0) - T2.FRptQty,
                    T1.FGoodQty = ISNULL(T1.FGoodQty, 0) - T2.FGoodQty,
                    T1.FBadQty = ISNULL(T1.FBadQty, 0) - T2.FBadQty               
                    FROM ZYSoft_LYYL_2019.dbo.t_MOReportQty T1 JOIN ZYSoft_LYYL_2019.dbo.t_MOReportList t2 on t1.FMOID = T2.FMOID
                    AND T1.FMoDId = T2.FMoDId AND T1.FMoRoutingDId = T2.FMoRoutingDId AND T1.FOpSeq = T2.FOpSeq AND T1.FStepCode=T2.FStepCode
                    WHERE T2.FID = '{0}' AND T2.FIsBuildFC=1 AND T2.FIsVerifyFlag=0  AND T2.FIsNeedConfirm=0", _DAPI_DelMORpt.FID);
                    ls_Sql.Add(v_str_sql);


                    //将待审核 数量 更新进汇总表
                    v_str_sql = string.Format(@"UPDATE T1 SET                    
                    T1.FUnVerifyQty = ISNULL(T1.FUnVerifyQty, 0) - T2.FUnVerifyQty              
                    FROM ZYSoft_LYYL_2019.dbo.t_MOReportQty T1 JOIN ZYSoft_LYYL_2019.dbo.t_MOReportList t2 on t1.FMOID = T2.FMOID
                    AND T1.FMoDId = T2.FMoDId AND T1.FMoRoutingDId = T2.FMoRoutingDId AND T1.FOpSeq = T2.FOpSeq AND T1.FStepCode=T2.FStepCode
                    WHERE T2.FID = '{0}' AND T2.FIsBuildFC=1 AND T2.FIsVerifyFlag=0 AND T2.FIsNeedVerify=1 ", _DAPI_DelMORpt.FID);
                    ls_Sql.Add(v_str_sql);

                    //将待确认 数量 更新进汇总表
                    v_str_sql = string.Format(@"UPDATE T1 SET                    
                    T1.FUnConfirmQty = ISNULL(T1.FUnConfirmQty, 0) - T2.FRptQty                 
                    FROM ZYSoft_LYYL_2019.dbo.t_MOReportQty T1 JOIN ZYSoft_LYYL_2019.dbo.t_MOReportList t2 on t1.FMOID = T2.FMOID
                    AND T1.FMoDId = T2.FMoDId AND T1.FMoRoutingDId = T2.FMoRoutingDId AND T1.FOpSeq = T2.FOpSeq AND T1.FStepCode=T2.FStepCode
                    WHERE T2.FID = '{0}' AND T2.FIsBuildFC=1 AND T2.FIsVerifyFlag=0 AND T2.FIsNeedConfirm=1 ", _DAPI_DelMORpt.FID);
                    ls_Sql.Add(v_str_sql);


                    //将报工数量、合格、不合格、待审核 数量 更新进汇总表
                    v_str_sql = string.Format(@"UPDATE T1 SET T1.FRptQty = ISNULL(T1.FRptQty, 0) - T2.FRptQty ,
                    T1.FBadQty=(ISNULL(T1.FRptQty, 0) - T2.FRptQty ) - T1.FGoodQty            
                    FROM ZYSoft_LYYL_2019.dbo.t_MOReportQty T1 JOIN ZYSoft_LYYL_2019.dbo.t_MOReportList t2 on t1.FMOID = T2.FMOID
                    AND T1.FMoDId = T2.FMoDId AND T1.FMoRoutingDId = T2.FMoRoutingDId AND T1.FOpSeq = T2.FOpSeq AND T1.FStepCode=T2.FStepCode
                    WHERE T2.FID = '{0}' AND T2.FIsBuildFC=1 AND T2.FIsVerifyFlag=1 ", _DAPI_DelMORpt.FID);
                    ls_Sql.Add(v_str_sql);


                   



                    //将待确认数量 更新进汇总表 找出原始汇报记录中的待确认数量
                    v_str_sql = string.Format(@"UPDATE T1 SET T1.FUnConfirmQty = ISNULL(T1.FUnConfirmQty, 0) + T2.FRptQty
                    FROM ZYSoft_LYYL_2019.dbo.t_MOReportQty T1 JOIN ZYSoft_LYYL_2019.dbo.t_MOReportList t2 on t1.FMOID = T2.FMOID
                    AND T1.FMoDId = T2.FMoDId AND T1.FMoRoutingDId = T2.FMoRoutingDId AND T1.FOpSeq = T2.FOpSeq AND T1.FStepCode=T2.FStepCode
                    WHERE T2.FConfirmGuid = '{0}'   ", v_str_Guid);
                    ls_Sql.Add(v_str_sql);


                    //将待评审数量 更新进汇总表 找出原始报工记录中的待评审数量
                    v_str_sql = string.Format(@"UPDATE T1 SET T1.FUnVerifyQty = ISNULL(T1.FUnVerifyQty, 0) + T2.FUnVerifyQty
                    FROM ZYSoft_LYYL_2019.dbo.t_MOReportQty T1 JOIN ZYSoft_LYYL_2019.dbo.t_MOReportList t2 on t1.FMOID = T2.FMOID
                    AND T1.FMoDId = T2.FMoDId AND T1.FMoRoutingDId = T2.FMoRoutingDId AND T1.FOpSeq = T2.FOpSeq AND T1.FStepCode=T2.FStepCode
                    WHERE T2.FVerifyGuid = '{0}'    ", v_str_Guid);
                    ls_Sql.Add(v_str_sql);


                    //将报工数量更新对工序计划表中
                    v_str_sql = string.Format(@"UPDATE T1 SET T1.ReportQty = ISNULL(T1.ReportQty, 0) - T2.FRptQty
                    FROM sfc_moroutingdetail T1 JOIN ZYSoft_LYYL_2019.dbo.t_MOReportList t2 on t1.MoRoutingId = T2.FMoRoutingId
                    AND T1.MoRoutingDId = T2.FMoRoutingDId AND T1.OperationId = T2.FMoRoutingId 
                    WHERE T2.FID = '{0}' AND T2.FIsBuildFC=1  ", _DAPI_DelMORpt.FID);
                    ls_Sql.Add(v_str_sql);


                    //恢复评审完成标记            
                    v_str_sql = string.Format(@"UPDATE [ZYSoft_LYYL_2019].[dbo].[t_MOReportList] SET FIsFinishVerify=0,
                    FVerifyer = '', FVerifyDate = NULL,FVerifyGuid = NULL WHERE FVerifyGuid = '{0}'",
                     v_str_Guid );
                    ls_Sql.Add(v_str_sql);

                    //恢复组长确认完成标记
                    v_str_sql = string.Format(@"UPDATE [ZYSoft_LYYL_2019].[dbo].[t_MOReportList] SET FIsFinishConfirm=0,
                    FConfirmer = '', FConfirmDate = NULL ,FConfirmGuid = NULL  WHERE FConfirmGuid = '{0}'",
                    v_str_Guid);
                    ls_Sql.Add(v_str_sql);

                  


                  
                    //删除报工记录
                    v_str_sql = string.Format(@"DELETE FROM ZYSoft_LYYL_2019.dbo.t_MOReportList 
                    WHERE FID = {0} ", _DAPI_DelMORpt.FID);
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


        /// <summary>
        /// 获取位置  迷
        /// </summary>
        /// <returns></returns>
        public string GetPosition()
        {
            try
            {
                ZYSoft.DB.Common.Configuration.ConnectionString = connStr;
                DataTable table = ZYSoft.DB.BLL.Common.ExecuteDataTable(string.Format("SELECT  FCode AS value ,FName AS label FROM ZYSoft_LYYL_2019.dbo.t_Place ", new object[0]));
                if (table == null)
                {
                    return BaseMethod.returnResult("", "未获取到位置信息", null);
                }
                if (table.Rows.Count == 0)
                {
                    return BaseMethod.returnResult("", "未获取到位置信息", null);
                }
                string str = JsonConvert.SerializeObject(table);
                return BaseMethod.returnResult("success", "success", str);
            }
            catch (Exception exception)
            {
                return BaseMethod.returnResult("", exception.Message, null);
            }
        }


        /// <summary>
        /// 获取导出数据
        /// </summary>
        /// <param name="v_str_Json"></param>
        /// <returns></returns>
        public string GetExportData(string v_str_Json)
        {
            try
            {
               
                string v_str_Result = "", v_str_sql = "", v_str_Columns = "";

                //解析
                DAPI.DAPI_Export _Input = JsonConvert.DeserializeObject<DAPI.DAPI_Export>(v_str_Json);

                foreach (DAPI.DAPI_Export.ExportColumn _Column in _Input.column)
                {
                    v_str_Columns = v_str_Columns+ _Column.field + " AS " + _Column.label + ",";
                }
                v_str_Columns = v_str_Columns.Substring(0, v_str_Columns.Length - 1);


                ZYSoft.DB.Common.Configuration.ConnectionString = connStr;

                if(_Input.reqType == "GetMORpt")  //导出可修改的工单记录
                {
                    v_str_sql = string.Format(@"SELECT {1} FROM (SELECT T4.MoCode,T2.SortSeq, T3.cInvCode,T3.cInvName,t3.cInvStd,t31.cComUnitName, T2.MDeptCode,t5.cDepName,T2.Qty iQty,t2.Define32 cProject,t2.Free1 cVersion, t6.StartDate,t6.DueDate,t2.MoLotCode, T1.*,t7.FName FPostion
                                        FROM ZYSoft_LYYL_2019.dbo.t_MOReportList T1 JOIN mom_orderdetail T2 ON T1.FMoDId = T2.MoDId AND T1.FMOID = T2.MoId
                                        LEFT JOIN dbo.Inventory T3 ON T2.InvCode = T3.cInvCode
										LEFT JOIN ComputationUnit t31 ON t3.cComUnitCode=t31.cComunitCode
                                        LEFT JOIN mom_order T4 ON T4.MoId = T2.MoId
										left outer join department t5 on t2.MDeptCode= t5.cDepCode
										LEFT JOIN mom_morder t6 ON t2.MoDId=t6.MoDId AND t2.moid=t6.MoId
										left outer join ZYSoft_LYYL_2019..t_Place t7 on t1.FPositionCode=t7.FCode
                                         WHERE {0}  ) T ", _Input.reqStr, v_str_Columns);

                }

                DataTable dt_Record = ZYSoft.DB.BLL.Common.ExecuteDataTable(v_str_sql);

                if (dt_Record == null)
                {
                    return returnResult("", "未获取到记录", null);
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
        /// 通用查询
        /// </summary>
        /// <param name="v_str_sql">sql语句</param>
        /// <returns></returns>
        public string ExecSql(string v_str_sql)
        {
            try
            {
                string v_str_Result = "";
                ZYSoft.DB.Common.Configuration.ConnectionString = connStr;
                DataTable dt_Record = ZYSoft.DB.BLL.Common.ExecuteDataTable(v_str_sql);
                if (dt_Record == null)
                {
                    return returnResult("", "未获取到记录", null);
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
    }
}
