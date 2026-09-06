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
import { Sort } from "@angular/material/sort";
import { MatTableDataSource } from "@angular/material/table";
import { Router } from "@angular/router";
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
import { PaymentMode } from "app/views/configuration/models/payment-mode/payment-mode.model";
import { AccountService } from "app/views/configuration/services/account.service";
import { CostCenterService } from "app/views/configuration/services/cost-center.service";
import { CustomerService } from "app/views/configuration/services/customer.service";
import { PaymentModeService } from "app/views/configuration/services/payment-mode.service";
import { PendingCheckedCount } from "app/views/purchase/models/pending-checked-count";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { Observable, merge, of } from "rxjs";
import { ReceivePaymentResponseDTO } from "../../../sales/models/receive-payment/receive-payment-response-dto.model";
import { ReceivePaymentSearchRequestDTO } from "../../../sales/models/receive-payment/receive-payment-search-request-dto.model";
import { ReceivePaymentService } from "../../../sales/services/receive-payment.service";
import { ReceivePaymentAggregatorModel } from "../../models/receive-payment/receive-payment-aggregator.model";

@Component({
  selector: "app-receive-payment",
  templateUrl: "./receive-payment.component.html",
  styleUrls: ["./receive-payment.component.scss"],
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
export class ReceivePaymentComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  viewColumnForm: FormGroup;
  expandedElement: any;
  dataSource: MatTableDataSource<ReceivePaymentResponseDTO>;
  reportAggregator = new ReceivePaymentAggregatorModel();
  totalCount: number;
  customers: Customer[];
  filterCustomers: Customer[];
  costCenters: CostCenter[];
  paymentModes: PaymentMode[];
  receivePaymentRequest = new ReceivePaymentSearchRequestDTO();
  receivePaymentStatuses: ENUM[];
  pendingCheckedModel: PendingCheckedCount;
  displayedColumns$: Observable<string[]>;
  currentFinancialYearId: string;

  private path = {
    addNew: "sales/money-receipt/add-new",
    edit: "sales/money-receipt",
    view: "sales/money-receipt/view",
  };

  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  //  * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "expand", label: "Expand" },
    { def: "actions", label: "Actions" },
    { def: "code", label: "Code" },
    { def: "paymentDate", label: "Payment Date" },
    { def: "customerId", label: "Customer" },
    { def: "fundTransferTransactionTypeId", label: "Transaction Type" },
    { def: "transactionNumber", label: "Transaction No" },
    { def: "costCenterId", label: "Cost Center" },
    { def: "paymentModeId", label: "Payment Mode" },
    { def: "totalAmount", label: "Total Amount" },
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
    public statusColorService: StatusColorService,
    private receivePaymentService: ReceivePaymentService,
    private customerService: CustomerService,
    private costCenterService: CostCenterService,
    private paymentModeService: PaymentModeService,
    private jwtAuth: JwtAuthService,
    private accountService: AccountService,
    private enumValueService: EnumValueService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private router: Router,
    private confirmDialogService: ConfirmDialogService // private localStorageService: LocalStoreService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.getPendingCheckedCount();
    this.getReceivePayments(this.receivePaymentRequest);
    this.getCurrentFinancialYearId();
    this.subscribeToFormChanges();
  }
  openExpansionPanel() {
    this.panelOpenState = true;
    this.getAllReceivePaymentStatuses();
    this.getAllCustomers();
    this.getAllCostCenters();
    this.getAllPaymentModes();
  }
  private initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
      fromDate: [null],
      toDate: [null],
      transactionNumber: [""],
      customerId: [null],
      costCenterId: [null],
      paymentModeId: [null],
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
    });
    this.viewColumnForm = this.fb.group({
      expand: [true],
      code: [true],
      paymentDate: [true],
      customerId: [true],
      fundTransferTransactionTypeId: [false],
      transactionNumber: [false],
      costCenterId: [true],
      paymentModeId: [true],
      totalAmount: [true],
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
        .map((col) => col.def)
    );
  }

  collapsed: string = "collapsed";
  expanded: string = "expanded";

  expandAll() {
    if (this.collapsed === "collapsed") this.collapsed = "expanded";
    else this.collapsed = "collapsed";
  }

  private getCurrentFinancialYearId() {
    this.jwtAuth.userProfile.subscribe((res: UserProfile) => {
      this.currentFinancialYearId = res.fyid;
    });
  }

  clearInput(evt: any): void {
    evt.stopPropagation();
    this.searchForm?.get("customerId").setValue(null);
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
      this.filterCustomers = this.customers = res.data?.item1;
    });
  }

  getAllCostCenters() {
    this.costCenterService.getAllCostCenters().subscribe((res) => {
      this.costCenters = res.data?.item1;
    });
  }

  getAllPaymentModes() {
    this.paymentModeService.getAllPaymentModes().subscribe((res) => {
      this.paymentModes = res.data?.item1;
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

  getReceivePayments(requestBody: ReceivePaymentSearchRequestDTO): void {
    this.receivePaymentService
      .getReceivePayments(requestBody)
      .subscribe((res) => {
        this.dataSource = new MatTableDataSource<ReceivePaymentResponseDTO>(
          res?.data?.item1
        );
        this.reportAggregates(requestBody);
        this.totalCount = res?.data?.item2;
        this.loading = false;
      });
  }

  reportAggregates(requestBody: ReceivePaymentSearchRequestDTO): void {
    this.receivePaymentService
      .reportAggregates(requestBody)
      .subscribe((res) => {
        this.reportAggregator = res.data;
      });
  }

  sortData(sort: Sort) {
    this.receivePaymentRequest.orderBy = sort.active;
    let isAsc = sort.direction == "asc";
    if (isAsc) this.receivePaymentRequest.isAscending = true;
    else this.receivePaymentRequest.isAscending = false;
    this.getReceivePayments(this.receivePaymentRequest);
  }

  getPendingCheckedCount(): void {
    this.receivePaymentService.getPendingCheckedCount().subscribe((res) => {
      this.pendingCheckedModel = res.data;
    });
  }

  getPending() {
    this.loading = true;
    this.receivePaymentRequest.receivePaymentStatus = 1;
    this.getReceivePayments(this.receivePaymentRequest);
  }

  getChecked() {
    this.loading = true;
    this.receivePaymentRequest.receivePaymentStatus = 2;
    this.getReceivePayments(this.receivePaymentRequest);
  }

  remove(id: string): void {
    this.receivePaymentService.deleteReceivePayment(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getReceivePayments(this.receivePaymentRequest);
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
      data.message
    );
  }

  reload() {
    this.searchForm.reset();
    this.loading = true;
    this.receivePaymentRequest = new ReceivePaymentSearchRequestDTO();
    this.getReceivePayments(this.receivePaymentRequest);
  }

  onSearch() {
    this.loading = true;
    this.receivePaymentRequest = {
      ...this.receivePaymentRequest,
      ...this.searchForm.value,
    };
    console.log(this.receivePaymentRequest);
    this.getReceivePayments(this.receivePaymentRequest);
  }

  navigateToAddNew() {
    this.router.navigate([this.path.addNew]);
  }

  navigateToView(item) {
    this.router.navigate([this.path.edit, item.id]);
    // this.localStorageService.setItem(item.id, item);
  }

  onPageChange(pageEvent) {
    this.receivePaymentRequest.page = pageEvent.pageIndex;
    this.receivePaymentRequest.rowsPerPage = pageEvent.pageSize;
    this.getReceivePayments(this.receivePaymentRequest);
  }

  openPopup(id: string) {
    this.http
      .get(
        environment.apiURL + "/ReceivePayment/single_file_view_download/" + id,
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

  printPdf(id) {
    this.http
      .get(environment.apiURL + "/ReportReceivePayment/" + id, {
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

  printReceivePaymentPdf(name: string) {
    this.http
      .post(
        environment.apiURL + "/ReceivePayment/" + name,
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

  trackBy(index: number, item: any) {
    return item?.id;
  }
}
