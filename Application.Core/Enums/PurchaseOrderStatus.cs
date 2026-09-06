namespace Application.Core.Enums
{
    public enum PurchaseOrderStatus
    {
        Pending = 1,
        Checked = 2,
        Approved = 3,
        SentToSupplier = 4,
        Item_Partially_Received = 5,
        Item_Completely_Received = 6,
        Closed = 7,
        Ready_For_GRN = 8,
    }
}
