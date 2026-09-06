import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { UserRequest } from "app/views/configuration/models/user/user-request.model";
import { User } from "app/views/configuration/models/user/user.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class UserApiService {
  baseURL = environment.apiURL + "/user";

  constructor(private httpClient: HttpClient) {}

  getAll(request: UserRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  create(user: User) {
    return this.httpClient.post(this.baseURL, user);
  }

  update(user: User) {
    return this.httpClient.post(this.baseURL + "/update", user);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }
}
