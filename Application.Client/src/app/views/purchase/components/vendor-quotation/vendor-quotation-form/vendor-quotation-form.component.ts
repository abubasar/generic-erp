import { ChangeDetectorRef, Component, OnInit, ViewChild } from "@angular/core";
import { FormArray, FormBuilder, FormGroup, Validators } from "@angular/forms";
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
import { Transport } from "app/shared/enums/transport";
import { VendorQuotationStatus } from "app/shared/enums/vendorQuotationStatus";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { SnackBarService } from "app/shared/services/snack-bar.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
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
import { PurchaseRequisitionResponseDetail } from "app/views/purchase/models/purchase-requisition/purchase-requisition-response-dto.model";
import { VendorQuotationRequestDTO } from "app/views/purchase/models/vendor-quotation/vendor-quotation-request-dto.model";
import {
  VendorQuotationResponseDTO,
  VendorQuotationResponseDetail,
} from "app/views/purchase/models/vendor-quotation/vendor-quotation-response-dto.model";
import { VendorQuotationService } from "app/views/purchase/services/vendor-quotation.service";
import { ToastrService } from "ngx-toastr";
import { finalize } from "rxjs";
import { PurchaseRequisitionListComponent } from "../purchase-requisition-list/purchase-requisition-list.component";

@Component({
  selector: "app-vendor-quotation-form",
  templateUrl: "./vendor-quotation-form.component.html",
  styleUrls: ["./vendor-quotation-form.component.scss"],
})
export class VendorQuotationFormComponent implements OnInit {
  @ViewChild("stepper") private stepper: MatStepper;
  isLoading: boolean = false;
  isViewMode: boolean = true;
  isChecked: boolean;
  formTitle: string;
  vendorQuotationForm: FormGroup;
  stores: Store[];
  suppliers: Supplier[];
  products: ProductView[];
  searchProducts: ProductView[];
  vendorQuotationDetailsData: any[] = [];
  transports: ENUM[];
  paymentModes: ENUM[];
  importPurchaseIncoTerms: ENUM[];
  importPurchasePaymentTerms: ENUM[];
  currencies: Currency[];

  data: VendorQuotationResponseDTO;
  businessType: string;
  // Define financial year date range here
  financialYearStartDate: Date; // Jul 1, 2023
  financialYearEndDate: Date; // Jun 30, 2024
  currentFinancialYearId: string;

  private path = {
    list: "purchase/manage-vendor-quotation",
    edit: "purchase/manage-vendor-quotation",
  };

  constructor(
    public dialog: MatDialog,
    public statusColorService: StatusColorService,
    private vendorQuotationService: VendorQuotationService,
    private storeService: StoreService,
    private supplierService: SupplierService,
    private enumValueService: EnumValueService,
    private currencyService: CurrencyService,
    private productService: ProductService,
    public dateFormatService: DateTimeFormatService,
    private jwtAuth: JwtAuthService,
    private fb: FormBuilder,
    private toastr: ToastrService,
    private activatedRoute: ActivatedRoute,
    private router: Router,
    // private localStorageService: LocalStoreService,
    private cdRef: ChangeDetectorRef,
    private snackBarService: SnackBarService
  ) {}

