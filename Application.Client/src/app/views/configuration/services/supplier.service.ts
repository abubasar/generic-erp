import { Injectable } from "@angular/core";
import { SupplierApiService } from "app/shared/api/configuration/supplier-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { map, Observable } from "rxjs";
import { SupplierRequest } from "../models/supplier/supplier-request.model";
import { Supplier } from "../models/supplier/supplier.model";

@Injectable({
  providedIn: "root",
})
export class SupplierService {
  constructor(private api: SupplierApiService) {}

  getAllSuppliers(): Observable<SearchResponse<Supplier>> {
    return this.api
      .getAlls()
      .pipe(map((response: SearchResponse<Supplier>) => response));
  }

  getSuppliers(
    supplierRequest: SupplierRequest
  ): Observable<SearchResponse<Supplier>> {
    return this.api
      .getAll(supplierRequest)
      .pipe(map((response: SearchResponse<Supplier>) => response));
  }

  createSupplier(supplier: Supplier): Observable<GeneralResponse<string>> {
    return this.api
      .create(supplier)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateSupplier(supplier: Supplier): Observable<GeneralResponse<string>> {
    return this.api
      .update(supplier)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteSupplier(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
}
