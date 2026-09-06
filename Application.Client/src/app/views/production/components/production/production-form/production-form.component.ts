import {
  NGX_MAT_DATE_FORMATS,
  NgxMatDateAdapter,
  NgxMatDateFormats,
} from "@angular-material-components/datetime-picker";
import { HttpClient } from "@angular/common/http";
import { ChangeDetectorRef, Component, OnInit, ViewChild } from "@angular/core";
import { FormArray, FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MatButton } from "@angular/material/button";
import { MAT_DATE_LOCALE } from "@angular/material/core";
import { MatDialog } from "@angular/material/dialog";
import { MatStepper } from "@angular/material/stepper";
import { ActivatedRoute, Router } from "@angular/router";
import {
  Inventory_Type_Id_Finished_Goods,
  Inventory_Type_Id_Raw_Materials,
} from "app/shared/consts/const";
import { ProductionStatus } from "app/shared/enums/productionStatus";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { inFinancialYearValidator } from "app/shared/validators/in-financial-year-validator";
import { Machine } from "app/views/configuration/models/machine/machine.model";
import { ProductRequest } from "app/views/configuration/models/product/product-request.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { Shift } from "app/views/configuration/models/shift/shift.model";
import { StoreRequest } from "app/views/configuration/models/store/store-request.model";
import { Store } from "app/views/configuration/models/store/store.model";
import { MachineService } from "app/views/configuration/services/machine.service";
import { ProductService } from "app/views/configuration/services/product.service";
import { ShiftService } from "app/views/configuration/services/shift.service";
import { StoreService } from "app/views/configuration/services/store.service";
import {
  CustomNgxDatetimeAdapter,
  MAT_MOMENT_DATE_ADAPTER_OPTIONS,
} from "app/views/production/helpers/CustomNgxDatetimeAdapter";
import { ManufacturingOrderResponseDetail } from "app/views/production/models/manufacturing-order/manufacturing-order-response-dto.model";
import {
  ProductionRequestDTO,
  ProductionRequestDetail,
} from "app/views/production/models/production/production-request-dto.model";
import {
  ProductionResponseDTO,
  ProductionResponseDetail,
} from "app/views/production/models/production/production-response-dto.model";
import { ProductionService } from "app/views/production/services/production.service";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { finalize } from "rxjs";
import { ManufacturingOrderListComponent } from "../manufacturing-order-list/manufacturing-order-list.component";
const CUSTOM_DATE_FORMATS: NgxMatDateFormats = {
  parse: {
    dateInput: "l, LTS",
  },
  display: {
    dateInput: "DD/MM/yyyy hh:mm A",
    monthYearLabel: "MMM YYYY",
    dateA11yLabel: "LL",
    monthYearA11yLabel: "MMMM YYYY",
  },
};
@Component({
  selector: "app-production-form",
  templateUrl: "./production-form.component.html",
  styleUrls: ["./production-form.component.scss"],
  providers: [
    {
      provide: NgxMatDateAdapter,
      useClass: CustomNgxDatetimeAdapter,
      deps: [MAT_DATE_LOCALE, MAT_MOMENT_DATE_ADAPTER_OPTIONS],
    },
    { provide: NGX_MAT_DATE_FORMATS, useValue: CUSTOM_DATE_FORMATS },
  ],
})
export class ProductionFormComponent implements OnInit {
  @ViewChild("btnCheck") btnCheck: MatButton;
  @ViewChild("btnApprove") btnApprove: MatButton;
  @ViewChild("stepper") private stepper: MatStepper;
  isLoading: boolean = false;
  isDialogOpen: boolean = false;
  isViewMode: boolean = true;
  isChecked: boolean;
  formTitle: string;
  productionForm: FormGroup;
  shifts: Shift[];
  machines: Machine[];
  fgstores: Store[];
  finishedProducts: ProductView[];
  rawMaterials: ProductView[];
  productionDetailsData: any[] = [];
  data: ProductionResponseDTO;
  // Define financial year date range here
  financialYearStartDate: Date; // Jul 1, 2023
  financialYearEndDate: Date; // Jun 30, 2024
  currentFinancialYearId: string;
  businessType: string;
  applyMinMax: boolean = false;

  private path = {
    list: "production/production",
    edit: "production/production",
  };

  constructor(
    private fb: FormBuilder,
    private dateFormatService: DateTimeFormatService,
    private jwtAuth: JwtAuthService,
    private productService: ProductService,
    private storeService: StoreService,
    private ProductionService: ProductionService,
    private shiftService: ShiftService,
    private machineService: MachineService,
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
      this.data = response?.production?.data;
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
    this.getAllShifts();
    this.getAllMachines();
    this.getAllFinishedProducts("");
    this.getAllRawMaterials("");
    this.getAllFinishedGoodsStores();
  }

