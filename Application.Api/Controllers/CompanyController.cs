
using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.Company;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Companies;
using Application.Services.ViewModels.Configuration;
using AutoMapper;
//using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICompanyService _companyService;
        public CompanyController(ICompanyService companyService,
           IWorkContext workContext,
            IUnitOfWork unitOfWork, IMapper mapper)
        {
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _companyService = companyService;
            _mapper = mapper;
        }
        [Authorize(Permissions.Companies.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<CompanyViewModel>, int>>> Search(CompanyRequestModel request)
        {
            return await Result<Tuple<List<CompanyViewModel>, int>>.SuccessAsync(await _companyService.SearchAsync(request), "Result Found");
        }

        [Authorize(Permissions.Companies.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] CompanyCreationDto companyCreationDto)
        {
            var companyId = await _companyService.AddAsync(companyCreationDto);
            return await Result<Guid>.SuccessAsync(companyId, "Company Added Successfully");

        }
        [Authorize(Permissions.Companies.Edit)]
        [HttpPost("/api/company/update")]
        public virtual async Task<Result> Put([FromBody] CompanyUpdateDto companyUpdateDto)
        {
            var companyId = await _companyService.UpdateAsync(companyUpdateDto);
            return await Result<Guid>.SuccessAsync(companyId, "Company Updated Successfully");


        }
        [Authorize(Permissions.Companies.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var companyId = await _companyService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(companyId, "Company Deleted Successfully");


        }

    }
}
