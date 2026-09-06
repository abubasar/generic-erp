import { ChangeDetectorRef, Component, OnInit, ViewChild } from "@angular/core";
import { FormArray, FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MatButton } from "@angular/material/button";
import { MatDialog } from "@angular/material/dialog";
import { MatStepper } from "@angular/material/stepper";
import { ActivatedRoute, Router } from "@angular/router";
import { Payment_Mode_Credit } from "app/shared/consts/const";
import { ImportPurchaseIncoTerm } from "app/shared/enums/importPurchaseIncoTerm";
import { ImportPurchasePaymentTerm } from "app/shared/enums/importPurchasePaymentTerm";
import { PaymentMode } from "app/shared/enums/paymentMode";
import { Priority } from "app/shared/enums/priority";
import { RequisitionStatus } from "app/shared/enums/requisitionStatus";
import { Transport } from "app/shared/enums/transport";
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
import { Currency } from "app/views/configuration/models/currency/currency.model";
import { Department } from "app/views/configuration/models/department/department.model";
import { ProductRequest } from "app/views/configuration/models/product/product-request.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { Store } from "app/views/configuration/models/store/store.model";
import { CurrencyService } from "app/views/configuration/services/currency.service";
import { DepartmentService } from "app/views/configuration/services/department.service";
import { ProductService } from "app/views/configuration/services/product.service";
import { StoreService } from "app/views/configuration/services/store.service";
import {
  PurchaseRequisitionRequestDTO,
  PurchaseRequisitionRequestDetail,
} from "app/views/purchase/models/purchase-requisition/purchase-requisition-request-dto.model";
import {
  PurchaseRequisitionResponseDTO,
  PurchaseRequisitionResponseDetail,
} from "app/views/purchase/models/purchase-requisition/purchase-requisition-response-dto.model";
import { PurchaseRequisitionService } from "app/views/purchase/services/purchase-requisition.service";
import { StockService } from "app/views/report/services/stock.service";
import { ToastrService } from "ngx-toastr";
import { finalize } from "rxjs";
import { SendRFQToVendorEmailComponent } from "../send-rfq-to-vendor-email/send-rfq-to-vendor-email.component";
@Component({
  selector: "app-send-rfq-to-vendor-form",
  templateUrl: "./send-rfq-to-vendor-form.component.html",
  styleUrls: ["./send-rfq-to-vendor-form.component.scss"],
})
export class SendRFQToVendorFormComponent implements OnInit {
  @ViewChild("btnCheck") btnCheck: MatButton;
  @ViewChild("btnApprove") btnApprove: MatButton;
  @ViewChild("stepper") private stepper: MatStepper;
  isLoading: boolean = false;
  isDialogOpen: boolean = false;
  isViewMode: boolean = false;
  isChecked: boolean;
  formTitle: string;
  purchaseRequisitionForm: FormGroup;
  stores: Store[];
  departments: Department[];
  priorities: ENUM[];
  transports: ENUM[];
  paymentModes: ENUM[];
  importPurchaseIncoTerms: ENUM[];
  importPurchasePaymentTerms: ENUM[];
  currencies: Currency[];
  products: ProductView[];
  searchProducts: ProductView[];
  purchaseRequisitionDetailsData: any[] = [];
  data: PurchaseRequisitionResponseDTO;
  selectedItemCurrentStockQuantity: number;
  currentFinancialYearId: string;

  private path = {
    list: "purchase/send-rfq-to-vendor",
  };

  constructor(
    public dialog: MatDialog,
    private fb: FormBuilder,
    private storeService: StoreService,
    private departmentService: DepartmentService,
    private enumValueService: EnumValueService,
    private currencyService: CurrencyService,
    private productService: ProductService,
    private purchaseRequisitionService: PurchaseRequisitionService,
    private dateFormatService: DateTimeFormatService,
    private jwtAuth: JwtAuthService,
    private toastr: ToastrService,
    private activatedRoute: ActivatedRoute,
    private router: Router,
    // private localStorageService: LocalStoreService,
    private stockService: StockService,
    private cdRef: ChangeDetectorRef,
    public statusColorService: StatusColorService,
    private snackBarService: SnackBarService,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.activatedRoute.data.subscribe((response: any) => {
      console.log("DATA FETCHING", response);
      this.data = response?.purchaseRequisition?.data;
    });
    this.getData();
    this.jwtAuth.userProfile.subscribe((res: UserProfile) => {
      this.currentFinancialYearId = res.fyid;
    });
    this.initializeForm();
  }

