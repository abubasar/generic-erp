export class BaseRequest {
  keyword: string;
  page: number;
  rowsPerPage: number;
  orderBy: string;
  isAscending: boolean;

  constructor() {
    this.keyword = "";
    this.page = 0;
    this.rowsPerPage = 15;
    this.orderBy = "createdOn";
    this.isAscending = false;
  }
}
