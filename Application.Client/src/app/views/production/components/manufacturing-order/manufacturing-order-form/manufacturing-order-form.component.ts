import { HttpClient } from "@angular/common/http";
import { ChangeDetectorRef, Component, OnInit, ViewChild } from "@angular/core";
import { FormArray, FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MatButton } from "@angular/material/button";
import { MatDialog } from "@angular/material/dialog";
import { MatStepper } from "@angular/material/stepper";
import { ActivatedRoute, Router } from "@angular/router";
import {
  Inventory_Type_Id_Finished_Goods,
  Inventory_Type_Id_Other_Items,
  Inventory_Type_Id_Raw_Materials,
} from "app/shared/consts/const";
import { ManufacturingOrderStatus } from "app/shared/enums/manufacturingOrderStatus";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { compareFieldsValidator } from "app/shared/validators/compare-fields-validators";
import { inFinancialYearValidator } from "app/shared/validators/in-financial-year-validator";
import { CostCenter } from "app/views/configuration/models/cost-center/cost-center.model";
import { ProductRequest } from "app/views/configuration/models/product/product-request.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { StoreRequest } from "app/views/configuration/models/store/store-request.model";
import { Store } from "app/views/configuration/models/store/store.model";
import { CostCenterService } from "app/views/configuration/services/cost-center.service";
import { ProductService } from "app/views/configuration/services/product.service";
import { StoreService } from "app/views/configuration/services/store.service";
import { BillOfMaterialResponseDetail } from "app/views/production/models/bill-of-material/bill-of-material-response-dto.model";
import {
  ManufacturingOrderRequestDTO,
  ManufacturingOrderRequestDetail,
} from "app/views/production/models/manufacturing-order/manufacturing-order-request-dto.model";
import {
  ManufacturingOrderResponseDTO,
  ManufacturingOrderResponseDetail,
} from "app/views/production/models/manufacturing-order/manufacturing-order-response-dto.model";
import { ManufacturingOrderService } from "app/views/production/services/manufacturing-order.service";
import { StockService } from "app/views/report/services/stock.service";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { finalize, forkJoin } from "rxjs";
import { BillOfMaterialListComponent } from "../bill-of-material-list/bill-of-material-list.component";

@Component({
  selector: "app-manufacturing-order-form",
  templateUrl: "./manufacturing-order-form.component.html",
  styleUrls: ["./manufacturing-order-form.component.scss"],
})
export class ManufacturingOrderFormComponent implements OnInit {
  @ViewChild("btnCheck") btnCheck: MatButton;
  @ViewChild("btnApprove") btnApprove: MatButton;
  @ViewChild("stepper") private stepper: MatStepper;
  isLoading: boolean = false;
  isDialogOpen: boolean = false;
  isBomItemLoading: boolean = false;
  isViewMode: boolean = true;
  isChecked: boolean;
  formTitle: string;
  manufacturingOrderForm: FormGroup;
  costCenters: CostCenter[];
  rawMaterialStores: Store[];
  finishedProducts: ProductView[];
  rawMaterials: ProductView[];
  manufacturingOrderDetailsData: any[] = [];
  data: ManufacturingOrderResponseDTO;
  // Define financial year date range here
  financialYearStartDate: Date; // Jul 1, 2023
  financialYearEndDate: Date; // Jun 30, 2024
  currentFinancialYearId: string;
  businessType: string;
  applyMinMax: boolean = false;

  private path = {
    list: "production/manufacturing-order",
    edit: "production/manufacturing-order",
  };