  ngOnInit(): void {
    this.activatedRoute.data.subscribe((response: any) => {
      this.data = response?.vendorQuotation?.data;
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
    const id = this.activatedRoute.snapshot.paramMap.get("id");
    if (id) {
      this.stepper.selectedIndex = 1;
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
    this.getAllStores();
    this.getAllSuppliers();
    this.getAllTransports();
    this.getAllPaymentModes();
    this.getAllCurrencies();
    this.getAllImportPurchaseIncoTerms();
    this.getAllImportPurchasePaymentTerms();
    //! it's important after close openPurchaseRequisitionDialog
    this.getAllProducts();
  }

  initializeForm() {
    this.createForm();
    this.populateForm();
    this.setFormTitle();
  }

  createForm(): void {
    this.vendorQuotationForm = this.fb.group({
      id: [this.data?.id || null],
      requisitionNo: [this.data?.requisitionNo || ""],
      referenceNo: [this.data?.referenceNo || ""],
      storeId: [this.data?.storeId, Validators.required],
      supplierId: [this.data?.supplierId, Validators.required],
      transport: [this.data?.transport || 0],
      paymentMode: [this.data?.paymentMode || 0],
      paymentTermInDays: [this.data?.paymentTermInDays || 0],
      deliveryTermInDays: [this.data?.deliveryTermInDays || 0],
      deliveryDate: [this.data?.deliveryDate, Validators.required],
      currencyId: [this.data?.currencyId || null],
      importPurchaseIncoTerm: [this.data?.importPurchaseIncoTerm || 0],
      importPurchasePaymentTerm: [this.data?.importPurchasePaymentTerm || 0],
      termAndCondition: [this.data?.termAndCondition || ""],
      totalAmount: [this.data?.totalAmount, Validators.required],
      remark: [this.data?.remark || ""],
      deletedVendorQuotationDetailIds: [""],
      vendorQuotationDetails: this.fb.array([]),
    });
  }

  get vendorQuotationDetails(): FormArray {
    return this.vendorQuotationForm.get("vendorQuotationDetails") as FormArray;
  }

  populateForm(): void {
    if (this.vendorQuotationForm.get("id").value) {
      this.populateVendorQuotationDetails(this.data);
    } else {
      this.addItem();
    }
  }

  setFormTitle() {
    if (this.vendorQuotationForm.get("id").value) {
      this.formTitle = "Edit Vendor Quotation";
    } else {
      this.formTitle = "Add Vendor Quotation";
    }
  }

  populateVendorQuotationDetails(data: VendorQuotationResponseDTO) {
    data.vendorQuotationDetails.forEach((item: VendorQuotationResponseDetail) =>
      this.addItem(item)
    );
  }

  addItem(item?: VendorQuotationResponseDetail): void {
    this.vendorQuotationDetails.push(this.createVendorQuotationDetail(item));
  }

  createVendorQuotationDetail(item?: VendorQuotationResponseDetail): FormGroup {
    return this.fb.group({
      id: [item?.id || null],
      productId: [item?.productId || "", Validators.required],
      product: [item?.product || ""],
      measurementUnitName: [
        item?.product?.measurementUnit?.name || "",
        Validators.required,
      ],
      quantity: [item?.quantity || "", Validators.required],
      rate: [item?.rate || "", Validators.required],
      amount: [item?.amount || "", Validators.required],
    });
  }

  getAllStores(): void {
    let storeRequest = new StoreRequest();
    storeRequest.page = -1;
    storeRequest.inventoryTypeId = Inventory_Type_Id_Raw_Materials;
    this.storeService.getStores(storeRequest).subscribe((res) => {
      this.stores = res?.data?.item1;
    });
  }

  getAllSuppliers(): void {
    this.supplierService.getAllSuppliers().subscribe((res) => {
      this.suppliers = res?.data?.item1;
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

  getStoreName(storeId) {
    return this.stores?.find((x) => x?.id === storeId)?.name;
  }

  getSupplierName(supplierId) {
    return this.suppliers?.find((x) => x?.id === supplierId)?.name;
  }

  getTransportName(value) {
    return Transport[value];
  }

  getPaymentModeName(value: number) {
    return PaymentMode[value];
  }

  isPaymentModeCredit() {
    return (
      this.vendorQuotationForm.get("paymentMode").value === Payment_Mode_Credit
    );
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

  getProductName(productId: string) {
    if (productId) {
      const product =
        this.products?.find((product) => product?.id === productId) ||
        this.data?.vendorQuotationDetails?.find(
          (x) => x?.productId == productId
        )?.product;
      if (!product) return "";
      return (
        product?.name +
        (this.businessType === "2" ? "" : ` (${product?.packSize?.name})`)
      );
    }
  }

  getVendorQuotationStatus(value) {
    return this.statusColorService.getVendorQuotationStatus(value);
  }

  getVendorQuotationStatusName(value: number) {
    return VendorQuotationStatus[value];
  }

  isPassingDataToNextStep = false;
  handleStepChange(event) {
    this.isPassingDataToNextStep = event.selectedIndex === 1;
    if (this.isPassingDataToNextStep) {
      this.vendorQuotationDetailsData = this.vendorQuotationForm.get(
        "vendorQuotationDetails"
      ).value;
    }
  }

  findProductById(id) {
    return this.products.find((product) => product?.id === id);
  }

  handleProductSelection(event, index) {
    const particularDetail = this.vendorQuotationDetails.at(index);
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
    });
    this.calculateAmount(index);
  }

  showSnackBar(message: string) {
    this.snackBarService.openSnackbar(message);
  }

  isExistSelectedProduct(productId: string, index: number): boolean {
    const isProductAdded = this.vendorQuotationDetails.value.some((item, i) => {
      return item.productId === productId && i !== index;
    });
    return isProductAdded;
  }

  clearInputFromDetails(evt: any, itemIndex: number): void {
    evt.stopPropagation();
    const particularDetail = this.vendorQuotationDetails.at(itemIndex);
    particularDetail?.patchValue({
      productId: null,
    });
  }

  handleProductSearch(event: any, itemIndex: number): void {
    const name = event?.target?.name;
    if (name === "productId") {
      const term = this.vendorQuotationDetails.at(itemIndex).get("productId");
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
    const cost = this.vendorQuotationDetails.value.reduce(
      (sum, item) => sum + item.quantity * item.rate,
      0
    );
    this.vendorQuotationForm.get("totalAmount").setValue(cost);
  }

  onControlChange(event: any, itemIndex: number): void {
    const name = event?.target?.name;
    if (name === "quantity" || name === "rate") {
      this.calculateAmount(itemIndex);
    }
  }

  calculateAmount(itemIndex: number) {
    const item = this.vendorQuotationDetails.at(itemIndex);
    const quantity = item.get("quantity")?.value;
    const ratePerUnit = item.get("rate")?.value;
    const amount = item.get("amount");
    amount?.setValue(quantity * ratePerUnit);
    this.calculateCost();
  }

  deletedIds: string = "";
  onDeleteItem(id: string, itemIndex: number): void {
    if (id) this.deletedIds += `${id},`;
    this.vendorQuotationDetails.removeAt(itemIndex);
    this.calculateCost();
  }

  private handleSuccessfulSave(res: GeneralResponse<string>): void {
    if (res?.succeeded) {
      this.handleVendorQuotationResponse(res?.data);
      this.toastr.success(res?.message);
      this.deletedIds = "";
    } else {
      this.isLoading = false;
      this.toastr.error(res?.message);
    }
  }

  private handleVendorQuotationResponse(vendorQuotationId: string): void {
    this.vendorQuotationService
      .getVendorQuotationById(vendorQuotationId)
      .subscribe({
        next: (vendorQuotationResponse) => {
          this.data = vendorQuotationResponse.data;
          this.isViewMode = true;
          this.isLoading = false;
          this.navigateToView(vendorQuotationResponse?.data?.id);
        },
        error: (err) => {
          location.reload();
        },
      });
  }

  private navigateToView(id: string) {
    this.router.navigate([this.path.edit, id]);
  }

  navigateToList() {
    this.router.navigate([this.path.list]);
  }

  backToView() {
    this.initializeForm();
    this.stepper.next();
    this.isViewMode = true;
  }

  unpost(requisitionNo: string) {
    this.isLoading = true;
    this.vendorQuotationService
      .unpostVendorQuotation(requisitionNo)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (res) => {
          if (res?.succeeded) {
            this.data.status = 1;
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

  addVendorQuotation(body: VendorQuotationRequestDTO): void {
    this.vendorQuotationService
      .createVendorQuotation(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  updateVendorQuotation(body: VendorQuotationRequestDTO): void {
    this.vendorQuotationService
      .updateVendorQuotation(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  onSubmit() {
    if (this.vendorQuotationForm.valid) {
      this.isLoading = true;
      const formValue = this.vendorQuotationForm?.value;
      if (!formValue.id) {
        this.addVendorQuotation(formValue);
      } else {
        formValue.deletedVendorQuotationDetailIds = this.deletedIds;
        this.updateVendorQuotation(formValue);
      }
    }
  }

  // openPurchaseRequisitionDialog
  openPurchaseRequisitionDialog() {
    const dialogRef = this.dialog.open(PurchaseRequisitionListComponent, {
      disableClose: true,
      panelClass: "add-bill-container",
      // minHeight: "calc(100vh - 90px)",
      minHeight: "auto",
      height: "auto",
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.vendorQuotationForm.reset();
        this.vendorQuotationForm.setControl(
          "vendorQuotationDetails",
          this.fb.array([])
        );

        this.vendorQuotationForm.markAllAsTouched();

        const formData = {
          requisitionNo: result?.requisitionNo,
          transport: result?.transport,
          paymentMode: result?.paymentMode,
          storeId: result?.storeId,
          paymentTermInDays: result?.paymentTermInDays,
          deliveryTermInDays: 0,
          deliveryDate: result?.expectedDeliveryDate,
          termAndCondition: result?.termAndCondition,
          remark: result?.remark,
          currencyId: result?.currencyId,
          importPurchaseIncoTerm: result?.importPurchaseIncoTerm,
          importPurchasePaymentTerm: result?.importPurchasePaymentTerm,
          totalAmount: result?.totalAmount,
        };
        this.vendorQuotationForm.patchValue(formData);

        result.purchaseRequisitionDetails.forEach(
          (item: PurchaseRequisitionResponseDetail) => {
            const vendorQuotationResponseDetail: VendorQuotationResponseDetail =
              {
                productId: item?.productId,
                product: item?.product,
                quantity: item?.quantity,
                rate: item?.rate,
                amount: item?.amount,
              };
            this.addItem(vendorQuotationResponseDetail);
          }
        );
      }
    });
  }
}
