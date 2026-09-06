import { HttpClient } from "@angular/common/http";
import {
  AfterViewInit,
  ChangeDetectorRef,
  Component,
  OnInit,
  ViewChild,
} from "@angular/core";
import { FormArray, FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MatButton } from "@angular/material/button";
import { MatDialog } from "@angular/material/dialog";
import { MatStepper } from "@angular/material/stepper";
import { ActivatedRoute, Router } from "@angular/router";
import {
  Inventory_Type_Id_Raw_Materials,
  Payment_Mode_Credit,
} from "app/shared/consts/const";
import { ImportPurchaseIncoTerm } from "app/shared/enums/importPurchaseIncoTerm";
import { ImportPurchasePaymentTerm } from "app/shared/enums/importPurchasePaymentTerm";
import { PaymentMode } from "app/shared/enums/paymentMode";
import { PurchaseOrderStatus } from "app/shared/enums/purchaseOrderStatus";
import { Transport } from "app/shared/enums/transport";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { ConfirmReadyForGrnDialogModel } from "app/shared/models/confirm-ready-for-grn-dialog.model";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { ConfirmReadyForGrnDialogService } from "app/shared/services/confirm-ready-for-grn-dialog.service";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { SnackBarService } from "app/shared/services/snack-bar.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { inFinancialYearValidator } from "app/shared/validators/in-financial-year-validator";
import { Currency } from "app/views/configuration/models/currency/currency.model";
import { DeliveryPlace } from "app/views/configuration/models/delivery-place/delivery-place.model";
import { PaymentMethod } from "app/views/configuration/models/payment-method/payment-method.model";
import { ProductRequest } from "app/views/configuration/models/product/product-request.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { StoreRequest } from "app/views/configuration/models/store/store-request.model";
import { Store } from "app/views/configuration/models/store/store.model";
import { Supplier } from "app/views/configuration/models/supplier/supplier.model";
import { CurrencyService } from "app/views/configuration/services/currency.service";
import { DeliveryPlaceService } from "app/views/configuration/services/delivery-place.service";
import { PaymentMethodService } from "app/views/configuration/services/payment-method.service";
import { ProductService } from "app/views/configuration/services/product.service";
import { StoreService } from "app/views/configuration/services/store.service";
import { SupplierService } from "app/views/configuration/services/supplier.service";
import {
  PurchaseOrderResponseDTO,
  PurchaseOrderResponseDetail,
} from "app/views/purchase/models/purchase-order/purchase-order-response-dto.model";
import { PurchaseRequisitionResponseDetail } from "app/views/purchase/models/purchase-requisition/purchase-requisition-response-dto.model";
import { VendorQuotationResponseDetail } from "app/views/purchase/models/vendor-quotation/vendor-quotation-response-dto.model";
import { PoCloseConfirmDialogService } from "app/views/purchase/services/po-close-confirm-dialog.service";
import { PurchaseOrderService } from "app/views/purchase/services/purchase-order.service";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { finalize } from "rxjs";
import { PurchaseRequisitionListComponent } from "../../vendor-quotation/purchase-requisition-list/purchase-requisition-list.component";
import { VendorQuotationListComponent } from "../vendor-quotation-list/vendor-quotation-list.component";

@Component({
  selector: "app-purchase-order-form",
  templateUrl: "./purchase-order-form.component.html",
  styleUrls: ["./purchase-order-form.component.scss"],
})
export class PurchaseOrderFormComponent implements OnInit, AfterViewInit {
  @ViewChild("btnCheck") btnCheck: MatButton;
  @ViewChild("btnApprove") btnApprove: MatButton;
  @ViewChild("stepper") private stepper: MatStepper;
  isLoading: boolean = false;
  isDialogOpen: boolean = false;
  isViewMode: boolean = true;
  isChecked: boolean;
  formTitle: string;
  purchaseOrderForm: FormGroup;
  stores: Store[];
  deliveryPlaces: DeliveryPlace[];
  paymentMethods: PaymentMethod[];
  suppliers: Supplier[];
  filterSuppliers: Supplier[];
  transports: ENUM[];
  paymentModes: ENUM[];
  importPurchaseIncoTerms: ENUM[];
  importPurchasePaymentTerms: ENUM[];
  currencies: Currency[];
  products: ProductView[];
  searchProducts: ProductView[];
  purchaseOrderDetailsData: any[] = [];

