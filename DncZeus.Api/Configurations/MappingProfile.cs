

using AutoMapper;
using DncZeus.Api.Entities;
using DncZeus.Api.ViewModels.Base.DncArea;
using DncZeus.Api.ViewModels.Base.DncBed;
using DncZeus.Api.ViewModels.Base.DncCareProject;
using DncZeus.Api.ViewModels.Base.DncCareWay;
using DncZeus.Api.ViewModels.Base.DncHospital;
using DncZeus.Api.ViewModels.Base.DncManager;
using DncZeus.Api.ViewModels.Base.DncPayWay;
using DncZeus.Api.ViewModels.Base.DncPc;
using DncZeus.Api.ViewModels.Rbac.DncIcon;
using DncZeus.Api.ViewModels.Rbac.DncMenu;
using DncZeus.Api.ViewModels.Rbac.DncPermission;
using DncZeus.Api.ViewModels.Rbac.DncRole;
using DncZeus.Api.ViewModels.Rbac.DncUser;

namespace DncZeus.Api.Configurations
{
    /// <summary>
    /// 
    /// </summary>
    public class MappingProfile : Profile
    {
        /// <summary>
        /// 
        /// </summary>
        public MappingProfile()
        {
            #region DncUser
            CreateMap<DncUser, UserJsonModel>();
            CreateMap<UserCreateViewModel, DncUser>();
            CreateMap<UserEditViewModel, DncUser>();
            #endregion

            #region DncRole
            CreateMap<DncRole, RoleJsonModel>();
            CreateMap<RoleCreateViewModel, DncRole>();
            #endregion

            #region DncMenu
            CreateMap<DncMenu, MenuJsonModel>();
            CreateMap<MenuCreateViewModel, DncMenu>();
            CreateMap<MenuEditViewModel, DncMenu>();
            CreateMap<DncMenu, MenuEditViewModel>();
            #endregion

            #region DncIcon
            CreateMap<DncIcon, IconCreateViewModel>();
            CreateMap<IconCreateViewModel, DncIcon>();
            #endregion

            #region DncPermission
            CreateMap<DncPermission, PermissionJsonModel>()
                .ForMember(d => d.MenuName, s => s.MapFrom(x => x.Menu.Name))
                .ForMember(d => d.PermissionTypeText, s => s.MapFrom(x => x.Type.ToString()));
            CreateMap<PermissionCreateViewModel, DncPermission>();
            CreateMap<PermissionEditViewModel, DncPermission>();
            CreateMap<DncPermission, PermissionEditViewModel>();
            #endregion

            #region DncHospital
            CreateMap<DncHospital, HospitalCreateViewModel>();
            CreateMap<HospitalCreateViewModel, DncHospital>();
            #endregion

            #region DncManager
            CreateMap<DncManager, ManagerCreateViewModel>();
            CreateMap<ManagerCreateViewModel, DncManager>();
            #endregion

            #region DncArea
            CreateMap<DncArea, AreaCreateViewModel>();
            CreateMap<AreaCreateViewModel, DncArea>();
            #endregion

            #region DncPayWay
            CreateMap<DncPayWay, PayWayCreateViewModel>();
            CreateMap<PayWayCreateViewModel, DncPayWay>();
            #endregion

            #region DncCareProject
            CreateMap<DncCareProject, CareProjectCreateViewModel>();
            CreateMap<CareProjectCreateViewModel, DncCareProject>();
            #endregion

            #region DncCareWay
            CreateMap<DncCareWay, CareWayCreateViewModel>();
            CreateMap<CareWayCreateViewModel, DncCareWay>();
            #endregion

            #region DncBed
            CreateMap<DncBed, BedCreateViewModel>();
            CreateMap<BedCreateViewModel, DncBed>();
            #endregion

            #region DncPc
            CreateMap<DncPc, PcCreateViewModel>();
            CreateMap<PcCreateViewModel, DncPc>();
            #endregion
        }
    }
}
