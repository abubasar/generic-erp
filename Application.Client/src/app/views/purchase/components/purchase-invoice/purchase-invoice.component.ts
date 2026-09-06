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
import { ImportPurchaseIncoTerm } from "app/shared/enums/importPurchaseIncoTerm";
import { ImportPurchasePaymentTerm } from "app/shared/enums/importPurchasePaymentTerm";
import { PurchaseInvoiceStatus } from "app/shared/enums/purchaseInvoiceStatus";
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
import { PendingCheckedCount } from "../../models/pending-checked-count";
import { PurchaseInvoiceAggregatorModel } from "../../models/purchase-invoice/purchase-invoice-aggregator.model";
import { PurchaseInvoiceResponseDTO } from "../../models/purchase-invoice/purchase-invoice-response-dto.model";
import { PurchaseInvoiceSearchRequestDTO } from "../../models/purchase-invoice/purchase-invoice-search-request-dto.model";
import { PurchaseInvoiceService } from "../../services/purchase-invoice.service";

@Component({
  selector: "app-purchase-invoice",
  templateUrl: "./purchase-invoice.component.html",
  styleUrls: ["./purchase-invoice.component.scss"],
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
export class PurchaseInvoiceComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  suppliers: Supplier[];
  filterSuppliers: Supplier[];
  viewColumnForm: FormGroup;
  expandedElement: any;
  dataSource: MatTableDataSource<PurchaseInvoiceResponseDTO>;
  reportAggregator = new PurchaseInvoiceAggregatorModel();
  totalCount: number;
  purchaseInvoiceStatuses: ENUM[];
  currencies: Currency[];
  purchaseInvoiceRequest = new PurchaseInvoiceSearchRequestDTO();
  pendingCheckedModel: PendingCheckedCount;
  displayedColumns$: Observable<string[]>;
  pageSizeOptions: [] = Page_Size_Options;
  currentFinancialYearId: string;

  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  //  * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "expand", label: "Expand" },
    { def: "actions", label: "Actions" },
    { def: "purchaseInvoiceNo", label: "Purchase Invoice No." },
    { def: "invoiceDate", label: "Invoice Date" },
    { def: "grnno", label: "GRN No." },
    { def: "ponumber", label: "PO Number" },
    { def: "supplierPaymentCode", label: "Supplier Payment Code" },
    { def: "storeId", label: "Store" },
    { def: "supplierId", label: "Supplier" },
    { def: "supplierInvoiceNo", label: "Sup. Invoice No" },
    { def: "supplierInvoiceDate", label: "Sup. Invoice Date" },
    { def: "paymentTermInDays", label: "Payment Term In Days" },
    { def: "isImportPurchase", label: "Is Import Purchase" },
    { def: "proformaInvoiceNo", label: "Proforma Invoice No" },
    { def: "lcNumber", label: "LC Number" },
    { def: "exchangeRate", label: "Exchange Rate" },
    { def: "currencyId", label: "Currency" },
    { def: "importPurchaseIncoTerm", label: "Import Purchase Inco Term" },
    { def: "importPurchasePaymentTerm", label: "Import Purchase Payment Term" },
    { def: "billOfEntryNo", label: "Bill Of Entry No" },
    { def: "billOfEntryDate", label: "Bill Of Entry Date" },
    { def: "portOfLoading", label: "Port of Loading" },
    { def: "portOfDestination", label: "Port of Destination" },
    { def: "subtotal", label: "Subtotal" },
    { def: "discount", label: "Discount" },
    { def: "totalVat", label: "Vat" },
    { def: "total", label: "Total" },
    { def: "advancePaymentAmount", label: "Advance Payment Amount" },
    { def: "totalGrnAdjustmentAmount", label: "Total GRN Adjustment Amount" },
    { def: "netPayable", label: "Net Payable" },
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
    private purchaseInvoiceService: PurchaseInvoiceService,
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
    this.getPurchaseInvoices(this.purchaseInvoiceRequest);
    this.getPendingCheckedCount();
    this.getCurrentFinancialYearId();
    this.subscribeToFormChanges();
  }

  openExpansionPanel() {
    this.panelOpenState = true;
    this.getAllSuppliers();
    this.getAllPurchaseInvoiceStatuses();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
      fromDate: [null],
      toDate: [null],
      grnno: [""],
      supplierId: [null],
      isImportPurchase: [false],
      lcNumber: [""],
      purchaseInvoiceStatus: [null],
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
      purchaseInvoiceNo: [true],
      invoiceDate: [true],
      grnno: [true],
      ponumber: [false],
      supplierPaymentCode: [true],
      storeId: [true],
      supplierId: [true],
      supplierInvoiceNo: [false],
      supplierInvoiceDate: [false],
      paymentTermInDays: [false],
      isImportPurchase: [false],
      proformaInvoiceNo: [false],
      lcNumber: [false],
      exchangeRate: [false],
      currencyId: [false],
      importPurchaseIncoTerm: [false],
      importPurchasePaymentTerm: [false],
      billOfEntryNo: [false],
      billOfEntryDate: [false],
      portOfLoading: [false],
      portOfDestination: [false],
      subtotal: [true],
      discount: [false],
      totalVat: [false],
      total: [true],
      advancePaymentAmount: [true],
      totalGrnAdjustmentAmount: [true],
      netPayable: [true],
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

  clearInput(evt: any): void {
    evt.stopPropagation();
    this.searchForm?.get("supplierId").setValue(null);
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

  getPurchaseInvoices(requestBody): void {
    this.purchaseInvoiceService
      .getPurchaseInvoices(requestBody)
      .subscribe((res) => {
        this.dataSource = new MatTableDataSource<PurchaseInvoiceResponseDTO>(
          res?.data?.item1
        );
        this.reportAggregates(requestBody);
        this.totalCount = res?.data?.item2;
        this.loading = false;
      });
  }

  reportAggregates(requestBody: PurchaseInvoiceSearchRequestDTO): void {
    this.purchaseInvoiceService
      .reportAggregates(requestBody)
      .subscribe((res) => {
        this.reportAggregator = res.data;
      });
  }

  getPendingCheckedCount(): void {
    this.purchaseInvoiceService.getPendingCheckedCount().subscribe((res) => {
      this.pendingCheckedModel = res.data;
    });
  }

  getPending() {
    this.loading = true;
    this.purchaseInvoiceRequest.purchaseInvoiceStatus = 1;
    this.getPurchaseInvoices(this.purchaseInvoiceRequest);
  }

  getChecked() {
    this.loading = true;
    this.purchaseInvoiceRequest.purchaseInvoiceStatus = 2;
    this.getPurchaseInvoices(this.purchaseInvoiceRequest);
  }

  getAllPurchaseInvoiceStatuses() {
    this.enumValueService.getPurchaseInvoiceStatuses().subscribe((res) => {
      this.purchaseInvoiceStatuses = res;
    });
  }

  getPurchaseInvoiceStatus(value) {
    return this.statusColorService.getPurchaseInvoiceStatus(value);
  }

  getPurchaseInvoiceStatusName(value: number) {
    return PurchaseInvoiceStatus[value];
  }

  remove(id: string): void {
    this.purchaseInvoiceService.deletePurchaseInvoice(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getPurchaseInvoices(this.purchaseInvoiceRequest);
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
    this.purchaseInvoiceRequest = new PurchaseInvoiceSearchRequestDTO();
    this.getPurchaseInvoices(this.purchaseInvoiceRequest);
  }

  onSearch() {
    this.loading = true;
    this.purchaseInvoiceRequest = {
      ...this.purchaseInvoiceRequest,
      ...this.searchForm.value,
    };
    this.getPurchaseInvoices(this.purchaseInvoiceRequest);
  }

  private path = {
    addNew: "purchase/purchase-invoice/add-new",
    edit: "purchase/purchase-invoice",
  };

  navigateToAddNew() {
    this.router.navigate([this.path.addNew]);
  }

  navigateToView(item) {
    this.router.navigate([this.path.edit, item.id]);
    // this.localStorageService.setItem(item.id, item);
  }

  onPageChange(pageEvent) {
    this.purchaseInvoiceRequest.page = pageEvent.pageIndex;
    this.purchaseInvoiceRequest.rowsPerPage = pageEvent.pageSize;
    this.getPurchaseInvoices(this.purchaseInvoiceRequest);
  }

  printPdf(id) {
    this.http
      .get(environment.apiURL + "/ReportPurchaseInvoice/" + id, {
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

  printPurchaseInvoicePdf(name: string) {
    this.http
      .post(
        environment.apiURL + "/purchaseInvoice/" + name,
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
