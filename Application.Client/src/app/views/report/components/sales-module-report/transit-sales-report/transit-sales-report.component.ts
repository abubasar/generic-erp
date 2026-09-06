import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import {
  Department_Id_SALES_AND_MARKETING,
  Inventory_Type_Id_Finished_Goods,
} from "app/shared/consts/const";
import { Area } from "app/views/configuration/models/area/area.model";
import { Customer } from "app/views/configuration/models/customer/customer.model";
import { EmployeeRequest } from "app/views/configuration/models/employee/employee-request.model";
import { Employee } from "app/views/configuration/models/employee/employee.model";
import { Region } from "app/views/configuration/models/region/region.model";
import { StoreRequest } from "app/views/configuration/models/store/store-request.model";
import { Store } from "app/views/configuration/models/store/store.model";
import { Zone } from "app/views/configuration/models/zone/zone.model";
import { AreaService } from "app/views/configuration/services/area.service";
import { CustomerService } from "app/views/configuration/services/customer.service";
import { EmployeeService } from "app/views/configuration/services/employee.service";
import { RegionService } from "app/views/configuration/services/region.service";
import { StoreService } from "app/views/configuration/services/store.service";
import { ZoneService } from "app/views/configuration/services/zone.service";
import { environment } from "environments/environment";

@Component({
  selector: "app-transit-sales-report",
  templateUrl: "./transit-sales-report.component.html",
  styleUrls: ["./transit-sales-report.component.scss"],
})
export class TransitSalesReportComponent implements OnInit {
  searchForm: FormGroup;
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  stores: Store[];
  regions: Region[];
  filterRegions: Region[];
  zones: Zone[];
  filterZones: Zone[];
  lastFilterZones: Zone[];
  areas: Area[];
  filterAreas: Area[];
  lastFilterAreas: Area[];
  marketingOfficers: Employee[];
  filterMarketingOfficers: Employee[];
  lastFilterMarketingOfficers: Employee[];
  customers: Customer[];
  filterCustomers: Customer[];
  lastFilterCustomers: Customer[];

