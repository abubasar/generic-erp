import { Injectable } from "@angular/core";
import { TerritoryApiService } from "app/shared/api/configuration/territory-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { map, Observable } from "rxjs";
import { TerritoryRequest } from "../models/Territory/territory-request.model";
import { Territory } from "../models/Territory/territory.model";

@Injectable({
  providedIn: "root",
})
export class TerritoryService {
  constructor(private api: TerritoryApiService) {}

  getAllTerritories(): Observable<SearchResponse<Territory>> {
    return this.api
      .getAlls()
      .pipe(map((response: SearchResponse<Territory>) => response));
  }

  getTerritories(
    territoryRequest: TerritoryRequest
  ): Observable<SearchResponse<Territory>> {
    return this.api
      .getAll(territoryRequest)
      .pipe(map((response: SearchResponse<Territory>) => response));
  }

  createTerritory(territory: Territory): Observable<GeneralResponse<string>> {
    return this.api
      .create(territory)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateTerritory(territory: Territory): Observable<GeneralResponse<string>> {
    return this.api
      .update(territory)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteTerritory(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
}
