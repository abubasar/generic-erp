import { HttpClient } from "@angular/common/http";
import { ChangeDetectorRef, Component, OnInit, ViewChild } from "@angular/core";
import { FormArray, FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MatButton } from "@angular/material/button";
import { MatDialog } from "@angular/material/dialog";
import { MatStepper } from "@angular/material/stepper";
import { ActivatedRoute, Router } from "@angular/router";
import { GoodsInTransitId } from "app/shared/consts/const";
import { LCCostEntryStatus } from "app/shared/enums/lcCostEntryStatus";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { SnackBarService } from "app/shared/services/snack-bar.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { inFinancialYearValidator } from "app/shared/validators/in-financial-year-validator";
import { ControlAccount } from "app/views/configuration/models/account/control-account.model";
import { CostCenter } from "app/views/configuration/models/cost-center/cost-center.model";
import { AccountService } from "app/views/configuration/services/account.service";
import { CostCenterService } from "app/views/configuration/services/cost-center.service";
import { LCCostEntryRequestDTO } from "app/views/purchase/models/lc-cost-entry/lc-cost-entry-request-dto.model";
import {
  LCCostEntryResponseDTO,
  LCCostEntryResponseDetail,
} from "app/views/purchase/models/lc-cost-entry/lc-cost-entry-response-dto.model";
import { LcCostEntryService } from "app/views/purchase/services/lc-cost-entry.service";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { finalize } from "rxjs";
import { LcCostPurchaseOrderDialogComponent } from "../lc-cost-purchase-order-dialog/lc-cost-purchase-order-dialog.component";

@Component({
  selector: "app-lc-cost-entry-form",
  templateUrl: "./lc-cost-entry-form.component.html",
  styleUrls: ["./lc-cost-entry-form.component.scss"],
})
export class LcCostEntryFormComponent implements OnInit {
  @ViewChild("btnCheck") btnCheck: MatButton;
  @ViewChild("btnApprove") btnApprove: MatButton;
  @ViewChild("stepper") private stepper: MatStepper;
  isLoading: boolean = false;
  isDialogOpen: boolean = false;
  isViewMode: boolean = true;
  creditAccountLoading: boolean = false;
  debitAccountLoading: boolean = false;
  formTitle: string;
  isChecked: boolean;
  deletedIds: string = "";
  lcCostEntryForm: FormGroup;
  costCenters: CostCenter[];
  debitAccounts: ControlAccount[];
  filteredDebitAccounts: ControlAccount[];
  creditAccounts: ControlAccount[];
  filteredCreditAccounts: ControlAccount[];
  lcCostEntryDetailsData: any[] = [];
  data: LCCostEntryResponseDTO;

  // Define financial year date range here
  financialYearStartDate: Date; //Example: Jul 1, 2023
  financialYearEndDate: Date; //Example: Jun 30, 2024
  currentFinancialYearId: string;
  applyMinMax: boolean = false;

  private path = {
    list: "purchase/lc-cost-entry",
    edit: "purchase/lc-cost-entry",
  };

  constructor(
    public dialog: MatDialog,
    private fb: FormBuilder,
    private jwtAuth: JwtAuthService,
    private costCenterService: CostCenterService,
    private accountService: AccountService,
    private lcCostEntryService: LcCostEntryService,
    private dateFormatService: DateTimeFormatService,
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
      this.data = response?.lcCostEntry?.data;
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
    this.getAllCostCenters();
    this.getDebitAccounts();
    this.getAllControlAccounts();
  }

  initializeForm() {
    this.createForm();
    this.populateForm();
    this.setFormTitle();
    this.lcCostEntryForm.get("entryDate").markAsTouched();
  }