  initializeForm() {
    this.createForm();
    this.populateForm();
    this.setFormTitle();
    this.productionForm.get("productionDate").markAsTouched();
  }

  createForm(): void {
    this.productionForm = this.fb.group({
      id: [this.data?.id || null],
      productionNo: [this.data?.productionNo || ""],
      manufacturingOrderNo: [this.data?.manufacturingOrderNo || ""],
      bomNo: [this.data?.bomNo || "", Validators.required],
      extraDamageQuantity: [
        this.data?.extraDamageQuantity || 0,
        Validators.required,
      ],
      dustLooseInQuantity: [this.data?.dustLooseInQuantity || 0],
      dustLooseOutQuantity: [this.data?.dustLooseOutQuantity || 0],
      fgstoreId: [this.data?.fgstoreId || null, Validators.required],
      totalRmused: [this.data?.totalRmused || 0, Validators.required],
      rmCost: [this.data?.rmCost || 0, Validators.required],
      totalAdjustmentQuantity: [
        this.data?.totalAdjustmentQuantity || 0,
        Validators.required,
      ],
      productionDate: [
        this.data?.productionDate || this.dateFormatService.getPresentDate(),
      ],
      formulationNo: [this.data?.formulationNo || "", Validators.required],
      batchNo: [this.data?.batchNo || "", Validators.required],
      finishedProductId: [
        this.data?.finishedProductId || "",
        Validators.required,
      ],
      productionQuantity: [
        this.data?.productionQuantity || 0,
        Validators.required,
      ],
      actualProductionQuantity: [
        this.data?.actualProductionQuantity || 0,
        Validators.required,
      ],
      totalCost: [this.data?.totalCost || 0, Validators.required],
      shiftId: [this.data?.shiftId, Validators.required],
      machineId: [this.data?.machineId, Validators.required],
      startDateTime: [this.data?.startDateTime, Validators.required],
      endDateTime: [this.data?.endDateTime, Validators.required],
      breakTime: [this.data?.breakTime, Validators.required],
      remark: [this.data?.remark || ""],
      deletedProductionDetailIds: [""],
      productionDetails: this.fb.array([]),
    });
    this.setMinMaxDates();
  }

  setMinMaxDates() {
    if (this.applyMinMax) {
      this.productionForm
        .get("productionDate")
        .setValidators([
          Validators.required,
          inFinancialYearValidator(
            this.financialYearStartDate,
            this.financialYearEndDate
          ),
        ]);
    } else {
      this.productionForm
        .get("productionDate")
        .setValidators([Validators.required]);
    }

    this.productionForm.get("productionDate").updateValueAndValidity();
  }

  get productionDetails(): FormArray {
    return this.productionForm.get("productionDetails") as FormArray;
  }

  populateForm(): void {
    if (this.productionForm.get("id").value) {
      this.populateProductionDetails(this.data);
    } else {
      this.addItem();
    }
  }

  setFormTitle(): void {
    if (this.productionForm.get("id").value) {
      this.formTitle = "Edit Production";
    } else {
      this.formTitle = "Add Production";
    }
  }

  populateProductionDetails(data: ProductionResponseDTO): void {
    data.productionDetails.forEach((item: ProductionResponseDetail) =>
      this.addItem(item)
    );
  }

  addItem(item?: ProductionResponseDetail): void {
    this.productionDetails.push(this.createProductionDetail(item));
  }

  createProductionDetail(item?: ProductionRequestDetail): FormGroup {
    return this.fb.group({
      id: [item?.id || null],
      rawMaterialId: [item?.rawMaterialId || "", Validators.required],
      measurementUnitName: [item?.rawMaterial?.measurementUnit?.name || ""],
      rawMaterial: [item?.rawMaterial || ""],
      quantity: [item?.quantity || 0, Validators.required],
      adjustmentQuantity: [item?.adjustmentQuantity || 0, Validators.required],
      actualUsedQuantity: [item?.actualUsedQuantity || 0, Validators.required],
      amount: [item?.amount || 0, Validators.required],
    });
  }

  isPassingDataToNextStep = false;
  handleStepChange(event: any) {
    this.isPassingDataToNextStep = event.selectedIndex === 1;
    if (this.isPassingDataToNextStep) {
      this.productionDetailsData =
        this.productionForm.get("productionDetails").value;
    }
  }

