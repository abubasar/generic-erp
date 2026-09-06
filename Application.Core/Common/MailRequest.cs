using Microsoft.AspNetCore.Http;

namespace Application.Core.Common
{
    public class MailRequest
    {
        public string? ToEmail { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public List<IFormFile>? Attachments { get; set; }
        public byte[]? FileBytes { get; set; }
        public string? FileName { get; set; }
    }
}
