import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class SettingApiService {
  baseURL = environment.apiURL + "/setting";

  constructor(private httpClient: HttpClient) {}

  getSettingValue(key: string) {
    return this.httpClient.get(this.baseURL + `/${key}`);
  }
}
