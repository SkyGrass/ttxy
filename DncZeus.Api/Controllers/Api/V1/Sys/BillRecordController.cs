using AutoMapper;
using DncZeus.Api.Entities;
using DncZeus.Api.Entities.Enums;
using DncZeus.Api.Extensions;
using DncZeus.Api.Extensions.AuthContext;
using DncZeus.Api.Extensions.CustomException;
using DncZeus.Api.Models.Response;
using DncZeus.Api.RequestPayload.Base.Area;
using DncZeus.Api.RequestPayload.Sys;
using DncZeus.Api.Utils;
using DncZeus.Api.ViewModels.Base.DncArea;
using DncZeus.Api.ViewModels.Sys;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace DncZeus.Api.Controllers.Api.V1.Sys
{
    /// <summary>
    /// 
    /// </summary>
    //[CustomAuthorize]
    [Route("api/v1/sys/[controller]/[action]")]
    [ApiController]
    [CustomAuthorize]
    public class BillRecordController : ControllerBase
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private IHttpContextAccessor _accessor;

        private readonly DncZeusDbContext _dbContext;
        private readonly IMapper _mapper;


        public BillRecordController(DncZeusDbContext dbContext, IMapper mapper, IHttpContextAccessor accessor, IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            _accessor = accessor;
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult List(BillRecordRequestPayload payload)
        {
            using (_dbContext)
            {
                var query = _dbContext.T_Bill.AsQueryable();
                if (!string.IsNullOrEmpty(payload.Kw))
                {
                    query = query.Where(x => x.FOrderBillNo.Contains(payload.Kw.Trim()) ||
                    x.FClient.Contains(payload.Kw.Trim()) ||
                    x.FClientTel.Contains(payload.Kw.Trim()));
                }
                if (payload.IsCancelManageCost > CommonEnum.YesOrNo.All)
                {
                    query = query.Where(x => (CommonEnum.YesOrNo)(x.FIsCancelManageCost ? 1 : 0) ==
                        payload.IsCancelManageCost);
                }
                if (payload.IsClosed > CommonEnum.YesOrNo.All)
                {
                    query = query.Where(x => ((CommonEnum.YesOrNo)(x.FIsClosed ? 1 : 0)) == payload.IsClosed);
                }
                if (payload.FHospitalID > 0)
                {
                    query = query.Where(x => x.FHospitalID == payload.FHospitalID);
                }
                if (payload.FBedID > 0)
                {
                    query = query.Where(x => x.FBedID == payload.FBedID);
                }
                if (payload.FAreaID > 0)
                {
                    query = query.Where(x => x.FAreaID == payload.FAreaID);
                }
                if (payload.FManagerID > 0)
                {
                    query = query.Where(x => x.FManagerID == payload.FManagerID);
                }
                if (!string.IsNullOrEmpty(payload.FBeginDate))
                {
                    query = query.Where(x => x.FDate.CompareTo(DateTime.Parse(payload.FBeginDate)) >= 0);
                }
                if (!string.IsNullOrEmpty(payload.FEndDate))
                {
                    query = query.Where(x => x.FDate.CompareTo(DateTime.Parse(string.Format(@"{0} 23:59:59", payload.FEndDate))) <= 0);
                }

                var list = query.Paged(payload.CurrentPage, payload.PageSize).ToList();
                var totalCount = query.Count();
                var data = list.Select(_mapper.Map<T_Bill, BillRecordJsonModel>);
                var response = ResponseModelFactory.CreateResultInstance;
                response.SetData(data, totalCount);
                return Ok(response);
            }
        }

        [HttpGet("/api/v1/sys/billrecord/find_detail_by_id/{id}")]
        public IActionResult Detail(int id)
        {
            using (_dbContext)
            {
                var query = _dbContext.T_BillEntry.AsQueryable();
                query = query.Where(x => x.FID == id);
                var list = query.Paged().ToList();
                var totalCount = query.Count();
                var data = list.Select(_mapper.Map<T_BillEntry, BillRecordEntryJsonModel>);
                var response = ResponseModelFactory.CreateResultInstance;
                response.SetData(data);
                return Ok(response);
            }
        }
        [HttpGet("/api/v1/sys/billrecord/find_detailprice_by_id/{id}")]
        public IActionResult DetailPrice(int id)
        {
            using (_dbContext)
            {
                var query = _dbContext.T_BillPrice.AsQueryable();
                query = query.Where(x => x.FID == id);
                var list = query.Paged().ToList();
                var totalCount = query.Count();
                var data = list.Select(_mapper.Map<T_BillPrice, BillPriceEntryJsonModel>);
                var response = ResponseModelFactory.CreateResultInstance;
                response.SetData(data);
                return Ok(response);
            }
        }
        [HttpGet("/api/v1/sys/billrecord/find_detailperson_by_id/{id}")]
        public IActionResult DetailPerson(int id)
        {
            using (_dbContext)
            {
                var query = _dbContext.T_BillPerson.AsQueryable();
                query = query.Where(x => x.FID == id);
                var list = query.Paged().ToList();
                var totalCount = query.Count();
                var data = list.Select(_mapper.Map<T_BillPerson, BillPersonEntryJsonModel>);
                var response = ResponseModelFactory.CreateResultInstance;
                response.SetData(data);
                return Ok(response);
            }
        }
        [HttpGet("/api/v1/sys/billrecord/find_detailclient_by_id/{id}")]
        public IActionResult DetailClient(int id)
        {
            using (_dbContext)
            {
                var query = _dbContext.T_BillClient.AsQueryable();
                query = query.Where(x => x.FID == id);
                var list = query.Paged().ToList();
                var totalCount = query.Count();
                var data = list.Select(_mapper.Map<T_BillClient, BillClientEntryJsonModel>);
                var response = ResponseModelFactory.CreateResultInstance;
                response.SetData(data);
                return Ok(response);
            }
        }

        [HttpPost]
        public string exportExcel(BillRecordRequestPayload payload)
        {
            try
            {
                string queryStr = string.Empty;
                if (!string.IsNullOrEmpty(payload.Kw))
                {
                    queryStr += $"AND (FOrderBillNo like '%{payload.Kw}%' or FClientTel like '%{payload.Kw}%'  or FClient like '%{payload.Kw}%')";
                }

                if (payload.FHospitalID > 0)
                {
                    queryStr += $"AND  FHospitalID ='{payload.FHospitalID}'";
                }
                if (payload.FBedID > 0)
                {
                    queryStr += $"AND  FBedID ='{payload.FBedID}'";
                }
                if (payload.FAreaID > 0)
                {
                    queryStr += $"AND  FAreaID ='{payload.FAreaID}'";
                }
                if (payload.FManagerID > 0)
                {
                    queryStr += $"AND  FManagerID ='{payload.FManagerID}'";
                }
                if (!string.IsNullOrEmpty(payload.FBeginDate))
                {
                    queryStr += $"AND  FDate >='{payload.FBeginDate}'";
                }
                if (!string.IsNullOrEmpty(payload.FEndDate))
                {
                    queryStr += $"AND  FDate <='{payload.FEndDate} 23:59:59' ";
                }

                var fileType = "xlsx";
                var path = string.Format(@"{0}/excels", _hostingEnvironment.WebRootPath);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                var fileName = RandomHelper.GetRandomizer(10, true, false, true, false);
                path = string.Format(@"{0}/{1}.{2}", path, fileName, fileType);

                using (_dbContext)
                {
                    DataTable config = _dbContext.Database.SqlQuery(string.Format(@"SELECT * FROM dbo.DncViewConfig WHERE FViewId =1
                AND ISNULL(FIsClose,0)=0 ORDER BY FNo"));
                    DataTable configforSummary = _dbContext.Database.SqlQuery(string.Format(@"SELECT * FROM dbo.DncViewConfig WHERE FViewId =5
                AND ISNULL(FIsClose,0)=0 ORDER BY FNo"));
                    string columns = string.Empty;
                    string columnsforSummary = string.Empty;
                    if (config != null)
                    {
                        foreach (DataRow dr in config.Rows)
                        {
                            columns += $"{dr["FColName"]} as {dr["FLabelName"]},";
                        }

                        if (columns.EndsWith(","))
                        {
                            columns = columns.Substring(0, columns.Length - 1);
                        }
                    }
                    else
                    {
                        columns = "*";
                    }
                    if (configforSummary != null)
                    {
                        foreach (DataRow dr in configforSummary.Rows)
                        {
                            columnsforSummary += $"{dr["FColName"]} as {dr["FLabelName"]},";
                        }

                        if (columnsforSummary.EndsWith(","))
                        {
                            columnsforSummary = columnsforSummary.Substring(0, columnsforSummary.Length - 1);
                        }
                    }
                    else
                    {
                        columnsforSummary = "*";
                    }
                    DataTable source = _dbContext.Database.SqlQuery(string.Format(@"select {0} from vBillRecord where 1=1 {1}", columns, queryStr));
                    DataTable sourceSummary = _dbContext.Database.SqlQuery(string.Format(@"select {0} from vBillRecordSummary where 1=1 {1}", columnsforSummary, queryStr));
                    if (source != null && source.Rows.Count > 0 && sourceSummary != null && sourceSummary.Rows.Count > 0)
                    {
                        string errorMsg = "";

                        DataSet ds = new DataSet();
                        source.TableName = "明细记录";
                        sourceSummary.TableName = "汇总记录";
                        ds.Tables.Add(source);
                        ds.Tables.Add(sourceSummary);
                        List<int> result = new ExcelHelper(path).DataTableToExcel(ds, true, ref errorMsg);
                        if (result.Count > 0)
                        {
                            var fileBase64 = ExcelHelper.File2Base64(path);
                            if (!string.IsNullOrEmpty(fileBase64))
                            {
                                return JsonConvert.SerializeObject(new
                                {
                                    state = "success",
                                    msg = "",
                                    data = new
                                    {
                                        filename = string.Format(@"{0}.{1}", fileName, fileType),
                                        file = fileBase64
                                    }
                                });
                            }
                            else
                            {
                                return JsonConvert.SerializeObject(new
                                {
                                    state = "error",
                                    msg = "文件转化失败!"
                                });
                            }
                        }
                        else
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                state = "error",
                                msg = errorMsg
                            });
                        }
                    }
                    else
                    {
                        return JsonConvert.SerializeObject(new
                        {
                            state = "error",
                            msg = "没有查询到数据!",
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    state = "error",
                    msg = "导出数据发生异常!",
                });
            }
        }

        public string CheckList(BillCheckRequestPayload payload)
        {
            try
            {
                string queryStr = string.Empty;
                if (!string.IsNullOrEmpty(payload.Kw))
                {
                    queryStr += $"AND (FOrderBillNo like '%{payload.Kw}%' or FClientTel like '%{payload.Kw}%'  or FClient like '%{payload.Kw}%')";
                }

                if (payload.FHospitalID > 0)
                {
                    queryStr += $"AND  FHospitalID ='{payload.FHospitalID}'";
                }
                if (!string.IsNullOrEmpty(payload.FBeginDate))
                {
                    queryStr += $"AND  FDate >='{payload.FBeginDate}'";
                }
                if (!string.IsNullOrEmpty(payload.FEndDate))
                {
                    queryStr += $"AND  FDate <='{payload.FEndDate} 23:59:59 '";
                }
                if (payload.FIsConfirm > CommonEnum.IsConfirm.All)
                {
                    queryStr += $"AND FIsConfirm = {(payload.FIsConfirm == CommonEnum.IsConfirm.Yes ? 1 : 0)}";
                }
                using (_dbContext)
                {
                    DataTable source = _dbContext.Database.SqlQuery(string.Format(@"select * from vBillCheck where  1=1 {0}", queryStr));
                    bool flag = source != null && source.Rows.Count > 0;
                    return JsonConvert.SerializeObject(new
                    {
                        data = source,
                        state = flag ? "success" : "error",
                        msg = flag ? "" : "没有查询到数据!"
                    });
                }
            }
            catch (Exception)
            {
                return JsonConvert.SerializeObject(new
                {
                    data = new string[] { },
                    state = "error",
                    msg = "查询数据发生异常!",
                });
            }
        }

        public string CalcList(BillCalcRequestPayload payload)

        {
            try
            {
                string queryStr = string.Empty;
                if (!string.IsNullOrEmpty(payload.Kw))
                {
                    queryStr += $"AND (FOrderBillNo like '%{payload.Kw}%' or FClientTel like '%{payload.Kw}%'  or FClient like '%{payload.Kw}%')";
                }

                if (payload.FHospitalID > 0)
                {
                    queryStr += $"AND  FHospitalID ='{payload.FHospitalID}'";
                }
                if (!string.IsNullOrEmpty(payload.FBeginDate))
                {
                    queryStr += $"AND  FDate >='{payload.FBeginDate}'";
                }
                if (!string.IsNullOrEmpty(payload.FEndDate))
                {
                    queryStr += $"AND  FDate <='{payload.FEndDate} 23:59:59 '";
                }
                if (!string.IsNullOrEmpty(payload.FPBeginDate))
                {
                    queryStr += $"AND  FEnBeginDate >='{payload.FPBeginDate}'";
                }
                if (!string.IsNullOrEmpty(payload.FPEndDate))
                {
                    queryStr += $"AND  FEnEndDate <='{payload.FPEndDate} 23:59:59 '";
                }
                if (payload.FIsFinish > CommonEnum.IsFinish.All)
                {
                    queryStr += $"AND FIsFinishAccount = {(payload.FIsFinish == CommonEnum.IsFinish.Yes ? 1 : 0)}";
                }
                using (_dbContext)
                {
                    DataTable source = _dbContext.Database.SqlQuery(string.Format(@"select * from vBillCalc where  1=1 {0}", queryStr));
                    bool flag = source != null && source.Rows.Count > 0;
                    return JsonConvert.SerializeObject(new
                    {
                        data = source,
                        state = flag ? "success" : "error",
                        msg = flag ? "" : "没有查询到数据!"
                    });
                }
            }
            catch (Exception)
            {
                return JsonConvert.SerializeObject(new
                {
                    data = new string[] { },
                    state = "error",
                    msg = "查询数据发生异常!",
                });
            }
        }

        public string BuildList(BillBuildRequestPayload payload)
        {
            try
            {
                string queryStr = string.Empty;
                if (!string.IsNullOrEmpty(payload.Kw))
                {
                    queryStr += $"AND (FOrderBillNo like '%{payload.Kw}%' or FClientTel like '%{payload.Kw}%'  or FClient like '%{payload.Kw}%')";
                }

                if (payload.FHospitalID > 0)
                {
                    queryStr += $"AND  FHospitalID ='{payload.FHospitalID}'";
                }
                if (!string.IsNullOrEmpty(payload.FBeginDate))
                {
                    queryStr += $"AND  FDate >='{payload.FBeginDate}'";
                }
                if (!string.IsNullOrEmpty(payload.FEndDate))
                {
                    queryStr += $"AND  FDate <='{payload.FEndDate} 23:59:59 '";
                }
                if (payload.FIsBuild > CommonEnum.IsBuild.All)
                {
                    queryStr += $"AND FIsBuildPZ = {(payload.FIsBuild == CommonEnum.IsBuild.Yes ? 1 : 0)}";
                }
                using (_dbContext)
                {
                    DataTable source = _dbContext.Database.SqlQuery(string.Format(@"select * from vBillBuild where   FRecSum <>0  {0}", queryStr));
                    bool flag = source != null && source.Rows.Count > 0;
                    return JsonConvert.SerializeObject(new
                    {
                        data = source,
                        state = flag ? "success" : "error",
                        msg = flag ? "" : "没有查询到数据!"
                    });
                }
            }
            catch (Exception)
            {
                return JsonConvert.SerializeObject(new
                {
                    data = new string[] { },
                    state = "error",
                    msg = "查询数据发生异常!",
                });
            }
        }

        public string FeeList(FeeRecordRequestPayload payload)
        {
            try
            {
                string queryStr = string.Empty;
                if (!string.IsNullOrEmpty(payload.Kw))
                {
                    queryStr += $"AND (FOrderBillNo like '%{payload.Kw}%' or FClientTel like '%{payload.Kw}%'  or FClient like '%{payload.Kw}%')";
                }

                if (payload.FHospitalID > 0)
                {
                    queryStr += $"AND  FHospitalID ='{payload.FHospitalID}'";
                }
                if (payload.FBedID > 0)
                {
                    queryStr += $"AND  FBedID ='{payload.FBedID}'";
                }
                if (payload.FAreaID > 0)
                {
                    queryStr += $"AND  FAreaID ='{payload.FAreaID}'";
                }
                if (payload.FManagerID > 0)
                {
                    queryStr += $"AND  FManagerID ='{payload.FManagerID}'";
                }
                if (!string.IsNullOrEmpty(payload.FBeginDate))
                {
                    queryStr += $"AND  FBeginDate >='{payload.FBeginDate}'";
                }
                if (!string.IsNullOrEmpty(payload.FEndDate))
                {
                    queryStr += $"AND  FEndDate <='{payload.FEndDate} 23:59:59 '";
                }
                using (_dbContext)
                {
                    /*select * 
from(select ROW_NUMBER() OVER(ORDER BY fid desc) as counts from vFeeRecord
) as t*/
                    DataTable source = _dbContext.Database.SqlQuery(string.Format(@"select * 
                                            from(select *,ROW_NUMBER() OVER(ORDER BY fid desc) as counts from vFeeRecord where  1=1 {0}
                                            ) as t where  1=1  and  counts between {1} and {2}", queryStr, payload.PageSize * (payload.CurrentPage - 1),
                                           payload.CurrentPage * payload.PageSize));
                    bool flag = source != null && source.Rows.Count > 0;
                    DataTable source1 = _dbContext.Database.SqlQuery(string.Format(@"select *   from vFeeRecord  where  1=1   {0}", queryStr));
                    return JsonConvert.SerializeObject(new
                    {
                        totalCount = source1.Rows.Count,
                        data = source,
                        state = flag ? "success" : "error",
                        msg = flag ? "" : "没有查询到数据!"
                    });
                }
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    state = "error",
                    msg = "查询数据发生异常!",
                });
            }
        }

        public string PrepayList(PrepayRecordRequestPayload payload)
        {
            try
            {
                string queryStr = string.Empty;
                if (!string.IsNullOrEmpty(payload.Kw))
                {
                    queryStr += $"AND (FOrderBillNo like '%{payload.Kw}%' or FClientTel like '%{payload.Kw}%'  or FClient like '%{payload.Kw}%')";
                }

                if (payload.FHospitalID > 0)
                {
                    queryStr += $"AND  FHospitalID ='{payload.FHospitalID}'";
                }
                if (payload.FBedID > 0)
                {
                    queryStr += $"AND  FBedID ='{payload.FBedID}'";
                }
                if (payload.FAreaID > 0)
                {
                    queryStr += $"AND  FAreaID ='{payload.FAreaID}'";
                }
                if (payload.FManagerID > 0)
                {
                    queryStr += $"AND  FManagerID ='{payload.FManagerID}'";
                }
                if (!string.IsNullOrEmpty(payload.FBeginDate))
                {
                    queryStr += $"AND  FDate >='{payload.FBeginDate}'";
                }
                if (!string.IsNullOrEmpty(payload.FEndDate))
                {
                    queryStr += $"AND  FDate <='{payload.FEndDate} 23:59:59 '";
                }
                using (_dbContext)
                {
                    DataTable source = _dbContext.Database.SqlQuery(string.Format(@"select * 
                                            from(select *,ROW_NUMBER() OVER(ORDER BY fid desc) as counts from vBillYs where  1=1 {0}
                                            ) as t where  1=1  and  counts between {1} and {2}", queryStr, payload.PageSize * (payload.CurrentPage - 1),
                                           payload.CurrentPage * payload.PageSize));
                    bool flag = source != null && source.Rows.Count > 0;
                    DataTable source1 = _dbContext.Database.SqlQuery(string.Format(@"select *   from vBillYs  where  1=1   {0}", queryStr));
                    return JsonConvert.SerializeObject(new
                    {
                        totalCount = source1.Rows.Count,
                        data = source,
                        state = flag ? "success" : "error",
                        msg = flag ? "" : "没有查询到数据!"
                    });
                }
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    state = "error",
                    msg = "查询数据发生异常!",
                });
            }
        }

        public string FeeSummaryList(FeeRecordRequestPayload payload)
        {
            try
            {
                string queryStr = string.Empty;
                if (!string.IsNullOrEmpty(payload.Kw))
                {
                    queryStr += $"AND (FOrderBillNo like '%{payload.Kw}%' or FClientTel like '%{payload.Kw}%'  or FClient like '%{payload.Kw}%')";
                }

                if (payload.FHospitalID > 0)
                {
                    queryStr += $"AND  FHospitalID ='{payload.FHospitalID}'";
                }
                if (payload.FBedID > 0)
                {
                    queryStr += $"AND  FBedID ='{payload.FBedID}'";
                }
                if (payload.FAreaID > 0)
                {
                    queryStr += $"AND  FAreaID ='{payload.FAreaID}'";
                }
                if (payload.FManagerID > 0)
                {
                    queryStr += $"AND  FManagerID ='{payload.FManagerID}'";
                }
                if (!string.IsNullOrEmpty(payload.FBeginDate))
                {
                    queryStr += $"AND  FDate >='{payload.FBeginDate}'";
                }
                if (!string.IsNullOrEmpty(payload.FEndDate))
                {
                    queryStr += $"AND  FDate <='{payload.FEndDate} 23:59:59 '";
                }
                using (_dbContext)
                {
                    DataTable source = _dbContext.Database.SqlQuery(string.Format(@"select t1.*,
                                                                        (select top 1 FBeginPeriod 
                                                                        from vFeeRecord t2 where t1.FOrderBillNo=t2.FOrderBillNo and t1.FPcName=t2.FPcName and t1.FCareWayName=t2.FCareWayName 
                                                                        and t2.FBeginDate= t1.FBeginDate order by t2.FEntryID) FBeginPeriod,
                                                                        (select top 1 FEndPeriod from vFeeRecord t2 where t1.FOrderBillNo=t2.FOrderBillNo and t1.FPcName=t2.FPcName and t1.FCareWayName=t2.FCareWayName
                                                                         and t2.FEndDate= t1.FEndDate order by t2.FEntryID desc) FEndPeriod

                                                                        from (
                                                                        select FOrderBillNo,FHospitalName,FAreaName,FBedName,FManagerName,FPcName,
                                                                        FCareWayName,FCareProjectName, sum(FDay)FDay,sum(FCost) FCost,FClient,FClientTel,
                                                                        CONVERT(varchar,sum(FHospitalMgrCost)) FHospitalMgrCost,CONVERT(varchar,sum(FCompanyMgrCost))FCompanyMgrCost,
                                                                        SUM(FPersonCost)FPersonCost,sum(FHolidayCost)FHolidayCost ,sum(FTotalPersonCost)FTotalPersonCost,
                                                                        MIN(FBeginDate)FBeginDate,MAX(FEndDate) FEndDate
                                                                        from vFeeRecord  t1 where  1=1  {0}
                                                                        group by FOrderBillNo,FHospitalName,FAreaName,FBedName,FManagerName,FPcName,FCareWayName,FCareProjectName,FClient,FClientTel
                                                                        ) t1", queryStr));
                    bool flag = source != null && source.Rows.Count > 0;
                    return JsonConvert.SerializeObject(new
                    {
                        data = source,
                        state = flag ? "success" : "error",
                        msg = flag ? "" : "没有查询到数据!"
                    });
                }
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    state = "error",
                    msg = "查询数据发生异常!",
                });
            }
        }

        public string PcFeeList(PcFeeRecordRequestPayload payload)
        {
            try
            {
                string queryStr = string.Empty;
                if (!string.IsNullOrEmpty(payload.Kw))
                {
                    queryStr += $"AND (t4.Code like '%{payload.Kw}%' OR t4.Name like '%{payload.Kw}%')";
                }
                if (payload.FHospitalID > 0)
                {
                    queryStr += $"AND  t1.FHospitalID ='{payload.FHospitalID}'";
                }
                if (!string.IsNullOrEmpty(payload.FBeginDate))
                {
                    queryStr += $"AND  t2.FBeginDate >='{payload.FBeginDate}'";
                }
                if (!string.IsNullOrEmpty(payload.FEndDate))
                {
                    queryStr += $"AND t2.FEndDate <='{payload.FEndDate} 23:59:59 '";
                }
                using (_dbContext)
                {
                    DataTable source = _dbContext.Database.SqlQuery(string.Format(@" SELECT  SUM(ISNULL(t2.FHospitalMgrCost, 0)) AS FHospitalMgrCost, 
                SUM(ISNULL(t2.FCompanyMgrCost, 0)) AS FCompanyMgrCost, SUM(ISNULL(t2.FPersonCost, 0)) 
                AS FPersonCost, SUM(ISNULL(t2.FHolidayCost, 0)) AS FHolidayCost, 
                SUM(ISNULL(t2.FTotalPersonCost, 0)) AS FTotalPersonCost, t3.Name AS FHospitalName, 
                t4.Name AS FPcName, t4.Code AS FPcCode, SUM(ISNULL(t2.FDay, 0)) AS FDay, 
                SUM(ISNULL(t2.FCost, 0)) AS FCost, t2.FPersonID
FROM   dbo.t_Bill AS t1 LEFT OUTER JOIN
                dbo.t_BillDetail AS t2 ON t1.FID = t2.FID LEFT OUTER JOIN
                dbo.DncHospital AS t3 ON t1.FHospitalID = t3.Id LEFT OUTER JOIN
                dbo.DncPc AS t4 ON t2.FPersonID = t4.Id where 1=1 {0}
GROUP BY  t3.Name, t4.Name, t4.Code, t2.FPersonID", queryStr));
                    bool flag = source != null && source.Rows.Count > 0;
                    return JsonConvert.SerializeObject(new
                    {
                        totalCount = source.Rows.Count,
                        data = source,
                        state = flag ? "success" : "error",
                        msg = flag ? "" : "没有查询到数据!"
                    });
                }
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    state = "error",
                    msg = "查询数据发生异常!",
                });
            }
        }

        [HttpPost]
        public string exportExcelForCheck(BillCheckRequestPayload payload)
        {
            try
            {
                string queryStr = string.Empty;
                if (!string.IsNullOrEmpty(payload.Kw))
                {
                    queryStr += $"AND (FOrderBillNo like '%{payload.Kw}%' or FClientTel like '%{payload.Kw}%'  or FClient like '%{payload.Kw}%')";
                }

                if (payload.FHospitalID > 0)
                {
                    queryStr += $"AND  FHospitalID ='{payload.FHospitalID}'";
                }
                if (payload.FBedID > 0)
                {
                    queryStr += $"AND  FBedID ='{payload.FBedID}'";
                }
                if (payload.FAreaID > 0)
                {
                    queryStr += $"AND  FAreaID ='{payload.FAreaID}'";
                }
                if (payload.FManagerID > 0)
                {
                    queryStr += $"AND  FManagerID ='{payload.FManagerID}'";
                }
                if (!string.IsNullOrEmpty(payload.FBeginDate))
                {
                    queryStr += $"AND  FDate >='{payload.FBeginDate}'";
                }
                if (!string.IsNullOrEmpty(payload.FEndDate))
                {
                    queryStr += $"AND  FDate <='{payload.FEndDate} 23:59:59 '";
                }

                var fileType = "xlsx";
                var path = string.Format(@"{0}/excels", _hostingEnvironment.WebRootPath);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                var fileName = RandomHelper.GetRandomizer(10, true, false, true, false);
                path = string.Format(@"{0}/{1}.{2}", path, fileName, fileType);

                using (_dbContext)
                {
                    DataTable config = _dbContext.Database.SqlQuery(string.Format(@"SELECT * FROM dbo.DncViewConfig WHERE FViewId =3
                AND ISNULL(FIsClose,0)=0 ORDER BY FNo"));
                    string columns = string.Empty;
                    if (config != null)
                    {
                        foreach (DataRow dr in config.Rows)
                        {
                            columns += $"{dr["FColName"]} as {dr["FLabelName"]},";
                        }

                        if (columns.EndsWith(","))
                        {
                            columns = columns.Substring(0, columns.Length - 1);
                        }
                    }
                    else
                    {
                        columns = "*";
                    }
                    DataTable source = _dbContext.Database.SqlQuery(string.Format(@"select {0} from vBillCheck where 1=1 {1}", columns, queryStr));
                    if (source != null && source.Rows.Count > 0)
                    {
                        string errorMsg = "";
                        if (new ExcelHelper(path).DataTableToExcel(source, "页签1", true, ref errorMsg) > 0)
                        {
                            // excel to  base64
                            var fileBase64 = ExcelHelper.File2Base64(path);
                            if (!string.IsNullOrEmpty(fileBase64))
                            {
                                return JsonConvert.SerializeObject(new
                                {
                                    state = "success",
                                    msg = "",
                                    data = new
                                    {
                                        filename = string.Format(@"{0}.{1}", fileName, fileType),
                                        file = fileBase64
                                    }
                                });
                            }
                            else
                            {
                                return JsonConvert.SerializeObject(new
                                {
                                    state = "error",
                                    msg = "文件转化失败!"
                                });
                            }
                        }
                        else
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                state = "error",
                                msg = errorMsg
                            });
                        }
                    }
                    else
                    {
                        return JsonConvert.SerializeObject(new
                        {
                            state = "error",
                            msg = "没有查询到数据!",
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    state = "error",
                    msg = "导出数据发生异常!",
                });
            }
        }

        [HttpPost]
        public string exportExcelForFee(FeeRecordRequestPayload payload)
        {
            try
            {
                string queryStr = string.Empty;
                if (!string.IsNullOrEmpty(payload.Kw))
                {
                    queryStr += $"AND (FOrderBillNo like '%{payload.Kw}%' or FClientTel like '%{payload.Kw}%'  or FClient like '%{payload.Kw}%')";
                }

                if (payload.FHospitalID > 0)
                {
                    queryStr += $"AND  FHospitalID ='{payload.FHospitalID}'";
                }
                if (payload.FBedID > 0)
                {
                    queryStr += $"AND  FBedID ='{payload.FBedID}'";
                }
                if (payload.FAreaID > 0)
                {
                    queryStr += $"AND  FAreaID ='{payload.FAreaID}'";
                }
                if (payload.FManagerID > 0)
                {
                    queryStr += $"AND  FManagerID ='{payload.FManagerID}'";
                }
                if (!string.IsNullOrEmpty(payload.FBeginDate))
                {
                    queryStr += $"AND  FDate >='{payload.FBeginDate}'";
                }
                if (!string.IsNullOrEmpty(payload.FEndDate))
                {
                    queryStr += $"AND  FDate <='{payload.FEndDate} 23:59:59 '";
                }

                var fileType = "xlsx";
                var path = string.Format(@"{0}/excels", _hostingEnvironment.WebRootPath);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                var fileName = RandomHelper.GetRandomizer(10, true, false, true, false);
                path = string.Format(@"{0}/{1}.{2}", path, fileName, fileType);

                using (_dbContext)
                {
                    DataTable config = _dbContext.Database.SqlQuery(string.Format(@"SELECT * FROM dbo.DncViewConfig WHERE FViewId =2
                AND ISNULL(FIsClose,0)=0 ORDER BY FNo"));
                    string columns = string.Empty;
                    if (config != null)
                    {
                        foreach (DataRow dr in config.Rows)
                        {
                            columns += $"{dr["FColName"]} as {dr["FLabelName"]},";
                        }

                        if (columns.EndsWith(","))
                        {
                            columns = columns.Substring(0, columns.Length - 1);
                        }
                    }
                    else
                    {
                        columns = "*";
                    }
                    DataTable source = _dbContext.Database.SqlQuery(string.Format(@"select {0} from vFeeRecord where 1=1 {1}", columns, queryStr));
                    DataTable sourceSummary = _dbContext.Database.SqlQuery(string.Format(@"  SELECT FOrderBillNo'订单号',FHospitalName'医院',FAreaName'病区',FBedName'床位',FManagerName'管理老师',FPcName'护工',
                                                                                                                                                                    t.FCareWayName'护理方式',t.FCareProjectName'护理类型',t.FDay'护理天数',t.FCost'收款金额',t.FClient'患者姓名',t.FClientTel'联系方式',t.FHolidayCost'节日工资',
                                                                                                                                                                    t.FHospitalMgrCost'医院管理费',t.FCompanyMgrCost'公司管理费',
                                                                                                                                                                    t.FPersonCost '护工费',t.FTotalPersonCost '护工总费用',CONVERT(VARCHAR(10),t.FBeginDate,23)+'('+t.FBeginPeriod+')' '陪护开始日期',
                                                                                                                                                                    CONVERT(VARCHAR(10),t.FEndDate,23)+'('+t.FEndPeriod+')' '陪护结束日期' ,
                                                                                                                                                                    t.FTotalCost '护工总费用', SUBSTRING(T.FCostDetail,1,LEN(FCostDetail)-1) '护理费用说明' 
                                                                                                                                                                    FROM ( 
                                                                                                                                                                    SELECT t1.*,
                                                                                                                                                                    (select top 1 FBeginPeriod 
                                                                                                                                                                    from vFeeRecord t2 where t1.FOrderBillNo=t2.FOrderBillNo and t1.FPcName=t2.FPcName and t1.FCareWayName=t2.FCareWayName 
                                                                                                                                                                    and t2.FBeginDate= t1.FBeginDate order by t2.FEntryID) FBeginPeriod,
                                                                                                                                                                    (select top 1 FEndPeriod from vFeeRecord t2 where t1.FOrderBillNo=t2.FOrderBillNo and t1.FPcName=t2.FPcName and t1.FCareWayName=t2.FCareWayName
                                                                                                                                                                    and t2.FEndDate= t1.FEndDate order by t2.FEntryID desc) FEndPeriod 
                                                                                                                                                                    from (
                                                                                                                                                                    select FID,FOrderBillNo,FHospitalName,FAreaName,FBedName,FManagerName,FPcName,
                                                                                                                                                                    FCareWayName,FCareProjectName, sum(FDay)FDay,sum(FCost) FCost,FClient,FClientTel,
                                                                                                                                                                    CONVERT(varchar,sum(FHospitalMgrCost)) FHospitalMgrCost,CONVERT(varchar,sum(FCompanyMgrCost))FCompanyMgrCost,
                                                                                                                                                                    SUM(FPersonCost)FPersonCost,sum(FHolidayCost)FHolidayCost ,sum(FTotalPersonCost)FTotalPersonCost,
                                                                                                                                                                    CONVERT(varchar,sum(FHospitalMgrCost + FCompanyMgrCost + FPersonCost)) FTotalCost, 
                                                                                                                                                                    MIN(FBeginDate)FBeginDate,MAX(FEndDate) FEndDate,
                                                                                                                                                                    (SELECT CONVERT(VARCHAR,FPrice)+'X'+CONVERT(VARCHAR, FDay ) +'+'
                                                                                                                                                                    FROM (SELECT FID, FPrice,SUM(FDay) FDay FROM vFeeRecord U WHERE  u.FID=T1.FID 
                                                                                                                                                                    GROUP BY FID,FPrice) K FOR XML PATH('') ) FCostDetail 
                                                                                                                                                                    from vFeeRecord  t1 where  1=1  {0}
                                                                                                                                                                    group by FID,FOrderBillNo,FHospitalName,FAreaName,FBedName,FManagerName,FPcName,FCareWayName,FCareProjectName,FClient,FClientTel
                                                                                                                                                                    ) t1) AS t   ", queryStr));
                    if (source != null && source.Rows.Count > 0)
                    {
                        string errorMsg = "";
                        DataSet ds = new DataSet();
                        source.TableName = "明细记录";
                        sourceSummary.TableName = "汇总记录";
                        ds.Tables.Add(source);
                        ds.Tables.Add(sourceSummary);
                        List<int> result = new ExcelHelper(path).DataTableToExcel(ds, true, ref errorMsg);
                        if (result.Count() > 0)
                        {
                            // excel to  base64
                            var fileBase64 = ExcelHelper.File2Base64(path);
                            if (!string.IsNullOrEmpty(fileBase64))
                            {
                                return JsonConvert.SerializeObject(new
                                {
                                    state = "success",
                                    msg = "",
                                    data = new
                                    {
                                        filename = string.Format(@"{0}.{1}", fileName, fileType),
                                        file = fileBase64
                                    }
                                });
                            }
                            else
                            {
                                return JsonConvert.SerializeObject(new
                                {
                                    state = "error",
                                    msg = "文件转化失败!"
                                });
                            }
                        }
                        else
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                state = "error",
                                msg = errorMsg
                            });
                        }
                    }
                    else
                    {
                        return JsonConvert.SerializeObject(new
                        {
                            state = "error",
                            msg = "没有查询到数据!",
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    state = "error",
                    msg = "导出数据发生异常!",
                });
            }
        }

        [HttpPost]
        public string exportExcelForPrepay(PrepayRecordRequestPayload payload)
        {
            try
            {
                string queryStr = string.Empty;
                if (!string.IsNullOrEmpty(payload.Kw))
                {
                    queryStr += $"AND (FOrderBillNo like '%{payload.Kw}%' or FClientTel like '%{payload.Kw}%'  or FClient like '%{payload.Kw}%')";
                }

                if (payload.FHospitalID > 0)
                {
                    queryStr += $"AND  FHospitalID ='{payload.FHospitalID}'";
                }
                if (payload.FBedID > 0)
                {
                    queryStr += $"AND  FBedID ='{payload.FBedID}'";
                }
                if (payload.FAreaID > 0)
                {
                    queryStr += $"AND  FAreaID ='{payload.FAreaID}'";
                }
                if (payload.FManagerID > 0)
                {
                    queryStr += $"AND  FManagerID ='{payload.FManagerID}'";
                }
                if (!string.IsNullOrEmpty(payload.FBeginDate))
                {
                    queryStr += $"AND  FDate >='{payload.FBeginDate}'";
                }
                if (!string.IsNullOrEmpty(payload.FEndDate))
                {
                    queryStr += $"AND  FDate <='{payload.FEndDate} 23:59:59 '";
                }

                var fileType = "xlsx";
                var path = string.Format(@"{0}/excels", _hostingEnvironment.WebRootPath);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                var fileName = RandomHelper.GetRandomizer(10, true, false, true, false);
                path = string.Format(@"{0}/{1}.{2}", path, fileName, fileType);

                using (_dbContext)
                {
                    DataTable config = _dbContext.Database.SqlQuery(string.Format(@"SELECT * FROM dbo.DncViewConfig WHERE FViewId =4
                AND ISNULL(FIsClose,0)=0 ORDER BY FNo"));
                    string columns = string.Empty;
                    if (config != null)
                    {
                        foreach (DataRow dr in config.Rows)
                        {
                            columns += $"{dr["FColName"]} as {dr["FLabelName"]},";
                        }

                        if (columns.EndsWith(","))
                        {
                            columns = columns.Substring(0, columns.Length - 1);
                        }
                    }
                    else
                    {
                        columns = "*";
                    }
                    DataTable source = _dbContext.Database.SqlQuery(string.Format(@"select {0} from vBillYs where 1=1 {1}", columns, queryStr));
                    if (source != null && source.Rows.Count > 0)
                    {
                        string errorMsg = "";
                        if (new ExcelHelper(path).DataTableToExcel(source, "页签1", true, ref errorMsg) > 0)
                        {
                            // excel to  base64
                            var fileBase64 = ExcelHelper.File2Base64(path);
                            if (!string.IsNullOrEmpty(fileBase64))
                            {
                                return JsonConvert.SerializeObject(new
                                {
                                    state = "success",
                                    msg = "",
                                    data = new
                                    {
                                        filename = string.Format(@"{0}.{1}", fileName, fileType),
                                        file = fileBase64
                                    }
                                });
                            }
                            else
                            {
                                return JsonConvert.SerializeObject(new
                                {
                                    state = "error",
                                    msg = "文件转化失败!"
                                });
                            }
                        }
                        else
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                state = "error",
                                msg = errorMsg
                            });
                        }
                    }
                    else
                    {
                        return JsonConvert.SerializeObject(new
                        {
                            state = "error",
                            msg = "没有查询到数据!",
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    state = "error",
                    msg = "导出数据发生异常!",
                });
            }
        }

        [HttpPost]
        public string exportExcelForPcFee(PcFeeRecordRequestPayload payload)
        {
            try
            {
                string queryStr = string.Empty;
                if (!string.IsNullOrEmpty(payload.Kw))
                {
                    queryStr += $"AND (t4.Code like '%{payload.Kw}%' OR t4.Name like '%{payload.Kw}%')";
                }
                if (payload.FHospitalID > 0)
                {
                    queryStr += $"AND  t1.FHospitalID ='{payload.FHospitalID}'";
                }
                if (!string.IsNullOrEmpty(payload.FBeginDate))
                {
                    queryStr += $"AND  t2.FBeginDate >='{payload.FBeginDate}'";
                }
                if (!string.IsNullOrEmpty(payload.FEndDate))
                {
                    queryStr += $"AND  t2.FEndDate <='{payload.FEndDate} 23:59:59 '";
                }

                var fileType = "xlsx";
                var path = string.Format(@"{0}/excels", _hostingEnvironment.WebRootPath);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                var fileName = RandomHelper.GetRandomizer(10, true, false, true, false);
                path = string.Format(@"{0}/{1}.{2}", path, fileName, fileType);

                using (_dbContext)
                {
                    DataTable config = _dbContext.Database.SqlQuery(string.Format(@"SELECT * FROM dbo.DncViewConfig WHERE FViewId =6
                AND ISNULL(FIsClose,0)=0 ORDER BY FNo"));
                    string columns = string.Empty;
                    if (config != null)
                    {
                        foreach (DataRow dr in config.Rows)
                        {
                            columns += $"{dr["FColName"]} as {dr["FLabelName"]},";
                        }

                        if (columns.EndsWith(","))
                        {
                            columns = columns.Substring(0, columns.Length - 1);
                        }
                    }
                    else
                    {
                        columns = "*";
                    }

                    DataTable source = _dbContext.Database.SqlQuery(string.Format(@"select {0} from (SELECT  SUM(ISNULL(t2.FHospitalMgrCost, 0)) AS FHospitalMgrCost, 
                SUM(ISNULL(t2.FCompanyMgrCost, 0)) AS FCompanyMgrCost, SUM(ISNULL(t2.FPersonCost, 0)) 
                AS FPersonCost, SUM(ISNULL(t2.FHolidayCost, 0)) AS FHolidayCost, 
                SUM(ISNULL(t2.FTotalPersonCost, 0)) AS FTotalPersonCost, t3.Name AS FHospitalName, 
                t4.Name AS FPcName, t4.Code AS FPcCode, SUM(ISNULL(t2.FDay, 0)) AS FDay, 
                SUM(ISNULL(t2.FCost, 0)) AS FCost, t2.FPersonID
FROM   dbo.t_Bill AS t1 LEFT OUTER JOIN
                dbo.t_BillDetail AS t2 ON t1.FID = t2.FID LEFT OUTER JOIN
                dbo.DncHospital AS t3 ON t1.FHospitalID = t3.Id LEFT OUTER JOIN
                dbo.DncPc AS t4 ON t2.FPersonID = t4.Id where 1=1 {1}
GROUP BY  t3.Name, t4.Name, t4.Code, t2.FPersonID) as t", columns, queryStr));
                    if (source != null && source.Rows.Count > 0)
                    {
                        string errorMsg = "";
                        if (new ExcelHelper(path).DataTableToExcel(source, "护工工资汇总表", true, ref errorMsg) > 0)
                        {
                            // excel to  base64
                            var fileBase64 = ExcelHelper.File2Base64(path);
                            if (!string.IsNullOrEmpty(fileBase64))
                            {
                                return JsonConvert.SerializeObject(new
                                {
                                    state = "success",
                                    msg = "",
                                    data = new
                                    {
                                        filename = string.Format(@"{0}.{1}", fileName, fileType),
                                        file = fileBase64
                                    }
                                });
                            }
                            else
                            {
                                return JsonConvert.SerializeObject(new
                                {
                                    state = "error",
                                    msg = "文件转化失败!"
                                });
                            }
                        }
                        else
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                state = "error",
                                msg = errorMsg
                            });
                        }
                    }
                    else
                    {
                        return JsonConvert.SerializeObject(new
                        {
                            state = "error",
                            msg = "没有查询到数据!",
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    state = "error",
                    msg = "导出数据发生异常!",
                });
            }
        }

        [HttpPost]
        public ResponseModel UpdateBillIsStop(StopModel model)
        {
            using (_dbContext)
            {
                var parameters = model.ids.Split(",").Select((id, index) => new SqlParameter(string.Format("@p{0}", index), id)).ToList();
                var parameterNames = string.Join(", ", parameters.Select(p => p.ParameterName));
                var sql = string.Format(@"UPDATE t_Bill SET FIsStop=@FIsStop,FStopBillerID=@FStopBillerID,
                        FStopDate=GetDate() WHERE FID IN ({0})", parameterNames);
                parameters.Add(new SqlParameter("@FIsStop", model.isStop));
                parameters.Add(new SqlParameter("@FStopBillerID", model.userId));
                _dbContext.Database.ExecuteSqlCommand(sql, parameters);
                var response = ResponseModelFactory.CreateInstance;
                return response;
            }
        }


        [HttpPost]
        public ResponseModel UpdateBillIsConfirm(CheckModel model)
        {
            using (_dbContext)
            {
                var parameters = model.ids.Split(",").Select((id, index) => new SqlParameter(string.Format("@p{0}", index), id)).ToList();
                var parameterNames = string.Join(", ", parameters.Select(p => p.ParameterName));
                var sql = string.Format(@"UPDATE t_BillEntry SET FIsConfirm=@IsConfirm,FConfirmerID=@FConfirmerID,
                        FConfirmDate=GetDate() WHERE FEntryID IN ({0})", parameterNames);
                parameters.Add(new SqlParameter("@IsConfirm", (int)model.isConfirm));
                parameters.Add(new SqlParameter("@FConfirmerID", model.confirmerId));
                _dbContext.Database.ExecuteSqlCommand(sql, parameters);
                var response = ResponseModelFactory.CreateInstance;
                return response;
            }
        }

        [HttpPost]
        public IActionResult UpdateBillIsFinish(CalcModel model)
        {
            var response = ResponseModelFactory.CreateInstance;
            try
            {
                using (_dbContext)
                {
                    List<ReqModel> sql = new List<ReqModel>();
                    List<SqlParameter> parameters = new List<SqlParameter>();

                    model.calcId = Guid.NewGuid().ToString().ToLower();
                    var parameter = model.ids.Split(",").Select((id, index) => new SqlParameter(string.Format("@p{0}", index), id)).ToList();
                    var parameterNames = string.Join(", ", parameter.Select(p => p.ParameterName));

                    if (string.IsNullOrEmpty(model.beginDate) && string.IsNullOrEmpty(model.endDate))
                    {
                        model.multiple = 0;
                    }
                    parameters.AddRange(parameter);
                    parameters.Add(new SqlParameter("@FCalcID", model.calcId));
                    if (string.IsNullOrEmpty(model.beginDate))
                    {
                        SqlParameter parm = new SqlParameter("@FHolidayBeginDate", "");
                        parm.Value = DBNull.Value;
                        parameters.Add(parm);
                    }
                    else
                    {
                        parameters.Add(new SqlParameter("@FHolidayBeginDate", model.beginDate));
                    }
                    if (string.IsNullOrEmpty(model.endDate))
                    {
                        SqlParameter parm = new SqlParameter("@FHolidayEndDate", "");
                        parm.Value = DBNull.Value;
                        parameters.Add(parm);
                    }
                    else
                    {
                        parameters.Add(new SqlParameter("@FHolidayEndDate", model.endDate));
                    }
                    parameters.Add(new SqlParameter("@FMultiple", model.multiple));
                    sql.Add(new ReqModel()
                    {
                        sql = string.Format(@"UPDATE t_BillDetail SET FCalcID=@FCalcID,
                        FHolidayBeginDate=@FHolidayBeginDate , FHolidayEndDate=@FHolidayEndDate, FMultiple=@FMultiple WHERE FID IN ({0})", parameterNames),
                        parameters = parameters
                    });

                    parameters = new List<SqlParameter>();
                    parameter.Add(new SqlParameter("@FCalcID", model.calcId));
                    sql.Add(new ReqModel()
                    {
                        sql = string.Format(@"EXEC P_CalcCost @FCalcID", model.calcId),
                        parameters = parameter
                    });

                    _dbContext.Database.SetCommandTimeout(60 * 1000);
                    _dbContext.Database.BeginTransaction();
                    int effectRow = 0;
                    sql.ForEach(f =>
                    {
                        effectRow += _dbContext.Database.ExecuteSqlCommand(f.sql, f.parameters);
                    });
                    _dbContext.Database.CommitTransaction();
                    response.SetData("success");
                    response.SetSuccess();
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                _dbContext.Database.RollbackTransaction();
                response.SetData("error");
                response.SetFailed();
                return Ok(response);
            }
        }

        [HttpPost]
        public ResponseModel UpdateBillIsBuild(BuildModel model)
        {
            using (_dbContext)
            {
                model.calcId = Guid.NewGuid().ToString().ToLower();
                var parameters = model.ids.Split(",").Select((id, index) => new SqlParameter(string.Format("@p{0}", index), id)).ToList();
                var parameterNames = string.Join(", ", parameters.Select(p => p.ParameterName));
                var sql = string.Format(@"UPDATE t_BillEntry SET FCalcId=@FCalcId WHERE FEntryID IN ({0})", parameterNames);
                parameters.Add(new SqlParameter("@FCalcId", model.calcId));
                _dbContext.Database.ExecuteSqlCommand(sql, parameters);
                var response = ResponseModelFactory.CreateInstance;
                if (response.Code == 200)
                {
                    string resp = string.Empty;
                    //net request
                    var postContent = new MultipartFormDataContent();
                    postContent.Add(new StringContent("BuildTPlusVoucher"), "MethodName");
                    postContent.Add(new StringContent(JsonConvert.SerializeObject(new
                    {
                        FCalcID = model.calcId,
                        FPZDate = model.pzdate
                    })), "JSON");
                    if (HttpHelper.Post(ConfigurationManager.AppSettings.NetUrl, postContent, ref resp))
                    {
                        NetResponse respModel = JsonConvert.DeserializeObject<NetResponse>(resp);
                        if (respModel == null || !respModel.Result.ToUpper().Equals("Y"))
                        {
                            response.SetError(respModel == null ? "生成凭证发生错误!" : respModel.Message);
                        }
                    }
                    else
                    {
                        response.SetError("调用生成凭证过程发生错误!");
                    }
                }
                return response;
            }
        }

        public void ReverseBillIsBuild(string ids)
        {
            using (_dbContext)
            {
                var parameters = ids.Split(",").Select((id, index) => new SqlParameter(string.Format("@p{0}", index), id)).ToList();
                var parameterNames = string.Join(", ", parameters.Select(p => p.ParameterName));
                var sql = string.Format(@"UPDATE t_BillEntry SET FIsBuildPZ=0,FCalcId=NULL,
                        FPZBuildDate=NULL WHERE FEntryID IN ({0})", parameterNames);
                _dbContext.Database.ExecuteSqlCommand(sql, parameters);
            }
        }

        public class StopModel
        {
            public bool isStop { get; set; }
            public string userId { get; set; }
            public string ids { get; set; }
        }
        class ReqModel
        {
            public string sql { get; set; }
            public List<SqlParameter> parameters { get; set; }
        }
        public class CheckModel
        {
            public CommonEnum.IsConfirm isConfirm { get; set; }
            public string confirmerId { get; set; }
            public string ids { get; set; }
        }
        public class CalcModel
        {
            public CommonEnum.IsFinish isFinish { get; set; }
            public string calcId { get; set; }
            public string beginDate { get; set; }
            public string endDate { get; set; }
            public decimal multiple { get; set; }
            public string ids { get; set; }
        }
        public class BuildModel
        {
            public CommonEnum.IsBuild isBuild { get; set; }
            public string calcId { get; set; }
            public string ids { get; set; }
            public string pzdate { get; set; }
        }

        public class NetResponse
        {
            public string Result { get; set; }
            public string Message { get; set; }
        }
    }
}