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
import { Priority } from "app/shared/enums/priority";
import { RequisitionStatus } from "app/shared/enums/requisitionStatus";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { Observable, merge, of } from "rxjs";
import { PendingCheckedCount } from "../../models/pending-checked-count";
import { PurchaseRequisitionResponseDTO } from "../../models/purchase-requisition/purchase-requisition-response-dto.model";
import { PurchaseRequisitionSearchRequestDTO } from "../../models/purchase-requisition/purchase-requisition-search-request-dto.model";
import { PurchaseRequisitionService } from "../../services/purchase-requisition.service";
import { RfqSentSupplierComponent } from "./rfq-sent-supplier/rfq-sent-supplier.component";

@Component({
  selector: "app-purchase-requisition",
  templateUrl: "./purchase-requisition.component.html",
  styleUrls: ["./purchase-requisition.component.scss"],
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
export class PurchaseRequisitionComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  viewColumnForm: FormGroup;
  expandedElement: any;
  dataSource: MatTableDataSource<PurchaseRequisitionResponseDTO>;
  totalCount: number;
  purchaseRequisitionRequest = new PurchaseRequisitionSearchRequestDTO();
  requisitionStatuses: ENUM[];
  pendingCheckedModel: PendingCheckedCount;
  displayedColumns$: Observable<string[]>;
  pageSizeOptions: [] = Page_Size_Options;
  currentFinancialYearId: string;
  public path = {
    addNew: "purchase/purchase-requisition/add-new",
    edit: "purchase/purchase-requisition",
    view: "purchase/purchase-requisition/view",
  };

  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  //  * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "expand", label: "Expand" },
    { def: "actions", label: "Actions" },
    { def: "requisitionNo", label: "Requisition No." },
    { def: "department", label: "Department" },
    { def: "store", label: "Store" },
    { def: "requisitionDate", label: "Requisition Date" },
    { def: "expectedDeliveryDate", label: "Expected Delivery Date" },
    // { def: "total", label: "Total" },
    { def: "requestByName", label: "Request By" },
    { def: "priority", label: "Priority" },
    { def: "requisitionStatus", label: "Req. Status" },
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
    private purchaseRequisitionService: PurchaseRequisitionService,
    private enumValueService: EnumValueService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    private http: HttpClient,
    public toastr: ToastrService,
    private router: Router,
    private confirmDialogService: ConfirmDialogService // private localStorageService: LocalStoreService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.getPendingCheckedCount();
    this.getPurchaseRequisitions(this.purchaseRequisitionRequest);
    this.getCurrentFinancialYearId();
    this.subscribeToFormChanges();
  }

  openExpansionPanel() {
    this.panelOpenState = true;
    this.getAllRequisitionStatuses();
  }

  private initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
      requisitionStatusIds: [[]],
      fromDate: [null],
      toDate: [null],
    });
    this.viewColumnForm = this.fb.group({
      expand: [true],
      requisitionNo: [true],
      department: [true],
      store: [true],
      requisitionDate: [false],
      expectedDeliveryDate: [false],
      // total: [true],
      requestByName: [false],
      priority: [true],
      requisitionStatus: [true],
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

  getAllRequisitionStatuses() {
    this.enumValueService.getRequisitionStatuses().subscribe((res) => {
      this.requisitionStatuses = res;
    });
  }

  getRequisitionStatus(value: number) {
    return this.statusColorService.getRequisitionStatus(value);
  }

  getRequisitionStatusName(value: number) {
    return RequisitionStatus[value];
  }

  getPriority(value: number) {
    return this.statusColorService.getPriority(value);
  }
  getPriorityName(value: number) {
    return Priority[value];
  }

  getPurchaseRequisitions(
    requestBody: PurchaseRequisitionSearchRequestDTO
  ): void {
    this.purchaseRequisitionService
      .getPurchaseRequisitions(requestBody)
      .subscribe((res) => {
        this.dataSource =
          new MatTableDataSource<PurchaseRequisitionResponseDTO>(
            res?.data?.item1
          );
        this.totalCount = res?.data?.item2;
        this.loading = false;
      });
  }

  getPendingCheckedCount(): void {
    this.purchaseRequisitionService
      .getPendingCheckedCount()
      .subscribe((res) => {
        this.pendingCheckedModel = res.data;
      });
  }

  getPending() {
    this.loading = true;
    this.purchaseRequisitionRequest.requisitionStatusIds = [1];
    this.getPurchaseRequisitions(this.purchaseRequisitionRequest);
  }

  getChecked() {
    this.loading = true;
    this.purchaseRequisitionRequest.requisitionStatusIds = [2];
    this.getPurchaseRequisitions(this.purchaseRequisitionRequest);
  }

  remove(id: string): void {
    this.purchaseRequisitionService
      .deletePurchaseRequisition(id)
      .subscribe((res) => {
        console.log(res);
        if (res?.succeeded) {
          this.getPurchaseRequisitions(this.purchaseRequisitionRequest);
          this.toastr.info(res?.message);
        } else {
          this.toastr.error(res?.message);
        }
      });
  }

  openRFQSentSupplierListDialog(data: PurchaseRequisitionResponseDTO) {
    const dialogRef = this.dialog.open(RfqSentSupplierComponent, {
      disableClose: true,
      panelClass: "add-bill-container",
      minHeight: "auto",
      height: "auto",
      data: data,
    });
    dialogRef.afterClosed().subscribe((confirm) => {});
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
    this.purchaseRequisitionRequest = new PurchaseRequisitionSearchRequestDTO();
    this.getPurchaseRequisitions(this.purchaseRequisitionRequest);
  }

  onSearch() {
    this.loading = true;
    this.purchaseRequisitionRequest.keyword = this.searchForm.value.keyword;
    this.purchaseRequisitionRequest.requisitionStatusIds =
      this.searchForm.value.requisitionStatusIds;
    this.purchaseRequisitionRequest.fromDate = this.searchForm.value.fromDate;
    this.purchaseRequisitionRequest.toDate = this.searchForm.value.toDate;
    this.getPurchaseRequisitions(this.purchaseRequisitionRequest);
  }

  navigateToAddNew() {
    this.router.navigate([this.path.addNew]);
  }

  navigateToView(item) {
    this.router.navigate([this.path.edit, item.id]);
    // this.localStorageService.setItem(item.id, item);
  }

  onPageChange(pageEvent) {
    this.purchaseRequisitionRequest.page = pageEvent.pageIndex;
    this.purchaseRequisitionRequest.rowsPerPage = pageEvent.pageSize;
    this.getPurchaseRequisitions(this.purchaseRequisitionRequest);
  }

  printPdf(id) {
    this.http
      .get(environment.apiURL + "/ReportPurchaseRequisition/" + id, {
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
