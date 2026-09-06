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
import { Page_Size_Options } from "app/shared/consts/const";
import { SupplierPaymentStatus } from "app/shared/enums/supplierPaymentStatus";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { LocalStoreService } from "app/shared/services/local-store.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { CostCenter } from "app/views/configuration/models/cost-center/cost-center.model";
import { PaymentMode } from "app/views/configuration/models/payment-mode/payment-mode.model";
import { Supplier } from "app/views/configuration/models/supplier/supplier.model";
import { AccountService } from "app/views/configuration/services/account.service";
import { CostCenterService } from "app/views/configuration/services/cost-center.service";
import { PaymentModeService } from "app/views/configuration/services/payment-mode.service";
import { SupplierService } from "app/views/configuration/services/supplier.service";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { Observable, merge, of } from "rxjs";
import { PendingCheckedCount } from "../../models/pending-checked-count";
import { SupplierPaymentAggregatorModel } from "../../models/supplier-payment/supplier-payment-aggregator.model";
import { SupplierPaymentResponseDTO } from "../../models/supplier-payment/supplier-payment-response-dto.model";
import { SupplierPaymentSearchRequestDTO } from "../../models/supplier-payment/supplier-payment-search-request-dto.model";
import { SupplierPaymentService } from "../../services/supplier-payment.service";

