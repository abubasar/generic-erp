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
import { Priority } from "app/shared/enums/priority";
import { RequisitionStatus } from "app/shared/enums/requisitionStatus";
import { Transport } from "app/shared/enums/transport";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { Currency } from "app/views/configuration/models/currency/currency.model";
import { CurrencyService } from "app/views/configuration/services/currency.service";
import { ToastrService } from "ngx-toastr";
import { Observable, merge, of } from "rxjs";
import { PurchaseRequisitionResponseDTO } from "../../models/purchase-requisition/purchase-requisition-response-dto.model";
import { PurchaseRequisitionSearchRequestDTO } from "../../models/purchase-requisition/purchase-requisition-search-request-dto.model";
import { PurchaseRequisitionService } from "../../services/purchase-requisition.service";
import { SendRFQToVendorEmailComponent } from "./send-rfq-to-vendor-email/send-rfq-to-vendor-email.component";

@Component({
  selector: "app-send-rfq-to-vendor",
  templateUrl: "./send-rfq-to-vendor.component.html",
  styleUrls: ["./send-rfq-to-vendor.component.scss"],
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
export class SendRFQToVendorComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  viewColumnForm: FormGroup;
  expandedElement: any;
  dataSource: MatTableDataSource<PurchaseRequisitionResponseDTO>;
  totalCount: number;
  purchaseRequisitionRequest = new PurchaseRequisitionSearchRequestDTO();
  currencies: Currency[];
  requisitionStatuses: ENUM[];
  displayedColumns$: Observable<string[]>;
  pageSizeOptions: [] = Page_Size_Options;
  currentFinancialYearId: string;
  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  // * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "expand", label: "Expand" },
    { def: "actions", label: "Actions" },
    { def: "requisitionNo", label: "Requisition No." },
    { def: "department", label: "Department" },
    { def: "store", label: "Store" },
    { def: "requisitionDate", label: "Requisition Date" },
    { def: "expectedDeliveryDate", label: "Expected Delivery Date" },
    { def: "transport", label: "Transport" },
    { def: "paymentMode", label: "Payment Mode" },
    { def: "paymentTermInDays", label: "Payment Term In Days" },
    { def: "deliveryTermInDays", label: "Delivery Term In Days" },
    { def: "currencyId", label: "Currency" },
    { def: "importPurchaseIncoTerm", label: "Import Purchase Inco Term" },
    { def: "importPurchasePaymentTerm", label: "Import Purchase Payment Term" },
    { def: "remark", label: "Remark" },
    { def: "termAndCondition", label: "Terms & Conditions" },
    { def: "requestByName", label: "Request By" },
    { def: "priority", label: "Priority" },
    { def: "requisitionStatus", label: "Req. Status" },
  ];

  constructor(
    public dateFormatService: DateTimeFormatService,
    private jwtAuth: JwtAuthService,
    public statusColorService: StatusColorService,
    public currencyService: CurrencyService,
    private purchaseRequisitionService: PurchaseRequisitionService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private router: Router // private localStorageService: LocalStoreService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.getPurchaseRequisitions();
    this.getCurrentFinancialYearId();
    this.subscribeToFormChanges();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [null],
    });
    this.viewColumnForm = this.fb.group({
      expand: [true],
      requisitionNo: [true],
      department: [true],
      store: [true],
      requisitionDate: [false],
      expectedDeliveryDate: [false],
      transport: [true],
      paymentMode: [true],
      paymentTermInDays: [false],
      deliveryTermInDays: [false],
      currencyId: [false],
      importPurchaseIncoTerm: [false],
      importPurchasePaymentTerm: [false],
      remark: [false],
      termAndCondition: [false],
      requestByName: [false],
      priority: [true],
      requisitionStatus: [true],
      actions: [true],
    });
    this.updateDisplayedColumns();
  }

  private subscribeToFormChanges() {
    merge(this.viewColumnForm.valueChanges).subscribe(() => {
      this.updateDisplayedColumns();
    });
  }

  updateDisplayedColumns() {
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

  getRequisitionStatus(value) {
    return this.statusColorService.getRequisitionStatus(value);
  }

  getPriority(value: number) {
    return this.statusColorService.getPriority(value);
  }

  getRequisitionStatusName(value: number) {
    return RequisitionStatus[value];
  }

  getPriorityName(value: number) {
    return Priority[value];
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

  getPurchaseRequisitions(): void {
    this.purchaseRequisitionRequest.requisitionStatusIds = [3, 4];
    this.purchaseRequisitionService
      .getPurchaseRequisitions(this.purchaseRequisitionRequest)
      .subscribe((res) => {
        console.log(res);
        this.dataSource =
          new MatTableDataSource<PurchaseRequisitionResponseDTO>(
            res?.data?.item1
          );
        this.totalCount = res?.data?.item2;
        this.loading = false;
      });
  }

  openForm(row?: any): void {
    const dialogRef = this.dialog.open(SendRFQToVendorEmailComponent, {
      disableClose: true,
      panelClass: "add-bill-container",
      minHeight: "calc(100vh - 90px)",
      height: "auto",
      data: row,
    });
    dialogRef.afterClosed().subscribe((result) => {
      this.getPurchaseRequisitions();
    });
  }

  private path = {
    edit: "purchase/send-rfq-to-vendor",
  };

  navigateToView(item) {
    this.router.navigate([this.path.edit, item.id]);
    // this.localStorageService.setItem(item.id, item);
  }

  // private path = {
  //   email: "purchase/send-rfq-to-vendor/email",
  // };

  // navigateToSendEmail(item) {
  //   this.router.navigate([this.path.email, item.id]);
  //   this.localStorageService.setItem(item.id, item);
  // }

  reload() {
    this.searchForm.reset();
    this.loading = true;
    this.getPurchaseRequisitions();
  }

  onSearch() {
    this.loading = true;
    this.purchaseRequisitionRequest.keyword = this.searchForm.value.keyword;
    this.getPurchaseRequisitions();
  }

  onPageChange(pageEvent) {
    this.purchaseRequisitionRequest.page = pageEvent.pageIndex;
    this.purchaseRequisitionRequest.rowsPerPage = pageEvent.pageSize;
    this.getPurchaseRequisitions();
  }

  trackBy(index: number, item: any) {
    return item?.id;
  }
}
