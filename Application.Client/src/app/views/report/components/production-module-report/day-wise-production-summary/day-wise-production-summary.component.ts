import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { Inventory_Type_Id_Finished_Goods } from "app/shared/consts/const";
import { StoreRequest } from "app/views/configuration/models/store/store-request.model";
import { Store } from "app/views/configuration/models/store/store.model";
import { StoreService } from "app/views/configuration/services/store.service";
import { environment } from "environments/environment";
@Component({
  selector: "app-day-wise-production-summary",
  templateUrl: "./day-wise-production-summary.component.html",
  styleUrls: ["./day-wise-production-summary.component.scss"],
})
export class DayWiseProductionSummaryComponent implements OnInit {
  constructor(
    private http: HttpClient,
    private fb: FormBuilder,
    private storeService: StoreService
  ) {}

  searchForm: FormGroup;
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  stores: Store[];

  ngOnInit(): void {
    this.initializeForm();
    this.getAllStores();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      fromDate: [null, Validators.required],
      toDate: [null, Validators.required],
      fgstoreId: [null],
      reportType: [1, Validators.required],
    });
  }

  getAllStores() {
    let storeRequest = new StoreRequest();
    storeRequest.page = -1;
    storeRequest.inventoryTypeId = Inventory_Type_Id_Finished_Goods;
    this.storeService.getStores(storeRequest).subscribe((res) => {
      this.stores = res?.data.item1;
    });
  }

  printPdf($event) {
    if ($event?.submitter?.name == "pdf") {
      this.isLoading1 = true;
      this.searchForm.get("reportType").setValue(1);
    }
    if ($event?.submitter?.name == "excel") {
      this.searchForm.get("reportType").setValue(2);
      this.isLoading2 = true;
    }
    this.http
      .post(
        environment.apiURL + "/Report/day-wise-production-summary/print",
        this.searchForm.value,
        {
          responseType: "blob",
        }
      )
      .subscribe((response) => {
        if ($event?.submitter?.name == "pdf") {
          const file = new Blob([response], { type: "application/pdf" });
          const fileURL = URL.createObjectURL(file);
          const pdfWindow = window.open();
          pdfWindow.location.href = fileURL;
          this.isLoading1 = false;
        } else {
          const blob = new Blob([response], {
            type: "application/octet-stream",
          });
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement("a");
          link.href = url;
          link.setAttribute("download", "Day_Wise_Production_Summary.xlsx");
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }
}
