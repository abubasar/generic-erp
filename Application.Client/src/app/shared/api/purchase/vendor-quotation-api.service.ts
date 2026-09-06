import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { VendorQuotationRequestDTO } from "app/views/purchase/models/vendor-quotation/vendor-quotation-request-dto.model";
import { VendorQuotationSearchRequestDTO } from "app/views/purchase/models/vendor-quotation/vendor-quotation-search-request-dto.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class VendorQuotationApiService {
  baseURL = environment.apiURL + "/vendorQuotation";

  constructor(private httpClient: HttpClient) {}

  getAll(request: VendorQuotationSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  getById(id: string) {
    return this.httpClient.get(this.baseURL + `/${id}`);
  }

  create(vendorQuotation: VendorQuotationRequestDTO) {
    return this.httpClient.post(this.baseURL, vendorQuotation);
  }

  update(vendorQuotation: VendorQuotationRequestDTO) {
    return this.httpClient.post(this.baseURL + "/update", vendorQuotation);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }
  approve(id: string, requisitionNo: string) {
    return this.httpClient.post(
      this.baseURL + `/approve/${id}/${requisitionNo}`,
      {}
    );
  }

  unpost(requisitionNo: string) {
    return this.httpClient.post(this.baseURL + `/unpost/${requisitionNo}`, {});
  }
}
