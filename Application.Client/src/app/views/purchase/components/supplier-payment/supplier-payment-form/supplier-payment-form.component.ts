import { HttpClient } from "@angular/common/http";
import { ChangeDetectorRef, Component, OnInit, ViewChild } from "@angular/core";
import { FormArray, FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MatButton } from "@angular/material/button";
import { MatDialog } from "@angular/material/dialog";
import { MatStepper } from "@angular/material/stepper";
import { ActivatedRoute, Router } from "@angular/router";
import {
  BANK,
  BKASH,
  CASH,
  CashAtBankId,
  CashInHandId,
  Control_Accounts_Parent_Id,
  MobileBankingId,
} from "app/shared/consts/const";
import { SupplierPaymentStatus } from "app/shared/enums/supplierPaymentStatus";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { LocalStoreService } from "app/shared/services/local-store.service";
import { SnackBarService } from "app/shared/services/snack-bar.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { inFinancialYearValidator } from "app/shared/validators/in-financial-year-validator";
import { ControlAccount } from "app/views/configuration/models/account/control-account.model";
import { CostCenter } from "app/views/configuration/models/cost-center/cost-center.model";
import { FundTransferTransactionType } from "app/views/configuration/models/fund-transfer-transaction-type/fund-transfer-transaction-type.model";
import { PaymentMode } from "app/views/configuration/models/payment-mode/payment-mode.model";
import { Supplier } from "app/views/configuration/models/supplier/supplier.model";
import { AccountService } from "app/views/configuration/services/account.service";
import { CostCenterService } from "app/views/configuration/services/cost-center.service";
import { FundTransferTransactionTypeService } from "app/views/configuration/services/fund-transfer-transaction-type.service";
import { PaymentModeService } from "app/views/configuration/services/payment-mode.service";
import { SupplierService } from "app/views/configuration/services/supplier.service";
import {
  SupplierPaymentRequestDTO,
  SupplierPaymentRequestDetail,
} from "app/views/purchase/models/supplier-payment/supplier-payment-request-dto.model";
import {
  SupplierPaymentResponseDTO,
  SupplierPaymentResponseDetail,
} from "app/views/purchase/models/supplier-payment/supplier-payment-response-dto.model";
import { SupplierPaymentService } from "app/views/purchase/services/supplier-payment.service";
import { conditionalRequiredValidator } from "app/views/purchase/validators/conditional-required-validator";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { finalize } from "rxjs";
import { PurchaseInvoiceDialogComponent } from "../purchase-invoice-dialog/purchase-invoice-dialog.component";
import { PurchaseOrderDialogComponent } from "../purchase-order-dialog/purchase-order-dialog.component";

@Component({
  selector: "app-supplier-payment-form",
  templateUrl: "./supplier-payment-form.component.html",
  styleUrls: ["./supplier-payment-form.component.scss"],
})
export class SupplierPaymentFormComponent implements OnInit {
  @ViewChild("btnCheck") btnCheck: MatButton;
  @ViewChild("btnApprove") btnApprove: MatButton;
  @ViewChild("stepper") private stepper: MatStepper;
  isLoading: boolean = false;
  isDialogOpen: boolean = false;
  isViewMode: boolean = true;
  isPurchaseInvoiceHide: boolean = false;
  isPurchaseOrderHide: boolean = false;
  isChecked: boolean;
  formTitle: string;
  supplierPaymentForm: FormGroup;
  suppliers: Supplier[];
  filterSuppliers: Supplier[];
  costCenters: CostCenter[];
  paymentModes: PaymentMode[];
  supplierPaymentTypes: ENUM[];
  filteredCashBankAccounts: ControlAccount[];
  cashBankAccounts: ControlAccount[];
  fundTransferTransactionTypes: FundTransferTransactionType[];
  supplierPaymentDetailsData: any[] = [];
  data: SupplierPaymentResponseDTO;
  // Define financial year date range here
  financialYearStartDate: Date; // Jul 1, 2023
  financialYearEndDate: Date; // Jun 30, 2024
  currentFinancialYearId: string;
  applyMinMax: boolean = false;

  private path = {
    list: "/purchase/supplier-payment",
    edit: "purchase/supplier-payment",
  };

