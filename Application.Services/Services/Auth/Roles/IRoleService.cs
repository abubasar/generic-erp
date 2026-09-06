using Application.Core.Entities;
using Application.Services.Dtos.Role;
using Application.Services.SearchRequestModels.Auth;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Auth;

namespace Application.Services.Services.Auth.Roles
{
    public interface IRoleService : IBaseService<Role, RoleCreationDto, RoleUpdateDto, RoleRequestModel, RoleViewModel>
    {


    }
}
