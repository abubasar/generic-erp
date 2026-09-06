import { Injectable } from "@angular/core";
import { VendorQuotationApiService } from "app/shared/api/purchase/vendor-quotation-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { map, Observable } from "rxjs";
import { VendorQuotationRequestDTO } from "../models/vendor-quotation/vendor-quotation-request-dto.model";
import { VendorQuotationResponseDTO } from "../models/vendor-quotation/vendor-quotation-response-dto.model";
import { VendorQuotationSearchRequestDTO } from "../models/vendor-quotation/vendor-quotation-search-request-dto.model";

@Injectable({
  providedIn: "root",
})
export class VendorQuotationService {
  constructor(private api: VendorQuotationApiService) {}

  getVendorQuotations(
    vendorQuotationRequest: VendorQuotationSearchRequestDTO
  ): Observable<SearchResponse<VendorQuotationResponseDTO>> {
    return this.api
      .getAll(vendorQuotationRequest)
      .pipe(
        map((response: SearchResponse<VendorQuotationResponseDTO>) => response)
      );
  }

  getVendorQuotationById(
    id: string
  ): Observable<GeneralResponse<VendorQuotationResponseDTO>> {
    return this.api
      .getById(id)
      .pipe(
        map((response: GeneralResponse<VendorQuotationResponseDTO>) => response)
      );
  }

  createVendorQuotation(
    vendorQuotationDTO: VendorQuotationRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(vendorQuotationDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateVendorQuotation(
    vendorQuotationDTO: VendorQuotationRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(vendorQuotationDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteVendorQuotation(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  approveVendorQuotation(
    id: string,
    requisitionNo: string
  ): Observable<GeneralResponse<number>> {
    return this.api
      .approve(id, requisitionNo)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  unpostVendorQuotation(
    requisitionNo: string
  ): Observable<GeneralResponse<number>> {
    return this.api
      .unpost(requisitionNo)
      .pipe(map((response: GeneralResponse<number>) => response));
  }
}