  ngAfterViewInit() {
    const id = this.activatedRoute.snapshot.paramMap.get("id");
    if (id) {
      if (this.data?.requisitionStatus == 4) {
        this.stepper.selectedIndex = 1;
        this.isViewMode = true;
        this.btnCheck.focus();
        this.btnApprove.focus();
      }
    } else {
      this.isViewMode = !this.isViewMode;
    }
    this.cdRef.detectChanges();
  }

  // ngOnDestroy() {
  //   this.localStorageService.removeItem(
  //     this.activatedRoute.snapshot.paramMap.get("id")
  //   );
  // }

  showEdit() {
    this.stepper.previous();
    this.isViewMode = !this.isViewMode;
  }

  getData() {
    // const id = this.activatedRoute.snapshot.paramMap.get("id");
    // if (id) this.data = this.localStorageService.getItem(id);
    // this.isViewMode = this.route.snapshot.data["title"].includes("Add");
    this.getAllProducts();
    this.getAllDepartments();
    this.getAllStores();
    this.getAllPriorities();
    this.getAllTransports();
    this.getAllPaymentModes();
    this.getAllCurrencies();
    this.getAllImportPurchaseIncoTerms();
    this.getAllImportPurchasePaymentTerms();
  }

  initializeForm() {
    this.createForm();
    this.populateForm();
    this.setFormTitle();
  }

  createForm(): void {
    this.purchaseRequisitionForm = this.fb.group({
      id: [this.data?.id || null],
      requisitionNo: [this.data?.requisitionNo || ""],
      requisitionDate: [
        this.data?.requisitionDate || null,
        Validators.required,
      ],
      storeId: [this.data?.storeId, Validators.required],
      departmentId: [this.data?.departmentId, Validators.required],
      expectedDeliveryDate: [this.data?.expectedDeliveryDate || null],
      priority: [this.data?.priority, Validators.required],
      paymentTermInDays: [
        this.data?.paymentTermInDays || 0,
        Validators.required,
      ],
      transport: [this.data?.transport || 0, Validators.required],
      paymentMode: [this.data?.paymentMode || 0, Validators.required],
      termAndCondition: [this.data?.termAndCondition || ""],
      currencyId: [this.data?.currencyId || null],
      importPurchaseIncoTerm: [this.data?.importPurchaseIncoTerm || 0],
      importPurchasePaymentTerm: [this.data?.importPurchasePaymentTerm || 0],
      //  totalAmount: [this.data?.totalAmount, Validators.required],
      remark: [this.data?.remark || ""],
      deletedPurchaseRequisitionDetailIds: [""],
      purchaseRequisitionDetails: this.fb.array([]),
    });
  }

  get purchaseRequisitionDetails(): FormArray {
    return this.purchaseRequisitionForm.get(
      "purchaseRequisitionDetails"
    ) as FormArray;
  }

  populateForm(): void {
    if (this.purchaseRequisitionForm.get("id").value) {
      this.populatePurchaseRequisitionDetails(this.data);
      this.purchaseRequisitionForm.markAllAsTouched();
    } else {
      this.addItem();
    }
  }

  setFormTitle(): void {
    if (this.purchaseRequisitionForm.get("id").value) {
      this.formTitle = "Edit Purchase Requisition";
    } else {
      this.formTitle = "Add Purchase Requisition";
    }
  }

  populatePurchaseRequisitionDetails(
    data: PurchaseRequisitionResponseDTO
  ): void {
    data.purchaseRequisitionDetails.forEach(
      (item: PurchaseRequisitionResponseDetail) => this.addItem(item)
    );
  }

  addItem(item?: PurchaseRequisitionResponseDetail): void {
    this.purchaseRequisitionDetails.push(
      this.createPurchaseRequisitionDetail(item)
    );
  }

  createPurchaseRequisitionDetail(
    item?: PurchaseRequisitionRequestDetail
  ): FormGroup {
    return this.fb.group({
      id: [item?.id || null],
      productId: [item?.productId || "", Validators.required],
      product: [item?.product || "", Validators.required],
      measurementUnitName: [
        item?.product?.measurementUnit?.name || "",
        Validators.required,
      ],
      quantity: [item?.quantity, Validators.required],
      //rate: [item?.rate, Validators.required],
      // amount: [item?.amount, Validators.required],
      lastPurchaseRate: [item?.lastPurchaseRate || 0],
      alertQuantity: [item?.alertQuantity || 0],
      currentStockQuantity: [item?.currentStockQuantity || 0],
    });
  }

