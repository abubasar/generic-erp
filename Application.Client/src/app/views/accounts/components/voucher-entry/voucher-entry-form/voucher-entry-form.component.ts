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
import { VoucherEntryStatus } from "app/shared/enums/voucherEntryStatus";
import { VoucherType } from "app/shared/enums/voucherType";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { ENUM } from "app/shared/models/enum-value/enum.model";
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
  VoucherEntryRequestDTO,
  VoucherEntryRequestDetail,
} from "app/views/accounts/models/voucher-entry/voucher-entry-request-dto.model";
import {
  VoucherEntryResponseDTO,
  VoucherEntryResponseDetail,
} from "app/views/accounts/models/voucher-entry/voucher-entry-response-dto.model";
import { VoucherEntryService } from "app/views/accounts/services/voucher-entry.service";
import { ControlAccount } from "app/views/configuration/models/account/control-account.model";
import { CostCenter } from "app/views/configuration/models/cost-center/cost-center.model";
import { PaymentMode } from "app/views/configuration/models/payment-mode/payment-mode.model";
import { AccountService } from "app/views/configuration/services/account.service";
import { CostCenterService } from "app/views/configuration/services/cost-center.service";
import { PaymentModeService } from "app/views/configuration/services/payment-mode.service";
import { AddUpdateResponseDTO } from "app/views/purchase/models/add-update-response-dto.model";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { finalize } from "rxjs";

@Component({
  selector: "app-voucher-entry-form",
  templateUrl: "./voucher-entry-form.component.html",
  styleUrls: ["./voucher-entry-form.component.scss"],
})
export class VoucherEntryFormComponent implements OnInit {
  @ViewChild("btnCheck") btnCheck: MatButton;
  @ViewChild("btnApprove") btnApprove: MatButton;
  @ViewChild("stepper") private stepper: MatStepper;
  isLoading: boolean = false;
  isDialogOpen: boolean = false;
  isViewMode: boolean = true;
  isChecked: boolean;
  formTitle: string;
  voucherEntryForm: FormGroup;
  costCenters: CostCenter[];
  paymentModes: PaymentMode[];
  cashBankAccounts: ControlAccount[];
  controlAccounts: ControlAccount[];
  filterControlAccounts: ControlAccount[];
  filteredCashBankAccounts: ControlAccount[];
  voucherTypes: ENUM[];
  voucherEntryDetailsData: any[] = [];
  data: VoucherEntryResponseDTO;
  // Define financial year date range here
  financialYearStartDate: Date; // Jul 1, 2023
  financialYearEndDate: Date; // Jun 30, 2024
  currentFinancialYearId: string;
  applyMinMax: boolean = false;

  private path = {
    list: "accounts/voucher-entry",
    edit: "accounts/voucher-entry",
  };

