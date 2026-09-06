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
import { ImportPurchaseIncoTerm } from "app/shared/enums/importPurchaseIncoTerm";
import { ImportPurchasePaymentTerm } from "app/shared/enums/importPurchasePaymentTerm";
import { PurchaseInvoiceStatus } from "app/shared/enums/purchaseInvoiceStatus";
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
import { Currency } from "app/views/configuration/models/currency/currency.model";
import { ProductRequest } from "app/views/configuration/models/product/product-request.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { Store } from "app/views/configuration/models/store/store.model";
import { Supplier } from "app/views/configuration/models/supplier/supplier.model";
import { CurrencyService } from "app/views/configuration/services/currency.service";
import { ProductService } from "app/views/configuration/services/product.service";
import { StoreService } from "app/views/configuration/services/store.service";
import { SupplierService } from "app/views/configuration/services/supplier.service";
import { GoodsReceiveNoteResponseDetail } from "app/views/purchase/models/goods-receive-note/goods-receive-note-response-dto.model";
import {
  PurchaseInvoiceResponseDTO,
  PurchaseInvoiceResponseDetail,
} from "app/views/purchase/models/purchase-invoice/purchase-invoice-response-dto.model";
import { SupplierPaymentSearchRequestDTO } from "app/views/purchase/models/supplier-payment/supplier-payment-search-request-dto.model";
import { PurchaseInvoiceService } from "app/views/purchase/services/purchase-invoice.service";
import { SupplierPaymentService } from "app/views/purchase/services/supplier-payment.service";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { finalize } from "rxjs";
import { GRNListComponent } from "../grn-list/grn-list.component";
import { SupplierPaymentDialogComponent } from "../supplier-payment-dialog/supplier-payment-dialog.component";

@Component({
  selector: "app-purchase-invoice-form",
  templateUrl: "./purchase-invoice-form.component.html",
  styleUrls: ["./purchase-invoice-form.component.scss"],
})
export class PurchaseInvoiceFormComponent implements OnInit, AfterViewInit {
  @ViewChild("btnCheck") btnCheck: MatButton;
  @ViewChild("btnApprove") btnApprove: MatButton;
  @ViewChild("stepper") private stepper: MatStepper;
  isLoading: boolean = false;
  isDialogOpen: boolean = false;
  isViewMode: boolean = true;
  isChecked: boolean;
  formTitle: string;
  purchaseInvoiceForm: FormGroup;
  stores: Store[];
  filterStores: Store[];
  suppliers: Supplier[];
  filterSuppliers: Supplier[];
  products: ProductView[];
  searchProducts: ProductView[];
  importPurchaseIncoTerms: ENUM[];
  importPurchasePaymentTerms: ENUM[];
  currencies: Currency[];
  purchaseInvoiceDetailsData: any[] = [];
  cost = 0;
  data: PurchaseInvoiceResponseDTO;
  transactionalJournalAccounts: any[];
  businessType: string;
  // Define financial year date range here
  financialYearStartDate: Date; // Jul 1, 2023
  financialYearEndDate: Date; // Jun 30, 2024
  currentFinancialYearId: string;
  applyMinMax: boolean = false;

  private path = {
    list: "purchase/purchase-invoice",
    edit: "purchase/purchase-invoice",
  };

