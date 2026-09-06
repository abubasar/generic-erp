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
import { PaymentMode } from "app/shared/enums/paymentMode";
import { PurchaseOrderStatus } from "app/shared/enums/purchaseOrderStatus";
import { Transport } from "app/shared/enums/transport";
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
import { PurchaseOrderAggregatorModel } from "../../models/purchase-order/purchase-order-aggregator.model";
import { PurchaseOrderResponseDTO } from "../../models/purchase-order/purchase-order-response-dto.model";
import { PurchaseOrderSearchRequestDTO } from "../../models/purchase-order/purchase-order-search-request-dto.model";
import { PurchaseOrderService } from "../../services/purchase-order.service";
import { SupplierTransactionsAgainstPOComponent } from "./supplier-transactions-against-po/supplier-transactions-against-po.component";

@Component({
  selector: "app-purchase-order",
  templateUrl: "./purchase-order.component.html",
  styleUrls: ["./purchase-order.component.scss"],
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
export class PurchaseOrderComponent implements OnInit {
  loading: boolean = true;
  isLoadingClosePOButton: boolean = false;
  panelOpenState: boolean;
  searchForm: FormGroup;
  suppliers: Supplier[];
  filterSuppliers: Supplier[];
  viewColumnForm: FormGroup;
  expandedElement: any;
  dataSource: MatTableDataSource<PurchaseOrderResponseDTO>;
  reportAggregator = new PurchaseOrderAggregatorModel();
  totalCount: number;
  purchaseOrderStatuses: ENUM[];
  currencies: Currency[];
  purchaseOrderRequest = new PurchaseOrderSearchRequestDTO();
  pendingCheckedModel: PendingCheckedCount;
  displayedColumns$: Observable<string[]>;
  pageSizeOptions: [] = Page_Size_Options;
  currentFinancialYearId: string;

  private path = {
    addNew: "purchase/purchase-order/add-new",
    edit: "purchase/purchase-order",
    view: "purchase/purchase-order/view",
  };

  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  //  * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "expand", label: "Expand" },
    { def: "actions", label: "Actions" },
    { def: "ponumber", label: "PO Number" },
    { def: "quotationNo", label: "Quotation No." },
    { def: "requisitionNo", label: "Requisition No." },
    { def: "referenceNo", label: "Reference No." },
    { def: "podate", label: "PO Date" },
    { def: "storeId", label: "Store" },
    { def: "supplierId", label: "Supplier" },
    { def: "deliveryPlaceId", label: "Delivery Place" },
    { def: "deliveryDate", label: "Delivery Date" },
    { def: "productOrigin", label: "Product Origin" },
    { def: "packagingType", label: "Packaging Type" },
    { def: "expiryTime", label: "Expiry Time" },
    { def: "paymentMethodId", label: "Payment Method" },
    { def: "transport", label: "Transport" },
    { def: "paymentTermInDays", label: "Payment Term In Days" },
    { def: "deliveryTermInDays", label: "Delivery Term In Days" },
    { def: "paymentMode", label: "Payment Mode" },
    { def: "isImportPurchase", label: "Is Import Purchase" },
    { def: "proformaInvoiceNo", label: "Proforma Invoice No" },
    { def: "lcNumber", label: "LC Number" },
    { def: "exchangeRate", label: "Exchange Rate" },
    { def: "currencyId", label: "Currency" },
    { def: "importPurchaseIncoTerm", label: "Import Purchase Inco Term" },
    { def: "importPurchasePaymentTerm", label: "Import Purchase Payment Term" },
    { def: "portOfLoading", label: "Port of Loading" },
    { def: "portOfDestination", label: "Port of Destination" },
    // {
    //   def: "subtotal",
    //   label: "Subtotal",
    // },
    // {
    //   def: "discount",
    //   label: "Discount",
    // },
    { def: "total", label: "Total" },
    { def: "weightVariance", label: "Weight Variance" },
    { def: "status", label: "Status" },
    { def: "remark", label: "Remark" },
    { def: "termAndCondition", label: "Terms & Conditions" },
    { def: "isPartialDelivery", label: "Partial Delivery" },
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
    public currencyService: CurrencyService,
    private purchaseOrderService: PurchaseOrderService,
    private supplierService: SupplierService,
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
    this.getAllCurrencies();
    this.getPurchaseOrders(this.purchaseOrderRequest);
    this.getPendingCheckedCount();
    this.getCurrentFinancialYearId();
    this.subscribeToFormChanges();
  }

  openExpansionPanel() {
    this.panelOpenState = true;
    this.getAllSuppliers();
    this.getAllPurchaseOrderStatuses();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
      fromDate: [null],
      toDate: [null],
      supplierId: [null],
      isImportPurchase: [false],
      lcNumber: [""],
      purchaseOrderStatus: [null],
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
      ponumber: [true],
      quotationNo: [true],
      requisitionNo: [false],
      referenceNo: [false],
      podate: [false],
      storeId: [true],
      supplierId: [true],
      deliveryPlaceId: [false],
      deliveryDate: [false],
      productOrigin: [false],
      packagingType: [false],
      expiryTime: [false],
      paymentMethodId: [false],
      transport: [false],
      paymentMode: [false],
      paymentTermInDays: [false],
      deliveryTermInDays: [false],
      isImportPurchase: [false],
      proformaInvoiceNo: [false],
      lcNumber: [false],
      exchangeRate: [false],
      currencyId: [false],
      importPurchaseIncoTerm: [false],
      importPurchasePaymentTerm: [false],
      portOfLoading: [false],
      portOfDestination: [false],
      // subtotal: [true],
      // discount: [false],
      total: [true],
      weightVariance: [false],
      status: [true],
      remark: [false],
      termAndCondition: [false],
      isPartialDelivery: [false],
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

  getPurchaseOrders(requestBody): void {
    this.purchaseOrderService
      .getPurchaseOrders(requestBody)
      .subscribe((res) => {
        this.dataSource = new MatTableDataSource<PurchaseOrderResponseDTO>(
          res?.data?.item1
        );
        this.reportAggregates(requestBody);
        this.totalCount = res?.data?.item2;
        this.loading = false;
      });
  }

  reportAggregates(requestBody: PurchaseOrderSearchRequestDTO): void {
    this.purchaseOrderService.reportAggregates(requestBody).subscribe((res) => {
      this.reportAggregator = res.data;
    });
  }

  getPendingCheckedCount(): void {
    this.purchaseOrderService.getPendingCheckedCount().subscribe((res) => {
      this.pendingCheckedModel = res.data;
    });
  }

  getPending() {
    this.loading = true;
    this.purchaseOrderRequest.purchaseOrderStatuses = [1];
    this.getPurchaseOrders(this.purchaseOrderRequest);
  }

  getChecked() {
    this.loading = true;
    this.purchaseOrderRequest.purchaseOrderStatuses = [2];
    this.getPurchaseOrders(this.purchaseOrderRequest);
  }

  getAllPurchaseOrderStatuses() {
    this.enumValueService.getPurchaseOrderStatuses().subscribe((res) => {
      this.purchaseOrderStatuses = res;
    });
  }

  getPurchaseOrderStatus(value) {
    return this.statusColorService.getPurchaseOrderStatus(value);
  }

  getPurchaseOrderStatusName(value: number) {
    return PurchaseOrderStatus[value];
  }

  getTransportName(value: number) {
    return Transport[value];
  }

  getPaymentModeName(value: number) {
    return PaymentMode[value];
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

  close(id): void {
    this.isLoadingClosePOButton = true;
    this.purchaseOrderService.closePurchaseOrder(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getPurchaseOrders(this.purchaseOrderRequest);
        this.toastr.info(res?.message);
        this.isLoadingClosePOButton = false;
      } else {
        this.toastr.error(res?.message);
        this.isLoadingClosePOButton = false;
      }
    });
  }

  remove(id): void {
    this.purchaseOrderService.deletePurchaseOrder(id).subscribe((res) => {
      console.log("remove id res:", res);
      if (res?.succeeded) {
        this.getPurchaseOrders(this.purchaseOrderRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  openSupplierTransactionAgainstPODialog(data: PurchaseOrderResponseDTO) {
    const dialogRef = this.dialog.open(SupplierTransactionsAgainstPOComponent, {
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

  // Function to confirm deletion before calling the remove function
  confirmClose(id: string, code: string = "") {
    // Open a confirmation dialog with a custom message and callback function
    const data: ConfirmDialogModel = {
      title: "Confirm Close!",
      message: `Proceeding with the closure of this Purchase Order. Once confirmed, it's permanent!`,
    };
    this.confirmDialogService.confirmDialog(
      id,
      this.close.bind(this),
      data.message,
      data.title
    );
  }

  reload() {
    this.searchForm.reset();
    this.loading = true;
    this.purchaseOrderRequest = new PurchaseOrderSearchRequestDTO();
    this.getPurchaseOrders(this.purchaseOrderRequest);
  }

  onSearch() {
    this.loading = true;
    this.purchaseOrderRequest = {
      ...this.purchaseOrderRequest,
      ...this.searchForm.value,
    };
    this.getPurchaseOrders(this.purchaseOrderRequest);
  }

  navigateToAddNew() {
    this.router.navigate([this.path.addNew]);
  }

  navigateToView(item) {
    this.router.navigate([this.path.edit, item.id]);
    // this.localStorageService.setItem(item.id, item);
  }

  onPageChange(pageEvent) {
    this.purchaseOrderRequest.page = pageEvent.pageIndex;
    this.purchaseOrderRequest.rowsPerPage = pageEvent.pageSize;
    this.getPurchaseOrders(this.purchaseOrderRequest);
  }

  printPdf(id) {
    this.http
      .get(environment.apiURL + "/PurchaseOrder/print/" + id, {
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

  printLcCostPdf(id) {
    this.http
      .get(
        environment.apiURL +
          "/LCCostEntry/lc-cost-entry-against-po-print/" +
          id +
          "/1",
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

  printAllPurchasePdf(name: string) {
    this.http
      .post(
        environment.apiURL + "/PurchaseOrder/" + name,
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
