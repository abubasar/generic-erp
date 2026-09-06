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
import { ReceivePaymentStatus } from "app/shared/enums/receivePaymentStatus";
import { NumberToWordsConverter } from "app/shared/helpers/number-to-word-converter";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { inFinancialYearValidator } from "app/shared/validators/in-financial-year-validator";
import { ControlAccount } from "app/views/configuration/models/account/control-account.model";
import { CostCenter } from "app/views/configuration/models/cost-center/cost-center.model";
import { Customer } from "app/views/configuration/models/customer/customer.model";
import { FundTransferTransactionType } from "app/views/configuration/models/fund-transfer-transaction-type/fund-transfer-transaction-type.model";
import { PaymentMode } from "app/views/configuration/models/payment-mode/payment-mode.model";
import { AccountService } from "app/views/configuration/services/account.service";
import { CostCenterService } from "app/views/configuration/services/cost-center.service";
import { CustomerService } from "app/views/configuration/services/customer.service";
import { FundTransferTransactionTypeService } from "app/views/configuration/services/fund-transfer-transaction-type.service";
import { PaymentModeService } from "app/views/configuration/services/payment-mode.service";
import {
  ReceivePaymentRequestDTO,
  ReceivePaymentRequestDetail,
} from "app/views/sales/models/receive-payment/receive-payment-request-dto.model";
import {
  ReceivePaymentResponseDTO,
  ReceivePaymentResponseDetail,
} from "app/views/sales/models/receive-payment/receive-payment-response-dto.model";
import { ReceivePaymentService } from "app/views/sales/services/receive-payment.service";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { finalize } from "rxjs";

@Component({
  selector: "app-receive-payment-form",
  templateUrl: "./receive-payment-form.component.html",
  styleUrls: ["./receive-payment-form.component.scss"],
})
export class ReceivePaymentFormComponent implements OnInit {
  @ViewChild("btnCheck") btnCheck: MatButton;
  @ViewChild("btnApprove") btnApprove: MatButton;
  @ViewChild("stepper") private stepper: MatStepper;
  isLoading: boolean = false;
  isDialogOpen: boolean = false;
  isViewMode: boolean = true;
  isChecked: boolean;
  formTitle: string;
  receivePaymentForm: FormGroup;
  customers: Customer[];
  filterCustomers: Customer[];
  costCenters: CostCenter[];
  paymentModes: PaymentMode[];
  filteredCashBankAccounts: ControlAccount[];
  cashBankAccounts: ControlAccount[];
  fundTransferTransactionTypes: FundTransferTransactionType[];
  receivePaymentDetailsData: any[] = [];
  data: ReceivePaymentResponseDTO;

  // Define financial year date range here
  financialYearStartDate: Date; //Example: Jul 1, 2023
  financialYearEndDate: Date; //Example: Jun 30, 2024
  currentFinancialYearId: string;
  applyMinMax: boolean = false;

  private path = {
    list: "sales/money-receipt",
    edit: "sales/money-receipt",
  };

  constructor(
    private fb: FormBuilder,
    public dialog: MatDialog,
    private fundTransferTransactionTypeService: FundTransferTransactionTypeService,
    private customerService: CustomerService,
    private paymentModeService: PaymentModeService,
    private costCenterService: CostCenterService,
    private accountService: AccountService,
    private receivePaymentService: ReceivePaymentService,
    private jwtAuth: JwtAuthService,
    public dateFormatService: DateTimeFormatService,
    private toastr: ToastrService,
    private activatedRoute: ActivatedRoute,
    private router: Router,
    private cdRef: ChangeDetectorRef,
    private http: HttpClient,
    public statusColorService: StatusColorService,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.activatedRoute.data.subscribe((response: any) => {
      this.data = response?.receivePayment?.data;
    });
    this.jwtAuth.userProfile.subscribe((res: UserProfile) => {
      this.financialYearStartDate = new Date(res.fystartdate);
      this.financialYearEndDate = new Date(res.fyenddate);
      this.currentFinancialYearId = res.fyid;
    });
    this.getData();
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
    this.getAllCustomers();
    this.getAllCostCenters();
    this.getAllPaymentModes();
    this.getAllFundTransferTransactionTypes();
    //this.getAllControlAccounts();
  }

  initializeForm() {
    this.createForm();
    this.populateForm();
    this.setFormTitle();
    this.receivePaymentForm.get("paymentDate").markAsTouched();
  }

