import { Injectable } from "@angular/core";
import { CostCenterApiService } from "app/shared/api/configuration/cost-center-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { map, Observable } from "rxjs";
import { CostCenterRequest } from "../models/cost-center/cost-center-request.model";
import { CostCenter } from "../models/cost-center/cost-center.model";

@Injectable({
  providedIn: "root",
})
export class CostCenterService {
  constructor(private api: CostCenterApiService) {}

  getAllCostCenters(): Observable<SearchResponse<CostCenter>> {
    return this.api
      .getAlls()
      .pipe(map((response: SearchResponse<CostCenter>) => response));
  }

  getCostCenters(
    costCenterRequest: CostCenterRequest
  ): Observable<SearchResponse<CostCenter>> {
    return this.api
      .getAll(costCenterRequest)
      .pipe(map((response: SearchResponse<CostCenter>) => response));
  }

  createCostCenter(
    costCenter: CostCenter
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(costCenter)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateCostCenter(
    costCenter: CostCenter
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(costCenter)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteCostCenter(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
}
