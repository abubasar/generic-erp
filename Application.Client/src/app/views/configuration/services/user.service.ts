import { Injectable } from "@angular/core";
import { UserApiService } from "app/shared/api/configuration/user-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { map, Observable } from "rxjs";
import { UserRequest } from "../models/user/user-request.model";
import { User } from "../models/user/user.model";

@Injectable({
  providedIn: "root",
})
export class UserService {
  constructor(private api: UserApiService) {}

  getUsers(userRequest: UserRequest): Observable<SearchResponse<User>> {
    return this.api
      .getAll(userRequest)
      .pipe(map((response: SearchResponse<User>) => response));
  }

  createUser(user: User): Observable<GeneralResponse<string>> {
    return this.api
      .create(user)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateUser(user: User): Observable<GeneralResponse<string>> {
    return this.api
      .update(user)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteUser(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
}
