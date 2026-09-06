import { Injectable } from "@angular/core";
import { AreaApiService } from "app/shared/api/configuration/area-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { Observable, map } from "rxjs";
import { AreaRequest } from "../models/area/area-request.model";
import { Area } from "../models/area/area.model";
@Injectable({
  providedIn: "root",
})
export class AreaService {
  constructor(private api: AreaApiService) {}
  getAllAreas(): Observable<SearchResponse<Area>> {
    return this.api
      .getAlls()
      .pipe(map((response: SearchResponse<Area>) => response));
  }
  getAreas(areaRequest: AreaRequest): Observable<SearchResponse<Area>> {
    return this.api
      .getAll(areaRequest)
      .pipe(map((response: SearchResponse<Area>) => response));
  }
  createArea(area: Area): Observable<GeneralResponse<string>> {
    return this.api
      .create(area)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
  updateArea(area: Area): Observable<GeneralResponse<string>> {
    return this.api
      .update(area)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
  deleteArea(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
}