  constructor(
    public dialog: MatDialog,
    private fb: FormBuilder,
    private fundTransferTransactionTypeService: FundTransferTransactionTypeService,
    private supplierService: SupplierService,
    private paymentModeService: PaymentModeService,
    private costCenterService: CostCenterService,
    private accountService: AccountService,
    private enumValueService: EnumValueService,
    private supplierPaymentService: SupplierPaymentService,
    private dateFormatService: DateTimeFormatService,
    private jwtAuth: JwtAuthService,
    private toastr: ToastrService,
    private route: ActivatedRoute,
    private router: Router,
    private http: HttpClient,
    private localStorageService: LocalStoreService,
    private cdRef: ChangeDetectorRef,
    public statusColorService: StatusColorService,
    private snackBarService: SnackBarService,
    private confirmDialogService: ConfirmDialogService,
    private activatedRoute: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.activatedRoute.data.subscribe((response: any) => {
      this.data = response?.supplierPayment?.data;
    });
    this.getData();
    this.jwtAuth.userProfile.subscribe((res: UserProfile) => {
      this.financialYearStartDate = new Date(res.fystartdate);
      this.financialYearEndDate = new Date(res.fyenddate);
      this.currentFinancialYearId = res.fyid;
    });
    this.initializeForm();
  }

  ngAfterViewInit() {
    const id = this.route.snapshot.paramMap.get("id");
    if (id) {
      this.stepper.selectedIndex = 1;
      this.btnCheck.focus();
      this.btnApprove.focus();
    } else {
      this.isViewMode = false;
      this.applyMinMax = true;
      this.setMinMaxDates();
    }
    this.cdRef.detectChanges();
  }

  showEdit() {
    this.stepper.previous();
    this.isViewMode = false;
    this.applyMinMax = true;
    this.initializeForm();
  }

  getData() {
    this.getAllSuppliers();
    this.getAllCostCenters();
    this.getAllPaymentModes();
    this.getSupplierPaymentTypes();
    this.getAllFundTransferTransactionTypes();
    //this.getAllControlAccounts();
  }

  initializeForm() {
    this.createForm();
    this.populateForm();
    this.setFormTitle();
    this.supplierPaymentForm.get("paymentDate").markAsTouched();
  }

  createForm(): void {
    this.supplierPaymentForm = this.fb.group({
      id: [this.data?.id || null],
      code: [this.data?.code || ""],
      supplierPaymentType: [
        this.data?.supplierPaymentType,
        Validators.required,
      ],
      //ponumber: [this.data?.ponumber, ""],
      ponumber: [
        this.data?.ponumber || "",
        [conditionalRequiredValidator(() => this.poSelectionCondition())],
      ],
      //purchaseInvoiceNo: [this.data?.purchaseInvoiceNo || ""],
      paymentDate: [
        this.data?.paymentDate || this.dateFormatService.getPresentDate(),
      ],
      fundTransferTransactionTypeId: [
        this.data?.fundTransferTransactionTypeId || null,
      ],
      transactionNumber: [this.data?.transactionNumber || ""],
      supplierId: [this.data?.supplierId ?? null, Validators.required],
      costCenterId: [this.data?.costCenterId ?? null, Validators.required],
      paymentModeId: [this.data?.paymentModeId ?? null, Validators.required],
      totalAmount: [this.data?.totalAmount || 0],
      remark: [this.data?.remark || ""],
      deletedSupplierPaymentDetailIds: [""],
      supplierPaymentDetails: this.fb.array([]),
    });
    this.setMinMaxDates();
  }

  setMinMaxDates() {
    if (this.applyMinMax) {
      this.supplierPaymentForm
        .get("paymentDate")
        .setValidators([
          Validators.required,
          inFinancialYearValidator(
            this.financialYearStartDate,
            this.financialYearEndDate
          ),
        ]);
    } else {
      this.supplierPaymentForm
        .get("paymentDate")
        .setValidators([Validators.required]);
    }

    this.supplierPaymentForm.get("paymentDate").updateValueAndValidity();
  }

  poSelectionCondition(): boolean {
    if (this.supplierPaymentForm?.get("supplierPaymentType")?.value) {
      if (this.supplierPaymentForm?.get("supplierPaymentType")?.value == 1)
        return true;
      else return false;
    } else return false;
  }

  purchaseInvoiceSelectionCondition(): boolean {
    if (this.supplierPaymentForm?.get("supplierPaymentType")?.value) {
      if (this.supplierPaymentForm?.get("supplierPaymentType")?.value == 2)
        return true;
      else return false;
    } else return false;
  }

  get supplierPaymentDetails(): FormArray {
    return this.supplierPaymentForm.get("supplierPaymentDetails") as FormArray;
  }

