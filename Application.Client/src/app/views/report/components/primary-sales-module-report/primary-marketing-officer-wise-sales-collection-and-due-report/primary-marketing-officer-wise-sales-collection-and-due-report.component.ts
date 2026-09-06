import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { Department_Id_SALES_AND_MARKETING } from "app/shared/consts/const";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { Area } from "app/views/configuration/models/area/area.model";
import { EmployeeRequest } from "app/views/configuration/models/employee/employee-request.model";
import { Employee } from "app/views/configuration/models/employee/employee.model";
import { Region } from "app/views/configuration/models/region/region.model";
import { Territory } from "app/views/configuration/models/Territory/territory.model";
import { Zone } from "app/views/configuration/models/zone/zone.model";
import { AreaService } from "app/views/configuration/services/area.service";
import { EmployeeService } from "app/views/configuration/services/employee.service";
import { RegionService } from "app/views/configuration/services/region.service";
import { TerritoryService } from "app/views/configuration/services/territory.service";
import { ZoneService } from "app/views/configuration/services/zone.service";
import { environment } from "environments/environment";

@Component({
  selector:
    "app-primary-marketing-officer-wise-sales-collection-and-due-report",
  templateUrl:
    "./primary-marketing-officer-wise-sales-collection-and-due-report.component.html",
  styleUrls: [
    "./primary-marketing-officer-wise-sales-collection-and-due-report.component.scss",
  ],
})
export class PrimaryMarketingOfficerWiseSalesCollectionAndDueReportComponent implements OnInit {
  businessType: string;
  constructor(
    private http: HttpClient,
    private fb: FormBuilder,
    // private storeService: StoreService,
    private regionService: RegionService,
    private zoneService: ZoneService,
    private areaService: AreaService,
    private territoryService: TerritoryService,
    private employeeService: EmployeeService,
    // private productService: ProductService,
    private jwtAuth: JwtAuthService
  ) {}
  searchForm: FormGroup;
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  isLoading3: boolean = false;
  isLoading4: boolean = false;
  // products: ProductView[];
  // filterProducts: ProductView[];
  // stores: Store[];
  regions: Region[];
  filterRegions: Region[];
  zones: Zone[];
  filterZones: Zone[];
  lastFilterZones: Zone[];
  areas: Area[];
  filterAreas: Area[];
  lastFilterAreas: Area[];
  territories: Territory[];
  filterTerritories: Territory[];
  lastFilterTerritories: Territory[];
  marketingOfficers: Employee[];
  filterMarketingOfficers: Employee[];

