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
import { SaleQuotationStatus } from "app/shared/enums/saleQuotationStatus";
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
import { SaleQuotationAggregatorModel } from "../../models/sale-quotation/sale-quotation-aggregator.model";
import { SaleQuotationResponseDTO } from "../../models/sale-quotation/sale-quotation-response-dto.model";
import { SaleQuotationSearchRequestDTO } from "../../models/sale-quotation/sale-quotation-search-request-dto.model";
import { SaleQuotationService } from "../../services/sale-quotation.service";

@Component({
  selector: "app-sales-quotation",
  templateUrl: "./sales-quotation.component.html",
  styleUrls: ["./sales-quotation.component.scss"],
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
export class SalesQuotationComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  viewColumnForm: FormGroup;
  expandedElement: any;
  dataSource: MatTableDataSource<SaleQuotationResponseDTO>;
  reportAggregator = new SaleQuotationAggregatorModel();
  totalCount: number;
  saleQuotationRequest = new SaleQuotationSearchRequestDTO();
  saleQuotationStatuses: ENUM[];
  pendingCheckedModel: PendingCheckedCount;
  displayedColumns$: Observable<string[]>;
  pageSizeOptions: [] = Page_Size_Options;
  currentFinancialYearId: string;

  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  private path = {
    addNew: "sales/sales-quotation/add-new",
    edit: "sales/sales-quotation",
    view: "sales/sales-quotation/view",
  };

  //  * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "expand", label: "Expand" },
    { def: "actions", label: "Actions" },
    { def: "quotationNo", label: "Quotation No" },
    { def: "quotationDate", label: "Quotation Date" },
    { def: "customerId", label: "Customer" },
    { def: "referenceNo", label: "Reference No" },
    { def: "expiryDate", label: "Expiry Date" },
    { def: "termAndCondition", label: "Term & Condition" },
    { def: "subtotal", label: "Subtotal" },
    { def: "discount", label: "Discount" },
    { def: "total", label: "Total" },
    { def: "isMailSent", label: "Is Mail Sent" },
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
    private saleQuotationService: SaleQuotationService,
    private jwtAuth: JwtAuthService,
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
    this.getSaleQuotations(this.saleQuotationRequest);
    this.getCurrentFinancialYearId();
    this.subscribeToFormChanges();
  }

  openExpansionPanel() {
    this.panelOpenState = true;
    this.getAllSaleQuotationStatuses();
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
      quotationNo: [true],
      quotationDate: [true],
      customerId: [true],
      referenceNo: [true],
      expiryDate: [true],
      termAndCondition: [true],
      subtotal: [true],
      discount: [true],
      total: [true],
      isMailSent: [true],
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

  getAllSaleQuotationStatuses() {
    this.enumValueService.getSaleQuotationStatuses().subscribe((res) => {
      this.saleQuotationStatuses = res;
    });
  }

  getSaleQuotationStatus(value: number) {
    return this.statusColorService.getSaleQuotationStatus(value);
  }

  getSaleQuotationStatusName(value: number) {
    return SaleQuotationStatus[value];
  }

  getSaleQuotations(requestBody: SaleQuotationSearchRequestDTO): void {
    this.saleQuotationService
      .getSaleQuotations(requestBody)
      .subscribe((res) => {
        this.dataSource = new MatTableDataSource<SaleQuotationResponseDTO>(
          res?.data?.item1
        );
        this.reportAggregates(requestBody);
        this.totalCount = res?.data?.item2;
        this.loading = false;
      });
  }

  reportAggregates(requestBody: SaleQuotationSearchRequestDTO): void {
    this.saleQuotationService.reportAggregates(requestBody).subscribe((res) => {
      this.reportAggregator = res.data;
    });
  }

  getPendingCheckedCount(): void {
    this.saleQuotationService.getPendingCheckedCount().subscribe((res) => {
      this.pendingCheckedModel = res.data;
    });
  }

  getPending() {
    this.loading = true;
    this.saleQuotationRequest.saleQuotationStatus = 1;
    this.getSaleQuotations(this.saleQuotationRequest);
  }

  getChecked() {
    this.loading = true;
    this.saleQuotationRequest.saleQuotationStatus = 2;
    this.getSaleQuotations(this.saleQuotationRequest);
  }

  remove(id: string): void {
    this.saleQuotationService.deleteSaleQuotation(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getSaleQuotations(this.saleQuotationRequest);
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
    this.saleQuotationRequest = new SaleQuotationSearchRequestDTO();
    this.getSaleQuotations(this.saleQuotationRequest);
  }

  onSearch() {
    this.loading = true;
    this.saleQuotationRequest = {
      ...this.saleQuotationRequest,
      ...this.searchForm.value,
    };
    this.getSaleQuotations(this.saleQuotationRequest);
  }

  navigateToAddNew() {
    this.router.navigate([this.path.addNew]);
  }

  navigateToView(item) {
    this.router.navigate([this.path.edit, item.id]);
    // this.localStorageService.setItem(item.id, item);
  }

  onPageChange(pageEvent) {
    this.saleQuotationRequest.page = pageEvent.pageIndex;
    this.saleQuotationRequest.rowsPerPage = pageEvent.pageSize;
    this.getSaleQuotations(this.saleQuotationRequest);
  }

  printPdf(id) {
    this.http
      .get(environment.apiURL + "/ReportSaleQuotation/" + id, {
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
