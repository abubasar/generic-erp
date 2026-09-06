import { ChangeDetectorRef, Component, OnInit, ViewChild } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MatDialog } from "@angular/material/dialog";
import { MatStepper } from "@angular/material/stepper";
import { ActivatedRoute, Router } from "@angular/router";
import { ReverseId } from "app/shared/consts/const";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { FundTransferResponseDTO } from "app/views/accounts/models/fund-transfer/fund-transfer-response-dto.model";
import { FundTransferService } from "app/views/accounts/services/fund-transfer.service";
import { CostCenter } from "app/views/configuration/models/cost-center/cost-center.model";
import { AccountService } from "app/views/configuration/services/account.service";
import { CostCenterService } from "app/views/configuration/services/cost-center.service";
import { AddUpdateResponseDTO } from "app/views/purchase/models/add-update-response-dto.model";
import { ToastrService } from "ngx-toastr";

import { HttpClient } from "@angular/common/http";
import { MatButton } from "@angular/material/button";
import { Control_Accounts_Parent_Id } from "app/shared/consts/const";
import { FundTransferStatus } from "app/shared/enums/fundTransferStatus";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { SnackBarService } from "app/shared/services/snack-bar.service";
import { inFinancialYearValidator } from "app/shared/validators/in-financial-year-validator";
import { FundTransferRequestDTO } from "app/views/accounts/models/fund-transfer/fund-transfer-request-dto.model";
import { ControlAccount } from "app/views/configuration/models/account/control-account.model";
import { FundTransferTransactionType } from "app/views/configuration/models/fund-transfer-transaction-type/fund-transfer-transaction-type.model";
import { FundTransferTransactionTypeService } from "app/views/configuration/services/fund-transfer-transaction-type.service";
import { environment } from "environments/environment";
import { finalize } from "rxjs";

@Component({
  selector: "app-fund-transfer-form",
  templateUrl: "./fund-transfer-form.component.html",
  styleUrls: ["./fund-transfer-form.component.scss"],
})
export class FundTransferFormComponent implements OnInit {
  @ViewChild("btnCheck") btnCheck: MatButton;
  @ViewChild("btnApprove") btnApprove: MatButton;
  @ViewChild("stepper") private stepper: MatStepper;
  isLoading: boolean = false;
  isDialogOpen: boolean = false;
  isViewMode: boolean = true;
  // isChecked: boolean;
  formTitle: string;
  fundTransferForm: FormGroup;
  fundTransferTransactionTypes: FundTransferTransactionType[];
  costCenters: CostCenter[];
  cashBankAccounts: ControlAccount[];
  filteredCashBankAccounts: ControlAccount[];
  data: FundTransferResponseDTO;
  // Define financial year date range here
  financialYearStartDate: Date; // Jul 1, 2023
  financialYearEndDate: Date; // Jun 30, 2024
  currentFinancialYearId: string;
  applyMinMax: boolean = false;

  private path = {
    list: "accounts/fund-transfer",
    edit: "accounts/fund-transfer",
  };

