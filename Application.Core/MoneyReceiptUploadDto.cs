using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core
{
    public class MoneyReceiptUploadDto
    {
        public IFormFile? FileDetails { get; set; }
        public Guid ReceivePaymentId { get; set; }
    }
}
