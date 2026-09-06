import {
  animate,
  state,
  style,
  transition,
  trigger,
} from "@angular/animations";
import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { MatTableDataSource } from "@angular/material/table";
import { PaymentMode } from "app/shared/enums/paymentMode";
import { PurchaseOrderStatus } from "app/shared/enums/purchaseOrderStatus";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { SnackBarService } from "app/shared/services/snack-bar.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { PurchaseOrderResponseDTO } from "app/views/purchase/models/purchase-order/purchase-order-response-dto.model";
import { PurchaseOrderSearchRequestDTO } from "app/views/purchase/models/purchase-order/purchase-order-search-request-dto.model";
import { PurchaseOrderService } from "app/views/purchase/services/purchase-order.service";

@Component({
  selector: "app-purchase-order-list",
  templateUrl: "./purchase-order-list.component.html",
  styleUrls: ["./purchase-order-list.component.scss"],
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
export class PurchaseOrderListComponent implements OnInit {
  loading: boolean = true;
  selectedPurchaseOrder: PurchaseOrderResponseDTO = null;
  purchaseOrders: PurchaseOrderResponseDTO[];
  purchaseOrdersNo: PurchaseOrderResponseDTO[];
  purchaseOrdersLcNo: PurchaseOrderResponseDTO[];
  filterSuppliers: PurchaseOrderResponseDTO[];
  filterSuppliersPO: PurchaseOrderResponseDTO[];
  purchaseOrderRequest = new PurchaseOrderSearchRequestDTO();

  panelOpenState: false;
  searchForm: FormGroup;
  expandedElement: any;
  expandedRow: any;
  totalCount: number;
  purchaseOrderStatuses: ENUM[];

  displayedColumns: string[] = [
    "select", // its need for single selection
    "expand",
    "ponumber",
    "lcNumber",
    "supplierId",
    "deliveryDate",
    "total",
    "status",
  ];
  dataSource: MatTableDataSource<PurchaseOrderResponseDTO>;
  constructor(
    @Inject(MAT_DIALOG_DATA) public isImportPurchaseData: boolean,
    public dateFormatService: DateTimeFormatService,
    public statusColorService: StatusColorService,
    private purchaseOrderService: PurchaseOrderService,
    private fb: FormBuilder,
    private snackBarService: SnackBarService
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
        this.filterSuppliers = this.filterSuppliersPO;
        this.purchaseOrdersNo = this.purchaseOrdersLcNo = this.purchaseOrders;
        this.searchForm
          .get("supplierId")
          ?.patchValue(null, { emitEvent: false });
      }
      if (value.ponumber === "") {
        if (this.searchForm.value.supplierId) {
          this.purchaseOrdersNo = this.purchaseOrders.filter(
            (po) => po.supplierId === this.searchForm.value.supplierId
          );
        } else {
          this.purchaseOrdersNo = this.purchaseOrders;
        }
        this.searchForm.get("ponumber")?.patchValue("", { emitEvent: false });
      }

      if (value.lcNumber === "") {
        if (this.searchForm.value.supplierId) {
          this.purchaseOrdersLcNo = this.purchaseOrders.filter(
            (po) => po.supplierId === this.searchForm.value.supplierId
          );
        } else {
          this.purchaseOrdersLcNo = this.purchaseOrders;
        }
        this.searchForm.get("lcNumber")?.patchValue("", { emitEvent: false });
      }
    });
  }

  clearInput(evt: any): void {
    evt.stopPropagation();
    this.searchForm?.get("supplierId").setValue(null);
    this.filterSuppliers = this.filterSuppliersPO;
    this.purchaseOrdersNo = this.purchaseOrdersLcNo = this.purchaseOrders;
  }

  getPurchaseOrders(): void {
    this.purchaseOrderRequest.page = -1;
    this.purchaseOrderRequest.purchaseOrderStatuses = [3, 4, 5, 8];
    this.purchaseOrderService
      .getPurchaseOrders(this.purchaseOrderRequest)
      .subscribe((res) => {
        const poData = res?.data?.item1 || [];
        const filteredOrders = poData.filter(
          (order) => order.isImportPurchase === this.isImportPurchaseData
        );
        this.dataSource = new MatTableDataSource<PurchaseOrderResponseDTO>(
          filteredOrders
        );
        this.totalCount = res?.data?.item2;
        this.loading = false;
      });
  }

  getPurchaseOrdersNo(): void {
    this.purchaseOrderRequest.page = -1;
    this.purchaseOrderRequest.purchaseOrderStatuses = [3, 4, 5, 8];
    this.purchaseOrderService
      .getPurchaseOrders(this.purchaseOrderRequest)
      .subscribe((res) => {
        const poData = res?.data?.item1 || [];
        const filteredOrders = poData.filter(
          (order) => order.isImportPurchase === this.isImportPurchaseData
        );

        const seen = new Set();
        const uniqueSuppliersPO = filteredOrders.filter((po) => {
          const isDuplicate = seen.has(po.supplierId);
          seen.add(po.supplierId);
          return !isDuplicate;
        });

        this.purchaseOrdersNo =
          this.purchaseOrdersLcNo =
          this.purchaseOrders =
            filteredOrders;
        this.filterSuppliers = this.filterSuppliersPO = uniqueSuppliersPO;
      });
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
    this.filterSuppliers = this.filterSuppliersPO?.filter((option) =>
      option.supplier.name.toLowerCase().includes(filterValue)
    );
  }

  getSupplierName(supplierId: string) {
    if (!supplierId) {
      return;
    }
    const supplierAccount = this.filterSuppliersPO?.find(
      (purchaseOrder) => purchaseOrder?.supplierId === supplierId
    );
    return supplierAccount?.supplier?.name;
  }

  onSelectedSupplierId(supplierId: string) {
    this.searchForm.patchValue({
      ponumber: "",
      lcNumber: "",
    });
    this.purchaseOrdersNo = this.purchaseOrdersLcNo =
      this.purchaseOrders?.filter(
        (purchaseOrder) => purchaseOrder.supplierId === supplierId
      );
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

  onPONoChange(): void {
    const term = this.searchForm.get("ponumber");
    this._filterPONo(term.value || "");
  }

  private _filterPONo(value: string) {
    const filterValue = value.toLowerCase().trim();
    const supplierId = this.searchForm.value.supplierId;
    if (supplierId) {
      this.purchaseOrdersNo = this.purchaseOrders?.filter(
        (x) =>
          x.ponumber.toLowerCase().includes(filterValue) &&
          x.supplierId === supplierId
      );
    } else {
      this.purchaseOrdersNo = this.purchaseOrders?.filter((x) =>
        x.ponumber.toLowerCase().includes(filterValue)
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
      this.purchaseOrdersLcNo = this.purchaseOrders?.filter(
        (x) =>
          x.lcNumber.toLowerCase().includes(filterValue) &&
          x.supplierId === supplierId
      );
    } else {
      this.purchaseOrdersLcNo = this.purchaseOrders?.filter((x) =>
        x.lcNumber.toLowerCase().includes(filterValue)
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

  showSnackBar(message: string) {
    this.snackBarService.openSnackbar(message);
  }

  isContinueButtonDisabled: boolean = false;
  handleRadioChange($event) {
    if ($event.value.isImportPurchase) {
      if ($event.value.status == 8 || $event.value.status == 5) {
        this.isContinueButtonDisabled = false;
      } else {
        this.isContinueButtonDisabled = true;
        this.showSnackBar(
          "The status of your chosen Purchase Order should be marked as 'Ready for GRN'"
        );
      }
    } else this.isContinueButtonDisabled = false;
    this.selectedPurchaseOrder = $event.value;
  }
}
