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
import { SaleInvoiceStatus } from "app/shared/enums/saleInvoiceStatus";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { Customer } from "app/views/configuration/models/customer/customer.model";
import { StoreRequest } from "app/views/configuration/models/store/store-request.model";
import { Store } from "app/views/configuration/models/store/store.model";
import { CustomerService } from "app/views/configuration/services/customer.service";
import { StoreService } from "app/views/configuration/services/store.service";
import { PendingCheckedCount } from "app/views/purchase/models/pending-checked-count";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { Observable, merge, of } from "rxjs";
import { SaleInvoiceAggregatorModel } from "../../models/sale-invoice/sale-invoice-aggregator.model";
import { SaleInvoiceResponseDTO } from "../../models/sale-invoice/sale-invoice-response-dto.model";
import { SaleInvoiceSearchRequestDTO } from "../../models/sale-invoice/sale-invoice-search-request-dto.model";
import { SaleInvoiceService } from "../../services/sale-invoice.service";

@Component({
  selector: "app-sale-invoice",
  templateUrl: "./sale-invoice.component.html",
  styleUrls: ["./sale-invoice.component.scss"],
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
export class SaleInvoiceComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  stores: Store[];
  customers: Customer[];
  filterCustomers: Customer[];
  viewColumnForm: FormGroup;
  expandedElement: any;
  dataSource: MatTableDataSource<SaleInvoiceResponseDTO>;
  reportAggregator = new SaleInvoiceAggregatorModel();
  totalCount: number;
  saleInvoiceStatuses: ENUM[];
  saleInvoiceRequest = new SaleInvoiceSearchRequestDTO();
  pendingCheckedModel: PendingCheckedCount;
  displayedColumns$: Observable<string[]>;
  pageSizeOptions: [] = Page_Size_Options;
  currentFinancialYearId: string;
  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  //  * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "expand", label: "Expand" },
    { def: "actions", label: "Actions" },
    { def: "invoiceNo", label: "Sale Invoice No." },
    { def: "saleOrderNo", label: "Sale Order No." },
    { def: "deliveryNoteNo", label: "Delivery Note No." },
    { def: "invoiceDate", label: "Invoice Date" },
    { def: "storeId", label: "Store" },
    { def: "paymentTerm", label: "Payment Term" },
    { def: "customerId", label: "Customer" },
    { def: "bankDetails", label: "Bank Details" },
    { def: "termAndCondition", label: "Term And Condition" },
    { def: "subtotal", label: "Subtotal" },
    {
      def: "totalPercentageDiscountAmount",
      label: "Total Percentage Discount Amount",
    },
    { def: "discount", label: "Discount" },
    { def: "offerDiscount", label: "Offer Discount" },
    { def: "otherDiscount", label: "Other Discount" },
    { def: "total", label: "Net Sale" },
    { def: "transportationCost", label: "Transp Cost" },
    { def: "depoCharge", label: "Depo Charge" },
    { def: "netTotal", label: "Total Payable" },
    { def: "paid", label: "Paid" },
    { def: "referenceNo", label: "Reference No" },
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
    private saleInvoiceService: SaleInvoiceService,
    private customerService: CustomerService,
    private storeService: StoreService,
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
    this.getSaleInvoices();
    this.getCurrentFinancialYearId();
    this.getPendingCheckedCount();
    this.subscribeToFormChanges();
  }

  openExpansionPanel() {
    this.panelOpenState = true;
    this.getAllSaleInvoiceStatuses();
    this.getAllCustomers();
    this.getAllStores();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
      fromDate: [null],
      toDate: [null],
      storeId: [null],
      customerId: [null],
      saleInvoiceStatus: [null],
    });

    /**
     * * this code will be executed for instant changes of SearchForm's field
     */
    this.searchForm.valueChanges.subscribe((value) => {
      // Convert empty string to null
      if (value.customerId === "") {
        this.searchForm
          .get("customerId")
          ?.patchValue(null, { emitEvent: false });
      }
    });

    this.viewColumnForm = this.fb.group({
      expand: [true],
      invoiceNo: [true],
      saleOrderNo: [true],
      deliveryNoteNo: [true],
      invoiceDate: [true],
      storeId: [true],
      paymentTerm: [false],
      customerId: [true],
      referenceNo: [true],
      bankDetails: [false],
      termAndCondition: [false],
      subtotal: [false],
      totalPercentageDiscountAmount: [false],
      discount: [false],
      offerDiscount: [false],
      otherDiscount: [false],
      total: [false],
      paid: [false],
      transportationCost: [false],
      depoCharge: [false],
      netTotal: [true],
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

  clearInput(evt: any): void {
    evt.stopPropagation();
    this.searchForm?.get("customerId").setValue(null);
  }

  handleCustomerSearch(event: any): void {
    const name = event.target?.name;
    if (name === "customerId") {
      const term = this.searchForm.get("customerId");
      this.filterCustomer(term.value || "");
    }
  }

  private filterCustomer(value: string) {
    const filterValue = value.trim().toLowerCase();
    this.filterCustomers = this.customers?.filter(
      (option) =>
        option.name.toLowerCase().includes(filterValue) ||
        option.code?.slice(-4).toLowerCase().includes(filterValue)
    );
  }

  getCustomerName(customerId: string) {
    if (!customerId) {
      return;
    }
    const customerAccount = this.customers?.find(
      (customer) => customer?.id === customerId
    );
    return customerAccount?.name;
  }

  getAllCustomers() {
    this.customerService.getAllCustomers().subscribe((res) => {
      this.filterCustomers = this.customers = res.data?.item1;
    });
  }

  getAllStores() {
    let storeRequest = new StoreRequest();
    storeRequest.inventoryTypeId = Inventory_Type_Id_Finished_Goods;
    storeRequest.page = -1;
    this.storeService.getStores(storeRequest).subscribe((res) => {
      this.stores = res?.data.item1;
    });
  }

  getSaleInvoices(): void {
    this.saleInvoiceService
      .getSaleInvoices(this.saleInvoiceRequest)
      .subscribe((res) => {
        this.dataSource = new MatTableDataSource<SaleInvoiceResponseDTO>(
          res?.data?.item1
        );
        this.reportAggregates(this.saleInvoiceRequest);
        this.totalCount = res?.data?.item2;
        this.loading = false;
      });
  }

  reportAggregates(requestBody: SaleInvoiceSearchRequestDTO): void {
    this.saleInvoiceService.reportAggregates(requestBody).subscribe((res) => {
      this.reportAggregator = res.data;
    });
  }

  getPendingCheckedCount(): void {
    this.saleInvoiceService.getPendingCheckedCount().subscribe((res) => {
      this.pendingCheckedModel = res.data;
    });
  }

  getPending() {
    this.loading = true;
    this.saleInvoiceRequest.saleInvoiceStatus = 1;
    this.getSaleInvoices();
  }

  getChecked() {
    this.loading = true;
    this.saleInvoiceRequest.saleInvoiceStatus = 2;
    this.getSaleInvoices();
  }

  getAllSaleInvoiceStatuses() {
    this.enumValueService.getSaleInvoiceStatuses().subscribe((res) => {
      this.saleInvoiceStatuses = res;
    });
  }

  getSaleInvoiceStatus(value) {
    return this.statusColorService.getSaleInvoiceStatus(value);
  }

  getSaleInvoiceStatusName(value: number) {
    return SaleInvoiceStatus[value];
  }

  remove(id: string): void {
    this.saleInvoiceService.deleteSaleInvoice(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getSaleInvoices();
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
    this.saleInvoiceRequest = new SaleInvoiceSearchRequestDTO();
    this.getSaleInvoices();
  }

  onSearch() {
    this.loading = true;
    this.saleInvoiceRequest = {
      ...this.saleInvoiceRequest,
      ...this.searchForm.value,
    };
    this.getSaleInvoices();
  }

  private path = {
    addNew: "sales/sale-invoice/add-new",
    edit: "sales/sale-invoice",
    view: "sales/sale-invoice/view",
  };

  navigateToAddNew() {
    this.router.navigate([this.path.addNew]);
  }

  navigateToView(item) {
    this.router.navigate([this.path.edit, item.id]);
    // this.localStorageService.setItem(item.id, item);
  }

  onPageChange(pageEvent) {
    this.saleInvoiceRequest.page = pageEvent.pageIndex;
    this.saleInvoiceRequest.rowsPerPage = pageEvent.pageSize;
    this.getSaleInvoices();
  }

  printPdf(id) {
    this.http
      .get(environment.apiURL + "/ReportSaleInvoiceA5/" + id, {
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

  printVatPdf(name: string, id: string) {
    this.http
      .get(environment.apiURL + `/${name}/` + id, {
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

  printSaleInvoicePdf(name: string) {
    this.http
      .post(
        environment.apiURL + "/SaleInvoice/" + name,
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
