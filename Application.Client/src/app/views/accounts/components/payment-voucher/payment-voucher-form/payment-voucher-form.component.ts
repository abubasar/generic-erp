import { HttpClient } from "@angular/common/http";
import { ChangeDetectorRef, Component, OnInit, ViewChild } from "@angular/core";
import { FormArray, FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MatButton } from "@angular/material/button";
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
import { PaymentVoucherStatus } from "app/shared/enums/paymentVoucherStatus";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { SnackBarService } from "app/shared/services/snack-bar.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { inFinancialYearValidator } from "app/shared/validators/in-financial-year-validator";
import {
  PaymentVoucherRequestDTO,
  PaymentVoucherRequestDetail,
} from "app/views/accounts/models/payment-voucher/payment-voucher-request-dto.model";
import {
  PaymentVoucherResponseDTO,
  PaymentVoucherResponseDetail,
} from "app/views/accounts/models/payment-voucher/payment-voucher-response-dto.model";
import { PaymentVoucherService } from "app/views/accounts/services/payment-voucher.service";
import { ControlAccount } from "app/views/configuration/models/account/control-account.model";
import { CostCenter } from "app/views/configuration/models/cost-center/cost-center.model";
import { FundTransferTransactionType } from "app/views/configuration/models/fund-transfer-transaction-type/fund-transfer-transaction-type.model";
import { PaymentMode } from "app/views/configuration/models/payment-mode/payment-mode.model";
import { AccountService } from "app/views/configuration/services/account.service";
import { CostCenterService } from "app/views/configuration/services/cost-center.service";
import { FundTransferTransactionTypeService } from "app/views/configuration/services/fund-transfer-transaction-type.service";
import { PaymentModeService } from "app/views/configuration/services/payment-mode.service";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { finalize } from "rxjs";

@Component({
  selector: "app-payment-voucher-form",
  templateUrl: "./payment-voucher-form.component.html",
  styleUrls: ["./payment-voucher-form.component.scss"],
})
export class PaymentVoucherFormComponent implements OnInit {
  @ViewChild("btnCheck") btnCheck: MatButton;
  @ViewChild("btnApprove") btnApprove: MatButton;
  @ViewChild("stepper") private stepper: MatStepper;
  isLoading: boolean = false;
  isDialogOpen: boolean = false;
  isViewMode: boolean = true;
  isChecked: boolean;
  formTitle: string;
  paymentVoucherForm: FormGroup;
  costCenters: CostCenter[];
  paymentModes: PaymentMode[];
  cashBankAccounts: ControlAccount[];
  controlAccounts: ControlAccount[];
  filterControlAccounts: ControlAccount[];
  filteredCashBankAccounts: ControlAccount[];
  // voucherTypes: ENUM[];
  fundTransferTransactionTypes: FundTransferTransactionType[];
  paymentVoucherDetailsData: any[] = [];
  data: PaymentVoucherResponseDTO;
  // Define financial year date range here
  financialYearStartDate: Date; // Jul 1, 2023
  financialYearEndDate: Date; // Jun 30, 2024
  currentFinancialYearId: string;
  applyMinMax: boolean = false;

  private path = {
    list: "accounts/cash-payment-voucher",
    edit: "accounts/cash-payment-voucher",
  };

