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
import { JournalEntryStatus } from "app/shared/enums/journalEntryStatus";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { CostCenter } from "app/views/configuration/models/cost-center/cost-center.model";
import { AccountService } from "app/views/configuration/services/account.service";
import { CostCenterService } from "app/views/configuration/services/cost-center.service";
import { PendingCheckedCount } from "app/views/purchase/models/pending-checked-count";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { Observable, merge, of } from "rxjs";
import { JournalEntryResponseDTO } from "../../models/journal-entry/journal-entry-response-dto.model";
import { JournalEntrySearchRequestDTO } from "../../models/journal-entry/journal-entry-search-request-dto.model";
import { JournalEntryService } from "../../services/journal-entry.service";

@Component({
  selector: "app-journal-entry",
  templateUrl: "./journal-entry.component.html",
  styleUrls: ["./journal-entry.component.scss"],
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
export class JournalEntryComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  viewColumnForm: FormGroup;
  expandedElement: any;
  dataSource: MatTableDataSource<JournalEntryResponseDTO>;
  totalCount: number;
  costCenters: CostCenter[];
  journalEntryRequest = new JournalEntrySearchRequestDTO();
  journalEntryStatuses: ENUM[];
  pendingCheckedModel: PendingCheckedCount;
  displayedColumns$: Observable<string[]>;
  currentFinancialYearId: string;
  private path = {
    addNew: "accounts/journal-entry/add-new",
    edit: "accounts/journal-entry",
    view: "accounts/journal-entry/view",
  };

  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  //  * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "expand", label: "Expand" },
    { def: "actions", label: "Actions" },
    { def: "voucherNo", label: "Voucher No." },
    { def: "voucherDate", label: "Voucher Date" },
    { def: "costCenterId", label: "Cost Center" },
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
    private journalEntryService: JournalEntryService,
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
    this.getJournalEntries(this.journalEntryRequest);
    this.getCurrentFinancialYearId();
    this.subscribeToFormChanges();
  }

  openExpansionPanel() {
    this.panelOpenState = true;
    this.getAllJournalEntryStatuses();
    this.getAllCostCenters();
    // this.getAllFormAccounts();
  }

  private initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
      fromDate: [null],
      toDate: [null],
      voucherNo: [""],
      costCenterId: [null],
      journalEntryStatus: [null],
    });
    this.viewColumnForm = this.fb.group({
      expand: [true],
      voucherNo: [true],
      voucherDate: [true],
      costCenterId: [true],
      // referenceNo: [true],
      // journalType: [true],
      // cashBankAccountId: [true],
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

  getAllJournalEntryStatuses() {
    this.enumValueService.getJournalEntryStatuses().subscribe((res) => {
      this.journalEntryStatuses = res;
    });
  }

  getJournalEntryStatus(value: number) {
    return this.statusColorService.getJournalEntryStatus(value);
  }

  getJournalEntryStatusName(value: number) {
    return JournalEntryStatus[value];
  }

  getJournalEntries(requestBody: JournalEntrySearchRequestDTO): void {
    this.journalEntryService.getJournalEntries(requestBody).subscribe((res) => {
      this.dataSource = new MatTableDataSource<JournalEntryResponseDTO>(
        res?.data?.item1
      );
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  getPendingCheckedCount(): void {
    this.journalEntryService.getPendingCheckedCount().subscribe((res) => {
      this.pendingCheckedModel = res.data;
    });
  }

  getPending() {
    this.loading = true;
    this.journalEntryRequest.journalEntryStatus = 1;
    this.getJournalEntries(this.journalEntryRequest);
  }

  getChecked() {
    this.loading = true;
    this.journalEntryRequest.journalEntryStatus = 2;
    this.getJournalEntries(this.journalEntryRequest);
  }

  remove(id: string): void {
    this.journalEntryService.deleteJournalEntry(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getJournalEntries(this.journalEntryRequest);
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
    this.journalEntryRequest = new JournalEntrySearchRequestDTO();
    this.getJournalEntries(this.journalEntryRequest);
  }

  onSearch() {
    this.loading = true;
    this.journalEntryRequest = {
      ...this.journalEntryRequest,
      ...this.searchForm.value,
    };
    this.getJournalEntries(this.journalEntryRequest);
  }

  navigateToAddNew() {
    this.router.navigate([this.path.addNew]);
  }

  navigateToView(item) {
    this.router.navigate([this.path.edit, item.id]);
    // this.localStorageService.setItem(item.id, item);
  }

  onPageChange(pageEvent) {
    this.journalEntryRequest.page = pageEvent.pageIndex;
    this.journalEntryRequest.rowsPerPage = pageEvent.pageSize;
    this.getJournalEntries(this.journalEntryRequest);
  }

  printPdf(id) {
    this.http
      .get(environment.apiURL + "/ReportJournalEntry/" + id, {
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
