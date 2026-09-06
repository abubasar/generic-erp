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
import { SaleOrderStatus } from "app/shared/enums/saleOrderStatus";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { Customer } from "app/views/configuration/models/customer/customer.model";
import { FinancialYear } from "app/views/configuration/models/financial-year/financial-year.model";
import { StoreRequest } from "app/views/configuration/models/store/store-request.model";
import { Store } from "app/views/configuration/models/store/store.model";
import { CustomerService } from "app/views/configuration/services/customer.service";
import { StoreService } from "app/views/configuration/services/store.service";
import { PendingCheckedCount } from "app/views/purchase/models/pending-checked-count";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { Observable, merge, of } from "rxjs";
import { SaleOrderAggregatorModel } from "../../models/sale-order/sale-order-aggregator.model";
import { SaleOrderResponseDTO } from "../../models/sale-order/sale-order-response-dto.model";
import { SaleOrderSearchRequestDTO } from "../../models/sale-order/sale-order-search-request-dto.model";
import { SaleOrderService } from "../../services/sale-order.service";

@Component({
  selector: "app-sales-order",
  templateUrl: "./sales-order.component.html",
  styleUrls: ["./sales-order.component.scss"],
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
export class SalesOrderComponent implements OnInit {
  loading: boolean = true;
  isLoadingCloseSOButton: boolean = false;
  panelOpenState: boolean;
  searchForm: FormGroup;
  stores: Store[];
  customers: Customer[];
  filterCustomers: Customer[];
  viewColumnForm: FormGroup;
  expandedElement: any;
  dataSource: MatTableDataSource<SaleOrderResponseDTO>;
  reportAggregator = new SaleOrderAggregatorModel();
  totalCount: number;
  financialYears: FinancialYear[];
  saleOrderRequest = new SaleOrderSearchRequestDTO();
  saleOrderStatuses: ENUM[];
  pendingCheckedModel: PendingCheckedCount;
  displayedColumns$: Observable<string[]>;
  pageSizeOptions: [] = Page_Size_Options;
  currentFinancialYearId: string;

  private path = {
    addNew: "sales/sales-order/add-new",
    edit: "sales/sales-order",
    view: "sales/sales-order/view",
  };

  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  //  * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "expand", label: "Expand" },
    { def: "actions", label: "Actions" },
    { def: "saleOrderNo", label: "Sale Order No." },
    { def: "quotationNo", label: "Quotation No." },
    { def: "transport", label: "Transport" },
    { def: "orderDate", label: "Order Date" },
    { def: "paymentTerm", label: "Payment Term" },
    { def: "customerId", label: "Customer" },
    { def: "creditLimit", label: "Credit Limit" },
    { def: "limitAvailed", label: "Limit Availed" },
    { def: "deliveryDate", label: "Delivery Date" },
    { def: "storeId", label: "Store" },
    { def: "subtotal", label: "Subtotal" },
    { def: "discount", label: "Discount" },
    {
      def: "totalPercentageDiscountAmount",
      label: "Total Percentage Discount Amount",
    },
    { def: "offerDiscount", label: "Offer Discount" },
    { def: "otherDiscount", label: "Other Discount" },
    { def: "total", label: "Total" },
    { def: "transportationCost", label: "Transportation Cost" },
    { def: "depoCharge", label: "Depo Charge" },
    { def: "netTotal", label: "Net Total" },
    { def: "referenceNo", label: "Reference No." },
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
    private saleOrderService: SaleOrderService,
    private customerService: CustomerService,
    private storeService: StoreService,
    private jwtAuth: JwtAuthService,
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
    this.getSaleOrders(this.saleOrderRequest);
    this.getCurrentFinancialYearId();
    this.subscribeToFormChanges();
  }

  openExpansionPanel() {
    this.panelOpenState = true;
    this.getAllSaleOrderStatuses();
    this.getAllCustomers();
    this.getAllStores();
    // this.getAllFinancialYears();
  }

  private initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
      fromDate: [null],
      toDate: [null],
      storeId: [null],
      customerId: [null],
      saleOrderNo: [""],
      quotationNo: [""],
      saleOrderStatus: [null],
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
      saleOrderNo: [true],
      quotationNo: [false],
      transport: [false],
      orderDate: [true],
      paymentTerm: [false],
      customerId: [true],
      creditLimit: [false],
      limitAvailed: [false],
      deliveryDate: [false],
      storeId: [true],
      subtotal: [false],
      discount: [false],
      totalPercentageDiscountAmount: [false],
      offerDiscount: [false],
      otherDiscount: [false],
      total: [false],
      transportationCost: [false],
      depoCharge: [false],
      netTotal: [true],
      referenceNo: [true],
      remark: [false],
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

  private getCurrentFinancialYearId() {
    this.jwtAuth.userProfile.subscribe((res: UserProfile) => {
      this.currentFinancialYearId = res.fyid;
    });
  }

  getAllSaleOrderStatuses() {
    this.enumValueService.getSaleOrderStatuses().subscribe((res) => {
      this.saleOrderStatuses = res;
    });
  }

  getSaleOrderStatus(value: number) {
    return this.statusColorService.getSaleOrderStatus(value);
  }

  getSaleOrderStatusName(value: number) {
    return SaleOrderStatus[value];
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
    storeRequest.page = -1;
    storeRequest.inventoryTypeId = Inventory_Type_Id_Finished_Goods;
    this.storeService.getStores(storeRequest).subscribe((res) => {
      this.stores = res?.data.item1;
    });
  }

  getSaleOrders(requestBody: SaleOrderSearchRequestDTO): void {
    this.saleOrderService.getSaleOrders(requestBody).subscribe((res) => {
      this.dataSource = new MatTableDataSource<SaleOrderResponseDTO>(
        res?.data?.item1
      );
      this.reportAggregates(requestBody);
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  reportAggregates(requestBody: SaleOrderSearchRequestDTO): void {
    this.saleOrderService.reportAggregates(requestBody).subscribe((res) => {
      this.reportAggregator = res.data;
    });
  }

  getPendingCheckedCount(): void {
    this.saleOrderService.getPendingCheckedCount().subscribe((res) => {
      this.pendingCheckedModel = res.data;
    });
  }

  getPending() {
    this.loading = true;
    this.saleOrderRequest.saleOrderStatuses = [1];
    this.getSaleOrders(this.saleOrderRequest);
  }

  getChecked() {
    this.loading = true;
    this.saleOrderRequest.saleOrderStatuses = [2];
    this.getSaleOrders(this.saleOrderRequest);
  }

  close(id): void {
    this.isLoadingCloseSOButton = true;
    this.saleOrderService.closeSaleOrder(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getSaleOrders(this.saleOrderRequest);
        this.toastr.info(res?.message);
        this.isLoadingCloseSOButton = false;
      } else {
        this.toastr.error(res?.message);
        this.isLoadingCloseSOButton = false;
      }
    });
  }

  remove(id: string): void {
    this.saleOrderService.deleteSaleOrder(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getSaleOrders(this.saleOrderRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  // Function to confirm deletion before calling the remove function
  confirmClose(id: string, code: string = "") {
    // Open a confirmation dialog with a custom message and callback function
    const data: ConfirmDialogModel = {
      title: "Confirm Close!",
      message: `Proceeding with the closure of this Sale Order. Once confirmed, it's permanent!`,
    };
    this.confirmDialogService.confirmDialog(
      id,
      this.close.bind(this),
      data.message,
      data.title
    );
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
    this.saleOrderRequest = new SaleOrderSearchRequestDTO();
    this.getSaleOrders(this.saleOrderRequest);
  }

  onSearch() {
    this.loading = true;
    this.saleOrderRequest = {
      ...this.saleOrderRequest,
      ...this.searchForm.value,
    };
    this.getSaleOrders(this.saleOrderRequest);
  }

  navigateToAddNew() {
    this.router.navigate([this.path.addNew]);
  }

  navigateToView(item) {
    this.router.navigate([this.path.edit, item.id]);
  }

  onPageChange(pageEvent) {
    this.saleOrderRequest.page = pageEvent.pageIndex;
    this.saleOrderRequest.rowsPerPage = pageEvent.pageSize;
    this.getSaleOrders(this.saleOrderRequest);
  }

  printPdf(id) {
    this.http
      .get(environment.apiURL + "/ReportSaleOrder/" + id, {
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

  printSaleOrderPdf(name: string) {
    this.http
      .post(environment.apiURL + "/SaleOrder/" + name, this.searchForm.value, {
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
