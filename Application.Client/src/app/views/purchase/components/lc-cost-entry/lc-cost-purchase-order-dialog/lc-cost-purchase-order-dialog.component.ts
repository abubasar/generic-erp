import {
  animate,
  state,
  style,
  transition,
  trigger,
} from "@angular/animations";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { MatTableDataSource } from "@angular/material/table";
import { PaymentMode } from "app/shared/enums/paymentMode";
import { PurchaseOrderStatus } from "app/shared/enums/purchaseOrderStatus";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { SupplierService } from "app/views/configuration/services/supplier.service";
import { PurchaseOrderResponseDTO } from "app/views/purchase/models/purchase-order/purchase-order-response-dto.model";
import { PurchaseOrderSearchRequestDTO } from "app/views/purchase/models/purchase-order/purchase-order-search-request-dto.model";
import { PurchaseOrderService } from "app/views/purchase/services/purchase-order.service";

@Component({
  selector: "app-lc-cost-purchase-order-dialog",
  templateUrl: "./lc-cost-purchase-order-dialog.component.html",
  styleUrls: ["./lc-cost-purchase-order-dialog.component.scss"],
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
export class LcCostPurchaseOrderDialogComponent implements OnInit {
  loading: boolean = true;
  selectedPurchaseOrder: PurchaseOrderResponseDTO = null;
  purchaseOrders: PurchaseOrderResponseDTO[];
  purchaseOrdersNo: PurchaseOrderResponseDTO[];
  purchaseOrderRequest = new PurchaseOrderSearchRequestDTO();

  panelOpenState: false;
  searchForm: FormGroup;
  suppliers: PurchaseOrderResponseDTO[];
  filterSuppliers: PurchaseOrderResponseDTO[];
  expandedElement: any;
  expandedRow: any;
  totalCount: number;
  purchaseOrderStatuses: ENUM[];

  displayedColumns: string[] = [
    "select", // its need for single selection
    "expand",
    "ponumber",
    "lcNumber",
    // "quotationNo",
    // "requisitionNo",
    // "storeId",
    "supplierId",
    "deliveryDate",
    // "transport",
    // "paymentTermInDays",
    // "deliveryTermInDays",
    // "paymentMode",
    // "subtotal",
    // "discount",
    "total",
    "status",
    // "remark",
  ];
  dataSource: MatTableDataSource<PurchaseOrderResponseDTO>;
  constructor(
    public dateFormatService: DateTimeFormatService,
    public statusColorService: StatusColorService,
    private purchaseOrderService: PurchaseOrderService,
    private supplierService: SupplierService,
    private enumValueService: EnumValueService,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    this.getPurchaseOrders();
    this.getPurchaseOrdersNo();
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      ponumber: [""],
      lcNumber: [""],
      supplierId: [null],
    });

    /**
     * * this code will be executed for instant changes of SearchForm's field
     */
    this.searchForm.valueChanges.subscribe((value) => {
      // Convert empty string to null
      if (value.supplierId === "") {
        this.purchaseOrders = this.purchaseOrdersNo;
        this.searchForm
          .get("supplierId")
          ?.patchValue(null, { emitEvent: false });
      }
    });
  }

  clearInput(evt: any): void {
    evt.stopPropagation();
    this.searchForm?.get("supplierId").setValue(null);
    this.purchaseOrders = this.purchaseOrdersNo;
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
      option?.supplier?.name.toLowerCase().includes(filterValue)
    );
  }

  getSupplierName(supplierId: string) {
    if (!supplierId) {
      return;
    }
    const supplierAccount = this.suppliers?.find(
      (supplier) => supplier?.supplierId === supplierId
    );
    return supplierAccount?.supplier?.name;
  }

  onSelectedSupplierId(supplierId: string) {
    this.searchForm.patchValue({
      ponumber: "",
      lcNumber: "",
    });
    if (this.purchaseOrders) {
      this.purchaseOrders = this.purchaseOrdersNo?.filter(
        (x) => x.supplierId === supplierId
      );
    }
  }

  getAllPurchaseOrderStatuses() {
    this.enumValueService.getRequisitionStatuses().subscribe((res) => {
      this.purchaseOrderStatuses = res;
    });
  }

  getPurchaseOrderStatus(value) {
    return this.statusColorService.getPurchaseOrderStatus(value);
  }

  getPaymentModeName(value: number) {
    return PaymentMode[value];
  }

  getPurchaseOrderStatusName(value: number) {
    return PurchaseOrderStatus[value];
  }

  getPurchaseOrders(): void {
    this.purchaseOrderRequest.page = -1;
    this.purchaseOrderRequest.purchaseOrderStatus = 3;
    this.purchaseOrderRequest.isImportPurchase = true;
    this.purchaseOrderService
      .getPurchaseOrders(this.purchaseOrderRequest)
      .subscribe((res) => {
        this.dataSource = new MatTableDataSource<PurchaseOrderResponseDTO>(
          res?.data?.item1
        );
        this.totalCount = res?.data?.item2;
        this.loading = false;
      });
  }

  getPurchaseOrdersNo(): void {
    this.purchaseOrderRequest.page = -1;
    this.purchaseOrderRequest.purchaseOrderStatus = 3;
    this.purchaseOrderRequest.isImportPurchase = true;
    this.purchaseOrderService
      .getPurchaseOrders(this.purchaseOrderRequest)
      .subscribe((res) => {
        const seen = new Set();
        const uniqueSuppliersPO = res?.data?.item1.filter((po) => {
          const isDuplicate = seen.has(po.supplierId);
          seen.add(po.supplierId);
          return !isDuplicate;
        });
        this.purchaseOrders = this.purchaseOrdersNo = res?.data?.item1;
        this.filterSuppliers = this.suppliers = uniqueSuppliersPO;
      });
  }

  onPONoChange(): void {
    const term = this.searchForm.get("ponumber");
    this._filterPONo(term.value || "");
  }

  private _filterPONo(value: string) {
    const filterValue = value.toLowerCase().trim();
    const supplierId = this.searchForm.value.supplierId;
    if (supplierId) {
      this.purchaseOrders = this.purchaseOrdersNo?.filter(
        (x) =>
          x.ponumber.toLocaleLowerCase().includes(filterValue) &&
          x.supplierId === supplierId
      );
    } else {
      this.purchaseOrders = this.purchaseOrdersNo?.filter((x) =>
        x.ponumber.toLocaleLowerCase().includes(filterValue)
      );
    }
  }

  onLcNoChange(): void {
    const term = this.searchForm.get("lcNumber");
    this.filterLcNo(term.value || "");
  }

  private filterLcNo(value: string) {
    const filterValue = value.toLowerCase().trim();
    const supplierId = this.searchForm.value.supplierId;
    if (supplierId) {
      this.purchaseOrders = this.purchaseOrdersNo?.filter(
        (x) =>
          x.lcNumber.toLocaleLowerCase().includes(filterValue) &&
          x.supplierId === supplierId
      );
    } else {
      this.purchaseOrders = this.purchaseOrdersNo?.filter((x) =>
        x.lcNumber.toLocaleLowerCase().includes(filterValue)
      );
    }
  }

  onSearch() {
    this.loading = true;
    this.purchaseOrderRequest = {
      ...this.purchaseOrderRequest,
      ...this.searchForm.value,
    };
    this.getPurchaseOrders();
  }

  handleRadioChange($event) {
    this.selectedPurchaseOrder = $event.value;
  }
}
