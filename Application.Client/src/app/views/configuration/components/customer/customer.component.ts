import {
  animate,
  state,
  style,
  transition,
  trigger,
} from "@angular/animations";
import { HttpClient } from "@angular/common/http";
import { Component, OnInit, ViewChild } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { MatDialog } from "@angular/material/dialog";
import { MatMenuTrigger } from "@angular/material/menu";
import { MatTableDataSource } from "@angular/material/table";
import {
  Department_Id_SALES_AND_MARKETING,
  Page_Size_Options,
} from "app/shared/consts/const";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { Observable, distinctUntilChanged, merge, of } from "rxjs";
import { Area } from "../../models/area/area.model";
import { CustomerRequest } from "../../models/customer/customer-request.model";
import { Customer } from "../../models/customer/customer.model";
import { EmployeeRequest } from "../../models/employee/employee-request.model";
import { Employee } from "../../models/employee/employee.model";
import { Region } from "../../models/region/region.model";
import { Territory } from "../../models/Territory/territory.model";
import { Zone } from "../../models/zone/zone.model";
import { AreaService } from "../../services/area.service";
import { CustomerService } from "../../services/customer.service";
import { EmployeeService } from "../../services/employee.service";
import { RegionService } from "../../services/region.service";
import { TerritoryService } from "../../services/territory.service";
import { ZoneService } from "../../services/zone.service";
import { CustomerFormComponent } from "./customer-form/customer-form.component";

