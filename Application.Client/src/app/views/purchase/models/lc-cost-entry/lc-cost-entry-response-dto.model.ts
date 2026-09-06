import { Account } from "app/views/configuration/models/account/account.model";
import { CostCenter } from "app/views/configuration/models/cost-center/cost-center.model";
import { PurchaseOrderResponseDTO } from "../purchase-order/purchase-order-response-dto.model";

export interface LCCostEntryResponseDTO {
  id: string;
  code: string;
  purchaseOrderId: string;
  ponumber: string;
  lcNumber: string;
  entryDate: string;
  costCenterId: string;
  total: number;
  status: number;
  statusName: string;
  remark: string;
  checkedBy: string;
  approvedBy: string;
  createdOn: string;
  updatedOn: string;
  createdBy: string;
  updatedBy: string;
  financialYearId: string;
  costCenter: CostCenter;
  purchaseOrder: PurchaseOrderResponseDTO;
  lccostEntryDetails: LCCostEntryResponseDetail[];
}

export interface LCCostEntryResponseDetail {
  id?: string;
  lccostEntryId?: string;
  debitAccountId: string;
  creditAccountId: string;
  amount: number;
  debitAccount?: Account;
  creditAccount?: Account;
  isIncludedWithinLandedCost?: boolean;
}
