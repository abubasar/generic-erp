export interface GeneralResponse<T> {
  data?: T;
  error?: string;
  message?: string;
  succeeded?: boolean;
  statusCode?: number;
  versionNumber?: string;
  validationErrors?: ErrorModel[];
  source?: string;
  exception?: string;
}
export interface GeneralResponse2<T> {
  data?: T[];
  error?: string;
  message?: string;
  succeeded?: boolean;
  statusCode?: number;
  versionNumber?: string;
  source?: string;
  exception?: string;
}
export interface ErrorModel {
  fieldName?: string;
  message?: string;
}