@Component({
  selector: "app-customer",
  templateUrl: "./customer.component.html",
  styleUrls: ["./customer.component.scss"],
  animations: [
    trigger("detailExpand", [
      state("collapsed", style({ height: "0px", minHeight: "0" })),
      state("expanded", style({ height: "*" })),
      transition(
        "expanded <=> collapsed",
        animate("225ms cubic-bezier(0.4, 0.0, 0.2, 1)")
      ),
    ]),
  ],
})
export class CustomerComponent implements OnInit {
  businessType: string;
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
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
  viewColumnForm: FormGroup;
  expandedElement: any;
  dataSource: MatTableDataSource<Customer>;
  totalCount: number;
  customerRequest = new CustomerRequest();
  displayedColumns$: Observable<string[]>;
  pageSizeOptions: [] = Page_Size_Options;

  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  //  * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "expand", label: "Expand" },
    { def: "actions", label: "Actions" },
    { def: "code", label: "Code" },
    { def: "name", label: "Name" },
    { def: "ownersName", label: "Owner's Name" },
    { def: "contactNo", label: "Contact No" },
    { def: "email", label: "Email" },
    { def: "address", label: "Address" },
    { def: "nationalId", label: "NID" },
    { def: "tradeLicense", label: "Trade License" },
    { def: "contactPersonName", label: "Contact-Person Name" },
    { def: "contactPersonEmail", label: "Contact-Person Email" },
    { def: "contactPersonContactNo", label: "Contact-Person Contact No" },
    {
      def: "contactPersonDesignation",
      label: "Contact Person Designation",
    },
    { def: "customerZoneId", label: "Zone" },
    { def: "customerTerritoryId", label: "Territory" },
    { def: "customerMarketingOfficerId", label: "Marketing Officer" },
    { def: "customerCreditLimit", label: "Credit Limit" },
    { def: "customerAgreement", label: "Agreement" },
    { def: "customerCreditDays", label: "Credit Days" },
    { def: "customerTargetQuantity", label: "Target Quantity" },
  ];

  constructor(
    private customerService: CustomerService,
    private regionService: RegionService,
    private zoneService: ZoneService,
    private areaService: AreaService,
    private territoryService: TerritoryService,
    private employeeService: EmployeeService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService,
    private jwtAuth: JwtAuthService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.getAllRegions();
    this.getAllZones();
    this.getAllAreas();
    this.jwtAuth.userProfile
      .pipe(
        distinctUntilChanged(
          (prev, curr) => prev.businesstype === curr.businesstype
        )
      )
      .subscribe((res: UserProfile) => {
        this.businessType = res.businesstype;
        if (this.businessType === "1") {
          this.getAllTerritories();
        }
      });

    this.getAllMarketingOfficers();
    this.getCustomers(this.customerRequest);
    this.subscribeToFormChanges();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
      customerRegionId: [null],
      customerZoneId: [null],
      customerAreaId: [null],
      customerTerritoryId: [null],
      customerMarketingOfficerId: [null],
    });

    /**
     * * this code will be executed for instant changes of SearchForm's field
     */

    this.searchForm.valueChanges.subscribe((value) => {
      [
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

    this.viewColumnForm = this.fb.group({
      expand: [true],
      code: [true],
      name: [true],
      ownersName: [true],
      contactNo: [true],
      email: [false],
      address: [true],
      nationalId: [true],
      tradeLicense: [true],
      contactPersonName: [false],
      contactPersonContactNo: [false],
      contactPersonEmail: [false],
      contactPersonDesignation: [false],
      customerContactPersonName: [false],
      customerContactPersonDesignation: [false],
      customerZoneId: [false],
      customerMarketingOfficerId: [false],
      customerTerritoryId: [false],
      customerCreditLimit: [true],
      customerAgreement: [false],
      customerCreditDays: [false],
      customerTargetQuantity: [false],
      actions: [true],
    });
    this.updateDisplayedColumns();
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

  private subscribeToFormChanges() {
    merge(this.viewColumnForm.valueChanges).subscribe(() => {
      this.updateDisplayedColumns();
    });
  }

  private updateDisplayedColumns() {
    this.displayedColumns$ = of(
      this.columnDefinitions
        .filter((x) => this.viewColumnForm.get(x.def).value)
        .map((col) => col.def)
    );
  }

  collapsed: string = "collapsed";
  expanded: string = "expanded";

  expandAll() {
    if (this.collapsed === "collapsed") this.collapsed = "expanded";
    else this.collapsed = "collapsed";
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

  // -----------

  getCustomers(request: CustomerRequest): void {
    this.customerService.getCustomers(request).subscribe((res) => {
      this.dataSource = new MatTableDataSource<Customer>(res?.data?.item1);
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  remove(id): void {
    this.customerService.deleteCustomer(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getCustomers(this.customerRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  openForm(customer?: Customer): void {
    const dialogRef = this.dialog.open(CustomerFormComponent, {
      disableClose: true,
      data: customer,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getCustomers(this.customerRequest);
      }
    });
  }

  // Function to confirm deletion before calling the remove function
  confirmDelete(id: string, code: string = "") {
    // Open a confirmation dialog with a custom message and callback function
    const data: ConfirmDialogModel = {
      title: "Confirm Remove",
      message: `Deleting this item ${code} will affect related data. Confirm deletion?`,
    };
    this.confirmDialogService.confirmDialog(
      id,
      this.remove.bind(this),
      data.message
    );
  }

  reload() {
    this.loading = true;
    this.searchForm.reset();
    this.customerRequest = new CustomerRequest();
    this.getCustomers(this.customerRequest);
  }

  onSearch() {
    this.loading = true;
    this.updateCustomerRequest();
    this.getCustomers(this.customerRequest);
  }

  onPageChange(pageEvent) {
    const { pageIndex, pageSize } = pageEvent;
    this.updateCustomerRequest(pageIndex, pageSize);
    this.getCustomers(this.customerRequest);
  }

  private updateCustomerRequest(
    pageIndex = this.customerRequest.page,
    pageSize = this.customerRequest.rowsPerPage
  ) {
    this.customerRequest = {
      ...this.customerRequest,
      ...this.searchForm.value,
      page: pageIndex,
      rowsPerPage: pageSize,
    };
  }
  printPdf() {
    this.http
      .post(environment.apiURL + "/Customer/print", this.searchForm.value, {
        responseType: "blob",
      })
      .subscribe((response) => {
        //Create a Blob from the PDF Stream
        const file = new Blob([response], { type: "application/pdf" });
        //Build a URL from the file
        const fileURL = URL.createObjectURL(file);
        //Open the URL on new Window
        const pdfWindow = window.open();
        pdfWindow.location.href = fileURL;
      });
  }
}
