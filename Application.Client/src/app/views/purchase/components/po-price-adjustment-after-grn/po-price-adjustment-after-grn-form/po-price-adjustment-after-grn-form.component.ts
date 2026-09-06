import { HttpClient } from "@angular/common/http";
import { ChangeDetectorRef, Component, OnInit, ViewChild } from "@angular/core";
import { FormArray, FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MatButton } from "@angular/material/button";
import { MatDialog } from "@angular/material/dialog";
import { MatStepper } from "@angular/material/stepper";
import { ActivatedRoute, Router } from "@angular/router";
import { Inventory_Type_Id_Raw_Materials } from "app/shared/consts/const";
import { PoPriceAdjustmentAfterGrnStatus } from "app/shared/enums/poPriceAdjustmentAfterGrnStatus";
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
import { Supplier } from "app/views/configuration/models/supplier/supplier.model";
import { ProductService } from "app/views/configuration/services/product.service";
import { StoreService } from "app/views/configuration/services/store.service";
import { SupplierService } from "app/views/configuration/services/supplier.service";
import {
  GoodsReceiveNoteResponseDTO,
  GoodsReceiveNoteResponseDetail,
} from "app/views/purchase/models/goods-receive-note/goods-receive-note-response-dto.model";
import { PoPriceAdjustmentAfterGrnRequestDTO } from "app/views/purchase/models/po-price-adjustment-after-grn/po-price-adjustment-after-grn-request-dto.model";
import {
  PoPriceAdjustmentAfterGrnResponseDTO,
  PoPriceAdjustmentAfterGrnResponseDetail,
} from "app/views/purchase/models/po-price-adjustment-after-grn/po-price-adjustment-after-grn-response-dto.model";
import { PoPriceAdjustmentAfterGrnService } from "app/views/purchase/services/po-price-adjustment-after-grn.service";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { finalize } from "rxjs";
import { GrnDialogComponent } from "../grn-dialog/grn-dialog.component";

@Component({
  selector: "app-po-price-adjustment-after-grn-form",
  templateUrl: "./po-price-adjustment-after-grn-form.component.html",
  styleUrls: ["./po-price-adjustment-after-grn-form.component.scss"],
})
export class PoPriceAdjustmentAfterGrnFormComponent implements OnInit {
  @ViewChild("btnCheck") btnCheck: MatButton;
  @ViewChild("btnApprove") btnApprove: MatButton;
  @ViewChild("stepper") private stepper: MatStepper;
  isDataLoading: boolean = false;
  isLoading: boolean = false;
  isDialogOpen: boolean = false;
  isViewMode: boolean = true;
  isChecked: boolean;
  formTitle: string;
  poPriceAdjustmentAfterGrnForm: FormGroup;
  suppliers: Supplier[];
  filterSuppliers: Supplier[];
  stores: Store[];
  products: ProductView[];
  searchProducts: ProductView[];
  poPriceAdjustmentAfterGrnDetailsData: any[] = [];
  data: PoPriceAdjustmentAfterGrnResponseDTO;
  businessType: string;
  // Define financial year date range here
  financialYearStartDate: Date; // Jul 1, 2023
  financialYearEndDate: Date; // Jun 30, 2024
  currentFinancialYearId: string;
  applyMinMax: boolean = false;

  private path = {
    list: "/purchase/po-price-adjustment-after-grn",
    edit: "purchase/po-price-adjustment-after-grn",
  };

  constructor(
    public dialog: MatDialog,
    private fb: FormBuilder,
    private supplierService: SupplierService,
    private storeService: StoreService,
    private productService: ProductService,
    private poPriceAdjustmentAfterGrnService: PoPriceAdjustmentAfterGrnService,
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
      this.data = response.poPriceAdjustmentAfterGrn?.data;
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
    //! it's important after close openPoPriceAdjustmentAfterGrnDialog
    this.getAllProducts();
  }

  initializeForm() {
    this.createForm();
    this.populateForm();
    this.setFormTitle();
    this.poPriceAdjustmentAfterGrnForm.get("adjustmentDate").markAsTouched();
  }

  createForm(): void {
    this.poPriceAdjustmentAfterGrnForm = this.fb.group({
      id: [this.data?.id || null],
      referenceNo: [this.data?.referenceNo || ""],
      grnno: [this.data?.grnno ?? "", Validators.required],
      ponumber: [this.data?.ponumber || ""],
      adjustmentDate: [
        this.data?.adjustmentDate || this.dateFormatService.getPresentDate(),
      ],
      storeId: [this.data?.storeId ?? null, Validators.required],
      supplierId: [this.data?.supplierId ?? null, Validators.required],
      totalAmount: [this.data?.totalAmount ?? 0, Validators.required],
      remark: [this.data?.remark || ""],
      deletedPoPriceAdjustmentAfterGrnDetailIds: [""],
      poPriceAdjustmentAfterGrnDetails: this.fb.array([]),
    });
    this.setMinMaxDates();
  }

