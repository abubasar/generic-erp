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
import { LCCostEntryStatus } from "app/shared/enums/lcCostEntryStatus";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { CostCenter } from "app/views/configuration/models/cost-center/cost-center.model";
import { CostCenterService } from "app/views/configuration/services/cost-center.service";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { Observable, merge, of } from "rxjs";
import { LCCostEntryAggregatorModel } from "../../models/lc-cost-entry/lc-cost-entry-aggregator.model";
import { LCCostEntryResponseDTO } from "../../models/lc-cost-entry/lc-cost-entry-response-dto.model";
import { LCCostEntrySearchRequestDTO } from "../../models/lc-cost-entry/lc-cost-entry-search-request-dto.model";
import { PendingCheckedCount } from "../../models/pending-checked-count";
import { LcCostEntryService } from "../../services/lc-cost-entry.service";

@Component({
  selector: "app-lc-cost-entry",
  templateUrl: "./lc-cost-entry.component.html",
  styleUrls: ["./lc-cost-entry.component.scss"],
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
export class LcCostEntryComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  viewColumnForm: FormGroup;
  expandedElement: any;
  dataSource: MatTableDataSource<LCCostEntryResponseDTO>;
  reportAggregator = new LCCostEntryAggregatorModel();
  totalCount: number;
  costCenters: CostCenter[];
  lcCostEntryRequest = new LCCostEntrySearchRequestDTO();
  lcCostEntryStatuses: ENUM[];
  pendingCheckedModel: PendingCheckedCount;
  displayedColumns$: Observable<string[]>;
  currentFinancialYearId: string;

  private path = {
    addNew: "purchase/lc-cost-entry/add-new",
    edit: "purchase/lc-cost-entry",
    view: "purchase/lc-cost-entry/view",
  };

  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  //  * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "expand", label: "Expand" },
    { def: "actions", label: "Actions" },
    { def: "code", label: "Code" },
    { def: "ponumber", label: "PO Number" },
    { def: "lcNumber", label: "LC Number" },
    { def: "entryDate", label: "Entry Date" },
    { def: "costCenterId", label: "Cost Center" },
    { def: "total", label: "Total" },
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
    private lcCostEntryService: LcCostEntryService,
    private costCenterService: CostCenterService,
    private enumValueService: EnumValueService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private router: Router,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.getPendingCheckedCount();
    this.getLCCostEntries(this.lcCostEntryRequest);
    this.getCurrentFinancialYearId();
    this.subscribeToFormChanges();
  }

  openExpansionPanel() {
    this.panelOpenState = true;
    this.getAllLCCostEntryStatuses();
    this.getAllCostCenters();
  }

  private initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
      fromDate: [null],
      toDate: [null],
      ponumber: [""],
      lcNumber: [""],
      costCenterId: [null],
      lcCostEntryStatus: [null],
    });
    /**
     * * this code will be executed for instant changes of SearchForm's field
     */
    this.searchForm.valueChanges.subscribe((value) => {
      // Convert empty string to null
      if (value.costCenterId === "") {
        this.searchForm
          .get("costCenterId")
          ?.patchValue(null, { emitEvent: false });
      }
    });

    this.viewColumnForm = this.fb.group({
      expand: [true],
      code: [true],
      ponumber: [true],
      lcNumber: [true],
      entryDate: [true],
      costCenterId: [true],
      total: [true],
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

  getAllLCCostEntryStatuses() {
    this.enumValueService.getLCCostEntryStatuses().subscribe((res) => {
      this.lcCostEntryStatuses = res;
    });
  }

  getLCCostEntryStatus(value: number) {
    return this.statusColorService.getLCCostEntryStatus(value);
  }

  getLCCostEntryStatusName(value: number) {
    return LCCostEntryStatus[value];
  }

  getLCCostEntries(requestBody: LCCostEntrySearchRequestDTO): void {
    this.lcCostEntryService.getLCCostEntries(requestBody).subscribe((res) => {
      this.dataSource = new MatTableDataSource<LCCostEntryResponseDTO>(
        res?.data?.item1
      );
      this.totalCount = res?.data?.item2;
      this.reportAggregates(requestBody);
      this.loading = false;
    });
  }

  reportAggregates(requestBody: LCCostEntrySearchRequestDTO): void {
    this.lcCostEntryService.reportAggregates(requestBody).subscribe((res) => {
      this.reportAggregator = res.data;
    });
  }

  getPendingCheckedCount(): void {
    this.lcCostEntryService.getPendingCheckedCount().subscribe((res) => {
      this.pendingCheckedModel = res.data;
    });
  }

  getPending() {
    this.loading = true;
    this.lcCostEntryRequest.lcCostEntryStatus = 1;
    this.getLCCostEntries(this.lcCostEntryRequest);
  }

  getChecked() {
    this.loading = true;
    this.lcCostEntryRequest.lcCostEntryStatus = 2;
    this.getLCCostEntries(this.lcCostEntryRequest);
  }

  remove(id: string): void {
    this.lcCostEntryService.deleteLCCostEntry(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getLCCostEntries(this.lcCostEntryRequest);
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
    this.lcCostEntryRequest = new LCCostEntrySearchRequestDTO();
    this.getLCCostEntries(this.lcCostEntryRequest);
  }

  onSearch() {
    this.loading = true;
    this.lcCostEntryRequest = {
      ...this.lcCostEntryRequest,
      ...this.searchForm.value,
    };
    this.getLCCostEntries(this.lcCostEntryRequest);
  }

  navigateToAddNew() {
    this.router.navigate([this.path.addNew]);
  }

  navigateToView(item) {
    this.router.navigate([this.path.edit, item.id]);
    // this.localStorageService.setItem(item.id, item);
  }

  onPageChange(pageEvent) {
    this.lcCostEntryRequest.page = pageEvent.pageIndex;
    this.lcCostEntryRequest.rowsPerPage = pageEvent.pageSize;
    this.getLCCostEntries(this.lcCostEntryRequest);
  }

  printPdf(id) {
    this.http
      .get(environment.apiURL + "/ReportLcCostEntry/" + id, {
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

  printLCCostEntryPdf(name: string) {
    this.http
      .post(
        environment.apiURL + "/LCCostEntry/" + name,
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
