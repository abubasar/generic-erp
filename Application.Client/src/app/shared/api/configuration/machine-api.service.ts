import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { MachineRequest } from "app/views/configuration/models/machine/machine-request.model";
import { Machine } from "app/views/configuration/models/machine/machine.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class MachineApiService {
  baseURL = environment.apiURL + "/machine";

  constructor(private httpClient: HttpClient) {}

  getAlls() {
    return this.httpClient.post(this.baseURL + "/search", { page: -1 });
  }
  getAll(request: MachineRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  create(machine: Machine) {
    return this.httpClient.post(this.baseURL, machine);
  }

  update(machine: Machine) {
    return this.httpClient.post(this.baseURL + "/update", machine);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }
}