  createForm(): void {
    this.lcCostEntryForm = this.fb.group({
      id: [this.data?.id || null],
      code: [this.data?.code ?? ""],
      purchaseOrderId: [this.data?.purchaseOrderId, Validators.required],
      ponumber: [this.data?.ponumber, Validators.required],
      lcNumber: [this.data?.lcNumber, Validators.required],
      entryDate: [
        this.data?.entryDate || this.dateFormatService.getPresentDate(),
      ],
      costCenterId: [this.data?.costCenterId, Validators.required],
      total: [this.data?.total, Validators.required],
      remark: [this.data?.remark ?? ""],
      deletedLCCostEntryDetailIds: [""],
      lccostEntryDetails: this.fb.array([]),
    });
    this.setMinMaxDates();
  }

  setMinMaxDates() {
    if (this.applyMinMax) {
      this.lcCostEntryForm
        .get("entryDate")
        .setValidators([
          Validators.required,
          inFinancialYearValidator(
            this.financialYearStartDate,
            this.financialYearEndDate
          ),
        ]);
    } else {
      this.lcCostEntryForm
        .get("entryDate")
        .setValidators([Validators.required]);
    }
    this.lcCostEntryForm.get("entryDate").updateValueAndValidity();
  }

  createLCCostEntryDetail(item?: LCCostEntryResponseDetail): FormGroup {
    return this.fb.group({
      id: [item?.id || null],
      debitAccountId: [item?.debitAccountId || null, Validators.required],
      creditAccountId: [item?.creditAccountId || null, Validators.required],
      debitAccount: [item?.debitAccount || null],
      creditAccount: [item?.creditAccount || null],
      amount: [item?.amount, Validators.required],
      isIncludedWithinLandedCost: [item?.isIncludedWithinLandedCost || false],
    });
  }

  get lccostEntryDetails(): FormArray {
    return this.lcCostEntryForm.get("lccostEntryDetails") as FormArray;
  }

  addItem(item?: LCCostEntryResponseDetail): void {
    this.lccostEntryDetails.push(this.createLCCostEntryDetail(item));
  }

  populateLCCostEntryDetails(data: LCCostEntryResponseDTO): void {
    data.lccostEntryDetails.forEach((item: LCCostEntryResponseDetail) =>
      this.addItem(item)
    );
  }

  populateForm(): void {
    if (this.lcCostEntryForm.get("id").value) {
      this.populateLCCostEntryDetails(this.data);
    } else {
      this.addItem();
    }
  }

  setFormTitle() {
    if (this.lcCostEntryForm.get("id").value) {
      this.formTitle = "Edit LC Cost Entry";
    } else {
      this.formTitle = "Add LC Cost Entry";
    }
  }

  isPassingDataToNextStep = false;
  handleStepChange(event: any) {
    this.isPassingDataToNextStep = event.selectedIndex === 1;
    if (this.isPassingDataToNextStep) {
      this.lcCostEntryDetailsData =
        this.lcCostEntryForm.get("lccostEntryDetails").value;
    }
  }

  getLCCostEntryStatus(value) {
    return this.statusColorService.getLCCostEntryStatus(value);
  }

  getLCCostEntryStatusName(value: number) {
    return LCCostEntryStatus[value];
  }

  getAllCostCenters() {
    this.costCenterService.getAllCostCenters().subscribe((res) => {
      this.costCenters = res?.data?.item1;
    });
  }

  getCostCenterName(costCenterId: string) {
    return this.costCenters?.find((x) => x.id === costCenterId)?.name;
  }

  clearDebitInputDetails(evt: any, itemIndex: number): void {
    evt.stopPropagation();
    const particularDetail = this.lccostEntryDetails.at(itemIndex);
    particularDetail?.patchValue({
      debitAccountId: null,
    });
  }

  clearCreditInputDetails(evt: any, itemIndex: number): void {
    evt.stopPropagation();
    const particularDetail = this.lccostEntryDetails.at(itemIndex);
    particularDetail?.patchValue({
      creditAccountId: null,
    });
  }

