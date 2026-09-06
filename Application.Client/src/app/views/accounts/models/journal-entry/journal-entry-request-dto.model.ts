export interface JournalEntryRequestDTO {
  id?: string;
  voucherDate: string;
  costCenterId: string;
  remark: string;
  journalEntryDetails: JournalEntryRequestDetail[];
  deletedJournalEntryDetailIds?: string;
}

interface JournalEntryRequestDetail {
  id?: string;
  accountId: string;
  accountDescription: string;
  postType: number;
  amount: number;
}
