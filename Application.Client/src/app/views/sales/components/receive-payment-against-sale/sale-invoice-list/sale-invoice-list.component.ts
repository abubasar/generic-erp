import {
  animate,
  state,
  style,
  transition,
  trigger,
} from "@angular/animations";
import { Component, OnInit, ViewChild } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { MatDialog } from "@angular/material/dialog";
import { MatMenuTrigger } from "@angular/material/menu";
import { MatTableDataSource } from "@angular/material/table";
import { SaleInvoiceStatus } from "app/shared/enums/saleInvoiceStatus";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { SaleInvoiceResponseDTO } from "app/views/sales/models/sale-invoice/sale-invoice-response-dto.model";
import { SaleInvoiceSearchRequestDTO } from "app/views/sales/models/sale-invoice/sale-invoice-search-request-dto.model";
import { SaleInvoiceService } from "app/views/sales/services/sale-invoice.service";
import { ToastrService } from "ngx-toastr";
import { Observable, merge, of } from "rxjs";

@Component({
  selector: "app-sale-invoice-list",
  templateUrl: "./sale-invoice-list.component.html",
  styleUrls: ["./sale-invoice-list.component.scss"],
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
export class SaleInvoiceListComponent implements OnInit {
  dialogTitle: string = "Sale Invoice List";
  loading: boolean = true;
  selectedSaleInvoice: SaleInvoiceResponseDTO = null;
  saleInvoices: SaleInvoiceResponseDTO[];
  saleInvoiceRequest = new SaleInvoiceSearchRequestDTO();

  panelOpenState: boolean;
  searchForm: FormGroup;
  viewColumnForm: FormGroup;
  expandedElement: any;
  dataSource: MatTableDataSource<SaleInvoiceResponseDTO>;
  totalCount: number;
  saleInvoiceStatuses: ENUM[];
  displayedColumns$: Observable<string[]>;

  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  //  * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "select", label: "Select" },
    { def: "expand", label: "Expand" },
    { def: "actions", label: "Actions" },
    { def: "invoiceNo", label: "Sale Invoice No." },
    { def: "invoiceDate", label: "Invoice Date" },
    { def: "deliveryNoteNo", label: "Delivery Note No." },
    { def: "customerId", label: "Customer" },
    { def: "deliveryDate", label: "Delivery Date" },
    { def: "deliveryPlace", label: "Delivery Place" },
    { def: "referenceNo", label: "Reference No" },
    { def: "bankDetails", label: "Bank Details" },
    { def: "termAndCondition", label: "Term And Condition" },
    { def: "subtotal", label: "Subtotal" },
    { def: "discount", label: "Discount" },
    { def: "total", label: "Total" },
    { def: "status", label: "Status" },
    { def: "remark", label: "Remark" },
  ];

  constructor(
    public dateFormatService: DateTimeFormatService,
    public statusColorService: StatusColorService,
    private saleInvoiceService: SaleInvoiceService,
    private enumValueService: EnumValueService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.getAllSaleInvoiceStatuses();
    this.getSaleInvoices("");
    this.subscribeToFormChanges();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      saleInvoiceNo: [""],
    });
    this.viewColumnForm = this.fb.group({
      select: [true],
      expand: [true],
      invoiceNo: [true],
      invoiceDate: [true],
      deliveryNoteNo: [true],
      customerId: [false],
      deliveryDate: [true],
      deliveryPlace: [true],
      referenceNo: [false],
      bankDetails: [false],
      termAndCondition: [false],
      subtotal: [true],
      discount: [false],
      total: [true],
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

  getSaleInvoices(keyword: string): void {
    this.saleInvoiceRequest.saleInvoiceStatus = 3; // it will be uncomment after complete
    this.saleInvoiceRequest.keyword = keyword;
    this.saleInvoiceService
      .getSaleInvoices(this.saleInvoiceRequest)
      .subscribe((res) => {
        console.log(res);
        this.dataSource = new MatTableDataSource<SaleInvoiceResponseDTO>(
          res?.data?.item1
        );
        this.totalCount = res?.data?.item2;
        this.loading = false;
      });
  }

  getSaleInvoiceNo(keyword): void {
    this.saleInvoiceRequest.saleInvoiceStatus = 3;
    this.saleInvoiceRequest.keyword = keyword;
    this.saleInvoiceService
      .getSaleInvoices(this.saleInvoiceRequest)
      .subscribe((res) => {
        console.log(res?.data?.item1);
        this.saleInvoices = res?.data?.item1;
      });
  }

  onSaleInvoiceNoChange(): void {
    const term = this.searchForm.get("saleInvoiceNo");
    this._filterSaleInvoice(term.value || "");
  }

  private _filterSaleInvoice(value: string) {
    const filterValue = value.toLowerCase().trim();
    console.log("filterValue", filterValue);
    this.getSaleInvoiceNo(filterValue);
  }

  getAllSaleInvoiceStatuses() {
    this.enumValueService.getSaleInvoiceStatuses().subscribe((res) => {
      this.saleInvoiceStatuses = res;
    });
  }

  getSaleInvoiceStatus(value: number) {
    return this.statusColorService.getSaleInvoiceStatus(value);
  }

  getSaleInvoiceStatusName(value: number) {
    return SaleInvoiceStatus[value];
  }

  onSearch() {
    this.loading = true;
    console.log(this.searchForm.value);
    let saleInvoiceNo = this.searchForm.get("saleInvoiceNo").value;
    this.getSaleInvoices(saleInvoiceNo);
  }

  onPageChange(pageEvent) {
    this.saleInvoiceRequest.page = pageEvent.pageIndex;
    this.saleInvoiceRequest.rowsPerPage = pageEvent.pageSize;
    this.getSaleInvoices(this.searchForm.get("saleInvoiceNo").value);
  }

  handleRadioChange($event) {
    this.selectedSaleInvoice = $event.value;
  }
}