  createForm(): void {
    this.receivePaymentForm = this.fb.group({
      id: [this.data?.id || null],
      code: [this.data?.code || ""],
      paymentDate: [
        this.data?.paymentDate || this.dateFormatService.getPresentDate(),
      ],
      customerId: [this.data?.customerId, Validators.required],
      fundTransferTransactionTypeId: [
        this.data?.fundTransferTransactionTypeId || null,
      ],
      transactionNumber: [this.data?.transactionNumber || ""],
      costCenterId: [this.data?.costCenterId, Validators.required],
      paymentModeId: [this.data?.paymentModeId, Validators.required],
      totalAmount: [this.data?.totalAmount || 0],
      feedSalesPurpose: [this.data?.feedSalesPurpose || 0],
      creditRecoveryPurpose: [this.data?.creditRecoveryPurpose || 0],
      remark: [this.data?.remark || ""],
      deletedReceivePaymentDetailIds: [""],
      receivePaymentDetails: this.fb.array([]),
    });
    this.setMinMaxDates();
  }

  setMinMaxDates() {
    if (this.applyMinMax) {
      this.receivePaymentForm
        .get("paymentDate")
        .setValidators([
          Validators.required,
          inFinancialYearValidator(
            this.financialYearStartDate,
            this.financialYearEndDate
          ),
        ]);
    } else {
      this.receivePaymentForm
        .get("paymentDate")
        .setValidators([Validators.required]);
    }
    this.receivePaymentForm.get("paymentDate").updateValueAndValidity();
  }

  get receivePaymentDetails(): FormArray {
    return this.receivePaymentForm.get("receivePaymentDetails") as FormArray;
  }

  populateForm(): void {
    if (this.receivePaymentForm.get("id").value) {
      this.populateReceivePaymentDetails(this.data);
      this.getCashBankAccountsByPaymentModeId(this.data?.paymentModeId);
    } else {
      this.addItem();
      this.getAllCashBankAccounts(Control_Accounts_Parent_Id);
    }
  }

  setFormTitle(): void {
    if (this.receivePaymentForm.get("id").value) {
      this.formTitle = "Edit Money Receipt";
    } else {
      this.formTitle = "Add Money Receipt";
    }
  }

  populateReceivePaymentDetails(data: ReceivePaymentResponseDTO): void {
    data.receivePaymentDetails.forEach((item: ReceivePaymentResponseDetail) =>
      this.addItem(item)
    );
  }

  addItem(item?: ReceivePaymentResponseDetail): void {
    this.receivePaymentDetails.push(this.createReceivePaymentDetail(item));
  }

  createReceivePaymentDetail(item?: ReceivePaymentRequestDetail): FormGroup {
    return this.fb.group({
      id: [item?.id || null],
      accountId: [item?.accountId || null, Validators.required],
      account: [item?.account || null],
      accountDescription: [item?.accountDescription || ""],
      amount: [item?.amount, Validators.required],
    });
  }

  customerBalance: number = 0;
  onSelectedCustomer(customerId: string) {
    this.customerService
      .getCustomerCreditLimitBalance(customerId)
      .subscribe((res) => {
        this.customerBalance = res?.data?.balance;
      });
  }