  constructor(
    private fb: FormBuilder,
    private dateFormatService: DateTimeFormatService,
    private jwtAuth: JwtAuthService,
    private productService: ProductService,
    private storeService: StoreService,
    private costCenterService: CostCenterService,
    private manufacturingOrderService: ManufacturingOrderService,
    private stockService: StockService,
    private toastr: ToastrService,
    private http: HttpClient,
    private activatedRoute: ActivatedRoute,
    private router: Router,
    private cdRef: ChangeDetectorRef,
    public statusColorService: StatusColorService,
    private dialog: MatDialog,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.activatedRoute.data.subscribe((response: any) => {
      this.data = response?.manufacturingOrder?.data;
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
    this.getAllFinishedProducts("");
    this.getAllRawMaterials("");
    this.getAllRawMaterialStores();
  }

  initializeForm() {
    this.createForm();
    this.populateForm();
    this.setFormTitle();
    this.manufacturingOrderForm.get("scheduledDate").markAsTouched();
  }

  createForm(): void {
    this.manufacturingOrderForm = this.fb.group({
      id: [this.data?.id || null],
      manufacturingOrderNo: [this.data?.manufacturingOrderNo || ""],
      bomNo: [this.data?.bomNo || "", Validators.required],
      scheduledDate: [
        this.data?.scheduledDate || this.dateFormatService.getPresentDate(),
      ],
      formulationNo: [this.data?.formulationNo || "", Validators.required],
      finishedProductId: [
        this.data?.finishedProductId || null,
        Validators.required,
      ],
      productionQuantity: [
        this.data?.productionQuantity || 0,
        Validators.required,
      ],
      rawMaterialStoreId: [
        this.data?.rawMaterialStoreId || null,
        Validators.required,
      ],
      totalCost: [this.data?.totalCost || 0, Validators.required],
      totalRmused: [this.data?.totalRmused || 0, Validators.required],
      remark: [this.data?.remark || ""],
      deletedManufacturingOrderDetailIds: [""],
      manufacturingOrderDetails: this.fb.array([]),
    });
  }

  setMinMaxDates() {
    if (this.applyMinMax) {
      this.manufacturingOrderForm
        .get("scheduledDate")
        .setValidators([
          Validators.required,
          inFinancialYearValidator(
            this.financialYearStartDate,
            this.financialYearEndDate
          ),
        ]);
    } else {
      this.manufacturingOrderForm
        .get("scheduledDate")
        .setValidators([Validators.required]);
    }

    this.manufacturingOrderForm.get("scheduledDate").updateValueAndValidity();
  }

  get manufacturingOrderDetails(): FormArray {
    return this.manufacturingOrderForm.get(
      "manufacturingOrderDetails"
    ) as FormArray;
  }

  populateForm(): void {
    if (this.manufacturingOrderForm.get("id").value) {
      this.populateManufacturingOrderDetails(this.data);
    } else {
      this.addItem();
    }
  }

  setFormTitle(): void {
    if (this.manufacturingOrderForm.get("id").value) {
      this.formTitle = "Edit Manufacturing Order";
    } else {
      this.formTitle = "Add Manufacturing Order";
    }
  }

  populateManufacturingOrderDetails(data: ManufacturingOrderResponseDTO): void {
    data.manufacturingOrderDetails.forEach(
      (item: ManufacturingOrderResponseDetail) => this.addItem(item)
    );
  }

  addItem(item?: ManufacturingOrderResponseDetail): void {
    this.manufacturingOrderDetails.push(
      this.createManufacturingOrderDetail(item)
    );
  }

  createManufacturingOrderDetail(
    item?: ManufacturingOrderRequestDetail
  ): FormGroup {
    return this.fb.group(
      {
        id: [item?.id || null],
        rawMaterialId: [item?.rawMaterialId || "", Validators.required],
        measurementUnitName: [item?.rawMaterial?.measurementUnit?.name || ""],
        stockQuantity: [item?.stockQuantity || 0],
        percentage: [item?.percentage || 0],
        rawMaterial: [item?.rawMaterial || ""],
        quantity: [item?.quantity || 0, Validators.required],
        amount: [item?.amount || 0, Validators.required],
      },
      { validators: compareFieldsValidator("stockQuantity", "quantity") }
    );
  }

  isPassingDataToNextStep = false;
  handleStepChange(event: any) {
    this.isPassingDataToNextStep = event.selectedIndex === 1;
    if (this.isPassingDataToNextStep) {
      this.manufacturingOrderDetailsData = this.manufacturingOrderForm.get(
        "manufacturingOrderDetails"
      ).value;
    }
  }

  getAllRawMaterialStores(): void {
    let storeRequest = new StoreRequest();
    storeRequest.page = -1;
    storeRequest.inventoryTypeId = Inventory_Type_Id_Raw_Materials;
    this.storeService.getStores(storeRequest).subscribe((res) => {
      this.rawMaterialStores = res?.data?.item1;
    });
  }

  getStoreName(rawMaterialStoreId) {
    return this.rawMaterialStores?.find((x) => x?.id === rawMaterialStoreId)
      ?.name;
  }

  getManufacturingOrderStatus(value) {
    return this.statusColorService.getManufacturingOrderStatus(value);
  }

  getManufacturingOrderStatusName(value: number) {
    return ManufacturingOrderStatus[value];
  }

  getAllFinishedProducts(keyword: string): void {
    let productRequest = new ProductRequest();
    productRequest.inventoryTypeId = Inventory_Type_Id_Finished_Goods;
    productRequest.keyword = keyword;
    productRequest.page = -1;
    this.productService.getProducts(productRequest).subscribe((res) => {
      this.finishedProducts = res?.data?.item1;
    });
  }

  getFinishedProductName(productId: string) {
    const product = this.finishedProducts?.find((x) => x.id === productId);
    if (!product) return "";
    return `${product.name}${
      this.businessType === "1" ? ` (${product.packSize?.name})` : ""
    }`;
  }

  getAllRawMaterials(keyword: string): void {
    let productRequest = new ProductRequest();
    productRequest.inventoryTypeIds = [
      Inventory_Type_Id_Raw_Materials,
      Inventory_Type_Id_Other_Items,
    ];
    productRequest.keyword = keyword;
    productRequest.page = -1;
    this.productService.getProducts(productRequest).subscribe((res) => {
      this.rawMaterials = res?.data?.item1;
    });
  }

  getRawMaterialProductName(productId: string) {
    const product =
      this.rawMaterials?.find((product) => product?.id === productId) ||
      this.data?.manufacturingOrderDetails?.find(
        (x) => x?.rawMaterialId == productId
      )?.rawMaterial;
    return product?.name;
  }

  // findProductById(id: string) {
  //   return this.rawMaterials?.find((product) => product?.id === id);
  // }

  // private filterProduct(value: string) {
  //   this.getAllFinishedProducts(value.toLowerCase());
  // }

  calculateQuantity(itemIndex: number) {
    const item = this.manufacturingOrderDetails.at(itemIndex);
    const quantity = item.get("quantity");
    const percentage = item.get("percentage");
    const productionQuantity =
      this.manufacturingOrderForm.get("productionQuantity")?.value;
    const percentageValue = percentage?.value;
    const calculatedQuantity = (percentageValue * productionQuantity) / 100;
    const roundedQuantity = parseFloat(calculatedQuantity.toFixed(6));
    quantity?.setValue(roundedQuantity);
  }

  onControlChange(event: any, itemIndex?: number): void {
    const name = event?.target?.name;
    if (name === "quantity") {
      this.calculateUsedRMQty();
    } else if (name === "productionQuantity") {
      this.manufacturingOrderDetails.value.map((item, index) => {
        this.calculateQuantity(index);
        this.calculateUsedRMQty();
      });
    }
  }

  calculateUsedRMQty() {
    const usedRMQty = this.manufacturingOrderDetails?.value?.reduce(
      (sum, item) => sum + item?.quantity,
      0
    );
    this.manufacturingOrderForm.get("totalCost").setValue(0);
    this.manufacturingOrderForm.get("totalRmused").setValue(usedRMQty);
  }

  private handleSuccessfulSave(res: GeneralResponse<string>): void {
    if (res?.succeeded) {
      this.handleManufacturingOrderResponse(res?.data);
      this.toastr.success(res?.message);
      this.deletedIds = "";
    } else {
      this.toastr.error(res?.message);
      this.isLoading = false;
    }
  }

  private handleManufacturingOrderResponse(manufacturingOrderId: string): void {
    this.manufacturingOrderService
      .getManufacturingOrderById(manufacturingOrderId)
      .subscribe({
        next: (manufacturingOrderResponse) => {
          this.data = manufacturingOrderResponse.data;
          this.isViewMode = true;
          this.isLoading = false;
          this.navigateToView(manufacturingOrderResponse?.data?.id);
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

  private addManufacturingOrder(body: ManufacturingOrderRequestDTO): void {
    this.manufacturingOrderService
      .createManufacturingOrder(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  private updateManufacturingOrder(body: ManufacturingOrderRequestDTO): void {
    this.manufacturingOrderService
      .updateManufacturingOrder(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  deletedIds: string = "";
  // onDeleteItem(id: string, itemIndex: number): void {
  //   if (id) this.deletedIds += `${id},`;
  //   this.manufacturingOrderDetails.removeAt(itemIndex);
  // }

  onSubmit(): void {
    if (this.manufacturingOrderForm.valid) {
      this.isLoading = true;
      const formValue = this.manufacturingOrderForm.value;
      if (!formValue.id) {
        this.addManufacturingOrder(formValue);
      } else {
        formValue.deletedManufacturingOrderDetailIds = this.deletedIds;
        this.updateManufacturingOrder(formValue);
      }
    }
  }

  //openBOMDialog
  openBOMDialog() {
    if (this.manufacturingOrderForm.get("rawMaterialStoreId")?.value) {
      this.isBomItemLoading = true;
      const dialogRef = this.dialog.open(BillOfMaterialListComponent, {
        disableClose: true,
        panelClass: "add-bill-container",
        minHeight: "auto",
        height: "auto",
      });
      dialogRef.afterClosed().subscribe((result) => {
        if (result) {
          this.manufacturingOrderForm.setControl(
            "manufacturingOrderDetails",
            this.fb.array([])
          );
          this.manufacturingOrderForm.markAllAsTouched();
          this.manufacturingOrderForm.patchValue({
            bomNo: result?.bomNo,
            formulationNo: result?.formulationNo,
            finishedProductId: result?.finishedProductId,
          });

          result?.billOfMaterialDetails.forEach(
            (item: BillOfMaterialResponseDetail) => {
              let manufacturingOrderResponseDetail: any = {
                rawMaterialId: item?.rawMaterialId,
                rawMaterial: item?.rawMaterial,
                percentage: item?.percentage,
              };
              this.addItem(manufacturingOrderResponseDetail);
            }
          );
          // Create an array to hold all the observables
          const observables = result?.billOfMaterialDetails.map(
            (item: BillOfMaterialResponseDetail) => {
              return this.stockService.getItemStock(
                item?.rawMaterialId,
                this.manufacturingOrderForm.get("rawMaterialStoreId")?.value
              );
            }
          );

          // Use forkJoin to wait for all observables to complete
          forkJoin(observables).subscribe((responseStockQuantities) => {
            // Now that all the observables have completed, you can set the status to "loaded"
            this.isBomItemLoading = false;
            this.manufacturingOrderForm.setControl(
              "manufacturingOrderDetails",
              this.fb.array([])
            );
            // Process the responseStockQuantities and addItem for each item
            result?.billOfMaterialDetails.forEach(
              (item: BillOfMaterialResponseDetail, index) => {
                const responseStockQuantity = responseStockQuantities[index];
                let manufacturingOrderResponseDetail: any = {
                  rawMaterialId: item?.rawMaterialId,
                  rawMaterial: item?.rawMaterial,
                  percentage: item?.percentage,
                  stockQuantity: responseStockQuantity,
                };
                this.addItem(manufacturingOrderResponseDetail);
              }
            );
          });
        } else {
          this.isBomItemLoading = false;
        }
      });
    } else alert("Please Select Raw Material Store First");
  }

  check(id: string) {
    if (this.isLoading) return;
    this.isLoading = true;

    this.manufacturingOrderService
      .checkManufacturingOrder(id)
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

    this.manufacturingOrderService
      .approveManufacturingOrder(id)
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

    this.manufacturingOrderService
      .unpostManufacturingOrder(id, status)
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

  printManufacturingOrderDetailsPdf(id, reportType: string = "1") {
    this.http
      .get(
        environment.apiURL +
          `/ReportManufacturingOrder/manufacturing-order-details/${id}/${reportType}`,
        {
          responseType: "blob",
        }
      )
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

  printManufacturingOrderSummaryPdf(id, reportType: string = "1") {
    this.http
      .get(
        environment.apiURL +
          `/ReportManufacturingOrder/manufacturing-order-summary/${id}/${reportType}`,
        {
          responseType: "blob",
        }
      )
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
