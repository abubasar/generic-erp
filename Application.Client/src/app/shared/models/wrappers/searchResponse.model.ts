import { Data } from "./data.model";

export class SearchResponse<T> {
  data: Data<T>;
  error: string;
  message: string;
  statusCode?: number;
  succeeded: boolean;
}
