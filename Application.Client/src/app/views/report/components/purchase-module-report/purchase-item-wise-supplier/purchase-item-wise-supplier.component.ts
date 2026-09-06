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
import { environment } from "environments/environment";

@Component({
  selector: "app-purchase-item-wise-supplier",
  templateUrl: "./purchase-item-wise-supplier.component.html",
  styleUrls: ["./purchase-item-wise-supplier.component.scss"],
})
export class PurchaseItemWiseSupplierComponent implements OnInit {
  searchForm: FormGroup;
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  // suppliers: Supplier[];
  // filterSuppliers: Supplier[];
  businessType: string;
  products: ProductView[];
  filterProducts: ProductView[];
  stores: Store[];
  constructor(
    private http: HttpClient,
    private fb: FormBuilder,
    // private supplierService: SupplierService,
    private productService: ProductService,
    private storeService: StoreService,
    private jwtAuth: JwtAuthService
  ) {}

  // Initialize form control with null

  ngOnInit(): void {
    this.initializeForm();
    this.jwtAuth.userProfile.subscribe((res: UserProfile) => {
      this.businessType = res.businesstype;
    });
    // this.getAllSuppliers();
    this.getAllStores();
    this.getAllProducts();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      fromDate: ["", Validators.required],
      toDate: ["", Validators.required],
      // supplierId: [null],
      productId: [null],
      storeId: [null],
      reportType: [1, Validators.required],
    });
  }

  /**---------- Product Autocomplete------------ */
  onProductChange(value: string): void {
    const term = value.toLocaleLowerCase();
    this.filterProduct(term || "");
  }

  private filterProduct(value: string) {
    const filterValue = value.toLowerCase();
    this.filterProducts = this.products?.filter((option) =>
      option.name.toLowerCase().includes(filterValue)
    );
  }

  getAllProducts(): void {
    let productRequest = new ProductRequest();
    //productRequest.inventoryTypeId = Inventory_Type_Id_Raw_Materials;
    productRequest.isPurchaseProduct = true;
    // productRequest.keyword = keyword;
    productRequest.page = -1;
    productRequest.isAscending = true;
    this.productService.getProducts(productRequest).subscribe((res) => {
      this.filterProducts = this.products = res?.data?.item1;
    });
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

  /**---------- End Autocomplete----------- */
  // handleSupplierSearch(event: any): void {
  //   const name = event.target?.name;
  //   if (name === "supplierId") {
  //     const term = this.searchForm.get("supplierId");
  //     this.filterSupplier(term.value || "");
  //   }
  // }

  // private filterSupplier(value: string) {
  //   const filterValue = value.toLowerCase();
  //   this.filterSuppliers = this.suppliers?.filter((option) =>
  //     option.name.toLowerCase().includes(filterValue)
  //   );
  // }

  // getSupplierName(supplierId: string) {
  //   if (!supplierId) {
  //     return;
  //   }
  //   const supplierAccount = this.suppliers?.find(
  //     (supplier) => supplier?.id === supplierId
  //   );
  //   return supplierAccount?.name;
  // }

  // getAllSuppliers() {
  //   this.supplierService.getAllSuppliers().subscribe((res) => {
  //     this.suppliers = res.data?.item1;
  //   });
  // }

  getAllStores() {
    let storeRequest = new StoreRequest();
    storeRequest.inventoryTypeId = Inventory_Type_Id_Raw_Materials;
    this.storeService.getStores(storeRequest).subscribe((res) => {
      this.stores = res?.data.item1;
    });
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    if (fieldName === "productId") {
      this.searchForm?.get("productId").setValue(null);
      this.filterProducts = this.products;
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
        environment.apiURL +
          "/PurchaseInvoice/purchase-item-wise-supplier-print",
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
          link.setAttribute("download", "purchase_item_supplier_wise.xlsx");
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }
}
