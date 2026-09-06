import { ChangeDetectorRef, Component, OnInit, ViewChild } from "@angular/core";
import { FormArray, FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MatStepper } from "@angular/material/stepper";
import { ActivatedRoute, Router } from "@angular/router";
import { DeliveryNoteStatus } from "app/shared/enums/deliveryNoteStatus";
import { Transport } from "app/shared/enums/transport";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { Customer } from "app/views/configuration/models/customer/customer.model";
import { DeliveryPlace } from "app/views/configuration/models/delivery-place/delivery-place.model";
import { ProductRequest } from "app/views/configuration/models/product/product-request.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { Store } from "app/views/configuration/models/store/store.model";
import { CustomerService } from "app/views/configuration/services/customer.service";
import { DeliveryPlaceService } from "app/views/configuration/services/delivery-place.service";
import { ProductService } from "app/views/configuration/services/product.service";
import { StoreService } from "app/views/configuration/services/store.service";
import { DeliveryNoteRequestDTO } from "app/views/sales/models/delivery-note/delivery-note-request-dto.model";
import {
  DeliveryNoteResponseDTO,
  DeliveryNoteResponseDetail,
} from "app/views/sales/models/delivery-note/delivery-note-response-dto.model";
import { DeliveryNoteService } from "app/views/sales/services/delivery-note.service";
import { ToastrService } from "ngx-toastr";

import { HttpClient } from "@angular/common/http";
import { MatButton } from "@angular/material/button";
import { MatDialog } from "@angular/material/dialog";
import { Inventory_Type_Id_Finished_Goods } from "app/shared/consts/const";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { inFinancialYearValidator } from "app/shared/validators/in-financial-year-validator";
import { StoreRequest } from "app/views/configuration/models/store/store-request.model";
import { DeliveryNoteSearchRequestDTO } from "app/views/sales/models/delivery-note/delivery-note-search-request-dto.model";
import {
  SaleOrderResponseDTO,
  SaleOrderResponseDetail,
} from "app/views/sales/models/sale-order/sale-order-response-dto.model";
import { compareDeliveryNoteBonusQtyValidator } from "app/views/sales/validators/compare-delivery-note-bonus-qty-validators";
import { compareDeliveryNoteQtyValidator } from "app/views/sales/validators/compare-delivery-note-qty-validators";
import { environment } from "environments/environment";
import { finalize } from "rxjs";
import { SalesOrderListComponent } from "../sales-order-list/sales-order-list.component";

@Component({
  selector: "app-delivery-note-form",
  templateUrl: "./delivery-note-form.component.html",
  styleUrls: ["./delivery-note-form.component.scss"],
})
export class DeliveryNoteFormComponent implements OnInit {
  @ViewChild("btnCheck") btnCheck: MatButton;
  @ViewChild("btnApprove") btnApprove: MatButton;
  @ViewChild("stepper") private stepper: MatStepper;
  isLoading: boolean = false;
  isDialogOpen: boolean = false;
  isSalesOrderDialogHidden: boolean = true;
  isViewMode: boolean = true;
  isChecked: boolean;
  formTitle: string;
  deliveryNoteForm: FormGroup;
  customers: Customer[];
  filterCustomers: Customer[];
  stores: Store[];
  filterStores: Store[];
  deliveryPlaces: DeliveryPlace[];
  products: ProductView[];
  searchProducts: ProductView[];
  transports: ENUM[];
  deliveryNoteDetailsData: any[] = [];
  data: DeliveryNoteResponseDTO;

  deliveryNoteRequest = new DeliveryNoteSearchRequestDTO();
  deliveryNoteData: DeliveryNoteResponseDTO[];

  businessType: string;
  // Define financial year date range here
  financialYearStartDate: Date; //Example: Jul 1, 2023
  financialYearEndDate: Date; //Example: Jun 30, 2024
  currentFinancialYearId: string;
  applyMinMax: boolean = false;

  private path = {
    list: "sales/delivery-note",
    edit: "sales/delivery-note",
  };

