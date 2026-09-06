import { Injectable } from "@angular/core";
import { GenericApiService } from "app/shared/api/configuration/generic-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { map, Observable } from "rxjs";
import { GenericRequest } from "../models/generic/generic-request.model";
import { Generic } from "../models/generic/generic.model";

@Injectable({
  providedIn: "root",
})
export class GenericService {
  constructor(private api: GenericApiService) {}

  getAllGenerics(): Observable<SearchResponse<Generic>> {
    return this.api
      .getAlls()
      .pipe(map((response: SearchResponse<Generic>) => response));
  }

  getGenerics(
    genericRequest: GenericRequest
  ): Observable<SearchResponse<Generic>> {
    return this.api
      .getAll(genericRequest)
      .pipe(map((response: SearchResponse<Generic>) => response));
  }

  createGeneric(generic: Generic): Observable<GeneralResponse<string>> {
    return this.api
      .create(generic)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateGeneric(generic: Generic): Observable<GeneralResponse<string>> {
    return this.api
      .update(generic)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteGeneric(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
}
