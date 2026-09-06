import { ChangeDetectorRef, Component, OnInit, ViewChild } from "@angular/core";
import {
  FormArray,
  FormBuilder,
  FormGroup,
  ValidationErrors,
  Validators,
} from "@angular/forms";
import { MatDialog } from "@angular/material/dialog";
import { MatStepper } from "@angular/material/stepper";
import { ActivatedRoute, Router } from "@angular/router";
import { SaleReturnStatus } from "app/shared/enums/saleReturnStatus";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { Customer } from "app/views/configuration/models/customer/customer.model";
import { ProductRequest } from "app/views/configuration/models/product/product-request.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { Store } from "app/views/configuration/models/store/store.model";
import { CustomerWiseProductDiscountService } from "app/views/configuration/services/customer-wise-product-discount.service";
import { CustomerService } from "app/views/configuration/services/customer.service";
import { ProductService } from "app/views/configuration/services/product.service";
import { StoreService } from "app/views/configuration/services/store.service";
import { SaleReturnRequestDTO } from "app/views/sales/models/sale-return/sale-return-request-dto.model";
import {
  SaleReturnResponseDetail,
  SaleReturnResponseDTO,
} from "app/views/sales/models/sale-return/sale-return-response-dto.model";
import { SaleReturnService } from "app/views/sales/services/sale-return.service";
import { ToastrService } from "ngx-toastr";

import { HttpClient } from "@angular/common/http";
import { MatButton } from "@angular/material/button";
import { Inventory_Type_Id_Finished_Goods } from "app/shared/consts/const";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { SnackBarService } from "app/shared/services/snack-bar.service";
import { inFinancialYearValidator } from "app/shared/validators/in-financial-year-validator";
import { StoreRequest } from "app/views/configuration/models/store/store-request.model";
import {
  DeliveryNoteResponseDetail,
  DeliveryNoteResponseDTO,
} from "app/views/sales/models/delivery-note/delivery-note-response-dto.model";
import {
  SaleInvoiceResponseDetail,
  SaleInvoiceResponseDTO,
} from "app/views/sales/models/sale-invoice/sale-invoice-response-dto.model";
import { environment } from "environments/environment";
import { finalize } from "rxjs";
import { DeliveryNoteDialogComponent } from "../delivery-note-dialog/delivery-note-dialog.component";
import { SaleInvoiceDialogComponent } from "../sale-invoice-dialog/sale-invoice-dialog.component";

@Component({
  selector: "app-sale-return-form",
  templateUrl: "./sale-return-form.component.html",
  styleUrls: ["./sale-return-form.component.scss"],
})
export class SaleReturnFormComponent implements OnInit {
  @ViewChild("btnCheck") btnCheck: MatButton;
  @ViewChild("btnApprove") btnApprove: MatButton;
  @ViewChild("stepper") private stepper: MatStepper;
  isDataLoading: boolean = false;
  isLoading: boolean = false;
  isDialogOpen: boolean = false;
  isViewMode: boolean = true;
  isChecked: boolean;
  formTitle: string;
  saleReturnForm: FormGroup;
  customers: Customer[];
  filterCustomers: Customer[];
  stores: Store[];
  filterStores: Store[];
  products: ProductView[];
  searchProducts: ProductView[];
  saleReturnDetailsData: any[] = [];
  data: SaleReturnResponseDTO;

  businessType: string;
  // Define financial year date range here
  financialYearStartDate: Date; //Example: Jul 1, 2023
  financialYearEndDate: Date; //Example: Jun 30, 2024
  currentFinancialYearId: string;
  applyMinMax: boolean = false;

  private path = {
    list: "/sales/sale-return",
    edit: "sales/sale-return",
  };

