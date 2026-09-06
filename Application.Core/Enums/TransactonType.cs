namespace Application.Core.Enums
{
    public enum TransactonType
    {
        Purchase = 1,
        Issue = 2,
        Production = 3,
        Sale = 4,
        TransferIssue = 5,
        TransferReceive = 6,
        SaleReturn = 7,
        PurchaseReturn = 8,
        StockAdjustment_Plus= 9,
        StockAdjustment_Minus = 10
    }
    public enum StockReferenceType
    {
        GoodsReceiveNote = 1,
        ManufacturingOrder = 2,
        Production = 3,
        DeliveryNote = 4,
        StockTransfer = 5,
        SaleReturn = 6,
        PurchaseReturn = 7,
        StockAdjustment = 8
    }
}