  constructor(
    private http: HttpClient,
    private fb: FormBuilder,
    private customerService: CustomerService,
    private storeService: StoreService,
    private regionService: RegionService,
    private zoneService: ZoneService,
    private areaService: AreaService,
    private employeeService: EmployeeService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.getAllStores();
    this.getAllRegions();
    this.getAllZones();
    this.getAllAreas();
    this.getAllMarketingOfficers();
    this.getAllCustomers();
  }
  initializeForm() {
    this.searchForm = this.fb.group({
      toDate: [null, Validators.required],
      customerId: [null],
      storeId: [null],
      customerRegionId: [null],
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
      if (value.customerRegionId === "") {
        this.searchForm
          .get("customerRegionId")
          ?.patchValue(null, { emitEvent: false });
      }

      if (value.customerZoneId === "") {
        this.searchForm
          .get("customerZoneId")
          ?.patchValue(null, { emitEvent: false });
        if (value?.customerRegionId) {
          this.onSelectedRegion(value?.customerRegionId);
        } else {
          this.filterAreas = this.lastFilterAreas = this.areas;
          this.filterMarketingOfficers = this.lastFilterMarketingOfficers =
            this.marketingOfficers;
          this.filterCustomers = this.lastFilterCustomers = this.customers;
        }
      }

      if (value.customerAreaId === "") {
        this.searchForm
          .get("customerAreaId")
          ?.patchValue(null, { emitEvent: false });
        if (value?.customerZoneId) {
          this.onSelectedZone(value?.customerZoneId);
        } else if (value?.customerRegionId) {
          this.onSelectedRegion(value?.customerRegionId);
        } else {
          this.filterMarketingOfficers = this.lastFilterMarketingOfficers =
            this.marketingOfficers;
          this.filterCustomers = this.lastFilterCustomers = this.customers;
        }
      }

      if (value.customerMarketingOfficerId === "") {
        this.searchForm
          .get("customerMarketingOfficerId")
          ?.patchValue(null, { emitEvent: false });
        if (value?.customerAreaId) {
          this.onSelectedArea(value?.customerAreaId);
        } else if (value?.customerZoneId) {
          this.onSelectedZone(value?.customerZoneId);
        } else if (value?.customerRegionId) {
          this.onSelectedRegion(value?.customerRegionId);
        } else {
          this.filterCustomers = this.lastFilterCustomers = this.customers;
        }
      }

      if (value.customerId === "") {
        this.searchForm
          .get("customerId")
          ?.patchValue(null, { emitEvent: false });
      }
    });
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();

    if (fieldName == "customerRegionId") {
      this.searchForm.patchValue({
        customerRegionId: null,
      });
    }

    if (fieldName == "customerZoneId") {
      this.searchForm.patchValue({
        customerZoneId: null,
      });
      if (this.searchForm?.value?.customerRegionId) {
        this.onSelectedRegion(this.searchForm?.value?.customerRegionId);
      } else {
        this.filterAreas = this.lastFilterAreas = this.areas;
        this.filterMarketingOfficers = this.lastFilterMarketingOfficers =
          this.marketingOfficers;
        this.filterCustomers = this.lastFilterCustomers = this.customers;
      }
    }

    if (fieldName == "customerAreaId") {
      this.searchForm.patchValue({
        customerAreaId: null,
      });
      if (this.searchForm?.value?.customerZoneId) {
        this.onSelectedZone(this.searchForm?.value?.customerZoneId);
      } else if (this.searchForm?.value?.customerRegionId) {
        this.onSelectedRegion(this.searchForm?.value?.customerRegionId);
      } else {
        this.filterMarketingOfficers = this.lastFilterMarketingOfficers =
          this.marketingOfficers;
        this.filterCustomers = this.lastFilterCustomers = this.customers;
      }
    }

    if (fieldName == "customerMarketingOfficerId") {
      this.searchForm.patchValue({
        customerMarketingOfficerId: null,
      });
      if (this.searchForm?.value?.customerAreaId) {
        this.onSelectedArea(this.searchForm?.value?.customerAreaId);
      } else if (this.searchForm?.value?.customerZoneId) {
        this.onSelectedZone(this.searchForm?.value?.customerZoneId);
      } else if (this.searchForm?.value?.customerRegionId) {
        this.onSelectedRegion(this.searchForm?.value?.customerRegionId);
      } else {
        this.filterCustomers = this.lastFilterCustomers = this.customers;
      }
    }

    if (fieldName == "customerId") {
      this.searchForm.patchValue({
        customerId: null,
      });
    }
  }

  getAllRegions() {
    this.regionService.getAllRegions().subscribe((res) => {
      this.filterRegions = this.regions = res?.data?.item1?.sort((a, b) =>
        a.name.localeCompare(b.name)
      );
    });
  }

  handleRegionSearch(): void {
    const term = this.searchForm?.get("customerRegionId")?.value?.toLowerCase();
    this.filterRegions = this.regions?.filter((option) =>
      option.name.toLowerCase().includes(term)
    );
  }

  getRegionName(customerRegionId: string) {
    if (!customerRegionId) {
      return;
    }
    const regionData = this.regions?.find(
      (region) => region?.id === customerRegionId
    );
    return regionData?.name;
  }

  onSelectedRegion(customerRegionId: string) {
    this.searchForm.patchValue({
      customerZoneId: null,
      customerAreaId: null,
    });

    this.filterZones = this.lastFilterZones = this.zones?.filter(
      (x) => x.regionId === customerRegionId
    );

    let areaData = [];
    this.lastFilterZones.forEach((zone) => {
      this.areas?.filter((x) => {
        if (x?.zoneId === zone.id) {
          return areaData.push(x);
        }
      });
    });
    this.filterAreas = this.lastFilterAreas = areaData;
  }

  getAllZones() {
    this.zoneService.getAllZones().subscribe((res) => {
      this.filterZones =
        this.lastFilterZones =
        this.zones =
          res?.data?.item1?.sort((a, b) => a.name.localeCompare(b.name));
    });
  }

  handleZoneSearch(): void {
    const term = this.searchForm?.get("customerZoneId")?.value?.toLowerCase();
    this.filterZones = this.lastFilterZones?.filter((option) =>
      option.name.toLowerCase().includes(term)
    );
  }

