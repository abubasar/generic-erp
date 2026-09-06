using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Interfaces;
using Application.Core.Settings;
using MailKit.Net.Pop3;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using IMailService = Application.Core.Interfaces.IMailService;

namespace Application.Infrastructure
{

    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;
        private readonly IUnitOfWork _unitOfWork;

        public MailService(IOptions<MailSettings> mailSettings, IUnitOfWork unitOfWork)
        {
            _mailSettings = mailSettings.Value;
            _unitOfWork = unitOfWork;
        }

        public async Task SendEmailWithMultipleAttachmentsAsync(MailRequest mailRequest)
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(_mailSettings.Mail);
            email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));
            email.Subject = mailRequest.Subject;
            var builder = new BodyBuilder();
            if (mailRequest.Attachments != null)
            {
                foreach (var file in mailRequest.Attachments)
                {
                    if (file.Length > 0)
                    {
                        byte[] fileBytes;
                        await using (var ms = new MemoryStream())
                        {
                            await file.CopyToAsync(ms);
                            fileBytes = ms.ToArray();
                        }
                        builder.Attachments.Add(file.FileName, fileBytes, ContentType.Parse(file.ContentType));
                    }
                }
            }
            builder.HtmlBody = mailRequest.Body;
            email.Body = builder.ToMessageBody();
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_mailSettings.Mail, _mailSettings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        public async Task SendEmailAsync(MailRequest mailRequest)
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(_mailSettings.Mail);
            email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));
            email.Subject = mailRequest.Subject;
            var builder = new BodyBuilder();
            if (mailRequest.FileBytes != null)
            {
                builder.Attachments.Add(mailRequest.FileName, mailRequest.FileBytes, ContentType.Parse("application/pdf"));
            }
            builder.HtmlBody = mailRequest.Body;
            email.Body = builder.ToMessageBody();
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_mailSettings.Mail, _mailSettings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
        public async Task SendEmailFromDefaultEmailAddressAsync(MailRequest mailRequest)
        {
            var emailAccount = await _unitOfWork.Repository<EmailAccount>().TableNoTracking().SingleOrDefaultAsync(x => x.IsDefaultEmailAccount);
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(emailAccount!.Email);
            email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));
            email.Subject = mailRequest.Subject;
            var builder = new BodyBuilder();
            if (mailRequest.FileBytes != null)
            {
                builder.Attachments.Add(mailRequest.FileName, mailRequest.FileBytes, ContentType.Parse("application/pdf"));
            }
            builder.HtmlBody = mailRequest.Body;
            email.Body = builder.ToMessageBody();
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(emailAccount.Host, emailAccount.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(emailAccount.Email, emailAccount.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        public async Task SendWelcomeEmailAsync(WelcomeRequest request)
        {
            string filePath = Directory.GetCurrentDirectory() + "\\wwwroot\\Templates\\WelcomeTemplate.html";
            StreamReader str = new StreamReader(filePath);
            string mailText = await str.ReadToEndAsync();
            str.Close();

            mailText = mailText.Replace("[username]", request.UserName).Replace("[email]", request.ToEmail);

            var email = new MimeMessage();
            var emailAccount = await _unitOfWork.Repository<EmailAccount>().TableNoTracking().SingleOrDefaultAsync(x => x.IsDefaultEmailAccount);
            email.Sender = MailboxAddress.Parse(emailAccount!.Email);
            email.To.Add(MailboxAddress.Parse(request.ToEmail));
            email.Subject = $"Welcome {request.UserName}";

            var builder = new BodyBuilder();
            builder.HtmlBody = mailText;
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_mailSettings.Mail, _mailSettings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        public async Task GetIncomingMessages()
        {
            using var smtp = new Pop3Client();
            await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_mailSettings.Mail, _mailSettings.Password);

            var message = await smtp.GetMessageAsync(0);

            Console.WriteLine("From: {0}", message.From);
            Console.WriteLine("To: {0}", message.To);
            Console.WriteLine("Subject: {0}", message.Subject);
            Console.WriteLine("Body: {0}", message.Body);

            //  client.SetFlags(0, MessageFlags.Seen, true);

            await smtp.DisconnectAsync(true);

        }
    }
}