  setMinMaxDates() {
    if (this.applyMinMax) {
      this.poPriceAdjustmentAfterGrnForm
        .get("adjustmentDate")
        .setValidators([
          Validators.required,
          inFinancialYearValidator(
            this.financialYearStartDate,
            this.financialYearEndDate
          ),
        ]);
    } else {
      this.poPriceAdjustmentAfterGrnForm
        .get("adjustmentDate")
        .setValidators([Validators.required]);
    }

    this.poPriceAdjustmentAfterGrnForm
      .get("adjustmentDate")
      .updateValueAndValidity();
  }

  get poPriceAdjustmentAfterGrnDetails(): FormArray {
    return this.poPriceAdjustmentAfterGrnForm?.get(
      "poPriceAdjustmentAfterGrnDetails"
    ) as FormArray;
  }

  populateForm(): void {
    if (this.poPriceAdjustmentAfterGrnForm?.get("id").value) {
      this.populatePoPriceAdjustmentAfterGrnDetails(this.data);
    } else {
      this.addItem();
    }
  }

  setFormTitle(): void {
    if (this.poPriceAdjustmentAfterGrnForm.get("id").value) {
      this.formTitle = "Edit PO Price Adjustment After GRN";
    } else {
      this.formTitle = "Add PO Price Adjustment After GRN";
    }
  }

  populatePoPriceAdjustmentAfterGrnDetails(
    data: PoPriceAdjustmentAfterGrnResponseDTO
  ): void {
    data.poPriceAdjustmentAfterGrnDetails.forEach(
      (item: PoPriceAdjustmentAfterGrnResponseDetail) => this.addItem(item)
    );
  }

  addItem(item?: PoPriceAdjustmentAfterGrnResponseDetail): void {
    this.poPriceAdjustmentAfterGrnDetails.push(
      this.createPoPriceAdjustmentAfterGrnDetail(item)
    );
  }

  createPoPriceAdjustmentAfterGrnDetail(
    item?: PoPriceAdjustmentAfterGrnResponseDetail
  ): FormGroup {
    return this.fb.group({
      id: [item?.id ?? null],
      productId: [item?.productId ?? null, Validators.required],
      product: [item?.product ?? null],
      measurementUnitName: [item?.product?.measurementUnit?.name ?? ""],
      grnQuantity: [item?.grnQuantity ?? 0],
      grnRate: [item?.grnRate ?? 0],
      adjustmentQuantity: [item?.adjustmentQuantity ?? 0, Validators.required],
      adjustmentRate: [item?.adjustmentRate ?? 0, Validators.required],
      amount: [item?.amount ?? 0, Validators.required],
    });
  }

  isPassingDataToNextStep = false;
  handleStepChange(event: any) {
    this.isPassingDataToNextStep = event?.selectedIndex === 1;
    if (this.isPassingDataToNextStep) {
      this.poPriceAdjustmentAfterGrnDetailsData =
        this.poPriceAdjustmentAfterGrnForm.get(
          "poPriceAdjustmentAfterGrnDetails"
        ).value;
    }
  }

  getPoPriceAdjustmentAfterGrnById(id: string): void {
    this.poPriceAdjustmentAfterGrnService
      .getPoPriceAdjustmentAfterGrnById(id)
      .subscribe((res) => {
        this.data = res?.data;
        this.initializeForm();
      });
  }

  getPoPriceAdjustmentAfterGrnStatus(value) {
    return this.statusColorService.getPoPriceAdjustmentAfterGrnStatus(value);
  }

  getPoPriceAdjustmentAfterGrnStatusName(value: number) {
    return PoPriceAdjustmentAfterGrnStatus[value];
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    if (fieldName === "supplierId") {
      this.poPriceAdjustmentAfterGrnForm?.get("supplierId").setValue(null);
    }
  }

