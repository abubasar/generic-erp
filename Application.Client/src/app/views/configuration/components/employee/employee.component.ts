import { HttpClient } from "@angular/common/http";
import { Component, OnInit, ViewChild } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { MatDialog } from "@angular/material/dialog";
import { MatMenuTrigger } from "@angular/material/menu";
import { MatTableDataSource } from "@angular/material/table";
import { Page_Size_Options } from "app/shared/consts/const";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { Observable, merge, of } from "rxjs";
import { AreaRequest } from "../../models/area/area-request.model";
import { Area } from "../../models/area/area.model";
import { DepartmentRequest } from "../../models/department/department-request.model";
import { Department } from "../../models/department/department.model";
import { DesignationRequest } from "../../models/designation/designation-request.model";
import { Designation } from "../../models/designation/designation.model";
import { EmployeeRequest } from "../../models/employee/employee-request.model";
import { Employee } from "../../models/employee/employee.model";
import { JobLocationRequest } from "../../models/job-location/job-location-request.model";
import { JobLocation } from "../../models/job-location/job-location.model";
import { Region } from "../../models/region/region.model";
import { ZoneRequest } from "../../models/zone/zone-request.model";
import { Zone } from "../../models/zone/zone.model";
import { AreaService } from "../../services/area.service";
import { DepartmentService } from "../../services/department.service";
import { DesignationService } from "../../services/designation.service";
import { EmployeeService } from "../../services/employee.service";
import { JobLocationService } from "../../services/job-location.service";
import { RegionService } from "../../services/region.service";
import { ZoneService } from "../../services/zone.service";
import { EmployeeFormComponent } from "./employee-form/employee-form.component";

