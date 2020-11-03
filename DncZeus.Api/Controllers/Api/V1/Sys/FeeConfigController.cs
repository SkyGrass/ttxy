using AutoMapper;
using DncZeus.Api.Entities;
using DncZeus.Api.Entities.Enums;
using DncZeus.Api.Extensions;
using DncZeus.Api.Extensions.AuthContext;
using DncZeus.Api.Extensions.CustomException;
using DncZeus.Api.Models.Response;
using DncZeus.Api.RequestPayload.Base.Bed;
using DncZeus.Api.Utils;
using DncZeus.Api.ViewModels.Base.DncBed;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using static DncZeus.Api.Entities.Enums.CommonEnum;

namespace DncZeus.Api.Controllers.Api.V1.Sys
{
    /// <summary>
    /// 
    /// </summary>
    //[CustomAuthorize]
    [Route("api/v1/sys/[controller]/[action]")]
    [ApiController]
    [CustomAuthorize]
    public class FeeConfigController : ControllerBase
    {
        private readonly DncZeusDbContext _dbContext;
        private readonly IMapper _mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="mapper"></param>
        public FeeConfigController(DncZeusDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("/api/v1/sys/feeconfig/find_list_by_hosid/{id}")]
        public IActionResult FindByHosId(int id)
        {
            var response = ResponseModelFactory.CreateResultInstance;
            using (_dbContext)
            {
                var query = _dbContext.DncFeeConfig.Where(x => x.HospitalId == id);
                var config = query.FirstOrDefault();
                if (config != null)
                {
                    var managerFee = _dbContext.DncHosManageFee.Where(x => x.MainId == config.Id).ToList();
                    var workerFeeByCareProject = _dbContext.DncWorkerFeeByCareProject.Where(x => x.MainId == config.Id).ToList();
                    var workerFeeByCarePrice = _dbContext.DncWorkerFeeByCarePrice.Where(x => x.MainId == config.Id).ToList();
                    var workerFeeByCareWay = _dbContext.DncWorkerFeeByCareWay.Where(x => x.MainId == config.Id).ToList();
                    response.SetData(new
                    {
                        config,
                        managerFee,
                        workerFeeByCareProject,
                        workerFeeByCarePrice,
                        workerFeeByCareWay
                    });
                }
                else
                {
                    response.SetData(new
                    {
                        config = DBNull.Value,
                        managerFee = DBNull.Value,
                        workerFeeByCareProject = DBNull.Value,
                        workerFeeByCarePrice = DBNull.Value,
                        workerFeeByCareWay = DBNull.Value,
                    });
                }

                return Ok(response);
            }
        }

        class ReqModel
        {
            public string sql { get; set; }
            public List<SqlParameter> parameters { get; set; }
        }

