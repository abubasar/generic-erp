using Application.Core.Common;
using Application.Core.Exceptions;
using Application.Core.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;


namespace Application.Services.Services.Common
{
    public class BaseService<TEntity, TCreationDto, TUpdateDto, TRequestModel, TViewModel> : IBaseService<TEntity, TCreationDto, TUpdateDto, TRequestModel, TViewModel> where TRequestModel : BaseRequestModel<TEntity> where TEntity : class
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        public BaseService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
        }
        public virtual async Task<Tuple<List<TViewModel>, int>> SearchAsync(TRequestModel request)
        {
            var queryable = _unitOfWork.Repository<TEntity>().TableNoTracking().Where(request.GetExpression());
            queryable = request.OrderByFunc()(queryable);
            int count = await queryable.CountAsync();
            if (count == 0) return new Tuple<List<TViewModel>, int>(_mapper.Map<List<TViewModel>>(new List<TViewModel>()), 0);
            queryable = request.SkipAndTake(queryable);
            queryable = request.IncludeParents(queryable);
            return new Tuple<List<TViewModel>, int>(await queryable.Select(x => _mapper.Map<TViewModel>(x)).ToListAsync(), count);
        }

        public virtual async Task<Guid> AddAsync(TCreationDto creationDto)
        {
            var model = _mapper.Map<TEntity>(creationDto);
            model!.GetType().GetProperty("Id")!.SetValue(model, Guid.NewGuid());
            await _unitOfWork.Repository<TEntity>().AddAsync(model);
            await _unitOfWork.SaveChangesAsync();
            return Guid.Parse(model!.GetType().GetProperty("Id")!.GetValue(model)!.ToString()!);

        }

        public virtual async Task<Guid> UpdateAsync(TUpdateDto updateDto)
        {
            string id = updateDto!.GetType().GetProperty("Id")!.GetValue(updateDto)!.ToString()!;
            var obj = await _unitOfWork.Repository<TEntity>().FindAsync(Guid.Parse(id));
            if (obj == null) throw new NotFoundResultException("Not Found With this Id");
            var model = _mapper.Map(updateDto, obj);
            await _unitOfWork.Repository<TEntity>().UpdateAsync(model);
            await _unitOfWork.SaveChangesAsync();
            return Guid.Parse(id);
        }


        public virtual async Task<Guid> DeleteAsync(Guid id)
        {
            var obj = await _unitOfWork.Repository<TEntity>().FindAsync(id);
            if (obj == null) throw new NotFoundResultException("Entity Not Found With this Id");
            await _unitOfWork.Repository<TEntity>().DeleteAsync(obj);
            await _unitOfWork.SaveChangesAsync();
            return id;
        }
    }
}
