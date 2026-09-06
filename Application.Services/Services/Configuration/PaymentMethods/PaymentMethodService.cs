using Application.Core.Entities;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.PaymentMethod;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;
using AutoMapper;

namespace Application.Services.Services.Configuration.PaymentMethods
{
    public class PaymentMethodService : BaseService<PaymentMethod, PaymentMethodCreationDto, PaymentMethodUpdateDto, PaymentMethodRequestModel, PaymentMethodViewModel>, IPaymentMethodService
    {
        public PaymentMethodService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext) : base(unitOfWork, mapper, workContext)
        {
        }
    }
}
