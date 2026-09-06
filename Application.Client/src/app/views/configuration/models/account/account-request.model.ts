import { BaseRequest } from "app/shared/models/wrappers/baseRequest.model";
export class AccountRequest extends BaseRequest {
  constructor(){
    super();
    this.isAscending=true;
    this.orderBy="code";
  }
  accountTypeId: string;
  parentId: string;
}
