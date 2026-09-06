using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ViewModels.Accounts.Reports
{
    public class CogsViewModel
    {
        public decimal RawMaterialsOpeningValue { get; set; }
        public decimal RawMaterialsClosingValue { get; set; }
        public decimal FinishedGoodsOpeningValue { get; set; }
        public decimal FinishedGoodsClosingValue { get; set; }
        public decimal TotalCreditInPurchaseClearingAccount { get; set; }
        public decimal PurchasePriceVarianceDuringThisPeriod { get; set; }
        public decimal TotalCreditInLcCostClearingAccount { get; set; }
        public decimal BalanceInPurchaseReturnClearingAccount { get; set; }
        public decimal TotalCreditInPurchaseDiscount { get; set; }
        public decimal FactoryOverhead { get; set; }
        public decimal DirectExpenses { get; set; }
        public decimal WorkInProgressInventory { get; set; }
        public decimal Cogs { get; set; }
        public decimal TransportationCost { get; set; }
        public decimal GainLossInventoryVariance { get; set; }

    }
}
