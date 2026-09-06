import { HttpClient } from "@angular/common/http";
import { Component, OnInit, ViewChild } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { MatDialog } from "@angular/material/dialog";
import { MatMenuTrigger } from "@angular/material/menu";
import { MatTableDataSource } from "@angular/material/table";
import { Router } from "@angular/router";
import { FundTransferStatus } from "app/shared/enums/fundTransferStatus";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { CostCenter } from "app/views/configuration/models/cost-center/cost-center.model";
import { AccountService } from "app/views/configuration/services/account.service";
import { CostCenterService } from "app/views/configuration/services/cost-center.service";
import { PendingCheckedCount } from "app/views/purchase/models/pending-checked-count";
import { ToastrService } from "ngx-toastr";
import { Observable, merge, of } from "rxjs";
import { FundTransferResponseDTO } from "../../models/fund-transfer/fund-transfer-response-dto.model";
import { FundTransferSearchRequestDTO } from "../../models/fund-transfer/fund-transfer-search-request-dto.model";
import { FundTransferService } from "../../services/fund-transfer.service";

import { Control_Accounts_Parent_Id } from "app/shared/consts/const";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ControlAccount } from "app/views/configuration/models/account/control-account.model";
import { environment } from "environments/environment";
import { FundTransferAggregatorModel } from "../../models/fund-transfer/fund-transfer-aggregator.model";

