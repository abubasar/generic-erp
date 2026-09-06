namespace Application.Services.Dtos.Configuration.PaymentMethod
{
    public class PaymentMethodUpdateDto : PaymentMethodCreationDto
    {
        public Guid Id { get; set; }
    }
}
