import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { CostCenterRequest } from "app/views/configuration/models/cost-center/cost-center-request.model";
import { CostCenter } from "app/views/configuration/models/cost-center/cost-center.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class CostCenterApiService {
  baseURL = environment.apiURL + "/costCenter";

  constructor(private httpClient: HttpClient) {}

  getAlls() {
    return this.httpClient.post(this.baseURL + "/all", {});
  }

  getAll(request: CostCenterRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  create(costCenter: CostCenter) {
    return this.httpClient.post(this.baseURL, costCenter);
  }

  update(costCenter: CostCenter) {
    return this.httpClient.post(this.baseURL + "/update", costCenter);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }
}
