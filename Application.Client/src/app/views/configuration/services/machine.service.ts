import { Injectable } from "@angular/core";
import { MachineApiService } from "app/shared/api/configuration/machine-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { Observable, map } from "rxjs";
import { Machine } from "../models/machine/machine.model";
import { MachineRequest } from "../models/machine/machine-request.model";

@Injectable({
  providedIn: "root",
})
export class MachineService {
  constructor(private api: MachineApiService) {}

  getAllMachines(): Observable<SearchResponse<Machine>> {
    return this.api
      .getAlls()
      .pipe(map((response: SearchResponse<Machine>) => response));
  }
  getMachines(
    machineRequest: MachineRequest
  ): Observable<SearchResponse<Machine>> {
    return this.api
      .getAll(machineRequest)
      .pipe(map((response: SearchResponse<Machine>) => response));
  }

  createMachine(machine: Machine): Observable<GeneralResponse<string>> {
    return this.api
      .create(machine)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateMachine(machine: Machine): Observable<GeneralResponse<string>> {
    return this.api
      .update(machine)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteMachine(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
}
