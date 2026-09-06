import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { Department_Id_SALES_AND_MARKETING } from "app/shared/consts/const";
import { CollectionReportFilterType } from "app/shared/enums/collectionReportFilterType";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { CostCenter } from "app/views/configuration/models/cost-center/cost-center.model";
import { Customer } from "app/views/configuration/models/customer/customer.model";
import { EmployeeRequest } from "app/views/configuration/models/employee/employee-request.model";
import { Employee } from "app/views/configuration/models/employee/employee.model";
import { PaymentMode } from "app/views/configuration/models/payment-mode/payment-mode.model";
import { Region } from "app/views/configuration/models/region/region.model";
import { Zone } from "app/views/configuration/models/zone/zone.model";
import { CostCenterService } from "app/views/configuration/services/cost-center.service";
import { CustomerService } from "app/views/configuration/services/customer.service";
import { EmployeeService } from "app/views/configuration/services/employee.service";
import { PaymentModeService } from "app/views/configuration/services/payment-mode.service";
import { RegionService } from "app/views/configuration/services/region.service";
import { ZoneService } from "app/views/configuration/services/zone.service";
import { environment } from "environments/environment";

@Component({
  selector: "app-payment-collection-report",
  templateUrl: "./payment-collection-report.component.html",
  styleUrls: ["./payment-collection-report.component.scss"],
})
export class PaymentCollectionReportComponent implements OnInit {
  collectionReportFilterTypes = Object.keys(CollectionReportFilterType).map(
    (key) => ({
      name: key,
      value: CollectionReportFilterType[key],
    })
  );
  paymentModes: PaymentMode[];
  constructor(
    private http: HttpClient,
    private fb: FormBuilder,
    private zoneService: ZoneService,
    private regionService: RegionService,
    private customerService: CustomerService,
    private employeeService: EmployeeService,
    private costCenterService: CostCenterService,
    private paymentModeService: PaymentModeService,
    private jwtAuth: JwtAuthService
  ) {}
  searchForm: FormGroup;
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  costCenters: CostCenter[];
  zones: Zone[];
  filterZones: Zone[];
  lastFilterZones: Zone[];
  regions: Region[];
  filterRegions: Region[];
  lastFilterRegions: Region[];
  customers: Customer[];
  filterCustomers: Customer[];
  lastFilterCustomers: Customer[];
  marketingOfficers: Employee[];
  filterMarketingOfficers: Employee[];
  showFilterByCustomer: boolean = false;
  businessType: string;

  ngOnInit(): void {
    this.jwtAuth.userProfile.subscribe((res: UserProfile) => {
      this.businessType = res.businesstype;
      if (this.businessType === "2") {
        this.getAllZones();
      } else {
        this.getAllRegions();
      }
    });
    this.initializeForm();
    this.getAllCustomers();
    this.getAllCostCenters();
    this.getAllPaymentModes();
    this.getAllMarketingOfficers();
  }
  initializeForm() {
    this.searchForm = this.fb.group({
      fromDate: [null, Validators.required],
      toDate: [null, Validators.required],
      costCenterId: [null],
      paymentModeId: [null],
      accountTransactionType: [null],
      customerZoneId: [null],
      customerId: [null],
      customerMarketingOfficerId: [null],
      reportType: [1, Validators.required],
    });

    /**
     * * this code will be executed for instant changes of SearchForm's field
     */
    this.searchForm.valueChanges.subscribe((value) => {
      ["customerId", "customerZoneId", "customerMarketingOfficerId"].forEach(
        (field) => {
          if (value[field] === "") {
            this.searchForm.get(field)?.patchValue(null, { emitEvent: false });
            this.handleFilterChange(field);
          }
        }
      );
    });
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    this.searchForm.patchValue({ [fieldName]: null });
    this.handleFilterChange(fieldName);
  }

  handleFilterChange(fieldName: string): void {
    const value = this.searchForm?.value;
    switch (fieldName) {
      case "customerZoneId":
        this.resetFilters();
        this.applyFilter("zone", value?.customerZoneId);
        console.log("zone", value?.customerZoneId);
        break;
      case "customerId":
        this.resetFilters(["zones", "customers"]);
        this.applyFilter("customer", value?.customerId);
        break;
      case "customerMarketingOfficerId":
        this.filterMarketingOfficers = this.marketingOfficers;
        break;
    }
  }

