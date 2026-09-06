import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import {
  Department_Id_SALES_AND_MARKETING,
  Inventory_Type_Id_Finished_Goods,
} from "app/shared/consts/const";
import { AreaRequest } from "app/views/configuration/models/area/area-request.model";
import { Area } from "app/views/configuration/models/area/area.model";
import { Customer } from "app/views/configuration/models/customer/customer.model";
import { EmployeeRequest } from "app/views/configuration/models/employee/employee-request.model";
import { Employee } from "app/views/configuration/models/employee/employee.model";
import { StoreRequest } from "app/views/configuration/models/store/store-request.model";
import { Store } from "app/views/configuration/models/store/store.model";
import { Zone } from "app/views/configuration/models/zone/zone.model";
import { AreaService } from "app/views/configuration/services/area.service";
import { CustomerService } from "app/views/configuration/services/customer.service";
import { EmployeeService } from "app/views/configuration/services/employee.service";
import { StoreService } from "app/views/configuration/services/store.service";
import { ZoneService } from "app/views/configuration/services/zone.service";
import { environment } from "environments/environment";

@Component({
  selector: "app-sales-customer-item-wise",
  templateUrl: "./sales-customer-item-wise.component.html",
  styleUrls: ["./sales-customer-item-wise.component.scss"],
})
export class SalesCustomerItemWiseComponent implements OnInit {
  constructor(
    private http: HttpClient,
    private fb: FormBuilder,
    private customerService: CustomerService,
    private storeService: StoreService,
    private zoneService: ZoneService,
    private areaService: AreaService,
    private employeeService: EmployeeService,
  ) {}
  searchForm: FormGroup;
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  customers: Customer[];
  filterCustomers: Customer[];
  stores: Store[];
  zones: Zone[];
  isZoneSelected: boolean = false;
  areas: Area[];
  isAreaSelected: boolean = false;
  marketingOfficers: Employee[];

  ngOnInit(): void {
    this.initializeForm();
    this.getAllCustomers();
    this.getAllStores();
    this.getAllZones();
    this.getAllMarketingOfficers();
  }
  initializeForm() {
    this.searchForm = this.fb.group({
      fromDate: [null, Validators.required],
      toDate: [null, Validators.required],
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
      if (value.customerId === "") {
        this.searchForm
          .get("customerId")
          ?.patchValue(null, { emitEvent: false });
      }
    });
  }

  clearInput(evt: any): void {
    evt.stopPropagation();
    this.searchForm?.get("customerId").setValue(null);
  }

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
  handleCustomerSearch(event: any): void {
    const name = event.target?.name;
    if (name === "customerId") {
      const term = this.searchForm.get("customerId");
      this.filterCustomer(term.value || "");
    }
  }

  private filterCustomer(value: string) {
    const filterValue = value.trim().toLowerCase();
    this.filterCustomers = this.customers?.filter(
      (option) =>
        option.name.toLowerCase().includes(filterValue) ||
        option.code?.slice(-4).toLowerCase().includes(filterValue),
    );
  }

  getCustomerName(customerId: string) {
    if (!customerId) {
      return;
    }
    const customerAccount = this.customers?.find(
      (customer) => customer?.id === customerId,
    );
    return customerAccount?.name;
  }

  getAllCustomers() {
    this.customerService.getAllCustomers().subscribe((res) => {
      this.filterCustomers = this.customers = res.data?.item1;
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
        environment.apiURL + "/SaleInvoice/sales-customer-item-wise-print",
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
          link.setAttribute("download", "Customer_Item_Wise.xlsx");
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }
}
