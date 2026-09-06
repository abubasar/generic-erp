namespace Application.Core.Constants
{
    public static class AccountHeadConstants
    {
        //account Roots Code
        public const string NonCurrentAsset = "C3E8E158-1AA7-4160-9358-007A80D2CE6F";
        public const string CurrentAsset = "22688043-45EE-4115-9637-19F4A32F5A98";
        public const string OwnersEquity = "DD07995E-3F49-4326-BCF4-5357BF0355DA";
        public const string OthersEquity = "3B878403-AEBD-4E7C-8BF3-7C3B8B6AC04A";
        public const string NonCurrentLiabilities = "C7B4103B-8703-43D7-A235-000B4F07B6C9";
        public const string CurrentLiabilities = "D603492E-6B71-407C-AB49-9B0ACDC099B8";
        public const string Revenue = "0B114C3A-F1C7-49AE-A54E-0029FA7E1268";
        public const string CostOfGoodsSold = "CFFC6233-78D5-4CB5-8DFF-00370E8177EE";
        public const string OperatingExpenses = "E8B00859-29C9-4B9F-AF2D-B58617020CF7";
        public const string OtherIncome = "576D9241-135C-4387-80B6-40935ADC9C64";
        public const string NonOperatingExpenses = "40972A38-8B0A-4C8B-A548-3D28E839483F";
        public const string ContributiontoCSRFund = "9803751B-D516-4DA9-B360-978EC98846BB";
        public const string CurrentTax = "662A6C6F-09D2-43DA-A696-369B8C756393";
        //account Roots end
        public const string TradePayable = "b645e0ed-b562-4397-9c95-ab5d8e45fdb2";
        public const string TradeReceivable = "02D3C859-9FF9-4393-A351-C61A8D5C8334";
        public const string PurchaseClearingAccount = "3A71AD51-684D-4485-AB73-95EB126DF96E";
        public const string LCCostClearingAccount = "CA112047-F529-4B78-9C73-F0AD0482A4F7";
        public const string PurchaseReturnClearingAccount = "410FC7E7-A6E4-4FF0-B607-ADD87AAABA04";
        public const string CarryingPayable = "E3749624-1410-4D35-8B67-801BDF35D825";
        public const string CarryingExpenses_Sales = "2B7BAF14-19DC-4811-B030-5A66803B32E5";
        public const string RawMaterialsInventory = "C2969634-CC6F-4FAC-8F3F-8D8F4CD06137";
        public const string WorkInProgressInventory = "7E912FE0-3C3E-4CC1-A21B-8123F34934FD";
        public const string FinishedGoodsInventory = "64285947-97B2-4384-87D5-7DF72002E6A8";
        public const string DirectExpenses = "16EBC06A-696C-40AE-A0F3-62B56047CAD4";
        public const string FactoryOverhead = "B88340C0-67A0-4B42-9E40-CD101E843F4C";
        public const string DirectExpenses_Standard = "2DF7205B-C6DA-4FD1-B6EA-BD06D392AD8D";
        public const string FactoryOverhead_Standard = "2A0E9B94-F14D-47D3-BF2F-DD63FCDDA3B4";
        public const string COGS_Standard = "38379B9F-8DEB-4D87-A0F7-CBBCA6C2AFFF";
        public const string Sales = "6BC1A6E0-F6C9-4555-8819-943B7A9B5B24";
        public const string SaleReturn = "1918A910-4C03-455F-A078-31AA44DEDD3A";
        public const string InventoryAdjust = "4025017E-2282-4FCA-AE39-0AE329A752DC";
        public const string DepotCharge = "B93DEFD2-C16F-41CB-BF13-E835612F5DE1";
        public const string CashAndCashEquivalents = "D690995B-747A-45C6-91F6-19F5B2C8A4B0";
        public const string CashAtBank = "0D876F1C-4EDE-4FF6-9E37-52FAA4560F9F";
        public const string CashInHand = "BD5596F6-AC93-4F7C-A909-E8F6717572FF";
        public const string RetainedEarnings = "BD4CB478-5699-4DFC-B19C-D2F9DC14D959";
        public const string CurrentYearsProfitLoss = "CE5F1F14-925C-4D0D-91A3-D7C5AAA11994";
        public const string PurchasePriceVariance = "4CDC56EE-DAF3-49DB-8BAD-766F5CD285E4";
        public const string PurchaseDiscount = "4AA4D940-115F-4E7E-A878-B818BC746DFC";
        public const string BankCharge = "57953A76-6F03-4F95-AF8A-B8FCD81A2029";
        public const string VatOnSalesPayable = "91CC1EE1-25FD-441E-90FB-464472C11498";
        public const string GainLossInventoryVariance = "5EA472D9-EAFE-4918-83BF-933CBB1AA22E";
        
        //commission
        public const string MonthlyDiscount = "9D42889F-A3CA-4445-87D8-DBC5811C44CF";
        public const string YearlyDiscount = "C3EEB823-A66F-4C64-9CA0-6F9EA087E5B6";
        public const string TargetDiscount = "4143CF72-F126-4662-B852-8EAECF19673C"; 
        public const string CommissionPayable = "13A4746B-FE51-49D8-B87B-6012A4078E50";

        public static bool IsAccountIdExistsInConstants(Guid accountId)
        {
            foreach (var field in typeof(AccountHeadConstants).GetFields())
            {
                if (field.FieldType == typeof(string) && Guid.TryParse(field.GetValue(null)!.ToString(), out Guid guidValue))
                {
                    if (guidValue == accountId)
                    {
                        return true;
                    }
                }
            }

            return false; // Guid not found
        }
      public  static List<Guid> GetAllGuidsInConstants()
        {
            var guids = new List<Guid>();

            foreach (var field in typeof(AccountHeadConstants).GetFields())
            {
                if (field.FieldType == typeof(string) && Guid.TryParse(field.GetValue(null)!.ToString(), out Guid guidValue))
                {
                    guids.Add(guidValue);
                }
            }

            return guids;
        }

    }
}
