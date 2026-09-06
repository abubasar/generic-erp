import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { CategoryRequest } from "app/views/configuration/models/category/category-request.model";
import { Category } from "app/views/configuration/models/category/category.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class CategoryApiService {
  baseURL = environment.apiURL + "/category";

  constructor(private httpClient: HttpClient) {}

  getAlls() {
    return this.httpClient.post(this.baseURL + "/all", {});
  }

  getAll(request: CategoryRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  create(category: Category) {
    return this.httpClient.post(this.baseURL, category);
  }

  update(category: Category) {
    return this.httpClient.post(this.baseURL + "/update", category);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }
}
