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
import { SaleQuotationStatus } from "app/shared/enums/saleQuotationStatus";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { PendingCheckedCount } from "app/views/purchase/models/pending-checked-count";
import { SaleQuotationResponseDTO } from "app/views/sales/models/sale-quotation/sale-quotation-response-dto.model";
import { SaleQuotationSearchRequestDTO } from "app/views/sales/models/sale-quotation/sale-quotation-search-request-dto.model";
import { SaleQuotationService } from "app/views/sales/services/sale-quotation.service";
import { ToastrService } from "ngx-toastr";
import { Observable, merge, of } from "rxjs";

@Component({
  selector: "app-sales-quotation-list",
  templateUrl: "./sales-quotation-list.component.html",
  styleUrls: ["./sales-quotation-list.component.scss"],
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
export class SalesQuotationListComponent implements OnInit {
  loading: boolean = true;
  selectedSaleQuotation: SaleQuotationResponseDTO = null;
  saleQuotations: SaleQuotationResponseDTO[];
  saleQuotationRequest = new SaleQuotationSearchRequestDTO();

  panelOpenState: boolean;
  searchForm: FormGroup;
  viewColumnForm: FormGroup;
  expandedElement: any;
  dataSource: MatTableDataSource<SaleQuotationResponseDTO>;
  totalCount: number;
  saleQuotationStatuses: ENUM[];
  pendingCheckedModel: PendingCheckedCount;
  displayedColumns$: Observable<string[]>;

  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  //  * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "select", label: "Select" },
    { def: "expand", label: "Expand" },
    { def: "actions", label: "Actions" },
    { def: "quotationNo", label: "Quotation No" },
    { def: "quotationDate", label: "Quotation Date" },
    { def: "customerId", label: "customerId" },
    { def: "referenceNo", label: "Reference No" },
    { def: "expiryDate", label: "Expiry Date" },
    { def: "termAndCondition", label: "Term & Condition" },
    { def: "subtotal", label: "Subtotal" },
    { def: "discount", label: "Discount" },
    { def: "total", label: "Total" },
    { def: "isMailSent", label: "Is Mail Sent" },
    { def: "status", label: "Status" },
    { def: "remark", label: "Remark" },
  ];

  constructor(
    public statusColorService: StatusColorService,
    public dateFormatService: DateTimeFormatService,
    private saleQuotationService: SaleQuotationService,
    private enumValueService: EnumValueService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.getAllSaleQuotationStatuses();
    this.getSaleQuotations("");
    this.subscribeToFormChanges();
  }

  private initializeForm() {
    this.searchForm = this.fb.group({
      quotationNo: [""],
    });
    this.viewColumnForm = this.fb.group({
      select: [true],
      expand: [true],
      quotationNo: [true],
      quotationDate: [true],
      customerId: [true],
      referenceNo: [true],
      expiryDate: [true],
      termAndCondition: [true],
      subtotal: [true],
      discount: [true],
      total: [true],
      isMailSent: [true],
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

  getAllSaleQuotationStatuses() {
    this.enumValueService.getSaleQuotationStatuses().subscribe((res) => {
      this.saleQuotationStatuses = res;
    });
  }

  getSaleQuotationStatus(value: number) {
    return this.statusColorService.getSaleQuotationStatus(value);
  }

  getSaleQuotationStatusName(value: number) {
    return SaleQuotationStatus[value];
  }

  getSaleQuotations(keyword: string): void {
    this.saleQuotationRequest.saleQuotationStatus = 3;
    this.saleQuotationRequest.keyword = keyword;
    this.saleQuotationService
      .getSaleQuotations(this.saleQuotationRequest)
      .subscribe((res) => {
        this.dataSource = new MatTableDataSource<SaleQuotationResponseDTO>(
          res?.data?.item1
        );
        this.totalCount = res?.data?.item2;
        this.loading = false;
      });
  }

  getPurchaseInvoiceNo(keyword): void {
    this.saleQuotationRequest.saleQuotationStatus = 3;
    this.saleQuotationRequest.keyword = keyword;
    this.saleQuotationService
      .getSaleQuotations(this.saleQuotationRequest)
      .subscribe((res) => {
        console.log(res?.data?.item1);
        this.saleQuotations = res?.data?.item1;
      });
  }

  onSaleQuotationNoChange(): void {
    const term = this.searchForm.get("quotationNo");
    this._filterProduct(term.value || "");
  }

  private _filterProduct(value: string) {
    const filterValue = value.toLowerCase().trim();
    console.log("filterValue", filterValue);
    this.getPurchaseInvoiceNo(filterValue);
  }

  onSearch() {
    this.loading = true;
    let quotationNo = this.searchForm.get("quotationNo").value;
    this.getSaleQuotations(quotationNo);
  }

  onPageChange(pageEvent) {
    this.saleQuotationRequest.page = pageEvent.pageIndex;
    this.saleQuotationRequest.rowsPerPage = pageEvent.pageSize;
    this.getSaleQuotations(this.searchForm.get("quotationNo").value);
  }

  handleRadioChange($event) {
    this.selectedSaleQuotation = $event.value;
  }
}
