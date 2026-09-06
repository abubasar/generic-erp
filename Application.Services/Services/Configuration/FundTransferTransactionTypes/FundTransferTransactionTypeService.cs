using Application.Core.Entities;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.FundTransferTransactionType;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;
using AutoMapper;

namespace Application.Services.Services.Configuration.FundTransferTransactionTypes
{
    public class FundTransferTransactionTypeService : BaseService<FundTransferTransactionType, FundTransferTransactionTypeCreationDto, FundTransferTransactionTypeUpdateDto, FundTransferTransactionTypeRequestModel, FundTransferTransactionTypeViewModel>, IFundTransferTransactionTypeService
    {
        public FundTransferTransactionTypeService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext) : base(unitOfWork, mapper, workContext)
        {
        }
    }
}
