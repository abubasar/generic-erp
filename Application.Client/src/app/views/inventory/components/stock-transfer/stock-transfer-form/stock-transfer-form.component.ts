import { HttpClient } from "@angular/common/http";
import { ChangeDetectorRef, Component, OnInit, ViewChild } from "@angular/core";
import { FormArray, FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MatButton } from "@angular/material/button";
import { MatDialog } from "@angular/material/dialog";
import { MatStepper } from "@angular/material/stepper";
import { ActivatedRoute, Router } from "@angular/router";
import { Inventory_Type_Id_Finished_Goods } from "app/shared/consts/const";
import { StockTransferStatus } from "app/shared/enums/stockTransferStatus";
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
import { StockTransferRequestDTO } from "app/views/inventory/models/stock-transfer/stock-transfer-request-dto.model";
import {
  StockTransferResponseDTO,
  StockTransferResponseDetail,
} from "app/views/inventory/models/stock-transfer/stock-transfer-response-dto.model";
import { StockTransferService } from "app/views/inventory/services/stock-transfer.service";
import { StockService } from "app/views/report/services/stock.service";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { finalize } from "rxjs";

@Component({
  selector: "app-stock-transfer-form",
  templateUrl: "./stock-transfer-form.component.html",
  styleUrls: ["./stock-transfer-form.component.scss"],
})
export class StockTransferFormComponent implements OnInit {
  @ViewChild("btnCheck") btnCheck: MatButton;
  @ViewChild("btnApprove") btnApprove: MatButton;
  @ViewChild("stepper") private stepper: MatStepper;
  isLoading: boolean = false;
  isDialogOpen: boolean = false;
  isViewMode: boolean = true;
  isChecked: boolean;
  formTitle: string;
  stockTransferForm: FormGroup;
  products: ProductView[];
  searchProducts: ProductView[];
  stores: Store[];
  stockTransferDetailsData: any[] = [];
  data: StockTransferResponseDTO;
  businessType: string;
  // Define financial year date range here
  financialYearStartDate: Date; // Jul 1, 2023
  financialYearEndDate: Date; // Jun 30, 2024
  currentFinancialYearId: string;
  applyMinMax: boolean = false;

  private path = {
    list: "inventory/stock-transfer",
    edit: "inventory/stock-transfer",
  };

