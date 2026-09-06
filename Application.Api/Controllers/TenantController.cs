using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.Tenant;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Tenants;
using Application.Services.ViewModels.Configuration;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantService _tenantService;

        public TenantController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, ITenantService tenantService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _tenantService = tenantService;
        }

        [Authorize(Permissions.Tenants.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<TenantViewModel>, int>>> Search(TenantRequestModel request)
        {
            return await Result<Tuple<List<TenantViewModel>, int>>.SuccessAsync(await _tenantService.SearchAsync(request), "Result Found");
        }

        [Authorize(Permissions.Tenants.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] TenantCreationDto tenantCreationDto)
        {
            var tenantId = await _tenantService.AddAsync(tenantCreationDto);
            return await Result<Guid>.SuccessAsync(tenantId, "Tenant Added Successfully");
        }

        [Authorize(Permissions.Tenants.View)]
        [Route("{id}")]
        [HttpGet]
        public async Task<Result<TenantViewModel>> GetById(Guid id)
        {
            return await Result<TenantViewModel>.SuccessAsync(await _tenantService.GetByIdAsync(id), "Result Found");
        }

        //[Authorize(Permissions.Tenants.Edit)]
        //[HttpPost("update")]
        //public virtual async Task<Result> Put([FromBody] TenantUpdateDto tenantUpdateDto)
        //{
        //    var tenantId = await _tenantService.UpdateAsync(tenantUpdateDto);
        //    return await Result<Guid>.SuccessAsync(tenantId, "Tenant Updated Successfully");
        //}

        //[Authorize(Permissions.Tenants.Delete)]
        //[Route("delete/{id}")]
        //[HttpPost]
        //public virtual async Task<Result> Delete(Guid id)
        //{
        //    var tenantId = await _tenantService.DeleteAsync(id);
        //    return await Result<Guid>.SuccessAsync(tenantId, "Tenant Deleted Successfully");
        //}
    }
}