  data: PurchaseOrderResponseDTO;

  businessType: string;
  // Define financial year date range here
  financialYearStartDate: Date; // Jul 1, 2023
  financialYearEndDate: Date; // Jun 30, 2024
  currentFinancialYearId: string;
  applyMinMax: boolean = false;

  private path = {
    list: "purchase/purchase-order",
    edit: "purchase/purchase-order",
  };

  constructor(
    public dateFormatService: DateTimeFormatService,
    private jwtAuth: JwtAuthService,
    public dialog: MatDialog,
    private purchaseOrderService: PurchaseOrderService,
    private storeService: StoreService,
    private deliveryPlaceService: DeliveryPlaceService,
    private paymentMethodService: PaymentMethodService,
    private enumValueService: EnumValueService,
    private currencyService: CurrencyService,
    private supplierService: SupplierService,
    private productService: ProductService,
    private fb: FormBuilder,
    private toastr: ToastrService,
    private http: HttpClient,
    private activatedRoute: ActivatedRoute,
    private router: Router,
    // private localStorageService: LocalStoreService,
    private cdRef: ChangeDetectorRef,
    public statusColorService: StatusColorService,
    private snackBarService: SnackBarService,
    private confirmDialogService: ConfirmDialogService,
    private confirmReadyForGrnDialogService: ConfirmReadyForGrnDialogService,
    private poCloseConfirmDialogService: PoCloseConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.activatedRoute.data.subscribe((response: any) => {
      this.data = response?.purchaseOrder?.data;
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
  }

  ngAfterViewInit() {
    let id = this.activatedRoute.snapshot.paramMap.get("id");
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
    this.getAllDeliveryPlaces();
    this.getAllPaymentMethods();
    this.getAllSuppliers();
    this.getAllTransports();
    this.getAllPaymentModes();
    this.getAllCurrencies();
    this.getAllImportPurchaseIncoTerms();
    this.getAllImportPurchasePaymentTerms();
    //! it's important after close openPurchaseRequisitionDialog and openVendorQuotationDialog
    this.getAllProducts();
  }

  initializeForm() {
    this.createForm();
    this.populateForm();
    this.setFormTitle();
    this.purchaseOrderForm.get("podate").markAsTouched();
  }

  createForm() {
    this.purchaseOrderForm = this.fb.group({
      id: [this?.data?.id || null],
      quotationNo: [this.data?.quotationNo || ""],
      requisitionNo: [this.data?.requisitionNo || ""],
      referenceNo: [this.data?.referenceNo || ""],
      podate: [this.data?.podate || this.dateFormatService.getPresentDate()],
      storeId: [this.data?.storeId ?? null, Validators.required],
      supplierId: [this.data?.supplierId ?? null, Validators.required],
      deliveryPlaceId: [
        this.data?.deliveryPlaceId || null,
        Validators.required,
      ],
      productOrigin: [this.data?.productOrigin || ""],
      packagingType: [this.data?.packagingType || ""],
      expiryTime: [this.data?.expiryTime || ""],
      paymentMethodId: [
        this.data?.paymentMethodId ?? null,
        [this.customRequiredValidator("paymentMethodId")],
      ],
      transport: [
        this.data?.transport || 0,
        [this.customRequiredValidator("transport")],
      ],
      paymentTermInDays: [
        this.data?.paymentTermInDays || 0,
        Validators.required,
      ],
      deliveryTermInDays: [
        this.data?.deliveryTermInDays || 0,
        Validators.required,
      ],
      deliveryDate: [
        this.data?.deliveryDate || this.dateFormatService.getPresentDate(),
        Validators.required,
      ],
      paymentMode: [
        this.data?.paymentMode || 0,
        [this.customRequiredValidator("paymentMode")],
      ],
      subtotal: [this.data?.subtotal || 0],
      isImportPurchase: [this.data?.isImportPurchase || false],
      proformaInvoiceNo: [this.data?.proformaInvoiceNo || ""],
      lcNumber: [
        this.data?.lcNumber || "",
        [this.customRequiredValidator("lcNumber")],
      ],
      exchangeRate: [
        this.data?.exchangeRate || 0,
        [this.customRequiredValidator("exchangeRate")],
      ],
      currencyId: [
        this.data?.currencyId || null,
        [this.customRequiredValidator("currencyId")],
      ],
      importPurchaseIncoTerm: [this.data?.importPurchaseIncoTerm || 0],
      importPurchasePaymentTerm: [this.data?.importPurchasePaymentTerm || 0],
      portOfLoading: [this.data?.portOfLoading || ""],
      portOfDestination: [this.data?.portOfDestination || ""],
      termAndCondition: [this.data?.termAndCondition || ""],
      discount: [this.data?.discount || 0, Validators.required],
      total: [this.data?.total || 0],
      weightVariance: [this.data?.weightVariance || 0],
      remark: [this.data?.remark || ""],
      isPartialDelivery: [this.data?.isPartialDelivery || false],
      deletedPurchaseOrderDetailIds: [""],
      purchaseOrderDetails: this.fb.array([]),
    });

    this.setMinMaxDates();
  }

  setMinMaxDates() {
    if (this.applyMinMax) {
      this.purchaseOrderForm
        .get("podate")
        .setValidators([
          Validators.required,
          inFinancialYearValidator(
            this.financialYearStartDate,
            this.financialYearEndDate
          ),
        ]);
    } else {
      this.purchaseOrderForm.get("podate").setValidators([Validators.required]);
    }

    this.purchaseOrderForm.get("podate").updateValueAndValidity();
  }

  updateValidators() {
    this.purchaseOrderForm.get("lcNumber").updateValueAndValidity();
    this.purchaseOrderForm.get("exchangeRate").updateValueAndValidity();
    this.purchaseOrderForm.get("currencyId").updateValueAndValidity();
    this.purchaseOrderForm.get("paymentMode").updateValueAndValidity();
    this.purchaseOrderForm.get("transport").updateValueAndValidity();
    this.purchaseOrderForm.get("paymentMethodId").updateValueAndValidity();
  }

  // Common custom validator function
  customRequiredValidator(field: string) {
    return (control) => {
      const value = control.value;
      const isImportPurchase = this.purchaseOrderForm?.value?.isImportPurchase;

      if (isImportPurchase) {
        if (field === "lcNumber" && (value === undefined || value === "")) {
          return { lcNumberRequired: true };
        }
        if (
          field === "currencyId" &&
          (value === undefined || value === "" || value === null)
        ) {
          return { currencyIdRequired: true };
        }
        if (field === "exchangeRate" && value <= 0) {
          return { exchangeRateRequired: true };
        }
      }
      if (!isImportPurchase) {
        if (field === "paymentMode" && value == 0) {
          return { paymentModeRequired: true };
        }
        if (field === "transport" && value == 0) {
          return { transportRequired: true };
        }
        if (
          field === "paymentMethodId" &&
          (value === undefined || value === "" || value === null)
        ) {
          return { paymentMethodIdRequired: true };
        }
      }
      return null;
    };
  }

  get purchaseOrderDetails(): FormArray {
    return <FormArray>this.purchaseOrderForm.get("purchaseOrderDetails");
  }

  previousSupplierId: string = "";
  populateForm(): void {
    if (this.purchaseOrderForm.get("id").value) {
      this.previousSupplierId = this.data.supplierId;
      this.populatePurchaseOrderDetails(this.data);
    } else {
      this.addItem();
    }
  }

  setFormTitle() {
    if (this.purchaseOrderForm.get("id").value) {
      this.formTitle = "Edit Purchase Order";
    } else {
      this.formTitle = "Add Purchase Order";
    }
  }

  populatePurchaseOrderDetails(data: PurchaseOrderResponseDTO) {
    data.purchaseOrderDetails.forEach((item: PurchaseOrderResponseDetail) =>
      this.addItem(item)
    );
  }

  addItem(item?: PurchaseOrderResponseDetail): void {
    this.purchaseOrderDetails.push(this.createPurchaseOrderDetail(item));
  }

  createPurchaseOrderDetail(item?: PurchaseOrderResponseDetail): FormGroup {
    return this.fb.group({
      id: [item?.id || null],
      productId: [item?.productId || null, Validators.required],
      product: [item?.product || null],
      lastPoDetails: [item?.lastPoDetails || ""],
      poId: [item?.poId || null],
      lastPoStatus: [item?.lastPoStatus || 0],
      measurementUnitName: [item?.product?.measurementUnit?.name || ""],
      quantity: [item?.quantity || 0, Validators.required],
      receivedQuantity: [item?.receivedQuantity || 0],
      currencyRate: [item?.currencyRate ? item?.currencyRate : 0],
      rate: [item?.rate ? item?.rate : 0, Validators.required],
      currencyAmount: [item?.currencyAmount || 0],
      amount: [item?.amount || 0],
    });
  }

  hiddenStatuses: number[] = [1, 4, 5, 6, 7];

  getImportPurchaseValue(): boolean {
    return this.purchaseOrderForm?.get("isImportPurchase")?.value || false;
  }

  // Method to check if the current status should hide the button
  shouldHideUnpostButton(status?: number): boolean {
    return status == undefined || this.hiddenStatuses.includes(status);
  }

  getAllStores(businessType: string): void {
    let storeRequest = new StoreRequest();
    storeRequest.page = -1;
    if (businessType === "1") {
      this.storeService.getStores(storeRequest).subscribe((res) => {
        this.stores = res?.data?.item1;
      });
    }
    if (businessType === "2") {
      storeRequest.inventoryTypeId = Inventory_Type_Id_Raw_Materials;
      this.storeService.getStores(storeRequest).subscribe((res) => {
        this.stores = res?.data?.item1;
      });
    }
  }

  getAllDeliveryPlaces(): void {
    this.deliveryPlaceService.getAllDeliveryPlaces().subscribe((res) => {
      this.deliveryPlaces = res?.data?.item1;
    });
  }

  getAllPaymentMethods(): void {
    this.paymentMethodService.getAllPaymentMethods().subscribe((res) => {
      this.paymentMethods = res?.data?.item1;
    });
  }

  onSelectedSupplier(supplierId) {
    if (
      this.previousSupplierId != "" &&
      this.previousSupplierId != supplierId
    ) {
      this.previousSupplierId = supplierId;
      this.resetPurchaseOrderDetails(this.purchaseOrderDetails);
    } else {
      this.previousSupplierId = supplierId;
    }
  }

  resetPurchaseOrderDetails(arr: FormArray) {
    for (let i = 0; i <= arr.length; i++) {
      const purchaseOrderDetail = arr.at(i);
      purchaseOrderDetail?.patchValue({
        productId: null,
        lastPoDetails: "",
        poId: null,
        lastPoStatus: 0,
      });
    }
  }

  handleSupplierSearch(event: any): void {
    const name = event.target?.name;
    if (name === "supplierId") {
      const term = this.purchaseOrderForm.get("supplierId");
      this.filterSupplier(term.value || "");
    }
  }

  private filterSupplier(value: string) {
    const filterValue = value.toLowerCase();
    this.filterSuppliers = this.suppliers?.filter((option) =>
      option.name.toLowerCase().includes(filterValue)
    );
  }

  getSupplierName(supplierId: string) {
    if (!supplierId) {
      return;
    }
    const supplierAccount =
      this.suppliers?.find((supplier) => supplier?.id === supplierId) ||
      this.data?.supplier;
    return supplierAccount?.name;
  }

  getAllSuppliers(): void {
    this.supplierService.getAllSuppliers().subscribe((res) => {
      this.filterSuppliers = this.suppliers = res?.data?.item1;
    });
  }

  getAllTransports(): void {
    this.enumValueService.getTransports().subscribe((res) => {
      this.transports = res;
    });
  }

  getAllPaymentModes(): void {
    this.enumValueService.getPaymentModes().subscribe((res) => {
      this.paymentModes = res;
    });
  }

  getAllImportPurchaseIncoTerms(): void {
    this.enumValueService.getImportPurchaseIncoTerms().subscribe((res) => {
      this.importPurchaseIncoTerms = res;
    });
  }

  getAllImportPurchasePaymentTerms(): void {
    this.enumValueService.getImportPurchasePaymentTerms().subscribe((res) => {
      this.importPurchasePaymentTerms = res;
    });
  }

  getAllCurrencies(): void {
    this.currencyService.getAllCurrencies().subscribe((res) => {
      this.currencies = res?.data?.item1;
    });
  }

  getAllProducts(): void {
    let productRequest = new ProductRequest();
    // productRequest.inventoryTypeIds = [
    //   Inventory_Type_Id_Raw_Materials,
    //   Inventory_Type_Id_Other_Items,
    // ];
    productRequest.isPurchaseProduct = true;
    productRequest.page = -1;

    this.productService.getProducts(productRequest).subscribe((res) => {
      this.searchProducts = this.products = res?.data?.item1;
    });
  }

  getProductsByInventoryType(inventoryTypeId: string): void {
    let productRequest = new ProductRequest();
    productRequest.inventoryTypeIds = [inventoryTypeId];
    productRequest.isPurchaseProduct = true;
    productRequest.page = -1;

    this.productService.getProducts(productRequest).subscribe((res) => {
      this.searchProducts = this.products = res?.data?.item1;
    });
  }

  onSelectedStore(storeId) {
    this.resetProductInSaleOrderDetails(this.purchaseOrderDetails);
    this.getProductsByInventoryType(
      this.stores.find((x) => x.id === storeId).inventoryTypeId
    );
  }

  resetProductInSaleOrderDetails(arr: FormArray) {
    for (let i = 0; i <= arr.length; i++) {
      const purchaseOrderDetail = arr.at(i);
      purchaseOrderDetail?.patchValue({
        productId: null,
      });
    }
  }

  getCurrencyName(currencyId) {
    return this.currencies?.find((x) => x?.id === currencyId)?.name;
  }

  getImportPurchaseIncoTermName(value) {
    return ImportPurchaseIncoTerm[value];
  }

  getImportPurchasePaymentTermName(value) {
    return ImportPurchasePaymentTerm[value];
  }

  getStoreName(storeId) {
    return this.stores?.find((x) => x?.id === storeId)?.name;
  }

  getDeliveryPlaceName(deliveryPlaceId) {
    return this.deliveryPlaces?.find((x) => x?.id === deliveryPlaceId)?.name;
  }

  getPaymentMethodName(paymentMethodId) {
    return this.paymentMethods?.find((x) => x?.id === paymentMethodId)?.name;
  }

  getTransportName(value) {
    return Transport[value];
  }

  getPaymentModeName(value) {
    return PaymentMode[value];
  }

  isPaymentModeCredit() {
    return (
      this.purchaseOrderForm.get("paymentMode").value === Payment_Mode_Credit
    );
  }

  getProductName(productId: string) {
    if (!productId) {
      return;
    }

    const product =
      this.products?.find((product) => product?.id === productId) ||
      this.data?.purchaseOrderDetails?.find((x) => x?.productId == productId)
        ?.product;
    if (!product) return "";
    return (
      product?.name +
      (this.businessType === "2" ? "" : ` (${product?.packSize?.name})`)
    );
  }

  isPassingDataToNextStep = false;
  handleStepChange(event) {
    this.isPassingDataToNextStep = event.selectedIndex === 1;
    if (this.isPassingDataToNextStep) {
      this.purchaseOrderDetailsData = this.purchaseOrderForm.get(
        "purchaseOrderDetails"
      )?.value;
    }
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    if (fieldName === "supplierId") {
      this.purchaseOrderForm?.get("supplierId").setValue(null);
    }
  }

  clearInputFromDetails(evt: any, itemIndex: number): void {
    evt.stopPropagation();
    const particularDetail = this.purchaseOrderDetails.at(itemIndex);
    particularDetail?.patchValue({
      productId: null,
    });
  }

  handleProductSearch(event: any, itemIndex: number): void {
    const name = event?.target?.name;
    if (name === "productId") {
      const term = this.purchaseOrderDetails.at(itemIndex).get("productId");
      this.filterProduct(term?.value || "");
    }
  }

  filterProduct(searchTerm: string) {
    searchTerm = searchTerm.toLowerCase();
    this.searchProducts = this.products?.filter((product) =>
      product.name.toLowerCase().startsWith(searchTerm)
    );
  }

  findProductById(id) {
    return this.products?.find((product) => product.id === id);
  }

  handleProductSelection(event, index) {
    const particularDetail = this.purchaseOrderDetails.at(index);
    const productId = event?.option?.value;
    const storeId = this.purchaseOrderForm.get("storeId")?.value;

    if (!storeId) {
      this.showSnackBar("Please select a Store!");
      particularDetail.patchValue({
        productId: null,
      });
      return;
    }

    if (productId && this.purchaseOrderForm.value.supplierId) {
      this.purchaseOrderService
        .getLastPoDetails(this.purchaseOrderForm.value.supplierId, productId)
        .subscribe((res) => {
          if (res.data) {
            particularDetail.patchValue({
              lastPoDetails: `${res.data.poNumber}, Ordered Qty: ${res.data.orderedQuantity}, Received Qty: ${res.data.receivedQuantity}, Rate: ${res.data.rate},`,
              lastPoStatus: res.data.status,
              poId: res.data.poId,
            });
          } else {
            particularDetail.patchValue({
              lastPoDetails: "PO Not Found",
              lastPoStatus: 0,
              poId: null,
            });
          }
        });
    }

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
    });
    this.calculateAmount(index);
  }

