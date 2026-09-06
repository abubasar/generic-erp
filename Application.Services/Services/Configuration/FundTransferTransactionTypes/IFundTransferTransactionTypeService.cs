using Application.Core.Entities;
using Application.Services.Dtos.Configuration.FundTransferTransactionType;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;

namespace Application.Services.Services.Configuration.FundTransferTransactionTypes
{
    public interface IFundTransferTransactionTypeService : IBaseService<FundTransferTransactionType, FundTransferTransactionTypeCreationDto, FundTransferTransactionTypeUpdateDto, FundTransferTransactionTypeRequestModel, FundTransferTransactionTypeViewModel>
    {
    }
}
