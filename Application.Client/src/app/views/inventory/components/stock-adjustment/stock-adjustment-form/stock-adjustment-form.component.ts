import { HttpClient } from "@angular/common/http";
import { ChangeDetectorRef, Component, OnInit, ViewChild } from "@angular/core";
import { FormArray, FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MatButton } from "@angular/material/button";
import { MatDialog } from "@angular/material/dialog";
import { MatStepper } from "@angular/material/stepper";
import { ActivatedRoute, Router } from "@angular/router";
import { StockAdjustmentStatus } from "app/shared/enums/stockAdjustmentStatus";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { SnackBarService } from "app/shared/services/snack-bar.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { inFinancialYearValidator } from "app/shared/validators/in-financial-year-validator";
import { ProductRequest } from "app/views/configuration/models/product/product-request.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { StoreRequest } from "app/views/configuration/models/store/store-request.model";
import { Store } from "app/views/configuration/models/store/store.model";
import { ProductService } from "app/views/configuration/services/product.service";
import { StoreService } from "app/views/configuration/services/store.service";
import { StockAdjustmentRequestDTO } from "app/views/inventory/models/stock-adjustment/stock-adjustment-request-dto.model";
import {
  StockAdjustmentResponseDTO,
  StockAdjustmentResponseDetail,
} from "app/views/inventory/models/stock-adjustment/stock-adjustment-response-dto.model";
import { StockAdjustmentService } from "app/views/inventory/services/stock-adjustment.service";
import { compareStockAdjustmentStockValidator } from "app/views/purchase/validators/compare-stock-adjustment-stock-validators";
import { StockService } from "app/views/report/services/stock.service";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { finalize } from "rxjs";

@Component({
  selector: "app-stock-adjustment-form",
  templateUrl: "./stock-adjustment-form.component.html",
  styleUrls: ["./stock-adjustment-form.component.scss"],
})
export class StockAdjustmentFormComponent implements OnInit {
  @ViewChild("btnCheck") btnCheck: MatButton;
  @ViewChild("btnApprove") btnApprove: MatButton;
  @ViewChild("stepper") private stepper: MatStepper;
  isLoading: boolean = false;
  isDialogOpen: boolean = false;
  isViewMode: boolean = true;
  isChecked: boolean;
  formTitle: string;
  stockAdjustmentForm: FormGroup;
  stores: Store[];
  products: ProductView[];
  filterProducts: ProductView[];
  stockAdjustmentDetailsData: any[] = [];
  data: StockAdjustmentResponseDTO;

  businessType: string;
  // Define financial year date range here
  financialYearStartDate: Date; // Jul 1, 2023
  financialYearEndDate: Date; // Jun 30, 2024
  currentFinancialYearId: string;
  applyMinMax: boolean = false;

  private path = {
    list: "inventory/stock-adjustment",
    edit: "inventory/stock-adjustment",
  };

