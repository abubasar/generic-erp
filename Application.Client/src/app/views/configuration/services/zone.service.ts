import { Injectable } from "@angular/core";
import { ZoneApiService } from "app/shared/api/configuration/zone-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { map, Observable } from "rxjs";
import { ZoneRequest } from "../models/zone/zone-request.model";
import { Zone } from "../models/zone/zone.model";

@Injectable({
  providedIn: "root",
})
export class ZoneService {
  constructor(private api: ZoneApiService) {}
  getAllZones(): Observable<SearchResponse<Zone>> {
    return this.api
      .getAlls()
      .pipe(map((response: SearchResponse<Zone>) => response));
  }
  getZones(zoneRequest: ZoneRequest): Observable<SearchResponse<Zone>> {
    return this.api
      .getAll(zoneRequest)
      .pipe(map((response: SearchResponse<Zone>) => response));
  }
  createZone(zone: Zone): Observable<GeneralResponse<string>> {
    return this.api
      .create(zone)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
  updateZone(zone: Zone): Observable<GeneralResponse<string>> {
    return this.api
      .update(zone)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
  deleteZone(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
}