@Component({
  selector: "app-supplier-payment",
  templateUrl: "./supplier-payment.component.html",
  styleUrls: ["./supplier-payment.component.scss"],
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
export class SupplierPaymentComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  viewColumnForm: FormGroup;
  expandedElement: any;
  dataSource: MatTableDataSource<SupplierPaymentResponseDTO>;
  reportAggregator = new SupplierPaymentAggregatorModel();
  totalCount: number;
  suppliers: Supplier[];
  filterSuppliers: Supplier[];
  costCenters: CostCenter[];
  paymentModes: PaymentMode[];
  supplierPaymentTypes: ENUM[];
  supplierPaymentRequest = new SupplierPaymentSearchRequestDTO();
  supplierPaymentStatuses: ENUM[];
  pendingCheckedModel: PendingCheckedCount;
  displayedColumns$: Observable<string[]>;
  pageSizeOptions: [] = Page_Size_Options;
  currentFinancialYearId: string;
  private path = {
    addNew: "purchase/supplier-payment/add-new",
    edit: "purchase/supplier-payment",
    view: "purchase/supplier-payment/view",
  };

  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  //  * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "expand", label: "Expand" },
    { def: "actions", label: "Actions" },
    { def: "code", label: "Code" },
    { def: "supplierPaymentType", label: "Supplier Payment Type" },
    { def: "ponumber", label: "PO No" },
    { def: "paymentDate", label: "Payment Date" },
    { def: "fundTransferTransactionTypeId", label: "Transaction Type" },
    { def: "transactionNumber", label: "Transaction No" },
    { def: "supplierId", label: "Supplier" },
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
  ];

  constructor(
    public dateFormatService: DateTimeFormatService,
    private jwtAuth: JwtAuthService,
    public statusColorService: StatusColorService,
    private supplierPaymentService: SupplierPaymentService,
    private supplierService: SupplierService,
    private costCenterService: CostCenterService,
    private paymentModeService: PaymentModeService,
    private accountService: AccountService,
    private enumValueService: EnumValueService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private router: Router,
    private http: HttpClient,
    private localStorageService: LocalStoreService,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.getPendingCheckedCount();
    this.getSupplierPayments(this.supplierPaymentRequest);
    this.getCurrentFinancialYearId();
    this.subscribeToFormChanges();
  }

  openExpansionPanel() {
    this.panelOpenState = true;
    this.getAllSupplierPaymentStatuses();
    this.getAllSuppliers();
    this.getAllCostCenters();
    this.getAllPaymentModes();
    this.getSupplierPaymentTypes();
  }

  private initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
      fromDate: [null],
      toDate: [null],
      transactionNumber: [""],
      supplierId: [null],
      costCenterId: [null],
      paymentModeId: [null],
      supplierPaymentStatus: [null],
    });

    /**
     * * this code will be executed for instant changes of SearchForm's field
     */
    this.searchForm.valueChanges.subscribe((value) => {
      // Convert empty string to null
      if (value.supplierId === "") {
        this.searchForm
          .get("supplierId")
          ?.patchValue(null, { emitEvent: false });
      }
    });

    this.viewColumnForm = this.fb.group({
      expand: [true],
      code: [true],
      supplierPaymentType: [true],
      ponumber: [true],
      paymentDate: [true],
      fundTransferTransactionTypeId: [false],
      transactionNumber: [false],
      supplierId: [true],
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

  clearInput(evt: any): void {
    evt.stopPropagation();
    this.searchForm?.get("supplierId").setValue(null);
  }

  handleSupplierSearch(event: any): void {
    const name = event.target?.name;
    if (name === "supplierId") {
      const term = this.searchForm.get("supplierId");
      this.filterSupplier(term.value || "");
    }
  }

  private filterSupplier(value: string) {
    const filterValue = value.toLowerCase();
    this.filterSuppliers = this.suppliers?.filter((option) =>
      option.name.toLowerCase().includes(filterValue)
    );
  }

  private getCurrentFinancialYearId() {
    this.jwtAuth.userProfile.subscribe((res: UserProfile) => {
      this.currentFinancialYearId = res.fyid;
    });
  }

  getSupplierName(supplierId: string) {
    if (!supplierId) {
      return;
    }
    const supplierAccount = this.suppliers?.find(
      (supplier) => supplier?.id === supplierId
    );
    return supplierAccount?.name;
  }

  getAllSuppliers() {
    this.supplierService.getAllSuppliers().subscribe((res) => {
      this.filterSuppliers = this.suppliers = res.data?.item1;
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

  getAllSupplierPaymentStatuses() {
    this.enumValueService.getSupplierPaymentStatuses().subscribe((res) => {
      this.supplierPaymentStatuses = res;
    });
  }

  getSupplierPaymentStatus(value: number) {
    return this.statusColorService.getVoucherEntryStatus(value);
  }

  getSupplierPaymentStatusName(value: number) {
    return SupplierPaymentStatus[value];
  }

  getSupplierPaymentTypes() {
    return this.enumValueService.getSupplierPaymentTypes().subscribe((res) => {
      this.supplierPaymentTypes = res;
    });
  }

  getSupplierPayments(requestBody: SupplierPaymentSearchRequestDTO): void {
    this.supplierPaymentService
      .getSupplierPayments(requestBody)
      .subscribe((res) => {
        this.dataSource = new MatTableDataSource<SupplierPaymentResponseDTO>(
          res?.data?.item1
        );
        this.reportAggregates(requestBody);
        this.totalCount = res?.data?.item2;
        this.loading = false;
      });
  }

  reportAggregates(requestBody: SupplierPaymentSearchRequestDTO): void {
    this.supplierPaymentService
      .reportAggregates(requestBody)
      .subscribe((res) => {
        this.reportAggregator = res.data;
      });
  }

  sortData(sort: Sort) {
    this.supplierPaymentRequest.orderBy = sort.active;
    let isAsc = sort.direction == "asc";
    if (isAsc) this.supplierPaymentRequest.isAscending = true;
    else this.supplierPaymentRequest.isAscending = false;
    this.getSupplierPayments(this.supplierPaymentRequest);
  }

  getPendingCheckedCount(): void {
    this.supplierPaymentService.getPendingCheckedCount().subscribe((res) => {
      this.pendingCheckedModel = res.data;
    });
  }

  getPending() {
    this.loading = true;
    this.supplierPaymentRequest.supplierPaymentStatus = 1;
    this.getSupplierPayments(this.supplierPaymentRequest);
  }

  getChecked() {
    this.loading = true;
    this.supplierPaymentRequest.supplierPaymentStatus = 2;
    this.getSupplierPayments(this.supplierPaymentRequest);
  }

  remove(id: string): void {
    this.supplierPaymentService.deleteSupplierPayment(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getSupplierPayments(this.supplierPaymentRequest);
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
    this.supplierPaymentRequest = new SupplierPaymentSearchRequestDTO();
    this.getSupplierPayments(this.supplierPaymentRequest);
  }

  onSearch() {
    this.loading = true;
    this.supplierPaymentRequest = {
      ...this.supplierPaymentRequest,
      ...this.searchForm.value,
    };
    this.getSupplierPayments(this.supplierPaymentRequest);
  }

  navigateToAddNew() {
    this.router.navigate([this.path.addNew]);
  }

  navigateToView(item) {
    this.router.navigate([this.path.edit, item.id]);
    this.localStorageService.setItem(item.id, item);
  }

  onPageChange(pageEvent) {
    this.supplierPaymentRequest.page = pageEvent.pageIndex;
    this.supplierPaymentRequest.rowsPerPage = pageEvent.pageSize;
    this.getSupplierPayments(this.supplierPaymentRequest);
  }

  printPdf(id) {
    this.http
      .get(environment.apiURL + "/ReportSupplierPayment/" + id, {
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