  handleSupplierSearch(event: any): void {
    const name = event.target?.name;
    if (name === "supplierId") {
      const term = this.poPriceAdjustmentAfterGrnForm.get("supplierId");
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
      this.data?.poPriceAdjustmentAfterGrnDetails?.find(
        (x) => x?.productId == productId
      )?.product;
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
    const particularDetail = this.poPriceAdjustmentAfterGrnDetails.at(index);
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
    const isProductAdded = this.poPriceAdjustmentAfterGrnDetails.value.some(
      (item, i) => {
        return item.productId === productId && i !== index;
      }
    );
    return isProductAdded;
  }

  handleProductSearch(event: any, itemIndex: number) {
    const name = event?.target?.name;
    if (name === "productId") {
      const term = this.poPriceAdjustmentAfterGrnDetails
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

  calculateCost() {
    //variance += (item.AdjustmentQuantity  item.GrnRate - item.AdjustmentQuantity  item.AdjustmentRate);
    const subtotal = this.poPriceAdjustmentAfterGrnDetails?.value?.reduce(
      (sum, item) =>
        sum +
        (item.adjustmentQuantity * item.grnRate -
          item.adjustmentQuantity * item.adjustmentRate),
      0
    );

    this.poPriceAdjustmentAfterGrnForm?.get("totalAmount")?.setValue(subtotal);
  }

  onControlChange(event: any, itemIndex?: number): void {
    const name = event?.target?.name;
    if (name === "adjustmentQuantity" || name === "adjustmentRate") {
      this.calculateAmount(itemIndex);
    }
  }

  calculateAmount(itemIndex: number) {
    const item = this.poPriceAdjustmentAfterGrnDetails?.at(itemIndex);
    const adjustmentQuantity = item?.get("adjustmentQuantity")?.value;
    const grnRate = item?.get("grnRate")?.value;
    const adjustmentRate = item?.get("adjustmentRate")?.value;
    const amount = item?.get("amount");
    amount?.setValue(
      adjustmentQuantity * grnRate - adjustmentQuantity * adjustmentRate
    );
    this.calculateCost();
  }

  private handleSuccessfulSave(res: GeneralResponse<string>): void {
    if (res?.succeeded) {
      this.handlePoPriceAdjustmentAfterGrnResponse(res?.data);
      this.toastr.success(res?.message);
      this.deletedIds = "";
    } else {
      this.isLoading = false;
      this.toastr.success(res?.message);
    }
  }

  private handlePoPriceAdjustmentAfterGrnResponse(
    poPriceAdjustmentAfterGrnId: string
  ): void {
    this.poPriceAdjustmentAfterGrnService
      .getPoPriceAdjustmentAfterGrnById(poPriceAdjustmentAfterGrnId)
      .subscribe({
        next: (poPriceAdjustmentAfterGrnResponse) => {
          this.data = poPriceAdjustmentAfterGrnResponse.data;
          this.isViewMode = true;
          this.isLoading = false;
          this.navigateToView(poPriceAdjustmentAfterGrnResponse?.data?.id);
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

  private addPoPriceAdjustmentAfterGrn(
    body: PoPriceAdjustmentAfterGrnRequestDTO
  ): void {
    this.poPriceAdjustmentAfterGrnService
      .createPoPriceAdjustmentAfterGrn(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  private updatePoPriceAdjustmentAfterGrn(
    body: PoPriceAdjustmentAfterGrnRequestDTO
  ): void {
    this.poPriceAdjustmentAfterGrnService
      .updatePoPriceAdjustmentAfterGrn(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  deletedIds: string = "";
  onDeleteItem(id: string, itemIndex: number): void {
    if (id) this.deletedIds += `${id},`;
    this.poPriceAdjustmentAfterGrnDetails.removeAt(itemIndex);
    this.calculateCost();
  }

  onSubmit(): void {
    if (this.poPriceAdjustmentAfterGrnForm.valid) {
      this.isLoading = true;
      const formValue = this.poPriceAdjustmentAfterGrnForm.value;
      if (!formValue.id) {
        this.addPoPriceAdjustmentAfterGrn(formValue);
      } else {
        formValue.deletedPoPriceAdjustmentAfterGrnDetailIds = this.deletedIds;
        this.updatePoPriceAdjustmentAfterGrn(formValue);
      }
    }
  }

  //openGRNDialog
  openGRNDialog() {
    const dialogRef = this.dialog.open(GrnDialogComponent, {
      disableClose: true,
      panelClass: "add-bill-container",
      minHeight: "auto",
      height: "auto",
    });
    dialogRef.afterClosed().subscribe((result: GoodsReceiveNoteResponseDTO) => {
      if (!result) {
        return;
      }

      this.poPriceAdjustmentAfterGrnForm.reset();
      this.poPriceAdjustmentAfterGrnForm.setControl(
        "poPriceAdjustmentAfterGrnDetails",
        this.fb.array([])
      );
      this.poPriceAdjustmentAfterGrnForm.markAllAsTouched();

      this.poPriceAdjustmentAfterGrnForm.patchValue({
        grnno: result?.grnno,
        ponumber: result?.ponumber,
        storeId: result?.storeId,
        supplierId: result?.supplierId,
        adjustmentDate: this.dateFormatService.getPresentDate(),
        remark: result?.remark,
        totalAmount: 0,
      });
      result.goodsReceiveNoteDetails.forEach(
        (item: GoodsReceiveNoteResponseDetail) => {
          let poPriceAdjustmentAfterGrnResponseDetail: any = {
            productId: item?.productId,
            product: item?.product,
            grnQuantity: item?.grnquantity,
            grnRate: item?.rate,
            adjustmentQuantity: 0,
            adjustmentRate: 0,
            amount: 0,
          };
          this.addItem(poPriceAdjustmentAfterGrnResponseDetail);
        }
      );
    });
  }

  check(id: string) {
    if (this.isLoading) return;
    this.isLoading = true;

    this.poPriceAdjustmentAfterGrnService
      .checkPoPriceAdjustmentAfterGrn(id)
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

    this.poPriceAdjustmentAfterGrnService
      .approvePoPriceAdjustmentAfterGrn(id)
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

    this.poPriceAdjustmentAfterGrnService
      .unpostPoPriceAdjustmentAfterGrn(id, status)
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
      .get(environment.apiURL + "/ReportPoPriceAdjustmentAfterGrn/" + id, {
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