  populateForm(): void {
    if (this.supplierPaymentForm.get("id").value) {
      if (this.supplierPaymentForm.get("supplierPaymentType").value == 2) {
        this.isPurchaseInvoiceHide = true;
        this.isPurchaseOrderHide = false;
      } else {
        this.isPurchaseInvoiceHide = false;
        this.isPurchaseOrderHide = true;
      }

      this.populateSupplierPaymentDetails(this.data);
      this.getCashBankAccountsByPaymentModeId(this.data?.paymentModeId);
    } else {
      this.addItem();
      this.getAllCashBankAccounts(Control_Accounts_Parent_Id);
    }
  }

  setFormTitle(): void {
    if (this.supplierPaymentForm.get("id").value) {
      this.formTitle = "Edit Supplier Payment";
    } else {
      this.formTitle = "Add Supplier Payment";
    }
  }

  populateSupplierPaymentDetails(data: SupplierPaymentResponseDTO): void {
    data.supplierPaymentDetails.forEach((item: SupplierPaymentResponseDetail) =>
      this.addItem(item)
    );
  }

  addItem(item?: SupplierPaymentResponseDetail): void {
    this.supplierPaymentDetails.push(this.createSupplierPaymentDetail(item));
  }

  createSupplierPaymentDetail(item?: SupplierPaymentRequestDetail): FormGroup {
    return this.fb.group({
      id: [item?.id || null],
      accountId: [item?.accountId || "", Validators.required],
      account: [item?.account || ""],
      accountDescription: [item?.accountDescription || ""],
      purchaseInvoiceNo: [
        item?.purchaseInvoiceNo || "",
        [
          conditionalRequiredValidator(() =>
            this.purchaseInvoiceSelectionCondition()
          ),
        ],
      ],
      amount: [item?.amount, Validators.required],
    });
  }

  isPassingDataToNextStep = false;
  handleStepChange(event: any) {
    this.isPassingDataToNextStep = event.selectedIndex === 1;
    if (this.isPassingDataToNextStep) {
      this.supplierPaymentDetailsData = this.supplierPaymentForm.get(
        "supplierPaymentDetails"
      ).value;
    }
  }

  getAllFundTransferTransactionTypes() {
    this.fundTransferTransactionTypeService
      .getAllFundTransferTransactionTypes()
      .subscribe((res) => {
        this.fundTransferTransactionTypes = res?.data?.item1;
      });
  }

  getFundTransferTransactionTypeName(fundTransferTransactionTypeId: string) {
    return this.fundTransferTransactionTypes?.find(
      (x) => x.id === fundTransferTransactionTypeId
    )?.name;
  }

  getSupplierPaymentStatus(value) {
    return this.statusColorService.getSupplierPaymentStatus(value);
  }

  getSupplierPaymentStatusName(value: number) {
    return SupplierPaymentStatus[value];
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    if (fieldName === "supplierId") {
      this.supplierPaymentForm?.get("supplierId").setValue(null);
    }
  }

  handleSupplierSearch(event: any): void {
    const name = event.target?.name;
    if (name === "supplierId") {
      const term = this.supplierPaymentForm.get("supplierId");
      this.filterSupplier(term.value || "");
    }
  }

  private filterSupplier(value: string) {
    const filterValue = value.toLowerCase();
    this.filterSuppliers = this.suppliers?.filter((option) =>
      option.name.toLowerCase().includes(filterValue)
    );
  }

  getSupplierName(supplierId: string) {
    if (!supplierId) {
      return;
    }
    const supplierAccount =
      this.suppliers?.find((supplier) => supplier?.id === supplierId) ||
      this.data?.supplier;
    return supplierAccount?.name;
  }

  getAllSuppliers(): void {
    this.supplierService.getAllSuppliers().subscribe((res) => {
      this.filterSuppliers = this.suppliers = res?.data?.item1;
    });
  }

  getSupplierPaymentTypes() {
    return this.enumValueService.getSupplierPaymentTypes().subscribe((res) => {
      this.supplierPaymentTypes = res;
    });
  }

  getSupplierPaymentTypeName(supplierPaymentType: number) {
    if (!supplierPaymentType) {
      return;
    }
    const supplierPaymentTypeName =
      this.supplierPaymentTypes?.find(
        (type) => type?.value === supplierPaymentType
      ) || this.data?.supplierPaymentType;
    return supplierPaymentTypeName["name"]?.toString().replaceAll("_", " ");
  }

