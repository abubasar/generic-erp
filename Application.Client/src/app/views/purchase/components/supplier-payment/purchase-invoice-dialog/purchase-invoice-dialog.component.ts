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
import { PurchaseInvoiceStatus } from "app/shared/enums/purchaseInvoiceStatus";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { PurchaseInvoiceResponseDTO } from "app/views/purchase/models/purchase-invoice/purchase-invoice-response-dto.model";
import { PurchaseInvoiceSearchRequestDTO } from "app/views/purchase/models/purchase-invoice/purchase-invoice-search-request-dto.model";
import { PurchaseInvoiceService } from "app/views/purchase/services/purchase-invoice.service";
import { ToastrService } from "ngx-toastr";
import { Observable, merge, of } from "rxjs";

@Component({
  selector: "app-purchase-invoice-dialog",
  templateUrl: "./purchase-invoice-dialog.component.html",
  styleUrls: ["./purchase-invoice-dialog.component.scss"],
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
export class PurchaseInvoiceDialogComponent implements OnInit {
  dialogTitle: string = "Purchase Invoice List";
  loading: boolean = true;
  selectedPurchaseInvoice: PurchaseInvoiceResponseDTO = null;
  purchaseInvoices: PurchaseInvoiceResponseDTO[];
  purchaseInvoiceRequest = new PurchaseInvoiceSearchRequestDTO();

  panelOpenState: boolean;
  searchForm: FormGroup;
  viewColumnForm: FormGroup;
  expandedElement: any;
  dataSource: MatTableDataSource<PurchaseInvoiceResponseDTO>;
  totalCount: number;
  purchaseInvoiceStatuses: ENUM[];
  displayedColumns$: Observable<string[]>;

  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  //  * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "select", label: "Select" },
    { def: "expand", label: "Expand" },
    { def: "purchaseInvoiceNo", label: "Purchase Invoice No." },
    { def: "invoiceDate", label: "Invoice Date" },
    { def: "grnno", label: "GRN No." },
    { def: "ponumber", label: "PO Number" },
    { def: "storeId", label: "Store" },
    { def: "supplierId", label: "Supplier" },
    { def: "supplierInvoiceNo", label: "Sup. Invoice No" },
    { def: "supplierInvoiceDate", label: "Sup. Invoice Date" },
    { def: "paymentTermInDays", label: "Payment Term In Days" },
    { def: "subtotal", label: "Subtotal" },
    { def: "discount", label: "Discount" },
    { def: "totalVat", label: "Vat" },
    { def: "total", label: "Total" },
    { def: "paidAmount", label: "Paid" },
    { def: "advancePaymentAmount", label: "Advance Payment Amount" },
    { def: "payableAmount", label: "Payable" },
    { def: "status", label: "Status" },
    { def: "remark", label: "Remark" },
  ];

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: string,
    public dateFormatService: DateTimeFormatService,
    public statusColorService: StatusColorService,
    private purchaseInvoiceService: PurchaseInvoiceService,
    private enumValueService: EnumValueService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.getAllPurchaseInvoiceStatuses();
    this.getPurchaseInvoices("");
    this.subscribeToFormChanges();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      purchaseInvoiceNo: [""],
    });
    this.viewColumnForm = this.fb.group({
      select: [true],
      expand: [true],
      purchaseInvoiceNo: [true],
      invoiceDate: [false],
      grnno: [false],
      ponumber: [true],
      storeId: [false],
      supplierId: [true],
      supplierInvoiceNo: [false],
      supplierInvoiceDate: [true],
      paymentTermInDays: [true],
      subtotal: [false],
      discount: [false],
      totalVat: [false],
      total: [true],
      paidAmount: [true],
      advancePaymentAmount: [true],
      payableAmount: [true],
      status: [true],
      remark: [false],
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

  getPurchaseInvoices(keyword: string): void {
    this.purchaseInvoiceRequest.page = -1;
    this.purchaseInvoiceRequest.purchaseInvoiceStatuses = [3, 4]; // it will be uncomment after complete
    this.purchaseInvoiceRequest.keyword = keyword;
    this.purchaseInvoiceRequest.supplierId = this.data;
    this.purchaseInvoiceService
      .getPurchaseInvoices(this.purchaseInvoiceRequest)
      .subscribe((res) => {
        const piData = res?.data?.item1 || [];
        const filteredPIs = piData?.filter(
          (pi) => pi.isImportPurchase === false
        );
        this.dataSource = new MatTableDataSource<PurchaseInvoiceResponseDTO>(
          filteredPIs
        );
        this.totalCount = filteredPIs?.length;
        this.loading = false;
      });
  }

  getPurchaseInvoiceNo(keyword): void {
    this.purchaseInvoiceRequest.purchaseInvoiceStatuses = [3, 4];
    this.purchaseInvoiceRequest.keyword = keyword;
    this.purchaseInvoiceRequest.supplierId = this.data;
    this.purchaseInvoiceService
      .getPurchaseInvoices(this.purchaseInvoiceRequest)
      .subscribe((res) => {
        const piData = res?.data?.item1 || [];
        const filteredPIs = piData?.filter(
          (pi) => pi.isImportPurchase === false
        );
        this.purchaseInvoices = filteredPIs;
      });
  }

  onPurchaseInvoiceNoChange(): void {
    const term = this.searchForm.get("purchaseInvoiceNo");
    this._filterPurchaseInvoice(term.value || "");
  }

  private _filterPurchaseInvoice(value: string) {
    const filterValue = value.toLowerCase().trim();
    console.log("filterValue", filterValue);
    this.getPurchaseInvoiceNo(filterValue);
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

  onSearch() {
    this.loading = true;
    let purchaseInvoiceNo = this.searchForm.get("purchaseInvoiceNo").value;
    this.getPurchaseInvoices(purchaseInvoiceNo);
  }

  handleRadioChange($event) {
    this.selectedPurchaseInvoice = $event.value;
  }
}