  resetFilters(exceptions: string[] = []): void {
    if (!exceptions.includes("zones")) {
      this.businessType === "2"
        ? (this.filterZones = this.lastFilterZones = this.zones)
        : (this.filterRegions = this.lastFilterRegions = this.regions);
    }
    if (!exceptions.includes("customers")) {
      this.filterCustomers = this.lastFilterCustomers = this.customers;
    }
  }

  applyFilter(type: string, id: string): void {
    switch (type) {
      case "zone":
        if (id) {
          this.businessType === "2"
            ? (this.filterCustomers = this.customers?.filter(
                (x) => x.customerZoneId === id
              ))
            : (this.filterCustomers = this.customers?.filter(
                (x) => x.customerRegionId === id
              ));
        } else {
          this.filterCustomers = this.customers;
        }
        this.lastFilterCustomers = this.filterCustomers;
        break;
    }
  }

  onSelectedZone(customerZoneId: string) {
    this.searchForm.patchValue({
      customerId: null,
    });
    this.applyFilter("zone", customerZoneId);
  }

  getAllRegions() {
    this.regionService.getAllRegions().subscribe((res) => {
      this.filterRegions =
        this.lastFilterRegions =
        this.regions =
          res?.data?.item1;
    });
  }

  getAllZones() {
    this.zoneService.getAllZones().subscribe((res) => {
      this.filterZones = this.lastFilterZones = this.zones = res?.data?.item1;
    });
  }

  handleZoneSearch(): void {
    const term = this.searchForm?.get("customerZoneId")?.value?.toLowerCase();
    if (this.businessType === "2") {
      this.filterZones = this.lastFilterZones?.filter((option) =>
        option.name.toLowerCase().includes(term)
      );
    } else {
      this.filterRegions = this.lastFilterRegions?.filter((option) =>
        option.name.toLowerCase().includes(term)
      );
    }
  }

  getAllCustomers() {
    this.customerService.getAllCustomers().subscribe((res) => {
      this.filterCustomers =
        this.lastFilterCustomers =
        this.customers =
          res?.data?.item1;
    });
  }

  handleCustomerSearch(): void {
    const term = this.searchForm?.get("customerId")?.value?.toLowerCase();
    this.filterCustomers = this.lastFilterCustomers?.filter(
      (option) =>
        option.name.toLowerCase().includes(term) ||
        option.code?.slice(-4).toLowerCase().includes(term)
    );
  }

  getCustomerName(customerId: string) {
    if (!customerId) {
      return;
    }
    const customerData = this.customers?.find(
      (customer) => customer?.id === customerId
    );
    return customerData?.name;
  }

  getAllMarketingOfficers(): void {
    let employeeRequest = new EmployeeRequest();
    employeeRequest.page = -1;
    employeeRequest.departmentId = Department_Id_SALES_AND_MARKETING;
    this.employeeService.getEmployees(employeeRequest).subscribe((res) => {
      this.filterMarketingOfficers = this.marketingOfficers = res?.data?.item1;
    });
  }

  handleMarketingOfficerSearch(): void {
    const term = this.searchForm
      ?.get("customerMarketingOfficerId")
      ?.value?.toLowerCase();
    this.filterMarketingOfficers = this.marketingOfficers?.filter((option) =>
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

  getZoneName(customerZoneId: string) {
    if (!customerZoneId) {
      return;
    }
    if (this.businessType === "2") {
      const zoneData = this.zones?.find((zone) => zone?.id === customerZoneId);
      return zoneData?.name;
    } else {
      const regionData = this.regions?.find(
        (region) => region?.id === customerZoneId
      );
      return regionData?.name;
    }
  }

  getAllCostCenters() {
    this.costCenterService.getAllCostCenters().subscribe((res) => {
      this.costCenters = res?.data.item1;
    });
  }
  getAllPaymentModes() {
    this.paymentModeService.getAllPaymentModes().subscribe((res) => {
      this.paymentModes = res.data?.item1;
    });
  }
  onSelectedFilterType(accountTransactionType) {
    if (
      accountTransactionType === CollectionReportFilterType.Customer_Receipt
    ) {
      this.showFilterByCustomer = true;
    } else {
      this.showFilterByCustomer = false;
      this.searchForm.get("accountId").reset();
    }
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
        environment.apiURL + "/voucherEntry/payment-collection-report/print",
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
          link.setAttribute("download", "collection_report.xlsx");
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }
}
