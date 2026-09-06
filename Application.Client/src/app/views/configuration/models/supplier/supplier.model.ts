export interface Supplier {
  id?: string;
  code?: string;
  name: string;
  ownersName: string;
  contactNo: string;
  email: string;
  address: string;
  nationalId: string;
  tradeLicense: string;
  contactPersonName: string;
  contactPersonContactNo: string;
  contactPersonEmail: string;
  contactPersonDesignation: string;
  suppliedProductIds: string[];
  bankAccounts: BankAccount[];
  deletedBankAccountIds?: string;
}

export interface BankAccount {
  id?: string;
  name: string;
  accNo: string;
  routingNo: string;
  bankName: string;
  branchName: string;
}
