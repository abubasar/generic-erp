import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { Inventory_Type_Id_Finished_Goods } from "app/shared/consts/const";
import { ProductRequest } from "app/views/configuration/models/product/product-request.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { StoreRequest } from "app/views/configuration/models/store/store-request.model";
import { Store } from "app/views/configuration/models/store/store.model";
import { ProductService } from "app/views/configuration/services/product.service";
import { StoreService } from "app/views/configuration/services/store.service";
import { environment } from "environments/environment";

@Component({
  selector: "app-production-details-report",
  templateUrl: "./production-details-report.component.html",
  styleUrls: ["./production-details-report.component.scss"],
})
export class ProductionDetailsReportComponent implements OnInit {
  searchForm: FormGroup;
  finishedProducts: ProductView[];
  filterFinishedProducts: ProductView[];
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  stores: Store[];

  constructor(
    private http: HttpClient,
    private fb: FormBuilder,
    private productService: ProductService,
    private storeService: StoreService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.getAllProducts();
    this.getAllStores();
  }
  initializeForm() {
    this.searchForm = this.fb.group({
      fromDate: [null, Validators.required],
      toDate: [null, Validators.required],
      finishedProductId: [null],
      fgstoreId: [null],
      reportType: [1, Validators.required],
    });

    /**
     * * this code will be executed for instant changes of SearchForm's field
     */
    this.searchForm.valueChanges.subscribe((value) => {
      // Convert empty string to null
      if (value.finishedProductId === "") {
        this.searchForm
          .get("finishedProductId")
          ?.patchValue(null, { emitEvent: false });
      }
    });
  }

  clearInput(evt: any): void {
    evt.stopPropagation();
    this.searchForm?.get("finishedProductId").setValue(null);
  }

  /**---------- Product Autocomplete------------ */
  onProductChange(value: string): void {
    const term = value.toLowerCase();
    this.filterProduct(term || "");
  }

  private filterProduct(value: string): void {
    this.filterFinishedProducts = this.finishedProducts?.filter((option) =>
      option.name.toLowerCase().includes(value)
    );
  }

  getProductName(productId: string) {
    if (!productId) {
      return;
    }
    const product = this.finishedProducts?.find(
      (product) => product?.id === productId
    );
    return product?.name;
  }

  getAllProducts(): void {
    let productRequest = new ProductRequest();
    productRequest.page = -1;
    productRequest.inventoryTypeId = Inventory_Type_Id_Finished_Goods;
    this.productService.getProducts(productRequest).subscribe((res) => {
      this.filterFinishedProducts = this.finishedProducts = res?.data?.item1;
    });
  }
  /**---------- End Autocomplete----------- */

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
        environment.apiURL + "/Production/production-details-print",
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
          link.setAttribute("download", "Production_Details_2.xlsx");
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }
}
