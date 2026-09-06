using Application.Core.Entities;
using Application.Services.Dtos.Configuration.PaymentMethod;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;

namespace Application.Services.Services.Configuration.PaymentMethods
{
    public interface IPaymentMethodService : IBaseService<PaymentMethod, PaymentMethodCreationDto, PaymentMethodUpdateDto, PaymentMethodRequestModel, PaymentMethodViewModel>
    {
    }
}
