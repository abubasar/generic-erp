import { HttpClient } from "@angular/common/http";
import { ChangeDetectorRef, Component, OnInit, ViewChild } from "@angular/core";
import { FormArray, FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MatButton } from "@angular/material/button";
import { MatDialog } from "@angular/material/dialog";
import { MatStepper } from "@angular/material/stepper";
import { ActivatedRoute, Router } from "@angular/router";
import { SaleQuotationStatus } from "app/shared/enums/saleQuotationStatus";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { SnackBarService } from "app/shared/services/snack-bar.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { inFinancialYearValidator } from "app/shared/validators/in-financial-year-validator";
import { CustomerRequest } from "app/views/configuration/models/customer/customer-request.model";
import { Customer } from "app/views/configuration/models/customer/customer.model";
import { ProductRequest } from "app/views/configuration/models/product/product-request.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { CustomerService } from "app/views/configuration/services/customer.service";
import { ProductService } from "app/views/configuration/services/product.service";
import { AddUpdateResponseDTO } from "app/views/purchase/models/add-update-response-dto.model";
import {
  SaleQuotationRequestDTO,
  SaleQuotationRequestDetail,
} from "app/views/sales/models/sale-quotation/sale-quotation-request-dto.model";
import {
  SaleQuotationResponseDTO,
  SaleQuotationResponseDetail,
} from "app/views/sales/models/sale-quotation/sale-quotation-response-dto.model";
import { SaleQuotationService } from "app/views/sales/services/sale-quotation.service";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { finalize } from "rxjs";

@Component({
  selector: "app-sales-quotation-form",
  templateUrl: "./sales-quotation-form.component.html",
  styleUrls: ["./sales-quotation-form.component.scss"],
})
export class SaleQuotationFormComponent implements OnInit {
  @ViewChild("btnCheck") btnCheck: MatButton;
  @ViewChild("btnApprove") btnApprove: MatButton;
  @ViewChild("stepper") private stepper: MatStepper;
  isLoading: boolean = false;
  isDialogOpen: boolean = false;
  isViewMode: boolean = true;
  isChecked: boolean;
  formTitle: string;
  saleQuotationForm: FormGroup;
  products: ProductView[];
  filterProducts: ProductView[];
  customers: Customer[];
  filterCustomers: Customer[];
  saleQuotationDetailsData: any[] = [];
  data: SaleQuotationResponseDTO;

  businessType: string;
  // Define financial year date range here
  financialYearStartDate: Date; //Example: Jul 1, 2023
  financialYearEndDate: Date; //Example: Jun 30, 2024

  currentFinancialYearId: string;

  applyMinMax: boolean = false;

  private path = {
    list: "sales/sales-quotation",
    edit: "sales/sales-quotation",
  };

