import { HttpClient } from "@angular/common/http";
import { ChangeDetectorRef, Component, OnInit, ViewChild } from "@angular/core";
import { FormArray, FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MatButton } from "@angular/material/button";
import { MatDialog } from "@angular/material/dialog";
import { MatStepper } from "@angular/material/stepper";
import { ActivatedRoute, Router } from "@angular/router";
import { Inventory_Type_Id_Raw_Materials } from "app/shared/consts/const";
import { PurchaseReturnStatus } from "app/shared/enums/purchaseReturnStatus";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { SnackBarService } from "app/shared/services/snack-bar.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { inFinancialYearValidator } from "app/shared/validators/in-financial-year-validator";
import { ProductRequest } from "app/views/configuration/models/product/product-request.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { StoreRequest } from "app/views/configuration/models/store/store-request.model";
import { Store } from "app/views/configuration/models/store/store.model";
import { Supplier } from "app/views/configuration/models/supplier/supplier.model";
import { ProductService } from "app/views/configuration/services/product.service";
import { StoreService } from "app/views/configuration/services/store.service";
import { SupplierService } from "app/views/configuration/services/supplier.service";
import {
  GoodsReceiveNoteResponseDTO,
  GoodsReceiveNoteResponseDetail,
} from "app/views/purchase/models/goods-receive-note/goods-receive-note-response-dto.model";
import { PurchaseReturnRequestDTO } from "app/views/purchase/models/purchase-return/purchase-return-request-dto.model";
import {
  PurchaseReturnResponseDTO,
  PurchaseReturnResponseDetail,
} from "app/views/purchase/models/purchase-return/purchase-return-response-dto.model";
import { PurchaseReturnService } from "app/views/purchase/services/purchase-return.service";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { finalize } from "rxjs";
import { GRNDialogComponent } from "../grn-dialog/grn-dialog.component";

@Component({
  selector: "app-purchase-return-form",
  templateUrl: "./purchase-return-form.component.html",
  styleUrls: ["./purchase-return-form.component.scss"],
})
export class PurchaseReturnFormComponent implements OnInit {
  @ViewChild("btnCheck") btnCheck: MatButton;
  @ViewChild("btnApprove") btnApprove: MatButton;
  @ViewChild("stepper") private stepper: MatStepper;
  isDataLoading: boolean = false;
  isLoading: boolean = false;
  isDialogOpen: boolean = false;
  isViewMode: boolean = true;
  isChecked: boolean;
  formTitle: string;
  purchaseReturnForm: FormGroup;
  suppliers: Supplier[];
  filterSuppliers: Supplier[];
  stores: Store[];
  products: ProductView[];
  searchProducts: ProductView[];
  purchaseReturnDetailsData: any[] = [];
  data: PurchaseReturnResponseDTO;
  businessType: string;
  // Define financial year date range here
  financialYearStartDate: Date; // Jul 1, 2023
  financialYearEndDate: Date; // Jun 30, 2024
  currentFinancialYearId: string;
  applyMinMax: boolean = false;

  private path = {
    list: "/purchase/purchase-return",
    edit: "purchase/purchase-return",
  };