  constructor(
    public dateFormatService: DateTimeFormatService,
    private jwtAuth: JwtAuthService,
    private fb: FormBuilder,
    private productService: ProductService,
    private stockAdjustmentService: StockAdjustmentService,
    private stockService: StockService,
    private storeService: StoreService,
    private toastr: ToastrService,
    private http: HttpClient,
    private activatedRoute: ActivatedRoute,
    private router: Router,
    private cdRef: ChangeDetectorRef,
    public statusColorService: StatusColorService,
    private dialog: MatDialog,
    private snackBarService: SnackBarService,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.activatedRoute.data.subscribe((response: any) => {
      this.data = response?.stockAdjustment?.data;
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
  }

  initializeForm() {
    this.createForm();
    this.populateForm();
    this.setFormTitle();
    this.stockAdjustmentForm.get("adjustmentDate").markAsTouched();
  }

  createForm(): void {
    this.stockAdjustmentForm = this.fb.group({
      id: [this.data?.id || null],
      code: [this.data?.code || ""],
      adjustmentDate: [
        this.data?.adjustmentDate || this.dateFormatService.getPresentDate(),
      ],
      storeId: [this.data?.storeId ?? null, Validators.required],
      totalAdjustmentQty: [
        this.data?.totalAdjustmentQty || 0,
        Validators.required,
      ],
      remark: [this.data?.remark || ""],
      deletedStockAdjustmentDetailIds: [""],
      stockAdjustmentDetails: this.fb.array([]),
    });
    this.setMinMaxDates();
  }

  setMinMaxDates() {
    if (this.applyMinMax) {
      this.stockAdjustmentForm
        .get("adjustmentDate")
        .setValidators([
          Validators.required,
          inFinancialYearValidator(
            this.financialYearStartDate,
            this.financialYearEndDate
          ),
        ]);
    } else {
      this.stockAdjustmentForm
        .get("adjustmentDate")
        .setValidators([Validators.required]);
    }

    this.stockAdjustmentForm.get("adjustmentDate").updateValueAndValidity();
  }

  get stockAdjustmentDetails(): FormArray {
    return this.stockAdjustmentForm.get("stockAdjustmentDetails") as FormArray;
  }

  populateForm(): void {
    if (this.stockAdjustmentForm.get("id").value) {
      this.getProducts(this.data.store?.inventoryTypeId);
      this.populateStockAdjustmentDetails(this.data);
    } else {
      this.addItem();
    }
  }

  setFormTitle(): void {
    if (this.stockAdjustmentForm.get("id").value) {
      this.formTitle = "Edit Stock Adjustment";
    } else {
      this.formTitle = "Add Stock Adjustment";
    }
  }

  populateStockAdjustmentDetails(data: StockAdjustmentResponseDTO): void {
    data.stockAdjustmentDetails.forEach((item: StockAdjustmentResponseDetail) =>
      this.addItem(item)
    );
  }

  addItem(item?: StockAdjustmentResponseDetail): void {
    this.stockAdjustmentDetails.push(this.createStockAdjustmentDetail(item));
  }

  createStockAdjustmentDetail(item?: StockAdjustmentResponseDetail): FormGroup {
    return this.fb.group(
      {
        id: [item?.id || null],
        productId: [item?.productId || null, Validators.required],
        measurementUnitName: [item?.product?.measurementUnit?.name || ""],
        product: [item?.product || null],
        adjustmentQty: [item?.adjustmentQty || 0, Validators.required],
        currentStockQuantity: [item?.currentStockQuantity || 0],
        createdOn: [item?.createdOn || null],
        createdBy: [item?.createdBy || null],
      },
      {
        validators: compareStockAdjustmentStockValidator(
          "currentStockQuantity",
          "adjustmentQty"
        ),
      }
    );
  }

  isPassingDataToNextStep = false;
  handleStepChange(event: any) {
    this.isPassingDataToNextStep = event.selectedIndex === 1;
    if (this.isPassingDataToNextStep) {
      this.stockAdjustmentDetailsData = this.stockAdjustmentForm.get(
        "stockAdjustmentDetails"
      ).value;
    }
  }

  getAllStores(): void {
    let storeRequest = new StoreRequest();
    storeRequest.page = -1;
    this.storeService.getStores(storeRequest).subscribe((res) => {
      this.stores = res?.data?.item1;
    });
  }

  onSelectedStore(storeId) {
    this.getProducts(this.stores.find((x) => x.id == storeId)?.inventoryTypeId);
    this.resetProductInStockAdjustmentDetails(this.stockAdjustmentDetails);
  }

  resetProductInStockAdjustmentDetails(arr: FormArray) {
    for (let i = 0; i <= arr.length; i++) {
      const stockAdjustmentDetail = arr.at(i);
      stockAdjustmentDetail?.patchValue({
        productId: null,
      });
    }
  }

  getStoreName(storeId) {
    return this.stores?.find((x) => x?.id === storeId)?.name;
  }

  getStockAdjustmentStatus(value) {
    return this.statusColorService.getStockAdjustmentStatus(value);
  }

  getStockAdjustmentStatusName(value: number) {
    return StockAdjustmentStatus[value];
  }

  getProducts(inventoryTypeId: string): void {
    let productRequest = new ProductRequest();
    productRequest.inventoryTypeIds = [inventoryTypeId];
    productRequest.page = -1;
    this.productService.getProducts(productRequest).subscribe((res) => {
      this.filterProducts = this.products = res?.data?.item1;
    });
  }

  getProductName(productId: string) {
    const product =
      this.products?.find((product) => product?.id === productId) ||
      this.data?.stockAdjustmentDetails?.find((x) => x?.productId == productId)
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

  clearInputFromDetails(evt: any, itemIndex: number): void {
    evt.stopPropagation();
    const particularDetail = this.stockAdjustmentDetails.at(itemIndex);
    particularDetail?.patchValue({
      productId: null,
    });
  }

  handleProductSearch(event: any, itemIndex: number) {
    const name = event?.target?.name;
    if (name === "productId") {
      const term = this.stockAdjustmentDetails.at(itemIndex).get("productId");
      this.filterProduct(term.value || "");
    }
  }

  filterProduct(searchTerm: string) {
    searchTerm = searchTerm.toLowerCase();
    this.filterProducts = this.products?.filter((product) =>
      product.name.toLowerCase().startsWith(searchTerm)
    );
  }

  handleProductSelection(event, index) {
    const particularDetail = this.stockAdjustmentDetails.at(index);
    const productId = event?.option?.value;
    const storeId = this.stockAdjustmentForm.get("storeId")?.value;

    if (!storeId) {
      this.showSnackBar("Please select a Store first!");
      particularDetail?.patchValue({
        productId: null,
      });
      return;
    }

    if (this.isExistSelectedProduct(productId, index)) {
      this.showSnackBar("This Product already added!");
      particularDetail?.patchValue({
        rawMaterialId: null,
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

    this.stockService
      .getItemStock(productId, this.stockAdjustmentForm.get("storeId")?.value)
      .subscribe((responseStockQuantity) => {
        particularDetail.patchValue({
          currentStockQuantity: responseStockQuantity,
        });
      });
  }

  showSnackBar(message: string) {
    this.snackBarService.openSnackbar(message);
  }

  isExistSelectedProduct(productId: string, index: number): boolean {
    const isProductAdded = this.stockAdjustmentDetails.value.some((item, i) => {
      return item.productId === productId && i !== index;
    });
    return isProductAdded;
  }

  onControlChange(event: any, itemIndex?: number): void {
    const name = event?.target?.name;
    if (name === "adjustmentQty") {
      this.calculateTotalAdjustmentQty();
    }
  }

  calculateTotalAdjustmentQty() {
    const sumQty = this.stockAdjustmentDetails?.value?.reduce(
      (sum, item) => sum + item?.adjustmentQty,
      0
    );
    this.stockAdjustmentForm.get("totalAdjustmentQty").setValue(sumQty);
  }

  private handleSuccessfulSave(res: GeneralResponse<string>): void {
    if (res?.succeeded) {
      this.handleStockAdjustmentResponse(res?.data);
      this.toastr.success(res?.message);
      this.deletedIds = "";
    } else {
      this.toastr.error(res?.message);
      this.isLoading = false;
    }
  }

  private handleStockAdjustmentResponse(stockAdjustmentId: string): void {
    this.stockAdjustmentService
      .getStockAdjustmentById(stockAdjustmentId)
      .subscribe({
        next: (stockAdjustmentResponse) => {
          this.data = stockAdjustmentResponse.data;
          this.isViewMode = true;
          this.isLoading = false;
          this.navigateToView(stockAdjustmentResponse?.data?.id);
        },
        error: (err) => {
          location.reload();
        },
      });
  }

  public navigateToView(id: string) {
    this.router.navigate([this.path.edit, id]);
    this.cdRef.detectChanges();
    this.btnCheck.focus();
  }

  navigateToList() {
    this.router.navigate([this.path.list]);
  }

  backToView() {
    this.initializeForm();
    this.isViewMode = true;
    this.stepper.next();
  }

  private addStockAdjustment(body: StockAdjustmentRequestDTO): void {
    this.stockAdjustmentService
      .createStockAdjustment(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  private updateStockAdjustment(body: StockAdjustmentRequestDTO): void {
    this.stockAdjustmentService
      .updateStockAdjustment(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  deletedIds: string = "";
  onDeleteItem(id: string, itemIndex: number): void {
    if (id) this.deletedIds += `${id},`;
    this.stockAdjustmentDetails.removeAt(itemIndex);
    this.calculateTotalAdjustmentQty();
  }

  onSubmit(): void {
    if (this.stockAdjustmentForm.valid) {
      this.isLoading = true;
      const formValue = this.stockAdjustmentForm.value;
      if (!formValue.id) {
        this.addStockAdjustment(formValue);
      } else {
        formValue.deletedStockAdjustmentDetailIds = this.deletedIds;
        this.updateStockAdjustment(formValue);
      }
    }
  }

  check(id: string) {
    if (this.isLoading) return;
    this.isLoading = true;

    this.stockAdjustmentService
      .checkStockAdjustment(id)
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

    this.stockAdjustmentService
      .approveStockAdjustment(id)
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

    this.stockAdjustmentService
      .unpostStockAdjustment(id, status)
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
      .get(environment.apiURL + "/ReportStockAdjustment/" + id, {
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