  constructor(
    public dialog: MatDialog,
    private fb: FormBuilder,
    private customerService: CustomerService,
    private storeService: StoreService,
    private enumValueService: EnumValueService,
    private productService: ProductService,
    private saleReturnService: SaleReturnService,
    private jwtAuth: JwtAuthService,
    private dateFormatService: DateTimeFormatService,
    private toastr: ToastrService,
    private activatedRoute: ActivatedRoute,
    private router: Router,
    private http: HttpClient,
    private cdRef: ChangeDetectorRef,
    public statusColorService: StatusColorService,
    private customerWiseProductDiscountService: CustomerWiseProductDiscountService,
    private snackBarService: SnackBarService,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.activatedRoute.data.subscribe((response: any) => {
      this.data = response.saleReturn?.data;
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

  showEdit() {
    this.stepper.previous();
    this.isViewMode = false;
    this.applyMinMax = true;
    this.initializeForm();
  }

  getData() {
    this.getAllStores();
    this.getAllCustomers();
    //! it's important after close openSaleReturnDialog
    this.getAllProducts();
  }

  initializeForm() {
    this.createForm();
    this.populateForm();
    this.setFormTitle();
    this.saleReturnForm.get("saleReturnDate").markAsTouched();
  }

  createForm(): void {
    this.saleReturnForm = this.fb.group({
      id: [this.data?.id || null],
      invoiceNo: [
        this.data?.invoiceNo || "",
        this.businessType === "1" ? Validators.required : null,
      ],
      deliveryNoteNo: [
        this.data?.deliveryNoteNo || "",
        this.businessType === "2" ? Validators.required : null,
      ],
      referenceNo: [this.data?.referenceNo || ""],
      saleReturnDate: [
        this.data?.saleReturnDate || this.dateFormatService.getPresentDate(),
      ],
      storeId: [this.data?.storeId, Validators.required],
      customerId: [this.data?.customerId, Validators.required],
      customerMarketingOfficerId: [
        this.data?.customerMarketingOfficerId || null,
      ],
      customerTerritoryId: [this.data?.customerTerritoryId || null],
      subtotal: [this.data?.subtotal ?? 0],
      totalPercentageDiscountAmount: [
        this.data?.totalPercentageDiscountAmount ?? 0,
      ],
      otherDiscount: [this.data?.otherDiscount ?? 0],
      total: [this.data?.total, Validators.required],
      remark: [this.data?.remark || ""],
      deletedSaleReturnDetailIds: [""],
      saleReturnDetails: this.fb.array([]),
    });
    this.setMinMaxDates();
  }

  setMinMaxDates() {
    if (this.applyMinMax) {
      this.saleReturnForm
        .get("saleReturnDate")
        .setValidators([
          Validators.required,
          inFinancialYearValidator(
            this.financialYearStartDate,
            this.financialYearEndDate
          ),
        ]);
    } else {
      this.saleReturnForm
        .get("saleReturnDate")
        .setValidators([Validators.required]);
    }

    this.saleReturnForm.get("saleReturnDate").updateValueAndValidity();
  }

  get saleReturnDetails(): FormArray {
    return this.saleReturnForm?.get("saleReturnDetails") as FormArray;
  }

  populateForm(): void {
    if (this.saleReturnForm?.get("id").value) {
      this.populateSaleReturnDetails(this.data);
    } else {
      this.addItem();
    }
  }

  setFormTitle(): void {
    if (this.saleReturnForm.get("id").value) {
      this.formTitle = "Edit Sale Return";
    } else {
      this.formTitle = "Add Sale Return";
    }
  }

  populateSaleReturnDetails(data: SaleReturnResponseDTO): void {
    data.saleReturnDetails.forEach((item: SaleReturnResponseDetail) =>
      this.addItem(item)
    );
  }

  addItem(item?: SaleReturnResponseDetail): void {
    this.saleReturnDetails.push(this.createSaleReturnDetail(item));
  }

  createSaleReturnDetail(item?: SaleReturnResponseDetail): FormGroup {
    return this.fb.group(
      {
        id: [item?.id ?? null],
        productId: [item?.productId ?? "", Validators.required],
        product: [item?.product ?? ""],
        measurementUnitName: [item?.product?.measurementUnit?.name ?? ""],
        bagWeight: [item?.bagWeight, Validators.required],
        primaryQuantity: [item?.primaryQuantity ?? 0, Validators.required],
        quantity: [item?.quantity, Validators.required],
        primaryBonusQuantity: [
          item?.primaryBonusQuantity ?? 0,
          Validators.required,
        ],
        bonusQuantity: [item?.bonusQuantity ?? 0, Validators.required],
        rate: [item?.rate, Validators.required],
        returnPrimaryQuantity: [
          item?.returnPrimaryQuantity,
          Validators.required,
        ],
        returnQuantity: [item?.returnQuantity, Validators.required],
        returnPrimaryBonusQuantity: [
          item?.returnPrimaryBonusQuantity ?? 0,
          Validators.required,
        ],
        returnBonusQuantity: [
          item?.returnBonusQuantity ?? 0,
          Validators.required,
        ],
        vatPercentage: [item?.vatPercentage ?? 0],
        discountPercentage: [item?.discountPercentage ?? 0],
        percentageDiscountAmount: [item?.percentageDiscountAmount ?? 0],
        otherDiscountPerUnit: [item?.otherDiscountPerUnit ?? 0],
        amount: [item?.amount, Validators.required],
        createdOn: [item?.createdOn || null],
        createdBy: [item?.createdBy || null],
      },
      {
        validators: [
          this.validateReturnQuantity,
          this.validateReturnBonusQuantity,
        ],
      }
    );
  }

  validateReturnQuantity(group: FormGroup): ValidationErrors | null {
    const deliveredPrimaryQuantity = group.get("primaryQuantity")?.value ?? 0;
    const returnPrimaryQuantity =
      group.get("returnPrimaryQuantity")?.value ?? 0;

    return deliveredPrimaryQuantity >= returnPrimaryQuantity
      ? null
      : {
          returnQuantityExceedsDeliveredQuantity:
            "Return quantity cannot exceed delivered quantity.",
        };
  }

  validateReturnBonusQuantity(group: FormGroup): ValidationErrors | null {
    const deliveredPrimaryBonusQuantity =
      group.get("primaryBonusQuantity")?.value ?? 0;
    const returnPrimaryBonusQuantity =
      group.get("returnPrimaryBonusQuantity")?.value ?? 0;

    return deliveredPrimaryBonusQuantity >= returnPrimaryBonusQuantity
      ? null
      : {
          returnBonusQuantityExceedsDeliveredBonusQuantity:
            "Return bonus quantity cannot exceed delivered bonus quantity.",
        };
  }

  isPassingDataToNextStep = false;
  handleStepChange(event: any) {
    this.isPassingDataToNextStep = event?.selectedIndex === 1;
    if (this.isPassingDataToNextStep) {
      this.saleReturnDetailsData =
        this.saleReturnForm.get("saleReturnDetails").value;
    }
  }

  // getSaleReturnById(id: string): void {
  //   this.saleReturnService.getSaleReturnById(id).subscribe((res) => {
  //     console.log(res);
  //     this.data = res?.data;
  //     this.initializeForm();
  //   });
  // }

  getSaleReturnStatus(value) {
    return this.statusColorService.getSaleReturnStatus(value);
  }

  getSaleReturnStatusName(value: number) {
    return SaleReturnStatus[value];
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    if (fieldName === "customerId") {
      this.saleReturnForm?.get("customerId").setValue(null);
    }
  }

  clearInputFromDetails(evt: any, itemIndex: number): void {
    evt.stopPropagation();
    const particularDetail = this.saleReturnDetails.at(itemIndex);
    particularDetail?.patchValue({
      productId: null,
    });
  }

  handleCustomerSearch(event: any): void {
    const name = event.target?.name;
    if (name === "customerId") {
      const term = this.saleReturnForm.get("customerId");
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

  getAllCustomers(): void {
    this.customerService.getAllCustomers().subscribe((res) => {
      this.filterCustomers = this.customers = res?.data?.item1;
    });
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

  getAllStores(): void {
    let request = new StoreRequest();
    request.page = -1;
    request.inventoryTypeId = Inventory_Type_Id_Finished_Goods;
    this.storeService.getStores(request).subscribe((res) => {
      this.stores = res?.data?.item1;
    });
  }

  getStoreName(storeId: string) {
    if (!storeId) {
      return;
    }
    const store =
      this.stores?.find((store) => store.id === storeId) || this.data?.store;
    return store.name;
  }

  handleStoreSearch(event: any): void {
    const name = event.target?.name;
    if (name === "storeId") {
      const term = this.saleReturnForm.get("storeId");
      this.filterStore(term.value || "");
    }
  }

  private filterStore(value: string) {
    const filterValue = value.trim().toLowerCase();
    this.filterStores = this.stores?.filter((option) =>
      option.name.toLowerCase().includes(filterValue)
    );
  }

  getAllProducts(): void {
    let productRequest = new ProductRequest();
    //productRequest.inventoryTypeId = Inventory_Type_Id_Finished_Goods;
    productRequest.isSaleProduct = true;
    productRequest.page = -1;
    this.productService.getProducts(productRequest).subscribe((res) => {
      this.searchProducts = this.products = res?.data?.item1;
    });
  }

  // onSelectedCustomer(customerId: string) {
  //   this.customerService
  //     .getCustomerCreditLimitBalance(customerId)
  //     .subscribe((res) => {
  //       this.saleReturnForm
  //         ?.get("creditLimit")
  //         ?.setValue(res?.data?.creditLimit);
  //       this.saleReturnForm?.get("limitAvailed")?.setValue(res?.data?.balance);
  //     });
  // }

  getProductName(productId: string) {
    const product =
      this.products?.find((product) => product?.id === productId) ||
      this.data?.saleReturnDetails?.find((x) => x?.productId == productId)
        ?.product;
    // console.log(productId, product);
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

  handleProductSelection(event: any, index: number) {
    const particularDetail = this.saleReturnDetails.at(index);
    const productId = event?.option?.value;
    if (this.isExistSelectedProduct(productId, index)) {
      this.showSnackBar("This Product already added!");
      particularDetail?.patchValue({
        productId: null,
      });
      return;
    }

    const selectedProduct = this.findProductById(productId);
    if (!selectedProduct) return;
    particularDetail.patchValue({
      productId: selectedProduct?.id,
      product: selectedProduct,
      measurementUnitName: selectedProduct?.measurementUnit?.name,
      bagWeight: selectedProduct.bagWeight,
      rate: selectedProduct.salePrice,
    });
    this.calculateAmount(index);
  }

  showSnackBar(message: string) {
    this.snackBarService.openSnackbar(message);
  }

  isExistSelectedProduct(productId: string, index: number): boolean {
    const isProductAdded = this.saleReturnDetails.value.some((item, i) => {
      return item.productId === productId && i !== index;
    });
    return isProductAdded;
  }

  handleProductSearch(event: any, itemIndex: number) {
    const name = event?.target?.name;
    if (name === "productId") {
      const term = this.saleReturnDetails.at(itemIndex).get("productId");
      this.filterProduct(term.value || "");
    }
  }

  filterProduct(searchTerm: string) {
    searchTerm = searchTerm.toLowerCase();
    this.searchProducts = this.products?.filter((product) =>
      product.name.toLowerCase().startsWith(searchTerm)
    );
  }

  calculateCost() {
    const subtotal = this.saleReturnDetails?.value?.reduce(
      (sum, item) => sum + item?.returnQuantity * item?.rate,
      0
    );

    const totalPercentageDiscountAmount = this.saleReturnDetails?.value?.reduce(
      (sum, item) => sum + item?.percentageDiscountAmount,
      0
    );

    const otherDiscount = this.saleReturnDetails?.value?.reduce(
      (sum, item) => sum + item?.otherDiscountPerUnit * item?.returnQuantity,
      0
    );

    this.saleReturnForm?.get("subtotal")?.setValue(subtotal);
    this.saleReturnForm
      ?.get("totalPercentageDiscountAmount")
      ?.setValue(totalPercentageDiscountAmount);
    this.saleReturnForm?.get("otherDiscount")?.setValue(otherDiscount);
    this.saleReturnForm
      ?.get("total")
      ?.setValue(subtotal - totalPercentageDiscountAmount - otherDiscount);
  }

  onControlChange(event: any, itemIndex?: number): void {
    const name = event?.target?.name;
    if (name === "returnPrimaryQuantity") {
      this.calculateAmount(itemIndex);
    }
    if (name === "returnPrimaryBonusQuantity") {
      this.setReturnBonusQuantity(itemIndex);
    }
  }

  setReturnBonusQuantity(itemIndex: number) {
    const item = this.saleReturnDetails?.at(itemIndex);
    const bagWeight = item?.get("bagWeight")?.value;
    const returnPrimaryBonusQuantity = item?.get(
      "returnPrimaryBonusQuantity"
    ).value;
    const returnBonusQuantity = item?.get("returnBonusQuantity");
    returnBonusQuantity.setValue(bagWeight * returnPrimaryBonusQuantity);
  }

  calculateAmount(itemIndex: number) {
    const item = this.saleReturnDetails?.at(itemIndex);
    const bagWeight = item?.get("bagWeight")?.value;
    const primaryQuantity = item?.get("primaryQuantity")?.value;
    const primaryBonusQuantity = item?.get("primaryBonusQuantity")?.value;
    const quantity = item?.get("quantity");
    quantity.setValue(bagWeight * primaryQuantity);
    const ratePerUnit = item?.get("rate")?.value;
    const returnPrimaryQuantity = item?.get("returnPrimaryQuantity").value;
    const returnPrimaryBonusQuantity = item?.get("returnPrimaryBonusQuantity");
    const returnBonusQuantity = item?.get("returnBonusQuantity");
    const returnQuantity = item?.get("returnQuantity");
    if (returnPrimaryQuantity === primaryQuantity) {
      returnPrimaryBonusQuantity.setValue(primaryBonusQuantity);
      returnBonusQuantity.setValue(
        returnPrimaryBonusQuantity.value * bagWeight
      );
    } else {
      returnPrimaryBonusQuantity.setValue(0);
      returnBonusQuantity.setValue(0);
    }
    const discountPercentage = item?.get("discountPercentage")?.value;
    const percentageDiscountAmount = item?.get("percentageDiscountAmount");
    percentageDiscountAmount?.setValue(
      (returnPrimaryQuantity * ratePerUnit * discountPercentage) / 100
    );
    returnQuantity.setValue(bagWeight * returnPrimaryQuantity);
    const amount = item?.get("amount");
    amount?.setValue(returnQuantity.value * ratePerUnit);
    this.calculateCost();
  }

  private handleSuccessfulSave(res: GeneralResponse<string>): void {
    if (res?.succeeded) {
      this.handleSaleReturnResponse(res?.data);
      this.toastr.success(res?.message);
      this.deletedIds = "";
    } else {
      this.isLoading = false;
      this.toastr.success(res?.message);
    }
  }

  private handleSaleReturnResponse(saleReturnId: string): void {
    this.saleReturnService.getSaleReturnById(saleReturnId).subscribe({
      next: (saleReturnResponse) => {
        this.data = saleReturnResponse.data;
        this.isViewMode = true;
        this.isLoading = false;
        this.navigateToView(saleReturnResponse?.data?.id);
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

  private addSaleReturn(body: SaleReturnRequestDTO): void {
    this.saleReturnService
      .createSaleReturn(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  private updateSaleReturn(body: SaleReturnRequestDTO): void {
    this.saleReturnService
      .updateSaleReturn(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  deletedIds: string = "";
  onDeleteItem(id: string, itemIndex: number): void {
    if (id) this.deletedIds += `${id},`;
    this.saleReturnDetails.removeAt(itemIndex);
    this.calculateCost();
  }

  onSubmit(): void {
    if (this.saleReturnForm.valid) {
      this.isLoading = true;
      const formValue = this.saleReturnForm.value;
      if (!formValue.id) {
        this.addSaleReturn(formValue);
      } else {
        formValue.deletedSaleReturnDetailIds = this.deletedIds;
        this.updateSaleReturn(formValue);
      }
    }
  }

  //openSaleInvoiceDialog
  openSaleInvoiceDialog() {
    const dialogRef = this.dialog.open(SaleInvoiceDialogComponent, {
      disableClose: true,
      panelClass: "add-bill-container",
      minHeight: "auto",
      height: "auto",
    });
    dialogRef.afterClosed().subscribe((result: SaleInvoiceResponseDTO) => {
      if (!result) {
        return;
      }
      debugger;
      console.log(result);
      this.saleReturnForm.reset();
      this.saleReturnForm.setControl("saleReturnDetails", this.fb.array([]));
      this.saleReturnForm.markAllAsTouched();

      // console.log("Data Checking:  ", result);
      // this.saleReturnForm.markAllAsTouched();
      this.saleReturnForm.patchValue({
        invoiceNo: result?.invoiceNo,
        deliveryNoteNo: "",
        storeId: result?.storeId,
        customerId: result?.customerId,
        customerMarketingOfficerId: result?.customerMarketingOfficerId,
        customerTerritoryId: result?.customerTerritoryId,
        saleReturnDate: this.dateFormatService.getPresentDate(),
        remark: result?.remark,
        subtotal: 0,
        totalPercentageDiscountAmount: 0,
        otherDiscount: 0,
        total: 0,
      });
      result.saleInvoiceDetails.forEach((item: SaleInvoiceResponseDetail) => {
        let saleReturnResponseDetail: SaleReturnResponseDetail = {
          productId: item?.productId,
          product: item?.product,
          bagWeight: item?.product?.bagWeight,
          primaryQuantity: item?.primaryQuantity,
          quantity: item?.quantity,
          primaryBonusQuantity: item?.primaryBonusQuantity,
          bonusQuantity: item?.bonusQuantity,
          rate: item?.netRate,
          returnPrimaryQuantity: 0,
          returnQuantity: 0,
          returnPrimaryBonusQuantity: 0,
          returnBonusQuantity: 0,
          vatPercentage: item.vatPercentage,
          discountPercentage: item.discountPercentage,
          percentageDiscountAmount: 0,
          otherDiscountPerUnit: item.otherDiscountPerUnit,
          amount: 0,
        };
        this.addItem(saleReturnResponseDetail);
      });
    });
  }

  //openDeliveryNoteDialog
  openDeliveryNoteDialog() {
    const dialogRef = this.dialog.open(DeliveryNoteDialogComponent, {
      disableClose: true,
      panelClass: "add-bill-container",
      minHeight: "auto",
      height: "auto",
    });
    dialogRef.afterClosed().subscribe((result: DeliveryNoteResponseDTO) => {
      if (!result) {
        return;
      }

      this.saleReturnForm.reset();
      this.saleReturnForm.setControl("saleReturnDetails", this.fb.array([]));
      this.saleReturnForm.markAllAsTouched();

      console.log("Data Checking:  ", result);
      // this.saleReturnForm.markAllAsTouched();
      this.saleReturnForm.patchValue({
        invoiceNo: "",
        deliveryNoteNo: result?.deliveryNoteNo,
        storeId: result?.storeId,
        customerId: result?.customerId,
        saleReturnDate: this.dateFormatService.getPresentDate(),
        remark: result?.remark,
        subtotal: 0,
        totalPercentageDiscountAmount: 0,
        otherDiscount: 0,
        total: 0,
      });
      result.deliveryNoteDetails.forEach((item: DeliveryNoteResponseDetail) => {
        let saleReturnResponseDetail: SaleReturnResponseDetail = {
          productId: item?.productId,
          product: item?.product,
          bagWeight: item?.bagWeight,
          primaryQuantity: item?.deliveryPrimaryQuantity,
          quantity: item?.deliveryQuantity,
          primaryBonusQuantity: item?.deliveryPrimaryBonusQuantity,
          bonusQuantity: item?.bagWeight * item?.deliveryPrimaryBonusQuantity,
          rate: item?.netRate,
          returnPrimaryQuantity: 0,
          returnQuantity: 0,
          returnPrimaryBonusQuantity: 0,
          returnBonusQuantity: 0,
          vatPercentage: 0,
          discountPercentage: 0,
          percentageDiscountAmount: 0,
          otherDiscountPerUnit: item?.otherDiscountPerUnit,
          amount: 0,
        };
        this.addItem(saleReturnResponseDetail);
      });
    });
  }

  check(id: string) {
    if (this.isLoading) return;
    this.isLoading = true;

    this.saleReturnService
      .checkSaleReturn(id)
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

    this.saleReturnService
      .approveSaleReturn(id)
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

    this.saleReturnService
      .unpostSaleReturn(id, status)
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
      .get(environment.apiURL + "/ReportSaleReturn/" + id, {
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