  getZoneName(customerZoneId: string) {
    if (!customerZoneId) {
      return;
    }
    const zoneData = this.zones?.find((zone) => zone?.id === customerZoneId);
    return zoneData?.name;
  }

  onSelectedZone(customerZoneId: string) {
    this.searchForm.patchValue({
      customerAreaId: null,
    });
    this.filterAreas = this.lastFilterAreas = this.areas?.filter(
      (x) => x.zoneId === customerZoneId
    );
  }

  getAllAreas() {
    this.areaService.getAllAreas().subscribe((res) => {
      this.filterAreas =
        this.lastFilterAreas =
        this.areas =
          res?.data?.item1?.sort((a, b) => a.name.localeCompare(b.name));
    });
  }

  handleAreaSearch(): void {
    const term = this.searchForm?.get("customerAreaId")?.value?.toLowerCase();
    this.filterAreas = this.lastFilterAreas?.filter((option) =>
      option.name.toLowerCase().includes(term)
    );
  }

  getAreaName(customerAreaId: string) {
    if (!customerAreaId) {
      return;
    }
    const areaData = this.areas?.find((area) => area?.id === customerAreaId);
    return areaData?.name;
  }

  onSelectedArea(customerAreaId: string) {
  
  }

  getAllMarketingOfficers(): void {
    let employeeRequest = new EmployeeRequest();
    employeeRequest.page = -1;
    employeeRequest.departmentId = Department_Id_SALES_AND_MARKETING;
    this.employeeService.getEmployees(employeeRequest).subscribe((res) => {
      this.filterMarketingOfficers =
        this.lastFilterMarketingOfficers =
        this.marketingOfficers =
          res?.data?.item1?.sort((a, b) =>
            (a.firstName + a.lastName).localeCompare(b.firstName + b.lastName)
          );
    });
  }

  handleMarketingOfficerSearch(): void {
    const term = this.searchForm
      ?.get("customerMarketingOfficerId")
      ?.value?.toLowerCase();
    this.filterMarketingOfficers = this.lastFilterMarketingOfficers?.filter(
      (option) =>
        (option.firstName + " " + option.lastName).toLowerCase().includes(term)
    );
  }

  getMarketingOfficerName(customerMarketingOfficerId: string) {
    if (!customerMarketingOfficerId) {
      return;
    }
    const marketingOfficerData = this.marketingOfficers?.find(
      (marketingOfficer) => marketingOfficer?.id === customerMarketingOfficerId
    );
    return (
      marketingOfficerData?.firstName + " " + marketingOfficerData?.lastName
    );
  }

  onSelectedMarketingOfficer(customerMarketingOfficerId: string) {
    this.searchForm.patchValue({
      customerId: null,
    });
    this.filterCustomers = this.lastFilterCustomers = this.customers?.filter(
      (x) => x.customerMarketingOfficerId === customerMarketingOfficerId
    );
  }

  handleCustomerSearch(): void {
    const term = this.searchForm?.get("customerId")?.value?.toLowerCase();
    this.filterCustomers = this.lastFilterCustomers?.filter((option) =>
      option.name.toLowerCase().includes(term)
    );
  }

  getCustomerName(customerId: string) {
    if (!customerId) {
      return;
    }
    const customerAccount = this.customers?.find(
      (customer) => customer?.id === customerId
    );
    return customerAccount?.name;
  }

  getAllCustomers() {
    this.customerService.getAllCustomers().subscribe((res) => {
      this.filterCustomers =
        this.lastFilterCustomers =
        this.customers =
          res.data?.item1?.sort((a, b) => a.name.localeCompare(b.name));
    });
  }

  getAllStores() {
    let storeRequest = new StoreRequest();
    storeRequest.inventoryTypeId = Inventory_Type_Id_Finished_Goods;
    this.storeService.getStores(storeRequest).subscribe((res) => {
      this.stores = res?.data.item1?.sort((a, b) =>
        a.name.localeCompare(b.name)
      );
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
        environment.apiURL + "/SaleOrder/transit-sales-report-print",
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
          link.setAttribute("download", "Transit_Sales_Report.xlsx");
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }
}
