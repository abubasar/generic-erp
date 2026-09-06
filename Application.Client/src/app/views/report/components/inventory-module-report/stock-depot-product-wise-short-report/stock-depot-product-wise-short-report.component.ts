import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { Inventory_Type_Id_Finished_Goods } from "app/shared/consts/const";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { InventoryType } from "app/views/configuration/models/inventory-type/inventory-type.model";
import { ProductType } from "app/views/configuration/models/product-type/product-type.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { Store } from "app/views/configuration/models/store/store.model";
import { InventoryTypeService } from "app/views/configuration/services/inventory-type.service";
import { ProductTypeService } from "app/views/configuration/services/product-type.service";
import { ProductService } from "app/views/configuration/services/product.service";
import { StoreService } from "app/views/configuration/services/store.service";
import { environment } from "environments/environment";

@Component({
  selector: "app-stock-depot-product-wise-short-report",
  templateUrl: "./stock-depot-product-wise-short-report.component.html",
  styleUrls: ["./stock-depot-product-wise-short-report.component.scss"],
})
export class StockDepotProductWiseShortReportComponent implements OnInit {
  Inventory_Type_Id_Finished_Goods_ID: string =
    Inventory_Type_Id_Finished_Goods;
  businessType: string;
  searchForm: FormGroup;
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  products: ProductView[];
  filteredProducts: ProductView[];
  inventoryTypes: InventoryType[];
  stores: Store[];
  filteredStores: Store[];
  productTypes: ProductType[];
  filteredProductTypes: ProductType[];

  constructor(
    private storeService: StoreService,
    private productService: ProductService,
    private inventoryTypeService: InventoryTypeService,
    private productTypeService: ProductTypeService,
    private http: HttpClient,
    private fb: FormBuilder,
    private jwtAuth: JwtAuthService
  ) {}

  ngOnInit(): void {
    this.jwtAuth.userProfile.subscribe((res) => {
      this.businessType = res.businesstype;
    });
    this.getAllInventoryTypes();
    this.getAllProductTypes();
    this.getAllProducts();
    this.getAllStores();
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group(
      {
        fromDate: [null, Validators.required],
        toDate: [null, Validators.required],
        inventoryTypeId: [null, Validators.required],
        productTypeId: [null],
        productId: [null],
        storeId: [null],
        isDepotWiseReport: [false],
        isProductWiseReport: [false],
        reportType: [1, Validators.required],
      },
      { validator: this.requireCheckboxesToBeCheckedValidator() }
    );

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

  requireCheckboxesToBeCheckedValidator(): Validators {
    return (formGroup: FormGroup) => {
      const isDepotWiseReport = formGroup.controls["isDepotWiseReport"];
      const isProductWiseReport = formGroup.controls["isProductWiseReport"];
      if (isDepotWiseReport.value || isProductWiseReport.value) {
        return null;
      } else {
        return { requireCheckboxesToBeChecked: true };
      }
    };
  }

  clearInput(evt: any): void {
    evt.stopPropagation();
    this.searchForm?.get("productId").setValue(null);
  }

  onCheckboxChange(controlName: string): void {
    if (controlName === "isDepotWiseReport") {
      this.searchForm.patchValue({ isProductWiseReport: false });
    }
    if (controlName === "isProductWiseReport") {
      this.searchForm.patchValue({ isDepotWiseReport: false });
    }
  }

  getAllInventoryTypes() {
    this.inventoryTypeService.getAllInventoryTypes().subscribe((res) => {
      this.inventoryTypes = res?.data?.item1;
    });
  }

  onSelectedInventoryType(id: string) {
    this.searchForm?.patchValue({
      productTypeId: null,
      productId: null,
      storeId: null,
    });
    this.filteredProductTypes = this.productTypes?.filter(
      (type) => type.inventoryTypeId === id
    );
    this.filteredStores = this.stores?.filter((x) => x.inventoryTypeId == id);
    this.filteredProducts = this.products?.filter(
      (x) => x.inventoryTypeId == id
    );
  }

  getAllProductTypes() {
    this.productTypeService.getAllProductTypes().subscribe((res) => {
      this.productTypes = res?.data?.item1;
    });
  }

  onSelectedProductType(id: string) {
    this.searchForm?.patchValue({
      productId: null,
      storeId: null,
    });
    this.filteredProducts = this.products?.filter(
      (product) => product.productTypeId === id
    );
  }

  getAllStores() {
    this.storeService.getAllStores().subscribe((res) => {
      this.stores = res?.data.item1;
    });
  }

  /**---------- Product Autocomplete------------ */
  getAllProducts() {
    this.productService.getAllProducts().subscribe((res) => {
      this.filteredProducts = this.products = res?.data?.item1;
    });
  }

  onProductChange(value: string): void {
    const term = value.toLowerCase();
    this.filterProduct(term || "");
  }

  private filterProduct(value: string): void {
    this.filteredProducts = this.products?.filter((option) =>
      option.name.toLowerCase().includes(value)
    );
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

  printPdf($event) {
    let excelReportName = "Stock_Report.xlsx";
    if (this.searchForm.get("isDepotWiseReport").value)
      excelReportName = "Stock_Report_Depot_Wise.xlsx";
    if (this.searchForm.get("isProductWiseReport").value)
      excelReportName = "Stock_Report_Product_Wise.xlsx";

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
        environment.apiURL +
          "/Report/stock-depot-product-wise-short-report/print",
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
          link.setAttribute("download", excelReportName);
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }
}