  constructor(
    public dialog: MatDialog,
    private fb: FormBuilder,
    private customerService: CustomerService,
    private storeService: StoreService,
    private deliveryPlaceService: DeliveryPlaceService,
    private enumValueService: EnumValueService,
    private productService: ProductService,
    private deliveryNoteService: DeliveryNoteService,
    private jwtAuth: JwtAuthService,
    private dateFormatService: DateTimeFormatService,
    private toastr: ToastrService,
    private http: HttpClient,
    private activatedRoute: ActivatedRoute,
    private router: Router,
    // private localStorageService: LocalStoreService,
    private cdRef: ChangeDetectorRef,
    public statusColorService: StatusColorService,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.getDeliveryNotes();
    this.activatedRoute.data.subscribe((response: any) => {
      this.data = response?.deliveryNote?.data;
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
    this.getAllDeliveryPlaces();
    this.getAllCustomers();
    this.getTransports();
    this.getAllProducts();
  }

  initializeForm() {
    this.createForm();
    this.populateForm();
    this.setFormTitle();
    this.deliveryNoteForm.get("deliveryDate").markAsTouched();
  }

  createForm(): void {
    this.deliveryNoteForm = this.fb.group({
      id: [this.data?.id || null],
      saleOrderNo: [this.data?.saleOrderNo || ""],
      orderDate: [this.data?.orderDate ?? null],
      referenceNo: [this.data?.referenceNo || ""],
      deliveryDate: [
        this.data?.deliveryDate || this.dateFormatService.getPresentDate(),
      ],
      deliveryPlace: [this.data?.deliveryPlace || ""],
      transport: [this.data?.transport, Validators.required],
      customerId: [this.data?.customerId, Validators.required],
      storeId: [this.data?.storeId, Validators.required],
      moneyReceiptNo: [this.data?.moneyReceiptNo ?? ""],
      store: [this?.data?.store || null],
      creditLimit: [this.data?.creditLimit, Validators.required],
      limitAvailed: [this.data?.limitAvailed, Validators.required],
      truckNo: [this.data?.truckNo || ""],
      driverName: [this.data?.driverName || ""],
      driverContactNo: [this.data?.driverContactNo || ""],
      subtotal: [this.data?.subtotal ?? 0],
      discount: [this.data?.discount ?? 0],
      offerDiscount: [this.data?.offerDiscount ?? 0],
      otherDiscount: [this.data?.otherDiscount ?? 0],
      total: [this.data?.total ?? 0],
      transportationCost: [this.data?.transportationCost ?? 0],
      depoCharge: [this.data?.depoCharge ?? 0],
      netTotal: [this.data?.netTotal ?? 0],
      remark: [this.data?.remark || ""],
      deletedDeliveryNoteDetailIds: [""],
      deliveryNoteDetails: this.fb.array([]),
    });
    this.setMinMaxDates();
  }

  setMinMaxDates() {
    if (this.applyMinMax) {
      this.deliveryNoteForm
        .get("deliveryDate")
        .setValidators([
          Validators.required,
          inFinancialYearValidator(
            this.financialYearStartDate,
            this.financialYearEndDate
          ),
        ]);
    } else {
      this.deliveryNoteForm
        .get("deliveryDate")
        .setValidators([Validators.required]);
    }
    this.deliveryNoteForm.get("deliveryDate").updateValueAndValidity();
  }

  get deliveryNoteDetails(): FormArray {
    return this.deliveryNoteForm.get("deliveryNoteDetails") as FormArray;
  }

  populateForm(): void {
    if (this.deliveryNoteForm.get("id").value) {
      this.populateDeliveryNoteDetails(this.data);
    } else {
      this.addItem();
    }
  }

  setFormTitle(): void {
    if (this.deliveryNoteForm.get("id").value) {
      this.formTitle = "Edit Delivery Note";
    } else {
      this.formTitle = "Add Delivery Note";
    }
  }

  populateDeliveryNoteDetails(data: DeliveryNoteResponseDTO): void {
    data.deliveryNoteDetails.forEach((item: DeliveryNoteResponseDetail) =>
      this.addItem(item)
    );
  }

  addItem(item?: DeliveryNoteResponseDetail): void {
    this.deliveryNoteDetails.push(this.createDeliveryNoteDetail(item));
  }

  createDeliveryNoteDetail(item?: DeliveryNoteResponseDetail): FormGroup {
    return this.fb.group(
      {
        id: [item?.id || null],
        saleOrderDetailId: [item?.saleOrderDetailId || null],
        productId: [item?.productId || null, Validators.required],
        product: [item?.product || null],
        measurementUnitName: [item?.product?.measurementUnit?.name || ""],
        bagWeight: [item?.bagWeight || "", Validators.required],
        orderedPrimaryQuantity: [
          item?.orderedPrimaryQuantity || 0,
          Validators.required,
        ],
        orderedQuantity: [item?.orderedQuantity || 0, Validators.required],
        orderedPrimaryBonusQuantity: [item?.orderedPrimaryBonusQuantity || 0],
        orderedBonusQuantity: [item?.orderedBonusQuantity || 0],
        deliveryPrimaryQuantity: [item?.deliveryPrimaryQuantity || 0],
        delivered: [item?.delivered ?? 0],
        deliveryPrimaryBonusQuantity: [item?.deliveryPrimaryBonusQuantity],
        deliveredBonus: [item?.deliveredBonus ?? 0],
        deliveryQuantity: [item?.deliveryQuantity || 0],
        rate: [item?.rate, Validators.required],
        netRate: [item?.netRate],
        discountPerUnit: [item?.discountPerUnit ?? 0],
        discountAmount: [item?.discountAmount ?? 0],
        offerDiscountPerUnit: [item?.offerDiscountPerUnit ?? 0],
        invoiceDiscountPerUnit: [item?.invoiceDiscountPerUnit ?? 0],
        cashDiscountPerUnit: [item?.cashDiscountPerUnit ?? 0],
        specialDiscountPerUnit: [item?.specialDiscountPerUnit ?? 0],
        amount: [item?.amount, Validators.required],
        otherDiscountPerUnit: [item?.otherDiscountPerUnit ?? 0],
        transportationCostPerUnit: [item?.transportationCostPerUnit ?? 0],
        depoChargePerUnit: [item?.depoChargePerUnit ?? 0],
        createdOn: [item?.createdOn || null],
        createdBy: [item?.createdBy || null],
      },
      {
        validators: [
          compareDeliveryNoteQtyValidator(
            "delivered",
            "deliveryPrimaryQuantity",
            "orderedPrimaryQuantity"
          ),
          compareDeliveryNoteBonusQtyValidator(
            "deliveredBonus",
            "deliveryPrimaryBonusQuantity",
            "orderedPrimaryBonusQuantity"
          ),
        ],
      }
    );
  }

  isPassingDataToNextStep = false;
  handleStepChange(event: any) {
    this.isPassingDataToNextStep = event.selectedIndex === 1;
    if (this.isPassingDataToNextStep) {
      this.deliveryNoteDetailsData = this.deliveryNoteForm.get(
        "deliveryNoteDetails"
      ).value;
    }
  }

  getDeliveryNotes(): void {
    this.deliveryNoteRequest.page = -1;
    this.deliveryNoteRequest.deliveryNoteStatuses = [1, 2];
    this.deliveryNoteService
      .getDeliveryNotes(this.deliveryNoteRequest)
      .subscribe((res) => {
        this.deliveryNoteData = res?.data?.item1;
        this.isSalesOrderDialogHidden = false;
      });
  }

  getDeliveryNoteStatus(value) {
    return this.statusColorService.getDeliveryNoteStatus(value);
  }

  getDeliveryNoteStatusName(value: number) {
    return DeliveryNoteStatus[value];
  }

  getTransports() {
    return this.enumValueService.getTransports().subscribe((res) => {
      this.transports = res;
    });
  }

  getTransportName(value: number) {
    return Transport[value];
  }

  getAllDeliveryPlaces(): void {
    this.deliveryPlaceService.getAllDeliveryPlaces().subscribe((res) => {
      this.deliveryPlaces = res?.data?.item1;
    });
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
      this.storeService.getStores(request).subscribe((res) => {
        this.filterStores = this.stores = res?.data?.item1;
      });
    }
    if (businessType === "2") {
      request.inventoryTypeId = Inventory_Type_Id_Finished_Goods;
      this.storeService.getStores(request).subscribe((res) => {
        this.filterStores = this.stores = res?.data?.item1;
      });
    }
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

  getDeliveryPlaceName(deliveryPlaceId: string) {
    return this.deliveryPlaces?.find((x) => x?.id === deliveryPlaceId)?.name;
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    if (fieldName === "customerId") {
      this.deliveryNoteForm?.get("customerId").setValue(null);
    }
    if (fieldName === "storeId") {
      this.deliveryNoteForm?.get("storeId").setValue(null);
    }
  }

  handleCustomerSearch(event: any): void {
    const name = event.target?.name;
    if (name === "customerId") {
      const term = this.deliveryNoteForm.get("customerId");
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
    const supplierAccount =
      this.customers?.find((x) => x?.id === customerId) || this.data?.customer;
    return supplierAccount?.name;
  }

  handleStoreSearch(event: any): void {
    const name = event.target?.name;
    if (name === "storeId") {
      const term = this.deliveryNoteForm.get("storeId");
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

  onSelectedCustomer(customerId: string) {
    this.customerService
      .getCustomerCreditLimitBalance(customerId)
      .subscribe((res) => {
        this.deliveryNoteForm
          ?.get("creditLimit")
          ?.setValue(res?.data?.creditLimit);
        this.deliveryNoteForm
          ?.get("limitAvailed")
          ?.setValue(res?.data?.balance);
      });
  }

  getProductName(productId: string) {
    const product =
      this.products?.find((product) => product?.id === productId) ||
      this.data?.deliveryNoteDetails?.find((x) => x?.productId == productId)
        ?.product;
    if (!product) return "";
    return (
      product?.name +
      (this.businessType === "2"
        ? ` (${product.bagWeight} ${product?.measurementUnit?.name})`
        : ` (${product?.measurementUnit?.name})`)
    );
  }

  findProductById(id: string) {
    return this.products?.find((product) => product?.id === id);
  }

  handleProductSelection(event: any, index: number) {
    const particularDetail = this.deliveryNoteDetails.at(index);

    const selectedProduct = this.findProductById(event?.option?.value);
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

  handleProductSearch(event: any, itemIndex: number) {
    const name = event?.target?.name;
    if (name === "productId") {
      const term = this.deliveryNoteDetails.at(itemIndex).get("productId");
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
    const subtotal = this.deliveryNoteDetails?.value?.reduce(
      (sum, item) => sum + item?.deliveryQuantity * item?.rate,
      0
    );

    const discount = this.deliveryNoteDetails?.value?.reduce(
      (sum, item) => sum + item?.deliveryQuantity * item?.discountPerUnit,
      0
    );
    const offerDiscount = this.deliveryNoteDetails?.value?.reduce(
      (sum, item) => sum + item?.deliveryQuantity * item?.offerDiscountPerUnit,
      0
    );

    const otherDiscount = this.deliveryNoteDetails?.value?.reduce(
      (sum, item) => sum + item?.deliveryQuantity * item?.otherDiscountPerUnit,
      0
    );

    const transportationCost = this.deliveryNoteDetails?.value?.reduce(
      (sum, item) =>
        sum + item?.deliveryQuantity * item?.transportationCostPerUnit,
      0
    );

    const depoCharge = this.deliveryNoteDetails?.value?.reduce(
      (sum, item) => sum + item?.deliveryQuantity * item?.depoChargePerUnit,
      0
    );

    let total = subtotal - discount - offerDiscount - otherDiscount;
    this.deliveryNoteForm?.get("subtotal")?.setValue(subtotal);
    this.deliveryNoteForm?.get("discount")?.setValue(discount);
    this.deliveryNoteForm?.get("total")?.setValue(total);

    //transportation cost and depo charge and net total
    this.deliveryNoteForm?.get("otherDiscount")?.setValue(otherDiscount);
    this.deliveryNoteForm
      ?.get("transportationCost")
      ?.setValue(transportationCost);
    this.deliveryNoteForm?.get("depoCharge")?.setValue(depoCharge);
    this.deliveryNoteForm
      ?.get("netTotal")
      ?.setValue(total + transportationCost + depoCharge);
    //........................
  }

  onControlChange(event: any, itemIndex?: number): void {
    const name = event?.target?.name;
    if (name === "deliveryPrimaryQuantity") {
      this.calculateAmount(itemIndex);
    }
  }

  calculateAmount(itemIndex: number) {
    const item = this.deliveryNoteDetails?.at(itemIndex);
    //const orderedQuantity = item?.get("orderedQuantity")?.value;
    const deliveryQuantity = item?.get("deliveryQuantity");
    const deliveryPrimaryQuantity = item?.get("deliveryPrimaryQuantity")?.value;
    const discountPerUnit = item?.get("discountPerUnit")?.value;
    const discountAmount = item?.get("discountAmount");

    const ratePerUnit = item?.get("rate")?.value;
    const amount = item?.get("amount");
    const bagWeight = item?.get("bagWeight");
    deliveryQuantity?.setValue(bagWeight.value * deliveryPrimaryQuantity);
    discountAmount?.setValue(
      bagWeight.value * deliveryPrimaryQuantity * discountPerUnit
    );
    amount?.setValue(bagWeight.value * deliveryPrimaryQuantity * ratePerUnit);
    // this.deliveryNoteForm.get("discount").setValue(0);
    this.calculateCost();
  }

  private handleSuccessfulSave(res: GeneralResponse<string>): void {
    if (res?.succeeded) {
      this.handleDeliveryNoteResponse(res?.data);
      this.deletedIds = "";
      this.toastr.success(res?.message);
    } else {
      this.isLoading = false;
      this.toastr.error(res?.message);
    }
  }

  private handleDeliveryNoteResponse(deliveryNoteId: string): void {
    this.deliveryNoteService.getDeliveryNoteById(deliveryNoteId).subscribe({
      next: (deliveryNoteResponse) => {
        this.data = deliveryNoteResponse.data;
        this.isViewMode = true;
        this.isLoading = false;
        this.navigateToView(deliveryNoteResponse?.data?.id);
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

  private addDeliveryNote(body: DeliveryNoteRequestDTO): void {
    this.deliveryNoteService
      .createDeliveryNote(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  private updateDeliveryNote(body: DeliveryNoteRequestDTO): void {
    this.deliveryNoteService
      .updateDeliveryNote(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  deletedIds: string = "";
  onDeleteItem(id: string, itemIndex: number): void {
    if (id) this.deletedIds += `${id},`;
    this.deliveryNoteDetails.removeAt(itemIndex);
    this.calculateCost();
  }

  onSubmit(): void {
    if (this.deliveryNoteForm.valid) {
      this.isLoading = true;
      const formValue = this.deliveryNoteForm.value;
      if (!formValue.id) {
        this.addDeliveryNote(formValue);
      } else {
        formValue.deletedDeliveryNoteDetailIds = this.deletedIds;
        this.updateDeliveryNote(formValue);
      }
    }
  }

  check(id: string) {
    if (this.isLoading) return;
    this.isLoading = true;

    this.deliveryNoteService
      .checkDeliveryNote(id)
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

    this.deliveryNoteService
      .approveDeliveryNote(id)
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

    this.deliveryNoteService
      .unpostDeliveryNote(id, status)
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

  getOtherDiscountPerUnit(data: SaleOrderResponseDTO) {
    let totalQty = data?.saleOrderDetails.reduce(
      (acc, curr) => acc + curr.quantity,
      0
    );
    return data?.otherDiscount / totalQty;
  }

  getTransportationCostPerUnit(data: SaleOrderResponseDTO) {
    let totalQty = data?.saleOrderDetails.reduce(
      (acc, curr) => acc + curr.quantity,
      0
    );
    return data?.transportationCost / totalQty;
  }

  getDepoChargePerUnit(data: SaleOrderResponseDTO) {
    let totalQty = data?.saleOrderDetails.reduce(
      (acc, curr) => acc + curr.quantity,
      0
    );
    return data?.depoCharge / totalQty;
  }

  getDeliveryQtyWithoutBonusQty(item: SaleOrderResponseDetail) {
    return (
      (item?.primaryQuantity - item?.deliveredPrimaryQuantity) * item.bagWeight
    );
  }

  // openSalesOrderDialog
  openSalesOrderDialog() {
    const dialogRef = this.dialog.open(SalesOrderListComponent, {
      disableClose: true,
      panelClass: "add-bill-container",
      minHeight: "auto",
      height: "auto",
      data: this.deliveryNoteData,
    });
    dialogRef.afterClosed().subscribe((result: SaleOrderResponseDTO) => {
      if (!result) {
        return;
      }
      this.deliveryNoteForm.reset();
      this.deliveryNoteForm.setControl(
        "deliveryNoteDetails",
        this.fb.array([])
      );
      this.deliveryNoteForm.markAllAsTouched();
      let resultCost = result.saleOrderDetails.reduce(
        (prevVal, currVal) => {
          return {
            subtotal:
              prevVal.subtotal +
              this.getDeliveryQtyWithoutBonusQty(currVal) * currVal.rate,
            discount:
              prevVal.discount +
              this.getDeliveryQtyWithoutBonusQty(currVal) *
                currVal.discountPerUnit,
            offerDiscount:
              prevVal.offerDiscount +
              this.getDeliveryQtyWithoutBonusQty(currVal) *
                currVal.offerDiscountPerUnit,
            otherDiscount:
              prevVal.otherDiscount +
              this.getDeliveryQtyWithoutBonusQty(currVal) *
                currVal.otherDiscountPerUnit,
            transportationCost:
              prevVal.transportationCost +
              this.getDeliveryQtyWithoutBonusQty(currVal) *
                currVal.transportationCostPerUnit,
            depoCharge:
              prevVal.depoCharge +
              this.getDeliveryQtyWithoutBonusQty(currVal) *
                currVal.depoChargePerUnit,
          };
        },
        {
          subtotal: 0,
          discount: 0,
          offerDiscount: 0,
          otherDiscount: 0,
          transportationCost: 0,
          depoCharge: 0,
        }
      );

      const formData = {
        saleOrderNo: result?.saleOrderNo,
        orderDate: result?.orderDate,
        referenceNo: result?.referenceNo,
        deliveryDate: result?.deliveryDate,
        transport: result?.transport,
        customerId: result?.customerId,
        storeId: result?.storeId,
        moneyReceiptNo: result?.moneyReceiptNo,
        subtotal: resultCost?.subtotal,
        discount: resultCost?.discount,
        offerDiscount: resultCost?.offerDiscount,
        otherDiscount: resultCost.otherDiscount,
        total:
          resultCost?.subtotal -
          resultCost?.discount -
          resultCost?.offerDiscount -
          resultCost.otherDiscount,
        transportationCost: resultCost?.transportationCost,
        depoCharge: resultCost?.depoCharge,
        netTotal:
          resultCost?.subtotal -
          resultCost?.discount -
          resultCost?.offerDiscount -
          resultCost.otherDiscount +
          resultCost.transportationCost +
          resultCost.depoCharge,
        remark: result?.remark,
        // creditLimit: result?.creditLimit,
        // limitAvailed: result?.limitAvailed,
      };

      this.deliveryNoteForm.patchValue(formData);
      const customerId = this.deliveryNoteForm.get("customerId").value;
      if (customerId !== "" || customerId !== undefined) {
        this.onSelectedCustomer(customerId);
      }
      result?.saleOrderDetails.forEach((item: SaleOrderResponseDetail) => {
        const deliveryNoteResponseDetail: DeliveryNoteResponseDetail = {
          saleOrderDetailId: item?.id,
          productId: item?.productId,
          product: item?.product,
          bagWeight: item?.bagWeight,
          orderedPrimaryQuantity: item?.primaryQuantity,
          orderedQuantity: item?.quantity,
          orderedPrimaryBonusQuantity: item?.primaryBonusQuantity,
          orderedBonusQuantity: item?.bonusQuantity,
          deliveryPrimaryQuantity:
            item?.primaryQuantity - item?.deliveredPrimaryQuantity,
          deliveryQuantity: this.getDeliveryQtyWithoutBonusQty(item),
          delivered: item?.deliveredPrimaryQuantity,
          deliveryPrimaryBonusQuantity:
            item?.primaryBonusQuantity - item?.deliveredPrimaryBonusQuantity,
          deliveredBonus: item?.deliveredPrimaryBonusQuantity,
          rate: item?.rate,
          netRate: item?.netRate,
          discountPerUnit: item?.discountPerUnit,
          offerDiscountPerUnit: item?.offerDiscountPerUnit,
          invoiceDiscountPerUnit: item?.invoiceDiscountPerUnit,
          cashDiscountPerUnit: item?.cashDiscountPerUnit,
          specialDiscountPerUnit: item?.specialDiscountPerUnit,
          discountAmount:
            this.getDeliveryQtyWithoutBonusQty(item) * item?.discountPerUnit,
          amount: this.getDeliveryQtyWithoutBonusQty(item) * item?.rate,
          otherDiscountPerUnit: item?.otherDiscountPerUnit,
          transportationCostPerUnit: item?.transportationCostPerUnit,
          depoChargePerUnit: item?.depoChargePerUnit,
        };
        this.addItem(deliveryNoteResponseDetail);
      });
    });
  }

  printPdf(id) {
    this.http
      .get(environment.apiURL + "/ReportDeliveryNote/" + id, {
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

  printGatePassPdf(id) {
    this.http
      .get(environment.apiURL + "/DeliveryNote/gate-pass/" + id, {
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

  printVatPdf(id) {
    this.http
      .get(environment.apiURL + "/ReportDeliveryNoteVatInvoice/" + id, {
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
