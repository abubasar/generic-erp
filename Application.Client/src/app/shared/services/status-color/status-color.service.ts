import { Injectable } from "@angular/core";

@Injectable({
  providedIn: "root",
})
export class StatusColorService {
  constructor() {}

  getRequisitionStatus(value) {
    switch (value) {
      case 1:
        return { color: "accent", status: `Pending` };
      case 2:
        return { color: "warn", status: `Checked` };
      case 3:
        return { color: "primary", status: `Approved` };
      case 4:
        return { color: "warn", status: `Ready for RFQ Send` };
      case 5:
        return { color: "warn", status: `RFQ Sent` };
      case 6:
        return { color: "accent", status: `Vendor Selected` };
      case 7:
        return { color: "accent", status: `Order Created` };
      case 8:
        return { color: "accent", status: `Order Partial` };
      case 9:
        return { color: "primary", status: `Order Complete` };
      default:
        return { color: "", status: "" };
    }
  }

  getPriority(value: number) {
    switch (value) {
      case 1:
        return { color: "accent", status: `High` };
      case 2:
        return { color: "primary", status: `Medium` };
      case 3:
        return { color: "warn", status: `Low` };
      default:
        return { color: "", status: "" };
    }
  }

  getVendorQuotationStatus(value) {
    switch (value) {
      case 1:
        return { color: "accent", status: `Pending` };
      case 2:
        return { color: "primary", status: `Approved` };
      case 3:
        return { color: "warn", status: `Canceled` };
      default:
        return { color: "", status: `` };
    }
  }

  getPurchaseOrderStatus(value) {
    switch (value) {
      case 1:
        return { color: "accent", status: `Pending` };
      case 2:
        return { color: "primary", status: `Checked` };
      case 3:
        return { color: "warn", status: `Approved` };
      case 4:
        return { color: "warn", status: `Sent To Supplier` };
      case 5:
        return { color: "primary", status: `Item Partially Received` };
      case 6:
        return { color: "primary", status: `Item Completely Received` };
      case 7:
        return { color: "warn", status: `Closed` };
      case 8:
        return { color: "primary", status: `Ready For GRN` };
      default:
        return { color: "", status: "" };
    }
  }

  getGoodsReceiveNoteStatus(value) {
    switch (value) {
      case 1:
        return { color: "accent", status: `Pending` };
      case 2:
        return { color: "primary", status: `Checked` };
      case 3:
        return { color: "warn", status: `Approved` };
      case 4:
        return { color: "warn", status: `Invoice Generated` };
      default:
        return { color: "", status: "" };
    }
  }

  getPurchaseInvoiceStatus(value: number) {
    switch (value) {
      case 1:
        return { color: "accent", status: `Pending` };
      case 2:
        return { color: "primary", status: `Checked` };
      case 3:
        return { color: "primary", status: `Approved` };
      default:
        return { color: "", status: "" };
    }
  }

  getBOMStatus(value) {
    switch (value) {
      case 1:
        return { color: "accent", status: `Pending` };
      case 2:
        return { color: "warn", status: `Checked` };
      case 3:
        return { color: "primary", status: `Approved` };
      default:
        return { color: "", status: "" };
    }
  }

  getStockAdjustmentStatus(value) {
    switch (value) {
      case 1:
        return { color: "accent", status: `Pending` };
      case 2:
        return { color: "warn", status: `Checked` };
      case 3:
        return { color: "primary", status: `Approved` };
      default:
        return { color: "", status: "" };
    }
  }

  getManufacturingOrderStatus(value) {
    switch (value) {
      case 1:
        return { color: "accent", status: `Pending` };
      case 2:
        return { color: "warn", status: `Checked` };
      case 3:
        return { color: "primary", status: `Approved` };
      default:
        return { color: "", status: "" };
    }
  }

  getProductionStatus(value) {
    switch (value) {
      case 1:
        return { color: "accent", status: `Pending` };
      case 2:
        return { color: "warn", status: `Checked` };
      case 3:
        return { color: "primary", status: `Approved` };
      default:
        return { color: "", status: "" };
    }
  }

  getSaleQuotationStatus(value) {
    switch (value) {
      case 1:
        return { color: "accent", status: `Pending` };
      case 2:
        return { color: "warn", status: `Checked` };
      case 3:
        return { color: "primary", status: `Approved` };
      case 4:
        return { color: "warn", status: `Sale Order Created` };
      default:
        return { color: "", status: "" };
    }
  }

  getVoucherEntryStatus(value) {
    switch (value) {
      case 1:
        return { color: "accent", status: `Pending` };
      case 2:
        return { color: "warn", status: `Checked` };
      case 3:
        return { color: "primary", status: `Approved` };
      default:
        return { color: "", status: "" };
    }
  }

  getJournalEntryStatus(value) {
    switch (value) {
      case 1:
        return { color: "accent", status: `Pending` };
      case 2:
        return { color: "warn", status: `Checked` };
      case 3:
        return { color: "primary", status: `Approved` };
      default:
        return { color: "", status: "" };
    }
  }

