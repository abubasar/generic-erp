import {
  animate,
  state,
  style,
  transition,
  trigger,
} from "@angular/animations";
import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { MAT_DIALOG_DATA, MatDialog } from "@angular/material/dialog";
import { MatTableDataSource } from "@angular/material/table";
import { PaymentMode } from "app/shared/enums/paymentMode";
import { PurchaseOrderStatus } from "app/shared/enums/purchaseOrderStatus";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { PurchaseOrderResponseDTO } from "app/views/purchase/models/purchase-order/purchase-order-response-dto.model";
import { PurchaseOrderSearchRequestDTO } from "app/views/purchase/models/purchase-order/purchase-order-search-request-dto.model";
import { PurchaseOrderService } from "app/views/purchase/services/purchase-order.service";
import { SupplierTransactionsAgainstPOComponent } from "../../purchase-order/supplier-transactions-against-po/supplier-transactions-against-po.component";

@Component({
  selector: "app-purchase-order-dialog",
  templateUrl: "./purchase-order-dialog.component.html",
  styleUrls: ["./purchase-order-dialog.component.scss"],
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
export class PurchaseOrderDialogComponent implements OnInit {
  loading: boolean = true;
  selectedPurchaseOrder: PurchaseOrderResponseDTO = null;
  purchaseOrders: PurchaseOrderResponseDTO[];
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
    "actions",
  ];
  dataSource: MatTableDataSource<PurchaseOrderResponseDTO>;
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    public dateFormatService: DateTimeFormatService,
    public statusColorService: StatusColorService,
    private purchaseOrderService: PurchaseOrderService,
    private enumValueService: EnumValueService,
    private fb: FormBuilder,
    public dialog: MatDialog
  ) {}
  ngOnInit(): void {
    this.getPurchaseOrders("");
    this.initializeForm();
  }
  initializeForm() {
    this.searchForm = this.fb.group({
      ponumber: [""],
    });
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

  getPurchaseOrders(keyword): void {
    this.purchaseOrderRequest.page = -1;
    this.purchaseOrderRequest.purchaseOrderStatuses = [3, 4, 5, 6, 7];
    this.purchaseOrderRequest.keyword = keyword;
    this.purchaseOrderRequest.supplierId = this.data;
    this.purchaseOrderService
      .getPurchaseOrders(this.purchaseOrderRequest)
      .subscribe((res) => {
        const poData = res?.data?.item1 || [];
        const filteredPOs = poData?.filter(
          (po) => po.isImportPurchase === false
        );
        this.dataSource = new MatTableDataSource<PurchaseOrderResponseDTO>(
          filteredPOs
        );
        this.totalCount = filteredPOs?.length;
        this.loading = false;
      });
  }

  getPurchaseOrdersNo(keyword): void {
    this.purchaseOrderRequest.page = -1;
    this.purchaseOrderRequest.purchaseOrderStatuses = [3, 4, 5, 6, 7];
    this.purchaseOrderRequest.keyword = keyword;
    this.purchaseOrderRequest.supplierId = this.data;
    this.purchaseOrderService
      .getPurchaseOrders(this.purchaseOrderRequest)
      .subscribe((res) => {
        const poData = res?.data?.item1 || [];
        const filteredPOs = poData?.filter(
          (po) => po.isImportPurchase === false
        );
        this.purchaseOrders = filteredPOs;
      });
  }

  onGRNNoChange(): void {
    const term = this.searchForm.get("ponumber");
    this._filterProduct(term.value || "");
  }

  private _filterProduct(value: string) {
    const filterValue = value.toLowerCase().trim();
    this.getPurchaseOrdersNo(filterValue);
  }

  onSearch() {
    this.loading = true;
    let ponumber = this.searchForm.get("ponumber").value;
    this.getPurchaseOrders(ponumber);
  }

  // onPageChange(pageEvent) {
  //   this.purchaseOrderRequest.page = pageEvent.pageIndex;
  //   this.purchaseOrderRequest.rowsPerPage = pageEvent.pageSize;
  //   this.getPurchaseOrders("");
  // }
  handleRadioChange($event) {
    this.selectedPurchaseOrder = $event.value;
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
}
