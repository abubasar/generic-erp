using Application.Core.Entities;
using Application.Core.Exceptions;
using Application.Core.Interfaces;
using Application.Services.SearchRequestModels.Auth;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Auth;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Services.Auth.Users
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
        }



        public async Task<bool> UserExistsForEmployeeId(Guid id)
        {
            if (await _unitOfWork.Repository<User>().TableNoTracking().AnyAsync(x => x.EmployeeId == id)) return true;
            return false;
        }

        public virtual async Task<Tuple<List<UserViewModel>, int>> SearchAsync(UserRequestModel request)
        {
            var queryable = _unitOfWork.Repository<User>().TableNoTracking().Where(request.GetExpression());
            if (!queryable.Any()) throw new NotFoundResultException("Ooo! No Search Result Found.");
            queryable = request.CreateOrderByQueryable(queryable);
            int count = queryable.Count();
            queryable = request.SkipAndTake(queryable);
            queryable = request.IncludeParents(queryable);
            var list = await queryable.ToListAsync();
            return new Tuple<List<UserViewModel>, int>(_mapper.Map<List<UserViewModel>>(list), count);
        }

    }
}
