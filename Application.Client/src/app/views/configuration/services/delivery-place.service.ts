import { Injectable } from '@angular/core';
import { DeliveryPlaceApiService } from 'app/shared/api/configuration/delivery-place-api.service';
import { GeneralResponse } from 'app/shared/models/wrappers/generalResponse.model';
import { SearchResponse } from 'app/shared/models/wrappers/searchResponse.model';
import { map, Observable } from 'rxjs';
import { DeliveryPlaceRequest } from '../models/delivery-place/delivery-place-request.model';
import { DeliveryPlace } from '../models/delivery-place/delivery-place.model';

@Injectable({
  providedIn: 'root'
})
export class DeliveryPlaceService {
  constructor(private api: DeliveryPlaceApiService) {}

  getAllDeliveryPlaces(): Observable<SearchResponse<DeliveryPlace>> {
    return this.api
      .getAlls()
      .pipe(map((response: SearchResponse<DeliveryPlace>) => response));
  }
  getDeliveryPlaces(
    deliveryPlaceRequest: DeliveryPlaceRequest
  ): Observable<SearchResponse<DeliveryPlace>> {
    return this.api
      .getAll(deliveryPlaceRequest)
      .pipe(map((response: SearchResponse<DeliveryPlace>) => response));
  }

  createDeliveryPlace(deliveryPlace: DeliveryPlace): Observable<GeneralResponse<string>> {
    return this.api
      .create(deliveryPlace)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateDeliveryPlace(deliveryPlace: DeliveryPlace): Observable<GeneralResponse<string>> {
    return this.api
      .update(deliveryPlace)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteDeliveryPlace(id: string): Observable<GeneralResponse<string>> {
    return this.api.delete(id).pipe(map((response: GeneralResponse<string>) => response));
  }
}