  constructor(
    private fb: FormBuilder,
    private fundTransferTransactionTypeService: FundTransferTransactionTypeService,
    private paymentModeService: PaymentModeService,
    private costCenterService: CostCenterService,
    private enumValueService: EnumValueService,
    private accountService: AccountService,
    private paymentVoucherService: PaymentVoucherService,
    private dateFormatService: DateTimeFormatService,
    private jwtAuth: JwtAuthService,
    private toastr: ToastrService,
    private http: HttpClient,
    private activatedRoute: ActivatedRoute,
    private router: Router,
    private cdRef: ChangeDetectorRef,
    public statusColorService: StatusColorService,
    private snackBarService: SnackBarService,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.activatedRoute.data.subscribe((response: any) => {
      this.data = response?.paymentVoucher?.data;
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
    const id = this.activatedRoute.snapshot.paramMap.get("id");
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
    this.getAllCostCenters();
    this.getAllPaymentModes();
    this.getAllFundTransferTransactionTypes();
    // this.getVoucherTypes();
    this.getAllControlAccountsExcludingSuppliers();
  }

  initializeForm() {
    this.createForm();
    this.populateForm();
    this.setFormTitle();
    this.paymentVoucherForm.get("voucherDate").markAsTouched();
  }

  createForm(): void {
    this.paymentVoucherForm = this.fb.group({
      id: [this.data?.id || null],
      voucherNo: [this.data?.voucherNo || ""],
      voucherDate: [
        this.data?.voucherDate || this.dateFormatService.getPresentDate(),
      ],
      fundTransferTransactionTypeId: [
        this.data?.fundTransferTransactionTypeId || null,
      ],
      transactionNumber: [this.data?.transactionNumber || ""],
      costCenterId: [this.data?.costCenterId, Validators.required],
      paymentModeId: [this.data?.paymentModeId, Validators.required],
      referenceNo: [this.data?.referenceNo || ""],
      //voucherType: [this.data?.voucherType, Validators.required],
      cashBankAccountId: [this.data?.cashBankAccountId, Validators.required],
      totalAmount: [this.data?.totalAmount || 0],
      remark: [this.data?.remark || ""],
      deletedPaymentVoucherDetailIds: [""],
      paymentVoucherDetails: this.fb.array([]),
    });
    this.setMinMaxDates();
  }

  setMinMaxDates() {
    if (this.applyMinMax) {
      this.paymentVoucherForm
        .get("voucherDate")
        .setValidators([
          Validators.required,
          inFinancialYearValidator(
            this.financialYearStartDate,
            this.financialYearEndDate
          ),
        ]);
    } else {
      this.paymentVoucherForm
        .get("voucherDate")
        .setValidators([Validators.required]);
    }

    this.paymentVoucherForm.get("voucherDate").updateValueAndValidity();
  }

  get paymentVoucherDetails(): FormArray {
    return this.paymentVoucherForm.get("paymentVoucherDetails") as FormArray;
  }

  populateForm(): void {
    if (this.paymentVoucherForm.get("id").value) {
      this.populatePaymentVoucherDetails(this.data);
      this.getCashBankAccountsByPaymentModeId(this.data?.paymentModeId);
    } else {
      this.addItem();
      this.getAllCashBankAccounts(Control_Accounts_Parent_Id);
    }
  }

  setFormTitle(): void {
    if (this.paymentVoucherForm.get("id").value) {
      this.formTitle = "Edit Cash Payment Voucher";
    } else {
      this.formTitle = "Add Cash Payment Voucher";
    }
  }

  populatePaymentVoucherDetails(data: PaymentVoucherResponseDTO): void {
    data.paymentVoucherDetails.forEach((item: PaymentVoucherResponseDetail) =>
      this.addItem(item)
    );
  }

  addItem(item?: PaymentVoucherResponseDetail): void {
    this.paymentVoucherDetails.push(this.createPaymentVoucherDetail(item));
  }

  createPaymentVoucherDetail(item?: PaymentVoucherRequestDetail): FormGroup {
    return this.fb.group({
      id: [item?.id || null],
      accountId: [item?.accountId || null, Validators.required],
      account: [item?.account || null],
      accountDescription: [item?.accountDescription || ""],
      amount: [item?.amount, Validators.required],
    });
  }

  isPassingDataToNextStep = false;
  handleStepChange(event: any) {
    this.isPassingDataToNextStep = event.selectedIndex === 1;
    if (this.isPassingDataToNextStep) {
      this.paymentVoucherDetailsData = this.paymentVoucherForm.get(
        "paymentVoucherDetails"
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

  getPaymentVoucherStatus(value) {
    return this.statusColorService.getPaymentVoucherStatus(value);
  }

  getPaymentVoucherStatusName(value: number) {
    return PaymentVoucherStatus[value];
  }

  // getVoucherTypes() {
  //   // return this.enumValueService.getVoucherTypes().subscribe((res) => {
  //   //   this.voucherTypes = res;
  //   // });
  //   this.voucherTypes = Object.keys(VoucherType).map((key) => ({
  //     name: key,
  //     value: VoucherType[key] as number,
  //   }));
  // }

  // getVoucherTypeName(value: string) {
  //   if (value == "1") return "Cash_Payment_Voucher";
  //   else return "Cash_Received_Voucher";
  // }

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

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    if (fieldName === "cashBankAccountId") {
      this.paymentVoucherForm?.get("cashBankAccountId").setValue(null);
    }
  }

  clearInputFromDetails(evt: any, itemIndex: number): void {
    evt.stopPropagation();
    const particularDetail = this.paymentVoucherDetails.at(itemIndex);
    particularDetail?.patchValue({
      accountId: null,
    });
  }

  handleCashBankAccountSearch(event: any): void {
    const name = event.target?.name;
    if (name === "cashBankAccountId") {
      const term = this.paymentVoucherForm.get("cashBankAccountId");
      this.filterCashBankAccount(term.value || "");
    }
  }

  private filterCashBankAccount(value: string) {
    const filterValue = value.toLowerCase();
    this.filteredCashBankAccounts = this.cashBankAccounts?.filter((option) =>
      option.name.toLowerCase().includes(filterValue)
    );
  }

  getCashBankAccountName(cashBankAccountId: string) {
    if (!cashBankAccountId) {
      return;
    }
    const cashBankAccount =
      this.cashBankAccounts?.find(
        (cashBankAccount) => cashBankAccount?.id === cashBankAccountId
      ) || this.data?.cashBankAccount;
    return cashBankAccount?.name;
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

  // getAllCashBankAccounts(parentId) {
  //   this.accountService
  //     .getControlAccountsByParentId(parentId)
  //     .subscribe((res) => {
  //       this.cashBankAccounts = res?.data;
  //     });
  // }

  //getCashBankAccountName(cashBankAccountId: string) {
  //return this.cashBankAccounts?.find((x) => x?.id === cashBankAccountId).name;
  //}

  getCostCenterName(costCenterId: string) {
    return this.costCenters?.find((x) => x.id === costCenterId).name;
  }

  getPaymentModeName(paymentModeId: string) {
    return this.paymentModes?.find((x) => x.id === paymentModeId).name;
  }

  onSelectedPaymentMode(paymentModeId: string) {
    this.paymentVoucherForm.get("cashBankAccountId")?.setValue(null);
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

  /** -----------Start Autocomplete----------- */
  getAllControlAccountsExcludingSuppliers() {
    this.accountService
      .getAllControlAccountsExcludingSuppliers()
      .subscribe((res) => {
        this.filterControlAccounts = this.controlAccounts = res?.data;
      });
  }

  handleControlAccountSearch(event: any, itemIndex: number): void {
    const name = event.target?.name;
    if (name === "accountId") {
      const term = this.paymentVoucherDetails.at(itemIndex).get("accountId");
      this.filterControlAccount(term.value || "");
    }
  }

  private filterControlAccount(value: string) {
    const filterValue = value.toLowerCase();
    this.filterControlAccounts = this.controlAccounts?.filter(
      (option) =>
        option.name.toLowerCase().includes(filterValue) ||
        option.code?.split(".")?.pop()?.toLowerCase().includes(filterValue)
    );
  }

  findControlAccountById(id) {
    return this.controlAccounts.find((account) => account?.id === id);
  }

  handleControlAccountSelection(event, index) {
    const particularDetail = this.paymentVoucherDetails.at(index);
    const accountId = event?.option?.value;

    if (this.isExistSelectedAccount(accountId, index)) {
      this.showSnackBar("This Account is already added!");
      particularDetail?.patchValue({
        accountId: null,
      });
      return;
    }
    const selectedAccount = this.findControlAccountById(event?.option?.value);

    if (!selectedAccount) return;

    particularDetail.patchValue({
      accountId: selectedAccount.id,
      account: selectedAccount,
      accountDescription: selectedAccount.treeName,
      amount: this.paymentVoucherDetails.at(index).get("amount").value,
    });
  }

  showSnackBar(message: string) {
    this.snackBarService.openSnackbar(message);
  }

  isExistSelectedAccount(accountId: string, index: number): boolean {
    const isProductAdded = this.paymentVoucherDetails.value.some((item, i) => {
      return item.accountId === accountId && i !== index;
    });
    return isProductAdded;
  }

  getControlAccountName(controlAccountId: string) {
    if (!controlAccountId) {
      return;
    }
    const controlAccount =
      this.controlAccounts?.find(
        (account) => account?.id === controlAccountId
      ) ||
      this.data?.paymentVoucherDetails?.find(
        (x) => x?.accountId == controlAccountId
      )?.account;
    return (
      "(" + controlAccount.code?.split(".")?.pop() + ") " + controlAccount?.name
    );
  }

  onControlChange(event: any, itemIndex?: number): void {
    const name = event?.target?.name;
    if (name === "amount") {
      this.calculateAmount();
    }
  }

  calculateAmount() {
    const totalAmount = this.paymentVoucherDetails?.value?.reduce(
      (sum, item) => sum + item?.amount,
      0
    );
    this.paymentVoucherForm?.get("totalAmount")?.setValue(totalAmount);
  }

  /** ------------------End For Autocomplete---------- */

  private handleSuccessfulSave(res: GeneralResponse<string>): void {
    if (res?.succeeded) {
      this.handlePaymentVoucherResponse(res?.data);
      this.toastr.success(res?.message);
      this.deletedIds = "";
    } else {
      this.isLoading = false;
      this.toastr.error(res?.message);
    }
  }

  private navigateToView(id: string) {
    this.router.navigate([this.path.edit, id]);
    this.cdRef.detectChanges();
    this.btnCheck.focus();
  }

  navigateToList() {
    this.router.navigate([this.path.list]);
  }

  private handlePaymentVoucherResponse(paymentVoucherId: string): void {
    this.paymentVoucherService
      .getPaymentVoucherById(paymentVoucherId)
      .subscribe({
        next: (paymentVoucherResponse) => {
          this.data = paymentVoucherResponse.data;
          this.isViewMode = true;
          this.isLoading = false;
          this.navigateToView(paymentVoucherResponse?.data?.id);
        },
        error: (err) => {
          location.reload();
        },
      });
  }

  backToView() {
    this.initializeForm();
    this.stepper.next();
    this.isViewMode = true;
  }

  private addPaymentVoucher(body: PaymentVoucherRequestDTO): void {
    this.paymentVoucherService
      .createPaymentVoucher(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  private updatePaymentVoucher(body: PaymentVoucherRequestDTO): void {
    this.paymentVoucherService
      .updatePaymentVoucher(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  deletedIds: string = "";
  onDeleteItem(id: string, itemIndex: number): void {
    if (id) this.deletedIds += `${id},`;
    this.paymentVoucherDetails.removeAt(itemIndex);
    this.calculateAmount();
  }

  onSubmit(): void {
    if (this.paymentVoucherForm.valid) {
      this.isLoading = true;
      const formValue = this.paymentVoucherForm.value;
      if (!formValue.id) {
        this.addPaymentVoucher(formValue);
      } else {
        formValue.deletedPaymentVoucherDetailIds = this.deletedIds;
        this.updatePaymentVoucher(formValue);
      }
    }
  }

  check(id: string) {
    if (this.isLoading) return;
    this.isLoading = true;

    this.paymentVoucherService
      .checkPaymentVoucher(id)
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

    this.paymentVoucherService
      .approvePaymentVoucher(id)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (res) => {
          if (res?.succeeded) {
            this.data.status = res?.data as number;
            this.toastr.info(res?.message);
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

    this.paymentVoucherService
      .unpostPaymentVoucher(id, status)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (res) => {
          if (res?.succeeded) {
            this.data.status = res?.data as number;
            this.toastr.info(res?.message);
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
      .get(environment.apiURL + "/ReportPaymentVoucher/" + id, {
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
