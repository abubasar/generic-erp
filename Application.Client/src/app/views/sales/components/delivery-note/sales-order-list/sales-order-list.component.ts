import {
  animate,
  state,
  style,
  transition,
  trigger,
} from "@angular/animations";
import { Component, Inject, OnInit, ViewChild } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { MAT_DIALOG_DATA, MatDialog } from "@angular/material/dialog";
import { MatMenuTrigger } from "@angular/material/menu";
import { MatTableDataSource } from "@angular/material/table";
import { SaleOrderStatus } from "app/shared/enums/saleOrderStatus";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { FinancialYear } from "app/views/configuration/models/financial-year/financial-year.model";
import { PendingCheckedCount } from "app/views/purchase/models/pending-checked-count";
import { DeliveryNoteResponseDTO } from "app/views/sales/models/delivery-note/delivery-note-response-dto.model";
import { SaleOrderResponseDTO } from "app/views/sales/models/sale-order/sale-order-response-dto.model";
import { SaleOrderSearchRequestDTO } from "app/views/sales/models/sale-order/sale-order-search-request-dto.model";
import { SaleOrderService } from "app/views/sales/services/sale-order.service";
import { ToastrService } from "ngx-toastr";
import { Observable, merge, of } from "rxjs";

@Component({
  selector: "app-sales-order-list",
  templateUrl: "./sales-order-list.component.html",
  styleUrls: ["./sales-order-list.component.scss"],
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
export class SalesOrderListComponent implements OnInit {
  loading: boolean = true;
  selectedSaleOrder: SaleOrderResponseDTO = null;
  saleOrders: SaleOrderResponseDTO[];
  saleOrdersNo: SaleOrderResponseDTO[];
  customers: SaleOrderResponseDTO[];
  filterCustomers: SaleOrderResponseDTO[];
  saleOrderRequest = new SaleOrderSearchRequestDTO();

  panelOpenState: boolean;
  searchForm: FormGroup;
  viewColumnForm: FormGroup;
  expandedElement: any;
  dataSource: MatTableDataSource<SaleOrderResponseDTO>;
  financialYears: FinancialYear[];
  saleOrderStatuses: ENUM[];
  pendingCheckedModel: PendingCheckedCount;
  displayedColumns$: Observable<string[]>;

  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  //  * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "select", label: "Select" },
    { def: "expand", label: "Expand" },
    { def: "actions", label: "Actions" },
    { def: "saleOrderNo", label: "Sale Order No." },
    { def: "quotationNo", label: "Quotation No." },
    { def: "transport", label: "Transport" },
    { def: "orderDate", label: "Order Date" },
    { def: "customerId", label: "Customer" },
    { def: "creditLimit", label: "Credit Limit" },
    { def: "limitAvailed", label: "Limit Availed" },
    { def: "deliveryDate", label: "Delivery Date" },
    { def: "storeId", label: "Store" },
    { def: "subtotal", label: "Subtotal" },
    { def: "discount", label: "Discount" },
    { def: "specialDiscount", label: "Special Discount" },
    { def: "total", label: "Total" },
    { def: "transportationCost", label: "Transportation Cost" },
    { def: "depoCharge", label: "Depo Charge" },
    { def: "netTotal", label: "Net Total" },
    { def: "referenceNo", label: "Reference No." },
    { def: "status", label: "Status" },
    { def: "remark", label: "Remark" },
  ];

  constructor(
    @Inject(MAT_DIALOG_DATA)
    public deliveryNoteData: DeliveryNoteResponseDTO[],
    public dateFormatService: DateTimeFormatService,
    public statusColorService: StatusColorService,
    private saleOrderService: SaleOrderService,
    private enumValueService: EnumValueService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.getAllSaleOrderStatuses();
    this.getSaleOrders();
    this.getSaleOrderNo();
    this.subscribeToFormChanges();
  }

  private initializeForm() {
    this.searchForm = this.fb.group({
      saleOrderNo: [""],
      customerId: [null],
    });
    /**
     * * this code will be executed for instant changes of SearchForm's field
     */
    this.searchForm.valueChanges.subscribe((value) => {
      // Convert empty string to null
      if (value.customerId === "") {
        this.saleOrders = this.saleOrdersNo;
        this.searchForm
          .get("customerId")
          ?.patchValue(null, { emitEvent: false });
      }
    });
    this.viewColumnForm = this.fb.group({
      select: [true],
      expand: [true],
      saleOrderNo: [true],
      quotationNo: [false],
      transport: [false],
      orderDate: [true],
      customerId: [true],
      creditLimit: [false],
      limitAvailed: [false],
      deliveryDate: [false],
      referenceNo: [true],
      storeId: [true],
      subtotal: [false],
      discount: [false],
      specialDiscount: [false],
      total: [false],
      transportationCost: [false],
      depoCharge: [false],
      netTotal: [true],
      remark: [false],
      status: [true],
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

  getSaleOrders(): void {
    this.saleOrderRequest.page = -1;
    this.saleOrderRequest.saleOrderStatuses = [3, 4, 7];
    if (this.deliveryNoteData === null) {
      this.saleOrderService
        .getSaleOrders(this.saleOrderRequest)
        .subscribe((res) => {
          const filterData = res?.data?.item1;
          this.dataSource = new MatTableDataSource<SaleOrderResponseDTO>(
            filterData
          );
          this.loading = false;
        });
    } else {
      this.saleOrderService
        .getSaleOrders(this.saleOrderRequest)
        .subscribe((res) => {
          const filterData = res?.data?.item1.filter(
            (saleItem) =>
              !this.deliveryNoteData.some(
                (deliveryItem) =>
                  deliveryItem.saleOrderNo === saleItem.saleOrderNo
              )
          );
          this.dataSource = new MatTableDataSource<SaleOrderResponseDTO>(
            filterData
          );
          this.loading = false;
        });
    }
  }

  getSaleOrderNo(): void {
    this.saleOrderRequest.page = -1;
    this.saleOrderRequest.saleOrderStatuses = [3, 4, 7];
    if (this.deliveryNoteData === null) {
      this.saleOrderService
        .getSaleOrders(this.saleOrderRequest)
        .subscribe((res) => {
          const filterData = res?.data?.item1;
          const seen = new Set();
          const uniqueCustomersPO = filterData.filter((saleOrder) => {
            const isDuplicate = seen.has(saleOrder.customerId);
            seen.add(saleOrder.customerId);
            return !isDuplicate;
          });
          this.saleOrders = this.saleOrdersNo = filterData;
          this.filterCustomers = this.customers = uniqueCustomersPO;
        });
    } else {
      this.saleOrderService
        .getSaleOrders(this.saleOrderRequest)
        .subscribe((res) => {
          const filterData = res?.data?.item1.filter(
            (saleItem) =>
              !this.deliveryNoteData.some(
                (deliveryItem) =>
                  deliveryItem.saleOrderNo === saleItem.saleOrderNo
              )
          );
          const seen = new Set();
          const uniqueCustomersPO = filterData.filter((saleOrder) => {
            const isDuplicate = seen.has(saleOrder.customerId);
            seen.add(saleOrder.customerId);
            return !isDuplicate;
          });
          this.saleOrders = this.saleOrdersNo = filterData;
          this.filterCustomers = this.customers = uniqueCustomersPO;
        });
    }
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    if (fieldName === "saleOrderNo") {
      this.searchForm?.get("saleOrderNo").setValue("");
    }
    if (fieldName === "customerId") {
      this.searchForm?.get("customerId").setValue(null);
      this.saleOrders = this.saleOrdersNo;
    }
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
        option?.customer?.name.toLowerCase().includes(filterValue) ||
        option?.customer?.code?.slice(-4).toLowerCase().includes(filterValue)
    );
  }

  getCustomerName(customerId: string) {
    if (!customerId) {
      return;
    }
    const customerAccount = this.customers?.find(
      (customer) => customer?.customerId === customerId
    );
    return customerAccount?.customer?.name;
  }

  onSelectedCustomerId(customerId: string) {
    this.searchForm.patchValue({
      saleOrderNo: "",
    });
    if (this.saleOrders) {
      this.saleOrders = this.saleOrdersNo?.filter(
        (x) => x.customerId === customerId
      );
    }
  }

  onSaleOrderNoChange(): void {
    const term = this.searchForm.get("saleOrderNo");
    this._filterSaleOrder(term.value || "");
  }

  private _filterSaleOrder(value: string) {
    const filterValue = value.toLowerCase().trim();
    const customerId = this.searchForm.value.customerId;
    if (customerId) {
      this.saleOrders = this.saleOrdersNo?.filter(
        (x) =>
          x.saleOrderNo.toLocaleLowerCase().includes(filterValue) &&
          x.customerId === customerId
      );
    } else {
      this.saleOrders = this.saleOrdersNo?.filter((x) =>
        x.saleOrderNo.toLocaleLowerCase().includes(filterValue)
      );
    }
  }

  onSearch() {
    this.loading = true;
    this.saleOrderRequest = {
      ...this.saleOrderRequest,
      ...this.searchForm.value,
    };
    this.getSaleOrders();
  }

  handleRadioChange($event) {
    this.selectedSaleOrder = $event.value;
  }
}
