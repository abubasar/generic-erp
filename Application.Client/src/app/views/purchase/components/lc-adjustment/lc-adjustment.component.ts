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
import { LcAdjustmentStatus } from "app/shared/enums/lcAdjustmentStatus";
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
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { Observable, merge, of } from "rxjs";
import { LcAdjustmentResponseDTO } from "../../models/lc-adjustment/lc-adjustment-response-dto.model";
import { LcAdjustmentSearchRequestDTO } from "../../models/lc-adjustment/lc-adjustment-search-request-dto.model";
import { PendingCheckedCount } from "../../models/pending-checked-count";
import { LcAdjustmentService } from "../../services/lc-adjustment.service";

@Component({
  selector: "app-lc-adjustment",
  templateUrl: "./lc-adjustment.component.html",
  styleUrls: ["./lc-adjustment.component.scss"],
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
export class LcAdjustmentComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  viewColumnForm: FormGroup;
  expandedElement: any;
  dataSource: MatTableDataSource<LcAdjustmentResponseDTO>;
  totalCount: number;
  costCenters: CostCenter[];
  lcAdjustmentRequest = new LcAdjustmentSearchRequestDTO();
  lcAdjustmentStatuses: ENUM[];
  pendingCheckedModel: PendingCheckedCount;
  displayedColumns$: Observable<string[]>;
  currentFinancialYearId: string;

  private path = {
    addNew: "purchase/lc-adjustment/add-new",
    edit: "purchase/lc-adjustment",
    view: "purchase/lc-adjustment/view",
  };

  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  //  * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "expand", label: "Expand" },
    { def: "actions", label: "Actions" },
    { def: "code", label: "Code" },
    { def: "purchaseInvoiceNo", label: "Purchase Invoice No." },
    { def: "adjustmentDate", label: "Adjustment Date" },
    { def: "costCenterId", label: "Cost Center" },
    { def: "invoiceTotal", label: "Invoice Total" },
    { def: "lcMarginTotal", label: "LC Margin Total" },
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
    public statusColorService: StatusColorService,
    private jwtAuth: JwtAuthService,
    private lcAdjustmentService: LcAdjustmentService,
    private accountService: AccountService,
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
    this.getLcAdjustments(this.lcAdjustmentRequest);
    this.getCurrentFinancialYearId();
    this.subscribeToFormChanges();
  }

  openExpansionPanel() {
    this.panelOpenState = true;
    this.getAllLcAdjustmentStatuses();
    this.getAllCostCenters();
  }

  private initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
      fromDate: [null],
      toDate: [null],
      purchaseInvoiceNo: [""],
      costCenterId: [null],
      lcAdjustmentStatus: [null],
    });

    this.viewColumnForm = this.fb.group({
      expand: [true],
      code: [true],
      purchaseInvoiceNo: [true],
      adjustmentDate: [true],
      costCenterId: [true],
      invoiceTotal: [true],
      lcMarginTotal: [true],
      remark: [false],
      status: [true],
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

  getAllLcAdjustmentStatuses() {
    this.enumValueService.getLcAdjustmentStatuses().subscribe((res) => {
      this.lcAdjustmentStatuses = res;
    });
  }

  getLcAdjustmentStatus(value: number) {
    return this.statusColorService.getLcAdjustmentStatus(value);
  }

  getLcAdjustmentStatusName(value: number) {
    return LcAdjustmentStatus[value];
  }

  getLcAdjustments(requestBody: LcAdjustmentSearchRequestDTO): void {
    this.lcAdjustmentService.getLcAdjustments(requestBody).subscribe((res) => {
      this.dataSource = new MatTableDataSource<LcAdjustmentResponseDTO>(
        res?.data?.item1
      );
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  getPendingCheckedCount(): void {
    this.lcAdjustmentService.getPendingCheckedCount().subscribe((res) => {
      this.pendingCheckedModel = res.data;
    });
  }

  getPending() {
    this.loading = true;
    this.lcAdjustmentRequest.lcAdjustmentStatus = 1;
    this.getLcAdjustments(this.lcAdjustmentRequest);
  }

  getChecked() {
    this.loading = true;
    this.lcAdjustmentRequest.lcAdjustmentStatus = 2;
    this.getLcAdjustments(this.lcAdjustmentRequest);
  }

  remove(id: string): void {
    this.lcAdjustmentService.deleteLcAdjustment(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getLcAdjustments(this.lcAdjustmentRequest);
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
    this.lcAdjustmentRequest = new LcAdjustmentSearchRequestDTO();
    this.getLcAdjustments(this.lcAdjustmentRequest);
  }

  onSearch() {
    this.loading = true;
    this.lcAdjustmentRequest = {
      ...this.lcAdjustmentRequest,
      ...this.searchForm.value,
    };
    this.getLcAdjustments(this.lcAdjustmentRequest);
  }

  navigateToAddNew() {
    this.router.navigate([this.path.addNew]);
  }

  navigateToView(item) {
    this.router.navigate([this.path.edit, item.id]);
  }

  onPageChange(pageEvent) {
    this.lcAdjustmentRequest.page = pageEvent.pageIndex;
    this.lcAdjustmentRequest.rowsPerPage = pageEvent.pageSize;
    this.getLcAdjustments(this.lcAdjustmentRequest);
  }

  printPdf(id) {
    this.http
      .get(environment.apiURL + "/ReportLcAdjustment/" + id, {
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

  printLcAdjustmentPdf(name: string) {
    this.http
      .post(
        environment.apiURL + "/LcAdjustment/" + name,
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
