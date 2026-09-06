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
import { GRNStatus } from "app/shared/enums/grnStatus";
import { ImportPurchaseIncoTerm } from "app/shared/enums/importPurchaseIncoTerm";
import { ImportPurchasePaymentTerm } from "app/shared/enums/importPurchasePaymentTerm";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { Currency } from "app/views/configuration/models/currency/currency.model";
import { Supplier } from "app/views/configuration/models/supplier/supplier.model";
import { CurrencyService } from "app/views/configuration/services/currency.service";
import { SupplierService } from "app/views/configuration/services/supplier.service";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { Observable, merge, of } from "rxjs";
import { GoodsReceiveNoteAggregatorModel } from "../../models/goods-receive-note/goods-receive-note-aggregator.model";
import { GoodsReceiveNoteResponseDTO } from "../../models/goods-receive-note/goods-receive-note-response-dto.model";
import { GoodsReceiveNoteSearchRequestDTO } from "../../models/goods-receive-note/goods-receive-note-search-request-dto.model";
import { PendingCheckedCount } from "../../models/pending-checked-count";
import { GoodsReceiveNoteService } from "../../services/goods-receive-note.service";

@Component({
  selector: "app-goods-receive-note",
  templateUrl: "./goods-receive-note.component.html",
  styleUrls: ["./goods-receive-note.component.scss"],
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
export class GoodsReceiveNoteComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  suppliers: Supplier[];
  filterSuppliers: Supplier[];
  viewColumnForm: FormGroup;
  expandedElement: any;
  dataSource: MatTableDataSource<GoodsReceiveNoteResponseDTO>;
  reportAggregator = new GoodsReceiveNoteAggregatorModel();
  totalCount: number;
  goodsReceiveNoteStatuses: ENUM[];
  currencies: Currency[];
  goodsReceiveNoteRequest = new GoodsReceiveNoteSearchRequestDTO();
  pendingCheckedModel: PendingCheckedCount;
  displayedColumns$: Observable<string[]>;
  pageSizeOptions: [] = Page_Size_Options;
  currentFinancialYearId: string;
  private path = {
    addNew: "purchase/goods-receive-note/add-new",
    edit: "purchase/goods-receive-note",
    view: "purchase/goods-receive-note/view",
  };

  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  // * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "expand", label: "Expand" },
    { def: "actions", label: "Actions" },
    { def: "grnno", label: "GRN No." },
    { def: "grndate", label: "GRN Date" },
    { def: "ponumber", label: "PO Number" },
    { def: "storeId", label: "Store" },
    { def: "supplierId", label: "Supplier" },
    { def: "challanNo", label: "challan No" },
    { def: "challanDate", label: "challan Date" },
    { def: "truckNo", label: "Truck No." },
    { def: "driverName", label: "Driver Name" },
    { def: "driverContactNo", label: "Driver Contact No." },
    { def: "isImportPurchase", label: "Is Import Purchase" },
    { def: "proformaInvoiceNo", label: "Proforma Invoice No" },
    { def: "lcNumber", label: "LC Number" },
    { def: "exchangeRate", label: "Exchange Rate" },
    { def: "currencyId", label: "Currency" },
    { def: "importPurchaseIncoTerm", label: "Import Purchase Inco Term" },
    { def: "importPurchasePaymentTerm", label: "Import Purchase Payment Term" },
    { def: "portOfLoading", label: "Port of Loading" },
    { def: "portOfDestination", label: "Port of Destination" },
    { def: "subtotal", label: "Subtotal" },
    // { def: "discount", label: "Discount" },
    { def: "transportationCost", label: "Transportation Cost" },
    { def: "total", label: "Total" },
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
    public currencyService: CurrencyService,
    private goodsReceiveNoteService: GoodsReceiveNoteService,
    private supplierService: SupplierService,
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
    this.getAllCurrencies();
    this.getGoodsReceiveNotes(this.goodsReceiveNoteRequest);
    this.getPendingCheckedCount();
    this.getCurrentFinancialYearId();
    this.subscribeToFormChanges();
  }

  openExpansionPanel() {
    this.panelOpenState = true;
    this.getAllSuppliers();
    this.getAllGoodsReceiveNoteStatuses();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
      fromDate: [null],
      toDate: [null],
      ponumber: [""],
      supplierId: [null],
      isImportPurchase: [false],
      lcNumber: [""],
      grnStatus: [null],
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
      grnno: [true],
      grndate: [true],
      ponumber: [true],
      storeId: [true],
      supplierId: [true],
      challanNo: [false],
      challanDate: [false],
      truckNo: [false],
      driverName: [false],
      driverContactNo: [false],
      isImportPurchase: [false],
      proformaInvoiceNo: [false],
      lcNumber: [false],
      exchangeRate: [false],
      currencyId: [false],
      importPurchaseIncoTerm: [false],
      importPurchasePaymentTerm: [false],
      portOfLoading: [false],
      portOfDestination: [false],
      subtotal: [true],
      // discount: [false],
      transportationCost: [false],
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

  getImportPurchaseValue(): boolean {
    return this.searchForm?.get("isImportPurchase")?.value || false;
  }

  onChangeImportPurchase() {
    if (!this.searchForm.value.isImportPurchase) {
      this.searchForm.patchValue({
        lcNumber: "",
      });
    }
  }

  getAllCurrencies(): void {
    this.currencyService.getAllCurrencies().subscribe((res) => {
      this.currencies = res?.data?.item1;
    });
  }

  getCurrencyName(currencyId: string) {
    return this.currencies?.find((x) => x.id === currencyId)?.name;
  }

  getImportPurchaseIncoTermName(value: number) {
    return ImportPurchaseIncoTerm[value];
  }

  getImportPurchasePaymentTermName(value: number) {
    return ImportPurchasePaymentTerm[value];
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

  getGoodsReceiveNotes(requestBody): void {
    this.goodsReceiveNoteService
      .getGoodsReceiveNotes(requestBody)
      .subscribe((res) => {
        this.dataSource = new MatTableDataSource<GoodsReceiveNoteResponseDTO>(
          res?.data?.item1
        );
        this.reportAggregates(requestBody);
        this.totalCount = res?.data?.item2;
        this.loading = false;
      });
  }

  reportAggregates(requestBody: GoodsReceiveNoteSearchRequestDTO): void {
    this.goodsReceiveNoteService
      .reportAggregates(requestBody)
      .subscribe((res) => {
        this.reportAggregator = res.data;
      });
  }

  getPendingCheckedCount(): void {
    this.goodsReceiveNoteService.getPendingCheckedCount().subscribe((res) => {
      this.pendingCheckedModel = res.data;
    });
  }

  getPending() {
    this.loading = true;
    this.goodsReceiveNoteRequest.grnStatus = 1;
    this.getGoodsReceiveNotes(this.goodsReceiveNoteRequest);
  }

  getChecked() {
    this.loading = true;
    this.goodsReceiveNoteRequest.grnStatus = 2;
    this.getGoodsReceiveNotes(this.goodsReceiveNoteRequest);
  }

  getAllGoodsReceiveNoteStatuses() {
    this.enumValueService.getGRNStatuses().subscribe((res) => {
      this.goodsReceiveNoteStatuses = res;
    });
  }

  getGoodsReceiveNoteStatus(value) {
    return this.statusColorService.getGoodsReceiveNoteStatus(value);
  }

  getGoodsReceiveNoteName(value: number) {
    return GRNStatus[value];
  }

  remove(id: string): void {
    this.goodsReceiveNoteService.deleteGoodsReceiveNote(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getGoodsReceiveNotes(this.goodsReceiveNoteRequest);
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
    this.goodsReceiveNoteRequest = new GoodsReceiveNoteSearchRequestDTO();
    this.getGoodsReceiveNotes(this.goodsReceiveNoteRequest);
  }

  onSearch() {
    this.loading = true;
    this.goodsReceiveNoteRequest = {
      ...this.goodsReceiveNoteRequest,
      ...this.searchForm.value,
    };
    this.getGoodsReceiveNotes(this.goodsReceiveNoteRequest);
  }

  navigateToAddNew() {
    this.router.navigate([this.path.addNew]);
  }

  navigateToView(item) {
    this.router.navigate([this.path.edit, item.id]);
    // this.localStorageService.setItem(item.id, item);
  }

  onPageChange(pageEvent) {
    this.goodsReceiveNoteRequest.page = pageEvent.pageIndex;
    this.goodsReceiveNoteRequest.rowsPerPage = pageEvent.pageSize;
    this.getGoodsReceiveNotes(this.goodsReceiveNoteRequest);
  }

  printPdf(id) {
    this.http
      .get(environment.apiURL + "/ReportGoodsReceiveNote/" + id, {
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

  printGoodsReceiveNotePdf(name: string) {
    this.http
      .post(
        environment.apiURL + "/GoodsReceiveNote/" + name,
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
