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
  selector: "app-day-wise-consumption-rate",
  templateUrl: "./day-wise-consumption-rate.component.html",
  styleUrls: ["./day-wise-consumption-rate.component.scss"],
})
export class DayWiseConsumptionRateComponent implements OnInit {
  searchForm: FormGroup;
  rawMaterialsProductTypes: ProductType[];
  filterRawMaterialsProductTypes: ProductType[];
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  stores: Store[];

  constructor(
    private http: HttpClient,
    private fb: FormBuilder,
    private productTypeService: ProductTypeService,
    private storeService: StoreService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.getAllProductTypes();
    // this.getAllStores();
  }
  initializeForm() {
    this.searchForm = this.fb.group({
      fromDate: [null, Validators.required],
      toDate: [null, Validators.required],
      productTypeId: [null, Validators.required],
      storeId: [null],
      reportType: [1, Validators.required],
    });

    /**
     * * this code will be executed for instant changes of SearchForm's field
     */
    this.searchForm.valueChanges.subscribe((value) => {
      // Convert empty string to null
      if (value.productTypeId === "") {
        this.searchForm
          .get("productTypeId")
          ?.patchValue(null, { emitEvent: false });
      }
    });
  }

  clearInput(evt: any): void {
    evt.stopPropagation();
    this.searchForm?.get("productTypeId").setValue(null);
  }

  /**---------- ProductType Autocomplete------------ */
  onProductTypeChange(value: string): void {
    const term = value.toLowerCase();
    this.filterProductType(term || "");
  }

  private filterProductType(value: string): void {
    this.filterRawMaterialsProductTypes = this.rawMaterialsProductTypes?.filter(
      (option) => option.name.toLowerCase().includes(value)
    );
  }

  getProductTypeName(productTypeId: string) {
    if (!productTypeId) {
      return;
    }
    const productType = this.rawMaterialsProductTypes?.find(
      (productType) => productType?.id === productTypeId
    );
    return productType?.name;
  }

  getAllProductTypes(): void {
    let productTypeRequest = new ProductTypeRequest();
    productTypeRequest.inventoryTypeId = Inventory_Type_Id_Raw_Materials;
    this.productTypeService
      .getProductTypes(productTypeRequest)
      .subscribe((res) => {
        this.filterRawMaterialsProductTypes = this.rawMaterialsProductTypes =
          res?.data?.item1;
      });
  }
  /**---------- End Autocomplete----------- */

  getAllStores() {
    let storeRequest = new StoreRequest();
    storeRequest.page = -1;
    storeRequest.inventoryTypeId = Inventory_Type_Id_Raw_Materials;
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
        environment.apiURL + "/Report/day-wise-consumption-rate/print",
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
          link.setAttribute("download", "Day_Wise_Consumption_Rate.xlsx");
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }
}
