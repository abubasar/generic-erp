import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { Inventory_Type_Id_Finished_Goods } from "app/shared/consts/const";
import { StoreRequest } from "app/views/configuration/models/store/store-request.model";
import { Store } from "app/views/configuration/models/store/store.model";
import { StoreService } from "app/views/configuration/services/store.service";
import { environment } from "environments/environment";

@Component({
  selector: "app-primary-finished-goods-stock-report",
  templateUrl: "./primary-finished-goods-stock-report.component.html",
  styleUrls: ["./primary-finished-goods-stock-report.component.scss"],
})
export class PrimaryFinishedGoodsStockReportComponent implements OnInit {
  constructor(
    private http: HttpClient,
    private fb: FormBuilder,
    private storeService: StoreService
  ) {}
  searchForm: FormGroup;
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  stores: Store[];
  filterStores: Store[];
  ngOnInit(): void {
    this.initializeForm();
    this.getAllStores();
  }
  initializeForm() {
    this.searchForm = this.fb.group({
      fromDate: [null, Validators.required],
      toDate: [null, Validators.required],
      storeId: [null],
      reportType: [1, Validators.required],
    });

    /**
     * * this code will be executed for instant changes of SearchForm's field
     */
    this.searchForm.valueChanges.subscribe((value) => {
      // Convert empty string to null
      if (value.storeId === "") {
        this.searchForm.get("storeId")?.patchValue(null, { emitEvent: false });
      }
    });
  }

  clearInput(evt: any): void {
    evt.stopPropagation();
    this.searchForm?.get("storeId").setValue(null);
  }

  handleStoreSearch(event: any): void {
    const name = event.target?.name;
    if (name === "storeId") {
      const term = this.searchForm.get("storeId");
      this.filterStore(term.value || "");
    }
  }

  private filterStore(value: string) {
    const filterValue = value.toLowerCase();
    this.filterStores = this.stores?.filter((option) =>
      option.name.toLowerCase().includes(filterValue)
    );
  }

  getStoreName(storeId: string) {
    if (!storeId) {
      return;
    }
    const store = this.stores?.find((store) => store?.id === storeId);
    return store?.name;
  }

  getAllStores(): void {
    let storeRequest = new StoreRequest();
    storeRequest.page = -1;
    storeRequest.inventoryTypeId = Inventory_Type_Id_Finished_Goods;
    this.storeService.getStores(storeRequest).subscribe((res) => {
      this.filterStores = this.stores = res?.data.item1;
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
        environment.apiURL + "/Report/primary-finished-goods-stock/print",
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
          link.setAttribute("download", "Finished_Goods_Stock_Report.xlsx");
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }
}