        /// <summary>
        /// 创建床位
        /// </summary>
        /// <param name="model">床位视图实体</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(200)]
        public IActionResult Create(SaveForm model)
        {
            var response = ResponseModelFactory.CreateInstance;
            try
            {
                List<ReqModel> sql = new List<ReqModel>();
                List<SqlParameter> parameters = new List<SqlParameter>();
                sql.Clear();

                List<DncHosManageFee> listHosManagerFee = model.managerFee;
                SaveForm.WorkerFee workerFee = model.workerFee;

                int _ConfigId = -1;
                if (workerFee.feeByProjectDataSource.Count > 0)
                {
                    _ConfigId = 0;
                }
                if (workerFee.feeByPriceDataSource.Count > 0)
                {
                    _ConfigId = 1;
                }
                if (workerFee.feeByWayDataSource.Count > 0)
                {
                    _ConfigId = 2;
                }

                if (model.configId < 0)
                {
                    List<SqlParameter> p = new List<SqlParameter>();
                    p.Add(new SqlParameter("@TableName", "DncFeeConfig"));
                    p.Add(new SqlParameter("@Increment", "1"));
                    DataTable dt = _dbContext.Database.SqlQuery(string.Format(@"EXEC dbo.L_P_GetMaxID  'DncFeeConfig',1"));
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        model.configId = int.Parse(dt.Rows[0]["FMaxID"].ToString());
                        parameters = new List<SqlParameter>();
                        parameters.Add(new SqlParameter("@Id", model.configId));
                        parameters.Add(new SqlParameter("@Code", model.configId));
                        parameters.Add(new SqlParameter("@Name", model.configId));
                        parameters.Add(new SqlParameter("@Status", Status.Normal));
                        parameters.Add(new SqlParameter("@IsDeleted", IsDeleted.No));
                        parameters.Add(new SqlParameter("@HospitalId", model.hosId));
                        parameters.Add(new SqlParameter("@CreatedOn", DateTime.Now));
                        parameters.Add(new SqlParameter("@CreatedByUserGuid", AuthContextService.CurrentUser.Guid));
                        parameters.Add(new SqlParameter("@CreatedByUserName", AuthContextService.CurrentUser.DisplayName));
                        parameters.Add(new SqlParameter("@ConfigId", _ConfigId));
                        sql.Add(new ReqModel()
                        {
                            sql = string.Format(@"INSERT INTO dbo.DncFeeConfig
                                                            ( Id ,
                                                              Code ,
                                                              Name , 
                                                              Status ,
                                                              IsDeleted ,
                                                              HospitalId ,
                                                              CreatedOn ,
                                                              CreatedByUserGuid ,
                                                              CreatedByUserName ,
                                                              ConfigId
                                                            )
                                                    VALUES  (  @Id ,
                                                              @Code ,
                                                              @Name , 
                                                              @Status ,
                                                              @IsDeleted ,
                                                              @HospitalId ,
                                                              @CreatedOn ,
                                                              @CreatedByUserGuid ,
                                                              @CreatedByUserName ,
                                                              @ConfigId
                                                            )"),
                            parameters = parameters
                        });
                    }
                }
                else
                {
                    parameters.Add(new SqlParameter("@Id", model.configId));
                    parameters.Add(new SqlParameter("@ConfigId", _ConfigId));
                    sql.Add(new ReqModel()
                    {
                        sql = string.Format(@"Update dbo.DncFeeConfig Set ConfigId=@ConfigId Where Id=@Id"),
                        parameters = parameters
                    });
                }


                if (listHosManagerFee.Count > 0)
                {
                    parameters = new List<SqlParameter>();
                    parameters.Add(new SqlParameter("@mainid", model.configId));
                    sql.Add(new ReqModel()
                    {
                        sql = string.Format(@"delete from dnchosmanagefee where mainid=@mainid"),
                        parameters = parameters
                    });

                    listHosManagerFee.ForEach(item =>
                    {
                        if (item.MainId == 0)
                        {
                            item.MainId = model.configId;
                        }
                        parameters = new List<SqlParameter>();
                        parameters.Add(new SqlParameter("@mainid", model.configId));
                        parameters.Add(new SqlParameter("@itemid", item.ItemId));
                        parameters.Add(new SqlParameter("@linksymbol_1", item.LinkSymbol_1));
                        parameters.Add(new SqlParameter("@ratio_1", item.Ratio_1));

                        sql.Add(new ReqModel()
                        {
                            sql = string.Format(@"insert into dnchosmanagefee(mainid,itemid,linksymbol_1,ratio_1)values(@mainid,@itemid,@linksymbol_1,@ratio_1)"),
                            parameters = parameters
                        });

                    });
                }

                if (workerFee != null)
                {
                    List<DncWorkerFeeByCareProject> list1 = workerFee.feeByProjectDataSource;

                    parameters = new List<SqlParameter>();
                    parameters.Add(new SqlParameter("@mainid", model.configId));
                    sql.Add(new ReqModel()
                    { sql = string.Format(@"delete from dncworkerfeebycareproject where mainid=@mainid"), parameters = parameters });

                    if (list1.Count > 0)
                    {
                        list1.ForEach(item =>
                        {
                            if (item.MainId == 0)
                            {
                                item.MainId = model.configId;
                            }
                            parameters = new List<SqlParameter>();
                            parameters.Add(new SqlParameter("@MainId", model.configId));
                            parameters.Add(new SqlParameter("@ItemId", item.ItemId));
                            parameters.Add(new SqlParameter("@ItemName", item.ItemName));
                            parameters.Add(new SqlParameter("@SysParName_1", item.SysParName_1));
                            parameters.Add(new SqlParameter("@LinkSymbol_1", item.LinkSymbol_1));
                            parameters.Add(new SqlParameter("@Ratio_1", item.Ratio_1));
                            parameters.Add(new SqlParameter("@LinkSymbol_2", item.LinkSymbol_2));
                            parameters.Add(new SqlParameter("@Ratio_2", item.Ratio_2));
                            parameters.Add(new SqlParameter("@LinkSymbol_3", item.LinkSymbol_3));
                            parameters.Add(new SqlParameter("@SysParName_2", item.SysParName_2));
                            sql.Add(new ReqModel()
                            {
                                sql = string.Format(@"INSERT INTO dbo.DncWorkerFeeByCareProject
                                                                ( MainId ,
                                                                  ItemId ,
                                                                  ItemName ,
                                                                  SysParName_1 ,
                                                                  LinkSymbol_1 ,
                                                                  Ratio_1 ,
                                                                  LinkSymbol_2 ,
                                                                  Ratio_2 ,
                                                                  LinkSymbol_3 ,
                                                                  SysParName_2
                                                                )
                                                        VALUES  ( @MainId, @ItemId, @ItemName, @SysParName_1, @LinkSymbol_1,
                                                                    @Ratio_1, @LinkSymbol_2, @Ratio_2, @LinkSymbol_3, @SysParName_2)"),

                                parameters = parameters
                            });
                        });
                    }

                    List<DncWorkerFeeByCarePrice> list2 = workerFee.feeByPriceDataSource;

                    List<DecimalRange> checkList = new List<DecimalRange>();

                    parameters = new List<SqlParameter>();
                    parameters.Add(new SqlParameter("@mainid", model.configId));
                    sql.Add(new ReqModel()
                    {
                        sql = string.Format(@"delete from dncworkerfeebycareprice where mainid=@mainid"),
                        parameters = parameters
                    });

                    if (list2.Count > 0)
                    {
                        list2.ForEach(item =>
                        {
                            if (item.MainId == 0)
                            {
                                item.MainId = model.configId;
                            }

                            checkList.Add(new DecimalRange()
                            {
                                Min = item.StartPoint,
                                Max = item.EndPoint
                            });

                            parameters = new List<SqlParameter>();
                            parameters = new List<SqlParameter>();
                            parameters.Add(new SqlParameter("@MainId", model.configId));
                            parameters.Add(new SqlParameter("@StartPoint", item.StartPoint));
                            parameters.Add(new SqlParameter("@EndPoint", item.EndPoint));
                            parameters.Add(new SqlParameter("@Fee", item.Fee));
                            sql.Add(new ReqModel()
                            {
                                sql = string.Format(@"INSERT INTO dbo.DncWorkerFeeByCarePrice
		                                                                                    ( MainId, StartPoint, EndPoint, Fee )
		                                                                            VALUES  (  @MainId, @StartPoint, @EndPoint, @Fee)"),
                                parameters = parameters
                            });
                        });

                        if (checkList.ExistsIntersectionRange())
                        {
                            response.SetData("error");
                            response.SetFailed("费用区间存在交叉,禁止保存!");
                            return Ok(response);
                        }
                    }

                    List<DncWorkerFeeByCareWay> list3 = workerFee.feeByWayDataSource;

                    parameters = new List<SqlParameter>();
                    parameters.Add(new SqlParameter("@mainid", model.configId));
                    sql.Add(new ReqModel()
                    {
                        sql = string.Format(@"delete from dncworkerfeebycareway where mainid=@mainid"),
                        parameters = parameters
                    });

                    if (list3.Count > 0)
                    {
                        list3.ForEach(item =>
                        {
                            parameters = new List<SqlParameter>();
                            parameters.Add(new SqlParameter("@MainId", model.configId));
                            parameters.Add(new SqlParameter("@ItemId", item.ItemId));
                            parameters.Add(new SqlParameter("@ItemName", item.ItemName));
                            parameters.Add(new SqlParameter("@SysParName_1", item.SysParName_1));
                            parameters.Add(new SqlParameter("@LinkSymbol_1", item.LinkSymbol_1));
                            parameters.Add(new SqlParameter("@SysParName_2", item.SysParName_2));
                            parameters.Add(new SqlParameter("@LinkSymbol_2", item.LinkSymbol_2));
                            parameters.Add(new SqlParameter("@Ratio_1", item.Ratio_1));

                            sql.Add(new ReqModel()
                            {
                                sql = string.Format(@"INSERT INTO dbo.DncWorkerFeeByCareWay
				                                                          ( MainId ,
				                                                            ItemId ,
				                                                            ItemName ,
				                                                            SysParName_1 ,
				                                                            LinkSymbol_1 ,
				                                                            SysParName_2 ,
				                                                            LinkSymbol_2 ,
				                                                            Ratio_1
				                                                          )
				                                                  VALUES  (@MainId ,
				                                                            @ItemId ,
				                                                           @ItemName ,
				                                                            @SysParName_1 ,
				                                                            @LinkSymbol_1 ,
				                                                            @SysParName_2 ,
				                                                            @LinkSymbol_2 ,
				                                                            @Ratio_1)"),
                                parameters = parameters
                            });
                        });
                    }
                }

                _dbContext.Database.BeginTransaction();
                int effectRow = 0;
                sql.ForEach(f =>
                {
                    effectRow += _dbContext.Database.ExecuteSqlCommand(f.sql, f.parameters);
                });
                _dbContext.Database.CommitTransaction();
                response.SetData($"success|{model.configId}");
                response.SetSuccess();
                return Ok(response);
            }
            catch (Exception e)
            {
                _dbContext.Database.RollbackTransaction();
                response.SetData("error");
                response.SetFailed();
                return Ok(response);
            }
        }


        /// <summary>
        /// 删除床位
        /// </summary>
        /// <param name="isDeleted"></param>
        /// <param name="ids">床位ID字符串,多个以逗号隔开</param>
        /// <returns></returns>
        private ResponseModel UpdateIsDelete(IsDeleted isDeleted, string ids)
        {
            using (_dbContext)
            {
                var parameters = ids.Split(",").Select((id, index) => new SqlParameter(string.Format("@p{0}", index), id)).ToList();
                var parameterNames = string.Join(", ", parameters.Select(p => p.ParameterName));
                var sql = string.Format("UPDATE DncBed SET IsDeleted=@IsDeleted WHERE Id IN ({0})", parameterNames);
                parameters.Add(new SqlParameter("@IsDeleted", (int)isDeleted));
                _dbContext.Database.ExecuteSqlCommand(sql, parameters);
                var response = ResponseModelFactory.CreateInstance;
                return response;
            }
        }

    }
}