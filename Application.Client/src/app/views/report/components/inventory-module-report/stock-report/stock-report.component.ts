import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MatTableDataSource } from "@angular/material/table";
import {
  Inventory_Type_Id_Finished_Goods,
  Page_Size_Options,
} from "app/shared/consts/const";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { InventoryType } from "app/views/configuration/models/inventory-type/inventory-type.model";
import { ProductType } from "app/views/configuration/models/product-type/product-type.model";
import { ProductRequest } from "app/views/configuration/models/product/product-request.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { Store } from "app/views/configuration/models/store/store.model";
import { InventoryTypeService } from "app/views/configuration/services/inventory-type.service";
import { ProductTypeService } from "app/views/configuration/services/product-type.service";
import { ProductService } from "app/views/configuration/services/product.service";
import { StoreService } from "app/views/configuration/services/store.service";
import { environment } from "environments/environment";
import { StockResponseDTO } from "../../../models/stock-response-dto";
import { StockSearchRequestDTO } from "../../../models/stock-search-request-dto";
import { StockService } from "../../../services/stock.service";
@Component({
  selector: "app-stock-report",
  templateUrl: "./stock-report.component.html",
  styleUrls: ["./stock-report.component.scss"],
})
export class StockReportComponent implements OnInit {
  Inventory_Type_Id_Finished_Goods_ID: string =
    Inventory_Type_Id_Finished_Goods;
  businessType: string;
  panelOpenState: boolean;
  searchForm: FormGroup;
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  displayedColumns: string[] = [
    "productName",
    "storeName",
    "quantity",
    "value",
  ];
  dataSource: MatTableDataSource<StockResponseDTO>;
  totalCount: number;
  stockRequest = new StockSearchRequestDTO();
  stores: Store[];
  filteredStores: Store[];
  products: ProductView[];
  filterProducts: ProductView[];
  inventoryTypes: InventoryType[];
  productTypes: ProductType[];
  mapProductTypes: ProductType[];
  pageSizeOptions: [] = Page_Size_Options;

  constructor(
    private stockService: StockService,
    private fb: FormBuilder,
    private http: HttpClient,
    private storeService: StoreService,
    private productService: ProductService,
    private inventoryTypeService: InventoryTypeService,
    private productTypeService: ProductTypeService,
    private jwtAuth: JwtAuthService
  ) {}

  ngOnInit(): void {
    this.jwtAuth.userProfile.subscribe((res) => {
      this.businessType = res.businesstype;
    });
    this.getStockData(this.stockRequest);
    this.getAllInventoryTypes();
    this.getAllProductTypes();
    this.getAllStores();
    this.getAllProducts("");
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      storeId: [null],
      inventoryTypeId: [null],
      productTypeId: [null],
      productId: [null],
      reportType: [1, Validators.required],
    });
  }

  getStockData(request: StockSearchRequestDTO): void {
    this.stockService.getStockData(request).subscribe((res) => {
      this.dataSource = new MatTableDataSource<StockResponseDTO>(
        res?.data?.item1
      );
      this.totalCount = res?.data?.item2;
    });
  }

  getAllInventoryTypes() {
    this.inventoryTypeService.getAllInventoryTypes().subscribe((res) => {
      this.inventoryTypes = res?.data?.item1;
    });
  }

  getAllProductTypes() {
    this.productTypeService.getAllProductTypes().subscribe((res) => {
      this.productTypes = res?.data?.item1;
    });
  }

  onSelectedInventoryType(id: string) {
    this.mapProductTypes = this.productTypes?.filter(
      (type) => type.inventoryTypeId === id
    );
    this.filteredStores = this.stores?.filter((x) => x.inventoryTypeId == id);
  }

  onSelectedProductType(id: string) {
    this.searchForm.get("productId").reset();
    this.getAllProducts("");
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
    this.getAllProducts(value);
  }

  getAllProducts(keyword: string): void {
    let productRequest = new ProductRequest();
    productRequest.productTypeId = this.searchForm?.value?.productTypeId;
    productRequest.keyword = keyword;
    productRequest.page = -1;
    this.productService.getProducts(productRequest).subscribe((res) => {
      this.products = res?.data?.item1;
      this.filterProducts = res?.data?.item1;
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
  /**---------- End Autocomplete----------- */

  onSearch() {
    const formValue = this.searchForm.value;
    this.getStockData(formValue);
  }

  onPageChange(pageEvent) {
    this.stockRequest.page = pageEvent.pageIndex;
    this.stockRequest.rowsPerPage = pageEvent.pageSize;
    this.stockRequest.inventoryTypeId = this.searchForm?.value?.inventoryTypeId;
    this.stockRequest.productTypeId = this.searchForm?.value?.productTypeId;
    this.stockRequest.productId = this.searchForm?.value?.productId;
    this.stockRequest.storeId = this.searchForm?.value?.storeId;
    this.getStockData(this.stockRequest);
  }

  printPdf(buttonName) {
    if (buttonName == "pdf") {
      this.isLoading1 = true;
      this.searchForm.get("reportType").setValue(1);
    }
    if (buttonName == "excel") {
      this.searchForm.get("reportType").setValue(2);
      this.isLoading2 = true;
    }
    this.http
      .post(environment.apiURL + "/Report/stock/print", this.searchForm.value, {
        responseType: "blob",
      })
      .subscribe((response) => {
        if (buttonName == "pdf") {
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
          link.setAttribute("download", "stock_report.xlsx");
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }
}
