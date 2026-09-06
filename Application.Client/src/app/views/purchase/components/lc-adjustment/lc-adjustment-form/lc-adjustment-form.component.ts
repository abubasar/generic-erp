import { HttpClient } from "@angular/common/http";
import { ChangeDetectorRef, Component, OnInit, ViewChild } from "@angular/core";
import { FormArray, FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MatButton } from "@angular/material/button";
import { MatDialog } from "@angular/material/dialog";
import { MatStepper } from "@angular/material/stepper";
import { ActivatedRoute, Router } from "@angular/router";
import { LcMarginId } from "app/shared/consts/const";
import { LcAdjustmentStatus } from "app/shared/enums/lcAdjustmentStatus";
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
import { inFinancialYearValidator } from "app/shared/validators/in-financial-year-validator";
import { ControlAccount } from "app/views/configuration/models/account/control-account.model";
import { CostCenter } from "app/views/configuration/models/cost-center/cost-center.model";
import { AccountService } from "app/views/configuration/services/account.service";
import { CostCenterService } from "app/views/configuration/services/cost-center.service";
import { LcAdjustmentRequestDTO } from "app/views/purchase/models/lc-adjustment/lc-adjustment-request-dto.model";
import {
  LcAdjustmentResponseDTO,
  LcAdjustmentResponseDetail,
} from "app/views/purchase/models/lc-adjustment/lc-adjustment-response-dto.model";
import { LCCostEntrySearchRequestDTO } from "app/views/purchase/models/lc-cost-entry/lc-cost-entry-search-request-dto.model";
import { LcAdjustmentService } from "app/views/purchase/services/lc-adjustment.service";
import { LcCostEntryService } from "app/views/purchase/services/lc-cost-entry.service";
import { PurchaseOrderService } from "app/views/purchase/services/purchase-order.service";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { finalize } from "rxjs";
import { LcAdjustmentPurchaseInvoiceDialogComponent } from "../lc-adjustment-purchase-invoice-dialog/lc-adjustment-purchase-invoice-dialog.component";

@Component({
  selector: "app-lc-adjustment-form",
  templateUrl: "./lc-adjustment-form.component.html",
  styleUrls: ["./lc-adjustment-form.component.scss"],
})
export class LcAdjustmentFormComponent implements OnInit {
  @ViewChild("btnCheck") btnCheck: MatButton;
  @ViewChild("btnApprove") btnApprove: MatButton;
  @ViewChild("stepper") private stepper: MatStepper;
  isLoading: boolean = false;
  isDialogOpen: boolean = false;
  isViewMode: boolean = true;
  isChecked: boolean;
  formTitle: string;
  lcAdjustmentForm: FormGroup;
  totalCount: number;
  costCenters: CostCenter[];
  controlAccounts: ControlAccount[];
  filterControlAccounts: ControlAccount[];
  postTypes: ENUM[];
  lcAdjustmentDetailsData: any[] = [];
  data: LcAdjustmentResponseDTO;

  // Define financial year date range here
  financialYearStartDate: Date; //Example: Jul 1, 2023
  financialYearEndDate: Date; //Example: Jun 30, 2024
  currentFinancialYearId: string;
  applyMinMax: boolean = false;

  private path = {
    list: "purchase/lc-adjustment",
    edit: "purchase/lc-adjustment",
  };

