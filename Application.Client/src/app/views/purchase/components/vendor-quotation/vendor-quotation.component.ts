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
import { Router } from "@angular/router";
import { Page_Size_Options } from "app/shared/consts/const";
import { ImportPurchaseIncoTerm } from "app/shared/enums/importPurchaseIncoTerm";
import { ImportPurchasePaymentTerm } from "app/shared/enums/importPurchasePaymentTerm";
import { PaymentMode } from "app/shared/enums/paymentMode";
import { Transport } from "app/shared/enums/transport";
import { VendorQuotationStatus } from "app/shared/enums/vendorQuotationStatus";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { Currency } from "app/views/configuration/models/currency/currency.model";
import { CurrencyService } from "app/views/configuration/services/currency.service";
import { ToastrService } from "ngx-toastr";
import { Observable, merge, of } from "rxjs";
import { VendorQuotationResponseDTO } from "../../models/vendor-quotation/vendor-quotation-response-dto.model";
import { VendorQuotationSearchRequestDTO } from "../../models/vendor-quotation/vendor-quotation-search-request-dto.model";
import { VendorQuotationService } from "../../services/vendor-quotation.service";

@Component({
  selector: "app-manage-vendor-quotation",
  templateUrl: "./vendor-quotation.component.html",
  styleUrls: ["./vendor-quotation.component.scss"],
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
export class VendorQuotationComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  viewColumnForm: FormGroup;
  expandedElement: any;
  dataSource: MatTableDataSource<VendorQuotationResponseDTO>;
  totalCount: number;
  currencies: Currency[];
  vendorQuotationRequest = new VendorQuotationSearchRequestDTO();
  displayedColumns$: Observable<string[]>;
  pageSizeOptions: [] = Page_Size_Options;
  currentFinancialYearId: string;
  private path = {
    addNew: "purchase/manage-vendor-quotation/add-new",
    edit: "purchase/manage-vendor-quotation",
    compare: "purchase/manage-vendor-quotation/compare",
  };

  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  // * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "expand", label: "Expand" },
    { def: "actions", label: "Actions" },
    { def: "quotationNo", label: "Quotation No." },
    { def: "requisitionNo", label: "Requisition No." },
    { def: "referenceNo", label: "Reference No." },
    //{ def: "storeId", label: "storeId" },
    { def: "supplierId", label: "Supplier" },
    { def: "transport", label: "Transport" },
    { def: "paymentMode", label: "Payment Mode" },
    { def: "paymentTermInDays", label: "Payment Term In Days" },
    { def: "deliveryTermInDays", label: "Delivery Term In Days" },
    { def: "deliveryDate", label: "Delivery Date" },
    { def: "currencyId", label: "Currency" },
    { def: "importPurchaseIncoTerm", label: "Import Purchase Inco Term" },
    { def: "importPurchasePaymentTerm", label: "Import Purchase Payment Term" },
    { def: "totalAmount", label: "Total Amount" },
    { def: "status", label: "Status" },
    { def: "remark", label: "Remark" },
    { def: "termAndCondition", label: "Terms & Conditions" },
    // { def: "checkedBy", label: "Checked By" },
    { def: "approvedBy", label: "Approved By" },
    { def: "createdOn", label: "Created On" },
    { def: "updatedOn", label: "Updated On" },
    { def: "createdBy", label: "Created By" },
    { def: "updatedBy", label: "Updated By" },
  ];

  constructor(
    public dateFormatService: DateTimeFormatService,
    private jwtAuth: JwtAuthService,
    public statusColorService: StatusColorService,
    public currencyService: CurrencyService,
    private vendorQuotationService: VendorQuotationService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private router: Router,
    private confirmDialogService: ConfirmDialogService // private localStorageService: LocalStoreService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.getVendorQuotations(this.vendorQuotationRequest);
    this.getCurrentFinancialYearId();
    this.subscribeToFormChanges();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
      requisitionNo: [""],
      fromDate: [null],
      toDate: [null],
    });
    this.viewColumnForm = this.fb.group({
      expand: [true],
      quotationNo: [true],
      requisitionNo: [true],
      referenceNo: [false],
      //storeId: [false],
      supplierId: [true],
      transport: [false],
      paymentMode: [false],
      paymentTermInDays: [false],
      deliveryTermInDays: [false],
      deliveryDate: [false],
      currencyId: [false],
      importPurchaseIncoTerm: [false],
      importPurchasePaymentTerm: [false],
      totalAmount: [true],
      status: [true],
      remark: [false],
      termAndCondition: [false],
      // checkedBy: [false],
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

  getVendorQuotations(requestBody): void {
    this.vendorQuotationService
      .getVendorQuotations(requestBody)
      .subscribe((res) => {
        console.log(res);
        this.dataSource = new MatTableDataSource<VendorQuotationResponseDTO>(
          res?.data?.item1
        );
        this.totalCount = res?.data?.item2;
        this.loading = false;
      });
  }

  getVendorQuotationStatus(value) {
    return this.statusColorService.getVendorQuotationStatus(value);
  }

  getVendorQuotationStatusName(value: number) {
    return VendorQuotationStatus[value];
  }

  getTransportName(value: number) {
    return Transport[value];
  }

  getPaymentModeName(value: number) {
    return PaymentMode[value];
  }

  getAllCurrencies(): void {
    this.currencyService.getAllCurrencies().subscribe((res) => {
      this.currencies = res?.data?.item1;
    });
  }

  getCurrencyName(currencyId: string) {
    return this.currencies?.find((x) => x.id === currencyId)?.name;
  }

  getImportPurchaseIncoTermName(value: number) {
    return ImportPurchaseIncoTerm[value];
  }

  getImportPurchasePaymentTermName(value: number) {
    return ImportPurchasePaymentTerm[value];
  }

  remove(id): void {
    this.vendorQuotationService.deleteVendorQuotation(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getVendorQuotations(this.vendorQuotationRequest);
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
    this.vendorQuotationRequest = new VendorQuotationSearchRequestDTO();
    this.getVendorQuotations(this.vendorQuotationRequest);
  }

  onSearch() {
    this.loading = true;
    this.vendorQuotationRequest.keyword = this.searchForm.value.keyword;
    this.vendorQuotationRequest.requisitionNo =
      this.searchForm.value.requisitionNo;
    this.vendorQuotationRequest.fromDate = this.searchForm.value.fromDate;
    this.vendorQuotationRequest.toDate = this.searchForm.value.toDate;
    this.getVendorQuotations(this.vendorQuotationRequest);
  }

  navigateToAddNew() {
    this.router.navigate([this.path.addNew]);
  }

  navigateToView(item) {
    this.router.navigate([this.path.edit, item.id]);
    // this.localStorageService.setItem(item.id, item);
  }

  navigateToCompare() {
    this.router.navigate([this.path.compare]);
  }

  onPageChange(pageEvent) {
    this.vendorQuotationRequest.page = pageEvent.pageIndex;
    this.vendorQuotationRequest.rowsPerPage = pageEvent.pageSize;
    this.getVendorQuotations(this.vendorQuotationRequest);
  }

  trackBy(index: number, item: any) {
    return item?.id;
  }
}
