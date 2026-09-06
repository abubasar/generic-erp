import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { ShiftRequest } from "app/views/configuration/models/shift/shift-request.model";
import { Shift } from "app/views/configuration/models/shift/shift.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class ShiftApiService {
  baseURL = environment.apiURL + "/shift";

  constructor(private httpClient: HttpClient) {}

  getAlls() {
    return this.httpClient.post(this.baseURL + "/search", { page: -1 });
  }
  getAll(request: ShiftRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }
  create(shift: Shift) {
    return this.httpClient.post(this.baseURL, shift);
  }
  update(shift: Shift) {
    return this.httpClient.post(this.baseURL + "/update", shift);
  }
  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }
}