  getSaleOrderStatus(value) {
    switch (value) {
      case 1:
        return { color: "accent", status: `Pending` };
      case 2:
        return { color: "warn", status: `Checked` };
      case 3:
        return { color: "primary", status: `Approved` };
      case 4:
        return { color: "Warn", status: `Item Partially Delivered` };
      case 5:
        return { color: "primary", status: `Item Completely Delivered` };
      case 6:
        return { color: "warn", status: `Closed` };
      case 7:
        return { color: "warn", status: `Partially Invoiced` };
      case 8:
        return { color: "primary", status: `Fully Invoiced` };
      default:
        return { color: "", status: "" };
    }
  }

  getPurchaseReturnStatus(value) {
    switch (value) {
      case 1:
        return { color: "accent", status: `Pending` };
      case 2:
        return { color: "warn", status: `Checked` };
      case 3:
        return { color: "primary", status: `Approved` };
      default:
        return { color: "", status: "" };
    }
  }

  getPoPriceAdjustmentAfterGrnStatus(value) {
    switch (value) {
      case 1:
        return { color: "accent", status: `Pending` };
      case 2:
        return { color: "warn", status: `Checked` };
      case 3:
        return { color: "primary", status: `Approved` };
      default:
        return { color: "", status: "" };
    }
  }

  getSaleReturnStatus(value) {
    switch (value) {
      case 1:
        return { color: "accent", status: `Pending` };
      case 2:
        return { color: "warn", status: `Checked` };
      case 3:
        return { color: "primary", status: `Approved` };
      default:
        return { color: "", status: "" };
    }
  }

  getDeliveryNoteStatus(value) {
    switch (value) {
      case 1:
        return { color: "accent", status: `Pending` };
      case 2:
        return { color: "warn", status: `Checked` };
      case 3:
        return { color: "primary", status: `Approved` };
      default:
        return { color: "", status: "" };
    }
  }

  getSupplierPaymentAgainstPurchaseStatus(value) {
    switch (value) {
      case 1:
        return { color: "accent", status: `Pending` };
      case 2:
        return { color: "warn", status: `Checked` };
      case 3:
        return { color: "primary", status: `Approved` };
      default:
        return { color: "", status: "" };
    }
  }

  getSaleInvoiceStatus(value: number) {
    switch (value) {
      case 1:
        return { color: "accent", status: `Pending` };
      case 2:
        return { color: "primary", status: `Checked` };
      case 3:
        return { color: "primary", status: `Approved` };
      default:
        return { color: "", status: "" };
    }
  }

  getStockTransferStatus(value: number) {
    switch (value) {
      case 1:
        return { color: "accent", status: `Pending` };
      case 2:
        return { color: "primary", status: `Checked` };
      case 3:
        return { color: "primary", status: `Approved` };
      default:
        return { color: "", status: "" };
    }
  }

  getReceivePaymentAgainstSaleStatus(value: number) {
    switch (value) {
      case 1:
        return { color: "accent", status: `Pending` };
      case 2:
        return { color: "primary", status: `Checked` };
      case 3:
        return { color: "primary", status: `Approved` };
      default:
        return { color: "", status: "" };
    }
  }

  getReceivePaymentStatus(value) {
    switch (value) {
      case 1:
        return { color: "accent", status: `Pending` };
      case 2:
        return { color: "warn", status: `Checked` };
      case 3:
        return { color: "primary", status: `Approved` };
      default:
        return { color: "", status: "" };
    }
  }

  getSupplierPaymentStatus(value) {
    switch (value) {
      case 1:
        return { color: "accent", status: `Pending` };
      case 2:
        return { color: "warn", status: `Checked` };
      case 3:
        return { color: "primary", status: `Approved` };
      default:
        return { color: "", status: "" };
    }
  }

  getFundTransferStatus(value) {
    switch (value) {
      case 1:
        return { color: "accent", status: `Pending` };
      case 2:
        return { color: "warn", status: `Checked` };
      case 3:
        return { color: "primary", status: `Approved` };
      default:
        return { color: "", status: "" };
    }
  }

  getPaymentVoucherStatus(value) {
    switch (value) {
      case 1:
        return { color: "accent", status: `Pending` };
      case 2:
        return { color: "warn", status: `Checked` };
      case 3:
        return { color: "primary", status: `Approved` };
      default:
        return { color: "", status: "" };
    }
  }

  getReceiveVoucherStatus(value) {
    switch (value) {
      case 1:
        return { color: "accent", status: `Pending` };
      case 2:
        return { color: "warn", status: `Checked` };
      case 3:
        return { color: "primary", status: `Approved` };
      default:
        return { color: "", status: "" };
    }
  }

  getCustomerWiseProductDiscountStatus(value) {
    switch (value) {
      case 1:
        return { color: "accent", status: `Pending` };
      case 2:
        return { color: "warn", status: `Checked` };
      case 3:
        return { color: "primary", status: `Approved` };
      default:
        return { color: "", status: "" };
    }
  }

  getLCCostEntryStatus(value) {
    switch (value) {
      case 1:
        return { color: "accent", status: `Pending` };
      case 2:
        return { color: "warn", status: `Checked` };
      case 3:
        return { color: "primary", status: `Approved` };
      case 4:
        return { color: "primary", status: `Calculated Within Landed Cost` };
      default:
        return { color: "", status: "" };
    }
  }

  getLcAdjustmentStatus(value) {
    switch (value) {
      case 1:
        return { color: "accent", status: `Pending` };
      case 2:
        return { color: "warn", status: `Checked` };
      case 3:
        return { color: "primary", status: `Approved` };
      default:
        return { color: "", status: "" };
    }
  }
}