  getDebitAccounts() {
    this.debitAccountLoading = true;
    this.accountService
      .getControlAccountsByParentId(GoodsInTransitId)
      .subscribe((res) => {
        this.filteredDebitAccounts = this.debitAccounts = res?.data;
        this.debitAccountLoading = false;
      });
  }

  getAllControlAccounts() {
    this.creditAccountLoading = true;
    this.accountService.getAllControlAccounts().subscribe((res) => {
      this.filteredCreditAccounts = this.creditAccounts = res?.data;
      this.creditAccountLoading = false;
    });
  }

  handleDebitAccountSearch(event: any, itemIndex: number): void {
    const name = event.target?.name;
    if (name === "debitAccountId") {
      const term = this.lccostEntryDetails.at(itemIndex).get("debitAccountId");
      this.filterDebitAccount(term.value || "");
    }
  }

  private filterDebitAccount(value: string) {
    const filterValue = value.toLowerCase();
    this.filteredDebitAccounts = this.debitAccounts?.filter((option) =>
      option.name.toLowerCase().includes(filterValue)
    );
  }

  getDebitAccountName(debitAccountId: string) {
    if (!debitAccountId) {
      return;
    }
    const debitAccount =
      this.debitAccounts?.find(
        (debitAccount) => debitAccount?.id === debitAccountId
      ) ||
      this.data?.lccostEntryDetails?.find(
        (x) => x?.debitAccountId == debitAccountId
      )?.debitAccount;
    return debitAccount?.name;
  }

  handleCreditAccountSearch(event: any, itemIndex: number): void {
    const name = event.target?.name;
    if (name === "creditAccountId") {
      const term = this.lccostEntryDetails.at(itemIndex).get("creditAccountId");
      this.filterCreditAccount(term.value || "");
    }
  }

  private filterCreditAccount(value: string) {
    const filterValue = value.toLowerCase();
    this.filteredCreditAccounts = this.creditAccounts?.filter((option) =>
      option.name.toLowerCase().includes(filterValue)
    );
  }

  getCreditAccountName(creditAccountId: string) {
    if (!creditAccountId) {
      return;
    }
    const creditAccount =
      this.creditAccounts?.find(
        (creditAccount) => creditAccount?.id === creditAccountId
      ) ||
      this.data?.lccostEntryDetails?.find(
        (x) => x?.creditAccountId == creditAccountId
      )?.creditAccount;
    return creditAccount?.name;
  }

  findDebitAccountById(id) {
    return this.debitAccounts.find((debitAccount) => debitAccount?.id === id);
  }
  findCreditAccountById(id) {
    return this.creditAccounts.find(
      (creditAccount) => creditAccount?.id === id
    );
  }

  handleDebitAccountSelection(event: any, index: number) {
    const particularDetail = this.lccostEntryDetails.at(index);
    const debitAccountId = particularDetail?.value?.debitAccountId;
    const creditAccountId = particularDetail?.value?.creditAccountId;
    if (
      creditAccountId &&
      this.isExistSelectedAccount(debitAccountId, creditAccountId)
    ) {
      this.showSnackBar("This Debit Account already added in 'Credit Account'");
      particularDetail?.patchValue({
        debitAccountId: null,
      });
      return;
    }
    const selectedDebitAccount = this.findDebitAccountById(debitAccountId);
    if (!selectedDebitAccount) return;
    particularDetail.patchValue({
      debitAccountId: selectedDebitAccount.id,
      debitAccount: selectedDebitAccount,
      amount: this.lccostEntryDetails.at(index).get("amount").value,
    });
  }

