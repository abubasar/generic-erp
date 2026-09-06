import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MatTableDataSource } from "@angular/material/table";
import {
  Inventory_Type_Id_Finished_Goods,
  Page_Size_Options,
} from "app/shared/consts/const";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { Store } from "app/views/configuration/models/store/store.model";
import { ProductService } from "app/views/configuration/services/product.service";
import { StoreService } from "app/views/configuration/services/store.service";
import { environment } from "environments/environment";
import { StockResponseDTO } from "../../../models/stock-response-dto";
import { StockSearchRequestDTO } from "../../../models/stock-search-request-dto";
import { StockService } from "../../../services/stock.service";

@Component({
  selector: "app-low-stock-report",
  templateUrl: "./low-stock-report.component.html",
  styleUrls: ["./low-stock-report.component.scss"],
})
export class LowStockReportComponent implements OnInit {
  Inventory_Type_Id_Finished_Goods_ID: string = Inventory_Type_Id_Finished_Goods;
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
  products: ProductView[];
  filterProducts: ProductView[];
  pageSizeOptions: [] = Page_Size_Options;

  constructor(
    private stockService: StockService,
    private fb: FormBuilder,
    private http: HttpClient,
    private storeService: StoreService,
    private productService: ProductService,
    private jwtAuth: JwtAuthService
  ) {}

  ngOnInit(): void {
    this.jwtAuth.userProfile.subscribe((res: UserProfile) => {
      this.businessType = res.businesstype;
    });
    this.getAllStores();
    this.getAllProducts();
    this.getLowStockData(this.stockRequest);
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      storeId: [null],
      productId: [null],
      reportType: [1, Validators.required],
    });
  }

  getLowStockData(request: StockSearchRequestDTO): void {
    this.stockService.getLowStockData(request).subscribe((res) => {
      this.dataSource = new MatTableDataSource<StockResponseDTO>(
        res?.data?.item1
      );
      this.totalCount = res?.data?.item2;
    });
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
    // this.getAllProducts(value);
    this.filterProducts = this.products?.filter((option) =>
      option.name.toLowerCase().includes(value)
    );
  }

  getAllProducts(): void {
    // let productRequest = new ProductRequest();
    // productRequest.inventoryTypeId = Inventory_Type_Id_Raw_Materials;
    // productRequest.keyword = keyword;
    this.productService.getAllProducts().subscribe((res) => {
      this.products = res?.data?.item1;
      console.log(this.products);
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

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    if (fieldName === "productId") {
      this.searchForm?.get("productId").setValue(null);
    }
  }
  /**---------- End Autocomplete----------- */

  onSearch() {
    console.log(this.searchForm.value);
    const formValue = this.searchForm.value;
    this.getLowStockData(formValue);
  }

  onPageChange(pageEvent) {
    this.stockRequest.page = pageEvent.pageIndex;
    this.stockRequest.rowsPerPage = pageEvent.pageSize;
    this.stockRequest.productId = this.searchForm?.value?.productId;
    this.stockRequest.storeId = this.searchForm?.value?.storeId;
    this.getLowStockData(this.stockRequest);
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
      .post(
        environment.apiURL + "/Report/lowStock/print",
        this.searchForm.value,
        {
          responseType: "blob",
        }
      )
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
          link.setAttribute("download", "low_stock_report.xlsx");
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }
}
