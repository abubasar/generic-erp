import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import {
  Department_Id_SALES_AND_MARKETING,
  Inventory_Type_Id_Finished_Goods,
} from "app/shared/consts/const";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { AreaRequest } from "app/views/configuration/models/area/area-request.model";
import { Area } from "app/views/configuration/models/area/area.model";
import { EmployeeRequest } from "app/views/configuration/models/employee/employee-request.model";
import { Employee } from "app/views/configuration/models/employee/employee.model";
import { ProductRequest } from "app/views/configuration/models/product/product-request.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { StoreRequest } from "app/views/configuration/models/store/store-request.model";
import { Store } from "app/views/configuration/models/store/store.model";
import { Zone } from "app/views/configuration/models/zone/zone.model";
import { AreaService } from "app/views/configuration/services/area.service";
import { EmployeeService } from "app/views/configuration/services/employee.service";
import { ProductService } from "app/views/configuration/services/product.service";
import { StoreService } from "app/views/configuration/services/store.service";
import { ZoneService } from "app/views/configuration/services/zone.service";
import { environment } from "environments/environment";

@Component({
  selector: "app-sales-item-customer-wise",
  templateUrl: "./sales-item-customer-wise.component.html",
  styleUrls: ["./sales-item-customer-wise.component.scss"],
})
export class SalesItemCustomerWiseComponent implements OnInit {
  businessType: string;
  constructor(
    private http: HttpClient,
    private fb: FormBuilder,
    private storeService: StoreService,
    private zoneService: ZoneService,
    private areaService: AreaService,
    private employeeService: EmployeeService,
    private productService: ProductService,
    private jwtAuth: JwtAuthService,
  ) {}
  searchForm: FormGroup;
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  products: ProductView[];
  filterProducts: ProductView[];
  stores: Store[];
  zones: Zone[];
  isZoneSelected: boolean = false;
  areas: Area[];
  isAreaSelected: boolean = false;
  marketingOfficers: Employee[];

  ngOnInit(): void {
    this.jwtAuth.userProfile.subscribe((res: UserProfile) => {
      this.businessType = res.businesstype;
    });
    this.initializeForm();
    this.getAllProducts();
    this.getAllStores();
    this.getAllZones();
    this.getAllMarketingOfficers();
  }
  initializeForm() {
    this.searchForm = this.fb.group({
      fromDate: [null, Validators.required],
      toDate: [null, Validators.required],
      productId: [null],
      customerId: [null],
      storeId: [null],
      customerZoneId: [null],
      customerAreaId: [null],
      customerMarketingOfficerId: [null],
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

  /**---------- Product Autocomplete------------ */
  onProductChange(value: string): void {
    const term = value.toLowerCase();
    this.filterProduct(term || "");
  }

  private filterProduct(value: string): void {
    this.filterProducts = this.products?.filter((option) =>
      option.name.toLowerCase().includes(value),
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
    productRequest.page = -1;
    //productRequest.inventoryTypeId = Inventory_Type_Id_Finished_Goods;
    productRequest.isSaleProduct = true;
    this.productService.getProducts(productRequest).subscribe((res) => {
      this.filterProducts = this.products = res?.data?.item1;
    });
  }

  /**---------- End Autocomplete----------- */

  getAllZones() {
    this.zoneService.getAllZones().subscribe((res) => {
      this.zones = res?.data?.item1;
    });
  }
  onSelectedZoneId(id: string) {
    this.searchForm.patchValue({
      customerAreaId: null,
    });
    this.isZoneSelected = true;
    this.getAllAreasByZoneId(id);
  }

  getAllAreasByZoneId(zoneId: string) {
    let areaRequest = new AreaRequest();
    areaRequest.page = -1;
    areaRequest.zoneId = zoneId;
    this.areaService.getAreas(areaRequest).subscribe((res) => {
      this.areas = res?.data?.item1;
    });
  }

  onSelectedAreaId(id: string) {}

  getAllMarketingOfficers(): void {
    let employeeRequest = new EmployeeRequest();
    employeeRequest.page = -1;
    employeeRequest.departmentId = Department_Id_SALES_AND_MARKETING;
    this.employeeService.getEmployees(employeeRequest).subscribe((res) => {
      this.marketingOfficers = res?.data?.item1;
    });
  }
  getAllStores() {
    let storeRequest = new StoreRequest();
    storeRequest.inventoryTypeId = Inventory_Type_Id_Finished_Goods;
    this.storeService.getStores(storeRequest).subscribe((res) => {
      this.stores = res?.data.item1;
    });
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
        environment.apiURL + "/SaleInvoice/sales-item-customer-wise-print",
        this.searchForm.value,
        {
          responseType: "blob",
        },
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
          link.setAttribute("download", "Item_Customer_Wise.xlsx");
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }
}
