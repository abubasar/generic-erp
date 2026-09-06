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
import { PurchaseReturnStatus } from "app/shared/enums/purchaseReturnStatus";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { LocalStoreService } from "app/shared/services/local-store.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { Supplier } from "app/views/configuration/models/supplier/supplier.model";
import { SupplierService } from "app/views/configuration/services/supplier.service";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { Observable, merge, of } from "rxjs";
import { PendingCheckedCount } from "../../models/pending-checked-count";
import { PurchaseReturnAggregatorModel } from "../../models/purchase-return/purchase-return-aggregator.model";
import { PurchaseReturnResponseDTO } from "../../models/purchase-return/purchase-return-response-dto.model";
import { PurchaseReturnSearchRequestDTO } from "../../models/purchase-return/purchase-return-search-request-dto.model";
import { PurchaseReturnService } from "../../services/purchase-return.service";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { UserProfile } from "app/shared/models/user-profile-model";

@Component({
  selector: "app-purchase-return",
  templateUrl: "./purchase-return.component.html",
  styleUrls: ["./purchase-return.component.scss"],
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
export class PurchaseReturnComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  suppliers: Supplier[];
  filterSuppliers: Supplier[];
  viewColumnForm: FormGroup;
  expandedElement: any;
  dataSource: MatTableDataSource<PurchaseReturnResponseDTO>;
  reportAggregator = new PurchaseReturnAggregatorModel();
  totalCount: number;
  purchaseReturnRequest = new PurchaseReturnSearchRequestDTO();
  purchaseReturnStatuses: ENUM[];
  pendingCheckedModel: PendingCheckedCount;
  displayedColumns$: Observable<string[]>;
  pageSizeOptions: [] = Page_Size_Options;
  currentFinancialYearId: string;
  private path = {
    addNew: "purchase/purchase-return/add-new",
    edit: "purchase/purchase-return",
    view: "purchase/purchase-return/view",
  };

  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  //  * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "expand", label: "Expand" },
    { def: "actions", label: "Actions" },
    { def: "purchaseReturnNo", label: "Purchase Return No." },
    { def: "purchaseReturnDate", label: "Purchase Return Date" },
    { def: "referenceNo", label: "Reference No." },
    { def: "grnno", label: "GRN No." },
    { def: "storeId", label: "Store" },
    { def: "supplierId", label: "Supplier" },
    { def: "totalAmount", label: "Total" },
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
    private purchaseReturnService: PurchaseReturnService,
    private supplierService: SupplierService,
    private enumValueService: EnumValueService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private router: Router,
    private http: HttpClient,
    private localStorageService: LocalStoreService,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.getPendingCheckedCount();
    this.getPurchaseReturns(this.purchaseReturnRequest);
    this.getCurrentFinancialYearId();
    this.subscribeToFormChanges();
  }
  openExpansionPanel() {
    this.panelOpenState = true;
    this.getAllPurchaseReturnStatuses();
    this.getAllSuppliers();
  }

  private initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
      fromDate: [null],
      toDate: [null],
      purchaseReturnNo: [""],
      supplierId: [null],
      purchaseReturnStatus: [null],
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
      purchaseReturnNo: [true],
      purchaseReturnDate: [true],
      referenceNo: [true],
      grnno: [true],
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

  getAllPurchaseReturnStatuses() {
    this.enumValueService.getPurchaseReturnStatuses().subscribe((res) => {
      this.purchaseReturnStatuses = res;
    });
  }

  getPurchaseReturnStatus(value: number) {
    return this.statusColorService.getPurchaseReturnStatus(value);
  }

  getPurchaseReturnStatusName(value: number) {
    return PurchaseReturnStatus[value];
  }

  getPurchaseReturns(requestBody: PurchaseReturnSearchRequestDTO): void {
    this.purchaseReturnService
      .getPurchaseReturns(requestBody)
      .subscribe((res) => {
        console.log(res);
        this.dataSource = new MatTableDataSource<PurchaseReturnResponseDTO>(
          res?.data?.item1
        );
        this.reportAggregates(requestBody);
        this.totalCount = res?.data?.item2;
        this.loading = false;
      });
  }

  reportAggregates(requestBody: PurchaseReturnSearchRequestDTO): void {
    this.purchaseReturnService
      .reportAggregates(requestBody)
      .subscribe((res) => {
        this.reportAggregator = res.data;
      });
  }

  getPendingCheckedCount(): void {
    this.purchaseReturnService.getPendingCheckedCount().subscribe((res) => {
      this.pendingCheckedModel = res.data;
    });
  }

  getPending() {
    this.loading = true;
    this.purchaseReturnRequest.purchaseReturnStatus = 1;
    this.getPurchaseReturns(this.purchaseReturnRequest);
  }

  getChecked() {
    this.loading = true;
    this.purchaseReturnRequest.purchaseReturnStatus = 2;
    this.getPurchaseReturns(this.purchaseReturnRequest);
  }

  // Function to remove a data by ID
  remove(id: string): void {
    this.purchaseReturnService.deletePurchaseReturn(id).subscribe((res) => {
      console.log(res);
      if (res?.succeeded) {
        // Refresh the list
        this.getPurchaseReturns(this.purchaseReturnRequest);
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
    this.purchaseReturnRequest = new PurchaseReturnSearchRequestDTO();
    this.getPurchaseReturns(this.purchaseReturnRequest);
  }

  onSearch() {
    this.loading = true;
    this.purchaseReturnRequest = {
      ...this.purchaseReturnRequest,
      ...this.searchForm.value,
    };
    this.getPurchaseReturns(this.purchaseReturnRequest);
  }

  navigateToAddNew() {
    this.router.navigate([this.path.addNew]);
  }

  navigateToView(item) {
    this.router.navigate([this.path.edit, item.id]);
    // this.localStorageService.setItem(item.id, item);
  }

  onPageChange(pageEvent) {
    this.purchaseReturnRequest.page = pageEvent.pageIndex;
    this.purchaseReturnRequest.rowsPerPage = pageEvent.pageSize;
    this.getPurchaseReturns(this.purchaseReturnRequest);
  }

  printPdf(id) {
    this.http
      .get(environment.apiURL + "/ReportPurchaseReturn/" + id, {
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

  printPurchaseReturnPdf(name: string) {
    this.http
      .post(
        environment.apiURL + "/purchaseReturn/" + name,
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