  showSnackBar(message: string) {
    this.snackBarService.openSnackbar(message);
  }

  isExistSelectedProduct(productId: string, index: number): boolean {
    const isProductAdded = this.purchaseOrderDetails.value.some((item, i) => {
      return item.productId === productId && i !== index;
    });
    return isProductAdded;
  }

  setExchangeRateInPurchaseOrderDetails() {
    const poDetailLength = this.purchaseOrderDetails.length;
    for (let i = 0; i < poDetailLength; i++) {
      this.calculateAmount(i);
    }
  }

  calculateCost() {
    const cost = this.purchaseOrderDetails?.value?.reduce(
      (sum, item) => sum + item.quantity * item.rate,
      0
    );
    const discount = this.purchaseOrderForm.get("discount").value;
    this.purchaseOrderForm.get("subtotal").setValue(cost.toFixed(2));
    this.purchaseOrderForm.get("total").setValue((cost - discount).toFixed(2));
  }

  onControlChange(event: any, itemIndex?: number): void {
    const name = event?.target?.name;
    if (name === "quantity" || "rate" || "currencyRate") {
      this.calculateAmount(itemIndex);
    }
    if (name === "discount") {
      this.calculateCost();
    }
  }

  calculateAmount(itemIndex: number) {
    const item = this.purchaseOrderDetails.at(itemIndex);
    const exchangeRate = this.purchaseOrderForm.get("exchangeRate")?.value;
    const isImportPurchase =
      this.purchaseOrderForm.get("isImportPurchase")?.value;
    const quantity = item.get("quantity")?.value;
    const currencyRate = item.get("currencyRate")?.value;
    const amount = item.get("amount");
    const currencyAmount = item.get("currencyAmount");
    if (isImportPurchase) {
      const ratePerUnit = item.get("rate");
      ratePerUnit?.setValue(exchangeRate * currencyRate);
      currencyAmount?.setValue(quantity * currencyRate);
      amount?.setValue((quantity * currencyRate * exchangeRate).toFixed(2));
    } else {
      const ratePerUnit = item.get("rate")?.value;
      amount?.setValue((quantity * ratePerUnit).toFixed(2));
    }
    this.purchaseOrderForm.get("discount").setValue(0);
    this.calculateCost();
  }

