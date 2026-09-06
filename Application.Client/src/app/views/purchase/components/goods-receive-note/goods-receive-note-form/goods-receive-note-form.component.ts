import { HttpClient } from "@angular/common/http";
import { ChangeDetectorRef, Component, OnInit, ViewChild } from "@angular/core";
import { FormArray, FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MatButton } from "@angular/material/button";
import { MatDialog } from "@angular/material/dialog";
import { MatStepper } from "@angular/material/stepper";
import { ActivatedRoute, Router } from "@angular/router";
import { Inventory_Type_Id_Raw_Materials } from "app/shared/consts/const";
import { GRNStatus } from "app/shared/enums/grnStatus";
import { ImportPurchaseIncoTerm } from "app/shared/enums/importPurchaseIncoTerm";
import { ImportPurchasePaymentTerm } from "app/shared/enums/importPurchasePaymentTerm";
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
import { StoreRequest } from "app/views/configuration/models/store/store-request.model";
import { Store } from "app/views/configuration/models/store/store.model";
import { Supplier } from "app/views/configuration/models/supplier/supplier.model";
import { CurrencyService } from "app/views/configuration/services/currency.service";
import { ProductService } from "app/views/configuration/services/product.service";
import { StoreService } from "app/views/configuration/services/store.service";
import { SupplierService } from "app/views/configuration/services/supplier.service";
import {
  GoodsReceiveNoteResponseDTO,
  GoodsReceiveNoteResponseDetail,
} from "app/views/purchase/models/goods-receive-note/goods-receive-note-response-dto.model";
import {
  PurchaseOrderResponseDTO,
  PurchaseOrderResponseDetail,
} from "app/views/purchase/models/purchase-order/purchase-order-response-dto.model";
import { GoodsReceiveNoteService } from "app/views/purchase/services/goods-receive-note.service";
import { PurchaseOrderService } from "app/views/purchase/services/purchase-order.service";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { finalize } from "rxjs";
import { PurchaseOrderListComponent } from "../purchase-order-list/purchase-order-list.component";

@Component({
  selector: "app-goods-receive-note-form",
  templateUrl: "./goods-receive-note-form.component.html",
  styleUrls: ["./goods-receive-note-form.component.scss"],
})
export class GoodsReceiveNoteFormComponent implements OnInit {
  @ViewChild("btnCheck") btnCheck: MatButton;
  @ViewChild("btnApprove") btnApprove: MatButton;
  @ViewChild("stepper") private stepper: MatStepper;
  isLoading: boolean = false;
  isDialogOpen: boolean = false;
  isViewMode: boolean = true;
  isChecked: boolean;
  isFieldReadonly: boolean = false;
  formTitle: string;
  goodsReceiveNoteForm: FormGroup;
  purchaseOrders: PurchaseOrderResponseDTO[];
  stores: Store[];
  filterStores: Store[];
  suppliers: Supplier[];
  filterSuppliers: Supplier[];
  products: ProductView[];
  searchProducts: ProductView[];
  importPurchaseIncoTerms: ENUM[];
  importPurchasePaymentTerms: ENUM[];
  currencies: Currency[];
  goodsReceiveNoteDetailsData: any[] = [];
  data: GoodsReceiveNoteResponseDTO;
  transactionalJournalAccounts: any[];

  businessType: string;
  // Define financial year date range here
  financialYearStartDate: Date; // Jul 1, 2023
  financialYearEndDate: Date; // Jun 30, 2024
  currentFinancialYearId: string;

  applyMinMax: boolean = false;

  private path = {
    list: "purchase/goods-receive-note",
    edit: "purchase/goods-receive-note",
  };