  handleCreditAccountSelection(event: any, index: number) {
    const particularDetail = this.lccostEntryDetails.at(index);
    const debitAccountId = particularDetail?.value?.debitAccountId;
    const creditAccountId = particularDetail?.value?.creditAccountId;
    if (
      debitAccountId &&
      this.isExistSelectedAccount(debitAccountId, creditAccountId)
    ) {
      this.showSnackBar("This Credit Account already added in 'Debit Account'");
      particularDetail?.patchValue({
        creditAccountId: null,
      });
      return;
    }

    const selectedCreditAccount = this.findCreditAccountById(creditAccountId);
    if (!selectedCreditAccount) return;
    particularDetail.patchValue({
      creditAccountId: selectedCreditAccount.id,
      creditAccount: selectedCreditAccount,
      amount: this.lccostEntryDetails.at(index).get("amount").value,
    });
  }

  showSnackBar(message: string) {
    this.snackBarService.openSnackbar(message);
  }

  isExistSelectedAccount(
    debitAccountId: string,
    creditAccountId: string
  ): boolean {
    const isAccountSelected = debitAccountId === creditAccountId;
    return isAccountSelected;
  }

  calculateAmount() {
    const total = this.lccostEntryDetails?.value?.reduce(
      (sum, item) => sum + item?.amount,
      0
    );
    this.lcCostEntryForm?.get("total")?.setValue(total);
  }

  onControlChange(event: any, itemIndex?: number): void {
    const name = event?.target?.name;
    if (name === "amount") {
      this.calculateAmount();
    }
  }

  //openPurchaseOrderDialog
  openPurchaseOrderDialog() {
    const dialogRef = this.dialog.open(LcCostPurchaseOrderDialogComponent, {
      disableClose: true,
      panelClass: "add-bill-container",
      minHeight: "auto",
      height: "auto",
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.lcCostEntryForm.patchValue({
          purchaseOrderId: result?.id,
          ponumber: result?.ponumber,
          lcNumber: result?.lcNumber,
        });
      }
    });
  }

  private handleSuccessfulSave(res: GeneralResponse<string>): void {
    if (res?.succeeded) {
      this.handleLCCostEntryResponse(res?.data);
      this.toastr.success(res?.message);
      this.deletedIds = "";
    } else {
      this.isLoading = false;
      this.toastr.error(res?.message);
    }
  }

  private handleLCCostEntryResponse(lccostEntryId: string): void {
    this.lcCostEntryService.getLCCostEntryById(lccostEntryId).subscribe({
      next: (lcCostEntryResponse) => {
        this.data = lcCostEntryResponse.data;
        this.isViewMode = true;
        this.isLoading = false;
        this.navigateToView(lcCostEntryResponse?.data?.id);
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

  public navigateToList() {
    this.router.navigate([this.path.list]);
  }

  backToView() {
    this.initializeForm();
    this.stepper.next();
    this.isViewMode = true;
  }

  private addLCCostEntry(body: LCCostEntryRequestDTO): void {
    this.lcCostEntryService
      .createLCCostEntry(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  private updateLCCostEntry(body: LCCostEntryRequestDTO): void {
    this.lcCostEntryService
      .updateLCCostEntry(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  onDeleteItem(id: string, itemIndex: number): void {
    if (id) this.deletedIds += `${id},`;
    this.lccostEntryDetails.removeAt(itemIndex);
    this.calculateAmount();
  }

  onSubmit(): void {
    if (this.lcCostEntryForm.valid) {
      this.isLoading = true;
      const formValue = this.lcCostEntryForm.value;
      if (!formValue.id) {
        this.addLCCostEntry(formValue);
      } else {
        formValue.deletedLCCostEntryDetailIds = this.deletedIds;
        this.updateLCCostEntry(formValue);
      }
    }
  }

  check(id: string) {
    if (this.isLoading) return;
    this.isLoading = true;

    this.lcCostEntryService
      .checkLCCostEntry(id)
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

    this.lcCostEntryService
      .approveLCCostEntry(id)
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

    this.lcCostEntryService
      .unpostLCCostEntry(id, status)
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
      message: `This action will reverse the current status of LC Cost Entry Code ${code}. Confirm the status reversal?`,
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
      .get(environment.apiURL + "/ReportLcCostEntry/" + id, {
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
