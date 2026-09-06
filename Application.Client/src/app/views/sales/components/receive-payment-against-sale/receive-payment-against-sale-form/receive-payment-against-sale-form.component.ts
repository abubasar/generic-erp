import { ChangeDetectorRef, Component, OnInit, ViewChild } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MatDialog } from "@angular/material/dialog";
import { MatStepper } from "@angular/material/stepper";
import { ActivatedRoute, Router } from "@angular/router";
import { ReceivePaymentAgainstSaleStatus } from "app/shared/enums/receivePaymentAgainstSaleStatus";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { CostCenter } from "app/views/configuration/models/cost-center/cost-center.model";
import { AccountService } from "app/views/configuration/services/account.service";
import { CostCenterService } from "app/views/configuration/services/cost-center.service";
import { CustomerService } from "app/views/configuration/services/customer.service";
import { ToastrService } from "ngx-toastr";

import { HttpClient } from "@angular/common/http";
import { MatButton } from "@angular/material/button";
import { Control_Accounts_Parent_Id } from "app/shared/consts/const";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { inFinancialYearValidator } from "app/shared/validators/in-financial-year-validator";
import { Customer } from "app/views/configuration/models/customer/customer.model";
import { AddUpdateResponseDTO } from "app/views/purchase/models/add-update-response-dto.model";
import { ReceivePaymentAgainstSaleRequestDTO } from "app/views/sales/models/receive-payment-against-sale/receive-payment-against-sale-request-dto.model";
import { ReceivePaymentAgainstSaleResponseDTO } from "app/views/sales/models/receive-payment-against-sale/receive-payment-against-sale-response-dto.model";
import { ReceivePaymentAgainstSaleService } from "app/views/sales/services/receive-payment-against-sale.service";
import { SaleInvoiceService } from "app/views/sales/services/sale-invoice.service";
import { environment } from "environments/environment";
import { finalize } from "rxjs";
import { SaleInvoiceListComponent } from "../sale-invoice-list/sale-invoice-list.component";

@Component({
  selector: "app-receive-payment-against-sale-form",
  templateUrl: "./receive-payment-against-sale-form.component.html",
  styleUrls: ["./receive-payment-against-sale-form.component.scss"],
})
export class ReceivePaymentAgainstSaleFormComponent implements OnInit {
  @ViewChild("btnCheck") btnCheck: MatButton;
  @ViewChild("btnApprove") btnApprove: MatButton;
  @ViewChild("stepper") private stepper: MatStepper;
  isLoading: boolean = false;
  isDialogOpen: boolean = false;
  isViewMode: boolean = true;
  // isChecked: boolean;
  formTitle: string;
  receivePaymentAgainstSaleForm: FormGroup;
  customers: Customer[];
  filterCustomers: Customer[];
  customerBalance: number;
  costCenters: CostCenter[];
  toAccounts: any[];
  isAmountGreaterThanCustomerBalance: boolean = false;
  amountGreaterThanCustomerBalanceMessage: string;
  data: ReceivePaymentAgainstSaleResponseDTO;

  // Define financial year date range here
  financialYearStartDate: Date; //Example: Jul 1, 2023
  financialYearEndDate: Date; //Example: Jun 30, 2024
  currentFinancialYearId: string;
  applyMinMax: boolean = false;

  private path = {
    list: "sales/receive-payment-against-sale/",
    edit: "sales/receive-payment-against-sale",
  };

