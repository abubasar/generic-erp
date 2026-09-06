using Application.Core.Entities;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.Company;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Services.Configuration.Companies
{
    public class CompanyService : BaseService<Company, CompanyCreationDto, CompanyUpdateDto, CompanyRequestModel, CompanyViewModel>, ICompanyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;

        public CompanyService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext) : base(unitOfWork, mapper, workContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
        }


        public override async Task<Guid> AddAsync(CompanyCreationDto CompanyCreationDto)
        {
            var model = _mapper.Map<Company>(CompanyCreationDto);
            var count = _unitOfWork.Repository<Company>().TableNoTracking().IgnoreQueryFilters().Count() + 1;
            model.Id = Guid.NewGuid();
            model.Code = count.ToString().PadLeft(3, '0');
            await _unitOfWork.Repository<Company>().AddAsync(model);
            await _unitOfWork.SaveChangesAsync();
            return model.Id;

        }


    }
}
