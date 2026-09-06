import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { Inventory_Type_Id_Raw_Materials } from "app/shared/consts/const";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ProductRequest } from "app/views/configuration/models/product/product-request.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { StoreRequest } from "app/views/configuration/models/store/store-request.model";
import { Store } from "app/views/configuration/models/store/store.model";
import { ProductService } from "app/views/configuration/services/product.service";
import { StoreService } from "app/views/configuration/services/store.service";
import { PurchaseOrderResponseDTO } from "app/views/purchase/models/purchase-order/purchase-order-response-dto.model";
import { PurchaseOrderSearchRequestDTO } from "app/views/purchase/models/purchase-order/purchase-order-search-request-dto.model";
import { PurchaseOrderService } from "app/views/purchase/services/purchase-order.service";
import { environment } from "environments/environment";

@Component({
  selector: "app-grn-item-supplier-wise-report",
  templateUrl: "./grn-item-supplier-wise-report.component.html",
  styleUrls: ["./grn-item-supplier-wise-report.component.scss"],
})
export class GrnItemSupplierWiseReportComponent implements OnInit {
  businessType: string;
  constructor(
    private http: HttpClient,
    private fb: FormBuilder,
    private productService: ProductService,
    private storeService: StoreService,
    private purchaseOrderService: PurchaseOrderService,
    private jwtAuth: JwtAuthService
  ) {}

  searchForm: FormGroup;
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  products: ProductView[];
  filterProducts: ProductView[];
  stores: Store[];
  purchaseOrders: PurchaseOrderResponseDTO[];
  purchaseOrdersNo: PurchaseOrderResponseDTO[];
  purchaseOrderRequest = new PurchaseOrderSearchRequestDTO();

  ngOnInit(): void {
    this.initializeForm();
    this.jwtAuth.userProfile.subscribe((res: UserProfile) => {
      this.businessType = res.businesstype;
    });
    this.getAllProducts();
    this.getAllStores();
    this.getPurchaseOrdersNo();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      fromDate: ["", Validators.required],
      toDate: ["", Validators.required],
      productId: [null],
      storeId: [null],
      ponumber: [""],
      reportType: [1, Validators.required],
    });

    /**
     * * this code will be executed for instant changes of SearchForm's field
     */
    this.searchForm.valueChanges.subscribe((value) => {
      // Convert empty string to null
      if (value.productId === "") {
        this.searchForm
          .get("productId")
          ?.patchValue(null, { emitEvent: false });
      }
    });
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    if (fieldName === "productId") {
      this.searchForm?.get("productId").setValue(null);
    }
  }

  /**---------- Product Autocomplete------------ */
  onProductChange(value: string): void {
    const term = value.toLowerCase();
    this.filterProduct(term || "");
  }

  private filterProduct(value: string): void {
    this.filterProducts = this.products?.filter((option) =>
      option.name.toLowerCase().includes(value)
    );
  }

  getProductName(productId: string) {
    if (!productId) {
      return;
    }
    const product = this.products?.find((product) => product?.id === productId);
    if (!product) return "";
    return (
      product?.name +
      (this.businessType === "2" ? "" : ` (${product?.packSize?.name})`)
    );
  }

  getAllProducts(): void {
    let productRequest = new ProductRequest();
    //productRequest.inventoryTypeId = Inventory_Type_Id_Raw_Materials;
    productRequest.isPurchaseProduct = true;
    productRequest.page = -1;
    this.productService.getProducts(productRequest).subscribe((res) => {
      this.filterProducts = this.products = res?.data?.item1;
    });
  }
  /**---------- End Autocomplete----------- */

  getAllStores() {
    let storeRequest = new StoreRequest();
    storeRequest.inventoryTypeId = Inventory_Type_Id_Raw_Materials;
    this.storeService.getStores(storeRequest).subscribe((res) => {
      this.stores = res?.data.item1;
    });
  }

  getPurchaseOrdersNo(): void {
    this.purchaseOrderRequest.page = -1;
    this.purchaseOrderRequest.purchaseOrderStatuses = [5, 6, 7];
    this.purchaseOrderService
      .getPurchaseOrders(this.purchaseOrderRequest)
      .subscribe((res) => {
        this.purchaseOrdersNo = this.purchaseOrders = res?.data?.item1;
      });
  }

  onPONoChange(): void {
    const term = this.searchForm.get("ponumber");
    this._filterPONo(term.value || "");
  }

  private _filterPONo(value: string) {
    const filterValue = value.toLowerCase().trim();
    const supplierId = this.searchForm.value.supplierId;
    if (supplierId) {
      this.purchaseOrdersNo = this.purchaseOrders?.filter(
        (x) =>
          x.ponumber.toLowerCase().includes(filterValue) &&
          x.supplierId === supplierId
      );
    } else {
      this.purchaseOrdersNo = this.purchaseOrders?.filter((x) =>
        x.ponumber.toLowerCase().includes(filterValue)
      );
    }
  }

  printPdf($event) {
    if ($event?.submitter?.name == "pdf") {
      this.isLoading1 = true;
      this.searchForm.get("reportType").setValue(1);
    }
    if ($event?.submitter?.name == "excel") {
      this.isLoading2 = true;
      this.searchForm.get("reportType").setValue(2);
    }
    this.http
      .post(
        environment.apiURL + "/goodsReceiveNote/grn-item-supplier-wise-print",
        this.searchForm.value,
        {
          responseType: "blob",
        }
      )
      .subscribe((response) => {
        if ($event?.submitter?.name == "pdf") {
          //Create a Blob from the PDF Stream
          const file = new Blob([response], { type: "application/pdf" });
          //Build a URL from the file
          const fileURL = URL.createObjectURL(file);
          //Open the URL on new Window
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
          link.setAttribute("download", "grn_item_supplier_wise.xlsx");
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }
}
