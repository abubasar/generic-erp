import {
  animate,
  state,
  style,
  transition,
  trigger,
} from "@angular/animations";
import { SelectionModel } from "@angular/cdk/collections";
import { Component, Inject, OnInit, ViewChild } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { MAT_DIALOG_DATA, MatDialog } from "@angular/material/dialog";
import { MatMenuTrigger } from "@angular/material/menu";
import { MatTableDataSource } from "@angular/material/table";
import { SupplierPaymentStatus } from "app/shared/enums/supplierPaymentStatus";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { SupplierPaymentResponseDTO } from "app/views/purchase/models/supplier-payment/supplier-payment-response-dto.model";
import { SupplierPaymentSearchRequestDTO } from "app/views/purchase/models/supplier-payment/supplier-payment-search-request-dto.model";
import { SupplierPaymentService } from "app/views/purchase/services/supplier-payment.service";
import { ToastrService } from "ngx-toastr";
import { Observable, merge, of } from "rxjs";

@Component({
  selector: "app-supplier-payment-dialog",
  templateUrl: "./supplier-payment-dialog.component.html",
  styleUrls: ["./supplier-payment-dialog.component.scss"],
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
export class SupplierPaymentDialogComponent implements OnInit {
  loading: boolean = true;
  //selectedSupplierPayment: SupplierPaymentResponseDTO = null;
  selection = new SelectionModel<SupplierPaymentResponseDTO>(true, []); // its need for multiple selection
  supplierPayments: SupplierPaymentResponseDTO[];
  supplierPaymentRequest = new SupplierPaymentSearchRequestDTO();

  panelOpenState: boolean;
  searchForm: FormGroup;
  viewColumnForm: FormGroup;
  expandedElement: any;
  dataSource: MatTableDataSource<SupplierPaymentResponseDTO>;
  totalCount: number;
  supplierPaymentStatuses: ENUM[];
  displayedColumns$: Observable<string[]>;

  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  //  * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "select", label: "Select" },
    { def: "expand", label: "Expand" },
    { def: "code", label: "Code" },
    { def: "supplierPaymentType", label: "Supplier Payment Type" },
    { def: "ponumber", label: "PO No" },
    { def: "paymentDate", label: "Payment Date" },
    { def: "supplierId", label: "Supplier" },
    { def: "costCenterId", label: "Cost Center" },
    { def: "paymentModeId", label: "Payment Mode" },
    { def: "totalAmount", label: "Total Amount" },
    {
      def: "usedAmountInPurchaseInvoice",
      label: "Used Amount In Purchase Invoice",
    },
    { def: "remark", label: "Remark" },
    { def: "status", label: "Status" },
    { def: "checkedBy", label: "Checked By" },
    { def: "approvedBy", label: "Approved By" },
    { def: "createdOn", label: "Created On" },
    { def: "updatedOn", label: "Updated On" },
    { def: "createdBy", label: "Created By" },
    { def: "updatedBy", label: "Updated By" },
  ];

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    public dateFormatService: DateTimeFormatService,
    public statusColorService: StatusColorService,
    private supplierPaymentService: SupplierPaymentService,
    private enumValueService: EnumValueService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.getAllSupplierPaymentStatuses();
    this.getSupplierPayments();
    this.subscribeToFormChanges();
  }

  private initializeForm() {
    this.searchForm = this.fb.group({
      saleOrderNo: [""],
    });
    this.viewColumnForm = this.fb.group({
      select: [true],
      expand: [true],
      code: [true],
      supplierPaymentType: [true],
      ponumber: [true],
      paymentDate: [true],
      supplierId: [true],
      costCenterId: [false],
      paymentModeId: [false],
      totalAmount: [true],
      usedAmountInPurchaseInvoice: [true],
      status: [true],
      remark: [false],
      checkedBy: [false],
      approvedBy: [false],
      createdOn: [false],
      updatedOn: [false],
      createdBy: [false],
      updatedBy: [false],
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

  selectedPaymentAmount: number = 0;
  isDisable: boolean = false;

  selectedAmount(testData: any) {
    const payment = testData.reduce(
      (sum, item) =>
        sum + item?.totalAmount - item?.usedAmountInPurchaseInvoice,
      0
    );
    this.selectedPaymentAmount = payment;
    if (this.selectedPaymentAmount >= this.data.total) this.isDisable = true;
    else this.isDisable = false;
  }

  getAllSupplierPaymentStatuses() {
    this.enumValueService.getSaleOrderStatuses().subscribe((res) => {
      this.supplierPaymentStatuses = res;
    });
  }

  getSupplierPaymentStatus(value: number) {
    return this.statusColorService.getSupplierPaymentStatus(value);
  }

  getSupplierPaymentStatusName(value: number) {
    return SupplierPaymentStatus[value];
  }

  getSupplierPayments(): void {
    this.supplierPaymentRequest.page = -1;
    this.supplierPaymentRequest.supplierPaymentType = 1;
    this.supplierPaymentRequest.supplierId = this.data?.supplierId;
    this.supplierPaymentRequest.keyword = this.data?.ponumber;
    this.supplierPaymentRequest.supplierPaymentStatuses = [3, 4];
    this.supplierPaymentService
      .getSupplierPayments(this.supplierPaymentRequest)
      .subscribe((res) => {
        this.dataSource = new MatTableDataSource<SupplierPaymentResponseDTO>(
          res?.data?.item1
        );
        this.totalCount = res?.data?.item2;
        this.loading = false;
      });
  }

  // onPageChange(pageEvent) {
  //   this.supplierPaymentRequest.page = pageEvent.pageIndex;
  //   this.supplierPaymentRequest.rowsPerPage = pageEvent.pageSize;
  //   this.getSupplierPayments(this.searchForm.get("saleOrderNo").value);
  // }
}
