import { Injectable } from "@angular/core";
import { SettingApiService } from "app/shared/api/configuration/setting-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { map, Observable } from "rxjs";

@Injectable({
  providedIn: "root",
})
export class SettingService {
  constructor(private api: SettingApiService) {}

  getSettingValue(key: string): Observable<GeneralResponse<string>> {
    return this.api
      .getSettingValue(key)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
}
