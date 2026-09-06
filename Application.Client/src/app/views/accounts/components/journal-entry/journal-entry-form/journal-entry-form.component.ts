import { HttpClient } from "@angular/common/http";
import { ChangeDetectorRef, Component, OnInit, ViewChild } from "@angular/core";
import { FormArray, FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MatButton } from "@angular/material/button";
import { MatStepper } from "@angular/material/stepper";
import { ActivatedRoute, Router } from "@angular/router";
import { JournalEntryStatus } from "app/shared/enums/journalEntryStatus";
import { PostType } from "app/shared/enums/postType";
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
import { JournalEntryRequestDTO } from "app/views/accounts/models/journal-entry/journal-entry-request-dto.model";
import {
  JournalEntryResponseDTO,
  JournalEntryResponseDetail,
} from "app/views/accounts/models/journal-entry/journal-entry-response-dto.model";
import { JournalEntryService } from "app/views/accounts/services/journal-entry.service";
import { ControlAccount } from "app/views/configuration/models/account/control-account.model";
import { CostCenter } from "app/views/configuration/models/cost-center/cost-center.model";
import { AccountService } from "app/views/configuration/services/account.service";
import { CostCenterService } from "app/views/configuration/services/cost-center.service";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { finalize } from "rxjs";

@Component({
  selector: "app-journal-entry-form",
  templateUrl: "./journal-entry-form.component.html",
  styleUrls: ["./journal-entry-form.component.scss"],
})
export class JournalEntryFormComponent implements OnInit {
  @ViewChild("btnCheck") btnCheck: MatButton;
  @ViewChild("btnApprove") btnApprove: MatButton;
  @ViewChild("stepper") private stepper: MatStepper;
  isLoading: boolean = false;
  isDialogOpen: boolean = false;
  isViewMode: boolean = true;
  isChecked: boolean;
  formTitle: string;
  journalEntryForm: FormGroup;
  costCenters: CostCenter[];
  controlAccounts: ControlAccount[];
  filterControlAccounts: ControlAccount[];
  postTypes: ENUM[];
  journalEntryDetailsData: any[] = [];
  data: JournalEntryResponseDTO;
  currentFinancialYearId: string;

  private path = {
    list: "accounts/journal-entry",
    edit: "accounts/journal-entry",
  };

