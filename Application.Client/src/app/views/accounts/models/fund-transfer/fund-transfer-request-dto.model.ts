export interface FundTransferRequestDTO {
  id?: string;
  fundTransferDate: string;
  fundTransferTransactionTypeId: string;
  costCenterId: string;
  transferFromAccountId: string;
  transferToAccountId: string;
  amount: number;
  charges: number;
  remark: string;
}