  constructor(
    public dialog: MatDialog,
    private fb: FormBuilder,
    private fundTransferTransactionTypeService: FundTransferTransactionTypeService,
    private costCenterService: CostCenterService,
    private accountService: AccountService,
    private fundTransferService: FundTransferService,
    private dateFormatService: DateTimeFormatService,
    private jwtAuth: JwtAuthService,
    private toastr: ToastrService,
    private activatedRoute: ActivatedRoute,
    private router: Router,
    private http: HttpClient,
    private snackBarService: SnackBarService,
    private cdRef: ChangeDetectorRef,
    private statusColorService: StatusColorService,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.activatedRoute.data.subscribe((response: any) => {
      this.data = response?.fundTransfer?.data;
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
    this.setMinMaxDates();
  }

  getData() {
    this.getAllFundTransferTransactionTypes();
    this.getAllCostCenters();
    this.getAllToAccounts();
  }

  initializeForm() {
    this.createForm();
    this.setFormTitle();
    this.fundTransferForm.get("fundTransferDate").markAsTouched();
  }

  createForm(): void {
    this.fundTransferForm = this.fb.group({
      id: [this.data?.id || null],
      fundTransferNo: [this.data?.fundTransferNo ?? ""],
      fundTransferDate: [
        this.data?.fundTransferDate || this.dateFormatService.getPresentDate(),
      ],
      fundTransferTransactionTypeId: [
        this.data?.fundTransferTransactionTypeId,
        Validators.required,
      ],
      costCenterId: [this.data?.costCenterId, Validators.required],
      transferFromAccountId: [
        this.data?.transferFromAccountId,
        Validators.required,
      ],
      transferToAccountId: [
        this.data?.transferToAccountId,
        Validators.required,
      ],
      amount: [this.data?.amount, Validators.required],
      charges: [this.data?.charges ?? 0, Validators.required],
      remark: [this.data?.remark ?? ""],
    });
    this.setMinMaxDates();
  }

  setMinMaxDates() {
    if (this.applyMinMax) {
      this.fundTransferForm
        .get("fundTransferDate")
        .setValidators([
          Validators.required,
          inFinancialYearValidator(
            this.financialYearStartDate,
            this.financialYearEndDate
          ),
        ]);
    } else {
      this.fundTransferForm
        .get("fundTransferDate")
        .setValidators([Validators.required]);
    }

    this.fundTransferForm.get("fundTransferDate").updateValueAndValidity();
  }

  setFormTitle() {
    if (this.fundTransferForm.get("id").value) {
      this.formTitle = "Edit Fund Transfer";
    } else {
      this.formTitle = "Add Fund Transfer";
    }
  }

  isPassingDataToNextStep = false;
  handleStepChange(event: any) {
    this.isPassingDataToNextStep = event.selectedIndex === 1;
  }

  getAllFundTransferTransactionTypes() {
    this.fundTransferTransactionTypeService
      .getAllFundTransferTransactionTypes()
      .subscribe((res) => {
        this.fundTransferTransactionTypes = res?.data?.item1;
      });
  }

  getAllCostCenters() {
    this.costCenterService.getAllCostCenters().subscribe((res) => {
      this.costCenters = res?.data?.item1;
    });
  }

  cashBankLoading: boolean = false;
  getAllToAccounts() {
    this.cashBankLoading = true;
    this.accountService
      .getControlAccountsByParentId(Control_Accounts_Parent_Id)
      .subscribe((res) => {
        this.filteredCashBankAccounts = this.cashBankAccounts = res?.data;
        this.cashBankLoading = false;
      });
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    if (fieldName === "transferFromAccountId")
      this.fundTransferForm?.get("transferFromAccountId").setValue(null);
    if (fieldName === "transferToAccountId")
      this.fundTransferForm?.get("transferToAccountId").setValue(null);
  }

  handleCashBankAccountSearch(event: any): void {
    const name = event.target?.name;
    const termControl = this.fundTransferForm.get(name);

    if (termControl) {
      this.filterCashBankAccount(termControl.value || "");
    }
  }

  private filterCashBankAccount(value: string) {
    const filterValue = value.toLowerCase();
    this.filteredCashBankAccounts = this.cashBankAccounts?.filter((option) =>
      option.name.toLowerCase().includes(filterValue)
    );
  }

  handleCashBankFromAccountSelection(event: any) {
    const fromAccountId = this.fundTransferForm.get(
      "transferFromAccountId"
    )?.value;
    const toAccountId = this.fundTransferForm.get("transferToAccountId")?.value;
    const fundTransferTransactionTypeId = this.fundTransferForm.get(
      "fundTransferTransactionTypeId"
    )?.value;

    if (fundTransferTransactionTypeId != ReverseId) {
      if (
        toAccountId &&
        this.isFromAccountExistSelectedAccount(fromAccountId)
      ) {
        this.showSnackBar(
          "This Account already added in 'Transfer To Account'"
        );
        this.fundTransferForm.patchValue({
          transferFromAccountId: null,
        });
        return;
      }
    } else {
      console.log("first");
      if (toAccountId && fromAccountId != toAccountId) {
        this.showSnackBar(
          "'Transfer From Account' and 'Transfer To Account' should be the same"
        );
        this.fundTransferForm.patchValue({
          transferFromAccountId: null,
        });
      }
    }
  }

  handleCashBankToAccountSelection(event: any) {
    const fromAccountId = this.fundTransferForm.get(
      "transferFromAccountId"
    )?.value;
    const toAccountId = this.fundTransferForm.get("transferToAccountId")?.value;
    const fundTransferTransactionTypeId = this.fundTransferForm.get(
      "fundTransferTransactionTypeId"
    )?.value;
    if (fundTransferTransactionTypeId != ReverseId) {
      if (fromAccountId && this.isToAccountExistSelectedAccount(toAccountId)) {
        this.showSnackBar(
          "This Account already added in 'Transfer From Account'"
        );
        this.fundTransferForm.patchValue({
          transferToAccountId: null,
        });
        return;
      }
    } else {
      console.log("Second");
      if (fromAccountId && fromAccountId != toAccountId) {
        this.showSnackBar(
          "'Transfer From Account' and 'Transfer To Account' should be the same"
        );
        this.fundTransferForm.patchValue({
          transferToAccountId: null,
        });
      }
    }
  }

  showSnackBar(message: string) {
    this.snackBarService.openSnackbar(message);
  }

  isFromAccountExistSelectedAccount(fromAccountId: string): boolean {
    const isAccountSelected =
      this.fundTransferForm.get("transferToAccountId")?.value === fromAccountId;
    return isAccountSelected;
  }

  isToAccountExistSelectedAccount(toAccountId: string): boolean {
    const isAccountSelected =
      this.fundTransferForm.get("transferFromAccountId")?.value === toAccountId;
    return isAccountSelected;
  }

  getCashBankFromAccountName(cashBankAccountId: string) {
    if (!cashBankAccountId) {
      return;
    }
    const cashBankAccount =
      this.cashBankAccounts?.find(
        (cashBankAccount) => cashBankAccount?.id === cashBankAccountId
      ) || this.data?.transferFromAccount;
    return cashBankAccount?.name;
  }

  getCashBankToAccountName(cashBankAccountId: string) {
    if (!cashBankAccountId) {
      return;
    }
    const cashBankAccount =
      this.cashBankAccounts?.find(
        (cashBankAccount) => cashBankAccount?.id === cashBankAccountId
      ) || this.data?.transferToAccount;
    return cashBankAccount?.name;
  }

  getFundTransferTransactionTypeName(fundTransferTransactionTypeId: string) {
    return this.fundTransferTransactionTypes?.find(
      (x) => x.id === fundTransferTransactionTypeId
    )?.name;
  }

  getCostCenterName(costCenterId: string) {
    return this.costCenters?.find((x) => x.id === costCenterId)?.name;
  }

  private handleSuccessfulSave(
    res: GeneralResponse<AddUpdateResponseDTO>,
    body: FundTransferRequestDTO
  ): void {
    if (res?.succeeded) {
      this.data = {
        ...this.data,
        status: res?.data?.status,
        fundTransferNo: res?.data?.code,
        id: res?.data?.id,
      };
      // this.localStorageService.setItem(res?.data, {
      //   ...body,
      //   // it will be active when response wil be ok
      //   // requisitionStatus: res.data?.status,
      //   // requisitionNo: res.data?.code,
      //   // id: res?.data?.id,
      //   id: res?.data,
      // });
      this.isViewMode = true;
      this.navigateToView(res?.data?.id);
      this.toastr.success(res?.message);
      this.isLoading = false;
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

  public navigateToList() {
    this.router.navigate([this.path.list]);
  }

  backToView() {
    this.initializeForm();
    this.stepper.next();
    this.isViewMode = true;
  }

  private addFundTransfer(body: FundTransferRequestDTO): void {
    this.fundTransferService
      .createFundTransfer(body)
      .subscribe((res) => this.handleSuccessfulSave(res, body));
  }

  private updateFundTransfer(body: FundTransferRequestDTO): void {
    this.fundTransferService
      .updateFundTransfer(body)
      .subscribe((res) => this.handleSuccessfulSave(res, body));
  }

  onSubmit(): void {
    if (this.fundTransferForm.valid) {
      this.isLoading = true;
      const formValue = this.fundTransferForm.value;
      if (!formValue.id) {
        this.addFundTransfer(formValue);
      } else {
        this.updateFundTransfer(formValue);
      }
    }
  }

  check(id: string) {
    if (this.isLoading) return;
    this.isLoading = true;

    this.fundTransferService
      .checkFundTransfer(id)
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

    this.fundTransferService
      .approveFundTransfer(id)
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

    this.fundTransferService
      .unpostFundTransfer(id, status)
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

  getFundTransferStatus(value) {
    return this.statusColorService.getFundTransferStatus(value);
  }

  getFundTransferStatusName(value: number) {
    return FundTransferStatus[value];
  }

  onTransactionTypeChange(transactionTypeId: string): void {
    const fromAccountId = this.fundTransferForm.get(
      "transferFromAccountId"
    )?.value;
    const toAccountId = this.fundTransferForm.get("transferToAccountId")?.value;
    if (transactionTypeId == ReverseId && fromAccountId && toAccountId) {
      if (fromAccountId != toAccountId) {
        this.showSnackBar(
          "'Transfer From Account' and 'Transfer To Account' should be the same"
        );
        this.fundTransferForm.patchValue({
          transferToAccountId: null,
        });
        return;
      }
    }
    if (transactionTypeId != ReverseId && fromAccountId && toAccountId) {
      if (fromAccountId == toAccountId) {
        this.showSnackBar(
          "'Transfer From Account' and 'Transfer To Account' should be different"
        );
        this.fundTransferForm.patchValue({
          transferToAccountId: null,
        });
        return;
      }
    }
  }

  printPdf(id) {
    this.http
      .get(environment.apiURL + "/ReportFundTransfer/" + id, {
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
