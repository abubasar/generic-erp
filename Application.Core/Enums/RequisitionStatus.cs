namespace Application.Core.Enums
{
    public enum RequisitionStatus
    {
        Pending = 1,
        Checked = 2,
        Approved = 3,
        RFQ_Ready_For_Send = 4,
        RFQSent = 5,
        Vendor_Selected = 6,
        Order_Created = 7,
        Order_Partial = 8,
        Order_Complete = 9
    }
}
