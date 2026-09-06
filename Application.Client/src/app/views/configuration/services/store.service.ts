import { Injectable } from "@angular/core";
import { StoreApiService } from "app/shared/api/configuration/store-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { map, Observable } from "rxjs";
import { StoreRequest } from "../models/store/store-request.model";
import { Store } from "../models/store/store.model";

@Injectable({
  providedIn: "root",
})
export class StoreService {
  constructor(private api: StoreApiService) {}

  getAllStores(): Observable<SearchResponse<Store>> {
    return this.api
      .getAlls()
      .pipe(map((response: SearchResponse<Store>) => response));
  }
  getStores(storeRequest: StoreRequest): Observable<SearchResponse<Store>> {
    return this.api
      .getAll(storeRequest)
      .pipe(map((response: SearchResponse<Store>) => response));
  }

  createStore(store: Store): Observable<GeneralResponse<string>> {
    return this.api
      .create(store)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateStore(store: Store): Observable<GeneralResponse<string>> {
    return this.api
      .update(store)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteStore(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
}
