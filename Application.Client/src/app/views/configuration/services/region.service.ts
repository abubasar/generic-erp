import { Injectable } from "@angular/core";
import { RegionApiService } from "app/shared/api/configuration/region-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { map, Observable } from "rxjs";
import { RegionRequest } from "../models/region/region-request.model";
import { Region } from "../models/region/region.model";

@Injectable({
  providedIn: "root",
})
export class RegionService {
  constructor(private api: RegionApiService) {}

  getAllRegions(): Observable<SearchResponse<Region>> {
    return this.api
      .getAlls()
      .pipe(map((response: SearchResponse<Region>) => response));
  }
  getRegions(regionRequest: RegionRequest): Observable<SearchResponse<Region>> {
    return this.api
      .getAll(regionRequest)
      .pipe(map((response: SearchResponse<Region>) => response));
  }

  createRegion(region: Region): Observable<GeneralResponse<string>> {
    return this.api
      .create(region)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateRegion(region: Region): Observable<GeneralResponse<string>> {
    return this.api
      .update(region)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteRegion(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
}