@Component({
  selector: "app-fund-transfer",
  templateUrl: "./fund-transfer.component.html",
  styleUrls: ["./fund-transfer.component.scss"],
})
export class FundTransferComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  viewColumnForm: FormGroup;
  expandedElement: any;
  dataSource: MatTableDataSource<FundTransferResponseDTO>;
  reportAggregator = new FundTransferAggregatorModel();
  totalCount: number;
  costCenters: CostCenter[];
  cashBankAccounts: ControlAccount[];
  filteredCashBankAccounts: ControlAccount[];
  fundTransferRequest = new FundTransferSearchRequestDTO();
  fundTransferStatuses: ENUM[];
  pendingCheckedModel: PendingCheckedCount;
  displayedColumns$: Observable<string[]>;
  currentFinancialYearId: string;

  private path = {
    addNew: "accounts/fund-transfer/add-new",
    edit: "accounts/fund-transfer",
    view: "accounts/fund-transfer/view",
  };

  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  //  * Control column ordering and which columns are displayed.
  columnDefinitions = [
    // { def: "expand", label: "Expand" },
    { def: "actions", label: "Actions" },
    { def: "fundTransferNo", label: "Fund Transfer No." },
    { def: "fundTransferDate", label: "Fund Transfer Date" },
    { def: "fundTransferTransactionTypeId", label: "Transaction Type" },
    { def: "costCenterId", label: "Cost Center" },
    { def: "transferFromAccountId", label: "Transfer From Account" },
    { def: "transferToAccountId", label: "Transfer To Account" },
    { def: "amount", label: "Amount" },
    { def: "charges", label: "Charges" },
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
    private fundTransferService: FundTransferService,
    private accountService: AccountService,
    private costCenterService: CostCenterService,
    private enumValueService: EnumValueService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private router: Router,
    private http: HttpClient,
    // private localStorageService: LocalStoreService,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.getPendingCheckedCount();
    this.getFundTransfers(this.fundTransferRequest);
    this.getCurrentFinancialYearId();
    this.subscribeToFormChanges();
  }

  openExpansionPanel() {
    this.panelOpenState = true;
    this.getAllFundTransferStatuses();
    this.getAllCostCenters();
    this.getAllCashBankAccounts(Control_Accounts_Parent_Id);
  }

  private initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
      fromDate: [null],
      toDate: [null],
      costCenterId: [null],
      transferFromAccountId: [null],
      transferToAccountId: [null],
      fundTransferStatus: [null],
    });
    /**
     * * this code will be executed for instant changes of SearchForm's field
     */
    this.searchForm.valueChanges.subscribe((value) => {
      // Convert empty string to null
      if (value.transferFromAccountId === "") {
        this.searchForm
          .get("transferFromAccountId")
          ?.patchValue(null, { emitEvent: false });
      }
      if (value.transferToAccountId === "") {
        this.searchForm
          .get("transferToAccountId")
          ?.patchValue(null, { emitEvent: false });
      }
    });
    this.viewColumnForm = this.fb.group({
      // expand: [true],
      fundTransferNo: [true],
      fundTransferDate: [true],
      fundTransferTransactionTypeId: [true],
      costCenterId: [true],
      transferFromAccountId: [true],
      transferToAccountId: [true],
      amount: [true],
      charges: [true],
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

  cashBankLoading: boolean = false;
  getAllCashBankAccounts(parentId) {
    this.cashBankLoading = true;
    this.accountService
      .getControlAccountsByParentId(parentId)
      .subscribe((res) => {
        this.filteredCashBankAccounts = this.cashBankAccounts = res?.data;
        this.cashBankLoading = false;
      });
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    if (fieldName === "transferFromAccountId")
      this.searchForm?.get("transferFromAccountId").setValue(null);
    if (fieldName === "transferToAccountId")
      this.searchForm?.get("transferToAccountId").setValue(null);
  }

  handleCashBankFromAccountSearch(event: any): void {
    const name = event.target?.name;
    if (name === "transferFromAccountId") {
      const term = this.searchForm.get("transferFromAccountId");
      this.filterCashBankAccount(term.value || "");
    }
  }

  handleCashBankToAccountSearch(event: any): void {
    const name = event.target?.name;
    if (name === "transferToAccountId") {
      const term = this.searchForm.get("transferToAccountId");
      this.filterCashBankAccount(term.value || "");
    }
  }

  private filterCashBankAccount(value: string) {
    const filterValue = value.toLowerCase();
    this.filteredCashBankAccounts = this.cashBankAccounts?.filter((option) =>
      option.name.toLowerCase().includes(filterValue)
    );
  }

  getCashBankFromAccountName(toAccountId: string) {
    return this.cashBankAccounts?.find((x) => x.id === toAccountId)?.name;
  }

  getCashBankToAccountName(toAccountId: string) {
    return this.cashBankAccounts?.find((x) => x.id === toAccountId)?.name;
  }

  getAllFundTransferStatuses() {
    this.enumValueService.getFundTransferStatuses().subscribe((res) => {
      this.fundTransferStatuses = res;
    });
  }

  getFundTransferStatus(value: number) {
    return this.statusColorService.getFundTransferStatus(value);
  }

  getFundTransferStatusName(value: number) {
    return FundTransferStatus[value];
  }

  getFundTransfers(requestBody: FundTransferSearchRequestDTO): void {
    this.fundTransferService.getFundTransfers(requestBody).subscribe((res) => {
      this.dataSource = new MatTableDataSource<FundTransferResponseDTO>(
        res?.data?.item1
      );
      this.totalCount = res?.data?.item2;
      this.reportAggregates(requestBody);
      this.loading = false;
    });
  }

  reportAggregates(requestBody: FundTransferSearchRequestDTO): void {
    this.fundTransferService.reportAggregates(requestBody).subscribe((res) => {
      this.reportAggregator = res.data;
    });
  }

  getPendingCheckedCount(): void {
    this.fundTransferService.getPendingCheckedCount().subscribe((res) => {
      this.pendingCheckedModel = res.data;
    });
  }

  getPending() {
    this.loading = true;
    this.fundTransferRequest.fundTransferStatus = 1;
    this.getFundTransfers(this.fundTransferRequest);
  }

  getChecked() {
    this.loading = true;
    this.fundTransferRequest.fundTransferStatus = 2;
    this.getFundTransfers(this.fundTransferRequest);
  }

  remove(id: string): void {
    this.fundTransferService.deleteFundTransfer(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getFundTransfers(this.fundTransferRequest);
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
    this.fundTransferRequest = new FundTransferSearchRequestDTO();
    this.getFundTransfers(this.fundTransferRequest);
  }

  onSearch() {
    this.loading = true;
    this.fundTransferRequest = {
      ...this.fundTransferRequest,
      ...this.searchForm.value,
    };
    this.getFundTransfers(this.fundTransferRequest);
  }

  navigateToAddNew() {
    this.router.navigate([this.path.addNew]);
  }

  navigateToView(item) {
    this.router.navigate([this.path.edit, item.id]);
    // this.localStorageService.setItem(item.id, item);
  }

  onPageChange(pageEvent) {
    this.fundTransferRequest.page = pageEvent.pageIndex;
    this.fundTransferRequest.rowsPerPage = pageEvent.pageSize;
    this.getFundTransfers(this.fundTransferRequest);
  }

  printPdf(id) {
    this.http
      .get(environment.apiURL + "/ReportFundTransfer/" + id, {
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
  printFundTransferPdf(name: string) {
    this.http
      .post(
        environment.apiURL + "/FundTransfer/" + name,
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