  getAllPaymentModes(): void {
    this.paymentModeService.getAllPaymentModes().subscribe((res) => {
      this.paymentModes = res?.data?.item1;
    });
  }

  getAllCostCenters(): void {
    this.costCenterService.getAllCostCenters().subscribe((res) => {
      this.costCenters = res?.data?.item1;
    });
  }

  getCostCenterName(costCenterId: string) {
    return this.costCenters?.find((x) => x.id === costCenterId).name;
  }

  getPaymentModeName(paymentModeId: string) {
    return this.paymentModes?.find((x) => x.id === paymentModeId).name;
  }

  clearInputFromDetails(evt: any, itemIndex: number): void {
    evt.stopPropagation();
    const particularDetail = this.supplierPaymentDetails.at(itemIndex);
    particularDetail?.patchValue({
      accountId: null,
    });
  }

  /** -----------Start Autocomplete----------- */
  handleCashBankAccountSearch(event: any, itemIndex: number): void {
    const name = event.target?.name;
    if (name === "accountId") {
      const term = this.supplierPaymentDetails.at(itemIndex).get("accountId");
      this.filterCashBankAccount(term.value || "");
    }
  }

  private filterCashBankAccount(value: string) {
    const filterValue = value.toLowerCase();
    this.filteredCashBankAccounts = this.cashBankAccounts?.filter((option) =>
      option.name.toLowerCase().includes(filterValue)
    );
  }

  getCashBankAccountName(accountId: string) {
    if (!accountId) {
      return;
    }
    const cashBankAccount =
      this.cashBankAccounts?.find((account) => account?.id === accountId) ||
      this.data?.supplierPaymentDetails?.find((x) => x?.accountId == accountId)
        ?.account;
    return cashBankAccount?.name;
  }

  handleCashBankAccountSelection(event, index) {
    const particularDetail = this.supplierPaymentDetails.at(index);
    const selectedAccount = this.findCashBankAccountById(event?.option?.value);

    if (!selectedAccount) return;
    particularDetail.patchValue({
      accountId: selectedAccount.id,
      account: selectedAccount,
      accountDescription: selectedAccount.treeName,
      amount: this.supplierPaymentDetails.at(index).get("amount").value,
      // purchaseInvoiceNo: this.supplierPaymentDetails
      //   .at(index)
      //   .get("purchaseInvoiceNo").value,
    });
  }

  findCashBankAccountById(id) {
    return this.cashBankAccounts.find((account) => account?.id === id);
  }

  cashBankLoading: boolean = false;
  getAllCashBankAccounts(parentId) {
    this.cashBankLoading = true;
    this.accountService
      .getControlAccountsByParentId(parentId)
      .subscribe((res) => {
        this.filteredCashBankAccounts = this.cashBankAccounts = res?.data;
        this.cashBankLoading = false;
      });
  }

  onSelectedPaymentMode(paymentModeId: string) {
    for (const control of this.supplierPaymentDetails.controls) {
      control.patchValue({
        accountId: null,
        account: null,
        accountDescription: "",
      });
    }
    this.filteredCashBankAccounts = [];
    if (paymentModeId == CASH) {
      this.getAllCashBankAccounts(CashInHandId);
    } else if (paymentModeId == BKASH) {
      this.getAllCashBankAccounts(MobileBankingId);
    } else if (paymentModeId == BANK) {
      this.getAllCashBankAccounts(CashAtBankId);
    } else {
      this.getAllCashBankAccounts(CashAtBankId);
    }
  }

  getCashBankAccountsByPaymentModeId(paymentModeId: string) {
    if (paymentModeId == CASH) {
      this.getAllCashBankAccounts(CashInHandId);
    } else if (paymentModeId == BKASH) {
      this.getAllCashBankAccounts(MobileBankingId);
    } else if (paymentModeId == BANK) {
      this.getAllCashBankAccounts(CashAtBankId);
    } else {
      this.getAllCashBankAccounts(CashAtBankId);
    }
  }

  /** ------------------End For Autocomplete---------- */

  onControlChange(event: any, itemIndex?: number): void {
    const name = event?.target?.name;
    if (name === "amount") {
      this.calculateAmount();
    }
  }

  calculateAmount() {
    const totalAmount = this.supplierPaymentDetails?.value?.reduce(
      (sum, item) => sum + item?.amount,
      0
    );
    this.supplierPaymentForm?.get("totalAmount")?.setValue(totalAmount);
  }

