using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Dtos.Accounts.AccountsReceivable.ReceivePaymentAgainstSale
{
    public class ReceivePaymentAgainstSaleCreationDtoForDmsApp
    {
        public DateTime PaymentDate { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public Guid? CustomerMarketingOfficerId { get; set; }
        public decimal Amount { get; set; }
        public Guid CostCenterId { get; set; }
        public Guid ToAccountId { get; set; }
        public string? Remark { get; set; }
        public IFormFile? FileDetails { get; set; }
    }
}
