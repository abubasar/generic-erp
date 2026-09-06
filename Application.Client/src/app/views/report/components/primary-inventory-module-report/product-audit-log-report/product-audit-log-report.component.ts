import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { ProductService } from "app/views/configuration/services/product.service";
import { environment } from "environments/environment";

@Component({
  selector: "app-product-audit-log-report",
  templateUrl: "./product-audit-log-report.component.html",
  styleUrls: ["./product-audit-log-report.component.scss"],
})
export class ProductAuditLogReportComponent implements OnInit {
  constructor(
    private http: HttpClient,
    private fb: FormBuilder,
    private productService: ProductService
  ) {}
  searchForm: FormGroup;
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  products: ProductView[];
  filterProducts: ProductView[];
  ngOnInit(): void {
    this.initializeForm();
    this.getAllProducts();
  }
  initializeForm() {
    this.searchForm = this.fb.group({
      fromDate: [null, Validators.required],
      toDate: [null, Validators.required],
      productId: [null],
      reportType: [1, Validators.required],
    });
  }

  /**---------- Product Autocomplete------------ */
  onProductChange(value: string): void {
    const term = value.toLocaleLowerCase();
    this.filterProduct(term || "");
  }

  private filterProduct(value: string): void {
    this.filterProducts = this.products?.filter((option) =>
      option.name.toLowerCase().includes(value)
    );
  }

  getAllProducts(): void {
    this.productService.getAllProducts().subscribe((res) => {
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
    return product?.name + " " + "(" + product.packSize.name + ")";
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    if (fieldName === "productId") {
      this.searchForm?.get("productId").setValue(null);
    }
  }
  /**---------- End Autocomplete----------- */

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
        environment.apiURL + "/ProductAudit/product_audit_report/print",
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
          link.setAttribute("download", "Product_Change_History.xlsx");
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }
}
