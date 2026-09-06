namespace Application.Core.Enums
{
    public enum AccountTransactonType
    {
        Purchase = 1,
        PurchaseReturn = 2,
        SupplierPayment = 3,//voucher entry
        OtherPayment=4,//voucher entry
        Sale = 5,
        SaleReturn = 6,
        CustomerReceipt = 7,//voucher entry
        OtherReceipt = 8,//voucher entry
        JournalEntry = 9,
        FundTransfer = 10,
        LcCostEntry = 11,
        LcAdjustment = 12,
    }
}
