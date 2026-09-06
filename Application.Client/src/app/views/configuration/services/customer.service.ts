import { Injectable } from "@angular/core";
import { CustomerApiService } from "app/shared/api/configuration/customer-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { map, Observable } from "rxjs";
import { CreditLimitBalance } from "../models/customer/credit-limit-balance.model";
import { CustomerBalance } from "../models/customer/customer-balance.model";
import { CustomerRequest } from "../models/customer/customer-request.model";
import { Customer } from "../models/customer/customer.model";

@Injectable({
  providedIn: "root",
})
export class CustomerService {
  constructor(private api: CustomerApiService) {}

  getAllCustomers(): Observable<SearchResponse<Customer>> {
    return this.api
      .getAlls()
      .pipe(map((response: SearchResponse<Customer>) => response));
  }

  getCustomers(
    customerRequest: CustomerRequest
  ): Observable<SearchResponse<Customer>> {
    return this.api
      .getAll(customerRequest)
      .pipe(map((response: SearchResponse<Customer>) => response));
  }

  createCustomer(customer: Customer): Observable<GeneralResponse<string>> {
    return this.api
      .create(customer)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateCustomer(customer: Customer): Observable<GeneralResponse<string>> {
    return this.api
      .update(customer)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteCustomer(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  getCustomerCreditLimitBalance(
    id: string
  ): Observable<GeneralResponse<CreditLimitBalance>> {
    return this.api
      .creditLimitBalance(id)
      .pipe(map((response: GeneralResponse<CreditLimitBalance>) => response));
  }

  getCustomerBalance(id: string): Observable<GeneralResponse<CustomerBalance>> {
    return this.api
      .customerBalance(id)
      .pipe(map((response: GeneralResponse<CustomerBalance>) => response));
  }
}
