import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ProductRequest } from "app/views/configuration/models/product/product-request.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { ProductService } from "app/views/configuration/services/product.service";
import { environment } from "environments/environment";

@Component({
  selector: "app-primary-customer-wise-product-wise-sales-quantity-report",
  templateUrl:
    "./primary-customer-wise-product-wise-sales-quantity-report.component.html",
  styleUrls: [
    "./primary-customer-wise-product-wise-sales-quantity-report.component.scss",
  ],
})
export class PrimaryCustomerWiseProductWiseSalesQuantityReportComponent
  implements OnInit
{
  businessType: string;
  constructor(
    private http: HttpClient,
    private fb: FormBuilder,
    private productService: ProductService,
    private jwtAuth: JwtAuthService
  ) {}
  searchForm: FormGroup;
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  products: ProductView[];
  filterProducts: ProductView[];

  ngOnInit(): void {
    this.jwtAuth.userProfile.subscribe((res: UserProfile) => {
      this.businessType = res.businesstype;
    });
    this.initializeForm();
    this.getAllProducts();
  }
  initializeForm() {
    this.searchForm = this.fb.group({
      fromDate: [null, Validators.required],
      toDate: [null, Validators.required],
      productId: [null],
      customerId: [null],
      storeId: [null],
      customerRegionId: [null],
      customerZoneId: [null],
      customerAreaId: [null],
      customerTerritoryId: [null],
      customerMarketingOfficerId: [null],
      reportType: [1, Validators.required],
    });

    /**
     * * this code will be executed for instant changes of SearchForm's field
     */
    this.searchForm.valueChanges.subscribe((value) => {
      ["productId"].forEach((field) => {
        if (value[field] === "") {
          this.searchForm.get(field)?.patchValue(null, { emitEvent: false });
        }
      });
    });
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
      (this.businessType === "2"
        ? ` (${product.bagWeight} ${product?.measurementUnit?.name})`
        : ` (${product?.packSize?.name})`)
    );
  }

  getAllProducts(): void {
    let productRequest = new ProductRequest();
    productRequest.isSaleProduct = true;
    productRequest.page = -1;
    this.productService.getProducts(productRequest).subscribe((res) => {
      this.filterProducts = this.products = res?.data?.item1;
    });
  }

  /**---------- End Autocomplete----------- */
  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    this.searchForm.patchValue({ [fieldName]: null });
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
        environment.apiURL +
          "/SaleInvoice/primary-customer-wise-product-wise-sales-quantity-print",
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
          link.setAttribute("download", "Customer_Product_Sales_Quantity.xlsx");
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }
}