  constructor(
    private fb: FormBuilder,
    private productService: ProductService,
    private saleQuotationService: SaleQuotationService,
    private customerService: CustomerService,
    private jwtAuth: JwtAuthService,
    private dateFormatService: DateTimeFormatService,
    private toastr: ToastrService,
    private http: HttpClient,
    private activatedRoute: ActivatedRoute,
    private router: Router,
    // private localStorageService: LocalStoreService,
    private cdRef: ChangeDetectorRef,
    public statusColorService: StatusColorService,
    private dialog: MatDialog,
    private snackBarService: SnackBarService,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.activatedRoute.data.subscribe((response: any) => {
      this.data = response?.saleQuotation?.data;
    });
    this.jwtAuth.userProfile.subscribe((res: UserProfile) => {
      this.financialYearStartDate = new Date(res.fystartdate);
      this.financialYearEndDate = new Date(res.fyenddate);
      this.currentFinancialYearId = res.fyid;
      this.businessType = res.businesstype;
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
    this.getAllProducts();
    this.getAllCustomers();
  }

  initializeForm() {
    this.createForm();
    this.populateForm();
    this.setFormTitle();
    this.saleQuotationForm.get("quotationDate").markAsTouched();
  }

  createForm(): void {
    this.saleQuotationForm = this.fb.group({
      id: [this.data?.id || null],
      quotationNo: [this.data?.quotationNo || ""],
      quotationDate: [
        this.data?.quotationDate || this.dateFormatService.getPresentDate(),
      ],
      customerId: [this.data?.customerId || ""],
      referenceNo: [this.data?.referenceNo, Validators.required],
      expiryDate: [this.data?.expiryDate, Validators.required],
      termAndCondition: [this.data?.termAndCondition || ""],
      subtotal: [this.data?.subtotal || 0],
      discount: [this.data?.discount || 0],
      total: [this.data?.total || 0],
      remark: [this.data?.remark || ""],
      deletedSaleQuotationDetailIds: [""],
      saleQuotationDetails: this.fb.array([]),
    });
    this.setMinMaxDates();
  }

  setMinMaxDates() {
    if (this.applyMinMax) {
      this.saleQuotationForm
        .get("quotationDate")
        .setValidators([
          Validators.required,
          inFinancialYearValidator(
            this.financialYearStartDate,
            this.financialYearEndDate
          ),
        ]);
    } else {
      this.saleQuotationForm
        .get("quotationDate")
        .setValidators([Validators.required]);
    }
  }

  get saleQuotationDetails(): FormArray {
    return this.saleQuotationForm.get("saleQuotationDetails") as FormArray;
  }

  populateForm(): void {
    if (this.saleQuotationForm.get("id").value) {
      this.populateSaleQuotationDetails(this.data);
    } else {
      this.addItem();
    }
  }

  setFormTitle(): void {
    if (this.saleQuotationForm.get("id").value) {
      this.formTitle = "Edit Sale Quotation";
    } else {
      this.formTitle = "Add Sale Quotation";
    }
  }

  populateSaleQuotationDetails(data: SaleQuotationResponseDTO): void {
    data.saleQuotationDetails.forEach((item: SaleQuotationResponseDetail) =>
      this.addItem(item)
    );
  }

  addItem(item?: SaleQuotationResponseDetail): void {
    this.saleQuotationDetails.push(this.createSaleQuotationDetail(item));
  }

  createSaleQuotationDetail(item?: SaleQuotationRequestDetail): FormGroup {
    return this.fb.group({
      id: [item?.id || null],
      productId: [item?.productId || "", Validators.required],
      product: [item?.product ? item?.product : ""],
      measurementUnitName: [
        item?.product?.measurementUnit?.name
          ? item?.product?.measurementUnit?.name
          : "",
      ],
      bagWeight: [item?.bagWeight ? item?.bagWeight : 0, Validators.required],
      primaryQuantity: [
        item?.primaryQuantity ? item?.primaryQuantity : 0,
        Validators.required,
      ],
      quantity: [item?.quantity ? item?.quantity : 0, Validators.required],
      rate: [item?.rate ? item?.rate : 0, Validators.required],
      discountAmount: [item?.discountAmount ?? 0],
      discountPerUnit: [item?.discountPerUnit ?? 0],
      amount: [item?.amount ? item?.amount : 0],
      createdOn: [item?.createdOn || null],
      createdBy: [item?.createdBy || null],
    });
  }

  isPassingDataToNextStep = false;
  handleStepChange(event: any) {
    this.isPassingDataToNextStep = event.selectedIndex === 1;
    if (this.isPassingDataToNextStep) {
      this.saleQuotationDetailsData = this.saleQuotationForm.get(
        "saleQuotationDetails"
      ).value;
    }
  }

  getSaleQuotationStatus(value) {
    return this.statusColorService.getSaleQuotationStatus(value);
  }

  getSaleQuotationStatusName(value: number) {
    return SaleQuotationStatus[value];
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    if (fieldName === "customerId") {
      this.saleQuotationForm?.get("customerId").setValue(null);
    }
  }

  clearInputFromDetails(evt: any, itemIndex: number): void {
    evt.stopPropagation();
    const particularDetail = this.saleQuotationDetails.at(itemIndex);
    particularDetail?.patchValue({
      productId: null,
    });
  }

  handleCustomerSearch(event: any): void {
    const name = event.target?.name;
    if (name === "customerId") {
      const term = this.saleQuotationForm.get("customerId");
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
    let customerRequest = new CustomerRequest();
    customerRequest.page = -1;
    this.customerService.getCustomers(customerRequest).subscribe((res) => {
      this.filterCustomers = this.customers = res?.data?.item1;
    });
  }

  getAllProducts(): void {
    let productRequest = new ProductRequest();
    productRequest.isSaleProduct = true;
    productRequest.page = -1;
    this.productService.getProducts(productRequest).subscribe((res) => {
      this.filterProducts = this.products = res?.data?.item1;
    });
  }

  getProductName(productId: string) {
    const product =
      this.products?.find((product) => product?.id === productId) ||
      this.data?.saleQuotationDetails?.find((x) => x?.productId == productId)
        ?.product;
    if (!product) return "";
    return (
      product?.name +
      (this.businessType === "2"
        ? ` (${product.bagWeight} ${product?.measurementUnit?.name})`
        : ` (${product?.packSize?.name})`)
    );
  }

  findProductById(id: string) {
    return this.products?.find((product) => product?.id === id);
  }

  handleProductSelection(event, index) {
    const particularDetail = this.saleQuotationDetails.at(index);
    const productId = event?.option?.value;

    if (this.isExistSelectedProduct(productId, index)) {
      this.showSnackBar("This Product already added!");
      particularDetail.reset();
      return;
    }

    const selectedProduct = this.findProductById(productId);
    if (!selectedProduct) return;

    particularDetail.patchValue({
      productId: selectedProduct?.id,
      product: selectedProduct,
      measurementUnitName: selectedProduct?.measurementUnit?.name,
      bagWeight: selectedProduct.bagWeight,
    });
  }

  showSnackBar(message: string) {
    this.snackBarService.openSnackbar(message);
  }

  isExistSelectedProduct(productId: string, index: number): boolean {
    const isProductAdded = this.saleQuotationDetails.value.some((item, i) => {
      return item.productId === productId && i !== index;
    });
    return isProductAdded;
  }

  handleProductSearch(event: any, itemIndex: number) {
    const name = event?.target?.name;
    if (name === "productId") {
      const term = this.saleQuotationDetails.at(itemIndex).get("productId");
      this.filterProduct(term.value || "");
    }
  }

  filterProduct(searchTerm: string) {
    searchTerm = searchTerm.toLowerCase();
    this.filterProducts = this.products?.filter((product) =>
      product.name.toLowerCase().startsWith(searchTerm)
    );
  }

  calculateCost() {
    const cost = this.saleQuotationDetails?.value?.reduce(
      (sum, item) => sum + item?.quantity * item?.rate,
      0
    );
    const discount = this.saleQuotationDetails?.value?.reduce(
      (sum, item) => sum + item?.discountAmount,
      0
    );
    this.saleQuotationForm.get("discount")?.setValue(discount);
    this.saleQuotationForm.get("subtotal").setValue(cost);
    this.saleQuotationForm.get("total").setValue(cost - discount);
  }

  onControlChange(event: any, itemIndex?: number): void {
    console.log(event, itemIndex);
    const name = event?.target?.name;
    if (
      name === "primaryQuantity" ||
      name === "rate" ||
      name === "discountPerUnit"
    ) {
      this.calculateAmount(itemIndex);
    }
  }
  calculateAmount(itemIndex: number) {
    const item = this.saleQuotationDetails.at(itemIndex);
    const bagWeight = item.get("bagWeight");
    const primaryQuantity = item.get("primaryQuantity");
    const quantity = item.get("quantity");
    const ratePerUnit = item.get("rate")?.value;
    const amount = item.get("amount");
    quantity?.setValue(bagWeight.value * primaryQuantity.value);

    const discountPerUnit = item.get("discountPerUnit")?.value;
    const discountAmount = item.get("discountAmount");
    discountAmount?.setValue(quantity.value * discountPerUnit);

    amount?.setValue(quantity?.value * ratePerUnit);
    this.saleQuotationForm.get("discount").setValue(0);
    this.calculateCost();
  }

  private handleSuccessfulSave(
    res: GeneralResponse<AddUpdateResponseDTO>,
    body: SaleQuotationRequestDTO
  ): void {
    if (res?.succeeded) {
      this.data = {
        ...this.data,
        status: res?.data?.status,
        quotationNo: res?.data?.code,
        id: res?.data?.id,
      };
      // this.localStorageService.setItem(res?.data?.id, {
      //   ...body,
      //   status: res.data?.status,
      //   quotationNo: res.data?.code,
      //   id: res?.data?.id,
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

  private navigateToList() {
    this.router.navigate([this.path.list]);
  }

  backToView() {
    this.initializeForm();
    this.stepper.next();
    this.isViewMode = true;
  }

  private addSaleQuotation(body: SaleQuotationRequestDTO): void {
    this.saleQuotationService
      .createSaleQuotation(body)
      .subscribe((res) => this.handleSuccessfulSave(res, body));
  }

  private updateSaleQuotation(body: SaleQuotationRequestDTO): void {
    this.saleQuotationService
      .updateSaleQuotation(body)
      .subscribe((res) => this.handleSuccessfulSave(res, body));
  }

  deletedIds: string = "";
  onDeleteItem(id: string, itemIndex: number): void {
    if (id) this.deletedIds += `${id},`;
    this.saleQuotationDetails.removeAt(itemIndex);
    this.calculateCost();
  }

  onSubmit(): void {
    if (this.saleQuotationForm.valid) {
      this.isLoading = true;
      const formValue = this.saleQuotationForm.value;
      if (!formValue.id) {
        this.addSaleQuotation(formValue);
      } else {
        formValue.deletedSaleQuotationDetailIds = this.deletedIds;
        this.updateSaleQuotation(formValue);
      }
    }
  }

  check(id: string) {
    if (this.isLoading) return;
    this.isLoading = true;

    this.saleQuotationService
      .checkSaleQuotation(id)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (res) => {
          if (res?.succeeded) {
            this.data.status = res?.data as number;
            this.toastr.info(res.message);
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

    this.saleQuotationService
      .approveSaleQuotation(id)
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

    this.saleQuotationService
      .unpostSaleQuotation(id, status)
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
      .get(environment.apiURL + "/ReportSaleQuotation/" + id, {
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