  private handleSuccessfulSave(res: GeneralResponse<string>): void {
    if (res?.succeeded) {
      this.handleSupplierPaymentResponse(res?.data);
      this.toastr.success(res?.message);
      this.deletedIds = "";
    } else {
      this.isLoading = false;
      this.toastr.error(res?.message);
    }
  }
  private handleSupplierPaymentResponse(supplierPaymentId: string): void {
    this.supplierPaymentService
      .getSupplierPaymentById(supplierPaymentId)
      .subscribe({
        next: (supplierPaymentResponse) => {
          this.data = supplierPaymentResponse.data;
          this.isViewMode = true;
          this.isLoading = false;
          this.navigateToView(supplierPaymentResponse?.data?.id);
        },
        error: (err) => {
          location.reload();
        },
      });
  }

  private navigateToView(id: string) {
    this.router.navigate([this.path.edit, id]);
    this.cdRef.detectChanges();
    this.btnCheck.focus();
  }

  private navigateToList() {
    this.router.navigate([this.path.list]);
  }

  backToView() {
    this.initializeForm();
    this.stepper.next();
    this.isViewMode = true;
  }

  private addSupplierPayment(body: SupplierPaymentRequestDTO): void {
    this.supplierPaymentService
      .createSupplierPayment(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  private updateSupplierPayment(body: SupplierPaymentRequestDTO): void {
    this.supplierPaymentService
      .updateSupplierPayment(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  deletedIds: string = "";
  onDeleteItem(id: string, itemIndex: number): void {
    if (id) this.deletedIds += `${id},`;
    this.supplierPaymentDetails.removeAt(itemIndex);
    this.calculateAmount();
  }

  OnChangePaymentType(paymentType: number) {
    this.commonResetMethod();
    if (paymentType == 2) {
      this.isPurchaseInvoiceHide = true;
      this.isPurchaseOrderHide = false;
    } else {
      this.isPurchaseInvoiceHide = false;
      this.isPurchaseOrderHide = true;
    }
  }

  onSupplierSelected(event: any): void {
    this.commonResetMethod();
  }

  commonResetMethod() {
    this.supplierPaymentForm.get("ponumber")?.setValue("");
    this.supplierPaymentForm.get("totalAmount")?.setValue(0);
    for (const control of this.supplierPaymentDetails.controls) {
      control.patchValue({
        accountId: null,
        account: null,
        accountDescription: "",
        purchaseInvoiceNo: "",
        amount: 0,
      });
    }
  }

  //openPurchaseOrderDialog
  openPurchaseOrderDialog() {
    if (this.supplierPaymentForm.get("supplierId")?.value) {
      const dialogRef = this.dialog.open(PurchaseOrderDialogComponent, {
        disableClose: true,
        panelClass: "add-bill-container",
        minHeight: "auto",
        height: "auto",
        data: this.supplierPaymentForm.get("supplierId")?.value,
      });
      dialogRef.afterClosed().subscribe((result) => {
        if (result) {
          // this.supplierPaymentForm.setControl(
          //   "supplierPaymentDetails",
          //   this.fb.array([])
          // );
          this.supplierPaymentForm.markAllAsTouched();

          this.supplierPaymentForm.patchValue({
            ponumber: result?.ponumber,
          });
        }
      });
    } else alert("Please Select Supplier First");
  }

  //openPurchaseInvoiceDialog
  openPurchaseInvoiceDialog(itemIndex?: number) {
    if (this.supplierPaymentForm.get("supplierId")?.value) {
      const dialogRef = this.dialog.open(PurchaseInvoiceDialogComponent, {
        disableClose: true,
        panelClass: "add-bill-container",
        minHeight: "auto",
        height: "auto",
        data: this.supplierPaymentForm.get("supplierId")?.value,
      });
      dialogRef.afterClosed().subscribe((result) => {
        if (result) {
          this.handlePurchaseInvoiceSelection(
            result.netPayable - result.paidAmount,
            result.purchaseInvoiceNo,
            itemIndex
          );
        }
        this.calculateAmount();
      });
    } else alert("Please Select Supplier First");
  }

  handlePurchaseInvoiceSelection(payableAmount, purchaseInvoiceNo, index) {
    const particularDetail = this.supplierPaymentDetails.at(index);
    if (this.isExistSelectedPurchaseInvoiceNo(purchaseInvoiceNo, index)) {
      this.showSnackBar("This Purchase Invoice already added!");
      particularDetail?.patchValue({
        purchaseInvoiceNo: "",
      });
      return;
    } else {
      const item = this.supplierPaymentDetails.at(index);
      item.patchValue({
        purchaseInvoiceNo: purchaseInvoiceNo,
        amount: payableAmount,
      });
    }
  }

  isExistSelectedPurchaseInvoiceNo(
    purchaseInvoiceNo: string,
    index: number
  ): boolean {
    const isPurchaseInvoiceAdded = this.supplierPaymentDetails.value.some(
      (item, i) => {
        return item.purchaseInvoiceNo === purchaseInvoiceNo && i !== index;
      }
    );
    return isPurchaseInvoiceAdded;
  }

  showSnackBar(message: string) {
    this.snackBarService.openSnackbar(message);
  }

  onSubmit(): void {
    if (this.supplierPaymentForm.valid) {
      this.isLoading = true;
      const formValue = this.supplierPaymentForm.value;
      if (!formValue.id) {
        this.addSupplierPayment(formValue);
      } else {
        formValue.deletedSupplierPaymentDetailIds = this.deletedIds;
        this.updateSupplierPayment(formValue);
      }
    }
  }

  check(id: string) {
    if (this.isLoading) return;
    this.isLoading = true;

    this.supplierPaymentService
      .checkSupplierPayment(id)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (res) => {
          if (res?.succeeded) {
            this.data.status = res?.data as number;
            this.toastr.info(res?.message);
            this.cdRef.detectChanges();
            this.btnApprove.focus();
          } else {
            this.toastr.error(res?.message);
          }
        },
        error: () => {
          this.toastr.error("Something went wrong");
        },
      });
  }

  approve(id: string) {
    if (this.isLoading) return;
    this.isLoading = true;

    this.supplierPaymentService
      .approveSupplierPayment(id)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (res) => {
          if (res?.succeeded) {
            this.data.status = res?.data as number;
            this.toastr.info(res.message);
          } else {
            this.toastr.error(res?.message);
          }
        },
        error: () => {
          this.toastr.error("Something went wrong");
        },
      });
  }

