import { Component, Inject, OnInit } from "@angular/core";
import { FormArray, FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA, MatDialog } from "@angular/material/dialog";
import { Department_Id_SALES_AND_MARKETING } from "app/shared/consts/const";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { AreaRequest } from "app/views/configuration/models/area/area-request.model";
import { Area } from "app/views/configuration/models/area/area.model";
import {
  BankAccount,
  Customer,
} from "app/views/configuration/models/customer/customer.model";
import { EmployeeRequest } from "app/views/configuration/models/employee/employee-request.model";
import { Employee } from "app/views/configuration/models/employee/employee.model";
import { Region } from "app/views/configuration/models/region/region.model";
import { TerritoryRequest } from "app/views/configuration/models/Territory/territory-request.model";
import { Territory } from "app/views/configuration/models/Territory/territory.model";
import { ZoneRequest } from "app/views/configuration/models/zone/zone-request.model";
import { Zone } from "app/views/configuration/models/zone/zone.model";
import { AreaService } from "app/views/configuration/services/area.service";
import { CustomerService } from "app/views/configuration/services/customer.service";
import { EmployeeService } from "app/views/configuration/services/employee.service";
import { RegionService } from "app/views/configuration/services/region.service";
import { TerritoryService } from "app/views/configuration/services/territory.service";
import { ZoneService } from "app/views/configuration/services/zone.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-customer-form",
  templateUrl: "./customer-form.component.html",
  styleUrls: ["./customer-form.component.scss"],
})
export class CustomerFormComponent implements OnInit {
  formTitle: string;
  customerForm: FormGroup;
  // zones: Zone[];
  regions: Region[];
  isRegionSelected: boolean = false;
  zones: Zone[];
  isZoneSelected: boolean = false;
  areas: Area[];
  isAreaSelected: boolean = false;
  territories: Territory[];
  marketingOfficers: Employee[];
  filteredMarketingOfficers: Employee[];
  businessType: string;