  constructor(
    public dialog: MatDialog,
    private purchaseInvoiceService: PurchaseInvoiceService,
    private storeService: StoreService,
    private enumValueService: EnumValueService,
    private currencyService: CurrencyService,
    private supplierService: SupplierService,
    private productService: ProductService,
    private supplierPaymentService: SupplierPaymentService,
    private dateFormatService: DateTimeFormatService,
    private jwtAuth: JwtAuthService,
    private fb: FormBuilder,
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
    this.activatedRoute.data.subscribe((response: any) => {
      this.data = response?.purchaseInvoice?.data;
    });
    this.getData();
    this.jwtAuth.userProfile.subscribe((res: UserProfile) => {
      this.financialYearStartDate = new Date(res.fystartdate);
      this.financialYearEndDate = new Date(res.fyenddate);
      this.currentFinancialYearId = res.fyid;
      this.businessType = res.businesstype;
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

  getData() {
    this.getAllStores();
    this.getAllCurrencies();
    this.getAllImportPurchaseIncoTerms();
    this.getAllImportPurchasePaymentTerms();
    this.getAllSuppliers();
    //! it's important after close openGRNDialog
    this.getAllProducts();
  }

  initializeForm() {
    this.createForm();
    this.populateForm();
    this.setFormTitle();
    this.purchaseInvoiceForm.get("invoiceDate").markAsTouched();
  }

  createForm() {
    this.purchaseInvoiceForm = this.fb.group({
      id: [this.data?.id ?? null],
      invoiceDate: [
        this.data?.invoiceDate || this.dateFormatService.getPresentDate(),
      ],
      grnno: [this.data?.grnno ?? "", Validators.required],
      ponumber: [this.data?.ponumber ?? ""],
      supplierPaymentCode: [this.data?.supplierPaymentCode ?? ""],
      storeId: [this.data?.storeId ?? null, Validators.required],
      supplierId: [this.data?.supplierId ?? null, Validators.required],
      supplierInvoiceNo: [
        this.data?.supplierInvoiceNo ?? "",
        [this.customsRequiredValidator("supplierInvoiceNo")],
      ],
      supplierInvoiceDate: [
        this.data?.supplierInvoiceDate ?? null,
        [this.customsRequiredValidator("supplierInvoiceDate")],
      ],
      paymentTermInDays: [this.data?.paymentTermInDays ?? 0],
      isImportPurchase: [this.data?.isImportPurchase || false],
      proformaInvoiceNo: [this.data?.proformaInvoiceNo || ""],
      lcNumber: [this.data?.lcNumber || ""],
      exchangeRate: [this.data?.exchangeRate || 0],
      currencyId: [this.data?.currencyId || null],
      importPurchaseIncoTerm: [this.data?.importPurchaseIncoTerm || 0],
      importPurchasePaymentTerm: [this.data?.importPurchasePaymentTerm || 0],
      billOfEntryNo: [
        this.data?.billOfEntryNo ?? "",
        [this.customsRequiredValidator("billOfEntryNo")],
      ],
      billOfEntryDate: [
        this.data?.billOfEntryDate ?? null,
        [this.customsRequiredValidator("billOfEntryDate")],
      ],
      portOfLoading: [this.data?.portOfLoading || ""],
      portOfDestination: [this.data?.portOfDestination || ""],
      additionalLandedCost: [this.data?.additionalLandedCost || 0],
      adjustmentValue: [this.data?.adjustmentValue || 0],
      subtotal: [this.data?.subtotal ?? 0],
      discount: [this.data?.discount ?? 0, Validators.required],
      transportationCost: [this.data?.transportationCost ?? 0],
      totalVat: [this.data?.totalVat ?? 0],
      advancePaymentAmount: [
        this.data?.advancePaymentAmount ?? 0,
        [this.customRequiredValidator()],
      ],
      total: [this.data?.total ?? 0],
      totalGrnAdjustmentAmount: [this.data?.totalGrnAdjustmentAmount ?? 0],
      purchaseOrderTotal: [this.data?.purchaseOrderTotal || 0],
      netPayable: [this.data?.netPayable ?? 0],
      remark: [this.data?.remark ?? ""],
      deletedPurchaseInvoiceDetailIds: [""],
      purchaseInvoiceDetails: this.fb.array([]),
    });
    this.setMinMaxDates();
  }

  updateValidators() {
    this.purchaseInvoiceForm.get("supplierInvoiceNo").updateValueAndValidity();
    this.purchaseInvoiceForm
      .get("supplierInvoiceDate")
      .updateValueAndValidity();
    this.purchaseInvoiceForm.get("billOfEntryNo").updateValueAndValidity();
    this.purchaseInvoiceForm.get("billOfEntryDate").updateValueAndValidity();
  }

  // Common custom validator function
  customsRequiredValidator(field: string) {
    return (control) => {
      const value = control.value;
      const isImportPurchase =
        this.purchaseInvoiceForm?.value?.isImportPurchase;

      if (isImportPurchase) {
        if (
          field === "billOfEntryNo" &&
          (value === undefined || value === "")
        ) {
          return { billOfEntryNoRequired: true };
        }
        if (
          field === "billOfEntryDate" &&
          (value === undefined || value === "" || value === null)
        ) {
          return { billOfEntryDateRequired: true };
        }
      }
      if (!isImportPurchase) {
        if (
          field === "supplierInvoiceNo" &&
          (value === undefined || value === "")
        ) {
          return { supplierInvoiceNoRequired: true };
        }
        if (
          field === "supplierInvoiceDate" &&
          (value === undefined || value === "" || value === null)
        ) {
          return { supplierInvoiceDateRequired: true };
        }
      }
      return null;
    };
  }

  //this method will apply custom validation on advancePaymentAmount field
  customRequiredValidator() {
    return (control) => {
      const value = control.value;
      if (this.totalSupplierPaymentCount > 0 && value === 0) {
        return { customRequired: true };
      }
      return null;
    };
  }

  setMinMaxDates() {
    if (this.applyMinMax) {
      this.purchaseInvoiceForm
        .get("invoiceDate")
        .setValidators([
          Validators.required,
          inFinancialYearValidator(
            this.financialYearStartDate,
            this.financialYearEndDate
          ),
        ]);
    } else {
      this.purchaseInvoiceForm
        .get("invoiceDate")
        .setValidators([Validators.required]);
    }

    this.purchaseInvoiceForm.get("invoiceDate").updateValueAndValidity();
  }

  //Start  --this method will be being called for customRequiredValidator
  supplierPaymentRequest = new SupplierPaymentSearchRequestDTO();
  totalSupplierPaymentCount: number = 0;
  getSupplierPayments(): void {
    this.supplierPaymentRequest.page = -1;
    this.supplierPaymentRequest.supplierPaymentType = 1;
    this.supplierPaymentRequest.supplierId =
      this.purchaseInvoiceForm.value.supplierId;
    this.supplierPaymentRequest.keyword =
      this.purchaseInvoiceForm.value.ponumber;
    this.supplierPaymentRequest.supplierPaymentStatuses = [3, 4];
    this.supplierPaymentService
      .getSupplierPayments(this.supplierPaymentRequest)
      .subscribe((res) => {
        this.totalSupplierPaymentCount = res?.data?.item2;
        this.purchaseInvoiceForm.patchValue({
          advancePaymentAmount: 0,
        });
      });
  }
  //End

  get purchaseInvoiceDetails(): FormArray {
    return this.purchaseInvoiceForm.get("purchaseInvoiceDetails") as FormArray;
  }

  populateForm(): void {
    if (this.purchaseInvoiceForm.get("id").value) {
      this.populatePurchaseInvoiceDetails(this.data);
    } else {
      this.addItem();
    }
  }

  setFormTitle(): void {
    if (this.purchaseInvoiceForm.get("id").value) {
      this.formTitle = "Edit Purchase Invoice";
    } else {
      this.formTitle = "Add Purchase Invoice";
    }
  }

  populatePurchaseInvoiceDetails(data: PurchaseInvoiceResponseDTO) {
    data.purchaseInvoiceDetails.forEach((item: PurchaseInvoiceResponseDetail) =>
      this.addItem(item)
    );
  }

  addItem(item?: PurchaseInvoiceResponseDetail): void {
    this.purchaseInvoiceDetails.push(this.createPurchaseInvoiceDetail(item));
  }

  createPurchaseInvoiceDetail(item?: PurchaseInvoiceResponseDetail): FormGroup {
    return this.fb.group({
      id: [item?.id ?? null],
      productId: [item?.productId ?? null, Validators.required],
      product: [item?.product ?? null],
      measurementUnitName: [item?.product.measurementUnit.name ?? ""],
      grnquantity: [item?.grnquantity ?? 0, Validators.required],
      bagWeightDeductionQuantity: [item?.bagWeightDeductionQuantity ?? 0],
      numberOfBagQuantity: [item?.numberOfBagQuantity ?? 0],
      netQuantity: [item?.netQuantity ?? 0],
      currencyRate: [item?.currencyRate ? item?.currencyRate : 0],
      rate: [item?.rate ?? 0, Validators.required],
      rateAfterBagWeightDeduction: [item?.rateAfterBagWeightDeduction ?? 0],
      currencyAmount: [item?.currencyAmount || 0],
      amount: [item?.amount ?? 0],
      grnno: [item?.grnno ?? ""],
      grndate: [item?.grndate ?? null],
      vatPercentage: [item?.vatPercentage ?? 0],
    });
  }

  getImportPurchaseValue(): boolean {
    return this.purchaseInvoiceForm?.get("isImportPurchase")?.value || false;
  }

  getAllStores(): void {
    this.storeService.getAllStores().subscribe((res) => {
      this.stores = res?.data?.item1;
    });
  }

  getAllSuppliers(): void {
    this.supplierService.getAllSuppliers().subscribe((res) => {
      this.suppliers = res?.data?.item1;
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

  getCurrencyName(currencyId) {
    return this.currencies?.find((x) => x?.id === currencyId)?.name;
  }

  getImportPurchaseIncoTermName(value) {
    return ImportPurchaseIncoTerm[value];
  }

  getImportPurchasePaymentTermName(value) {
    return ImportPurchasePaymentTerm[value];
  }

  isPassingDataToNextStep = false;
  handleStepChange(event: any) {
    this.isPassingDataToNextStep = event.selectedIndex === 1;
    if (this.isPassingDataToNextStep) {
      this.purchaseInvoiceDetailsData = this.purchaseInvoiceForm.get(
        "purchaseInvoiceDetails"
      ).value;
      this.setTransactionalJournalAccounts();
    }
  }

  setTransactionalJournalAccounts() {
    if (this.purchaseInvoiceForm) {
      this.transactionalJournalAccounts = [
        {
          accountName: "Purchase Account",
          column: "debit",
          value: this.purchaseInvoiceForm?.get("subtotal")?.value,
        },
        // {
        //   accountName: "Vat",
        //   column: "debit",
        //   value: this.purchaseInvoiceForm?.get("totalVat")?.value,
        // },
        {
          accountName: "Discount",
          column: "credit",
          value: this.purchaseInvoiceForm?.get("discount")?.value,
        },
        {
          accountName: "Supplier Account",
          column: "credit",
          value: this.purchaseInvoiceForm?.get("total")?.value,
        },
        {
          accountName: "",
          row: "total",
          value:
            this.purchaseInvoiceForm?.get("total")?.value +
            this.purchaseInvoiceForm?.get("discount")?.value,
        },
        //
      ];
    }
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    if (fieldName === "supplierId") {
      this.purchaseInvoiceForm?.get("supplierId").setValue(null);
    }
    if (fieldName === "storeId") {
      this.purchaseInvoiceForm?.get("storeId").setValue(null);
    }
  }

  clearInputFromDetails(evt: any, itemIndex: number): void {
    evt.stopPropagation();
    const particularDetail = this.purchaseInvoiceDetails.at(itemIndex);
    particularDetail?.patchValue({
      productId: null,
    });
  }

  handleSupplierSearch(event: any): void {
    const name = event.target?.name;
    if (name === "supplierId") {
      const term = this.purchaseInvoiceForm.get("supplierId");
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

  handleStoreSearch(event: any): void {
    const name = event.target?.name;
    if (name === "storeId") {
      const term = this.purchaseInvoiceForm.get("storeId");
      this.filterStore(term.value || "");
    }
  }

  private filterStore(value: string) {
    const filterValue = value.toLowerCase();
    this.filterStores = this.stores?.filter((option) =>
      option.name.toLowerCase().includes(filterValue)
    );
  }

  getStoreName(storeId) {
    if (!storeId) {
      return;
    }
    const store =
      this.stores?.find((x) => x?.id === storeId) || this.data?.store;
    return store?.name;
  }

  handleProductSearch(event: any, itemIndex: number): void {
    const name = event.target.name;
    if (name === "productId") {
      const term = this.purchaseInvoiceDetails.at(itemIndex).get("productId");
      this.filterProduct(term.value || "");
    }
  }

  filterProduct(searchTerm: string) {
    searchTerm = searchTerm.toLowerCase();
    this.searchProducts = this.products?.filter((product) =>
      product.name.toLowerCase().startsWith(searchTerm)
    );
  }

  getAllProducts(): void {
    let productRequest = new ProductRequest();
    //productRequest.inventoryTypeId = Inventory_Type_Id_Raw_Materials;
    productRequest.isPurchaseProduct = true;
    productRequest.page = -1;
    this.productService.getProducts(productRequest).subscribe((res) => {
      this.searchProducts = this.products = res?.data?.item1;
    });
  }

  findProductById(id) {
    return this.products.find((product) => product.id === id);
  }

  handleProductSelection(event, index) {
    const particularDetail = this.purchaseInvoiceDetails.at(index);
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
    });
    this.calculateAmount(index);
  }

  showSnackBar(message: string) {
    this.snackBarService.openSnackbar(message);
  }

  isExistSelectedProduct(productId: string, index: number): boolean {
    const isProductAdded = this.purchaseInvoiceDetails.value.some((item, i) => {
      return item.productId === productId && i !== index;
    });
    return isProductAdded;
  }

  getProductName(productId: string) {
    if (!productId) {
      return;
    }
    const product =
      this.products?.find((product) => product?.id === productId) ||
      this.data?.purchaseInvoiceDetails?.find((x) => x?.productId == productId)
        ?.product;
    if (!product) return "";
    return (
      product?.name +
      (this.businessType === "2" ? "" : ` (${product?.packSize?.name})`)
    );
  }

  calculateCost() {
    const cost = this.purchaseInvoiceDetails?.value?.reduce(
      (sum, item) => sum + item?.grnquantity * item?.rate,
      0
    );
    const vatCost = this.purchaseInvoiceDetails?.value?.reduce(
      (sum, item) => sum + (item?.vatPercentage * item?.amount) / 100,
      0
    );
    this.purchaseInvoiceForm.get("totalVat").setValue(vatCost);
    const discount = this.purchaseInvoiceForm.get("discount").value;
    const advancePaymentAmount = this.purchaseInvoiceForm.get(
      "advancePaymentAmount"
    ).value;
    const totalGrnAdjustmentAmount = this.purchaseInvoiceForm.get(
      "totalGrnAdjustmentAmount"
    ).value;
    this.purchaseInvoiceForm.get("subtotal").setValue(cost);
    this.purchaseInvoiceForm.get("total").setValue(cost - discount + vatCost);
    this.purchaseInvoiceForm
      .get("netPayable")
      .setValue(
        cost -
          advancePaymentAmount -
          totalGrnAdjustmentAmount -
          discount +
          vatCost
      );
  }

  onControlChange(event: any, itemIndex?: number): void {
    if (event?.target) {
      const name = event?.target?.name;
      if (
        name === "grnquantity" ||
        name === "rate" ||
        name === "vatPercentage"
      ) {
        this.calculateAmount(itemIndex);
      }
      if (name === "discount") {
        this.calculateCost();
      }
    }
  }

  calculateAmount(itemIndex: number) {
    const item = this.purchaseInvoiceDetails.at(itemIndex);

    const quantity = item.get("grnquantity").value;
    const ratePerUnit = item.get("rate").value;
    const amount = item.get("amount");

    amount?.setValue(quantity * ratePerUnit);
    this.purchaseInvoiceForm.get("discount").setValue(0);
    this.calculateCost();
  }

  deletedIds: string = "";
  onDeleteItem(id: string, itemIndex: number): void {
    if (id) this.deletedIds += `${id},`;
    this.purchaseInvoiceDetails.removeAt(itemIndex);
    this.calculateCost();
  }

  private handleSuccessfulSave(res: GeneralResponse<string>): void {
    if (res?.succeeded) {
      this.handlePurchaseInvoiceResponse(res?.data);
      this.toastr.success(res?.message);
      this.deletedIds = "";
    } else {
      this.isLoading = false;
      this.toastr.error(res?.message);
    }
  }

  private handlePurchaseInvoiceResponse(purchaseInvoiceId: string): void {
    this.purchaseInvoiceService
      .getPurchaseInvoiceById(purchaseInvoiceId)
      .subscribe({
        next: (purchaseInvoiceResponse) => {
          this.data = purchaseInvoiceResponse.data;
          this.isViewMode = true;
          this.isLoading = false;
          this.navigateToView(purchaseInvoiceResponse?.data?.id);
        },
        error: (err) => {
          location.reload();
        },
      });
  }

  private refreshPurchaseInvoiceData(purchaseInvoiceId: string): void {
    this.purchaseInvoiceService
      .getPurchaseInvoiceById(purchaseInvoiceId)
      .subscribe({
        next: (purchaseInvoiceResponse) => {
          this.data = purchaseInvoiceResponse.data;
          this.initializeForm();
          //Start -- this method is being called for customRequiredValidator
          this.getSupplierPayments();
          //End
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

  navigateToList() {
    this.router.navigate([this.path.list]);
  }

  backToView() {
    this.initializeForm();
    this.stepper.next();
    this.isViewMode = true;
  }

  addPurchaseInvoice(body): void {
    this.purchaseInvoiceService
      .createPurchaseInvoice(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  updatePurchaseInvoice(body): void {
    this.purchaseInvoiceService
      .updatePurchaseInvoice(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  onSubmit() {
    if (this.purchaseInvoiceForm.valid) {
      this.isLoading = true;
      const formValue = this.purchaseInvoiceForm.value;
      if (!formValue.id) {
        this.addPurchaseInvoice(formValue);
      } else {
        formValue.deletedPurchaseInvoiceDetailIds = this.deletedIds;
        this.updatePurchaseInvoice(formValue);
      }
    }
  }

  toggleImportPurchase() {
    if (!this.purchaseInvoiceForm.value.isImportPurchase) {
      this.purchaseInvoiceForm.patchValue({
        billOfEntryNo: "",
        billOfEntryDate: null,
      });
    } else {
      this.purchaseInvoiceForm.patchValue({
        supplierInvoiceNo: "",
        supplierInvoiceDate: null,
      });
    }
    this.updateValidators(); //! this method has been called binding custom Validator with purchaseInvoiceForm.
  }

  // openGRNDialog
  openGRNDialog() {
    const dialogRef = this.dialog.open(GRNListComponent, {
      disableClose: true,
      panelClass: "add-bill-container",
      minHeight: "auto",
      height: "auto",
      data: this.purchaseInvoiceForm.get("isImportPurchase")?.value,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        let commaSeparatedGRNNo = result.map((grn) => grn.grnno).join(",");
        let resultCost = result.reduce(
          (prevVal, currVal) => {
            return {
              subtotal: prevVal.subtotal + currVal.subtotal,
              discount: prevVal.discount + currVal.discount,
              transportationCost:
                prevVal.transportationCost + currVal.transportationCost,
              total: prevVal.subtotal + currVal.subtotal,
              totalGrnAdjustmentAmount:
                prevVal.totalGrnAdjustmentAmount +
                currVal.totalGrnAdjustmentAmount,
              additionalLandedCost:
                prevVal.additionalLandedCost + currVal.additionalLandedCost,
            };
          },
          {
            subtotal: 0,
            discount: 0,
            transportationCost: 0,
            total: 0,
            totalGrnAdjustmentAmount: 0,
            additionalLandedCost: 0,
          }
        );

        // let subtotal = result.reduce((prevVal, currVal) => {
        //   return prevVal + currVal.subtotal;
        // }, 0);
        // let discount = result.reduce((prevVal, currVal) => {
        //   return prevVal + currVal.discount;
        // }, 0);
        // let total = result.reduce((prevVal, currVal) => {
        //   return prevVal + currVal.total;
        // }, 0);

        this.purchaseInvoiceForm.reset();
        this.purchaseInvoiceForm.setControl(
          "purchaseInvoiceDetails",
          this.fb.array([])
        );
        this.purchaseInvoiceForm.markAllAsTouched();

        this.purchaseInvoiceForm.patchValue({
          invoiceDate: this.dateFormatService.getPresentDate(),
          grnno: commaSeparatedGRNNo,
          ponumber: result[0].ponumber,
          storeId: result[0].storeId,
          supplierId: result[0].supplierId,
          supplierInvoiceNo: "",
          supplierInvoiceDate: null,
          paymentTermInDays: result[0].paymentTermInDays,
          isImportPurchase: result[0].isImportPurchase,
          proformaInvoiceNo: result[0].proformaInvoiceNo,
          lcNumber: result[0].lcNumber,
          exchangeRate: result[0].exchangeRate,
          currencyId: result[0].currencyId,
          importPurchaseIncoTerm: result[0].importPurchaseIncoTerm,
          importPurchasePaymentTerm: result[0].importPurchasePaymentTerm,
          billOfEntryNo: "",
          billOfEntryDate: null,
          portOfLoading: result[0].portOfLoading,
          portOfDestination: result[0].portOfDestination,
          remark: result[0].remark,
          additionalLandedCost: resultCost.additionalLandedCost,
          adjustmentValue: result[0].purchaseOrderTotal - resultCost.subtotal,
          subtotal: resultCost.subtotal,
          discount: resultCost.discount,
          transportationCost: resultCost.transportationCost,
          total: resultCost.total,
          totalVat: 0,
          totalGrnAdjustmentAmount: resultCost.totalGrnAdjustmentAmount,
          purchaseOrderTotal: result[0].purchaseOrderTotal,
          supplierPaymentCode: "",
          advancePaymentAmount: 0,
          //netPayable: resultCost.total - resultCost.totalGrnAdjustmentAmount,
        });

        if (result[0].isImportPurchase) {
          this.purchaseInvoiceForm.patchValue({
            netPayable: result[0].purchaseOrderTotal,
          });
        } else {
          this.purchaseInvoiceForm.patchValue({
            netPayable: resultCost.total - resultCost.totalGrnAdjustmentAmount,
          });
        }

        result.forEach((grn) => {
          grn.goodsReceiveNoteDetails.forEach(
            (item: GoodsReceiveNoteResponseDetail) => {
              let purchaseInvoiceResponseDetail: any = {
                productId: item.productId,
                product: item?.product,
                grnquantity: item.grnquantity,
                bagWeightDeductionQuantity: item.bagWeightDeductionQuantity,
                numberOfBagQuantity: item.numberOfBagQuantity,
                netQuantity: item.netQuantity,
                currencyRate: item?.currencyRate,
                rate: item.rate,
                rateAfterBagWeightDeduction: item.rateAfterBagWeightDeduction,
                currencyAmount: item?.currencyAmount,
                amount: item.amount,
                grnno: grn?.grnno,
                grndate: grn?.grndate,
              };
              this.addItem(purchaseInvoiceResponseDetail);
            }
          );
        });
        // Start --this method is being called for customRequiredValidator
        if (!this.purchaseInvoiceForm.value.isImportPurchase) {
          this.getSupplierPayments();
        }
        //End
      }
    });
  }

  // openSupplierPaymentDialog
  openSupplierPaymentDialog() {
    if (this.purchaseInvoiceForm.get("supplierId")?.value) {
      const dialogRef = this.dialog.open(SupplierPaymentDialogComponent, {
        disableClose: true,
        panelClass: "add-bill-container",
        minHeight: "auto",
        height: "auto",
        data: this.purchaseInvoiceForm?.value,
      });
      dialogRef.afterClosed().subscribe((result) => {
        if (result) {
          let commaSeparatedSupplierPaymentNo = result
            .map((supplierPayment) => supplierPayment.code)
            .join(",");

          let resultCost = result.reduce(
            (prevVal, currVal) => {
              return {
                totalAmount: prevVal.totalAmount + currVal.totalAmount,
                usedAmountInPurchaseInvoice:
                  prevVal.usedAmountInPurchaseInvoice +
                  currVal.usedAmountInPurchaseInvoice,
              };
            },
            {
              totalAmount: 0,
              usedAmountInPurchaseInvoice: 0,
            }
          );

          this.purchaseInvoiceForm.markAllAsTouched();
          this.purchaseInvoiceForm.patchValue({
            supplierPaymentCode: commaSeparatedSupplierPaymentNo,
            advancePaymentAmount:
              resultCost?.totalAmount -
                resultCost?.usedAmountInPurchaseInvoice >
              this.purchaseInvoiceForm.value.total
                ? this.purchaseInvoiceForm.value.total
                : resultCost?.totalAmount -
                  resultCost?.usedAmountInPurchaseInvoice,
          });
        }
        this.calculateCost();
      });
    } else alert("Please Select GRN First");
  }

  getPurchaseInvoiceStatus(value) {
    return this.statusColorService.getPurchaseInvoiceStatus(value);
  }

  getPurchaseInvoiceStatusName(value: number) {
    return PurchaseInvoiceStatus[value];
  }

  check(id: string) {
    if (this.isLoading) return;
    this.isLoading = true;

    this.purchaseInvoiceService
      .checkPurchaseInvoice(id)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (res) => {
          if (res?.succeeded) {
            this.data.status = res?.data as unknown as number;
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

    this.purchaseInvoiceService
      .approvePurchaseInvoice(id)
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

    this.purchaseInvoiceService
      .unpostPurchaseInvoice(id, status)
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

  resetSupplierPaymentCodeAndAdvanceAmount(id: string) {
    if (this.isLoading) return;
    this.isLoading = true;

    this.purchaseInvoiceService
      .resetSupplierPaymentCodeAndAdvanceAmount(id)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (res) => {
          if (res?.succeeded) {
            this.refreshPurchaseInvoiceData(res?.data);
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

  confirmResetAdvanceAmount(id: string) {
    if (this.isDialogOpen) return;
    this.isDialogOpen = true;

    const data: ConfirmDialogModel = {
      title: "Confirm Reset Advance Amount",
      message: `This action will remove the associated advance amount. Are you sure to remove?`,
    };
    const dialogRef = this.confirmDialogService.confirmDialog(
      id,
      this.resetSupplierPaymentCodeAndAdvanceAmount.bind(this),
      data.message,
      data.title
    );

    dialogRef.afterClosed().subscribe(() => (this.isDialogOpen = false));
  }

  printPdf(id) {
    this.http
      .get(environment.apiURL + "/ReportPurchaseInvoice/" + id, {
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