  isPassingDataToNextStep = false;
  handleStepChange(event: any) {
    this.isPassingDataToNextStep = event.selectedIndex === 1;
    if (this.isPassingDataToNextStep) {
      this.purchaseRequisitionDetailsData = this.purchaseRequisitionForm.get(
        "purchaseRequisitionDetails"
      ).value;
    }
  }

  getRequisitionStatus(value) {
    return this.statusColorService.getRequisitionStatus(value);
  }

  getRequisitionStatusName(value: number) {
    return RequisitionStatus[value];
  }

  // calculateCost() {
  //   const cost = this.purchaseRequisitionDetails.value.reduce(
  //     (sum: number, item: { quantity: number; rate: number }) =>
  //       sum + item.quantity * item.rate,
  //     0
  //   );
  //   this.purchaseRequisitionForm.get("totalAmount").setValue(cost);
  // }

  // onControlChange(event: any, itemIndex: number): void {
  //   const name = event?.target?.name;
  //   if (name === "quantity" || name === "rate") {
  //     this.calculateAmount(itemIndex);
  //   }
  // }

  // calculateAmount(itemIndex: number) {
  //   const item = this.purchaseRequisitionDetails.at(itemIndex);
  //   const quantity = item.get("quantity")?.value;
  //   const ratePerUnit = item.get("rate")?.value;
  //   const amount = item.get("amount");
  //   amount?.setValue(quantity * ratePerUnit);
  //   this.calculateCost();
  // }

  getAllStores(): void {
    this.storeService.getAllStores().subscribe((res) => {
      this.stores = res?.data?.item1;
    });
  }

  getAllDepartments(): void {
    this.departmentService.getAllDepartments().subscribe((res) => {
      this.departments = res?.data?.item1;
    });
  }

  getAllPriorities() {
    this.enumValueService.getPriorities().subscribe((res) => {
      this.priorities = res;
    });
  }