  constructor(
    private fb: FormBuilder,
    private paymentModeService: PaymentModeService,
    private costCenterService: CostCenterService,
    private enumValueService: EnumValueService,
    private accountService: AccountService,
    private voucherEntryService: VoucherEntryService,
    private dateFormatService: DateTimeFormatService,
    private jwtAuth: JwtAuthService,
    private toastr: ToastrService,
    private http: HttpClient,
    private activatedRoute: ActivatedRoute,
    private router: Router,
    // private localStorageService: LocalStoreService,
    private cdRef: ChangeDetectorRef,
    public statusColorService: StatusColorService,
    private snackBarService: SnackBarService,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    console.log(
      "Activated route data in Component:::",
      this.activatedRoute.data
    );
    this.activatedRoute.data.subscribe((response: any) => {
      console.log("DATA FETCHING", response);
      this.data = response?.voucherEntry?.data;
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

  // ngOnDestroy() {
  //   this.localStorageService.removeItem(this.activatedRoute.snapshot.paramMap.get("id"));
  // }

  showEdit() {
    this.stepper.previous();
    this.isViewMode = false;
    this.applyMinMax = true;
    this.setMinMaxDates();
  }

  getData() {
    // const id = this.activatedRoute.snapshot.paramMap.get("id");
    // if (id) this.data = this.localStorageService.getItem(id);

    this.getAllCostCenters();
    this.getAllPaymentModes();
    this.getVoucherTypes();
    this.getAllControlAccounts();
  }

  initializeForm() {
    this.createForm();
    this.populateForm();
    this.setFormTitle();
    this.voucherEntryForm.get("voucherDate").markAsTouched();
  }

  createForm(): void {
    this.voucherEntryForm = this.fb.group({
      id: [this.data?.id || null],
      voucherNo: [this.data?.voucherNo || ""],
      voucherDate: [
        this.data?.voucherDate || this.dateFormatService.getPresentDate(),
      ],
      costCenterId: [this.data?.costCenterId, Validators.required],
      paymentModeId: [this.data?.paymentModeId, Validators.required],
      referenceNo: [this.data?.referenceNo || ""],
      voucherType: [this.data?.voucherType, Validators.required],
      cashBankAccountId: [this.data?.cashBankAccountId, Validators.required],
      totalAmount: [this.data?.totalAmount || 0],
      remark: [this.data?.remark || ""],
      deletedVoucherEntryDetailIds: [""],
      voucherEntryDetails: this.fb.array([]),
    });
    this.setMinMaxDates();
  }

  setMinMaxDates() {
    if (this.applyMinMax) {
      this.voucherEntryForm
        .get("voucherDate")
        .setValidators([
          Validators.required,
          inFinancialYearValidator(
            this.financialYearStartDate,
            this.financialYearEndDate
          ),
        ]);
    } else {
      this.voucherEntryForm
        .get("voucherDate")
        .setValidators([Validators.required]);
    }
    this.voucherEntryForm.get("voucherDate").updateValueAndValidity();
  }

  get voucherEntryDetails(): FormArray {
    return this.voucherEntryForm.get("voucherEntryDetails") as FormArray;
  }

  populateForm(): void {
    if (this.voucherEntryForm.get("id").value) {
      this.populateVoucherEntryDetails(this.data);
      this.getCashBankAccountsByPaymentModeId(this.data?.paymentModeId);
    } else {
      this.addItem();
      this.getAllCashBankAccounts(Control_Accounts_Parent_Id);
    }
  }

  setFormTitle(): void {
    if (this.voucherEntryForm.get("id").value) {
      this.formTitle = "Edit Purchase VoucherEntry";
    } else {
      this.formTitle = "Add Purchase VoucherEntry";
    }
  }

  populateVoucherEntryDetails(data: VoucherEntryResponseDTO): void {
    data.voucherEntryDetails.forEach((item: VoucherEntryResponseDetail) =>
      this.addItem(item)
    );
  }

  addItem(item?: VoucherEntryResponseDetail): void {
    this.voucherEntryDetails.push(this.createVoucherEntryDetail(item));
  }

  createVoucherEntryDetail(item?: VoucherEntryRequestDetail): FormGroup {
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
      this.voucherEntryDetailsData = this.voucherEntryForm.get(
        "voucherEntryDetails"
      ).value;
    }
  }

  getVoucherEntryStatus(value) {
    return this.statusColorService.getVoucherEntryStatus(value);
  }

  getVoucherEntryStatusName(value: number) {
    return VoucherEntryStatus[value];
  }

  getVoucherTypes() {
    // return this.enumValueService.getVoucherTypes().subscribe((res) => {
    //   this.voucherTypes = res;
    // });
    this.voucherTypes = Object.keys(VoucherType).map((key) => ({
      name: key,
      value: VoucherType[key] as number,
    }));
  }

  getVoucherTypeName(value: string) {
    if (value == "1") return "Cash_Payment_Voucher";
    else return "Cash_Received_Voucher";
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

  handleCashBankAccountSearch(event: any): void {
    const name = event.target?.name;
    if (name === "cashBankAccountId") {
      const term = this.voucherEntryForm.get("cashBankAccountId");
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
    this.voucherEntryForm.get("cashBankAccountId").reset();
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
  getAllControlAccounts() {
    this.accountService.getAllControlAccounts().subscribe((res) => {
      console.log(res);
      this.controlAccounts = res?.data;
    });
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    if (fieldName === "cashBankAccountId") {
      this.voucherEntryForm?.get("cashBankAccountId").setValue(null);
    }
  }

  clearInputFromDetails(evt: any, itemIndex: number): void {
    evt.stopPropagation();
    const particularDetail = this.voucherEntryDetails.at(itemIndex);
    particularDetail?.patchValue({
      accountId: null,
    });
  }

  handleControlAccountSearch(event: any, itemIndex: number): void {
    const name = event.target?.name;
    if (name === "accountId") {
      const term = this.voucherEntryDetails.at(itemIndex).get("accountId");
      this.filterControlAccount(term.value || "");
    }
  }

  private filterControlAccount(value: string) {
    const filterValue = value.toLowerCase();
    this.filterControlAccounts = this.controlAccounts?.filter((option) =>
      option.name.toLowerCase().includes(filterValue)
    );
    console.log("filterAccount", this.filterControlAccounts);
  }

  findControlAccountById(id) {
    return this.controlAccounts.find((account) => account?.id === id);
  }

  handleControlAccountSelection(event, index) {
    const particularDetail = this.voucherEntryDetails.at(index);
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
      amount: this.voucherEntryDetails.at(index).get("amount").value,
    });

    // console.log(particularDetail);
  }

  showSnackBar(message: string) {
    this.snackBarService.openSnackbar(message);
  }

  isExistSelectedAccount(accountId: string, index: number): boolean {
    const isProductAdded = this.voucherEntryDetails.value.some((item, i) => {
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
      this.data?.voucherEntryDetails?.find(
        (x) => x?.accountId == controlAccountId
      )?.account;
    return controlAccount?.name;
  }

  onControlChange(event: any, itemIndex?: number): void {
    const name = event?.target?.name;
    if (name === "amount") {
      this.calculateAmount();
    }
  }

  calculateAmount() {
    const totalAmount = this.voucherEntryDetails?.value?.reduce(
      (sum, item) => sum + item?.amount,
      0
    );
    this.voucherEntryForm?.get("totalAmount")?.setValue(totalAmount);
  }

  /** ------------------End For Autocomplete---------- */

  private handleSuccessfulSave(
    res: GeneralResponse<AddUpdateResponseDTO>,
    body: VoucherEntryRequestDTO
  ): void {
    if (res?.succeeded) {
      this.data = {
        ...this.data,
        status: res?.data?.status,
        voucherNo: res?.data?.code,
        id: res?.data?.id,
      };
      // this.localStorageService.setItem(res?.data?.id, {
      //   ...body,
      //   status: res.data?.status,
      //   voucherNo: res.data?.code,
      //   id: res?.data?.id,
      // });
      this.isViewMode = true;
      this.navigateToView(res?.data?.id);
      this.toastr.success(res?.message);
      this.isLoading = false;
    } else {
      this.isLoading = false;
      console.error(res);
      this.toastr.error(res?.message);
    }
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

  private addVoucherEntry(body: VoucherEntryRequestDTO): void {
    this.voucherEntryService
      .createVoucherEntry(body)
      .subscribe((res) => this.handleSuccessfulSave(res, body));
  }

  private updateVoucherEntry(body: VoucherEntryRequestDTO): void {
    this.voucherEntryService
      .updateVoucherEntry(body)
      .subscribe((res) => this.handleSuccessfulSave(res, body));
  }

  deletedIds: string = "";
  onDeleteItem(id: string, itemIndex: number): void {
    if (id) this.deletedIds += `${id},`;
    this.voucherEntryDetails.removeAt(itemIndex);
    this.calculateAmount();
  }

  onSubmit(): void {
    if (this.voucherEntryForm.valid) {
      this.isLoading = true;
      const formValue = this.voucherEntryForm.value;
      if (!formValue.id) {
        this.addVoucherEntry(formValue);
      } else {
        formValue.deletedVoucherEntryDetailIds = this.deletedIds;
        this.updateVoucherEntry(formValue);
      }
    }
  }

  check(id: string) {
    if (this.isLoading) return;
    this.isLoading = true;

    this.voucherEntryService
      .checkVoucherEntry(id)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (res) => {
          if (res?.succeeded) {
            // this.localStorageService.setItem(this.data?.id, this.data);
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

    this.voucherEntryService
      .approveVoucherEntry(id)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (res) => {
          if (res?.succeeded) {
            // this.localStorageService.setItem(this.data?.id, this.data);
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

    this.voucherEntryService
      .unpostVoucherEntry(id, status)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (res) => {
          if (res?.succeeded) {
            // this.localStorageService.setItem(this.data?.id, this.data);
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
      .get(environment.apiURL + "/ReportVoucherEntry/" + id, {
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
