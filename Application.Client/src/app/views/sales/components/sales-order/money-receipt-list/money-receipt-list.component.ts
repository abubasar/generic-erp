import {
  animate,
  state,
  style,
  transition,
  trigger,
} from "@angular/animations";
import { SelectionModel } from "@angular/cdk/collections";
import { Component, Inject, OnInit, ViewChild } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { MAT_DIALOG_DATA, MatDialogRef } from "@angular/material/dialog";
import { MatMenuTrigger } from "@angular/material/menu";
import { MatTableDataSource } from "@angular/material/table";
import { Router } from "@angular/router";
import { ReceivePaymentStatus } from "app/shared/enums/receivePaymentStatus";
import { ENUM } from "app/shared/models/enum-value/enum.model";
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
import { ReceivePaymentResponseDTO } from "app/views/sales/models/receive-payment/receive-payment-response-dto.model";
import { ReceivePaymentSearchRequestDTO } from "app/views/sales/models/receive-payment/receive-payment-search-request-dto.model";
import { ReceivePaymentService } from "app/views/sales/services/receive-payment.service";
import { ToastrService } from "ngx-toastr";
import { Observable, merge, of } from "rxjs";

@Component({
  selector: "app-money-receipt-list",
  templateUrl: "./money-receipt-list.component.html",
  styleUrls: ["./money-receipt-list.component.scss"],
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
export class MoneyReceiptListComponent implements OnInit {
  loading: boolean = true;
  selection = new SelectionModel<ReceivePaymentResponseDTO>(true, []); // its need for multiple selection
  receivePayments: ReceivePaymentResponseDTO[];
  panelOpenState: boolean;
  searchForm: FormGroup;
  viewColumnForm: FormGroup;
  expandedElement: any;
  dataSource: MatTableDataSource<ReceivePaymentResponseDTO>;
  totalCount: number;
  customers: Customer[];
  costCenters: CostCenter[];
  paymentModes: PaymentMode[];
  receivePaymentRequest = new ReceivePaymentSearchRequestDTO();
  receivePaymentStatuses: ENUM[];
  pendingCheckedModel: PendingCheckedCount;
  displayedColumns$: Observable<string[]>;
  private path = {
    addNew: "sales/money-receipt/add-new",
    edit: "sales/money-receipt",
    view: "sales/money-receipt/view",
  };

  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  //  * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "select", label: "Select" }, // its need for multiple selection
    { def: "expand", label: "Expand" },
    { def: "code", label: "Code" },
    { def: "paymentDate", label: "Payment Date" },
    { def: "customerId", label: "Customer" },
    { def: "costCenterId", label: "Cost Center" },
    { def: "paymentModeId", label: "Payment Mode" },
    { def: "totalAmount", label: "Total Amount" },
    { def: "remark", label: "Remark" },
    { def: "status", label: "Status" },
  ];

  constructor(
    public dialogRef: MatDialogRef<MoneyReceiptListComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    public dateFormatService: DateTimeFormatService,
    public statusColorService: StatusColorService,
    private receivePaymentService: ReceivePaymentService,
    private customerService: CustomerService,
    private costCenterService: CostCenterService,
    private paymentModeService: PaymentModeService,
    private accountService: AccountService,
    private enumValueService: EnumValueService,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private router: Router,
    private confirmDialogService: ConfirmDialogService // private localStorageService: LocalStoreService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.getAllReceivePaymentStatuses();
    this.getAllCustomers();
    this.getAllCostCenters();
    this.getAllPaymentModes();
    this.getReceivePayments();
    this.subscribeToFormChanges();
  }

  private initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [null],
    });
    this.viewColumnForm = this.fb.group({
      select: [true],
      expand: [true],
      code: [true],
      paymentDate: [true],
      customerId: [true],
      costCenterId: [true],
      paymentModeId: [true],
      totalAmount: [true],
      status: [true],
      remark: [true],
    });
    this.updateDisplayedColumns();
  }
  getMoneyReceiptNo(keyword): void {
    this.receivePaymentRequest.receivePaymentStatuses = [3, 4];
    this.receivePaymentRequest.keyword = keyword;
    this.receivePaymentRequest.customerId = this.data;
    this.receivePaymentService
      .getReceivePayments(this.receivePaymentRequest)
      .subscribe((res) => {
        this.receivePayments = res?.data?.item1;
      });
  }

  onMoneyReceiptNoChange(): void {
    const term = this.searchForm.get("keyword");
    this._filterProduct(term.value || "");
  }

  private _filterProduct(value: string) {
    const filterValue = value.toLowerCase().trim();
    this.getMoneyReceiptNo(filterValue);
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
  getAllCustomers() {
    this.customerService.getAllCustomers().subscribe((res) => {
      this.customers = res.data?.item1;
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

  getReceivePayments(): void {
    this.receivePaymentRequest.customerId = this.data;
    this.receivePaymentRequest.receivePaymentStatuses = [3,4];
    this.receivePaymentService
      .getReceivePayments(this.receivePaymentRequest)
      .subscribe((res) => {
        console.log(res);
        this.dataSource = new MatTableDataSource<ReceivePaymentResponseDTO>(
          res?.data?.item1
        );
        this.totalCount = res?.data?.item2;
        this.loading = false;
      });
  }

  onSearch() {
    this.loading = true;
    this.getReceivePayments();
  }

  onPageChange(pageEvent) {
    this.receivePaymentRequest.page = pageEvent.pageIndex;
    this.receivePaymentRequest.rowsPerPage = pageEvent.pageSize;
    this.getReceivePayments();
  }
}
