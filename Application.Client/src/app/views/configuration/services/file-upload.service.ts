import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { environment } from "environments/environment";
import { Observable } from "rxjs";
import { FileUploadModel } from "../models/file-upload/file-upload.model";

@Injectable({
  providedIn: "root",
})
export class FileUploadService {
  baseURL = environment.apiURL + "/ReceivePayment";
  constructor(private httpClient: HttpClient) {}

  //Start of multiple file upload
  uploadFiles(files: FileUploadModel[]): Observable<any> {
    const formData = new FormData();
    files.forEach((file, index) => {
      formData.append(
        `receivePaymentPictureMappingCreationDto[${index}].FileDetails`,
        file.fileDetails
      );
      formData.append(
        `receivePaymentPictureMappingCreationDto[${index}].receivePaymentId`,
        file.receivePaymentId
      );
    });
    return this.httpClient.post<any>(
      this.baseURL + "/receive_payment_picture_mapping",
      formData
    );
  }
  //End of multiple file upload

  //Start of Single file upload
  uploadFile(file: FileUploadModel): Observable<any> {
    // Change parameter to single file
    const formData = new FormData();
    formData.append("creationDto.FileDetails", file.fileDetails);
    formData.append("creationDto.receivePaymentId", file.receivePaymentId);

    return this.httpClient.post<any>(this.baseURL + "/single_file", formData);
  }
  //End of Single file upload
}
