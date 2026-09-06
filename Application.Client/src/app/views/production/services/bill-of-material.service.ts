import { Injectable } from "@angular/core";
import { BillOfMaterialApiService } from "app/shared/api/production/bill-of-material-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { PendingCheckedCount } from "app/views/purchase/models/pending-checked-count";
import { map, Observable } from "rxjs";
import { BillOfMaterialRequestDTO } from "../models/bill-of-material/bill-of-material-request-dto.model";
import { BillOfMaterialResponseDTO } from "../models/bill-of-material/bill-of-material-response-dto.model";
import { BillOfMaterialSearchRequestDTO } from "../models/bill-of-material/bill-of-material-search-request-dto.model";

@Injectable({
  providedIn: "root",
})
export class BillOfMaterialService {
  constructor(private api: BillOfMaterialApiService) {}

  getBillOfMaterials(
    billOfMaterialRequest: BillOfMaterialSearchRequestDTO
  ): Observable<SearchResponse<BillOfMaterialResponseDTO>> {
    return this.api
      .getAll(billOfMaterialRequest)
      .pipe(
        map((response: SearchResponse<BillOfMaterialResponseDTO>) => response)
      );
  }

  getBillOfMaterialById(
    id: string
  ): Observable<GeneralResponse<BillOfMaterialResponseDTO>> {
    return this.api
      .getById(id)
      .pipe(
        map((response: GeneralResponse<BillOfMaterialResponseDTO>) => response)
      );
  }

  getPendingCheckedCount(): Observable<GeneralResponse<PendingCheckedCount>> {
    return this.api
      .getPendingCheckedCount()
      .pipe(map((response: GeneralResponse<PendingCheckedCount>) => response));
  }
  createBillOfMaterial(
    billOfMaterialDTO: BillOfMaterialRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(billOfMaterialDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateBillOfMaterial(
    billOfMaterialDTO: BillOfMaterialRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(billOfMaterialDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteBillOfMaterial(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  checkBillOfMaterial(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .check(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  approveBillOfMaterial(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .approve(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }
  unpostBillOfMaterial(id: string, status: number): Observable<GeneralResponse<number>> {
    return this.api
      .unpost(id, status)
      .pipe(map((response: GeneralResponse<number>) => response));
  }
}
