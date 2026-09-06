import { Injectable } from "@angular/core";
import { CategoryApiService } from "app/shared/api/configuration/category-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { map, Observable } from "rxjs";
import { CategoryRequest } from "../models/category/category-request.model";
import { Category } from "../models/category/category.model";

@Injectable({
  providedIn: "root",
})
export class CategoryService {
  constructor(private api: CategoryApiService) {}

  getAllCategories(): Observable<SearchResponse<Category>> {
    return this.api
      .getAlls()
      .pipe(map((response: SearchResponse<Category>) => response));
  }

  getCategories(
    categoryRequest: CategoryRequest
  ): Observable<SearchResponse<Category>> {
    return this.api
      .getAll(categoryRequest)
      .pipe(map((response: SearchResponse<Category>) => response));
  }

  createCategory(category: Category): Observable<GeneralResponse<string>> {
    return this.api
      .create(category)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateCategory(category: Category): Observable<GeneralResponse<string>> {
    return this.api
      .update(category)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteCategory(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
}