  constructor(
    private fb: FormBuilder,
    private costCenterService: CostCenterService,
    private enumValueService: EnumValueService,
    private accountService: AccountService,
    private journalEntryService: JournalEntryService,
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
      this.data = response?.journalEntry?.data;
    });
    this.getData();
    this.getCurrentFinancialYearId();
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
    }
    this.cdRef.detectChanges();
  }

  showEdit() {
    this.stepper.previous();
    this.isViewMode = false;
    this.initializeForm();
  }

  getData() {
    this.getAllCostCenters();
    this.getPostTypes();
    this.getAllControlAccounts();
  }

  initializeForm() {
    this.createForm();
    this.populateForm();
    this.setFormTitle();
  }

  createForm(): void {
    this.journalEntryForm = this.fb.group({
      id: [this.data?.id || null],
      voucherNo: [this.data?.voucherNo || ""],
      voucherDate: [
        this.data?.voucherDate || this.dateFormatService.getPresentDate(),
        Validators.required,
      ],
      costCenterId: [this.data?.costCenterId, Validators.required],
      remark: [this.data?.remark || ""],
      deletedJournalEntryDetailIds: [""],
      journalEntryDetails: this.fb.array([]),
    });
  }

  get journalEntryDetails(): FormArray {
    return this.journalEntryForm.get("journalEntryDetails") as FormArray;
  }

  populateForm(): void {
    if (this.journalEntryForm.get("id").value) {
      this.populateJournalEntryDetails(this.data);
    } else {
      this.addItem();
    }
  }

  setFormTitle(): void {
    if (this.journalEntryForm.get("id").value) {
      this.formTitle = "Edit Journal Entry";
    } else {
      this.formTitle = "Add Journal Entry";
    }
  }

  populateJournalEntryDetails(data: JournalEntryResponseDTO): void {
    data.journalEntryDetails.forEach((item: JournalEntryResponseDetail) =>
      this.addItem(item)
    );
  }

  addItem(item?: JournalEntryResponseDetail): void {
    this.journalEntryDetails.push(this.createJournalEntryDetail(item));
  }

  createJournalEntryDetail(item?: JournalEntryResponseDetail): FormGroup {
    return this.fb.group({
      id: [item?.id || null],
      accountId: [item?.accountId || null, Validators.required],
      postType: [item?.postType, Validators.required],
      account: [item?.account || null],
      accountDescription: [item?.accountDescription || ""],
      amount: [item?.amount, Validators.required],
      isOpeningBalance: [item?.isOpeningBalance || false],
    });
  }

  isPassingDataToNextStep = false;
  handleStepChange(event: any) {
    if (this.isDebitCreditEqual()) {
      this.isPassingDataToNextStep = event.selectedIndex === 1;
      if (this.isPassingDataToNextStep) {
        this.journalEntryDetailsData = this.journalEntryForm.get(
          "journalEntryDetails"
        ).value;
      }
    }
  }

  goToNext() {
    if (this.isDebitCreditEqual()) {
      this.stepper.next();
    }
  }

  /** ------Check debit and credit equal or not------- */
  isDebitCreditEqual() {
    let debitTotal = 0;
    let creditTotal = 0;
    let data = [];
    data = this.journalEntryForm.get("journalEntryDetails").value;
    const allAreTrue = data.every((obj) => obj.isOpeningBalance === true);
    const allAreFalse = data.every((obj) => obj.isOpeningBalance === false);
    data.forEach((item) => {
      if (item.postType === 1) {
        debitTotal += item.amount;
      } else if (item.postType === 2) {
        creditTotal += item.amount;
      }
    });

    // Round to 2 decimal places to avoid floating-point precision errors
    const roundedDebitTotal = parseFloat(debitTotal.toFixed(2));
    const roundedCreditTotal = parseFloat(creditTotal.toFixed(2));

    if (
      roundedDebitTotal === roundedCreditTotal &&
      (allAreTrue || allAreFalse)
    ) {
      return true;
    } else {
      const message =
        "Please ensure debit and credit are equal and in case of opening balance entry,ensure all checkbox are checked.\n" +
        `Debit Total: ${roundedDebitTotal}\n` +
        `Credit Total: ${roundedCreditTotal}`;

      alert(message);
      return false;
    }
  }
  /** -------------------End----------------- */

  private getCurrentFinancialYearId() {
    this.jwtAuth.userProfile.subscribe((res: UserProfile) => {
      this.currentFinancialYearId = res.fyid;
    });
  }

  getJournalEntryStatus(value) {
    return this.statusColorService.getJournalEntryStatus(value);
  }

  getJournalEntryStatusName(value: number) {
    return JournalEntryStatus[value];
  }

  getPostTypes() {
    return this.enumValueService.getPostTypes().subscribe((res) => {
      this.postTypes = res;
    });
  }

  getPostTypeName(value: number) {
    return PostType[value];
  }

  getAllCostCenters(): void {
    this.costCenterService.getAllCostCenters().subscribe((res) => {
      this.costCenters = res?.data?.item1;
    });
  }

  clearInputDetails(evt: any, itemIndex: number): void {
    evt.stopPropagation();
    const particularDetail = this.journalEntryDetails.at(itemIndex);
    particularDetail?.patchValue({
      accountId: null,
    });
  }

  handleControlAccountSearch(event: any, itemIndex: number): void {
    const name = event.target?.name;
    if (name === "accountId") {
      const term = this.journalEntryDetails.at(itemIndex).get("accountId");
      this.filterControlAccount(term.value || "");
    }
  }

  private filterControlAccount(value: string) {
    const filterValue = value.toLowerCase();
    this.filterControlAccounts = this.controlAccounts?.filter(
      (option) =>
        option?.name?.toLowerCase().includes(filterValue) ||
        option.code?.split(".")?.pop()?.toLowerCase().includes(filterValue)
    );
  }

  findControlAccountById(id) {
    return this.controlAccounts.find((account) => account?.id === id);
  }

  handleControlAccountSelection(event, index) {
    const particularDetail = this.journalEntryDetails.at(index);
    const accountId = event?.option?.value;

    if (this.isExistSelectedAccount(accountId, index)) {
      this.showSnackBar("This Account is already added!");
      particularDetail?.patchValue({
        accountId: null,
      });
      return;
    }

    const selectedControlAccount = this.findControlAccountById(accountId);

    if (!selectedControlAccount) return;
    particularDetail.patchValue({
      accountId: selectedControlAccount.id,
      account: selectedControlAccount,
      accountDescription: selectedControlAccount.treeName,
      amount: this.journalEntryDetails.at(index).get("amount").value,
    });
  }

  showSnackBar(message: string) {
    this.snackBarService.openSnackbar(message);
  }

  isExistSelectedAccount(accountId: string, index: number): boolean {
    const isProductAdded = this.journalEntryDetails.value.some((item, i) => {
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
      this.data?.journalEntryDetails?.find(
        (x) => x?.accountId == controlAccountId
      )?.account;
    return (
      "(" + controlAccount.code?.split(".")?.pop() + ") " + controlAccount?.name
    );
  }

  getAllControlAccounts() {
    this.accountService.getAllControlAccounts().subscribe((res) => {
      this.filterControlAccounts = this.controlAccounts = res?.data;
    });
  }

  getCostCenterName(costCenterId: string) {
    return this.costCenters?.find((x) => x.id === costCenterId).name;
  }

  private handleSuccessfulSave(res: GeneralResponse<string>): void {
    if (res?.succeeded) {
      this.handleJournalEntryResponse(res?.data);
      this.toastr.success(res?.message);
      this.deletedIds = "";
    } else {
      this.isLoading = false;
      this.toastr.error(res?.message);
    }
  }

  private handleJournalEntryResponse(journalEntryId: string): void {
    this.journalEntryService.getJournalEntryById(journalEntryId).subscribe({
      next: (journalEntryResponse) => {
        this.data = journalEntryResponse.data;
        this.isViewMode = true;
        this.isLoading = false;
        this.navigateToView(journalEntryResponse?.data?.id);
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

  navigateToList() {
    this.router.navigate([this.path.list]);
  }

  backToView() {
    this.initializeForm();
    this.stepper.next();
    this.isViewMode = true;
  }

  private addJournalEntry(body: JournalEntryRequestDTO): void {
    this.journalEntryService
      .createJournalEntry(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  private updateJournalEntry(body: JournalEntryRequestDTO): void {
    this.journalEntryService
      .updateJournalEntry(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  deletedIds: string = "";
  onDeleteItem(id: string, itemIndex: number): void {
    if (id) this.deletedIds += `${id},`;
    this.journalEntryDetails.removeAt(itemIndex);
  }

  onSubmit(): void {
    if (this.journalEntryForm.valid) {
      this.isLoading = true;
      const formValue = this.journalEntryForm.value;
      if (!formValue.id) {
        this.addJournalEntry(formValue);
      } else {
        formValue.deletedJournalEntryDetailIds = this.deletedIds;
        this.updateJournalEntry(formValue);
      }
    }
  }

  check(id: string) {
    if (this.isLoading) return;
    this.isLoading = true;

    this.journalEntryService
      .checkJournalEntry(id)
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

    this.journalEntryService
      .approveJournalEntry(id)
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
    this.journalEntryService
      .unpostJournalEntry(id, status)
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
      .get(environment.apiURL + "/ReportJournalEntry/" + id, {
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