@Component({
  selector: "app-employee",
  templateUrl: "./employee.component.html",
  styleUrls: ["./employee.component.scss"],
})
export class EmployeeComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  viewColumnForm: FormGroup;
  departments: Department[];
  designations: Designation[];
  filteredDesignations: Designation[] = [];
  dataSource: MatTableDataSource<Employee>;
  totalCount: number;
  employeeRequest = new EmployeeRequest();
  displayedColumns$: Observable<string[]>;
  employeeTypes: ENUM[];
  jobLocations: JobLocation[];
  isEmployeeTypeMarketingOfficer: boolean = false;
  regions: Region[];
  zones: Zone[];
  areas: Area[];
  pageSizeOptions: [] = Page_Size_Options;

  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  //  * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "actions", label: "Actions" },
    { def: "employeeIdNo", label: "Employee Id No." },
    { def: "fullName", label: "Name" },
    { def: "department", label: "Department" },
    { def: "designation", label: "Designation" },
    { def: "jobLocation", label: "Job Location" },
    { def: "contactNo", label: "Contact No" },
    { def: "email", label: "Email" },
    { def: "address", label: "Address" },
    { def: "dateOfBirth", label: "DOB" },
    { def: "joiningDate", label: "Joining Date" },
    { def: "genderName", label: "Gender Name" },
    { def: "bloodGroupName", label: "Blood Group Name" },
    { def: "maritalStatusName", label: "Marital Status" },
  ];

  constructor(
    private employeeService: EmployeeService,
    private departmentService: DepartmentService,
    private designationService: DesignationService,
    private enumValueService: EnumValueService,
    private jobLocationService: JobLocationService,
    private regionService: RegionService,
    private zoneService: ZoneService,
    private areaService: AreaService,
    public dialog: MatDialog,
    public dateFormatService: DateTimeFormatService,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.getAllJobLocations();
    this.getAllDepartments();
    this.getAllDesignations();
    this.initializeForm();
    this.getEmployees(this.employeeRequest);
    // this.getEmployeeTypes();
    // this.getAllRegions();
    // this.getAllZones();
    // this.getAllAreas();
    this.subscribeToFormChanges();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [null],
      // employeeType: [null],
      // regionId: [null],
      // zoneId: [null],
      // areaId: [null],
      departmentId: [null],
      designationId: [null],
      jobLocationId: [null],
      employeeIdNo: [null],
    });
    this.viewColumnForm = this.fb.group({
      employeeIdNo: [false],
      fullName: [true],
      department: [true],
      designation: [true],
      jobLocation: [true],
      contactNo: [true],
      email: [false],
      address: [false],
      dateOfBirth: [false],
      joiningDate: [true],
      genderName: [false],
      bloodGroupName: [false],
      maritalStatusName: [false],
      actions: [true],
    });
    this.updateDisplayedColumns();
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

  getAllJobLocations(): void {
    let jobLocationRequest = new JobLocationRequest();
    jobLocationRequest.page = -1;
    this.jobLocationService
      .getJobLocations(jobLocationRequest)
      .subscribe((res) => {
        // console.log(res?.data?.item1);
        this.jobLocations = res?.data?.item1;
      });
  }

  getAllDepartments(): void {
    let departmentRequest = new DepartmentRequest();
    departmentRequest.page = -1;
    this.departmentService
      .getDepartments(departmentRequest)
      .subscribe((res) => {
        // console.log(res?.data?.item1);
        this.departments = res?.data?.item1;
      });
  }

  getAllDesignations(): void {
    let designationRequest = new DesignationRequest();
    designationRequest.page = -1;
    this.designationService
      .getDesignations(designationRequest)
      .subscribe((res) => {
        // console.log(res?.data?.item1);
        this.designations = res?.data?.item1;
        this.filteredDesignations = this.designations;
      });
  }

  onSelectedDepartmentId(id: string) {
    this.searchForm.patchValue({
      designationId: null,
    });
    this.filteredDesignations = this.designations?.filter(
      (x) => x.departmentId == id
    );
    // console.log(this.designations, this.filteredDesignations);
  }

  getEmployees(request: EmployeeRequest): void {
    this.employeeService.getEmployees(request).subscribe((res) => {
      this.dataSource = new MatTableDataSource<Employee>(res?.data?.item1);
      console.log(res?.data?.item1);
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  // getEmployeeTypes() {
  //   this.enumValueService.getEmployeeTypes().subscribe((res) => {
  //     this.employeeTypes = res;
  //   });
  // }

  // getAllZones() {
  //   this.zoneService.getAllZones().subscribe((res) => {
  //     this.zones = res?.data?.item1;
  //   });
  // }

  // getAllAreas() {
  //   this.areaService.getAllAreas().subscribe((res) => {
  //     this.areas = res?.data?.item1;
  //   });
  // }

  // getAllRegions() {
  //   this.regionService.getAllRegions().subscribe((res) => {
  //     this.regions = res?.data?.item1;
  //   });
  // }

  // onSelectedRegionId(id: string) {
  //   this.searchForm.patchValue({
  //     zoneId: null,
  //     areaId: null,
  //   });

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
  //   this.searchForm.patchValue({
  //     areaId: null,
  //   });
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

  remove(id): void {
    this.employeeService.deleteEmployee(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getEmployees(this.employeeRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }
  openForm(employee?: Employee): void {
    const dialogRef = this.dialog.open(EmployeeFormComponent, {
      disableClose: true,
      data: employee,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getEmployees(this.employeeRequest);
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
    this.employeeRequest = new EmployeeRequest();
    this.getEmployees(this.employeeRequest);
  }

  onSearch() {
    this.loading = true;
    this.employeeRequest = {
      ...this.employeeRequest,
      ...this.searchForm.value,
    };
    this.getEmployees(this.employeeRequest);
  }

  onPageChange(pageEvent) {
    const { pageIndex, pageSize } = pageEvent;

    this.employeeRequest = {
      ...this.employeeRequest,
      ...this.searchForm.value,
      page: pageIndex,
      rowsPerPage: pageSize,
    };

    this.getEmployees(this.employeeRequest);
  }
  printPdf() {
    this.http
      .post(environment.apiURL + "/Employee/print", this.searchForm.value, {
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
