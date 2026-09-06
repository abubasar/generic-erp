export interface Account {
  id?: string;
  code?: string;
  name: string;
  accountTypeId: string;
  accountTypeName?: string;
  level: number;
  parentId: string;
  parentName?: string;
  isControlAccount: boolean;
}
