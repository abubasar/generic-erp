using Application.Core.Entities;
using Application.Core.Interfaces;
using Application.Services.Dtos.Role;
using Application.Services.SearchRequestModels.Auth;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Auth;
using AutoMapper;

namespace Application.Services.Services.Auth.Roles
{
    public class RoleService : BaseService<Role, RoleCreationDto, RoleUpdateDto, RoleRequestModel, RoleViewModel>, IRoleService
    {

        public RoleService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext) : base(unitOfWork, mapper, workContext) { }



    }
}