  constructor(
    public dialog: MatDialog,
    private fb: FormBuilder,
    private customerService: CustomerService,
    private costCenterService: CostCenterService,
    private accountService: AccountService,
    private saleInvoiceService: SaleInvoiceService,
    private receivePaymentAgainstSaleService: ReceivePaymentAgainstSaleService,
    private jwtAuth: JwtAuthService,
    private dateFormatService: DateTimeFormatService,
    private toastr: ToastrService,
    private activatedRoute: ActivatedRoute,
    private router: Router,
    private http: HttpClient,
    // private localStorageService: LocalStoreService,
    private cdRef: ChangeDetectorRef,
    private statusColorService: StatusColorService,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    console.log(
      "Activated route data in Component:::",
      this.activatedRoute.data
    );
    this.activatedRoute.data.subscribe((response: any) => {
      console.log("DATA FETCHING", response);
      this.data = response?.receivePaymentAgainstSale?.data;
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
      this.getCustomerBalance(this.data.customerId);
      this.checkAmount();
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
    // this.getCustomerBalance(this.data.customerId);
    // this.checkAmount();
    this.stepper.previous();
    this.isViewMode = false;
    this.applyMinMax = true;
    this.initializeForm();
  }

  getData() {
    // const id = this.activatedRoute.snapshot.paramMap.get("id");
    // if (id) this.data = this.localStorageService.getItem(id);

    this.getAllCustomers();
    this.getAllCostCenters();
    this.getAllToAccounts();
    // this.getAllSaleInvoices();
  }

  initializeForm() {
    this.createForm();
    this.setFormTitle();
    this.receivePaymentAgainstSaleForm.get("paymentDate").markAsTouched();
  }

  createForm(): void {
    this.receivePaymentAgainstSaleForm = this.fb.group({
      id: [this.data?.id || null],
      paymentDate: [
        this.data?.paymentDate || this.dateFormatService.getPresentDate(),
        Validators.required,
      ],
      invoiceNo: [this.data?.invoiceNo ?? ""],
      customerId: [this.data?.customerId || null],
      customerBalance: [""],
      amount: [this.data?.amount ?? 0, Validators.required],
      costCenterId: [this.data?.costCenterId, Validators.required],
      toAccountId: [this.data?.toAccountId, Validators.required],
      remark: [this.data?.remark ?? ""],
    });
    this.setMinMaxDates();
  }

  setMinMaxDates() {
    if (this.applyMinMax) {
      this.receivePaymentAgainstSaleForm
        .get("paymentDate")
        .setValidators([
          Validators.required,
          inFinancialYearValidator(
            this.financialYearStartDate,
            this.financialYearEndDate
          ),
        ]);
    } else {
      this.receivePaymentAgainstSaleForm
        .get("paymentDate")
        .setValidators([Validators.required]);
    }
    this.receivePaymentAgainstSaleForm
      .get("paymentDate")
      .updateValueAndValidity();
  }

  setFormTitle() {
    if (this.receivePaymentAgainstSaleForm.get("id").value) {
      this.formTitle = "Edit Receive Payment Against Sale";
    } else {
      this.formTitle = "Add Receive Payment Against Sale";
    }
  }

  isPassingDataToNextStep = false;
  handleStepChange(event: any) {
    this.isPassingDataToNextStep = event.selectedIndex === 1;
  }

  handleCustomerSearch(event: any): void {
    const name = event.target?.name;
    if (name === "customerId") {
      const term = this.receivePaymentAgainstSaleForm.get("customerId");
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

  onSelectedCustomer(customerId: string) {
    this.getCustomerBalance(customerId);
  }

  getCustomerBalance(customerId: string) {
    this.customerService.getCustomerBalance(customerId).subscribe((res) => {
      console.log(res?.data?.balance);
      this.customerBalance = res?.data?.balance;
      this.receivePaymentAgainstSaleForm
        ?.get("customerBalance")
        ?.setValue(res?.data?.balance);
      this.checkAmount();
    });
  }

  checkAmount(): void {
    let dueBalance =
      this.receivePaymentAgainstSaleForm.get("customerBalance").value;
    const amount = this.receivePaymentAgainstSaleForm.get("amount").value;

    this.isAmountGreaterThanCustomerBalance = amount > dueBalance;

    // Set the message if credit balance is insufficient
    if (this.isAmountGreaterThanCustomerBalance) {
      this.amountGreaterThanCustomerBalanceMessage = `Amount cannot be greater than Customer Balance. \n Customer Balance is: ${parseFloat(
        dueBalance
      ).toFixed(2)} \n Amount: ${this.receivePaymentAgainstSaleForm
        .get("amount")
        .value?.toFixed(2)}`;
    }

    console.log(
      this.receivePaymentAgainstSaleForm.get("customerBalance").value +
        this.amountGreaterThanCustomerBalanceMessage
    );
  }

  getAllCustomers(): void {
    this.customerService.getAllCustomers().subscribe((res) => {
      this.filterCustomers = this.customers = res?.data?.item1;
    });
  }

  getAllCostCenters() {
    this.costCenterService.getAllCostCenters().subscribe((res) => {
      this.costCenters = res?.data?.item1;
    });
  }

  getAllToAccounts() {
    this.accountService
      .getControlAccountsByParentId(Control_Accounts_Parent_Id)
      .subscribe((res) => {
        console.log(res);
        this.toAccounts = res?.data;
      });
  }

  // this code will be removed after implementing Dialog Component
  // saleInvoices: SaleInvoiceResponseDTO[];
  // getAllSaleInvoices(): void {
  //   const requestBody = new SaleInvoiceSearchRequestDTO();
  //   requestBody.page = -1;
  //   this.saleInvoiceService.getSaleInvoices(requestBody).subscribe((res) => {
  //     console.log(res);
  //     this.saleInvoices = res?.data?.item1;
  //   });
  // }

  getCostCenterName(costCenterId: string) {
    return this.costCenters?.find((x) => x.id === costCenterId)?.name;
  }

  getToAccountName(toAccountId: string) {
    return this.toAccounts?.find((x) => x.id === toAccountId)?.name;
  }

  // getLinkedDocument(saleInvoiceId: string) {
  //   const document = this.saleInvoices?.find((x) => x.id === saleInvoiceId);
  //   return document?.invoiceNo + "," + document?.customer?.name;
  // }

  private handleSuccessfulSave(
    res: GeneralResponse<AddUpdateResponseDTO>,
    body: ReceivePaymentAgainstSaleRequestDTO
  ): void {
    if (res?.succeeded) {
      this.data = {
        ...this.data,
        status: res?.data?.status,
        code: res?.data?.code,
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
      console.log("res", res);
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
    console.log(id);
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

  private addReceivePaymentAgainstSale(
    body: ReceivePaymentAgainstSaleRequestDTO
  ): void {
    this.receivePaymentAgainstSaleService
      .createReceivePaymentAgainstSale(body)
      .subscribe((res) => this.handleSuccessfulSave(res, body));
  }

  private updateReceivePaymentAgainstSale(
    body: ReceivePaymentAgainstSaleRequestDTO
  ): void {
    this.receivePaymentAgainstSaleService
      .updateReceivePaymentAgainstSale(body)
      .subscribe((res) => this.handleSuccessfulSave(res, body));
  }

  onSubmit(): void {
    if (this.receivePaymentAgainstSaleForm.valid) {
      this.isLoading = true;
      const formValue = this.receivePaymentAgainstSaleForm.value;
      console.log(formValue);
      if (!formValue.id) {
        this.addReceivePaymentAgainstSale(formValue);
      } else {
        this.updateReceivePaymentAgainstSale(formValue);
      }
    }
  }

  check(id: string) {
    if (this.isLoading) return;
    this.isLoading = true;

    this.receivePaymentAgainstSaleService
      .checkReceivePaymentAgainstSale(id)
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

  approve(id: string) {
    if (this.isLoading) return;
    this.isLoading = true;

    this.receivePaymentAgainstSaleService
      .approveReceivePaymentAgainstSale(id)
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

    this.receivePaymentAgainstSaleService
      .unpostReceivePaymentAgainstSale(id, status)
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

  //openSaleInvoiceDialog
  openSaleInvoiceDialog() {
    const dialogRef = this.dialog.open(SaleInvoiceListComponent, {
      disableClose: true,
      panelClass: "add-bill-container",
      minHeight: "auto",
      height: "auto",
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        console.log("result", result);
        this.receivePaymentAgainstSaleForm.reset();

        this.receivePaymentAgainstSaleForm.markAllAsTouched();

        this.receivePaymentAgainstSaleForm.patchValue({
          paymentDate: this.dateFormatService.getPresentDate(),
          saleInvoiceId: result?.id,
          amount: result?.total,
          customerId: result?.customerId,
        });
      }
    });
  }

  getReceivePaymentAgainstSaleStatus(value) {
    return this.statusColorService.getReceivePaymentAgainstSaleStatus(value);
  }

  getReceivePaymentAgainstSaleStatusName(value: number) {
    return ReceivePaymentAgainstSaleStatus[value];
  }

  printPdf(id) {
    this.http
      .get(environment.apiURL + "/ReportReceivePaymentAgainstSale/" + id, {
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
