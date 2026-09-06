using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Dtos.Accounts.AccountsReceivable.ReceivePaymentAgainstSale
{
    public class ReceivePaymentAgainstSalePictureMappingCreationDto
    {
        public IFormFile? FileDetails { get; set; }
        public Guid ReceivePaymentAgainstSaleId { get; set; }
    }
}
