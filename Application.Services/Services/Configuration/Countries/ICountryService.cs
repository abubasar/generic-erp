using Application.Core.Entities;
using Application.Services.Dtos.Configuration.Country;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;

namespace Application.Services.Services.Configuration.Countries
{
    public interface ICountryService : IBaseService<Country, CountryCreationDto, CountryUpdateDto, CountryRequestModel, CountryViewModel>
    {
    }
}
