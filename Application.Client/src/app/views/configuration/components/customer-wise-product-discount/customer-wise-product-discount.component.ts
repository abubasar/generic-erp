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
import { Router } from "@angular/router";
import {
  Department_Id_SALES_AND_MARKETING,
  Page_Size_Options,
} from "app/shared/consts/const";
import { CustomerWiseProductDiscountStatus } from "app/shared/enums/customerWiseProductDiscountStatus";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { Observable, merge, of } from "rxjs";
import { Area } from "../../models/area/area.model";
import { CustomerWiseProductDiscountRequest } from "../../models/customer-wise-product-discount/customer-wise-product-discount-request.model";
import { CustomerWiseProductDiscount } from "../../models/customer-wise-product-discount/customer-wise-product-discount.model";
import { Customer } from "../../models/customer/customer.model";
import { EmployeeRequest } from "../../models/employee/employee-request.model";
import { Employee } from "../../models/employee/employee.model";
import { Zone } from "../../models/zone/zone.model";
import { AreaService } from "../../services/area.service";
import { CustomerWiseProductDiscountService } from "../../services/customer-wise-product-discount.service";
import { CustomerService } from "../../services/customer.service";
import { EmployeeService } from "../../services/employee.service";
import { ZoneService } from "../../services/zone.service";
import { CustomerWiseProductDiscountAddFormComponent } from "./customer-wise-product-discount-add-form/customer-wise-product-discount-add-form.component";