  constructor(
    public dialog: MatDialog,
    private fb: FormBuilder,
    private supplierService: SupplierService,
    private storeService: StoreService,
    private enumValueService: EnumValueService,
    private productService: ProductService,
    private purchaseReturnService: PurchaseReturnService,
    private dateFormatService: DateTimeFormatService,
    private jwtAuth: JwtAuthService,
    private toastr: ToastrService,
    private activatedRoute: ActivatedRoute,
    private router: Router,
    private http: HttpClient,
    private cdRef: ChangeDetectorRef,
    public statusColorService: StatusColorService,
    private snackBarService: SnackBarService,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.activatedRoute.data.subscribe((response: any) => {
      this.data = response.purchaseReturn?.data;
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
    this.getAllSuppliers();
    //! it's important after close openPurchaseReturnDialog
    this.getAllProducts();
  }

  initializeForm() {
    this.createForm();
    this.populateForm();
    this.setFormTitle();
    this.purchaseReturnForm.get("purchaseReturnDate").markAsTouched();
  }

  createForm(): void {
    this.purchaseReturnForm = this.fb.group({
      id: [this.data?.id || null],
      referenceNo: [this.data?.referenceNo || ""],
      grnno: [this.data?.grnno || ""],
      purchaseReturnDate: [
        this.data?.purchaseReturnDate ||
          this.dateFormatService.getPresentDate(),
      ],
      storeId: [this.data?.storeId ?? null, Validators.required],
      supplierId: [this.data?.supplierId ?? null, Validators.required],
      totalAmount: [this.data?.totalAmount ?? 0, Validators.required],
      remark: [this.data?.remark || ""],
      deletedPurchaseReturnDetailIds: [""],
      purchaseReturnDetails: this.fb.array([]),
    });
    this.setMinMaxDates();
  }

  setMinMaxDates() {
    if (this.applyMinMax) {
      this.purchaseReturnForm
        .get("purchaseReturnDate")
        .setValidators([
          Validators.required,
          inFinancialYearValidator(
            this.financialYearStartDate,
            this.financialYearEndDate
          ),
        ]);
    } else {
      this.purchaseReturnForm
        .get("purchaseReturnDate")
        .setValidators([Validators.required]);
    }

    this.purchaseReturnForm.get("purchaseReturnDate").updateValueAndValidity();
  }

  get purchaseReturnDetails(): FormArray {
    return this.purchaseReturnForm?.get("purchaseReturnDetails") as FormArray;
  }

  populateForm(): void {
    if (this.purchaseReturnForm?.get("id").value) {
      this.populatePurchaseReturnDetails(this.data);
    } else {
      this.addItem();
    }
  }

  setFormTitle(): void {
    if (this.purchaseReturnForm.get("id").value) {
      this.formTitle = "Edit Purchase Return";
    } else {
      this.formTitle = "Add Purchase Return";
    }
  }

  populatePurchaseReturnDetails(data: PurchaseReturnResponseDTO): void {
    data.purchaseReturnDetails.forEach((item: PurchaseReturnResponseDetail) =>
      this.addItem(item)
    );
  }

  addItem(item?: PurchaseReturnResponseDetail): void {
    this.purchaseReturnDetails.push(this.createPurchaseReturnDetail(item));
  }

  createPurchaseReturnDetail(item?: PurchaseReturnResponseDetail): FormGroup {
    return this.fb.group({
      id: [item?.id ?? null],
      productId: [item?.productId ?? null, Validators.required],
      product: [item?.product ?? null],
      measurementUnitName: [item?.product?.measurementUnit?.name ?? ""],
      grnquantity: [item?.grnquantity ?? 0],
      grnBagWeightDeductionQty: [item?.grnBagWeightDeductionQty ?? 0],
      quantity: [item?.quantity ?? 0, Validators.required],
      bagWeightDeductionQty: [item?.bagWeightDeductionQty ?? 0],
      numberOfBagQuantity: [item?.numberOfBagQuantity ?? 0],
      netQuantity: [item?.netQuantity ?? 0],
      rate: [item?.rate ?? 0, Validators.required],
      amount: [item?.amount ?? 0, Validators.required],
    });
  }

  isPassingDataToNextStep = false;
  handleStepChange(event: any) {
    this.isPassingDataToNextStep = event?.selectedIndex === 1;
    if (this.isPassingDataToNextStep) {
      this.purchaseReturnDetailsData = this.purchaseReturnForm.get(
        "purchaseReturnDetails"
      ).value;
    }
  }

  getPurchaseReturnById(id: string): void {
    this.purchaseReturnService.getPurchaseReturnById(id).subscribe((res) => {
      this.data = res?.data;
      this.initializeForm();
    });
  }

  getPurchaseReturnStatus(value) {
    return this.statusColorService.getPurchaseReturnStatus(value);
  }

  getPurchaseReturnStatusName(value: number) {
    return PurchaseReturnStatus[value];
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    if (fieldName === "supplierId") {
      this.purchaseReturnForm?.get("supplierId").setValue(null);
    }
  }

  clearInputFromDetails(evt: any, itemIndex: number): void {
    evt.stopPropagation();
    const particularDetail = this.purchaseReturnDetails.at(itemIndex);
    particularDetail?.patchValue({
      productId: null,
    });
  }

  handleSupplierSearch(event: any): void {
    const name = event.target?.name;
    if (name === "supplierId") {
      const term = this.purchaseReturnForm.get("supplierId");
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

  getAllStores(): void {
    let storeRequest = new StoreRequest();
    storeRequest.page = -1;
    storeRequest.inventoryTypeId = Inventory_Type_Id_Raw_Materials;
    this.storeService.getStores(storeRequest).subscribe((res) => {
      this.stores = res?.data?.item1;
    });
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

  getStoreName(storeId: string) {
    return this.stores?.find((x) => x.id === storeId).name;
  }

  getProductName(productId: string) {
    const product =
      this.products?.find((product) => product?.id === productId) ||
      this.data?.purchaseReturnDetails?.find((x) => x?.productId == productId)
        ?.product;
    if (!product) return "";
    return (
      product?.name +
      (this.businessType === "2" ? "" : ` (${product?.packSize?.name})`)
    );
  }

  findProductById(id: string) {
    return this.products?.find((product) => product?.id === id);
  }

  handleProductSelection(event: any, index: number) {
    const particularDetail = this.purchaseReturnDetails.at(index);
    const productId = event?.option?.value;

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
      rate: selectedProduct.purchasePrice,
    });
    this.calculateAmount(index);
  }

  showSnackBar(message: string) {
    this.snackBarService.openSnackbar(message);
  }

  isExistSelectedProduct(productId: string, index: number): boolean {
    const isProductAdded = this.purchaseReturnDetails.value.some((item, i) => {
      return item.productId === productId && i !== index;
    });
    return isProductAdded;
  }

  handleProductSearch(event: any, itemIndex: number) {
    const name = event?.target?.name;
    if (name === "productId") {
      const term = this.purchaseReturnDetails.at(itemIndex).get("productId");
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
    const subtotal = this.purchaseReturnDetails?.value?.reduce(
      (sum, item) => sum + item?.quantity * item?.rate,
      0
    );

    this.purchaseReturnForm?.get("totalAmount")?.setValue(subtotal);
  }

  onControlChange(event: any, itemIndex?: number): void {
    const name = event?.target?.name;
    const item = this.purchaseReturnDetails.at(itemIndex);
    if (this.purchaseReturnForm.get("purchaseReturnDate").value) {
    }
    if (name === "quantity" || name === "bagWeightDeductionQty") {
      this.calculateNetQuantity(itemIndex);
    }
    if (name === "quantity" || name === "rate") {
      this.calculateAmount(itemIndex);
    }
    if (name == "numberOfBagQuantity") {
      if (item.get("numberOfBagQuantity").value == null) {
        item.get("numberOfBagQuantity").setValue(0);
      }
    }
  }

  calculateNetQuantity(itemIndex: number) {
    const item = this.purchaseReturnDetails?.at(itemIndex);
    const quantity = item?.get("quantity");
    const bagWeightDeductionQty = item?.get("bagWeightDeductionQty")?.value;
    const netQuantity = item?.get("netQuantity");
    netQuantity.setValue(quantity.value - bagWeightDeductionQty);
  }

  calculateAmount(itemIndex: number) {
    const item = this.purchaseReturnDetails?.at(itemIndex);
    const quantity = item?.get("quantity");
    const ratePerUnit = item?.get("rate")?.value;
    const amount = item?.get("amount");
    amount?.setValue(quantity.value * ratePerUnit);
    this.calculateCost();
  }

  private handleSuccessfulSave(res: GeneralResponse<string>): void {
    if (res?.succeeded) {
      this.handlePurchaseReturnResponse(res?.data);
      this.toastr.success(res?.message);
      this.deletedIds = "";
    } else {
      this.isLoading = false;
      this.toastr.success(res?.message);
    }
  }

  private handlePurchaseReturnResponse(purchaseReturnId: string): void {
    this.purchaseReturnService
      .getPurchaseReturnById(purchaseReturnId)
      .subscribe({
        next: (purchaseReturnResponse) => {
          this.data = purchaseReturnResponse.data;
          this.isViewMode = true;
          this.isLoading = false;
          this.navigateToView(purchaseReturnResponse?.data?.id);
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

  private addPurchaseReturn(body: PurchaseReturnRequestDTO): void {
    this.purchaseReturnService
      .createPurchaseReturn(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  private updatePurchaseReturn(body: PurchaseReturnRequestDTO): void {
    this.purchaseReturnService
      .updatePurchaseReturn(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  deletedIds: string = "";
  onDeleteItem(id: string, itemIndex: number): void {
    if (id) this.deletedIds += `${id},`;
    this.purchaseReturnDetails.removeAt(itemIndex);
    this.calculateCost();
  }

  onSubmit(): void {
    if (this.purchaseReturnForm.valid) {
      this.isLoading = true;
      const formValue = this.purchaseReturnForm.value;
      if (!formValue.id) {
        this.addPurchaseReturn(formValue);
      } else {
        formValue.deletedPurchaseReturnDetailIds = this.deletedIds;
        this.updatePurchaseReturn(formValue);
      }
    }
  }

  //openGRNDialog
  openGRNDialog() {
    const dialogRef = this.dialog.open(GRNDialogComponent, {
      disableClose: true,
      panelClass: "add-bill-container",
      minHeight: "auto",
      height: "auto",
    });
    dialogRef.afterClosed().subscribe((result: GoodsReceiveNoteResponseDTO) => {
      if (!result) {
        return;
      }

      this.purchaseReturnForm.reset();
      this.purchaseReturnForm.setControl(
        "purchaseReturnDetails",
        this.fb.array([])
      );
      this.purchaseReturnForm.markAllAsTouched();

      this.purchaseReturnForm.patchValue({
        grnno: result?.grnno,
        storeId: result?.storeId,
        supplierId: result?.supplierId,
        purchaseReturnDate: this.dateFormatService.getPresentDate(),
        remark: result?.remark,
        totalAmount: 0,
      });
      result.goodsReceiveNoteDetails.forEach(
        (item: GoodsReceiveNoteResponseDetail) => {
          let purchaseReturnResponseDetail: any = {
            productId: item?.productId,
            product: item?.product,
            grnquantity: item?.grnquantity,
            grnBagWeightDeductionQty: item?.bagWeightDeductionQuantity,
            quantity: 0,
            bagWeightDeductionQty: 0,
            netQuantity: 0,
            rate: item?.rate,
            amount: 0,
          };
          this.addItem(purchaseReturnResponseDetail);
        }
      );
    });
  }

  check(id: string) {
    if (this.isLoading) return;
    this.isLoading = true;

    this.purchaseReturnService
      .checkPurchaseReturn(id)
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

    this.purchaseReturnService
      .approvePurchaseReturn(id)
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

    this.purchaseReturnService
      .unpostPurchaseReturn(id, status)
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
      .get(environment.apiURL + "/ReportPurchaseReturn/" + id, {
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