  constructor(
    @Inject(MAT_DIALOG_DATA)
    private data: Customer,
    private dialog: MatDialog,
    private customerService: CustomerService,
    private regionService: RegionService,
    private zoneService: ZoneService,
    private areaService: AreaService,
    private territoryService: TerritoryService,
    private employeeService: EmployeeService,
    private jwtAuth: JwtAuthService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit() {
    // this.initializeForm();
    this.getAllRegions();
    this.getAllMarketingOfficers();
    // this.getAllZones();
    let isDataLoaded = false;
    this.jwtAuth.userProfile.subscribe((res: UserProfile) => {
      this.businessType = res.businesstype;
      if (!isDataLoaded) {
        isDataLoaded = true;
        this.initializeForm();
      }
    });
  }

  initializeForm() {
    this.createForm();
    this.populateForm();
    this.setFormTitle();
    if (this.data?.customerRegionId) {
      this.isRegionSelected = true;
      this.getAllZonesByRegionId(this.data.customerRegionId);
    }
    if (this.data?.customerZoneId) {
      this.isZoneSelected = true;
      this.getAllAreasByZoneId(this.data.customerZoneId);
    }
    if (this.data?.customerAreaId) {
      if (this.businessType === "1") {
        this.isAreaSelected = true;
        this.getAllTerritoriesByAreaId(this.data.customerAreaId);
      }
    }
  }

  createForm(): void {
    this.customerForm = this.fb.group({
      id: [this.data?.id ?? null],
      name: [this.data?.name, Validators.required],
      ownersName: [this.data?.ownersName, Validators.required],
      contactNo: [
        this.data?.contactNo,
        [Validators.required, Validators.pattern("^01[3-9]\\d{8}$")],
      ],
      email: [this.data?.email ?? ""],
      address: [this.data?.address ?? ""],
      nationalId: [this.data?.nationalId ?? ""],
      tradeLicense: [this.data?.tradeLicense ?? ""],
      contactPersonName: [this.data?.contactPersonName, Validators.required],
      contactPersonContactNo: [
        this.data?.contactPersonContactNo,
        [Validators.required, Validators.pattern("^01[3-9]\\d{8}$")],
      ],
      contactPersonEmail: [this.data?.contactPersonEmail ?? ""],
      contactPersonDesignation: [this.data?.contactPersonDesignation ?? ""],
      customerRegionId: [this.data?.customerRegionId, Validators.required],
      customerZoneId: [this.data?.customerZoneId, Validators.required],
      customerAreaId: [this.data?.customerAreaId, Validators.required],
      customerTerritoryId: [
        this.data?.customerTerritoryId,
        this.businessType == "1" ? Validators.required : null,
      ],
      customerMarketingOfficerId: [
        this.data?.customerMarketingOfficerId,
        Validators.required,
      ],
      customerCreditLimit: [
        this.data?.customerCreditLimit,
        Validators.required,
      ],
      customerAgreement: [this.data?.customerAgreement ?? false],
      customerCreditDays: [this.data?.customerCreditDays ?? ""],
      customerTargetQuantity: [this.data?.customerTargetQuantity ?? ""],
      bankAccounts: this.fb.array([]),
    });

    this.customerForm.valueChanges.subscribe((value) => {
      // Convert empty string to null
      if (value.customerMarketingOfficerId === "") {
        this.customerForm
          .get("customerMarketingOfficerId")
          ?.patchValue(null, { emitEvent: false });
      }
    });
  }

  get bankAccounts(): FormArray {
    return this.customerForm.get("bankAccounts") as FormArray;
  }

  populateForm(): void {
    if (this.customerForm.get("id").value) {
      this.populateBankAccounts(this.data);
    } else {
      this.addItem();
    }
  }

  populateBankAccounts(data: Customer): void {
    data.bankAccounts.forEach((item: BankAccount) => this.addItem(item));
  }

  addItem(item?: BankAccount): void {
    this.bankAccounts.push(this.createBankAccount(item));
  }

  createBankAccount(item?: BankAccount): FormGroup {
    return this.fb.group({
      id: [item?.id ?? null],
      name: [item?.name ?? "", Validators.required],
      accNo: [item?.accNo ?? "", Validators.required],
      routingNo: [item?.routingNo ?? ""],
      bankName: [item?.bankName ?? "", Validators.required],
      branchName: [item?.branchName ?? "", Validators.required],
    });
  }

  setFormTitle(): void {
    if (this.customerForm.get("id").value) {
      this.formTitle = "Edit Customer";
    } else {
      this.formTitle = "Add Customer";
    }
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    if (fieldName === "customerMarketingOfficerId")
      this.customerForm?.get("customerMarketingOfficerId").setValue(null);
  }

  getAllRegions() {
    this.regionService.getAllRegions().subscribe((res) => {
      this.regions = res?.data?.item1;
    });
  }

  onSelectedRegionId(id: string) {
    this.customerForm.patchValue({
      customerZoneId: null,
      customerAreaId: null,
      customerTerritoryId: null,
    });
    this.isRegionSelected = true;
    this.getAllZonesByRegionId(id);
  }

  getAllZonesByRegionId(regionId: string) {
    let zoneRequest = new ZoneRequest();
    zoneRequest.page = -1;
    zoneRequest.regionId = regionId;
    this.zoneService.getZones(zoneRequest).subscribe((res) => {
      this.zones = res?.data?.item1;
    });
  }

  onSelectedZoneId(id: string) {
    this.customerForm.patchValue({
      customerAreaId: null,
      customerTerritoryId: null,
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

  onSelectedAreaId(id: string) {
    this.customerForm.patchValue({
      customerTerritoryId: null,
    });
    if (this.businessType === "1") {
      this.isAreaSelected = true;
      this.getAllTerritoriesByAreaId(id);
    }
  }

  getAllTerritoriesByAreaId(areaId: string) {
    let territoryRequest = new TerritoryRequest();
    territoryRequest.page = -1;
    territoryRequest.areaId = areaId;
    this.territoryService.getTerritories(territoryRequest).subscribe((res) => {
      this.territories = res?.data?.item1;
    });
  }

  getAllMarketingOfficers(): void {
    let employeeRequest = new EmployeeRequest();
    employeeRequest.page = -1;
    employeeRequest.departmentId = Department_Id_SALES_AND_MARKETING;
    this.employeeService.getEmployees(employeeRequest).subscribe((res) => {
      this.filteredMarketingOfficers = this.marketingOfficers =
        res?.data?.item1;
    });
  }

  handleMarketingOfficerSearch(event: any): void {
    const name = event.target?.name;
    if (name === "customerMarketingOfficerId") {
      const term = this.customerForm.get("customerMarketingOfficerId");
      this.filterMarketingOfficer(term.value || "");
    }
  }

  private filterMarketingOfficer(value: string) {
    const filterValue = value.toLowerCase();
    this.filteredMarketingOfficers = this.marketingOfficers?.filter((option) =>
      (option.firstName + " " + option.lastName)
        .toLowerCase()
        .includes(filterValue)
    );
  }

  getMarketingOfficerName(customerMarketingOfficerId) {
    if (!customerMarketingOfficerId) {
      return;
    }
    const marketingOfficer =
      this.marketingOfficers?.find(
        (x) => x?.id === customerMarketingOfficerId
      ) || this.data?.customerMarketingOfficer;
    return marketingOfficer?.firstName + " " + marketingOfficer?.lastName;
  }

  handleSuccessfulSave(res: any) {
    if (res?.succeeded) {
      this.toastr.success(res?.message);
      this.dialog.closeAll();
    } else {
      this.toastr.error(res?.message);
    }
  }

  addCustomer(body): void {
    this.customerService
      .createCustomer(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  updateCustomer(body): void {
    this.customerService
      .updateCustomer(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  deletedIds: string = "";
  onDeleteItem(id: string, itemIndex: number): void {
    if (id) this.deletedIds += `${id},`;
    this.bankAccounts.removeAt(itemIndex);
  }

  onSubmit() {
    if (this.customerForm.valid) {
      // this.isLoading = true;
      const formValue = this.customerForm.value;
      if (!formValue.id) {
        this.addCustomer(formValue);
      } else {
        formValue.deletedBankAccountIds = this.deletedIds;
        this.updateCustomer(formValue);
      }
    }
  }
}
