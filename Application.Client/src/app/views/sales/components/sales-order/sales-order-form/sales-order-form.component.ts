import { ChangeDetectorRef, Component, OnInit, ViewChild } from "@angular/core";
import { FormArray, FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MatStepper } from "@angular/material/stepper";
import { ActivatedRoute, Router } from "@angular/router";
import { SaleOrderStatus } from "app/shared/enums/saleOrderStatus";
import { Transport } from "app/shared/enums/transport";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { Customer } from "app/views/configuration/models/customer/customer.model";
import { ProductRequest } from "app/views/configuration/models/product/product-request.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { Store } from "app/views/configuration/models/store/store.model";
import { CustomerService } from "app/views/configuration/services/customer.service";
import { StoreService } from "app/views/configuration/services/store.service";
import { SaleOrderRequestDTO } from "app/views/sales/models/sale-order/sale-order-request-dto.model";
import {
  SaleOrderResponseDTO,
  SaleOrderResponseDetail,
} from "app/views/sales/models/sale-order/sale-order-response-dto.model";
import { SaleOrderService } from "app/views/sales/services/sale-order.service";
import { ToastrService } from "ngx-toastr";

import { HttpClient, HttpHeaders } from "@angular/common/http";
import { MatButton } from "@angular/material/button";
import { MatDialog } from "@angular/material/dialog";
import { Inventory_Type_Id_Finished_Goods } from "app/shared/consts/const";
import { PaymentTerm } from "app/shared/enums/paymentTerm";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { SnackBarService } from "app/shared/services/snack-bar.service";
import { inFinancialYearValidator } from "app/shared/validators/in-financial-year-validator";
import { CustomerWiseProductDiscountByCustomerId } from "app/views/configuration/models/customer-wise-product-discount/customer-wise-product-discount-by-customer-id.model";
import { ActiveDiscountProductWiseByDateResponse } from "app/views/configuration/models/discount-product-wise/active-discount-product-wise-by-date-response.model";
import { DiscountProductWiseByDateRequest } from "app/views/configuration/models/discount-product-wise/discount-product-wise-by-date-request-model";
import { StoreRequest } from "app/views/configuration/models/store/store-request.model";
import { CustomerWiseProductDiscountService } from "app/views/configuration/services/customer-wise-product-discount.service";
import { DiscountProductWiseService } from "app/views/configuration/services/discount-product-wise.service";
import { ProductService } from "app/views/configuration/services/product.service";
import { StockService } from "app/views/report/services/stock.service";
import {
  SaleQuotationResponseDTO,
  SaleQuotationResponseDetail,
} from "app/views/sales/models/sale-quotation/sale-quotation-response-dto.model";
import { compareSalesOrderStockValidator } from "app/views/sales/validators/compare-sales-order-stock-validators";
import { environment } from "environments/environment";
import { finalize } from "rxjs";
import { MoneyReceiptListComponent } from "../money-receipt-list/money-receipt-list.component";
import { SalesQuotationListComponent } from "../sales-quotation-list/sales-quotation-list.component";

@Component({
  selector: "app-sales-order-form",
  templateUrl: "./sales-order-form.component.html",
  styleUrls: ["./sales-order-form.component.scss"],
})
export class SalesOrderFormComponent implements OnInit {
  @ViewChild("btnCheck") btnCheck: MatButton;
  @ViewChild("btnApprove") btnApprove: MatButton;
  @ViewChild("stepper") private stepper: MatStepper;
  isLoading: boolean = false;
  isDialogOpen: boolean = false;
  isViewMode: boolean = true;
  showMessage: boolean = false;
  isCreditBalanceInsufficient: boolean = false;
  creditBalanceInsufficientMessage: string;
  isChecked: boolean;
  formTitle: string;
  saleOrderForm: FormGroup;
  customers: Customer[];
  filterCustomers: Customer[];
  activeOfferDiscounts: ActiveDiscountProductWiseByDateResponse[];
  activeCustomerDiscounts: CustomerWiseProductDiscountByCustomerId[];
  stores: Store[];
  products: ProductView[];
  searchProducts: ProductView[];
  transports: ENUM[];
  paymentTerms: ENUM[];
  saleOrderDetailsData: any[] = [];
  data: SaleOrderResponseDTO;
  depoChargePerKg: number = 0;
  eTag: string;

  businessType: string;
  // Define financial year date range here
  financialYearStartDate: Date; //Example: Jul 1, 2023
  financialYearEndDate: Date; //Example: Jun 30, 2024
  currentFinancialYearId: string;
  applyMinMax: boolean = false;

  private path = {
    list: "/sales/sales-order",
    edit: "sales/sales-order",
  };

