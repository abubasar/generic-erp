using Application.Core.Common;
using Application.Core.Entities;
using Application.Services.Dtos.Configuration.DiscountProductWise;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;
using Application.Services.ViewModels.Sale;

namespace Application.Services.Services.Configuration.DiscountProductWises
{
    public interface IDiscountProductWiseService : IBaseService<DiscountProductWise, DiscountProductWiseCreationDto, DiscountProductWiseUpdateDto, DiscountProductWiseRequestModel, DiscountProductWiseViewModel>
    {
        new Task<AddUpdateResponseModel> AddAsync(DiscountProductWiseCreationDto creationDto);
        new Task<AddUpdateResponseModel> UpdateAsync(DiscountProductWiseUpdateDto updateDto);
        Task<DiscountProductWiseByDateViewModel> GetDiscountProductWiseByProductId(DiscountProductWiseByProductIdRequestModel request);
        Task<List<ActiveDiscountProductWiseByDateViewModel>> GetActiveDiscountProductWiseByDate(DiscountProductWiseByDateRequestModel request);
    }
}