  unpost(id: string, status: number) {
    if (this.isLoading) return;
    this.isLoading = true;

    this.supplierPaymentService
      .unpostSupplierPayment(id, status)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (res) => {
          if (res?.succeeded) {
            this.data.status = res?.data as number;
            this.toastr.info(res.message);
          } else {
            this.toastr.error(res?.message);
          }
        },
        error: () => {
          this.toastr.error("Something went wrong");
        },
      });
  }

  confirmCheck(id: string, code: string = "") {
    if (this.isDialogOpen) return;
    this.isDialogOpen = true;

    const data: ConfirmDialogModel = {
      title: "Confirm Check",
      message: `Confirm status update to 'Checked' for item ${code}?`,
    };
    const dialogRef = this.confirmDialogService.confirmDialog(
      id,
      this.check.bind(this),
      data.message,
      data.title
    );

    dialogRef.afterClosed().subscribe(() => (this.isDialogOpen = false));
  }

  confirmApprove(id: string, code: string = "") {
    if (this.isDialogOpen) return;
    this.isDialogOpen = true;

    const data: ConfirmDialogModel = {
      title: "Confirm Approve",
      message: `Confirm status update to 'Approved' for item ${code}?`,
    };
    const dialogRef = this.confirmDialogService.confirmDialog(
      id,
      this.approve.bind(this),
      data.message,
      data.title
    );

    dialogRef.afterClosed().subscribe(() => (this.isDialogOpen = false));
  }

  confirmUnpost(id: string, code: string = "", status: number = 0) {
    if (this.isDialogOpen) return;
    this.isDialogOpen = true;

    const data: ConfirmDialogModel = {
      title: "Confirm Unpost",
      message: `This action will reverse the current status of item ${code}. Confirm the status reversal?`,
    };
    const dialogRef = this.confirmDialogService.confirmDialog(
      id,
      (id: string) => this.unpost(id, status),
      data.message,
      data.title
    );

    dialogRef.afterClosed().subscribe(() => (this.isDialogOpen = false));
  }

  printPdf(id) {
    this.http
      .get(environment.apiURL + "/ReportSupplierPayment/" + id, {
        responseType: "blob",
      })
      .subscribe((response) => {
        //Create a Blob from the PDF Stream
        const file = new Blob([response], { type: "application/pdf" });
        //Build a URL from the file
        const fileURL = URL.createObjectURL(file);
        //Open the URL on new Window
        const pdfWindow = window.open();
        pdfWindow.location.href = fileURL;
      });
  }
}