  isPassingDataToNextStep = false;
  handleStepChange(event: any) {
    this.isPassingDataToNextStep = event.selectedIndex === 1;
    if (this.isPassingDataToNextStep) {
      this.receivePaymentDetailsData = this.receivePaymentForm.get(
        "receivePaymentDetails"
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

  getReceivePaymentStatus(value) {
    return this.statusColorService.getReceivePaymentStatus(value);
  }

  getReceivePaymentStatusName(value: number) {
    return ReceivePaymentStatus[value];
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    if (fieldName === "customerId")
      this.receivePaymentForm?.get("customerId").setValue(null);
  }

  clearInputFromDetails(evt: any, itemIndex: number): void {
    evt.stopPropagation();
    const particularDetail = this.receivePaymentDetails.at(itemIndex);
    particularDetail?.patchValue({
      accountId: null,
    });
  }

  handleCustomerSearch(event: any): void {
    const name = event.target?.name;
    if (name === "customerId") {
      const term = this.receivePaymentForm.get("customerId");
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
    const customerAccount =
      this.customers?.find((customer) => customer?.id === customerId) ||
      this.data?.customer;
    return customerAccount?.name;
  }

  getAllCustomers(): void {
    this.customerService.getAllCustomers().subscribe((res) => {
      this.filterCustomers = this.customers = res?.data?.item1;
    });
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

  /** -----------Start Autocomplete----------- */
  handleCashBankAccountSearch(event: any, itemIndex: number): void {
    const name = event.target?.name;
    if (name === "accountId") {
      const term = this.receivePaymentDetails.at(itemIndex).get("accountId");
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
      this.data?.receivePaymentDetails?.find((x) => x?.accountId == accountId)
        ?.account;
    return cashBankAccount?.name;
  }

  handleCashBankAccountSelection(event, index) {
    const particularDetail = this.receivePaymentDetails.at(index);
    const selectedAccount = this.findCashBankAccountById(event?.option?.value);

    if (!selectedAccount) return;
    particularDetail.patchValue({
      accountId: selectedAccount.id,
      account: selectedAccount,
      accountDescription: selectedAccount.treeName,
      amount: this.receivePaymentDetails.at(index).get("amount").value,
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
    for (const control of this.receivePaymentDetails.controls) {
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

  onCreditRecoveryAmountChange(event: any): void {
    const name = event?.target?.name;
    if (name === "creditRecoveryPurpose") {
      this.calculateFeedSalesPurposeAmount(event?.target?.value);
      if (event?.target?.value > this.customerBalance) {
        alert("Exceeded Customer Due " + this.customerBalance);
        this.receivePaymentForm.get("creditRecoveryPurpose").setValue(0);
      }
    }
  }

  calculateAmount() {
    const totalAmount = this.receivePaymentDetails?.value?.reduce(
      (sum, item) => sum + item?.amount,
      0
    );
    this.receivePaymentForm?.get("totalAmount")?.setValue(totalAmount);
    this.receivePaymentForm?.get("feedSalesPurpose")?.setValue(totalAmount);
  }

  calculateFeedSalesPurposeAmount(creditRecoveryPurposeAmount) {
    var totalAmount = this.receivePaymentForm?.get("totalAmount").value;
    this.receivePaymentForm
      ?.get("feedSalesPurpose")
      ?.setValue(totalAmount - creditRecoveryPurposeAmount);
  }

  /** ------------------End For Autocomplete---------- */

  private handleSuccessfulSave(res: GeneralResponse<string>): void {
    if (res?.succeeded) {
      this.handleReceivePaymentResponse(res?.data);
      this.toastr.success(res?.message);
      this.deletedIds = "";
    } else {
      this.isLoading = false;
      this.toastr.error(res?.message);
    }
  }

  private handleReceivePaymentResponse(receivePaymentId: string): void {
    this.receivePaymentService
      .getReceivePaymentById(receivePaymentId)
      .subscribe({
        next: (receivePaymentResponse) => {
          this.data = receivePaymentResponse.data;
          this.isViewMode = true;
          this.isLoading = false;
          this.navigateToView(receivePaymentResponse?.data?.id);
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

  convertAmount(amount: number): string {
    const converter = new NumberToWordsConverter();
    return converter.convertAmount(amount);
  }

  navigateToList() {
    this.router.navigate([this.path.list]);
  }

  backToView() {
    this.initializeForm();
    this.stepper.next();
    this.isViewMode = true;
  }

  private addReceivePayment(body: ReceivePaymentRequestDTO): void {
    this.receivePaymentService
      .createReceivePayment(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  private updateReceivePayment(body: ReceivePaymentRequestDTO): void {
    this.receivePaymentService
      .updateReceivePayment(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  deletedIds: string = "";
  onDeleteItem(id: string, itemIndex: number): void {
    if (id) this.deletedIds += `${id},`;
    this.receivePaymentDetails.removeAt(itemIndex);
    this.calculateAmount();
  }

  onSubmit(): void {
    if (this.receivePaymentForm.valid) {
      this.isLoading = true;
      const formValue = this.receivePaymentForm.value;
      if (!formValue.id) {
        this.addReceivePayment(formValue);
      } else {
        formValue.deletedReceivePaymentDetailIds = this.deletedIds;
        this.updateReceivePayment(formValue);
      }
    }
  }

  check(id: string) {
    if (this.isLoading) return;
    this.isLoading = true;

    this.receivePaymentService
      .checkReceivePayment(id)
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

    this.receivePaymentService
      .approveReceivePayment(id)
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

  send(id: string) {
    if (this.isLoading) return;
    this.isLoading = true;

    this.receivePaymentService
      .sendMoneyReceipt(id)
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

    this.receivePaymentService
      .unpostReceivePayment(id, status)
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

  confirmSendToCustomer(id: string, code: string = "") {
    if (this.isDialogOpen) return;
    this.isDialogOpen = true;

    const data: ConfirmDialogModel = {
      title: "Confirm Sent to Customer",
      message: `Confirm send sms to customer for item ${code}?`,
    };
    const dialogRef = this.confirmDialogService.confirmDialog(
      id,
      this.send.bind(this),
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

  openPopup(id: string) {
    this.http
      .get(
        environment.apiURL + "/ReceivePayment/single_file_view_download/" + id,
        {
          responseType: "blob",
        }
      )
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

  printPdf(id) {
    this.http
      .get(environment.apiURL + "/ReportReceivePayment/" + id, {
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
