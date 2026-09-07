using Application.Core.Entities;
using Application.Core.Exceptions;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.Tenant;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Services.Configuration.Tenants
{
    public class TenantService : BaseService<Tenant, TenantCreationDto, TenantUpdateDto, TenantRequestModel, TenantViewModel>, ITenantService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        public TenantService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext) : base(unitOfWork, mapper, workContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
        }

        public override async Task<Tuple<List<TenantViewModel>, int>> SearchAsync(TenantRequestModel request)
        {
            var queryable = _unitOfWork.Repository<Tenant>().TableUnfiltered().Where(request.GetExpression());
            if (!queryable.Any()) throw new NotFoundResultException("Ooo! No Search Result Found.");
            queryable = request.CreateOrderByQueryable(queryable);
            int count = queryable.Count();
            queryable = request.SkipAndTake(queryable);
            queryable = request.IncludeParents(queryable);
            var list = await queryable.ToListAsync();
            var mapps = _mapper.Map<List<TenantViewModel>>(list);
            return new Tuple<List<TenantViewModel>, int>(mapps, count);
        }

        public async Task<TenantViewModel> GetByIdAsync(Guid? id)
        {
            var tenant = await _unitOfWork.Repository<Tenant>().FindAsync(x => x.Id == id);
            if (tenant == null) throw new NotFoundResultException("Tenant Not Found With this Id");
            return _mapper.Map<TenantViewModel>(tenant);
        }


        public override async Task<Guid> AddAsync(TenantCreationDto tenantCreationDto)
        {
            var tenant = _mapper.Map<Tenant>(tenantCreationDto);
            var count = _unitOfWork.Repository<Tenant>().TableUnfiltered().IgnoreQueryFilters().Count() + 1;
            tenant.Id = Guid.NewGuid();
            tenant.Code = count.ToString().PadLeft(3, '0');
            await _unitOfWork.Repository<Tenant>().AddAsync(tenant);
            await _unitOfWork.SaveChangesAsync();
            return tenant.Id;
        }

        //public override async Task<Guid> UpdateAsync(TenantUpdateDto tenantUpdateDto)
        //{
        //    var dbTenant = await _unitOfWork.Repository<Tenant>().FindAsync(tenantUpdateDto.Id);
        //    var tenant = _mapper.Map(tenantUpdateDto, dbTenant);
        //    await _unitOfWork.Repository<Tenant>().UpdateAsync(tenant);
        //    await _unitOfWork.SaveChangesAsync();
        //    return tenant.Id;
        //}
    }
}
