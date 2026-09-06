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
import { ManufacturingOrderStatus } from "app/shared/enums/manufacturingOrderStatus";
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
import { ManufacturingOrderAggregatorModel } from "../../models/manufacturing-order/manufacturing-order-aggregator.model";
import { ManufacturingOrderResponseDTO } from "../../models/manufacturing-order/manufacturing-order-response-dto.model";
import { ManufacturingOrderSearchRequestDTO } from "../../models/manufacturing-order/manufacturing-order-search-request-dto.model";
import { ManufacturingOrderService } from "../../services/manufacturing-order.service";

@Component({
  selector: "app-manufacturing-order",
  templateUrl: "./manufacturing-order.component.html",
  styleUrls: ["./manufacturing-order.component.scss"],
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
export class ManufacturingOrderComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  viewColumnForm: FormGroup;
  expandedElement: any;
  dataSource: MatTableDataSource<ManufacturingOrderResponseDTO>;
  reportAggregator = new ManufacturingOrderAggregatorModel();
  totalCount: number;
  manufacturingOrderRequest = new ManufacturingOrderSearchRequestDTO();
  manufacturingOrderStatuses: ENUM[];
  pendingCheckedModel: PendingCheckedCount;
  displayedColumns$: Observable<string[]>;
  pageSizeOptions: [] = Page_Size_Options;
  currentFinancialYearId: string;
  businessType: string;
  private path = {
    addNew: "production/manufacturing-order/add-new",
    edit: "production/manufacturing-order",
    view: "production/manufacturing-order/view",
  };

  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  //  * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "expand", label: "Expand" },
    { def: "actions", label: "Actions" },
    { def: "manufacturingOrderNo", label: "Manufacturing Order No." },
    { def: "bomNo", label: "BOM No." },
    { def: "scheduledDate", label: "Scheduled Date" },
    { def: "formulationNo", label: "Formulation No." },
    { def: "finishedProduct", label: "Finished Product" },
    { def: "rawMaterialStore", label: "Raw Material Store" },
    { def: "productionQuantity", label: "Production Quantity" },
    { def: "totalRmused", label: "Total RM Used" },
    { def: "rmCost", label: "RM Cost" },
    {
      def: "standardDirectExpenseAmount",
      label: "Standard Direct Expense Amount",
    },
    {
      def: "standardFactoryOverheadAmount",
      label: "Standard Factory Overhead Amount",
    },
    { def: "totalCost", label: "Total Cost" },
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
    private dateFormatService: DateTimeFormatService,
    private jwtAuth: JwtAuthService,
    public statusColorService: StatusColorService,
    private manufacturingOrderService: ManufacturingOrderService,
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
    this.getPendingCheckedCount();
    this.getManufacturingOrders(this.manufacturingOrderRequest);
    this.getCurrentFinancialYearId();
    this.subscribeToFormChanges();
  }

  openExpansionPanel() {
    this.panelOpenState = true;
    this.getAllManufacturingOrderStatuses();
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
      manufacturingOrderNo: [true],
      bomNo: [true],
      scheduledDate: [true],
      formulationNo: [true],
      finishedProduct: [true],
      rawMaterialStore: [true],
      productionQuantity: [true],
      totalRmused: [true],
      rmCost: [true],
      standardDirectExpenseAmount: [false],
      standardFactoryOverheadAmount: [false],
      totalCost: [true],
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
      this.businessType = res.businesstype;
    });
  }

  getAllManufacturingOrderStatuses() {
    this.enumValueService.getManufacturingOrderStatuses().subscribe((res) => {
      this.manufacturingOrderStatuses = res;
    });
  }

  getManufacturingOrderStatus(value: number) {
    return this.statusColorService.getManufacturingOrderStatus(value);
  }

  getManufacturingOrderStatusName(value: number) {
    return ManufacturingOrderStatus[value];
  }

  getManufacturingOrders(
    requestBody: ManufacturingOrderSearchRequestDTO
  ): void {
    this.manufacturingOrderService
      .getManufacturingOrders(requestBody)
      .subscribe((res) => {
        this.dataSource = new MatTableDataSource<ManufacturingOrderResponseDTO>(
          res?.data?.item1
        );
        this.reportAggregates(requestBody);
        console.log(res?.data?.item1);
        this.totalCount = res?.data?.item2;
        this.loading = false;
      });
  }

  reportAggregates(requestBody: ManufacturingOrderSearchRequestDTO): void {
    this.manufacturingOrderService
      .reportAggregates(requestBody)
      .subscribe((res) => {
        this.reportAggregator = res.data;
        console.log("-----------", this.reportAggregator);
      });
  }

  getPendingCheckedCount(): void {
    this.manufacturingOrderService.getPendingCheckedCount().subscribe((res) => {
      this.pendingCheckedModel = res.data;
    });
  }

  getPending() {
    this.loading = true;
    this.manufacturingOrderRequest.manufacturingOrderStatus = 1;
    this.getManufacturingOrders(this.manufacturingOrderRequest);
  }
  getChecked() {
    this.loading = true;
    this.manufacturingOrderRequest.manufacturingOrderStatus = 2;
    this.getManufacturingOrders(this.manufacturingOrderRequest);
  }
  remove(id: string): void {
    this.manufacturingOrderService
      .deleteManufacturingOrder(id)
      .subscribe((res) => {
        if (res?.succeeded) {
          this.getManufacturingOrders(this.manufacturingOrderRequest);
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
    this.manufacturingOrderRequest = new ManufacturingOrderSearchRequestDTO();
    this.getManufacturingOrders(this.manufacturingOrderRequest);
  }

  onSearch() {
    this.loading = true;
    this.manufacturingOrderRequest = {
      ...this.manufacturingOrderRequest,
      ...this.searchForm.value,
    };
    this.getManufacturingOrders(this.manufacturingOrderRequest);
  }

  navigateToAddNew() {
    this.router.navigate([this.path.addNew]);
  }

  navigateToView(item) {
    this.router.navigate([this.path.edit, item.id]);
    // this.localStorageService.setItem(item.id, item);
  }

  onPageChange(pageEvent) {
    this.manufacturingOrderRequest.page = pageEvent.pageIndex;
    this.manufacturingOrderRequest.rowsPerPage = pageEvent.pageSize;
    this.getManufacturingOrders(this.manufacturingOrderRequest);
  }

  generateReport(event: Event, id: number, type: "details" | "summary") {
    const targetName = (event.currentTarget as HTMLButtonElement).name;
    const reportType = targetName === "pdf" ? 1 : 2;

    const endpoint =
      environment.apiURL +
      `/ReportManufacturingOrder/manufacturing-order-${type}/${id}/${reportType}`;

    this.http
      .get(endpoint, { responseType: "blob" })
      .subscribe((response) =>
        this.handleFileResponse(response, targetName, type)
      );
  }

  private handleFileResponse(
    response: Blob,
    targetName: string,
    type: string
  ): void {
    const fileType =
      targetName === "pdf" ? "application/pdf" : "application/octet-stream";
    // Create a Blob from the PDF Stream
    const file = new Blob([response], { type: fileType });
    const fileURL = URL.createObjectURL(file);
    if (targetName === "pdf") {
      // Open the URL in a new window
      const pdfWindow = window.open();
      pdfWindow.location.href = fileURL;
    } else {
      const link = document.createElement("a");
      link.href = fileURL;
      // Generate a dynamic filename based on the report type
      link.setAttribute("download", `material_issue_${type}.xlsx`);
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link); // Cleanup after download
    }
  }

  printManufacturingOrderDetailsPdf(event: Event, id: number) {
    this.generateReport(event, id, "details");
  }

  printManufacturingOrderSummaryPdf(event: Event, id: number) {
    this.generateReport(event, id, "summary");
  }

  trackBy(index: number, item: any) {
    return item?.id;
  }
}