  getAllFinishedGoodsStores(): void {
    let storeRequest = new StoreRequest();
    storeRequest.page = -1;
    storeRequest.inventoryTypeId = Inventory_Type_Id_Finished_Goods;
    this.storeService.getStores(storeRequest).subscribe((res) => {
      this.fgstores = res?.data?.item1;
    });
  }

  getAllShifts(): void {
    this.shiftService.getAllShifts().subscribe((res) => {
      this.shifts = res?.data?.item1;
    });
  }

  getAllMachines(): void {
    this.machineService.getAllMachines().subscribe((res) => {
      this.machines = res?.data?.item1;
    });
  }

  getShiftName(shiftId: string) {
    return this.shifts?.find((x) => x.id === shiftId).name;
  }

  getMachineName(machineId: string) {
    return this.machines?.find((x) => x.id === machineId).name;
  }

  getStoreName(fgstoreId) {
    return this.fgstores?.find((x) => x?.id === fgstoreId)?.name;
  }

  getProductionStatus(value) {
    return this.statusColorService.getProductionStatus(value);
  }

  getProductionStatusName(value: number) {
    return ProductionStatus[value];
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
    const product = this.finishedProducts?.find(
      (product) => product?.id === productId
    );
    if (!product) return "";
    return `${product.name}${
      this.businessType === "1" ? ` (${product?.packSize?.name})` : ""
    }`;
  }

  getAllRawMaterials(keyword: string): void {
    let productRequest = new ProductRequest();
    productRequest.inventoryTypeId = Inventory_Type_Id_Raw_Materials;
    productRequest.keyword = keyword;
    productRequest.page = -1;
    this.productService.getProducts(productRequest).subscribe((res) => {
      this.rawMaterials = res?.data?.item1;
    });
  }

  getRawMaterialProductName(productId: string) {
    const product =
      this.rawMaterials?.find((product) => product?.id === productId) ||
      this.data?.productionDetails?.find((x) => x?.rawMaterialId == productId)
        ?.rawMaterial;
    return product?.name;
  }

  // findProductById(id: string) {
  //   return this.rawMaterials?.find((product) => product?.id === id);
  // }

  onControlChange(event: any): void {
    const name = event?.target?.name;
    if (name === "actualProductionQuantity") {
      const actualProductionQuantity = this.productionForm.get(
        "actualProductionQuantity"
      )?.value;
      const productionQuantity =
        this.productionForm.get("productionQuantity")?.value;
      this.productionForm
        .get("extraDamageQuantity")
        .setValue(actualProductionQuantity - productionQuantity);
    }
  }

  onAdjustmentControlChange(event: any, itemIndex?: number): void {
    const name = event?.target?.name;
    if (name === "adjustmentQuantity") {
      this.calculateAmount(itemIndex);
      this.calculateAdjustmentTotalQty();
    }
  }

  calculateAdjustmentTotalQty() {
    const sumQty = this.productionDetails?.value?.reduce(
      (sum, item) => sum + item?.adjustmentQuantity,
      0
    );
    this.productionForm.get("totalAdjustmentQuantity").setValue(sumQty);
  }

  calculateAmount(itemIndex: number) {
    const item = this.productionDetails?.at(itemIndex);
    const quantity = item?.get("quantity")?.value;
    const adjustmentQuantity = item?.get("adjustmentQuantity")?.value;
    const actualUsedQuantity = item?.get("actualUsedQuantity");
    actualUsedQuantity?.setValue(quantity + adjustmentQuantity);
  }

  private handleSuccessfulSave(res: GeneralResponse<string>): void {
    if (res?.succeeded) {
      this.handleProductionResponse(res?.data);
      this.toastr.success(res?.message);
      this.deletedIds = "";
    } else {
      this.toastr.error(res?.message);
      this.isLoading = false;
    }
  }

