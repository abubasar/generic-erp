export class ChartOfAccount {
  id: string;
  code: string;
  name: string;
  type: string;
  level: number;
  parentId: string;
  subAccountHeads: ChartOfAccount[];
}
