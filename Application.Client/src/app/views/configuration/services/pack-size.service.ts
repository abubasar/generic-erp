import { Injectable } from "@angular/core";
import { PackSizeApiService } from "app/shared/api/configuration/pack-size-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { map, Observable } from "rxjs";
import { PackSizeRequest } from "../models/pack-size/pack-size-request.model";
import { PackSize } from "../models/pack-size/pack-size.model";

@Injectable({
  providedIn: "root",
})
export class PackSizeService {
  constructor(private api: PackSizeApiService) {}

  getAllPackSizes(): Observable<SearchResponse<PackSize>> {
    return this.api
      .getAlls()
      .pipe(map((response: SearchResponse<PackSize>) => response));
  }

  getPackSizes(
    packSizeRequest: PackSizeRequest
  ): Observable<SearchResponse<PackSize>> {
    return this.api
      .getAll(packSizeRequest)
      .pipe(map((response: SearchResponse<PackSize>) => response));
  }

  createPackSize(packSize: PackSize): Observable<GeneralResponse<string>> {
    return this.api
      .create(packSize)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updatePackSize(packSize: PackSize): Observable<GeneralResponse<string>> {
    return this.api
      .update(packSize)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deletePackSize(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
}