  private handleProductionResponse(productionId: string): void {
    this.ProductionService.getProductionById(productionId).subscribe({
      next: (productionResponse) => {
        this.data = productionResponse.data;
        this.isViewMode = true;
        this.isLoading = false;
        this.navigateToView(productionResponse?.data?.id);
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

  private addProduction(body: ProductionRequestDTO): void {
    this.ProductionService.createProduction(body).subscribe((res) =>
      this.handleSuccessfulSave(res)
    );
  }

  private updateProduction(body: ProductionRequestDTO): void {
    this.ProductionService.updateProduction(body).subscribe((res) =>
      this.handleSuccessfulSave(res)
    );
  }

  deletedIds: string = "";

  onSubmit(): void {
    if (this.productionForm.valid) {
      this.isLoading = true;
      const formValue = this.productionForm.value;
      if (!formValue.id) {
        this.addProduction(formValue);
      } else {
        formValue.deletedProductionDetailIds = this.deletedIds;
        this.updateProduction(formValue);
      }
    }
  }

  //openManufacturingOrderDialog
  openManufacturingOrderDialog() {
    const dialogRef = this.dialog.open(ManufacturingOrderListComponent, {
      disableClose: true,
      panelClass: "add-bill-container",
      minHeight: "auto",
      height: "auto",
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.productionForm.reset();
        this.productionForm.setControl("productionDetails", this.fb.array([]));

        this.productionForm.markAllAsTouched();

        this.productionForm.patchValue({
          productionDate: this.dateFormatService.getPresentDate(),
          manufacturingOrderNo: result?.manufacturingOrderNo,
          bomNo: result?.bomNo,
          formulationNo: result?.formulationNo,
          finishedProductId: result?.finishedProductId,
          productionQuantity: result?.productionQuantity,
          totalRmused: result?.totalRmused,
          rmCost: result?.rmCost,
          totalAdjustmentQuantity: 0,
          totalCost: result?.totalCost,
        });

        result.manufacturingOrderDetails.forEach(
          (item: ManufacturingOrderResponseDetail) => {
            let responseDetail: any = {
              rawMaterialId: item?.rawMaterialId,
              rawMaterial: item?.rawMaterial,
              quantity: item?.quantity,
              adjustmentQuantity: 0,
              actualUsedQuantity: item?.quantity,
              amount: item?.amount,
            };
            this.addItem(responseDetail);
          }
        );
      }
    });
  }

  check(id: string) {
    if (this.isLoading) return;
    this.isLoading = true;

    this.ProductionService.checkProduction(id)
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

    this.ProductionService.approveProduction(id)
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

    this.ProductionService.unpostProduction(id, status)
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

  getNetDuration(): string {
    const totalSeconds = this.getDurationInSeconds();
    const breakMinutes =
      Number(this.productionForm.get("breakTime")?.value) || 0;
    const breakSeconds = breakMinutes * 60;

    let finalSeconds = totalSeconds - breakSeconds;
    if (finalSeconds < 0) finalSeconds = 0;

    return this.formatSeconds(finalSeconds);
  }

  getTotalDuration(): string {
    const totalSeconds = this.getDurationInSeconds();
    return this.formatSeconds(totalSeconds);
  }

  getProductionPerHour(): string {
    const totalSeconds = this.getDurationInSeconds();
    const breakMinutes =
      Number(this.productionForm.get("breakTime")?.value) || 0;
    const breakSeconds = breakMinutes * 60;

    let finalSeconds = totalSeconds - breakSeconds;
    if (finalSeconds < 0) finalSeconds = 0;

    return this.formatProductions(finalSeconds);
  }

  private getDurationInSeconds(): number {
    const start = new Date(this.productionForm.get("startDateTime")?.value);
    const end = new Date(this.productionForm.get("endDateTime")?.value);

    if (!start || !end) return 0;

    const diffMs = end.getTime() - start.getTime();
    if (diffMs < 0) return 0;

    return Math.floor(diffMs / 1000);
  }

  private formatSeconds(totalSeconds: number): string {
    const hours = Math.floor(totalSeconds / 3600);
    const minutes = Math.floor((totalSeconds % 3600) / 60);
    return `${this.pad(hours)}:${this.pad(minutes)} Hour`;
  }

  private formatProductions(totalSeconds: number): string {
    const actualProductionQuantity = this.productionForm.get(
      "actualProductionQuantity"
    )?.value;
    const minutes = Math.floor(totalSeconds / 60);
    const productionPerHour = (
      (actualProductionQuantity * 60) /
      minutes
    ).toFixed(2);
    return `${productionPerHour}`;
  }

  private pad(n: number): string {
    return n.toString().padStart(2, "0");
  }

  getMeasurementUnitName(productId: string) {
    const product = this.finishedProducts?.find(
      (product) => product?.id === productId
    );
    if (!product) return "";
    return `${product?.measurementUnit?.name}`;
  }

  printPdf(id) {
    this.http
      .get(
        environment.apiURL +
          "/ReportProduction/production-bill-details/" +
          id +
          "/1",
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
  printProductionSummaryPdf(id) {
    this.http
      .get(
        environment.apiURL + "/ReportProduction/production-bill-summary/" + id,
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
