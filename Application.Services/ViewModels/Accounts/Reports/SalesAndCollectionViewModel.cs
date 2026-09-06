using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ViewModels.Accounts.Reports
{
    public class SalesAndCollectionViewModel
    {
        public Guid Id { get; set; }
        public string? CustomerCode { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerAddress { get; set; }
        public Guid? RegionId { get; set; }
        public Guid? ZoneId { get; set; }
        public Guid? AreaId { get; set; }
        public Guid? TerritoryId { get; set; }
        public Guid? CustomerMarketingOfficerId { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ThisPeriodSaleValue { get; set; }
        public decimal ThisPeriodSaleReturnValue { get; set; }
        public decimal ThisPeriodCollection { get; set; }
        public decimal ThisPeriodAdjustment { get; set; }
        public decimal ClosingBalance { get; set; }
    }
}
