import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { BillOfMaterialRequestDTO } from "app/views/production/models/bill-of-material/bill-of-material-request-dto.model";
import { BillOfMaterialSearchRequestDTO } from "app/views/production/models/bill-of-material/bill-of-material-search-request-dto.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class BillOfMaterialApiService {
  baseURL = environment.apiURL + "/billOfMaterial";

  constructor(private httpClient: HttpClient) {}

  getAll(request: BillOfMaterialSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  getById(id: string) {
    return this.httpClient.get(this.baseURL + `/${id}`);
  }

  getPendingCheckedCount() {
    return this.httpClient.get(this.baseURL + "/pending-checked");
  }
  create(billOfMaterial: BillOfMaterialRequestDTO) {
    return this.httpClient.post(this.baseURL, billOfMaterial);
  }
  update(billOfMaterial: BillOfMaterialRequestDTO) {
    return this.httpClient.post(this.baseURL + "/update", billOfMaterial);
  }
  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }
  check(id: string) {
    return this.httpClient.post(this.baseURL + `/check/${id}`, {});
  }
  approve(id: string) {
    return this.httpClient.post(this.baseURL + `/approve/${id}`, {});
  }
  unpost(id: string, status: number) {
    return this.httpClient.post(this.baseURL + `/unpost/${id}/${status}`, {});
  }
}
