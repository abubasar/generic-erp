using Application.Core.Entities;
using Application.Core.Exceptions;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.FinancialYear;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;
using AutoMapper;

namespace Application.Services.Services.Configuration.FinancialYears
{
    public class FinancialYearService : BaseService<FinancialYear, FinancialYearCreationDto, FinancialYearUpdateDto, FinancialYearRequestModel, FinancialYearViewModel>, IFinancialYearService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        public FinancialYearService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext) : base(unitOfWork, mapper, workContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
        }
        public override async Task<Guid> AddAsync(FinancialYearCreationDto financialYearCreationDto)
        {
            var model = _mapper.Map<FinancialYear>(financialYearCreationDto);
            model.Id = Guid.NewGuid();
            if (financialYearCreationDto.Name!.Trim().Length == 9)
            {
                string[] parts = financialYearCreationDto.Name!.Trim().Split('-');
                model.Code = parts[0].Substring(parts[0].Length - 2) + parts[1].Substring(parts[1].Length - 2);
            }
            else throw new BadRequestException("Financial Year Name Should Be Like 2000-2001 & Must Be 9 Character");
            await _unitOfWork.Repository<FinancialYear>().AddAsync(model);
            await _unitOfWork.SaveChangesAsync();
            return model.Id;
        }
        public override async Task<Guid> UpdateAsync(FinancialYearUpdateDto FinancialYearUpdateDto)
        {
            var financialYear = await _unitOfWork.Repository<FinancialYear>().FindAsync(FinancialYearUpdateDto.Id);
            if (financialYear == null) throw new NotFoundResultException("Financial Year Not Found With this Id");
            var model = _mapper.Map(FinancialYearUpdateDto, financialYear);
            if (FinancialYearUpdateDto.Name!.Trim().Length == 9)
            {
                string[] parts = FinancialYearUpdateDto.Name!.Trim().Split('-');
                financialYear.Code = parts[0].Substring(parts[0].Length - 2) + parts[1].Substring(parts[1].Length - 2);
            }
            else throw new BadRequestException("Financial Year Name Should Be Like 2000-2001 & Must Be 9 Character");
            await _unitOfWork.Repository<FinancialYear>().UpdateAsync(model);
            await _unitOfWork.SaveChangesAsync();
            return model.Id;
        }
    }
}
