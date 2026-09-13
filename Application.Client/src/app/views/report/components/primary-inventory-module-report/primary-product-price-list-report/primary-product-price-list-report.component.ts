import { HttpClient } from "@angular/common/http";
import { Component, OnInit, ChangeDetectionStrategy } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { IndustryProfileService } from "app/shared/services/industry-profile/industry-profile.service";
import { Category } from "app/views/configuration/models/category/category.model";
import { InventoryType } from "app/views/configuration/models/inventory-type/inventory-type.model";
import { CategoryService } from "app/views/configuration/services/category.service";
import { InventoryTypeService } from "app/views/configuration/services/inventory-type.service";
import { environment } from "environments/environment";

@Component({
    selector: "app-primary-product-price-list-report",
    templateUrl: "./primary-product-price-list-report.component.html",
    styleUrls: ["./primary-product-price-list-report.component.scss"],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class PrimaryProductPriceListReportComponent implements OnInit {
  panelOpenState: boolean;
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  searchForm: FormGroup;
  businessTypes: string;
  categories: Category[];
  inventoryTypes: InventoryType[];

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    public industryProfile: IndustryProfileService,
    private inventoryService: InventoryTypeService,
    private categoryService: CategoryService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.getAllInventoryTypes();
    if (this.industryProfile.isPharma) {
      this.getAllCategories();
    }
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      storeId: [null],
      inventoryTypeId: [null],
      categoryId: [null],
      productTypeId: [null],
      productId: [null],
      isPurchaseProduct: [false],
      isSaleProduct: [false],
      reportType: [1, Validators.required],
    });
  }

  getAllInventoryTypes() {
    this.inventoryService.getAllInventoryTypes().subscribe((response) => {
      this.inventoryTypes = response.data.item1;
    });
  }

  getAllCategories() {
    this.categoryService.getAllCategories().subscribe((response) => {
      {
        this.categories = response.data.item1;
      }
    });
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
        environment.apiURL + "/product/print-product-price-list",
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
          link.setAttribute("download", "stock_report.xlsx");
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }
}
