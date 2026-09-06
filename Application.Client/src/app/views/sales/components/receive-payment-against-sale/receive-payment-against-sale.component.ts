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
import { ReceivePaymentStatus } from "app/shared/enums/receivePaymentStatus";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { CostCenter } from "app/views/configuration/models/cost-center/cost-center.model";
import { Customer } from "app/views/configuration/models/customer/customer.model";
import { EmployeeRequest } from "app/views/configuration/models/employee/employee-request.model";
import { Employee } from "app/views/configuration/models/employee/employee.model";
import { CostCenterService } from "app/views/configuration/services/cost-center.service";
import { CustomerService } from "app/views/configuration/services/customer.service";
import { EmployeeService } from "app/views/configuration/services/employee.service";
import { PendingCheckedCount } from "app/views/purchase/models/pending-checked-count";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { Observable, merge, of } from "rxjs";
import { ReceivePaymentAgainstSaleAggregatorModel } from "../../models/receive-payment-against-sale/receive-payment-against-sale-aggregator.model";
import { ReceivePaymentAgainstSaleResponseDTO } from "../../models/receive-payment-against-sale/receive-payment-against-sale-response-dto.model";
import { ReceivePaymentAgainstSaleSearchRequestDTO } from "../../models/receive-payment-against-sale/receive-payment-against-sale-search-request-dto.model";
import { ReceivePaymentAgainstSaleService } from "../../services/receive-payment-against-sale.service";
@Component({
  selector: "app-receive-payment-against-sale",
  templateUrl: "./receive-payment-against-sale.component.html",
  styleUrls: ["./receive-payment-against-sale.component.scss"],
})
export class ReceivePaymentAgainstSaleComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  viewColumnForm: FormGroup;
  dataSource: MatTableDataSource<ReceivePaymentAgainstSaleResponseDTO>;
  reportAggregator = new ReceivePaymentAgainstSaleAggregatorModel();
  totalCount: number;
  customers: Customer[];
  filterCustomers: Customer[];
  marketingOfficers: Employee[];
  filterMarketingOfficers: Employee[];
  costCenters: CostCenter[];
  receivePaymentStatuses: ENUM[];
  pendingCheckedModel: PendingCheckedCount;
  receivePaymentAgainstSaleRequest =
    new ReceivePaymentAgainstSaleSearchRequestDTO();
  displayedColumns$: Observable<string[]>;
  pageSizeOptions: [] = Page_Size_Options;
  currentFinancialYearId: string;

  private path = {
    addNew: "sales/receive-payment-against-sale/add-new",
    edit: "sales/receive-payment-against-sale",
    view: "sales/receive-payment-against-sale/view",
  };

  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  //  * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "actions", label: "Actions" },
    { def: "code", label: "code" },
    { def: "paymentDate", label: "Payment Date" },
    { def: "invoiceNo", label: "Invoice No" },
    { def: "customerId", label: "Customer Name" },
    { def: "costCenterId", label: "Cost Center" },
    { def: "amount", label: "Amount" },
    { def: "toAccountId", label: "To Account" },
    { def: "remark", label: "Remark" },
    { def: "status", label: "Status" },
    { def: "checkedBy", label: "Checked By" },
    { def: "approvedBy", label: "Approved By" },
    { def: "createdOn", label: "Created On" },
    { def: "updatedOn", label: "Updated On" },
    { def: "createdBy", label: "Created By" },
    { def: "updatedBy", label: "Updated By" },
    { def: "receipt", label: "Receipt" },
  ];

  constructor(
    public dateFormatService: DateTimeFormatService,
    private receivePaymentAgainstSaleService: ReceivePaymentAgainstSaleService,
    private customerService: CustomerService,
    private employeeService: EmployeeService,
    private costCenterService: CostCenterService,
    private enumValueService: EnumValueService,
    public statusColorService: StatusColorService,
    private jwtAuth: JwtAuthService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private router: Router, // private localStorageService: LocalStoreService
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService,
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.getCurrentFinancialYearId();
    this.getPendingCheckedCount();
    this.getReceivePaymentAgainstSales(this.receivePaymentAgainstSaleRequest);
    this.subscribeToFormChanges();
  }

  openExpansionPanel() {
    this.panelOpenState = true;
    this.getAllReceivePaymentStatuses();
    this.getAllCustomers();
    this.getAllCostCenters();
    this.getAllMarketingOfficers();
    // this.getAllFinancialYears();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
      fromDate: [null],
      toDate: [null],
      customerId: [null],
      customerMarketingOfficerId: [null],
      costCenterId: [null],
      receivePaymentStatus: [null],
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
      if (value.costCenterId === "") {
        this.searchForm
          .get("costCenterId")
          ?.patchValue(null, { emitEvent: false });
      }
      if (value.customerMarketingOfficerId === "") {
        this.searchForm
          .get("customerMarketingOfficerId")
          .patchValue(null, { emitEvent: false });
      }
    });

    this.viewColumnForm = this.fb.group({
      code: [true],
      paymentDate: [true],
      invoiceNo: [true],
      customerId: [true],
      amount: [true],
      costCenterId: [true],
      toAccountId: [true],
      status: [true],
      remark: [true],
      checkedBy: [false],
      approvedBy: [false],
      createdOn: [false],
      updatedOn: [false],
      createdBy: [false],
      updatedBy: [false],
      receipt: [true],
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
        .map((col) => col.def),
    );
  }

  private getCurrentFinancialYearId() {
    this.jwtAuth.userProfile.subscribe((res: UserProfile) => {
      this.currentFinancialYearId = res.fyid;
    });
  }

  getReceivePaymentAgainstSales(
    requestBody: ReceivePaymentAgainstSaleSearchRequestDTO,
  ): void {
    this.receivePaymentAgainstSaleService
      .getReceivePaymentAgainstSales(requestBody)
      .subscribe((res) => {
        this.dataSource =
          new MatTableDataSource<ReceivePaymentAgainstSaleResponseDTO>(
            res?.data?.item1,
          );
        this.reportAggregates(requestBody);
        this.totalCount = res?.data?.item2;
        this.loading = false;
      });
  }

  reportAggregates(
    requestBody: ReceivePaymentAgainstSaleSearchRequestDTO,
  ): void {
    this.receivePaymentAgainstSaleService
      .reportAggregates(requestBody)
      .subscribe((res) => {
        this.reportAggregator = res.data;
      });
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    if (fieldName === "customerId") {
      this.searchForm?.get("customerId").setValue(null);
    } else if (fieldName === "customerMarketingOfficerId") {
      this.searchForm?.get("customerMarketingOfficerId").setValue(null);
    }
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

  getAllMarketingOfficers(): void {
    let employeeRequest = new EmployeeRequest();
    employeeRequest.page = -1;
    employeeRequest.departmentId = Department_Id_SALES_AND_MARKETING;
    this.employeeService.getEmployees(employeeRequest).subscribe((res) => {
      this.filterMarketingOfficers = this.marketingOfficers =
        res?.data?.item1?.sort((a, b) =>
          (a.firstName + a.lastName).localeCompare(b.firstName + b.lastName),
        );
    });
  }

  handleMarketingOfficerSearch(): void {
    const term = this.searchForm
      ?.get("customerMarketingOfficerId")
      ?.value?.toLowerCase();
    this.filterMarketingOfficers = this.marketingOfficers?.filter((option) =>
      (option.firstName + " " + option.lastName).toLowerCase().includes(term),
    );
  }

  getMarketingOfficerName(customerMarketingOfficerId: string) {
    if (!customerMarketingOfficerId) {
      return;
    }
    const marketingOfficerData = this.marketingOfficers?.find(
      (marketingOfficer) => marketingOfficer?.id === customerMarketingOfficerId,
    );
    return (
      marketingOfficerData?.firstName + " " + marketingOfficerData?.lastName
    );
  }

  getAllCustomers() {
    this.customerService.getAllCustomers().subscribe((res) => {
      this.filterCustomers = this.customers = res?.data?.item1;
    });
  }

  getAllCostCenters() {
    this.costCenterService.getAllCostCenters().subscribe((res) => {
      this.costCenters = res?.data?.item1;
    });
  }

  getAllReceivePaymentStatuses() {
    this.enumValueService.getReceivePaymentStatuses().subscribe((res) => {
      this.receivePaymentStatuses = res;
    });
  }

  getReceivePaymentStatus(value: number) {
    return this.statusColorService.getReceivePaymentStatus(value);
  }

  getReceivePaymentStatusName(value: number) {
    return ReceivePaymentStatus[value];
  }

  getPendingCheckedCount(): void {
    this.receivePaymentAgainstSaleService
      .getPendingCheckedCount()
      .subscribe((res) => {
        this.pendingCheckedModel = res.data;
      });
  }

  getPending() {
    this.loading = true;
    this.receivePaymentAgainstSaleRequest.receivePaymentStatus = 1;
    this.getReceivePaymentAgainstSales(this.receivePaymentAgainstSaleRequest);
  }

  getChecked() {
    this.loading = true;
    this.receivePaymentAgainstSaleRequest.receivePaymentStatus = 2;
    this.getReceivePaymentAgainstSales(this.receivePaymentAgainstSaleRequest);
  }

  remove(id: string): void {
    this.receivePaymentAgainstSaleService
      .deleteReceivePaymentAgainstSale(id)
      .subscribe((res) => {
        if (res?.succeeded) {
          this.getReceivePaymentAgainstSales(
            this.receivePaymentAgainstSaleRequest,
          );
          this.toastr.info(res?.message);
        } else {
          this.toastr.error(res?.message);
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
      data.message,
    );
  }

  reload() {
    this.searchForm.reset();
    this.loading = true;
    this.receivePaymentAgainstSaleRequest =
      new ReceivePaymentAgainstSaleSearchRequestDTO();
    this.getReceivePaymentAgainstSales(this.receivePaymentAgainstSaleRequest);
  }

  onSearch() {
    this.loading = true;
    this.receivePaymentAgainstSaleRequest = {
      ...this.receivePaymentAgainstSaleRequest,
      ...this.searchForm.value,
    };
    this.getReceivePaymentAgainstSales(this.receivePaymentAgainstSaleRequest);
  }

  navigateToAddNew() {
    this.router.navigate([this.path.addNew]);
  }

  navigateToView(item: ReceivePaymentAgainstSaleResponseDTO) {
    this.router.navigate([this.path.edit, item.id]);
    // this.localStorageService.setItem(item.id, item);
  }

  onPageChange(pageEvent) {
    this.receivePaymentAgainstSaleRequest.page = pageEvent.pageIndex;
    this.receivePaymentAgainstSaleRequest.rowsPerPage = pageEvent.pageSize;
    this.getReceivePaymentAgainstSales(this.receivePaymentAgainstSaleRequest);
  }

  openPopup(id: string) {
    this.http
      .get(
        environment.apiURL +
          "/ReceivePaymentAgainstSale/single_file_view_download/" +
          id,
        {
          responseType: "blob",
        },
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

  printPdf(id: string) {
    this.http
      .get(environment.apiURL + "/ReportReceivePaymentAgainstSale/" + id, {
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
