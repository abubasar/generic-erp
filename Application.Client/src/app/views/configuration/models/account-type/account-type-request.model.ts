import { BaseRequest } from "app/shared/models/wrappers/baseRequest.model";
export class AccountTypeRequest extends BaseRequest {
    constructor(){
        super();
         this.orderBy = "startingNumber";
         this.isAscending=true;
    }
}
