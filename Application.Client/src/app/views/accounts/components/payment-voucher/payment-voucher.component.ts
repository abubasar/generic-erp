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
import { Control_Accounts_Parent_Id } from "app/shared/consts/const";
import { PaymentVoucherStatus } from "app/shared/enums/paymentVoucherStatus";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { CostCenter } from "app/views/configuration/models/cost-center/cost-center.model";
import { PaymentMode } from "app/views/configuration/models/payment-mode/payment-mode.model";
import { AccountService } from "app/views/configuration/services/account.service";
import { CostCenterService } from "app/views/configuration/services/cost-center.service";
import { PaymentModeService } from "app/views/configuration/services/payment-mode.service";
import { PendingCheckedCount } from "app/views/purchase/models/pending-checked-count";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { Observable, merge, of } from "rxjs";
import { PaymentVoucherAggregatorModel } from "../../models/payment-voucher/payment-voucher-aggregator.model";
import { PaymentVoucherResponseDTO } from "../../models/payment-voucher/payment-voucher-response-dto.model";
import { PaymentVoucherSearchRequestDTO } from "../../models/payment-voucher/payment-voucher-search-request-dto.model";
import { PaymentVoucherService } from "../../services/payment-voucher.service";

@Component({
  selector: "app-payment-voucher",
  templateUrl: "./payment-voucher.component.html",
  styleUrls: ["./payment-voucher.component.scss"],
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
export class PaymentVoucherComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  viewColumnForm: FormGroup;
  expandedElement: any;
  dataSource: MatTableDataSource<PaymentVoucherResponseDTO>;
  reportAggregator = new PaymentVoucherAggregatorModel();
  totalCount: number;
  costCenters: CostCenter[];
  paymentModes: PaymentMode[];
  // voucherTypes: ENUM[];
  paymentVoucherRequest = new PaymentVoucherSearchRequestDTO();
  paymentVoucherStatuses: ENUM[];
  pendingCheckedModel: PendingCheckedCount;
  displayedColumns$: Observable<string[]>;
  currentFinancialYearId: string;
  private path = {
    addNew: "accounts/cash-payment-voucher/add-new",
    edit: "accounts/cash-payment-voucher",
    view: "accounts/cash-payment-voucher/view",
  };

  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;
  //  * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "expand", label: "Expand" },
    { def: "actions", label: "Actions" },
    { def: "voucherNo", label: "Payment Voucher No." },
    { def: "voucherDate", label: "Voucher Date" },
    { def: "fundTransferTransactionTypeId", label: "Transaction Type" },
    { def: "transactionNumber", label: "Transaction No" },
    { def: "costCenterId", label: "Cost Center" },
    { def: "paymentModeId", label: "Payment Mode" },
    { def: "referenceNo", label: "Reference No" },
    // { def: "voucherType", label: "Voucher Type" },
    { def: "cashBankAccountId", label: "Cash/Bank Account" },
    { def: "totalAmount", label: "Total Amount" },
    { def: "status", label: "Status" },
    { def: "remark", label: "Remark" },
    { def: "checkedBy", label: "Checked By" },
    { def: "approvedBy", label: "Approved By" },
    { def: "createdOn", label: "Created On" },
    { def: "updatedOn", label: "Updated On" },
    { def: "createdBy", label: "Created By" },
    { def: "updatedBy", label: "Updated By" },
  ];

  constructor(
    public dateFormatService: DateTimeFormatService,
    private jwtAuth: JwtAuthService,
    public statusColorService: StatusColorService,
    private paymentVoucherService: PaymentVoucherService,
    private costCenterService: CostCenterService,
    private paymentModeService: PaymentModeService,
    private accountService: AccountService,
    private enumValueService: EnumValueService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private router: Router,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService // private localStorageService: LocalStoreService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.getAllCashBankAccounts();
    this.getPendingCheckedCount();
    this.getPaymentVouchers();
    this.getCurrentFinancialYearId();
    this.subscribeToFormChanges();
  }

  openExpansionPanel() {
    this.panelOpenState = true;
    this.getAllPaymentVoucherStatuses();
    this.getAllCostCenters();
    this.getAllPaymentModes();
    // this.getVoucherTypes();
  }

  private initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
      fromDate: [this.dateFormatService.getPresentDate()],
      toDate: [this.dateFormatService.getPresentDate()],
      voucherNo: [""],
      transactionNumber: [""],
      // voucherType: [null],
      costCenterId: [null],
      paymentModeId: [null],
      paymentVoucherStatus: [null],
      accountTransactionType: [null],
    });
    this.viewColumnForm = this.fb.group({
      expand: [true],
      voucherNo: [true],
      voucherDate: [true],
      fundTransferTransactionTypeId: [false],
      transactionNumber: [false],
      costCenterId: [true],
      paymentModeId: [true],
      referenceNo: [false],
      // voucherType: [false],
      cashBankAccountId: [true],
      totalAmount: [true],
      status: [true],
      remark: [false],
      checkedBy: [false],
      approvedBy: [false],
      createdOn: [false],
      updatedOn: [false],
      createdBy: [false],
      updatedBy: [false],
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
  expandRow(element) {
    if (this.expandedElement == element) {
      this.expandedElement = null;
    } else {
      this.expandedElement = element;
    }
    // this.expandedElement = this.expandedElement === element ? null : element;
  }

  expandAll() {
    if (this.collapsed === "collapsed") this.collapsed = "expanded";
    else this.collapsed = "collapsed";
  }

  private getCurrentFinancialYearId() {
    this.jwtAuth.userProfile.subscribe((res: UserProfile) => {
      this.currentFinancialYearId = res.fyid;
    });
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

  cashBankAccounts: any[];
  getAllCashBankAccounts() {
    this.accountService
      .getControlAccountsByParentId(Control_Accounts_Parent_Id)
      .subscribe((res) => {
        this.cashBankAccounts = res?.data;
      });
  }

  getCashBankAccountName(cashBankAccountId) {
    return this.cashBankAccounts?.find((x) => x?.id === cashBankAccountId)
      ?.name;
  }

  getAllPaymentVoucherStatuses() {
    this.enumValueService.getPaymentVoucherStatuses().subscribe((res) => {
      this.paymentVoucherStatuses = res;
    });
  }

  getPaymentVoucherStatus(value: number) {
    return this.statusColorService.getPaymentVoucherStatus(value);
  }

  getPaymentVoucherStatusName(value: number) {
    return PaymentVoucherStatus[value];
  }

  // getVoucherTypeName(value: string) {
  //   if (value == "1") return "Cash_Payment_Voucher";
  //   else return "Cash_Received_Voucher";
  // }
  // getVoucherTypes() {
  //   return this.enumValueService.getVoucherTypes().subscribe((res) => {
  //     this.voucherTypes = res;
  //   });
  // }

  getPaymentVouchers(): void {
    this.paymentVoucherRequest.fromDate = this.searchForm.value.fromDate;
    this.paymentVoucherRequest.toDate = this.searchForm.value.toDate;
    this.paymentVoucherService
      .getPaymentVouchers(this.paymentVoucherRequest)
      .subscribe((res) => {
        this.dataSource = new MatTableDataSource<PaymentVoucherResponseDTO>(
          res?.data?.item1
        );
        this.reportAggregates(this.paymentVoucherRequest);
        this.totalCount = res?.data?.item2;
        this.loading = false;
      });
  }

  reportAggregates(requestBody: PaymentVoucherSearchRequestDTO): void {
    this.paymentVoucherService
      .reportAggregates(requestBody)
      .subscribe((res) => {
        this.reportAggregator = res.data;
      });
  }

  sortData(sort: Sort) {
    this.paymentVoucherRequest.orderBy = sort.active;
    let isAsc = sort.direction == "asc";
    if (isAsc) this.paymentVoucherRequest.isAscending = true;
    else this.paymentVoucherRequest.isAscending = false;
    this.getPaymentVouchers();
  }

  getPendingCheckedCount(): void {
    this.paymentVoucherService.getPendingCheckedCount().subscribe((res) => {
      this.pendingCheckedModel = res.data;
    });
  }

  getPending() {
    this.loading = true;
    this.paymentVoucherRequest.paymentVoucherStatus = 1;
    this.getPaymentVouchers();
  }

  getChecked() {
    this.loading = true;
    this.paymentVoucherRequest.paymentVoucherStatus = 2;
    this.getPaymentVouchers();
  }

  remove(id: string): void {
    this.paymentVoucherService.deletePaymentVoucher(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getPaymentVouchers();
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
    this.getPaymentVouchers();
  }

  onSearch() {
    this.loading = true;
    this.paymentVoucherRequest = {
      ...this.paymentVoucherRequest,
      ...this.searchForm.value,
    };
    this.getPaymentVouchers();
  }

  navigateToAddNew() {
    this.router.navigate([this.path.addNew]);
  }

  navigateToView(item) {
    this.router.navigate([this.path.edit, item.id]);
    // this.localStorageService.setItem(item.id, item);
  }

  onPageChange(pageEvent) {
    this.paymentVoucherRequest.page = pageEvent.pageIndex;
    this.paymentVoucherRequest.rowsPerPage = pageEvent.pageSize;
    this.getPaymentVouchers();
  }

  printPdf(id) {
    this.http
      .get(environment.apiURL + "/ReportPaymentVoucher/" + id, {
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
  trackBy(index: number, item: any) {
    return item?.id;
  }
}
