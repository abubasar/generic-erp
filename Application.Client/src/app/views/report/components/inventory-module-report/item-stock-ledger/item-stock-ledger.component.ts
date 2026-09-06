import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { Inventory_Type_Id_Finished_Goods } from "app/shared/consts/const";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { InventoryType } from "app/views/configuration/models/inventory-type/inventory-type.model";
import { ProductRequest } from "app/views/configuration/models/product/product-request.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { Store } from "app/views/configuration/models/store/store.model";
import { InventoryTypeService } from "app/views/configuration/services/inventory-type.service";
import { ProductService } from "app/views/configuration/services/product.service";
import { StoreService } from "app/views/configuration/services/store.service";
import { environment } from "environments/environment";

@Component({
  selector: "app-item-stock-ledger",
  templateUrl: "./item-stock-ledger.component.html",
  styleUrls: ["./item-stock-ledger.component.scss"],
})
export class ItemStockLedgerComponent implements OnInit {
  Inventory_Type_Id_Finished_Goods_ID: string;
  businessType: string;
  searchForm: FormGroup;
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  stores: Store[];
  products: ProductView[];
  inventoryTypes: InventoryType[];
  filteredProducts: ProductView[];
  filteredStores: Store[];

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private storeService: StoreService,
    private productService: ProductService,
    private inventoryTypeService: InventoryTypeService,
    private jwtAuth: JwtAuthService
  ) {}

  ngOnInit(): void {
    this.Inventory_Type_Id_Finished_Goods_ID = Inventory_Type_Id_Finished_Goods;
    this.jwtAuth.userProfile.subscribe((res) => {
      this.businessType = res.businesstype;
    });
    this.getAllStores();
    this.getAllProducts("");
    this.getAllInventoryTypes();
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      fromDate: [null, Validators.required],
      toDate: [null, Validators.required],
      inventoryTypeId: [null, Validators.required],
      productId: [null, Validators.required],
      storeId: [null],
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

  getAllStores() {
    this.storeService.getAllStores().subscribe((res) => {
      this.stores = res?.data.item1;
    });
  }

  /**---------- Product Autocomplete------------ */
  onProductChange(value: string): void {
    const term = value.toLocaleLowerCase();
    this.filterProduct(term || "");
  }

  private filterProduct(value: string): void {
    this.filteredProducts = this.products?.filter(
      (product) =>
        product.name.toLowerCase().startsWith(value) &&
        product.inventoryTypeId == this.searchForm.value?.inventoryTypeId
    );
  }

  getAllProducts(keyword: string): void {
    let productRequest = new ProductRequest();
    productRequest.keyword = keyword;
    productRequest.page = -1;
    this.productService.getProducts(productRequest).subscribe((res) => {
      this.products = res?.data?.item1;
    });
  }

  getProductName(productId: string) {
    if (!productId) {
      return;
    }
    const product = this.products?.find((product) => product?.id === productId);
    if (!product) return "";
    return product?.name + this.getProductNameDetails(product);
  }

  private getProductNameDetails(product: any): string {
    if (
      product?.inventoryType?.id !== this.Inventory_Type_Id_Finished_Goods_ID
    ) {
      return "";
    }

    return this.businessType === "2"
      ? ` (${product?.bagWeight} ${product?.measurementUnit?.name})`
      : ` (${product?.packSize?.name})`;
  }

  getAllInventoryTypes() {
    this.inventoryTypeService.getAllInventoryTypes().subscribe((res) => {
      this.inventoryTypes = res?.data?.item1;
    });
  }

  onSelectedInventoryType(id: string) {
    this.searchForm.patchValue({
      productId: null,
      storeId: null,
    });
    this.filterProduct("");
    this.filteredStores = this.stores?.filter((x) => x.inventoryTypeId == id);
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
        environment.apiURL + "/Report/item-stock-ledger/print",
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
          link.setAttribute(
            "download",
            "stock_ledger_summary_report_detail.xlsx"
          );
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }
}
