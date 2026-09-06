import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { Inventory_Type_Id_Raw_Materials } from "app/shared/consts/const";
import { ProductTypeRequest } from "app/views/configuration/models/product-type/product-type-request.model";
import { ProductType } from "app/views/configuration/models/product-type/product-type.model";
import { StoreRequest } from "app/views/configuration/models/store/store-request.model";
import { Store } from "app/views/configuration/models/store/store.model";
import { ProductTypeService } from "app/views/configuration/services/product-type.service";
import { StoreService } from "app/views/configuration/services/store.service";
import { environment } from "environments/environment";

@Component({
  selector: "app-raw-materials-stock-report",
  templateUrl: "./raw-materials-stock-report.component.html",
  styleUrls: ["./raw-materials-stock-report.component.scss"],
})
export class RawMaterialsStockReportComponent implements OnInit {
  constructor(
    private http: HttpClient,
    private fb: FormBuilder,
    private storeService: StoreService,
    private productTypeService: ProductTypeService
  ) {}
  searchForm: FormGroup;
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  stores: Store[];
  filterStores: Store[];
  productTypes: ProductType[];
  filterProductTypes: ProductType[];
  ngOnInit(): void {
    this.initializeForm();
    this.getAllProductTypes();
    this.getAllStores();
  }
  initializeForm() {
    this.searchForm = this.fb.group({
      fromDate: [null, Validators.required],
      toDate: [null, Validators.required],
      storeId: [null, Validators.required],
      productTypeId: [null],
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
      if (value.productTypeId === "") {
        this.searchForm
          .get("productTypeId")
          ?.patchValue(null, { emitEvent: false });
      }
    });
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    if (fieldName === "productTypeId") {
      this.searchForm?.get("productTypeId").setValue(null);
    }
    if (fieldName === "storeId") {
      this.searchForm?.get("storeId").setValue(null);
    }
  }

  handleStoreSearch(): void {
    const term = this.searchForm?.get("storeId")?.value;
    this.filterStore(term || "");
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
    storeRequest.inventoryTypeId = Inventory_Type_Id_Raw_Materials;
    this.storeService.getStores(storeRequest).subscribe((res) => {
      this.filterStores = this.stores = res?.data.item1;
    });
  }

  handleProductTypeSearch(): void {
    const term = this.searchForm?.get("productTypeId")?.value;
    this.filterProductType(term || "");
  }

  private filterProductType(value: string) {
    const filterValue = value.toLowerCase();
    this.filterProductTypes = this.productTypes?.filter((option) =>
      option.name.toLowerCase().includes(filterValue)
    );
  }

  getProductTypeName(productTypeId: string) {
    if (!productTypeId) {
      return;
    }
    const productType = this.productTypes?.find(
      (productType) => productType?.id === productTypeId
    );
    return productType?.name;
  }

  getAllProductTypes(): void {
    let productTypeRequest = new ProductTypeRequest();
    productTypeRequest.page = -1;
    productTypeRequest.inventoryTypeId = Inventory_Type_Id_Raw_Materials;
    this.productTypeService
      .getProductTypes(productTypeRequest)
      .subscribe((res) => {
        this.filterProductTypes = this.productTypes = res?.data.item1;
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
        environment.apiURL + "/Report/raw-materials-stock/print",
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
          link.setAttribute("download", "Item_RM_Stock_Report.xlsx");
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }
}