  deletedIds: string = "";
  onDeleteItem(id: string, itemIndex: number): void {
    if (id) this.deletedIds += `${id},`;
    this.purchaseOrderDetails.removeAt(itemIndex);
    this.calculateCost();
  }

  private handleSuccessfulSave(res: GeneralResponse<string>): void {
    if (res?.succeeded) {
      this.handlePurchaseOrderResponse(res?.data);
      this.toastr.success(res?.message);
      this.deletedIds = "";
    } else {
      this.isLoading = false;
      this.toastr.error(res?.message);
    }
  }

  private handlePurchaseOrderResponse(purchaseOrderId: string): void {
    this.purchaseOrderService.getPurchaseOrderById(purchaseOrderId).subscribe({
      next: (purchaseOrderResponse) => {
        this.data = purchaseOrderResponse.data;
        this.isViewMode = true;
        this.isLoading = false;
        this.navigateToView(purchaseOrderResponse?.data?.id);
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

  handleDeliveryDate() {
    this.purchaseOrderForm
      .get("deliveryDate")
      .setValue(
        this.dateFormatService.getDateDynamically(
          this.purchaseOrderForm.get("deliveryTermInDays").value
        )
      );
  }

  addPurchaseOrder(body): void {
    this.purchaseOrderService
      .createPurchaseOrder(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  updatePurchaseOrder(body): void {
    this.purchaseOrderService
      .updatePurchaseOrder(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  onSubmit() {
    if (this.purchaseOrderForm.valid) {
      this.isLoading = true;
      const formValue = this.purchaseOrderForm.value;
      if (!formValue.id) {
        this.addPurchaseOrder(formValue);
      } else {
        formValue.deletedPurchaseOrderDetailIds = this.deletedIds;
        this.updatePurchaseOrder(formValue);
      }
    }
  }

  toggleImportPurchase() {
    if (!this.purchaseOrderForm.value.isImportPurchase) {
      this.purchaseOrderForm.patchValue({
        proformaInvoiceNo: "",
        lcNumber: "",
        exchangeRate: 0,
        currencyId: null,
        importPurchaseIncoTerm: 0,
        importPurchasePaymentTerm: 0,
        portOfLoading: "",
        portOfDestination: "",
      });
      const poDetailLength = this.purchaseOrderDetails.length;
      for (let i = 0; i < poDetailLength; i++) {
        const poDetail = this.purchaseOrderDetails.at(i);
        poDetail.patchValue({
          currencyRate: 0,
          rate: 0,
          currencyAmount: 0,
          amount: 0,
        });
        this.calculateAmount(i);
      }
    } else {
      this.purchaseOrderForm.patchValue({
        paymentMethodId: null,
        transport: 0,
        paymentMode: 0,
      });
    }
    this.updateValidators(); //! this method has been called binding custom Validator with purchaseInvoiceForm.
  }

  // openVendorQuotationDialog
  openVendorQuotationDialog() {
    const dialogRef = this.dialog.open(VendorQuotationListComponent, {
      disableClose: true,
      panelClass: "add-bill-container",
      minHeight: "auto",
      height: "auto",
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (!result) {
        return;
      }

      this.purchaseOrderForm.reset();
      this.purchaseOrderForm.setControl(
        "purchaseOrderDetails",
        this.fb.array([])
      );
      this.purchaseOrderForm.markAllAsTouched();

      const formData = {
        quotationNo: result?.quotationNo,
        requisitionNo: result?.requisitionNo,
        referenceNo: result?.referenceNo,
        storeId: result?.storeId,
        supplierId: result?.supplierId,
        transport: result?.transport,
        paymentMode: result?.paymentMode,
        paymentTermInDays: result?.paymentTermInDays,
        deliveryTermInDays: result?.deliveryTermInDays,
        deliveryDate: result?.deliveryDate,
        termAndCondition: result?.termAndCondition,
        weightVariance: 0,
        isPartialDelivery: false,
        remark: result?.remark,
        isImportPurchase: false,
        proformaInvoiceNo: "",
        lcNumber: "",
        exchangeRate: 0,
        currencyId: result?.currencyId,
        importPurchaseIncoTerm: result?.importPurchaseIncoTerm,
        importPurchasePaymentTerm: result?.importPurchasePaymentTerm,
        portOfLoading: "",
        portOfDestination: "",
        subtotal: result?.totalAmount,
        discount: 0,
        total: result?.totalAmount,
      };
      this.purchaseOrderForm.patchValue(formData);

      result.vendorQuotationDetails.forEach(
        (item: VendorQuotationResponseDetail) => {
          const purchaseOrderResponseDetail: PurchaseOrderResponseDetail = {
            productId: item?.productId,
            product: item?.product,
            quantity: item?.quantity,
            currencyRate: 0,
            rate: item?.rate,
            currencyAmount: 0,
            amount: item?.amount,
          };
          this.addItem(purchaseOrderResponseDetail);
        }
      );
    });
  }

  // openPurchaseRequisitionDialog
  openPurchaseRequisitionDialog() {
    const dialogRef = this.dialog.open(PurchaseRequisitionListComponent, {
      data: "fromPurchaseOrderComponent",
      disableClose: true,
      panelClass: "add-bill-container",
      minHeight: "auto",
      height: "auto",
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (!result) {
        return;
      }

      this.purchaseOrderForm.reset();
      this.purchaseOrderForm.setControl(
        "purchaseOrderDetails",
        this.fb.array([])
      );
      this.purchaseOrderForm.markAllAsTouched();

      const formData = {
        requisitionNo: result?.requisitionNo,
        deliveryDate: result?.expectedDeliveryDate,
        termAndCondition: result?.termAndCondition,
        storeId: result?.storeId,
        transport: result?.transport,
        paymentTermInDays: result?.paymentTermInDays,
        paymentMode: result?.paymentMode,
        weightVariance: 0,
        isPartialDelivery: false,
        remark: result?.remark,
        isImportPurchase: false,
        proformaInvoiceNo: "",
        lcNumber: "",
        exchangeRate: 0,
        currencyId: result?.currencyId,
        importPurchaseIncoTerm: result?.importPurchaseIncoTerm,
        importPurchasePaymentTerm: result?.importPurchasePaymentTerm,
        portOfLoading: "",
        portOfDestination: "",
        subtotal: result?.totalAmount,
        discount: 0,
        total: result?.totalAmount,
      };
      this.purchaseOrderForm.patchValue(formData);

      result.purchaseRequisitionDetails.forEach(
        (item: PurchaseRequisitionResponseDetail) => {
          const purchaseOrderResponseDetail: any = {
            productId: item?.productId,
            product: item?.product,
            quantity: item?.quantity - item?.orderedQuantity,
            currencyRate: 0,
            rate: item?.rate,
            currencyAmount: 0,
            amount: item?.amount,
          };
          this.addItem(purchaseOrderResponseDetail);
        }
      );
    });
  }

  getPurchaseOrderStatus(value) {
    return this.statusColorService.getPurchaseOrderStatus(value);
  }

  getPurchaseOrderStatusName(value: number) {
    return PurchaseOrderStatus[value];
  }

  check(id: string) {
    if (this.isLoading) return;
    this.isLoading = true;

    this.purchaseOrderService
      .checkPurchaseOrder(id)
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

    this.purchaseOrderService
      .approvePurchaseOrder(id)
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

  readyForGrn(id: string) {
    if (this.isLoading) return;
    this.isLoading = true;

    this.purchaseOrderService
      .readyForGrnPurchaseOrder(id)
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

    this.purchaseOrderService
      .unpostPurchaseOrder(id, status)
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

    this.purchaseOrderService
      .sendPurchaseOrder(id)
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

  close(id: string, index: number): void {
    if (this.isLoading) return;
    this.isLoading = true;

    const item = this.purchaseOrderDetails.at(index);
    this.purchaseOrderService
      .closePurchaseOrder(id)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (res) => {
          if (res?.succeeded) {
            item.get("lastPoStatus").setValue(res.data);
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

  confirmReadyForGrn(id: string, code: string = "") {
    if (this.isDialogOpen) return;
    this.isDialogOpen = true;

    const data: ConfirmReadyForGrnDialogModel = {
      id: id,
      title: "Confirm Ready For GRN",
      message: `Confirm status update to 'Ready For GRN' for item ${code}?`,
    };
    const dialogRef = this.confirmReadyForGrnDialogService.confirmDialog(
      id,
      this.readyForGrn.bind(this),
      data.message,
      data.title
    );

    dialogRef.afterClosed().subscribe(() => (this.isDialogOpen = false));
  }

  confirmSendToSupplier(id: string) {
    if (this.isDialogOpen) return;
    this.isDialogOpen = true;

    const data: ConfirmDialogModel = {
      title: "Confirm Send to Supplier",
      message: `Are You Confirmed to Send Purchase Order to this Supplier?`,
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

  confirmClose(poId: string, itemIndex?: number) {
    if (this.isDialogOpen) return;
    this.isDialogOpen = true;

    const data: ConfirmDialogModel = {
      title: "Confirm Close!",
      message: `Proceeding with the closure of this Purchase Order. Once confirmed, it's permanent!`,
    };
    const dialogRef = this.poCloseConfirmDialogService.confirmDialog(
      poId,
      this.close.bind(this),
      data.message,
      data.title,
      itemIndex
    );

    dialogRef.afterClosed().subscribe(() => (this.isDialogOpen = false));
  }

  printPdf(id) {
    this.http
      .get(environment.apiURL + "/PurchaseOrder/print/" + id, {
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
