using Application.Services.SearchRequestModels.Auth;
using Application.Services.ViewModels.Auth;

namespace Application.Services.Services.Auth.Users
{
    public interface IUserService
    {
        Task<bool> UserExistsForEmployeeId(Guid id);
        Task<Tuple<List<UserViewModel>, int>> SearchAsync(UserRequestModel request);
    }
}