  constructor(
    public dialog: MatDialog,
    private fb: FormBuilder,
    private jwtAuth: JwtAuthService,
    private costCenterService: CostCenterService,
    private enumValueService: EnumValueService,
    private accountService: AccountService,
    private lcAdjustmentService: LcAdjustmentService,
    private lcCostEntryService: LcCostEntryService,
    private purchaseOrderService: PurchaseOrderService,
    private dateFormatService: DateTimeFormatService,
    private toastr: ToastrService,
    private activatedRoute: ActivatedRoute,
    private router: Router,
    private http: HttpClient,
    private cdRef: ChangeDetectorRef,
    public statusColorService: StatusColorService,
    private snackBarService: SnackBarService,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.activatedRoute.data.subscribe((response: any) => {
      this.data = response?.lcAdjustment?.data;
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
    this.getPostTypes();
    this.getAllControlAccounts();
  }

  initializeForm() {
    this.createForm();
    this.populateForm();
    this.setFormTitle();
    this.lcAdjustmentForm.get("adjustmentDate").markAsTouched();
  }

  createForm(): void {
    this.lcAdjustmentForm = this.fb.group({
      id: [this.data?.id || null],
      code: [this.data?.code || ""],
      purchaseInvoiceId: [this.data?.purchaseInvoiceId, Validators.required],
      purchaseInvoiceNo: [this.data?.purchaseInvoiceNo, Validators.required],
      adjustmentDate: [
        this.data?.adjustmentDate || this.dateFormatService.getPresentDate(),
      ],
      costCenterId: [this.data?.costCenterId || null, Validators.required],
      invoiceTotal: [this.data?.invoiceTotal, Validators.required],
      lcMarginTotal: [this.data?.lcMarginTotal, Validators.required],
      remark: [this.data?.remark || ""],
      deletedLcAdjustmentDetailIds: [""],
      lcAdjustmentDetails: this.fb.array([]),
    });
    this.setMinMaxDates();
  }

  setMinMaxDates() {
    if (this.applyMinMax) {
      this.lcAdjustmentForm
        .get("adjustmentDate")
        .setValidators([
          Validators.required,
          inFinancialYearValidator(
            this.financialYearStartDate,
            this.financialYearEndDate
          ),
        ]);
    } else {
      this.lcAdjustmentForm
        .get("adjustmentDate")
        .setValidators([Validators.required]);
    }
    this.lcAdjustmentForm.get("adjustmentDate").updateValueAndValidity();
  }

  get lcAdjustmentDetails(): FormArray {
    return this.lcAdjustmentForm.get("lcAdjustmentDetails") as FormArray;
  }

  populateForm(): void {
    if (this.lcAdjustmentForm.get("id").value) {
      this.populateLcAdjustmentDetails(this.data);
    } else {
      this.addItem();
    }
  }

  setFormTitle(): void {
    if (this.lcAdjustmentForm.get("id").value) {
      this.formTitle = "Edit LC Adjustment";
    } else {
      this.formTitle = "Add LC Adjustment";
    }
  }

  populateLcAdjustmentDetails(data: LcAdjustmentResponseDTO): void {
    data.lcAdjustmentDetails.forEach((item: LcAdjustmentResponseDetail) =>
      this.addItem(item)
    );
  }

  addItem(item?: LcAdjustmentResponseDetail): void {
    this.lcAdjustmentDetails.push(this.createLcAdjustmentDetail(item));
  }

  createLcAdjustmentDetail(item?: LcAdjustmentResponseDetail): FormGroup {
    return this.fb.group({
      id: [item?.id || null],
      accountId: [item?.accountId || null, Validators.required],
      postType: [item?.postType, Validators.required],
      account: [item?.account || null],
      accountDescription: [item?.accountDescription || ""],
      amount: [item?.amount, Validators.required],
    });
  }

  isPassingDataToNextStep = false;
  handleStepChange(event: any) {
    if (this.isDebitCreditEqual()) {
      this.isPassingDataToNextStep = event.selectedIndex === 1;
      if (this.isPassingDataToNextStep) {
        this.lcAdjustmentDetailsData = this.lcAdjustmentForm.get(
          "lcAdjustmentDetails"
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
    data = this.lcAdjustmentForm.get("lcAdjustmentDetails").value;
    data.forEach((item) => {
      if (item.postType === 1) {
        debitTotal += item.amount;
      } else if (item.postType === 2) {
        creditTotal += item.amount;
      }
    });

    if (debitTotal === creditTotal) {
      return true;
    } else {
      const message =
        "Please ensure debit and credit are equal.\n" +
        `Debit Total: ${debitTotal}\n` +
        `Credit Total: ${creditTotal}`;

      alert(message);
      return false;
    }
  }
  /** -------------------End----------------- */

  getLcAdjustmentStatus(value) {
    return this.statusColorService.getLcAdjustmentStatus(value);
  }

  getLcAdjustmentStatusName(value: number) {
    return LcAdjustmentStatus[value];
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
    const particularDetail = this.lcAdjustmentDetails.at(itemIndex);
    particularDetail?.patchValue({
      accountId: null,
    });
  }

  private filterControlAccount(value: string) {
    const filterValue = value.toLowerCase();
    this.filterControlAccounts = this.controlAccounts?.filter((option) =>
      option?.name?.toLowerCase().includes(filterValue)
    );
  }

  handleControlAccountSearch(event: any, itemIndex: number): void {
    const name = event.target?.name;
    if (name === "accountId") {
      const term = this.lcAdjustmentDetails.at(itemIndex).get("accountId");
      this.filterControlAccount(term.value || "");
    }
  }

  findControlAccountById(id) {
    return this.controlAccounts.find((account) => account?.id === id);
  }

  isExistSelectedAccount(accountId: string, index: number): boolean {
    const isProductAdded = this.lcAdjustmentDetails.value.some((item, i) => {
      return item.accountId === accountId && i !== index;
    });
    return isProductAdded;
  }

  handleControlAccountSelection(event, index) {
    const particularDetail = this.lcAdjustmentDetails.at(index);
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
      amount: this.lcAdjustmentDetails.at(index).get("amount").value,
    });
  }

  showSnackBar(message: string) {
    this.snackBarService.openSnackbar(message);
  }

  getControlAccountName(controlAccountId: string) {
    if (!controlAccountId) {
      return;
    }
    const controlAccount =
      this.controlAccounts?.find(
        (account) => account?.id === controlAccountId
      ) ||
      this.data?.lcAdjustmentDetails?.find(
        (x) => x?.accountId == controlAccountId
      )?.account;
    return controlAccount?.name;
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
      this.handleLcAdjustmentResponse(res?.data);
      this.toastr.success(res?.message);
      this.deletedIds = "";
    } else {
      this.isLoading = false;
      this.toastr.error(res?.message);
    }
  }

  private handleLcAdjustmentResponse(lcAdjustmentId: string): void {
    this.lcAdjustmentService.getLcAdjustmentById(lcAdjustmentId).subscribe({
      next: (lcAdjustmentResponse) => {
        this.data = lcAdjustmentResponse.data;
        this.isViewMode = true;
        this.isLoading = false;
        this.navigateToView(lcAdjustmentResponse?.data?.id);
      },
      error: (err) => {
        location.reload();
      },
    });
  }

  purchaseOrderSubtotal: number = 0;
  sumLcMarginTotal: number = 0;
  //openPurchaseInvoiceDialog
  openPurchaseInvoiceDialog() {
    const dialogRef = this.dialog.open(
      LcAdjustmentPurchaseInvoiceDialogComponent,
      {
        disableClose: true,
        panelClass: "add-bill-container",
        minHeight: "auto",
        height: "auto",
      }
    );
    dialogRef.beforeClosed().subscribe((result) => {
      if (result) {
        this.sumLcMarginTotal = 0;
        const lcCostEntryRequest = new LCCostEntrySearchRequestDTO();
        lcCostEntryRequest.ponumber = result.ponumber;
        lcCostEntryRequest.lcCostEntryStatus = 4;
        this.lcCostEntryService.getLCCostEntries(lcCostEntryRequest).subscribe({
          next: (res) => {
            if (res?.data?.item2 > 0) {
              res?.data?.item1.forEach((item: any) => {
                item.lccostEntryDetails.forEach((detail: any) => {
                  if (detail.debitAccountId == LcMarginId.toLowerCase()) {
                    this.sumLcMarginTotal += detail.amount;
                  }
                });
              });
            } else {
              this.sumLcMarginTotal = 0;
            }
          },
          error: (err) => {
            return;
          },
        });
      }
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.lcAdjustmentForm.setControl(
          "lcAdjustmentDetails",
          this.fb.array([])
        );

        this.purchaseOrderSubtotal = 0;
        if (result?.adjustmentValue != 0) {
          this.purchaseOrderSubtotal =
            result?.subtotal + result?.adjustmentValue;
        } else {
          this.purchaseOrderSubtotal = result?.subtotal;
        }

        this.lcAdjustmentForm.patchValue({
          purchaseInvoiceId: result?.id,
          purchaseInvoiceNo: result?.purchaseInvoiceNo,
          invoiceTotal: this.purchaseOrderSubtotal,
          lcMarginTotal: this.sumLcMarginTotal,
        });

        const lcAdjustmentDetailInitialData = [
          {
            accountId: result.supplierId,
            amount: this.purchaseOrderSubtotal,
            postType: 1,
          },
          {
            accountId: LcMarginId.toLowerCase(),
            amount: this.sumLcMarginTotal,
            postType: 2,
          },
        ];

        lcAdjustmentDetailInitialData.forEach((item: any) => {
          if (item.amount > 0) {
            const selectedControlAccount = this.findControlAccountById(
              item.accountId
            );
            let lcAdjustmentDetail: any = {
              accountId: selectedControlAccount.id,
              account: selectedControlAccount,
              accountDescription: selectedControlAccount.treeName,
              postType: item.postType,
              amount: item.amount,
            };
            this.addItem(lcAdjustmentDetail);
          }
        });
      }
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

  private addLcAdjustment(body: LcAdjustmentRequestDTO): void {
    this.lcAdjustmentService
      .createLcAdjustment(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  private updateLcAdjustment(body: LcAdjustmentRequestDTO): void {
    this.lcAdjustmentService
      .updateLcAdjustment(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  deletedIds: string = "";
  onDeleteItem(id: string, itemIndex: number): void {
    if (id) this.deletedIds += `${id},`;
    this.lcAdjustmentDetails.removeAt(itemIndex);
  }

  onSubmit(): void {
    if (this.lcAdjustmentForm.valid) {
      console.log("lcMarginTotal: ", this.lcAdjustmentForm.value.lcMarginTotal);
      this.isLoading = true;
      const formValue = this.lcAdjustmentForm.value;
      console.log("FormValue-----", formValue);
      if (!formValue.id) {
        this.addLcAdjustment(formValue);
      } else {
        formValue.deletedLcAdjustmentDetailIds = this.deletedIds;
        this.updateLcAdjustment(formValue);
      }
    }
  }

  check(id: string) {
    if (this.isLoading) return;
    this.isLoading = true;

    this.lcAdjustmentService
      .checkLcAdjustment(id)
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

    this.lcAdjustmentService
      .approveLcAdjustment(id)
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

    this.lcAdjustmentService
      .unpostLcAdjustment(id, status)
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
      .get(environment.apiURL + "/ReportLcAdjustment/" + id, {
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
