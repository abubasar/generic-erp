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
import {
  Inventory_Type_Id_Finished_Goods,
  Page_Size_Options,
} from "app/shared/consts/const";
import { StockTransferStatus } from "app/shared/enums/stockTransferStatus";
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
import { StockTransferResponseDTO } from "../../../inventory/models/stock-transfer/stock-transfer-response-dto.model";
import { StockTransferSearchRequestDTO } from "../../../inventory/models/stock-transfer/stock-transfer-search-request-dto.model";
import { StockTransferService } from "../../../inventory/services/stock-transfer.service";

@Component({
  selector: "app-stock-transfer",
  templateUrl: "./stock-transfer.component.html",
  styleUrls: ["./stock-transfer.component.scss"],
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
export class StockTransferComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  viewColumnForm: FormGroup;
  expandedElement: any;
  dataSource: MatTableDataSource<StockTransferResponseDTO>;
  totalCount: number;
  stockTransferRequest = new StockTransferSearchRequestDTO();
  stockTransferStatuses: ENUM[];
  pendingCheckedModel: PendingCheckedCount;
  displayedColumns$: Observable<string[]>;
  pageSizeOptions: [] = Page_Size_Options;
  currentFinancialYearId: string;
  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  private path = {
    addNew: "inventory/stock-transfer/add-new",
    edit: "inventory/stock-transfer",
    view: "inventory/stock-transfer/view",
  };

  //  * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "expand", label: "Expand" },
    { def: "actions", label: "Actions" },
    { def: "transferNo", label: "Transfer No" },
    { def: "sourceId", label: "Source" },
    { def: "destinationId", label: "Destination" },
    { def: "transferDate", label: "Transfer Date" },
    { def: "truckNo", label: "Truck No" },
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
    public statusColorService: StatusColorService,
    private dateFormatService: DateTimeFormatService,
    private jwtAuth: JwtAuthService,
    private stockTransferService: StockTransferService,
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
    this.getStockTransfers(this.stockTransferRequest);
    this.getCurrentFinancialYearId();
    this.subscribeToFormChanges();
  }

  openExpansionPanel() {
    this.panelOpenState = true;
    this.getAllStockTransferStatuses();
  }

  private initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
      stockTransferStatus: [null],
      fromDate: [null],
      toDate: [null],
    });
    this.viewColumnForm = this.fb.group({
      expand: [true],
      sourceId: [true],
      destinationId: [true],
      transferNo: [true],
      transferDate: [true],
      truckNo: [true],
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

  isFinishedGoodsInventory(inventoryTypeId: string): boolean {
    return inventoryTypeId === Inventory_Type_Id_Finished_Goods;
  }

  getAllStockTransferStatuses() {
    this.enumValueService.getStockTransferStatuses().subscribe((res) => {
      this.stockTransferStatuses = res;
    });
  }

  getStockTransferStatus(value: number) {
    return this.statusColorService.getStockTransferStatus(value);
  }

  getStockTransferStatusName(value: number) {
    return StockTransferStatus[value];
  }

  getStockTransfers(request: StockTransferSearchRequestDTO): void {
    this.loading = true;
    this.stockTransferService.getStockTransfers(request).subscribe((res) => {
      this.dataSource = new MatTableDataSource<StockTransferResponseDTO>(
        res?.data?.item1
      );
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  getPendingCheckedCount(): void {
    this.stockTransferService.getPendingCheckedCount().subscribe((res) => {
      this.pendingCheckedModel = res.data;
    });
  }

  getPending() {
    this.stockTransferRequest.stockTransferStatus = 1;
    this.getStockTransfers(this.stockTransferRequest);
  }

  getChecked() {
    this.stockTransferRequest.stockTransferStatus = 2;
    this.getStockTransfers(this.stockTransferRequest);
  }

  remove(id: string): void {
    this.stockTransferService.deleteStockTransfer(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getStockTransfers(this.stockTransferRequest);
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

  // onSearch() {
  //   this.stockTransferRequest.keyword = this.searchForm.value.keyword;
  //   this.stockTransferRequest.stockTransferStatus =
  //     this.searchForm.value.status;
  //   this.stockTransferRequest.fromDate = this.searchForm.value.fromDate;
  //   this.stockTransferRequest.toDate = this.searchForm.value.toDate;
  //   this.getStockTransfers();
  // }

  navigateToAddNew() {
    this.router.navigate([this.path.addNew]);
  }

  navigateToView(item) {
    this.router.navigate([this.path.edit, item.id]);
    // this.localStorageService.setItem(item.id, item);
  }

  // onPageChange(pageEvent) {
  //   this.stockTransferRequest.page = pageEvent.pageIndex;
  //   this.stockTransferRequest.rowsPerPage = pageEvent.pageSize;
  //   this.getStockTransfers();
  // }

  reload() {
    this.searchForm.reset();
    this.loading = true;
    this.stockTransferRequest = new StockTransferSearchRequestDTO();
    this.getStockTransfers(this.stockTransferRequest);
  }

  onSearch() {
    this.loading = true;
    this.updateTransferRequest();
    this.getStockTransfers(this.stockTransferRequest);
  }

  onPageChange(pageEvent) {
    this.updateTransferRequest(pageEvent.pageIndex, pageEvent.pageSize);
    this.getStockTransfers(this.stockTransferRequest);
  }

  private updateTransferRequest(
    pageIndex = this.stockTransferRequest.page, // may be here have a ghapla
    pageSize = this.stockTransferRequest.rowsPerPage // may be here have a ghapla
  ) {
    const { keyword, fromDate, toDate, stockTransferStatus } =
      this.searchForm.value;

    this.stockTransferRequest = {
      ...this.stockTransferRequest,
      keyword,
      fromDate,
      toDate,
      stockTransferStatus,
      page: pageIndex,
      rowsPerPage: pageSize,
    };
  }

  printPdf(id) {
    this.http
      .get(environment.apiURL + "/ReportStockTransfer/" + id, {
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

  printStockTransferPdf(name: string) {
    this.http
      .post(
        environment.apiURL + "/stockTransfer/" + name,
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
