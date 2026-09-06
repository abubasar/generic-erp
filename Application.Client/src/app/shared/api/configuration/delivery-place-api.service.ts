import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { DeliveryPlaceRequest } from 'app/views/configuration/models/delivery-place/delivery-place-request.model';
import { DeliveryPlace } from 'app/views/configuration/models/delivery-place/delivery-place.model';
import { environment } from 'environments/environment';

@Injectable({
  providedIn: 'root'
})
export class DeliveryPlaceApiService {
  baseURL = environment.apiURL + "/deliveryPlace";

  constructor(private httpClient: HttpClient) {}

  getAlls() {
    return this.httpClient.post(this.baseURL + "/search", { page: -1 });
  }
  getAll(request: DeliveryPlaceRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  create(deliveryPlace: DeliveryPlace) {
    return this.httpClient.post(this.baseURL, deliveryPlace);
  }

  update(deliveryPlace: DeliveryPlace) {
    return this.httpClient.post(this.baseURL + "/update", deliveryPlace);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }
}
