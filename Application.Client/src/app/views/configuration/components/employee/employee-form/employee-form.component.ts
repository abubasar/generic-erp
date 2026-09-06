import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { EmployeeType } from "app/shared/enums/employeeTypes";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { AreaRequest } from "app/views/configuration/models/area/area-request.model";
import { Area } from "app/views/configuration/models/area/area.model";

import { DepartmentRequest } from "app/views/configuration/models/department/department-request.model";
import { Department } from "app/views/configuration/models/department/department.model";
import { DesignationRequest } from "app/views/configuration/models/designation/designation-request.model";
import { Designation } from "app/views/configuration/models/designation/designation.model";
import { Employee } from "app/views/configuration/models/employee/employee.model";
import { JobLocationRequest } from "app/views/configuration/models/job-location/job-location-request.model";
import { JobLocation } from "app/views/configuration/models/job-location/job-location.model";
import { Region } from "app/views/configuration/models/region/region.model";
import { ZoneRequest } from "app/views/configuration/models/zone/zone-request.model";
import { Zone } from "app/views/configuration/models/zone/zone.model";
import { AreaService } from "app/views/configuration/services/area.service";
import { DepartmentService } from "app/views/configuration/services/department.service";
import { DesignationService } from "app/views/configuration/services/designation.service";
import { EmployeeService } from "app/views/configuration/services/employee.service";
import { JobLocationService } from "app/views/configuration/services/job-location.service";
import { RegionService } from "app/views/configuration/services/region.service";
import { ZoneService } from "app/views/configuration/services/zone.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-employee-form",
  templateUrl: "./employee-form.component.html",
  styleUrls: ["./employee-form.component.scss"],
})
export class EmployeeFormComponent implements OnInit {
  formTitle: string;
  employeeForm: FormGroup;
  genders: ENUM[];
  bloodGroups: ENUM[];
  maritalStatuses: ENUM[];
  departments: Department[];
  isDepartmentSelected: boolean = false;
  designations: Designation[];
  filteredDesignations: Designation[] = [];
  jobLocations: JobLocation[];
  employeeTypes: ENUM[];
  isEmployeeTypeMarketingOfficer: boolean = false;
  regions: Region[];
  isRegionSelected: boolean = false;
  zones: Zone[];
  isZoneSelected: boolean = false;
  areas: Area[];

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: Employee,
    private employeeService: EmployeeService,
    private departmentService: DepartmentService,
    private designationService: DesignationService,
    private jobLocationService: JobLocationService,
    private regionService: RegionService,
    private zoneService: ZoneService,
    private areaService: AreaService,
    private enumValueService: EnumValueService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.getAllBloodGroups();
    this.getAllDepartments();
    this.getAllGenders();
    this.getAllJobLocations();
    this.getAllMaritalStatuses();
    // this.getAllEmployeeTypes();
    this.getAllDesignations();
    this.initializeForm();
    console.log(this.data);
    if (this.data?.departmentId) {
      // this.getAllDesignations();
      this.isDepartmentSelected = true;
    }
    // if (this.data?.employeeType == EmployeeType.SALES_AND_MARKETING) {
    //   this.getAllRegions();
    //   this.isEmployeeTypeMarketingOfficer = true;
    //   this.isRegionSelected = true;
    // }
    // if (this.data?.regionId) {
    //   this.getAllZonesByRegionId(this.data?.regionId);
    // }
    // if (this.data?.zoneId) {
    //   this.isZoneSelected = true;
    //   this.getAllAreasByZoneId(this.data.zoneId);
    // }
  }

  initializeForm() {
    this.employeeForm = this.fb.group({
      id: [this.data?.id ?? null],
      firstName: [this.data?.firstName, Validators.required],
      lastName: [this.data?.lastName, Validators.required],
      employeeIdNo: [this.data?.employeeIdNo, Validators.required],
      contactNo: [
        this.data?.contactNo,
        [Validators.required, Validators.pattern("^01[3-9]\\d{8}$")],
      ],
      email: [this.data?.email ?? ""],
      address: [this.data?.address, Validators.required],
      dateOfBirth: [this.data?.dateOfBirth, Validators.required],
      joiningDate: [this.data?.joiningDate, Validators.required],
      gender: [this.data?.gender, Validators.required],
      bloodGroup: [this.data?.bloodGroup, Validators.required],
      maritalStatus: [this.data?.maritalStatus, Validators.required],
      departmentId: [this.data?.departmentId, Validators.required],
      designationId: [this.data?.designationId, Validators.required],
      jobLocationId: [this.data?.jobLocationId, Validators.required],
      // employeeType: [this.data?.employeeType ?? null, Validators.required],
      // regionId: [this.data?.regionId ?? null],
      // zoneId: [this.data?.zoneId ?? null],
      // areaId: [this.data?.areaId ?? null],
    });
    if (
      this.employeeForm.get("id").value === "" ||
      this.employeeForm.get("id").value == null
    ) {
      this.formTitle = "Add Employee";
    } else {
      this.formTitle = "Edit Employee";
    }
  }

  getAllGenders() {
    this.enumValueService.getGenders().subscribe((res) => {
      this.genders = res;
    });
  }

  getAllBloodGroups() {
    this.enumValueService.getBloodGroups().subscribe((res) => {
      this.bloodGroups = res;
    });
  }

  getAllMaritalStatuses() {
    this.enumValueService.getMaritalStatues().subscribe((res) => {
      this.maritalStatuses = res;
    });
  }

  getAllDepartments(): void {
    let departmentRequest = new DepartmentRequest();
    departmentRequest.page = -1;
    this.departmentService
      .getDepartments(departmentRequest)
      .subscribe((res) => {
        console.log(res?.data?.item1);
        this.departments = res?.data?.item1;
      });
  }

  getAllDesignations(): void {
    let designationRequest = new DesignationRequest();
    designationRequest.page = -1;
    this.designationService
      .getDesignations(designationRequest)
      .subscribe((res) => {
        console.log(res?.data?.item1);
        this.designations = res?.data?.item1;
        this.onSelectedDepartmentId(this.data?.departmentId);
      });
  }

  getAllJobLocations(): void {
    let jobLocationRequest = new JobLocationRequest();
    jobLocationRequest.page = -1;
    this.jobLocationService
      .getJobLocations(jobLocationRequest)
      .subscribe((res) => {
        console.log(res?.data?.item1);
        this.jobLocations = res?.data?.item1;
      });
  }

  // getAllEmployeeTypes() {
  //   this.enumValueService.getEmployeeTypes().subscribe((res) => {
  //     this.employeeTypes = res;
  //   });
  // }

  onSelectedDepartmentId(id: string) {
    this.isDepartmentSelected = true;
    this.filteredDesignations = this.designations?.filter(
      (x) => x.departmentId == id
    );
    console.log(this.designations, this.filteredDesignations);
  }

  // onSelectedEmployeeType(value: number) {
  //   if (value === EmployeeType.SALES_AND_MARKETING) {
  //     this.isEmployeeTypeMarketingOfficer = true;
  //     this.getAllRegions();
  //   } else {
  //     this.employeeForm.patchValue({
  //       regionId: null,
  //       zoneId: null,
  //       areaId: null,
  //     });
  //     this.isEmployeeTypeMarketingOfficer = false;
  //   }
  // }

  // getAllRegions() {
  //   this.regionService.getAllRegions().subscribe((res) => {
  //     this.regions = res?.data?.item1;
  //   });
  // }

  // onSelectedRegionId(id: string) {
  //   this.employeeForm.patchValue({
  //     zoneId: null,
  //     areaId: null,
  //   });
  //   this.isRegionSelected = true;
  //   this.getAllZonesByRegionId(id);
  // }

  // getAllZonesByRegionId(regionId: string) {
  //   let zoneRequest = new ZoneRequest();
  //   zoneRequest.page = -1;
  //   zoneRequest.regionId = regionId;
  //   this.zoneService.getZones(zoneRequest).subscribe((res) => {
  //     this.zones = res?.data?.item1;
  //   });
  // }

  // onSelectedZoneId(id: string) {
  //   this.employeeForm.patchValue({
  //     areaId: null,
  //   });
  //   this.isZoneSelected = true;
  //   this.getAllAreasByZoneId(id);
  // }

  // getAllAreasByZoneId(zoneId: string) {
  //   let areaRequest = new AreaRequest();
  //   areaRequest.page = -1;
  //   areaRequest.zoneId = zoneId;
  //   this.areaService.getAreas(areaRequest).subscribe((res) => {
  //     this.areas = res?.data?.item1;
  //   });
  // }

  // getDeliveryNoteStatus(value: number) {
  //   return this.statusColorService.getDeliveryNoteStatus(value);
  // }

  // getDeliveryNoteStatusName(value: number) {
  //   return DeliveryNoteStatus[value];
  // }

  addEmployee(body): void {
    this.employeeService.createEmployee(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  updateEmployee(body): void {
    this.employeeService.updateEmployee(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  onSubmit() {
    if (this.employeeForm.value) {
      if (
        this.employeeForm.get("id").value === "" ||
        this.employeeForm.get("id").value == null
      ) {
        this.addEmployee(this.employeeForm.value);
      } else {
        this.updateEmployee(this.employeeForm.value);
      }
    }
  }
}
