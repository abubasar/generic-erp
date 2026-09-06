using Application.Core.Entities;
using Application.Services.Dtos.Configuration.PaymentMode;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;

namespace Application.Services.Services.Configuration.PaymentModes
{
    public interface IPaymentModeService : IBaseService<PaymentMode, PaymentModeCreationDto, PaymentModeUpdateDto, PaymentModeRequestModel, PaymentModeViewModel>
    {
    }
}
