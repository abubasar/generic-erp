export interface LCCostEntryRequestDTO {
  id?: string;
  purchaseOrderId: string;
  ponumber: string;
  lcNumber: string;
  entryDate: string;
  costCenterId?: string;
  total: number;
  remark: string;
  lccostEntryDetails: LCCostEntryRequestDetail[];
  deletedLCCostEntryDetailIds?: string;
}

interface LCCostEntryRequestDetail {
  id?: string;
  debitAccountId: string;
  creditAccountId: string;
  amount: number;
  isIncludedWithinLandedCost: boolean;
}
