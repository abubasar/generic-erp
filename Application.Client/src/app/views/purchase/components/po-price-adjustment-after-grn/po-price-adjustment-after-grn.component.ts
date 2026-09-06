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
import { PoPriceAdjustmentAfterGrnStatus } from "app/shared/enums/poPriceAdjustmentAfterGrnStatus";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { Supplier } from "app/views/configuration/models/supplier/supplier.model";
import { SupplierService } from "app/views/configuration/services/supplier.service";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { Observable, merge, of } from "rxjs";
import { PendingCheckedCount } from "../../models/pending-checked-count";
import { PoPriceAdjustmentAfterGrnAggregatorModel } from "../../models/po-price-adjustment-after-grn/po-price-adjustment-after-grn-aggregator.model";
import { PoPriceAdjustmentAfterGrnResponseDTO } from "../../models/po-price-adjustment-after-grn/po-price-adjustment-after-grn-response-dto.model";
import { PoPriceAdjustmentAfterGrnSearchRequestDTO } from "../../models/po-price-adjustment-after-grn/po-price-adjustment-after-grn-search-request-dto.model";
import { PoPriceAdjustmentAfterGrnService } from "../../services/po-price-adjustment-after-grn.service";

@Component({
  selector: "app-po-price-adjustment-after-grn",
  templateUrl: "./po-price-adjustment-after-grn.component.html",
  styleUrls: ["./po-price-adjustment-after-grn.component.scss"],
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
export class PoPriceAdjustmentAfterGrnComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  suppliers: Supplier[];
  filterSuppliers: Supplier[];
  viewColumnForm: FormGroup;
  expandedElement: any;
  dataSource: MatTableDataSource<PoPriceAdjustmentAfterGrnResponseDTO>;
  reportAggregator = new PoPriceAdjustmentAfterGrnAggregatorModel();
  totalCount: number;
  poPriceAdjustmentAfterGrnRequest =
    new PoPriceAdjustmentAfterGrnSearchRequestDTO();
  poPriceAdjustmentAfterGrnStatuses: ENUM[];
  pendingCheckedModel: PendingCheckedCount;
  displayedColumns$: Observable<string[]>;
  pageSizeOptions: [] = Page_Size_Options;
  currentFinancialYearId: string;
  private path = {
    addNew: "purchase/po-price-adjustment-after-grn/add-new",
    edit: "purchase/po-price-adjustment-after-grn",
    view: "purchase/po-price-adjustment-after-grn/view",
  };

  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  //  * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "expand", label: "Expand" },
    { def: "actions", label: "Actions" },
    { def: "code", label: "Code" },
    { def: "adjustmentDate", label: "Adjustment Date" },
    { def: "referenceNo", label: "Reference No." },
    { def: "grnno", label: "GRN No." },
    { def: "ponumber", label: "PO No." },
    { def: "storeId", label: "Store" },
    { def: "supplierId", label: "Supplier" },
    { def: "totalAmount", label: "Total Variance" },
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
    private poPriceAdjustmentAfterGrnService: PoPriceAdjustmentAfterGrnService,
    private supplierService: SupplierService,
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
    this.getPoPriceAdjustmentAfterGrns(this.poPriceAdjustmentAfterGrnRequest);
    this.getCurrentFinancialYearId();
    this.subscribeToFormChanges();
  }

  openExpansionPanel() {
    this.panelOpenState = true;
    this.getAllPoPriceAdjustmentAfterGrnStatuses();
    this.getAllSuppliers();
  }

  private initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
      fromDate: [null],
      toDate: [null],
      code: [""],
      supplierId: [null],
      poPriceAdjustmentAfterGrnStatus: [null],
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
      adjustmentDate: [true],
      referenceNo: [true],
      grnno: [true],
      ponumber: [true],
      storeId: [true],
      supplierId: [true],
      totalAmount: [true],
      remark: [true],
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

  getAllSuppliers(): void {
    this.supplierService.getAllSuppliers().subscribe((res) => {
      this.filterSuppliers = this.suppliers = res?.data?.item1;
    });
  }

  getAllPoPriceAdjustmentAfterGrnStatuses() {
    this.enumValueService
      .getPoPriceAdjustmentAfterGrnStatuses()
      .subscribe((res) => {
        this.poPriceAdjustmentAfterGrnStatuses = res;
      });
  }

  getPoPriceAdjustmentAfterGrnStatus(value: number) {
    return this.statusColorService.getPoPriceAdjustmentAfterGrnStatus(value);
  }

  getPoPriceAdjustmentAfterGrnStatusName(value: number) {
    return PoPriceAdjustmentAfterGrnStatus[value];
  }

  getPoPriceAdjustmentAfterGrns(
    requestBody: PoPriceAdjustmentAfterGrnSearchRequestDTO
  ): void {
    this.poPriceAdjustmentAfterGrnService
      .getPoPriceAdjustmentAfterGrns(requestBody)
      .subscribe((res) => {
        this.dataSource =
          new MatTableDataSource<PoPriceAdjustmentAfterGrnResponseDTO>(
            res?.data?.item1
          );
        this.reportAggregates(requestBody);
        this.totalCount = res?.data?.item2;
        this.loading = false;
      });
  }

  reportAggregates(
    requestBody: PoPriceAdjustmentAfterGrnSearchRequestDTO
  ): void {
    this.poPriceAdjustmentAfterGrnService
      .reportAggregates(requestBody)
      .subscribe((res) => {
        this.reportAggregator = res.data;
      });
  }

  getPendingCheckedCount(): void {
    this.poPriceAdjustmentAfterGrnService
      .getPendingCheckedCount()
      .subscribe((res) => {
        this.pendingCheckedModel = res.data;
      });
  }

  getPending() {
    this.loading = true;
    this.poPriceAdjustmentAfterGrnRequest.poPriceAdjustmentAfterGrnStatus = 1;
    this.getPoPriceAdjustmentAfterGrns(this.poPriceAdjustmentAfterGrnRequest);
  }

  getChecked() {
    this.loading = true;
    this.poPriceAdjustmentAfterGrnRequest.poPriceAdjustmentAfterGrnStatus = 2;
    this.getPoPriceAdjustmentAfterGrns(this.poPriceAdjustmentAfterGrnRequest);
  }

  // Function to remove a data by ID
  remove(id: string): void {
    this.poPriceAdjustmentAfterGrnService
      .deletePoPriceAdjustmentAfterGrn(id)
      .subscribe((res) => {
        if (res?.succeeded) {
          // Refresh the list
          this.getPoPriceAdjustmentAfterGrns(
            this.poPriceAdjustmentAfterGrnRequest
          );
          // Display a toastr message with the response message
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
    this.poPriceAdjustmentAfterGrnRequest =
      new PoPriceAdjustmentAfterGrnSearchRequestDTO();
    this.getPoPriceAdjustmentAfterGrns(this.poPriceAdjustmentAfterGrnRequest);
  }

  onSearch() {
    this.loading = true;
    this.poPriceAdjustmentAfterGrnRequest = {
      ...this.poPriceAdjustmentAfterGrnRequest,
      ...this.searchForm.value,
    };
    this.getPoPriceAdjustmentAfterGrns(this.poPriceAdjustmentAfterGrnRequest);
  }

  navigateToAddNew() {
    this.router.navigate([this.path.addNew]);
  }

  navigateToView(item) {
    this.router.navigate([this.path.edit, item.id]);
  }

  onPageChange(pageEvent) {
    this.poPriceAdjustmentAfterGrnRequest.page = pageEvent.pageIndex;
    this.poPriceAdjustmentAfterGrnRequest.rowsPerPage = pageEvent.pageSize;
    this.getPoPriceAdjustmentAfterGrns(this.poPriceAdjustmentAfterGrnRequest);
  }

  printPdf(id) {
    this.http
      .get(environment.apiURL + "/ReportPoPriceAdjustmentAfterGrn/" + id, {
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

  printPoPriceAdjustmentAfterGrnPdf(name: string) {
    this.http
      .post(
        environment.apiURL + "/poPriceAdjustmentAfterGrn/" + name,
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