@Component({
  selector: "app-customer-wise-product-discount",
  templateUrl: "./customer-wise-product-discount.component.html",
  styleUrls: ["./customer-wise-product-discount.component.scss"],
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
export class CustomerWiseProductDiscountComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  customers: Customer[];
  allCustomer: Customer[];
  filterCustomers: Customer[];
  zones: Zone[];
  filterArea: Area[];
  allArea: Area[];
  filterMarketingOfficer: Employee[];
  allMarketingOfficer: Employee[];
  viewColumnForm: FormGroup;
  expandedElement: any;
  dataSource: MatTableDataSource<CustomerWiseProductDiscount>;
  totalCount: number;
  customerWiseProductDiscountStatuses: ENUM[];
  customerWiseProductDiscountRequest = new CustomerWiseProductDiscountRequest();
  displayedColumns$: Observable<string[]>;
  pageSizeOptions: [] = Page_Size_Options;

  private path = {
    addNew: "configuration/customer-wise-product-discount/add-new",
    edit: "configuration/customer-wise-product-discount",
  };

  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  //  * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "expand", label: "Expand" },
    { def: "actions", label: "Actions" },
    { def: "code", label: "Customer Code" },
    { def: "customerId", label: "Customer Name" },
    { def: "applicableDate", label: "Applicable Date" },
    { def: "isActive", label: "Is Active" },
    { def: "status", label: "Status" },
    { def: "checkedBy", label: "Checked By" },
    { def: "approvedBy", label: "Approved By" },
    { def: "createdOn", label: "Created On" },
    { def: "createdBy", label: "Created By" },
  ];

  constructor(
    private customerWiseProductDiscountService: CustomerWiseProductDiscountService,
    public dateFormatService: DateTimeFormatService,
    private customerService: CustomerService,
    private zoneService: ZoneService,
    private areaService: AreaService,
    private employeeService: EmployeeService,
    private enumValueService: EnumValueService,
    public statusColorService: StatusColorService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private router: Router,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.getAllCustomers();
    this.getAllZones();
    this.getAllArea();
    this.getAllMarketingOfficer();
    this.getAllCustomerWiseProductDiscountStatuses();
    this.updateDiscountRequest();
    this.getCustomerWiseProductDiscounts(
      this.customerWiseProductDiscountRequest
    );
    this.subscribeToFormChanges();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
      fromDate: [null],
      toDate: [null],
      customerId: [null],
      customerZoneId: [null],
      customerAreaId: [null],
      customerMarketingOfficerId: [null],
      isActive: [true],
      customerWiseProductDiscountStatus: [null],
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
      if (value.customerMarketingOfficerId === "") {
        this.searchForm
          .get("customerMarketingOfficerId")
          ?.patchValue(null, { emitEvent: false });
      }
    });

    this.viewColumnForm = this.fb.group({
      expand: [true],
      code: [true],
      customerId: [true],
      applicableDate: [true],
      isActive: [true],
      status: [true],
      checkedBy: [true],
      approvedBy: [true],
      createdOn: [true],
      createdBy: [true],
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

  collapsed: string = "collapsed";
  expanded: string = "expanded";

  expandAll() {
    if (this.collapsed === "collapsed") this.collapsed = "expanded";
    else this.collapsed = "collapsed";
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    if (fieldName === "customerId") {
      this.searchForm?.get("customerId").setValue(null);
    }
    if (fieldName == "customerMarketingOfficerId") {
      this.searchForm.patchValue({
        customerMarketingOfficerId: null,
      });
      this.filterMarketingOfficer = this.allMarketingOfficer;
    }
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
    if (id) {
      this.filterArea = this.allArea?.filter((x) => x.zoneId === id);
    } else {
      this.filterArea = this.allArea;
    }
  }

  getAllArea() {
    this.areaService.getAllAreas().subscribe((res) => {
      this.filterArea = this.allArea = res?.data?.item1;
    });
  }

  getAllMarketingOfficer(): void {
    let employeeRequest = new EmployeeRequest();
    employeeRequest.page = -1;
    employeeRequest.departmentId = Department_Id_SALES_AND_MARKETING;
    this.employeeService.getEmployees(employeeRequest).subscribe((res) => {
      this.filterMarketingOfficer = this.allMarketingOfficer = res?.data?.item1;
    });
  }

  handleMarketingOfficerSearch(): void {
    const term = this.searchForm
      ?.get("customerMarketingOfficerId")
      ?.value?.toLowerCase();
    this.filterMarketingOfficer = this.allMarketingOfficer?.filter((option) =>
      (option.firstName + " " + option.lastName).toLowerCase().includes(term)
    );
  }

  getMarketingOfficerName(customerMarketingOfficerId: string) {
    if (!customerMarketingOfficerId) {
      return;
    }
    const marketingOfficerData = this.allMarketingOfficer?.find(
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
    this.filterCustomers = this.allCustomer = this.customers?.filter(
      (x) => x.customerMarketingOfficerId === customerMarketingOfficerId
    );
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
    this.filterCustomers = this.allCustomer?.filter(
      (option) =>
        option.name?.toLowerCase().includes(filterValue) ||
        option.code?.slice(-4).toLowerCase().includes(filterValue)
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
        this.allCustomer =
        this.customers =
          res.data?.item1;
    });
  }

  getCustomerWiseProductDiscounts(
    request: CustomerWiseProductDiscountRequest
  ): void {
    this.customerWiseProductDiscountService
      .getCustomerWiseProductDiscounts(request)
      .subscribe((res) => {
        this.dataSource = new MatTableDataSource<CustomerWiseProductDiscount>(
          res?.data?.item1
        );
        this.totalCount = res?.data?.item2;
        this.loading = false;
      });
  }

  getAllCustomerWiseProductDiscountStatuses() {
    this.enumValueService
      .getCustomerWiseProductDiscountStatuses()
      .subscribe((res) => {
        this.customerWiseProductDiscountStatuses = res;
      });
  }

  getCustomerWiseProductDiscountStatus(value) {
    return this.statusColorService.getCustomerWiseProductDiscountStatus(value);
  }

  getCustomerWiseProductDiscountStatusName(value: number) {
    return CustomerWiseProductDiscountStatus[value];
  }

  remove(id): void {
    this.customerWiseProductDiscountService
      .deleteCustomerWiseProductDiscount(id)
      .subscribe((res) => {
        if (res?.succeeded) {
          this.getCustomerWiseProductDiscounts(
            this.customerWiseProductDiscountRequest
          );
          this.toastr.info(res?.message);
        } else {
          this.toastr.error(res?.message);
        }
      });
  }

  navigateToAddNew() {
    this.router.navigate([this.path.addNew]);
  }

  navigateToView(item) {
    this.router.navigate([this.path.edit, item.id]);
  }

  openAddNewForm() {
    const dialogRef = this.dialog.open(
      CustomerWiseProductDiscountAddFormComponent,
      {
        disableClose: true,
      }
    );
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getCustomerWiseProductDiscounts(
          this.customerWiseProductDiscountRequest
        );
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
    this.searchForm.reset(); // make all fields value null
    this.searchForm.patchValue({
      keyword: "", // it should be string
      isActive: true, // it should be bool
    });
    this.updateDiscountRequest(0, 15);
    console.log("reload request:", this.customerWiseProductDiscountRequest);
    this.getCustomerWiseProductDiscounts(
      this.customerWiseProductDiscountRequest
    );
  }

  onSearch() {
    this.loading = true;
    this.updateDiscountRequest();
    this.getCustomerWiseProductDiscounts(
      this.customerWiseProductDiscountRequest
    );
  }

  onPageChange(pageEvent) {
    this.updateDiscountRequest(pageEvent.pageIndex, pageEvent.pageSize);
    this.getCustomerWiseProductDiscounts(
      this.customerWiseProductDiscountRequest
    );
  }

  private updateDiscountRequest(
    pageIndex = this.customerWiseProductDiscountRequest.page,
    pageSize = this.customerWiseProductDiscountRequest.rowsPerPage
  ) {
    this.customerWiseProductDiscountRequest = {
      ...this.customerWiseProductDiscountRequest,
      ...this.searchForm.value,
      page: pageIndex,
      rowsPerPage: pageSize,
    };
  }
  printPdf(id) {
    this.http
      .get(environment.apiURL + "/ReportCustomerWiseProductDiscount/" + id, {
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
  printDetailsPdf() {
    this.http
      .post(
        environment.apiURL +
          "/CustomerWiseProductDiscount/customer-wise-product-discount-print",
        this.searchForm.value,
        {
          responseType: "blob",
        }
      )
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