  constructor(
    public dialog: MatDialog,
    private fb: FormBuilder,
    private customerService: CustomerService,
    private storeService: StoreService,
    private stockService: StockService,
    private discountProductWiseService: DiscountProductWiseService,
    private enumValueService: EnumValueService,
    private productService: ProductService,
    private saleOrderService: SaleOrderService,
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
      this.data = response?.saleOrder?.data;
      this.eTag = response?.saleOrder?.versionNumber;
    });
    let isDataLoaded = false;
    this.jwtAuth.userProfile.subscribe((res: UserProfile) => {
      this.financialYearStartDate = new Date(res.fystartdate);
      this.financialYearEndDate = new Date(res.fyenddate);
      this.currentFinancialYearId = res.fyid;
      this.businessType = res.businesstype;
      if (!isDataLoaded) {
        this.getData(res.businesstype);
        isDataLoaded = true;
      }
    });
    this.initializeForm();
    this.subscribeToFormControlChanges("otherDiscount");
    this.subscribeToFormControlChanges("transportationCost");
  }

  private subscribeToFormControlChanges(controlName: string): void {
    this.saleOrderForm.get(controlName).valueChanges.subscribe((newValue) => {
      if (newValue === "" || newValue === null) {
        this.saleOrderForm
          .get(controlName)
          ?.patchValue(0, { emitEvent: false });
      }
    });
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

  getData(businessType: string) {
    this.getAllStores(businessType);
    this.getAllCustomers();
    this.getTransports();
    this.getPaymentTerms();
    //! it's important after close openSaleOrderDialog
    this.getAllProducts();
  }

  initializeForm() {
    this.createForm();
    this.populateForm();
    this.setFormTitle();
    this.saleOrderForm.get("orderDate").markAsTouched();
  }

  createForm(): void {
    this.saleOrderForm = this.fb.group({
      id: [this.data?.id ?? null],
      quotationNo: [this.data?.quotationNo ?? ""],
      referenceNo: [this.data?.referenceNo ?? ""],
      orderDate: [
        this.data?.orderDate ?? this.dateFormatService.getPresentDate(),
      ],
      deliveryDate: [
        this.data?.deliveryDate ?? this.dateFormatService.getPresentDate(),
        Validators.required,
      ],
      transport: [
        this.data?.transport ?? 0,
        this.businessType == "2" ? Validators.required : null,
      ],
      paymentTerm: [
        this.data?.paymentTerm ?? 0,
        this.businessType == "1" ? Validators.required : null,
      ],
      customerId: [this.data?.customerId, Validators.required],
      customerTerritoryId: [this.data?.customerTerritoryId ?? null],
      storeId: [this.data?.storeId, Validators.required],
      creditLimit: [this.data?.creditLimit, Validators.required],
      limitAvailed: [this.data?.limitAvailed, Validators.required],
      subtotal: [this.data?.subtotal ?? 0, Validators.required],
      discount: [this.data?.discount ?? 0],
      totalPercentageDiscountAmount: [
        this.data?.totalPercentageDiscountAmount ?? 0,
      ],
      offerDiscount: [this.data?.offerDiscount ?? 0],
      otherDiscount: [this.data?.otherDiscount ?? 0, Validators.min(0)],
      total: [this.data?.total ?? 0, Validators.required],
      transportationCost: [
        this.data?.transportationCost ?? 0,
        Validators.min(0),
      ],
      depoCharge: [this.data?.depoCharge ?? 0, Validators.min(0)],
      netTotal: [this.data?.netTotal ?? 0, Validators.min(0)],
      remark: [this.data?.remark ?? ""],
      moneyReceiptNo: [this.data?.moneyReceiptNo ?? ""],
      //TODO: If possible delete this code.
      customerWiseProductDiscountId: [null],
      discountProductWiseId: [null],
      deletedSaleOrderDetailIds: [""],
      saleOrderDetails: this.fb.array([]),
    });
    if (this.businessType === "2") {
      this.getActiveOfferDiscountByDate();
    }
    this.setMinMaxDates();
  }

  setMinMaxDates() {
    if (this.applyMinMax) {
      this.saleOrderForm
        .get("orderDate")
        .setValidators([
          Validators.required,
          inFinancialYearValidator(
            this.financialYearStartDate,
            this.financialYearEndDate
          ),
        ]);
    } else {
      this.saleOrderForm.get("orderDate").setValidators([Validators.required]);
    }
    this.saleOrderForm.get("orderDate").updateValueAndValidity();
  }

  get saleOrderDetails(): FormArray {
    return this.saleOrderForm.get("saleOrderDetails") as FormArray;
  }

  populateForm(): void {
    if (this.saleOrderForm.get("id").value) {
      if (this.businessType === "2") {
        this.getActiveCustomerDiscountByCustomerId(this.data?.customerId);
      }
      this.populateSaleOrderDetails(this.data);
      this.setCustomerBalanceAndCreditLimit(this.data?.customerId);
    } else {
      this.addItem();
    }
  }

  setFormTitle(): void {
    if (this.saleOrderForm.get("id").value) {
      this.formTitle = "Edit Sales Order";
    } else {
      this.formTitle = "Add Sales Order";
    }
  }

  populateSaleOrderDetails(data: SaleOrderResponseDTO): void {
    data.saleOrderDetails.forEach((item: SaleOrderResponseDetail) =>
      this.addItem(item)
    );
  }

  addItem(item?: SaleOrderResponseDetail): void {
    this.saleOrderDetails.push(this.createSaleOrderDetail(item));
  }

  createSaleOrderDetail(item?: SaleOrderResponseDetail): FormGroup {
    return this.fb.group(
      {
        id: [item?.id ?? null],
        productId: [item?.productId ?? "", Validators.required],
        product: [item?.product ?? ""],
        measurementUnitName: [item?.product?.measurementUnit?.name ?? ""],
        bagWeight: [item?.bagWeight, Validators.required],
        primaryQuantity: [
          item?.primaryQuantity ?? 0,
          [Validators.required, Validators.min(0)],
        ],
        primaryBonusQuantity: [
          item?.primaryBonusQuantity ?? 0,
          [Validators.required, Validators.min(0)],
        ],
        quantity: [item?.quantity, Validators.required],
        bonusQuantity: [item?.bonusQuantity ?? 0],
        rate: [item?.rate, Validators.required],
        vatPercentage: [item?.vatPercentage ?? 0],
        netRate: [item?.netRate ?? 0],
        discountPercentage: [item?.discountPercentage ?? 0],
        percentageDiscountAmount: [item?.percentageDiscountAmount ?? 0],
        discountAmount: [item?.discountAmount ?? 0],
        discountPerUnit: [item?.discountPerUnit ?? 0],
        offerDiscountPerUnit: [item?.offerDiscountPerUnit ?? 0],
        invoiceDiscountPerUnit: [item?.invoiceDiscountPerUnit ?? 0],
        cashDiscountPerUnit: [item?.cashDiscountPerUnit ?? 0],
        specialDiscountPerUnit: [item?.specialDiscountPerUnit ?? 0],
        currentStockQuantity: [item?.currentStockQuantity || 0],
        deliveredPrimaryQuantity: [item?.deliveredPrimaryQuantity || 0],
        deliveredPrimaryBonusQuantity: [
          item?.deliveredPrimaryBonusQuantity || 0,
        ],
        amount: [item?.amount, Validators.required],
        createdOn: [item?.createdOn || null],
        createdBy: [item?.createdBy || null],
      },
      {
        validators: [
          compareSalesOrderStockValidator("currentStockQuantity", "quantity"),
        ],
      }
    );
  }
  isPassingDataToNextStep = false;
  handleStepChange(event: any) {
    console.log(this.saleOrderForm);
    this.isPassingDataToNextStep = event.selectedIndex === 1;
    if (this.isPassingDataToNextStep) {
      this.saleOrderDetailsData =
        this.saleOrderForm.get("saleOrderDetails").value;
    }
  }

  getSaleOrderStatus(value) {
    return this.statusColorService.getSaleOrderStatus(value);
  }

  getSaleOrderStatusName(value: number) {
    return SaleOrderStatus[value];
  }

  getTransports() {
    return this.enumValueService.getTransports().subscribe((res) => {
      this.transports = res;
    });
  }

  getTransportName(value: number) {
    return Transport[value];
  }

  getPaymentTerms() {
    return this.enumValueService.getPaymentTerms().subscribe((res) => {
      this.paymentTerms = res;
    });
  }

  getPaymentTermName(value: number) {
    return PaymentTerm[value];
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    if (fieldName === "customerId") {
      this.saleOrderForm?.get("customerId").setValue(null);
    }
  }

  clearInputFromDetails(evt: any, itemIndex: number): void {
    evt.stopPropagation();
    const particularDetail = this.saleOrderDetails.at(itemIndex);
    particularDetail?.patchValue({
      productId: null,
    });
  }

  handleCustomerSearch(event: any): void {
    const name = event.target?.name;
    if (name === "customerId") {
      const term = this.saleOrderForm.get("customerId");
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

  getAllStores(businessType: string): void {
    let request = new StoreRequest();
    request.page = -1;
    if (businessType === "1") {
      // TODO: It should be changed to Finished Goods or Raw Material
      request.inventoryTypeId = Inventory_Type_Id_Finished_Goods;
      this.storeService.getStores(request).subscribe((res) => {
        this.stores = res?.data?.item1;
      });
    }
    if (businessType === "2") {
      request.inventoryTypeId = Inventory_Type_Id_Finished_Goods;
      this.storeService.getStores(request).subscribe((res) => {
        this.stores = res?.data?.item1;
        /**
         * This code is important to get depoChargePerKg in Edit Form
         */
        if (this.saleOrderForm.get("id")?.value) {
          this.depoChargePerKg = res?.data?.item1.find(
            (x) => x.id === this.data.storeId
          ).depoChargePerKg;
        }
      });
    }
  }

  getAllProducts(): void {
    let productRequest = new ProductRequest();
    productRequest.page = -1;
    //productRequest.inventoryTypeId = Inventory_Type_Id_Finished_Goods;
    productRequest.isSaleProduct = true;
    this.productService.getProducts(productRequest).subscribe((res) => {
      this.searchProducts = this.products = res?.data?.item1;
    });
  }

  getStoreName(storeId: string) {
    return this.stores?.find((x) => x.id === storeId).name;
  }

  getDepoChargePerKg(storeId: string) {
    this.depoChargePerKg = this.stores?.find(
      (x) => x.id === storeId
    ).depoChargePerKg;
  }

  /**
   * Execute when Store Select From Dropdown
   */
  onSelectedStore(storeId: string) {
    if (this.businessType === "2") {
      this.resetProductInSaleOrderDetails(this.saleOrderDetails);
      this.getDepoChargePerKg(storeId);
    }
  }

  onSelectedCustomer(customerId: string) {
    if (this.businessType === "2") {
      this.getActiveCustomerDiscountByCustomerId(customerId);
      this.resetProductInSaleOrderDetails(this.saleOrderDetails);
    }
    this.customerService
      .getCustomerCreditLimitBalance(customerId)
      .subscribe((res) => {
        this.saleOrderForm
          ?.get("creditLimit")
          ?.setValue(res?.data?.creditLimit);
        this.saleOrderForm?.get("limitAvailed")?.setValue(res?.data?.balance);
      });
    this.saleOrderForm
      .get("customerTerritoryId")
      .setValue(this.findCustomerById(customerId)?.customerTerritoryId ?? null);
  }

  findCustomerById(customerId: string) {
    return this.customers?.find((customer) => customer?.id === customerId);
  }

  setCustomerBalanceAndCreditLimit(customerId: string) {
    this.customerService
      .getCustomerCreditLimitBalance(customerId)
      .subscribe((res) => {
        this.saleOrderForm
          ?.get("creditLimit")
          ?.setValue(res?.data?.creditLimit);
        this.saleOrderForm?.get("limitAvailed")?.setValue(res?.data?.balance);
        this.calculateNetTotal();
      });
  }

  getActiveOfferDiscountByDate(): void {
    const discountProductWiseRequest = new DiscountProductWiseByDateRequest();
    discountProductWiseRequest.checkDate =
      this.saleOrderForm.get("orderDate")?.value;
    this.discountProductWiseService
      .getActiveDiscountProductWiseByDate(discountProductWiseRequest)
      .subscribe((res) => {
        this.activeOfferDiscounts = res.data;
        this.setOfferDiscountInSaleOrderDetails(this.saleOrderDetails);
      });
  }

  getActiveCustomerDiscountByCustomerId(customerId: string): void {
    this.customerWiseProductDiscountService
      .getCustomerWiseProductDiscountByCustomerId(customerId)
      .subscribe((res) => (this.activeCustomerDiscounts = res.data));
  }

  onOrderDateChange() {
    if (this.businessType === "2") {
      this.getActiveOfferDiscountByDate();
    }
  }

  setOfferDiscountInSaleOrderDetails(arr: FormArray) {
    for (let i = 0; i < arr.length; i++) {
      const saleOrderDetail = arr.at(i);
      //set offer discount
      if (this.activeOfferDiscounts?.length > 0) {
        this.activeOfferDiscounts.find((x) => {
          if (x.productId == saleOrderDetail.value.productId) {
            saleOrderDetail.patchValue({
              offerDiscountPerUnit: x?.discountAmountPerKg,
            });
            this.saleOrderForm.patchValue({
              discountProductWiseId: x?.discountProductWiseId,
            });
          }
        });
        this.calculateAmount(i);
      } else {
        saleOrderDetail.patchValue({
          offerDiscountPerUnit: 0,
        });
        this.saleOrderForm.patchValue({
          discountProductWiseId: null,
        });
        this.calculateAmount(i);
      }
    }
  }

  resetProductInSaleOrderDetails(arr: FormArray) {
    for (let i = 0; i <= arr.length; i++) {
      const saleOrderDetail = arr.at(i);
      saleOrderDetail?.patchValue({
        productId: null,
      });
    }
  }

  getProductName(productId: string) {
    const product =
      this.products?.find((product) => product?.id === productId) ||
      this.data?.saleOrderDetails?.find((x) => x?.productId == productId)
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

  //forkJoinLoading: boolean = false;
  // handleProductSelection(event: any, index: number) {
  //   const particularDetail = this.saleOrderDetails.at(index);
  //   const productId = event?.option?.value;
  //   const storeId = this.saleOrderForm.get("storeId")?.value;
  //   const customerId = this.saleOrderForm.get("customerId")?.value;
  //   const checkDate = this.saleOrderForm.value.orderDate;

  //   if (!storeId && !customerId && !checkDate) {
  //     this.showSnackBar("Please select a Store & a Customer and Order Date!");
  //     particularDetail.patchValue({
  //       productId: null,
  //     });
  //     return;
  //   }

  //   if (!storeId) {
  //     this.showSnackBar("Please select a Store!");
  //     particularDetail.patchValue({
  //       productId: null,
  //     });
  //     return;
  //   }

  //   if (!customerId) {
  //     this.showSnackBar("Please select a customer!");
  //     particularDetail.patchValue({
  //       productId: null,
  //     });
  //     return;
  //   }

  //   if (!checkDate) {
  //     this.showSnackBar("Please select a order date!");
  //     particularDetail.patchValue({
  //       productId: null,
  //     });
  //     return;
  //   }

  //   if (this.isExistSelectedProduct(productId, index)) {
  //     this.showSnackBar("This Product already added!");
  //     particularDetail.patchValue({
  //       productId: null,
  //     });
  //     return;
  //   }

  //   const selectedProduct = this.findProductById(productId);
  //   if (!selectedProduct) return;

  //   particularDetail.patchValue({
  //     productId: selectedProduct?.id,
  //     product: selectedProduct,
  //     measurementUnitName: selectedProduct?.measurementUnit?.name,
  //     bagWeight: selectedProduct.bagWeight,
  //     rate: selectedProduct.salePrice,
  //     bagQuantity: 0,
  //     quantity: 0,
  //     bonusBagQuantity: 0,
  //     bonusQuantity: 0,
  //   });

  //   /**
  //    * *ForkJoin
  //    */

  //   const discountProductWiseRequest =
  //     new DiscountProductWiseByProductIdRequest();
  //   discountProductWiseRequest.checkDate = checkDate;
  //   discountProductWiseRequest.productId = productId;

  //   const itemStock$ = this.stockService.getItemStock(productId, storeId);
  //   const discountProductWise$ =
  //     this.discountProductWiseService.getDiscountProductWiseByProductId(
  //       discountProductWiseRequest
  //     );
  //   const customerWiseProductDiscount$ =
  //     this.customerWiseProductDiscountService.getCustomerWiseProductDiscountByProductId(
  //       customerId,
  //       selectedProduct?.id
  //     );

  //   this.forkJoinLoading = true;
  //   forkJoin([
  //     itemStock$,
  //     discountProductWise$,
  //     customerWiseProductDiscount$,
  //   ]).subscribe(
  //     ([responseStockQuantity, offerDiscountRes, customerDiscountRes]) => {
  //       this.forkJoinLoading = false;

  //       particularDetail.patchValue({
  //         currentStockQuantity: responseStockQuantity,
  //         offerDiscountPerUnit: offerDiscountRes?.data?.discountAmountPerKg,
  //         discountPerUnit: parseFloat(
  //           (
  //             customerDiscountRes?.data?.invoiceDiscountPerUnit +
  //             customerDiscountRes?.data?.cashDiscountPerUnit +
  //             customerDiscountRes?.data?.specialDiscountPerUnit
  //           ).toFixed(2)
  //         ),
  //         invoiceDiscountPerUnit:
  //           customerDiscountRes?.data?.invoiceDiscountPerUnit,
  //         cashDiscountPerUnit: customerDiscountRes?.data?.cashDiscountPerUnit,
  //         specialDiscountPerUnit:
  //           customerDiscountRes?.data?.specialDiscountPerUnit,
  //       });

  //       this.saleOrderForm.patchValue({
  //         customerWiseProductDiscountId:
  //           customerDiscountRes?.data?.customerWiseProductDiscountId,
  //         discountProductWiseId: offerDiscountRes?.data?.discountProductWiseId,
  //       });
  //     }
  //   );

  //   this.calculateAmount(index);
  // }

  handleProductSelection(event: any, index: number) {
    const particularDetail = this.saleOrderDetails.at(index);
    const productId = event?.option?.value;
    const storeId = this.saleOrderForm.get("storeId")?.value;
    const customerId = this.saleOrderForm.get("customerId")?.value;
    const checkDate = this.saleOrderForm.value.orderDate;

    if (!storeId && !customerId && !checkDate) {
      this.showSnackBar("Please select a Store & a Customer and Order Date!");
      particularDetail.patchValue({
        productId: null,
      });
      return;
    }

    if (!storeId) {
      this.showSnackBar("Please select a Store!");
      particularDetail.patchValue({
        productId: null,
      });
      return;
    }

    if (!customerId) {
      this.showSnackBar("Please select a customer!");
      particularDetail.patchValue({
        productId: null,
      });
      return;
    }

    if (!checkDate) {
      this.showSnackBar("Please select a order date!");
      particularDetail.patchValue({
        productId: null,
      });
      return;
    }

    if (this.isExistSelectedProduct(productId, index)) {
      this.showSnackBar("This Product already added!");
      particularDetail.patchValue({
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
      vatPercentage: selectedProduct.vatPercentage,
      discountPercentage: 0,
      primaryQuantity: 0,
      quantity: 0,
      primaryBonusQuantity: 0,
      bonusQuantity: 0,
    });

    //set offer discount
    if (this.activeOfferDiscounts?.length > 0) {
      const offerDiscount = this.activeOfferDiscounts.find(
        (x) => x.productId == selectedProduct.id
      );

      if (offerDiscount) {
        particularDetail.patchValue({
          offerDiscountPerUnit: offerDiscount?.discountAmountPerKg,
        });
        this.saleOrderForm.patchValue({
          discountProductWiseId: offerDiscount?.discountProductWiseId,
        });
      } else {
        particularDetail.patchValue({
          offerDiscountPerUnit: 0,
        });
        this.saleOrderForm.patchValue({
          discountProductWiseId: null,
        });
      }
    } else {
      particularDetail.patchValue({
        offerDiscountPerUnit: 0,
      });
      this.saleOrderForm.patchValue({
        discountProductWiseId: null,
      });
    }

    // set customer discount
    if (this.activeCustomerDiscounts?.length > 0) {
      const discount = this.activeCustomerDiscounts.find(
        (x) => x.productId === selectedProduct.id
      );

      if (discount) {
        particularDetail.patchValue({
          discountPerUnit: parseFloat(
            (
              discount.invoiceDiscountPerUnit +
              discount.cashDiscountPerUnit +
              discount.specialDiscountPerUnit
            ).toFixed(2)
          ),
          invoiceDiscountPerUnit: discount.invoiceDiscountPerUnit,
          cashDiscountPerUnit: discount.cashDiscountPerUnit,
          specialDiscountPerUnit: discount.specialDiscountPerUnit,
        });

        this.saleOrderForm.patchValue({
          customerWiseProductDiscountId: discount.customerWiseProductDiscountId,
        });
      } else {
        // if product does not have a discount
        particularDetail.patchValue({
          discountPerUnit: 0,
          invoiceDiscountPerUnit: 0,
          cashDiscountPerUnit: 0,
          specialDiscountPerUnit: 0,
        });
        this.saleOrderForm.patchValue({
          customerWiseProductDiscountId: null,
        });
      }
    } else {
      // if discount product wise list does not have a discount
      particularDetail.patchValue({
        discountPerUnit: 0,
        invoiceDiscountPerUnit: 0,
        cashDiscountPerUnit: 0,
        specialDiscountPerUnit: 0,
      });
      this.saleOrderForm.patchValue({
        customerWiseProductDiscountId: null,
      });
    }

    this.stockService.getItemStock(productId, storeId).subscribe((res) =>
      particularDetail.patchValue({
        currentStockQuantity: res,
      })
    );

    this.calculateAmount(index);
  }

  showSnackBar(message: string) {
    this.snackBarService.openSnackbar(message);
  }

  isExistSelectedProduct(productId: string, index: number): boolean {
    const isProductAdded = this.saleOrderDetails.value.some((item, i) => {
      return item.productId === productId && i !== index;
    });
    return isProductAdded;
  }

  handleProductSearch(event: any, itemIndex: number) {
    const name = event?.target?.name;
    if (name === "productId") {
      const term = this.saleOrderDetails.at(itemIndex).get("productId");
      this.filterProduct(term.value || "");
    }
  }

  filterProduct(searchTerm: string) {
    searchTerm = searchTerm.toLowerCase();
    this.searchProducts = this.products.filter((product) =>
      product.name.toLowerCase().startsWith(searchTerm)
    );
  }

  calculateCost() {
    const calculateTotal = (property) => {
      return this.saleOrderDetails?.value?.reduce(
        (sum, item) => sum + item?.quantity * item?.[property],
        0
      );
    };

    const depoCharge = this.saleOrderDetails?.value?.reduce(
      (sum, item) => sum + item?.quantity * this.depoChargePerKg,
      0
    );

    const totalPercentageDiscountAmount = this.saleOrderDetails?.value?.reduce(
      (sum, item) => sum + item?.percentageDiscountAmount,
      0
    );

    const subtotal = calculateTotal("rate");
    const discount = calculateTotal("discountPerUnit");
    const offerDiscount = calculateTotal("offerDiscountPerUnit");

    this.saleOrderForm?.get("subtotal")?.setValue(subtotal);
    this.saleOrderForm
      ?.get("totalPercentageDiscountAmount")
      ?.setValue(totalPercentageDiscountAmount);
    this.saleOrderForm?.get("discount")?.setValue(discount);
    this.saleOrderForm?.get("offerDiscount")?.setValue(offerDiscount);
    this.saleOrderForm
      ?.get("total")
      ?.setValue(
        subtotal - totalPercentageDiscountAmount - discount - offerDiscount
      );
    this.saleOrderForm?.get("depoCharge")?.setValue(depoCharge);

    this.calculateNetTotal();
  }

  onControlChange(event: any, itemIndex?: number): void {
    const name = event?.target?.name;
    if (name === "primaryQuantity" || name === "discountPercentage") {
      this.calculateAmount(itemIndex);
      //this.calculateDepoCharge(itemIndex);
    }
    if (name === "primaryBonusQuantity") {
      this.calculateBonusQty(itemIndex);
    }
    if (
      name === "otherDiscount" ||
      name === "transportationCost" ||
      name === "depoCharge"
    ) {
      this.calculateNetTotal();
    }
  }

  calculateBonusQty(itemIndex: number) {
    const item = this.saleOrderDetails?.at(itemIndex);
    const bagWeight = item?.get("bagWeight")?.value;
    const primaryBonusQuantity = item?.get("primaryBonusQuantity")?.value;
    const bonusQuantity = item?.get("bonusQuantity");
    bonusQuantity?.setValue(bagWeight * primaryBonusQuantity);
  }

  calculateNetTotal() {
    const subtotal = this.saleOrderForm?.get("subtotal")?.value;
    const totalPercentageDiscountAmount = this.saleOrderForm?.get(
      "totalPercentageDiscountAmount"
    )?.value;
    const discount = this.saleOrderForm?.get("discount")?.value;
    const offerDiscount = this.saleOrderForm?.get("offerDiscount")?.value;
    const otherDiscount = this.saleOrderForm?.get("otherDiscount")?.value;
    const total = this.saleOrderForm?.get("total");
    const transportationCost =
      this.saleOrderForm?.get("transportationCost")?.value;
    const depoCharge = this.saleOrderForm?.get("depoCharge")?.value;
    total?.setValue(
      subtotal -
        (totalPercentageDiscountAmount +
          discount +
          otherDiscount +
          offerDiscount)
    );
    this.saleOrderForm
      ?.get("netTotal")
      ?.setValue(
        subtotal -
          (totalPercentageDiscountAmount +
            discount +
            otherDiscount +
            offerDiscount) +
          transportationCost +
          depoCharge
      );
    const netTotal = this.saleOrderForm?.get("netTotal")?.value;
    const creditLimit = this.saleOrderForm?.get("creditLimit")?.value;
    const limitAvailed = this.saleOrderForm?.get("limitAvailed")?.value;

    // Calculate the available credit after deducting the limit availed
    const availableCredit = creditLimit - limitAvailed;

    // Check if netTotal exceeds available credit
    this.isCreditBalanceInsufficient =
      netTotal > 0 && netTotal > availableCredit;

    // Set the message if credit balance is insufficient
    if (this.isCreditBalanceInsufficient) {
      this.creditBalanceInsufficientMessage = `Insufficient Credit Balance. Total Payable cannot exceed available Credit. \n Customer has available Credit: ${availableCredit.toFixed(
        2
      )} \n Total Payable: ${netTotal.toFixed(2)}`;
    }
  }

  calculateAmount(itemIndex: number) {
    const item = this.saleOrderDetails?.at(itemIndex);
    const bagWeight = item?.get("bagWeight")?.value;
    const primaryQuantity = item?.get("primaryQuantity")?.value;
    const quantity = item?.get("quantity");
    const discountPerUnit = item?.get("discountPerUnit")?.value;
    const offerDiscountPerUnit = item?.get("offerDiscountPerUnit")?.value;
    const discountAmount = item?.get("discountAmount");
    const vatPercentage = item?.get("vatPercentage")?.value;
    const discountPercentage = item?.get("discountPercentage")?.value;
    const percentageDiscountAmount = item?.get("percentageDiscountAmount");
    const netRate = item?.get("netRate");
    quantity?.setValue(bagWeight * primaryQuantity);
    const ratePerUnit = item?.get("rate")?.value;
    const amount = item?.get("amount");
    discountAmount.setValue(quantity.value * discountPerUnit);
    netRate.setValue(
      parseFloat(
        (ratePerUnit - discountPerUnit - offerDiscountPerUnit).toFixed(2)
      )
    );
    amount?.setValue(parseFloat((quantity.value * ratePerUnit).toFixed(2)));
    percentageDiscountAmount.setValue(
      parseFloat(
        ((amount.value * discountPercentage) / (100 + vatPercentage)).toFixed(2)
      )
    );
    this.saleOrderForm.get("discount").setValue(0);
    this.saleOrderForm.get("totalPercentageDiscountAmount").setValue(0);
    this.saleOrderForm.get("offerDiscount").setValue(0);
    /**
     * ! If We go for view or edit mode, other discount is being 0. As a result, the value of netTotal
     * ! (Total Payable) is being increased.
     * TODO: carefully otherDiscount 0 korte hobe.
     */
    //this.saleOrderForm.get("otherDiscount").setValue(0);
    this.calculateCost();
  }

  isTransportSelected(): boolean {
    const transportControl = this.saleOrderForm.get("transport").value;
    return transportControl === 1;
  }

  private handleSuccessfulSave(res: GeneralResponse<string>): void {
    if (res?.succeeded) {
      this.handleSaleOrderResponse(res?.data);
      this.toastr.success(res?.message);
      this.deletedIds = "";
    } else {
      this.isLoading = false;
      this.toastr.error(res?.message);
    }
  }

  private handleSaleOrderResponse(saleOrderId: string): void {
    this.saleOrderService.getSaleOrderById(saleOrderId).subscribe({
      next: (saleOrderResponse) => {
        this.data = saleOrderResponse.data;
        this.isViewMode = true;
        this.isLoading = false;
        this.navigateToView(saleOrderResponse?.data?.id);
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
    //location.reload();
  }

  private navigateToList() {
    this.router.navigate([this.path.list]);
  }

  backToView() {
    this.initializeForm();
    this.stepper.next();
    this.isViewMode = true;
  }

  private addSaleOrder(body: SaleOrderRequestDTO): void {
    this.saleOrderService
      .createSaleOrder(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  private updateSaleOrder(body: SaleOrderRequestDTO): void {
    const headers = new HttpHeaders().set("If-None-Match", this.eTag);
    this.saleOrderService
      .updateSaleOrder(body, headers)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  deletedIds: string = "";
  onDeleteItem(id: string, itemIndex: number): void {
    if (id) this.deletedIds += `${id},`;
    this.saleOrderDetails.removeAt(itemIndex);
    this.calculateCost();
  }

  onSubmit(): void {
    if (this.saleOrderForm.valid) {
      this.isLoading = true;
      const formValue = this.saleOrderForm.value;
      if (!formValue.id) {
        this.addSaleOrder(formValue);
      } else {
        formValue.deletedSaleOrderDetailIds = this.deletedIds;
        this.updateSaleOrder(formValue);
      }
    }
  }

  check(id: string) {
    if (this.isLoading) return;
    this.isLoading = true;
    
    this.saleOrderService
      .checkSaleOrder(id)
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

    this.saleOrderService
      .approveSaleOrder(id)
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

    this.saleOrderService
      .unpostSaleOrder(id, status)
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

  // openSalesQuotationDialog
  openSalesQuotationDialog() {
    const dialogRef = this.dialog.open(SalesQuotationListComponent, {
      disableClose: true,
      panelClass: "add-bill-container",
      minHeight: "auto",
      height: "auto",
    });
    dialogRef.afterClosed().subscribe((result: SaleQuotationResponseDTO) => {
      if (!result) {
        return;
      }
      this.saleOrderForm.reset();
      this.saleOrderForm.setControl("saleOrderDetails", this.fb.array([]));
      this.saleOrderForm.markAllAsTouched();
      const formData = {
        quotationNo: result?.quotationNo,
        referenceNo: result?.referenceNo,
        orderDate: this.dateFormatService?.getPresentDate(),
        deliveryDate: this.dateFormatService?.getPresentDate(),
        transport: 0,
        paymentTerm: 0,
        customerId: result?.customerId,
        customerTerritoryId:
          this.findCustomerById(result?.customerId)?.customerTerritoryId ??
          null,
        storeId: "",
        subtotal: result?.subtotal,
        discount: result?.discount,
        totalPercentageDiscountAmount: 0,
        offerDiscount: 0,
        specialDiscount: 0,
        total: result?.total,
        transportationCost: 0,
        depoCharge: 0,
        netTotal: result?.total,
        remark: result?.remark,
      };
      this.saleOrderForm.patchValue(formData);

      const customerId = this.saleOrderForm.get("customerId").value;
      if (customerId !== "" || customerId !== undefined) {
        this.onSelectedCustomer(customerId);
      }

      result?.saleQuotationDetails.forEach(
        (item: SaleQuotationResponseDetail) => {
          const saleOrderResponseDetail: SaleOrderResponseDetail = {
            productId: item?.productId,
            product: item?.product,
            bagWeight: item?.bagWeight,
            primaryQuantity: item?.primaryQuantity,
            primaryBonusQuantity: 0,
            quantity: item?.quantity,
            bonusQuantity: 0,
            rate: item?.rate,
            vatPercentage: item?.product?.vatPercentage,
            discountPercentage: 0,
            percentageDiscountAmount: 0,
            netRate: item?.rate,
            discountAmount: item?.discountAmount,
            offerDiscountPerUnit: 0,
            discountPerUnit: item?.discountPerUnit,
            invoiceDiscountPerUnit: 0,
            cashDiscountPerUnit: 0,
            specialDiscountPerUnit: 0,
            amount: item?.amount,
            otherDiscountPerUnit: 0,
            transportationCostPerUnit: 0,
            depoChargePerUnit: 0,
          };
          this.addItem(saleOrderResponseDetail);
        }
      );
    });
  }

  openMoneyReceiptDialog() {
    if (!this.saleOrderForm.get("customerId").value) {
      this.showSnackBar("Please select a customer!");
      return;
    }
    const dialogRef = this.dialog.open(MoneyReceiptListComponent, {
      disableClose: true,
      panelClass: "add-bill-container",
      minHeight: "auto",
      height: "auto",
      data: this.saleOrderForm.get("customerId")?.value,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        let commaSeparatedMoneyReceiptNoNo = result
          .map((receipt) => receipt.code)
          .join(",");
        let resultCost = result.reduce(
          (prevVal, currVal) => {
            return {
              totalAmount: prevVal.totalAmount + currVal.totalAmount,
            };
          },
          {
            totalAmount: 0,
          }
        );
        this.saleOrderForm.patchValue({
          moneyReceiptNo: commaSeparatedMoneyReceiptNoNo,
        });
      }
    });
  }

  printPdf(id) {
    this.http
      .get(environment.apiURL + "/ReportSaleOrder/" + id, {
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