  constructor(
    public dialog: MatDialog,
    private goodsReceiveNoteService: GoodsReceiveNoteService,
    private storeService: StoreService,
    private enumValueService: EnumValueService,
    private currencyService: CurrencyService,
    private supplierService: SupplierService,
    private purchaseOrderService: PurchaseOrderService,
    private productService: ProductService,
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
      this.data = response?.goodsReceiveNote?.data;
    });
    let isDataLoaded = false;
    // this.getData();
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

  // ngOnDestroy() {
  //   this.localStorageService.removeItem(this.activatedRoute.snapshot.paramMap.get("id"));
  // }

  getData(businessType: string) {
    // let id = this.activatedRoute.snapshot.paramMap.get("id");
    // if (id) this.data = this.localStorageService.getItem(id);

    this.getAllStores(businessType);
    this.getAllCurrencies();
    this.getAllImportPurchaseIncoTerms();
    this.getAllImportPurchasePaymentTerms();
    this.getAllSuppliers();
    //! it's important after close openPurchaseOrderDialog
    this.getAllProducts();
  }

  initializeForm() {
    this.createForm();
    this.populateForm();
    this.setFormTitle();
    this.goodsReceiveNoteForm.get("grndate").markAsTouched();
  }

  createForm() {
    this.goodsReceiveNoteForm = this.fb.group({
      id: [this.data?.id ?? null],
      grndate: [this.data?.grndate || this.dateFormatService.getPresentDate()],
      ponumber: [this.data?.ponumber ?? "", Validators.required],
      storeId: [this.data?.storeId, Validators.required],
      supplierId: [this.data?.supplierId, Validators.required],
      challanNo: [this.data?.challanNo, Validators.required],
      challanDate: [this.data?.challanDate, Validators.required],
      truckNo: [this.data?.truckNo ?? ""],
      driverName: [this.data?.driverName ?? ""],
      driverContactNo: [this.data?.driverContactNo ?? ""],
      transport: [this.data?.transport],
      paymentTermInDays: [this.data?.paymentTermInDays ?? 0],
      isImportPurchase: [this.data?.isImportPurchase || false],
      proformaInvoiceNo: [this.data?.proformaInvoiceNo || ""],
      lcNumber: [this.data?.lcNumber || ""],
      exchangeRate: [this.data?.exchangeRate || 0],
      currencyId: [this.data?.currencyId || null],
      importPurchaseIncoTerm: [this.data?.importPurchaseIncoTerm || 0],
      importPurchasePaymentTerm: [this.data?.importPurchasePaymentTerm || 0],
      portOfLoading: [this.data?.portOfLoading || ""],
      portOfDestination: [this.data?.portOfDestination || ""],
      purchaseOrderTotal: [this.data?.purchaseOrderTotal || 0],
      subtotal: [this.data?.subtotal ?? 0],
      discount: [this.data?.discount ?? 0, Validators.required],
      transportationCost: [
        this.data?.transportationCost ?? 0,
        [this.customRequiredValidator()],
      ],
      total: [this.data?.total ?? 0],
      remark: [this.data?.remark ?? ""],
      deletedGoodsReceiveNoteDetailIds: [""],
      goodsReceiveNoteDetails: this.fb.array([]),
    });
    this.setMinMaxDates();
  }

  setMinMaxDates() {
    if (this.applyMinMax) {
      this.goodsReceiveNoteForm
        .get("grndate")
        .setValidators([
          Validators.required,
          inFinancialYearValidator(
            this.financialYearStartDate,
            this.financialYearEndDate
          ),
        ]);
    } else {
      this.goodsReceiveNoteForm
        .get("grndate")
        .setValidators([Validators.required]);
    }

    this.goodsReceiveNoteForm.get("grndate").updateValueAndValidity();
  }

  get goodsReceiveNoteDetails(): FormArray {
    return this.goodsReceiveNoteForm.get(
      "goodsReceiveNoteDetails"
    ) as FormArray;
  }

  populateForm(): void {
    if (this.goodsReceiveNoteForm.get("id").value) {
      this.populateGoodsReceiveNoteDetails(this.data);
    } else {
      this.addItem();
    }
  }

  setFormTitle(): void {
    if (this.goodsReceiveNoteForm.get("id").value) {
      this.formTitle = "Edit Goods Receive Note";
    } else {
      this.formTitle = "Add Goods Receive Note";
    }
  }

  populateGoodsReceiveNoteDetails(data: GoodsReceiveNoteResponseDTO) {
    data.goodsReceiveNoteDetails.forEach(
      (item: GoodsReceiveNoteResponseDetail) => this.addItem(item)
    );
  }

  addItem(item?: GoodsReceiveNoteResponseDetail): void {
    this.goodsReceiveNoteDetails.push(this.createGoodsReceiveNoteDetail(item));
  }

  createGoodsReceiveNoteDetail(
    item?: GoodsReceiveNoteResponseDetail
  ): FormGroup {
    return this.fb.group({
      id: [item?.id ?? null],
      purchaseOrderDetailId: [item?.purchaseOrderDetailId ?? null],
      productId: [item?.productId ?? null, Validators.required],
      product: [item?.product ?? null],
      measurementUnitName: [item?.product?.measurementUnit?.name ?? ""],
      poquantity: [item?.poquantity ?? 0],
      grnquantity: [item?.grnquantity, Validators.required],
      bagWeightDeductionQuantity: [item?.bagWeightDeductionQuantity ?? 0],
      numberOfBagQuantity: [item?.numberOfBagQuantity ?? 0],
      netQuantity: [item?.netQuantity ?? 0],
      batchNo: [item?.batchNo ?? ""],
      expiryDate: [item?.expiryDate ?? null],
      receivedQuantity: [item?.receivedQuantity ?? 0],
      rejectedQuantity: [item?.rejectedQuantity ?? 0],
      rejectionReason: [item?.rejectionReason ?? ""],
      currencyRate: [item?.currencyRate ? item?.currencyRate : 0],
      rate: [item?.rate, Validators.required],
      rateAfterBagWeightDeduction: [item?.rateAfterBagWeightDeduction ?? 0],
      currencyAmount: [item?.currencyAmount || 0],
      amount: [item?.amount],
    });
  }

  //this method will apply custom validation on transportationCost field
  customRequiredValidator() {
    return (control) => {
      const value = control.value;
      //this.goodsReceiveNoteForm?.get("transport")?.value can be two types. one is 1 means Party & two is 2 means Own_Transport. It's an enum value.
      if (this.goodsReceiveNoteForm?.get("transport")?.value == 2) {
        if (value < 1) {
          return { customRequired: true };
        }
      }
      return null;
    };
  }

  getImportPurchaseValue(): boolean {
    return this.goodsReceiveNoteForm?.get("isImportPurchase")?.value || false;
  }

  getAllPurchaseOrders(): void {
    this.purchaseOrderService.getAllPurchaseOrders().subscribe((res) => {
      this.purchaseOrders = res?.data?.item1;
    });
  }

  getAllStores(businessType: string): void {
    let storeRequest = new StoreRequest();
    storeRequest.page = -1;
    if (businessType === "1") {
      this.storeService.getStores(storeRequest).subscribe((res) => {
        this.filterStores = this.stores = res?.data?.item1;
      });
    }
    if (businessType === "2") {
      storeRequest.inventoryTypeId = Inventory_Type_Id_Raw_Materials;
      this.storeService.getStores(storeRequest).subscribe((res) => {
        this.filterStores = this.stores = res?.data?.item1;
      });
    }
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    if (fieldName === "supplierId") {
      this.goodsReceiveNoteForm?.get("supplierId").setValue(null);
    }
    if (fieldName === "storeId") {
      this.goodsReceiveNoteForm?.get("storeId").setValue(null);
    }
  }

  clearInputFromDetails(evt: any, itemIndex: number): void {
    evt.stopPropagation();
    const particularDetail = this.goodsReceiveNoteDetails.at(itemIndex);
    particularDetail?.patchValue({
      productId: null,
    });
  }

  handleSupplierSearch(event: any): void {
    const name = event.target?.name;
    if (name === "supplierId") {
      const term = this.goodsReceiveNoteForm.get("supplierId");
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
      const term = this.goodsReceiveNoteForm.get("storeId");
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

  getAllSuppliers(): void {
    this.supplierService.getAllSuppliers().subscribe((res) => {
      this.filterSuppliers = this.suppliers = res?.data?.item1;
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
      this.goodsReceiveNoteDetailsData = this.goodsReceiveNoteForm.get(
        "goodsReceiveNoteDetails"
      ).value;
      this.setTransactionalJournalAccountsData();
    }
  }

  setTransactionalJournalAccountsData() {
    if (this.goodsReceiveNoteForm) {
      this.transactionalJournalAccounts = [
        {
          accountName: "Purchase Account",
          column: "credit",
          value: this.goodsReceiveNoteForm?.get("subtotal")?.value,
        },
        {
          accountName: "Transportation Cost",
          column: "credit",
          value: this.goodsReceiveNoteForm?.get("transportationCost")?.value,
        },
        {
          accountName: "RM Inventory Account",
          column: "debit",
          value: this.goodsReceiveNoteForm?.get("total")?.value,
        },
        {
          accountName: "",
          row: "total",
          value: this.goodsReceiveNoteForm?.get("total")?.value,
        },
      ];
    }
  }

  handleProductSearch(event: any, itemIndex: number): void {
    const name = event?.target?.name;
    if (name === "productId") {
      const term = this.goodsReceiveNoteDetails.at(itemIndex).get("productId");
      this.filterProduct(term?.value || "");
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

  findProductById(id) {
    return this.products?.find((product) => product.id === id);
  }

  handleProductSelection(event, index) {
    const particularDetail = this.goodsReceiveNoteDetails.at(index);
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
    const isProductAdded = this.goodsReceiveNoteDetails.value.some(
      (item, i) => {
        return item.productId === productId && i !== index;
      }
    );
    return isProductAdded;
  }

  getProductName(productId: string) {
    if (!productId) {
      return;
    }
    const product =
      this.products?.find((product) => product?.id === productId) ||
      this.data?.goodsReceiveNoteDetails?.find((x) => x?.productId == productId)
        ?.product;
    if (!product) return "";
    return (
      product?.name +
      (this.businessType === "2" ? "" : ` (${product?.packSize?.name})`)
    );
  }

  calculateCost() {
    const cost = this.goodsReceiveNoteDetails?.value?.reduce(
      (sum, item) => sum + item?.grnquantity * item?.rate,
      0
    );
    const discount = this.goodsReceiveNoteForm.get("discount")?.value;
    const transportationCost =
      this.goodsReceiveNoteForm.get("transportationCost")?.value;
    this.goodsReceiveNoteForm.get("subtotal").setValue(cost);
    this.goodsReceiveNoteForm
      .get("total")
      .setValue(cost - discount + transportationCost);
  }

  onControlChange(event: any, itemIndex?: number): void {
    const name = event?.target?.name;
    const item = this.goodsReceiveNoteDetails.at(itemIndex);
    if (name === "grnquantity" || name === "rate") {
      this.calculateAmount(itemIndex);
      this.onControlBagWeightDeductionQtyChange(itemIndex);
    }
    if (name === "discount" || name === "transportationCost") {
      this.calculateCost();
    }
    if (name == "rejectedQuantity") {
      if (item.get("rejectedQuantity").value == null) {
        item.get("rejectedQuantity").setValue(0);
      }
    }
    if (name == "numberOfBagQuantity") {
      if (item.get("numberOfBagQuantity").value == null) {
        item.get("numberOfBagQuantity").setValue(0);
      }
    }
  }

  calculateAmount(itemIndex: number) {
    const item = this.goodsReceiveNoteDetails.at(itemIndex);
    const isImportPurchase = this.getImportPurchaseValue();
    const grnquantity = item.get("grnquantity");
    const ratePerUnit = item.get("rate")?.value;
    const amount = item.get("amount");
    const currencyAmount = item.get("currencyAmount");
    if (isImportPurchase) {
      const currencyRate = item.get("currencyRate")?.value;
      currencyAmount?.setValue(grnquantity?.value * currencyRate);
    }
    amount?.setValue(grnquantity?.value * ratePerUnit);
    this.goodsReceiveNoteForm.get("discount").setValue(0);
    this.goodsReceiveNoteForm.get("transportationCost")?.setValue(0);
    this.calculateCost();
  }

  onControlBagWeightDeductionQtyChange(itemIndex: number) {
    const item = this.goodsReceiveNoteDetails.at(itemIndex);
    const grnquantity = item.get("grnquantity")?.value;
    const bagWeightDeductionQuantity = item.get(
      "bagWeightDeductionQuantity"
    )?.value;
    const netQuantity = item.get("netQuantity");
    const rate = item.get("rate")?.value;
    const rateAfterBagWeightDeduction = item.get("rateAfterBagWeightDeduction");
    netQuantity?.setValue(grnquantity - bagWeightDeductionQuantity);
    if (grnquantity - bagWeightDeductionQuantity == 0)
      rateAfterBagWeightDeduction?.setValue(rate);
    else {
      rateAfterBagWeightDeduction?.setValue(
        (grnquantity * rate) / (grnquantity - bagWeightDeductionQuantity)
      );
    }
  }

  deletedIds: string = "";
  onDeleteItem(id: string, itemIndex: number): void {
    if (id) this.deletedIds += `${id},`;
    this.goodsReceiveNoteDetails.removeAt(itemIndex);
    this.calculateCost();
  }

  private handleSuccessfulSave(res: GeneralResponse<string>): void {
    if (res?.succeeded) {
      this.handleGoodsReceiveNoteResponse(res?.data);
      this.toastr.success(res?.message);
      this.deletedIds = "";
    } else {
      this.isLoading = false;
      this.toastr.error(res?.message);
    }
  }

  private handleGoodsReceiveNoteResponse(goodsReceiveNoteId: string): void {
    this.goodsReceiveNoteService
      .getGoodsReceiveNoteById(goodsReceiveNoteId)
      .subscribe({
        next: (goodsReceiveNoteResponse) => {
          this.data = goodsReceiveNoteResponse.data;
          this.isViewMode = true;
          this.isLoading = false;
          this.navigateToView(goodsReceiveNoteResponse?.data?.id);
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

  addGoodsReceiveNote(body): void {
    this.goodsReceiveNoteService
      .createGoodsReceiveNote(body)
      .subscribe((res) => {
        // this.data.status = 1;
        this.handleSuccessfulSave(res);
      });
  }

  updateGoodsReceiveNote(body): void {
    this.goodsReceiveNoteService
      .updateGoodsReceiveNote(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  onSubmit() {
    if (this.goodsReceiveNoteForm.valid) {
      this.isLoading = true;
      const formValue = this.goodsReceiveNoteForm?.value;
      if (!formValue.id) {
        this.addGoodsReceiveNote(formValue);
      } else {
        formValue.deletedGoodsReceiveNoteDetailIds = this.deletedIds;
        this.updateGoodsReceiveNote(formValue);
      }
    }
  }

  //openPurchaseOrderDialog
  openPurchaseOrderDialog() {
    const dialogRef = this.dialog.open(PurchaseOrderListComponent, {
      disableClose: true,
      panelClass: "add-bill-container",
      minHeight: "auto",
      height: "auto",
      data: this.goodsReceiveNoteForm.get("isImportPurchase")?.value,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.isFieldReadonly = true;
        this.goodsReceiveNoteForm.reset();

        this.goodsReceiveNoteForm.setControl(
          "goodsReceiveNoteDetails",
          this.fb.array([])
        );

        const totalReceivedAmount = result.purchaseOrderDetails.reduce(
          (acc, o) => acc + o.receivedQuantity * o.rate,
          0
        );

        this.goodsReceiveNoteForm.markAllAsTouched();

        this.goodsReceiveNoteForm.patchValue({
          grndate: this.dateFormatService.getPresentDate(),
          ponumber: result?.ponumber,
          supplierId: result?.supplierId,
          storeId: result?.storeId,
          transport: result?.transport,
          subtotal: result?.subtotal - totalReceivedAmount,
          discount: result?.discount,
          transportationCost: 0,
          total: result?.total - totalReceivedAmount,
          paymentTermInDays: result?.paymentTermInDays,
          isImportPurchase: result?.isImportPurchase,
          proformaInvoiceNo: result?.proformaInvoiceNo,
          lcNumber: result?.lcNumber,
          exchangeRate: result?.exchangeRate,
          currencyId: result?.currencyId,
          importPurchaseIncoTerm: result?.importPurchaseIncoTerm,
          importPurchasePaymentTerm: result?.importPurchasePaymentTerm,
          portOfLoading: result?.portOfLoading,
          portOfDestination: result?.portOfDestination,
          purchaseOrderTotal: result?.subtotal,
        });

        result.purchaseOrderDetails.forEach(
          (item: PurchaseOrderResponseDetail) => {
            let grnResponseDetail: any = {
              purchaseOrderDetailId: item?.id,
              productId: item?.productId,
              product: item?.product,
              poquantity: item?.quantity,
              grnquantity: item?.quantity - item?.receivedQuantity,
              bagWeightDeductionQuantity: 0,
              netQuantity: item?.quantity - item?.receivedQuantity,
              receivedQuantity: item?.receivedQuantity,
              currencyRate: item?.currencyRate,
              rate: item?.rate,
              rateAfterBagWeightDeduction: item?.rate,
              amount: (item?.quantity - item?.receivedQuantity) * item?.rate,
              currencyAmount:
                (item?.quantity - item?.receivedQuantity) * item?.currencyRate,
            };
            this.addItem(grnResponseDetail);
          }
        );
      }
    });
  }

  getGoodsReceiveNoteStatus(value) {
    return this.statusColorService.getGoodsReceiveNoteStatus(value);
  }

  getGoodsReceiveNoteName(value: number) {
    return GRNStatus[value];
  }

  check(id: string) {
    if (this.isLoading) return;
    this.isLoading = true;

    this.goodsReceiveNoteService
      .checkGoodsReceiveNote(id)
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

    this.goodsReceiveNoteService
      .approveGoodsReceiveNote(id)
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
    this.goodsReceiveNoteService
      .unpostGoodsReceiveNote(id, status)
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
      .get(environment.apiURL + "/ReportGoodsReceiveNote/" + id, {
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
