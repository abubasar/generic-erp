
using Application.Core.Entities;
using Application.Services.Dtos.Configuration.Company;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;

namespace Application.Services.Services.Configuration.Companies
{
    public interface ICompanyService : IBaseService<Company, CompanyCreationDto, CompanyUpdateDto, CompanyRequestModel, CompanyViewModel>
    {


    }
}
