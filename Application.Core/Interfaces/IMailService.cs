using Application.Core.Common;

namespace Application.Core.Interfaces
{
    public interface IMailService
    {
        Task SendEmailAsync(MailRequest mailRequest);
        Task SendWelcomeEmailAsync(WelcomeRequest request);
        Task SendEmailFromDefaultEmailAddressAsync(MailRequest mailRequest);
        Task GetIncomingMessages();
    }
}
