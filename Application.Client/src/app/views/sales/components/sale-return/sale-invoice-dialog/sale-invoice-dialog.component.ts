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
import { SaleInvoiceStatus } from "app/shared/enums/saleInvoiceStatus";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { Customer } from "app/views/configuration/models/customer/customer.model";
import { CustomerService } from "app/views/configuration/services/customer.service";
import { SaleInvoiceResponseDTO } from "app/views/sales/models/sale-invoice/sale-invoice-response-dto.model";
import { SaleInvoiceSearchRequestDTO } from "app/views/sales/models/sale-invoice/sale-invoice-search-request-dto.model";
import { SaleInvoiceService } from "app/views/sales/services/sale-invoice.service";

@Component({
  selector: "app-sale-invoice-dialog",
  templateUrl: "./sale-invoice-dialog.component.html",
  styleUrls: ["./sale-invoice-dialog.component.scss"],
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
export class SaleInvoiceDialogComponent implements OnInit {
  dialogTitle = "Select Sale Invoice";
  loading: boolean = true;
  selectedSaleInvoice: SaleInvoiceResponseDTO = null;
  // selection = new SelectionModel<DeliveryNoteResponseDTO>(true, []); // its need for multiple selection
  saleInvoices: SaleInvoiceResponseDTO[];
  filteredSaleInvoices: SaleInvoiceResponseDTO[];
  saleInvoiceRequest = new SaleInvoiceSearchRequestDTO();

  panelOpenState = false;
  searchForm: FormGroup;
  customers: Customer[];
  filterCustomers: Customer[];
  expandedElement: any;
  expandedRow: any;
  totalCount: number;
  saleInvoiceStatuses: ENUM[];
  displayedColumns: string[] = [
    "select", // its need for multiple selection
    "expand",
    "saleOrderNo",
    "invoiceNo",
    "customerId",
    "storeId",
    "invoiceDate",
    "referenceNo",
    "status",
  ];
  dataSource: MatTableDataSource<SaleInvoiceResponseDTO>;

  constructor(
    public dateFormatService: DateTimeFormatService,
    public statusColorService: StatusColorService,
    private saleInvoiceService: SaleInvoiceService,
    private customerService: CustomerService,
    private enumValueService: EnumValueService,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    this.getSaleInvoices();
    this.getSaleInvoicesNo();
    this.getAllCustomers();
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      saleInvoiceNo: [""],
      customerId: [null],
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
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    if (fieldName === "customerId") {
      this.searchForm?.get("customerId").setValue(null);
    }
    if (fieldName === "saleInvoiceNo") {
      this.searchForm?.get("saleInvoiceNo").setValue("");
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
      this.customers = res.data?.item1;
    });
  }

  getAllSaleInvoiceStatuses() {
    this.enumValueService.getSaleInvoiceStatuses().subscribe((res) => {
      console.log("rq status:", res);
      this.saleInvoiceStatuses = res;
    });
  }

  getSaleInvoiceStatus(value) {
    return this.statusColorService.getSaleInvoiceStatus(value);
  }

  getSaleInvoiceName(value: number) {
    return SaleInvoiceStatus[value];
  }

  getSaleInvoices(): void {
    // this.saleInvoiceRequest.saleInvoiceStatus = 3;
    this.saleInvoiceRequest.SaleInvoiceStatuses = [3, 4];

    //this.saleInvoiceRequest.keyword = keyword;
    //this.saleInvoiceRequest.rowsPerPage=5;
    this.saleInvoiceService
      .getSaleInvoices(this.saleInvoiceRequest)
      .subscribe((res) => {
        this.dataSource = new MatTableDataSource<SaleInvoiceResponseDTO>(
          res?.data?.item1
        );
        this.totalCount = res?.data?.item2;
        this.loading = false;
      });
  }

  getSaleInvoicesNo(): void {
    // this.saleInvoiceRequest.keyword = keyword;
    this.saleInvoiceRequest.SaleInvoiceStatuses = [3, 4];
    this.saleInvoiceService
      .getSaleInvoices(this.saleInvoiceRequest)
      .subscribe((res) => {
        this.filteredSaleInvoices = this.saleInvoices = res?.data?.item1;
      });
  }

  onSaleInvoiceNoChange(): void {
    const term = this.searchForm.get("saleInvoiceNo");
    this._filterSaleInvoice(term.value || "");
  }

  private _filterSaleInvoice(value: string) {
    const filterValue = value.toLowerCase().trim();
    this.filteredSaleInvoices = this.saleInvoices.filter((option) =>
      option.invoiceNo.toLowerCase().includes(filterValue)
    );
    // this.getSaleInvoicesNo(filterValue);
  }

  onSearch() {
    this.loading = true;
    console.log(this.searchForm.value);
    this.saleInvoiceRequest.keyword = this.searchForm.value.saleInvoiceNo;
    this.saleInvoiceRequest.customerId = this.searchForm.value.customerId;
    //let deliveryNoteNo = this.searchForm.get("deliveryNoteNo").value;
    this.getSaleInvoices();
  }

  onPageChange(pageEvent) {
    this.saleInvoiceRequest.page = pageEvent.pageIndex;
    this.saleInvoiceRequest.rowsPerPage = pageEvent.pageSize;
    this.getSaleInvoices();
  }

  handleRadioChange($event) {
    this.selectedSaleInvoice = $event.value;
  }
}