  getAllProducts(): void {
    let productRequest = new ProductRequest();
    //productRequest.inventoryTypeId = Inventory_Type_Id_Raw_Materials;
    productRequest.isPurchaseProduct = true;
    productRequest.page = -1;
    this.productService.getProducts(productRequest).subscribe((res) => {
      this.products = res?.data?.item1;
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

  getCurrencyName(currencyId: string) {
    return this.currencies?.find((x) => x.id === currencyId)?.name;
  }

  getDepartmentName(departmentId: string) {
    return this.departments?.find((x) => x.id === departmentId).name;
  }

  getStoreName(storeId: string) {
    return this.stores?.find((x) => x.id === storeId).name;
  }

  getPriorityName(value: number) {
    return Priority[value];
  }

  getTransportName(value: number) {
    return Transport[value];
  }

  getPaymentModeName(value: number) {
    return PaymentMode[value];
  }

  isPaymentModeCredit() {
    return (
      this.purchaseRequisitionForm.get("paymentMode").value ===
      Payment_Mode_Credit
    );
  }

  getImportPurchaseIncoTermName(value: number) {
    return ImportPurchaseIncoTerm[value];
  }

  getImportPurchasePaymentTermName(value: number) {
    return ImportPurchasePaymentTerm[value];
  }

  getProductName(productId: string) {
    const product =
      this.products?.find((product) => product?.id === productId) ||
      this.data?.purchaseRequisitionDetails?.find(
        (x) => x?.productId == productId
      )?.product;
    return product?.name;
  }

  findProductById(id: string) {
    return this.products?.find((product) => product?.id === id);
  }

  handleProductSelection(event: any, index: number) {
    const particularDetail = this.purchaseRequisitionDetails.at(index);
    const productId = event.option.value;
    const storeId = this.purchaseRequisitionForm.get("storeId")?.value;

    if (!storeId) {
      this.showSnackBar("Please select a Store first!");
      particularDetail.reset();
      return;
    }

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
      lastPurchaseRate: selectedProduct?.lastPurchaseRate,
      measurementUnitName: selectedProduct?.measurementUnit?.name,
      alertQuantity: selectedProduct?.alertQuantity,
    });
    this.stockService
      .getItemStock(
        event?.option?.value,
        this.purchaseRequisitionForm.get("storeId")?.value
      )
      .subscribe((responseStockQuantity) => {
        particularDetail.patchValue({
          currentStockQuantity: responseStockQuantity,
        });
      });

    // this.calculateAmount(index);
  }

  showSnackBar(message: string) {
    this.snackBarService.openSnackbar(message);
  }

  isExistSelectedProduct(productId: string, index: number): boolean {
    const isProductAdded = this.purchaseRequisitionDetails.value.some(
      (item, i) => {
        return item.productId === productId && i !== index;
      }
    );
    return isProductAdded;
  }

  handleStoreSelection() {
    this.purchaseRequisitionDetails.reset();
  }

  handleProductSearch(event: any, itemIndex: number) {
    const name = event?.target?.name;
    if (name === "productId") {
      const term = this.purchaseRequisitionDetails
        .at(itemIndex)
        .get("productId");
      this.filterProduct(term.value || "");
    }
  }

  filterProduct(searchTerm: string) {
    searchTerm = searchTerm.toLowerCase();
    this.searchProducts = this.products?.filter((product) =>
      product.name.toLowerCase().startsWith(searchTerm)
    );
  }

  private handleSuccessfulSave(res: GeneralResponse<string>): void {
    if (res?.succeeded) {
      this.handleSendRfqToVendorResponse(res?.data);
      // this.navigateToList();
      this.toastr.success(res?.message);
    } else {
      this.isLoading = false;
      this.toastr.error(res?.message);
    }
  }

  private handleSendRfqToVendorResponse(id: string): void {
    this.purchaseRequisitionService.getPurchaseRequisitionById(id).subscribe({
      next: (response) => {
        this.data = response.data;
        this.isViewMode = true;
        this.isLoading = false;
      },
      error: (err) => {
        location.reload();
      },
    });
  }

  navigateToList() {
    this.router.navigate([this.path.list]);
  }

  backToView() {
    this.initializeForm();
    this.stepper.next();
    this.isViewMode = true;
  }

  private addPurchaseRequisition(body: PurchaseRequisitionRequestDTO): void {
    this.purchaseRequisitionService
      .createPurchaseRequisition(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  // private updatePurchaseRequisition(body: PurchaseRequisitionRequestDTO): void {
  //   this.purchaseRequisitionService
  //     .updatePurchaseRequisition(body)
  //     .subscribe((res) => this.handleSuccessfulSave(res));
  // }

  private preparePurchaseRequisitionForRFQ(
    body: PurchaseRequisitionRequestDTO
  ): void {
    this.purchaseRequisitionService
      .preparePurchaseRequisitionForRFQ(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  deletedIds: string = "";
  onDeleteItem(id: string, itemIndex: number): void {
    if (id) this.deletedIds += `${id},`;
    this.purchaseRequisitionDetails.removeAt(itemIndex);
    // this.calculateCost();
  }

  onSubmit(): void {
    if (this.purchaseRequisitionForm.valid) {
      this.isLoading = true;
      const formValue = this.purchaseRequisitionForm.value;
      if (!formValue.id) {
        this.addPurchaseRequisition(formValue);
      } else {
        formValue.deletedPurchaseRequisitionDetailIds = this.deletedIds;
        this.preparePurchaseRequisitionForRFQ(formValue);
      }
    }
  }

  check(id: string) {
    if (this.isLoading) return;
    this.isLoading = true;

    this.purchaseRequisitionService
      .checkPurchaseRequisition(id)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (res) => {
          if (res?.succeeded) {
            this.data.requisitionStatus = res?.data as unknown as number;
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

    this.purchaseRequisitionService
      .approvePurchaseRequisition(id)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (res) => {
          if (res?.succeeded) {
            this.data.requisitionStatus = res?.data as unknown as number;
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

    this.purchaseRequisitionService
      .unpostPurchaseRequisition(id, status)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (res) => {
          if (res?.succeeded) {
            this.data.requisitionStatus = res?.data as unknown as number;
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

  sendRFQ(): void {
    if (this.isDialogOpen) return;
    this.isDialogOpen = true;

    const dialogRef = this.dialog.open(SendRFQToVendorEmailComponent, {
      disableClose: true,
      panelClass: "add-bill-container",
      minHeight: "auto",
      height: "auto",
      data: this.data,
    });

    dialogRef.afterClosed().subscribe(() => (this.isDialogOpen = false));
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
}
