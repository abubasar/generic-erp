import { Account } from "app/views/configuration/models/account/account.model";
import { CostCenter } from "app/views/configuration/models/cost-center/cost-center.model";
import { FundTransferTransactionType } from "app/views/configuration/models/fund-transfer-transaction-type/fund-transfer-transaction-type.model";

export interface FundTransferResponseDTO {
  id: string;
  fundTransferNo: string;
  fundTransferDate: string;
  fundTransferTransactionTypeId: string;
  costCenterId: string;
  transferFromAccountId: string;
  transferToAccountId: string;
  amount: number;
  charges: number;
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
  fundTransferTransactionType: FundTransferTransactionType;
  costCenter: CostCenter;
  transferFromAccount: Account;
  transferToAccount: Account;
}
