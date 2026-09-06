import { config } from "config";

export const environment = {
  production: true,
  apiURL: config.apiUrl,
  baseURL:config.baseUrl
};