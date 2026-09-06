import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { Inventory_Type_Id_Raw_Materials } from "app/shared/consts/const";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ProductType } from "app/views/configuration/models/product-type/product-type.model";
import { ProductRequest } from "app/views/configuration/models/product/product-request.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { ProductTypeService } from "app/views/configuration/services/product-type.service";
import { ProductService } from "app/views/configuration/services/product.service";
import { environment } from "environments/environment";

@Component({
  selector: "app-current-stock-purchase-rate-and-quantity",
  templateUrl: "./current-stock-purchase-rate-and-quantity.component.html",
  styleUrls: ["./current-stock-purchase-rate-and-quantity.component.scss"],
})
export class CurrentStockPurchaseRateAndQuantityComponent implements OnInit {
  searchForm: FormGroup;
  businessType: string;
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  products: ProductView[];
  filteredProducts: ProductView[];
  productTypes: ProductType[];
  filteredProductTypes: ProductType[];

  constructor(
    private http: HttpClient,
    private fb: FormBuilder,
    private productService: ProductService,
    private productTypeService: ProductTypeService,
    private jwtAuth: JwtAuthService,
  ) {}

  ngOnInit(): void {
    this.jwtAuth.userProfile.subscribe((res) => {
      this.businessType = res.businesstype;
    });
    this.initializeForm();
    this.getAllProductTypes();
    this.getAllProducts();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      toDate: [null, Validators.required],
      productId: [null],
      productTypeId: [null],
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

  clearInput(evt: any): void {
    evt.stopPropagation();
    this.searchForm?.get("productId").setValue(null);
  }

  getAllProductTypes() {
    this.productTypeService.getAllProductTypes().subscribe((res) => {
      this.productTypes = res?.data?.item1.filter(
        (productType) =>
          productType.inventoryTypeId === Inventory_Type_Id_Raw_Materials,
      );
    });
  }

  onSelectedProductType(id: string) {
    this.searchForm?.patchValue({
      productId: null,
    });
    this.filteredProducts = this.products?.filter(
      (product) => product.productTypeId === id,
    );
  }

  //**---------- Product Autocomplete------------ */

  getAllProducts(): void {
    let productRequest = new ProductRequest();
    productRequest.inventoryTypeId = Inventory_Type_Id_Raw_Materials;
    productRequest.isPurchaseProduct = true;
    productRequest.page = -1;
    this.productService.getProducts(productRequest).subscribe((res) => {
      this.filteredProducts = this.products = res?.data?.item1;
    });
  }

  onProductChange(value: string): void {
    const term = value.toLowerCase();
    this.filterProduct(term || "");
  }

  private filterProduct(value: string): void {
    this.filteredProducts = this.products?.filter((option) =>
      option.name.toLowerCase().includes(value),
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
    if (product?.inventoryType?.id !== Inventory_Type_Id_Raw_Materials) {
      return "";
    }

    return this.businessType === "2"
      ? ` (${product?.bagWeight} ${product?.measurementUnit?.name})`
      : ` (${product?.packSize?.name})`;
  }
  /**---------- End Autocomplete----------- */

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
        environment.apiURL +
          "/goodsReceiveNote/current_stock_purchase_rate_and_quantity",
        this.searchForm.value,
        {
          responseType: "blob",
        },
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
