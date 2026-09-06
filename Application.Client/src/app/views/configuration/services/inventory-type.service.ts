import { Injectable } from "@angular/core";
import { InventoryTypeApiService } from "app/shared/api/configuration/inventory-type-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { map, Observable } from "rxjs";
import { InventoryTypeRequest } from "../models/inventory-type/inventory-type-request.model";
import { InventoryType } from "../models/inventory-type/inventory-type.model";

@Injectable({
  providedIn: "root",
})
export class InventoryTypeService {
  constructor(private api: InventoryTypeApiService) {}

  getAllInventoryTypes(): Observable<SearchResponse<InventoryType>> {
    return this.api
      .getAlls()
      .pipe(map((response: SearchResponse<InventoryType>) => response));
  }
  getInventoryTypes(
    inventoryTypeRequest: InventoryTypeRequest
  ): Observable<SearchResponse<InventoryType>> {
    return this.api
      .getAll(inventoryTypeRequest)
      .pipe(map((response: SearchResponse<InventoryType>) => response));
  }

  createInventoryType(
    inventoryType: InventoryType
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(inventoryType)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateInventoryType(
    inventoryType: InventoryType
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(inventoryType)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteInventoryType(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
}