  constructor(
    private fb: FormBuilder,
    private productService: ProductService,
    private stockTransferService: StockTransferService,
    private storeService: StoreService,
    private stockService: StockService,
    private dateFormatService: DateTimeFormatService,
    private jwtAuth: JwtAuthService,
    private toastr: ToastrService,
    private activatedRoute: ActivatedRoute,
    private router: Router,
    private http: HttpClient,
    // private localStorageService: LocalStoreService,
    private cdRef: ChangeDetectorRef,
    public statusColorService: StatusColorService,
    private dialog: MatDialog,
    private snackBarService: SnackBarService,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.activatedRoute.data.subscribe((response: any) => {
      this.data = response?.stockTransfer?.data;
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
    this.getAllProducts();
    this.getAllStores();
  }

  initializeForm() {
    this.createForm();
    this.populateForm();
    this.setFormTitle();
    this.stockTransferForm.get("transferDate").markAsTouched();
  }

  createForm(): void {
    this.stockTransferForm = this.fb.group({
      id: [this.data?.id || null],
      sourceId: [this.data?.sourceId || null, Validators.required],
      source: [this.data?.source || null],
      destinationId: [this.data?.destinationId || null, Validators.required],
      destination: [this.data?.destination || null],
      transferNo: [this.data?.transferNo || null],
      transferDate: [
        this.data?.transferDate || this.dateFormatService.getPresentDate(),
      ],
      truckNo: [this.data?.truckNo || ""],
      remark: [this.data?.remark || ""],
      deletedStockTransferDetailIds: [""],
      stockTransferDetails: this.fb.array([]),
    });
    this.setMinMaxDates();
  }

  setMinMaxDates() {
    if (this.applyMinMax) {
      this.stockTransferForm
        .get("transferDate")
        .setValidators([
          Validators.required,
          inFinancialYearValidator(
            this.financialYearStartDate,
            this.financialYearEndDate
          ),
        ]);
    } else {
      this.stockTransferForm
        .get("transferDate")
        .setValidators([Validators.required]);
    }

    this.stockTransferForm.get("transferDate").updateValueAndValidity();
  }

  get stockTransferDetails(): FormArray {
    return this.stockTransferForm.get("stockTransferDetails") as FormArray;
  }

  populateForm(): void {
    if (this.stockTransferForm.get("id").value) {
      this.populateStockTransferDetails(this.data);
    } else {
      this.addItem();
    }
  }

  setFormTitle(): void {
    if (this.stockTransferForm.get("id").value) {
      this.formTitle = "Edit Stock Transfer";
    } else {
      this.formTitle = "Add Stock Transfer";
    }
  }

  populateStockTransferDetails(data: StockTransferResponseDTO): void {
    data.stockTransferDetails.forEach((item: StockTransferResponseDetail) =>
      this.addItem(item)
    );
  }

  addItem(item?: StockTransferResponseDetail): void {
    this.stockTransferDetails.push(this.createStockTransferDetail(item));
  }

  createStockTransferDetail(item?: StockTransferResponseDetail): FormGroup {
    return this.fb.group({
      id: [item?.id || null],
      productId: [item?.productId || null, Validators.required],
      product: [item?.product ? item?.product : ""],
      measurementUnitName: [
        item?.product?.measurementUnit?.name
          ? item?.product?.measurementUnit?.name
          : "",
      ],
      bagWeight: [item?.product?.bagWeight || 0],
      transferBagQuantity: [
        item?.transferBagQuantity ?? 0,
        Validators.required,
      ],
      transferQuantity: [
        item?.transferQuantity ? item?.transferQuantity : 0,
        Validators.required,
      ],
      currentStockQuantity: [item?.currentStockQuantity || 0],
    });
  }

  isPassingDataToNextStep = false;
  handleStepChange(event: any) {
    this.isPassingDataToNextStep = event.selectedIndex === 1;
    if (this.isPassingDataToNextStep) {
      this.stockTransferDetailsData = this.stockTransferForm.get(
        "stockTransferDetails"
      ).value;
    }
  }

  getStockTransferStatus(value) {
    return this.statusColorService.getStockTransferStatus(value);
  }

  getStockTransferStatusName(value: number) {
    return StockTransferStatus[value];
  }

  getAllStores(): void {
    let request = new StoreRequest();
    request.page = -1;
    this.storeService.getStores(request).subscribe((res) => {
      this.stores = res?.data?.item1;
    });
  }

  getStoreName(id) {
    return this.stores?.find((x) => x.id === id).name;
  }

  getAllProducts(): void {
    let productRequest = new ProductRequest();
    productRequest.page = -1;
    this.productService.getProducts(productRequest).subscribe((res) => {
      this.products = res?.data?.item1;
      if (this.data?.sourceId) {
        this.loadProductsByInventoryType(this.data?.source?.inventoryTypeId);
      } else {
        this.searchProducts = this.products;
      }
    });
  }

  loadProductsByInventoryType(inventoryTypeId: string): void {
    this.searchProducts = this.products.filter(
      (product) => product.inventoryTypeId === inventoryTypeId
    );
  }

  getProductName(productId: string) {
    const product =
      this.products?.find((product) => product?.id === productId) ||
      this.data?.stockTransferDetails?.find((x) => x?.productId == productId)
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

  findStoreById(id: string) {
    return this.stores?.find((store) => store?.id === id);
  }

  handleSourceSelection(event: any, targetName) {
    const fromStoreId = this.stockTransferForm.get("sourceId")?.value;
    const toStoreId = this.stockTransferForm.get("destinationId")?.value;

    const selectedStore = this.findStoreById(event?.value);
    if (!selectedStore) return;

    if (targetName === "sourceId") {
      if (toStoreId && this.isFromStoreExistSelectedStore(fromStoreId)) {
        this.showSnackBar("This Store already added in 'Destination'");
        this.stockTransferForm.patchValue({
          sourceId: null,
          source: null,
        });
        return;
      } else if (
        toStoreId &&
        !this.isSameInventoryType(selectedStore, toStoreId)
      ) {
        this.showSnackBar(
          "Source and Destination stores must have the same Inventory Type"
        );
        this.stockTransferForm.patchValue({
          sourceId: null,
          source: null,
        });
        return;
      } else {
        this.stockTransferForm.patchValue({
          sourceId: selectedStore?.id,
          source: selectedStore,
        });
        // Load products based on the source store inventory type
        this.loadProductsByInventoryType(selectedStore.inventoryTypeId);
        this.resetProductInStockTransferDetails(this.stockTransferDetails);
      }
    }

    if (targetName === "destinationId") {
      if (fromStoreId && this.isToStoreExistSelectedStore(toStoreId)) {
        this.showSnackBar("This Store already added in 'Source'");
        this.stockTransferForm.patchValue({
          destinationId: null,
          destination: null,
        });
        return;
      } else if (
        fromStoreId &&
        !this.isSameInventoryType(selectedStore, fromStoreId)
      ) {
        this.showSnackBar(
          "Source and Destination stores must have the same Inventory Type"
        );
        this.stockTransferForm.patchValue({
          destinationId: null,
          destination: null,
        });
        return;
      } else {
        this.stockTransferForm.patchValue({
          destinationId: selectedStore?.id,
          destination: selectedStore,
        });
      }
    }
  }

  resetProductInStockTransferDetails(arr: FormArray) {
    for (let i = 0; i <= arr.length; i++) {
      const stockTransferDetails = arr.at(i);
      stockTransferDetails?.patchValue({
        productId: null,
      });
    }
  }

  isFinishedGoodsInventoryType(): boolean {
    const sourceId = this.stockTransferForm.get("sourceId")?.value;
    if (!sourceId) return false;
    const selectedStore = this.findStoreById(sourceId);
    return selectedStore?.inventoryTypeId === Inventory_Type_Id_Finished_Goods;
  }

  isSameInventoryType(selectedStore: any, otherStoreId: string): boolean {
    const otherStore = this.findStoreById(otherStoreId);
    return (
      otherStore && selectedStore.inventoryTypeId === otherStore.inventoryTypeId
    );
  }

  isFromStoreExistSelectedStore(sourceId: string): boolean {
    return this.stockTransferForm.get("destinationId")?.value === sourceId;
  }

  isToStoreExistSelectedStore(destinationId: string): boolean {
    return this.stockTransferForm.get("sourceId")?.value === destinationId;
  }

  handleProductSelection(event, index) {
    const particularDetail = this.stockTransferDetails.at(index);
    const productId = event?.option?.value;
    const storeId = this.stockTransferForm.get("sourceId")?.value;

    if (!storeId) {
      this.showSnackBar("Please select a Source first!");
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
      measurementUnitName: selectedProduct?.measurementUnit?.name,
      bagWeight: selectedProduct.bagWeight,
      transferBagQuantity: 0,
      transferQuantity: 0,
    });

    this.stockService
      .getItemStock(productId, this.stockTransferForm.get("sourceId")?.value)
      .subscribe((responseStockQuantity) => {
        console.log("responseStockQuantity", responseStockQuantity);
        particularDetail.patchValue({
          currentStockQuantity: responseStockQuantity,
        });
      });
  }

  showSnackBar(message: string) {
    this.snackBarService.openSnackbar(message);
  }

  isExistSelectedProduct(productId: string, index: number): boolean {
    const isProductAdded = this.stockTransferDetails.value.some((item, i) => {
      return item.productId === productId && i !== index;
    });
    return isProductAdded;
  }

  clearInputFromDetails(evt: any, itemIndex: number): void {
    evt.stopPropagation();
    const particularDetail = this.stockTransferDetails.at(itemIndex);
    particularDetail?.patchValue({
      productId: null,
    });
  }

  handleProductSearch(event: any, itemIndex: number) {
    const name = event?.target?.name;
    if (name === "productId") {
      const term = this.stockTransferDetails.at(itemIndex).get("productId");
      this.filterProduct(term.value || "");
    }
  }

  filterProduct(searchTerm: string) {
    searchTerm = searchTerm.trim().toLowerCase();
    const sourceId = this.stockTransferForm?.get("sourceId")?.value;
    if (sourceId) {
      const inventoryTypeId = this.stores.find(
        (store) => store.id === sourceId
      )?.inventoryTypeId;
      this.searchProducts = this.products
        ?.filter((product) => product.inventoryTypeId == inventoryTypeId)
        .filter((product) => product.name.toLowerCase().startsWith(searchTerm));
    } else {
      this.searchProducts = this.products?.filter((product) =>
        product.name.toLowerCase().startsWith(searchTerm)
      );
    }
  }

  onControlChange(event: any, itemIndex?: number): void {
    const name = event?.target?.name;
    if (name === "transferBagQuantity") {
      this.calculateTransferQuantity(itemIndex);
    }
  }

  calculateTransferQuantity(itemIndex: number) {
    const item = this.stockTransferDetails.at(itemIndex);
    const bagWeight = item.get("bagWeight");
    const bagQuantity = item.get("transferBagQuantity");
    const quantity = item.get("transferQuantity");
    if (this.isFinishedGoodsInventoryType) {
      quantity?.setValue(bagWeight.value * bagQuantity.value);
    }
  }

  private handleSuccessfulSave(res: GeneralResponse<string>): void {
    if (res?.succeeded) {
      this.handleStockTransferResponse(res?.data);
      this.toastr.success(res?.message);
      this.deletedIds = "";
    } else {
      this.isLoading = false;
      this.toastr.error(res?.message);
    }
  }

  private handleStockTransferResponse(stockTransferId: string): void {
    this.stockTransferService.getStockTransferById(stockTransferId).subscribe({
      next: (stockTransferResponse) => {
        this.data = stockTransferResponse.data;
        this.isViewMode = true;
        this.isLoading = false;
        this.navigateToView(stockTransferResponse?.data?.id);
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

  public navigateToList() {
    this.router.navigate([this.path.list]);
  }

  backToView() {
    this.initializeForm();
    this.stepper.next();
    this.isViewMode = true;
  }

  private addStockTransfer(body: StockTransferRequestDTO): void {
    this.stockTransferService
      .createStockTransfer(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  private updateStockTransfer(body: StockTransferRequestDTO): void {
    this.stockTransferService
      .updateStockTransfer(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  deletedIds: string = "";
  onDeleteItem(id: string, itemIndex: number): void {
    if (id) this.deletedIds += `${id},`;
    this.stockTransferDetails.removeAt(itemIndex);
  }

  onSubmit(): void {
    if (this.stockTransferForm.valid) {
      this.isLoading = true;
      const formValue = this.stockTransferForm.value;
      if (!formValue.id) {
        this.addStockTransfer(formValue);
      } else {
        formValue.deletedStockTransferDetailIds = this.deletedIds;
        this.updateStockTransfer(formValue);
      }
    }
  }

  check(id: string) {
    if (this.isLoading) return;
    this.isLoading = true;

    this.stockTransferService
      .checkStockTransfer(id)
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

    this.stockTransferService
      .approveStockTransfer(id)
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

    this.stockTransferService
      .unpostStockTransfer(id, status)
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

  printPdf(id) {
    this.http
      .get(environment.apiURL + "/ReportStockTransfer/" + id, {
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
