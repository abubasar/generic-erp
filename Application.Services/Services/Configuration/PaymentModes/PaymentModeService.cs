using Application.Core.Entities;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.PaymentMode;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;
using AutoMapper;

namespace Application.Services.Services.Configuration.PaymentModes
{
    public class PaymentModeService : BaseService<PaymentMode, PaymentModeCreationDto, PaymentModeUpdateDto, PaymentModeRequestModel, PaymentModeViewModel>, IPaymentModeService
    {
        public PaymentModeService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext) : base(unitOfWork, mapper, workContext)
        {
        }
    }
}