  ngOnInit(): void {
    this.jwtAuth.userProfile.subscribe((res: UserProfile) => {
      this.businessType = res.businesstype;
    });
    this.initializeForm();
    // this.getAllProducts();
    // this.getAllStores();
    this.getAllRegions();
    this.getAllZones();
    this.getAllAreas();
    this.getAllTerritories();
    this.getAllMarketingOfficers();
  }
  initializeForm() {
    this.searchForm = this.fb.group({
      fromDate: [null, Validators.required],
      toDate: [null, Validators.required],
      // productId: [null],
      // customerId: [null],
      // storeId: [null],
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
      [
        // "productId",
        "customerRegionId",
        "customerZoneId",
        "customerAreaId",
        "customerTerritoryId",
        "customerMarketingOfficerId",
      ].forEach((field) => {
        if (value[field] === "") {
          this.searchForm.get(field)?.patchValue(null, { emitEvent: false });
          this.handleFilterChange(field);
        }
      });
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
      case "customerRegionId":
        this.resetFilters();
        this.applyFilter("region", value?.customerRegionId);
        break;
      case "customerZoneId":
        this.resetFilters(["zones"]);
        this.applyFilter("zone", value?.customerZoneId);
        console.log("zone", value?.customerZoneId);
        break;
      case "customerAreaId":
        this.resetFilters(["zones", "areas"]);
        this.applyFilter("area", value?.customerAreaId);
        break;
      case "customerTerritoryId":
        this.resetFilters(["zones", "areas", "territories"]);
        this.applyFilter("territory", value?.customerTerritoryId);
        break;
      case "customerMarketingOfficerId":
        this.filterMarketingOfficers = this.marketingOfficers;
        break;
    }
  }

  resetFilters(exceptions: string[] = []): void {
    if (!exceptions.includes("zones")) {
      this.filterZones = this.lastFilterZones = this.zones;
    }
    if (!exceptions.includes("areas")) {
      this.filterAreas = this.lastFilterAreas = this.areas;
    }
    if (!exceptions.includes("territories")) {
      this.filterTerritories = this.lastFilterTerritories = this.territories;
    }
  }

  applyFilter(type: string, id: string): void {
    switch (type) {
      case "region":
        this.filterZones = id
          ? this.zones?.filter((x) => x.regionId === id)
          : this.zones;
        this.lastFilterZones = this.filterZones;
        this.applyFilter("zone", this.searchForm?.value?.customerZoneId);
        break;

      case "zone":
        if (id) {
          this.filterAreas = this.areas?.filter((x) => x.zoneId === id);
        } else if (this.searchForm?.value?.customerRegionId) {
          this.filterAreas = this.getRelatedAreas(this.lastFilterZones);
        } else {
          this.filterAreas = this.areas;
        }
        this.lastFilterAreas = this.filterAreas;
        this.applyFilter("area", this.searchForm?.value?.customerAreaId);
        break;

      case "area":
        if (id) {
          this.filterTerritories = this.territories?.filter(
            (x) => x.areaId === id
          );
        } else if (this.searchForm?.value?.customerZoneId) {
          this.filterTerritories = this.getRelatedTerritories(
            this.lastFilterAreas
          );
        } else if (this.searchForm?.value?.customerRegionId) {
          this.filterAreas = this.getRelatedAreas(this.lastFilterZones);
          this.lastFilterAreas = this.filterAreas;
          this.filterTerritories = this.getRelatedTerritories(
            this.lastFilterAreas
          );
        } else {
          this.filterTerritories = this.territories;
        }
        this.lastFilterTerritories = this.filterTerritories;
        break;
    }
  }

  /**
   * Get all areas related to the given zones.
   */
  private getRelatedAreas(zones: any[]): any[] {
    return (
      zones?.flatMap((zone) =>
        this.areas?.filter((area) => area.zoneId === zone.id)
      ) || []
    );
  }

  /**
   * Get all territories related to the given areas.
   */
  private getRelatedTerritories(areas: any[]): any[] {
    return (
      areas?.flatMap((area) =>
        this.territories?.filter((territory) => territory.areaId === area.id)
      ) || []
    );
  }

  onSelectedRegion(customerRegionId: string) {
    this.searchForm.patchValue({
      customerZoneId: null,
      customerAreaId: null,
      customerTerritoryId: null,
    });
    this.applyFilter("region", customerRegionId);
  }

  onSelectedZone(customerZoneId: string) {
    this.searchForm.patchValue({
      customerAreaId: null,
      customerTerritoryId: null,
    });
    this.applyFilter("zone", customerZoneId);
  }

  onSelectedArea(customerAreaId: string) {
    this.searchForm.patchValue({ customerTerritoryId: null });
    this.applyFilter("area", customerAreaId);
  }

  // -----------

  getAllRegions() {
    this.regionService.getAllRegions().subscribe((res) => {
      this.filterRegions = this.regions = res?.data?.item1;
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

  getAllZones() {
    this.zoneService.getAllZones().subscribe((res) => {
      this.filterZones = this.lastFilterZones = this.zones = res?.data?.item1;
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

  getAllAreas() {
    this.areaService.getAllAreas().subscribe((res) => {
      this.filterAreas = this.lastFilterAreas = this.areas = res?.data?.item1;
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

  getAllTerritories() {
    this.territoryService.getAllTerritories().subscribe((res) => {
      this.filterTerritories =
        this.lastFilterTerritories =
        this.territories =
          res?.data?.item1;
    });
  }

  handleTerritorySearch(): void {
    const term = this.searchForm
      ?.get("customerTerritoryId")
      ?.value?.toLowerCase();
    this.filterTerritories = this.lastFilterTerritories?.filter((option) =>
      option.name.toLowerCase().includes(term)
    );
  }

  getTerritoryName(customerTerritoryId: string) {
    if (!customerTerritoryId) {
      return;
    }
    const territoryData = this.territories?.find(
      (territory) => territory?.id === customerTerritoryId
    );
    return territoryData?.name;
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

  printPdf($event) {
    if ($event?.submitter?.name == "pdf") {
      this.isLoading1 = true;
      this.searchForm.get("reportType").setValue(1);
    }
    if ($event?.submitter?.name == "excel") {
      this.searchForm.get("reportType").setValue(2);
      this.isLoading2 = true;
    }
    if ($event?.submitter?.name == "pdf2") {
      this.isLoading3 = true;
      this.searchForm.get("reportType").setValue(1);
    }
    if ($event?.submitter?.name == "excel2") {
      this.searchForm.get("reportType").setValue(2);
      this.isLoading4 = true;
    }
    this.http
      .post(
        environment.apiURL +
          ($event?.submitter?.name == "pdf" ||
          $event?.submitter?.name == "excel"
            ? "/Account/primary-marketing-officer-wise-sales-collection-and-due-report-print"
            : "/Account/primary-marketing-officer-wise-sales-collection-and-due-details-report-print"),
        this.searchForm.value,
        {
          responseType: "blob",
        },
      )
      .subscribe((response) => {
        if (
          $event?.submitter?.name == "pdf" ||
          $event?.submitter?.name == "pdf2"
        ) {
          const file = new Blob([response], { type: "application/pdf" });
          const fileURL = URL.createObjectURL(file);
          const pdfWindow = window.open();
          pdfWindow.location.href = fileURL;
          if ($event?.submitter?.name == "pdf") {
            this.isLoading1 = false;
          } else {
            this.isLoading3 = false;
          }
        } else {
          const blob = new Blob([response], {
            type: "application/octet-stream",
          });
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement("a");
          link.href = url;
          if ($event?.submitter?.name == "excel") {
            link.setAttribute(
              "download",
              "Marketing_Officer_Wise_Sales_Collection_And_Due_Report.xlsx",
            );
            this.isLoading2 = false;
          } else {
            link.setAttribute(
              "download",
              "Marketing_Officer_Wise_Sales_Collection_And_Due_Details_Report.xlsx",
            );
            this.isLoading4 = false;
          }
          document.body.appendChild(link);
          link.click();
        }
      });
  }
}
