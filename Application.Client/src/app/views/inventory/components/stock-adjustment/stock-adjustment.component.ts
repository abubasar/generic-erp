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
import { Page_Size_Options } from "app/shared/consts/const";
import { StockAdjustmentStatus } from "app/shared/enums/stockAdjustmentStatus";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { PendingCheckedCount } from "app/views/purchase/models/pending-checked-count";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { Observable, merge, of } from "rxjs";
import { StockAdjustmentResponseDTO } from "../../models/stock-adjustment/stock-adjustment-response-dto.model";
import { StockAdjustmentSearchRequestDTO } from "../../models/stock-adjustment/stock-adjustment-search-request-dto.model";
import { StockAdjustmentService } from "../../services/stock-adjustment.service";

@Component({
  selector: "app-stock-adjustment",
  templateUrl: "./stock-adjustment.component.html",
  styleUrls: ["./stock-adjustment.component.scss"],
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
export class StockAdjustmentComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  viewColumnForm: FormGroup;
  expandedElement: any;
  dataSource: MatTableDataSource<StockAdjustmentResponseDTO>;
  totalCount: number;
  stockAdjustmentRequest = new StockAdjustmentSearchRequestDTO();
  stockAdjustmentStatuses: ENUM[];
  pendingCheckedModel: PendingCheckedCount;
  displayedColumns$: Observable<string[]>;
  pageSizeOptions: [] = Page_Size_Options;
  currentFinancialYearId: string;
  private path = {
    addNew: "inventory/stock-adjustment/add-new",
    edit: "inventory/stock-adjustment",
    view: "inventory/stock-adjustment/view",
  };

  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  //  * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "expand", label: "Expand" },
    { def: "actions", label: "Actions" },
    { def: "code", label: "Code" },
    { def: "adjustmentDate", label: "Adjustment Date" },
    { def: "store", label: "Store" },
    { def: "totalAdjustmentQty", label: "Total Adjustment Qty" },
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
    public statusColorService: StatusColorService,
    private stockAdjustmentService: StockAdjustmentService,
    private jwtAuth: JwtAuthService,
    private enumValueService: EnumValueService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private router: Router,
    private confirmDialogService: ConfirmDialogService,
    private http: HttpClient // private localStorageService: LocalStoreService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.getPendingCheckedCount();
    this.getStockAdjustments(this.stockAdjustmentRequest);
    this.getCurrentFinancialYearId();
    this.subscribeToFormChanges();
  }

  openExpansionPanel() {
    this.panelOpenState = true;
    this.getAllStockAdjustmentStatuses();
  }

  private initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
      status: [null],
      fromDate: [null],
      toDate: [null],
    });
    this.viewColumnForm = this.fb.group({
      expand: [true],
      code: [true],
      adjustmentDate: [true],
      store: [true],
      totalAdjustmentQty: [true],
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

  getAllStockAdjustmentStatuses() {
    this.enumValueService.getStockAdjustmentStatuses().subscribe((res) => {
      this.stockAdjustmentStatuses = res;
    });
  }

  getStockAdjustmentStatus(value: number) {
    return this.statusColorService.getStockAdjustmentStatus(value);
  }

  getStockAdjustmentStatusName(value: number) {
    return StockAdjustmentStatus[value];
  }

  getStockAdjustments(requestBody: StockAdjustmentSearchRequestDTO): void {
    this.stockAdjustmentService
      .getStockAdjustments(requestBody)
      .subscribe((res) => {
        this.dataSource = new MatTableDataSource<StockAdjustmentResponseDTO>(
          res?.data?.item1
        );
        this.totalCount = res?.data?.item2;
        this.loading = false;
      });
  }

  getPendingCheckedCount(): void {
    this.stockAdjustmentService.getPendingCheckedCount().subscribe((res) => {
      this.pendingCheckedModel = res.data;
    });
  }

  getPending() {
    this.loading = true;
    this.stockAdjustmentRequest.stockAdjustmentStatus = 1;
    this.getStockAdjustments(this.stockAdjustmentRequest);
  }

  getChecked() {
    this.loading = true;
    this.stockAdjustmentRequest.stockAdjustmentStatus = 2;
    this.getStockAdjustments(this.stockAdjustmentRequest);
  }

  remove(id: string): void {
    this.stockAdjustmentService.deleteStockAdjustment(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getStockAdjustments(this.stockAdjustmentRequest);
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
    this.stockAdjustmentRequest = new StockAdjustmentSearchRequestDTO();
    this.getStockAdjustments(this.stockAdjustmentRequest);
  }

  onSearch() {
    this.loading = true;
    this.stockAdjustmentRequest.keyword = this.searchForm.value.keyword;
    this.stockAdjustmentRequest.stockAdjustmentStatus =
      this.searchForm.value.status;
    this.stockAdjustmentRequest.fromDate = this.searchForm.value.fromDate;
    this.stockAdjustmentRequest.toDate = this.searchForm.value.toDate;
    this.getStockAdjustments(this.stockAdjustmentRequest);
  }

  navigateToAddNew() {
    this.router.navigate([this.path.addNew]);
  }

  navigateToView(item) {
    this.router.navigate([this.path.edit, item.id]);
    // this.localStorageService.setItem(item.id, item);
  }

  onPageChange(pageEvent) {
    this.stockAdjustmentRequest.page = pageEvent.pageIndex;
    this.stockAdjustmentRequest.rowsPerPage = pageEvent.pageSize;
    this.getStockAdjustments(this.stockAdjustmentRequest);
  }

  printPdf(id) {
    this.http
      .get(environment.apiURL + "/ReportStockAdjustment/" + id, {
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

  printStockAdjustmentPdf(name: string) {
    this.http
      .post(
        environment.apiURL + "/StockAdjustment/" + name,
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
