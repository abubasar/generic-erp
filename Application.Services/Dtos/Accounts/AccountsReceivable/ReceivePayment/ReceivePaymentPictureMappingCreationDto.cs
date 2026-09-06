using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Dtos.Accounts.AccountsReceivable.ReceivePayment
{
    public class ReceivePaymentPictureMappingCreationDto
    {
        public IFormFile? FileDetails { get; set; }
        public Guid ReceivePaymentId { get; set; }
    }
}
