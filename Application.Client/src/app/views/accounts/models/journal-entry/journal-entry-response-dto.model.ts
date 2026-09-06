import { Account } from "app/views/configuration/models/account/account.model";
import { CostCenter } from "app/views/configuration/models/cost-center/cost-center.model";

export interface JournalEntryResponseDTO {
  id?: string;
  voucherNo: string;
  voucherDate: string;
  costCenterId: string;
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
  journalEntryDetails: JournalEntryResponseDetail[];
}

export interface JournalEntryResponseDetail {
  id?: string;
  journalEntryId?: string;
  accountId: string;
  accountDescription: string;
  postType: number;
  amount: number;
  account?: Account;
  isOpeningBalance?: boolean;
}
