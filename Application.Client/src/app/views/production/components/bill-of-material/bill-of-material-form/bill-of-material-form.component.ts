import { HttpClient } from "@angular/common/http";
import {
  AfterViewInit,
  ChangeDetectorRef,
  Component,
  OnInit,
  ViewChild,
} from "@angular/core";
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
import { BOMStatus } from "app/shared/enums/bomStatus";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { SnackBarService } from "app/shared/services/snack-bar.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { ProductRequest } from "app/views/configuration/models/product/product-request.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { ProductService } from "app/views/configuration/services/product.service";
import {
  BillOfMaterialRequestDTO,
  BillOfMaterialRequestDetail,
} from "app/views/production/models/bill-of-material/bill-of-material-request-dto.model";
import {
  BillOfMaterialResponseDTO,
  BillOfMaterialResponseDetail,
} from "app/views/production/models/bill-of-material/bill-of-material-response-dto.model";
import { BillOfMaterialService } from "app/views/production/services/bill-of-material.service";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { finalize } from "rxjs";
import { BillOfMaterialListComponent } from "../../manufacturing-order/bill-of-material-list/bill-of-material-list.component";

@Component({
  selector: "app-bill-of-material-form",
  templateUrl: "./bill-of-material-form.component.html",
  styleUrls: ["./bill-of-material-form.component.scss"],
})
export class BillOfMaterialFormComponent implements OnInit, AfterViewInit {
  @ViewChild("btnCheck") btnCheck: MatButton;
  @ViewChild("btnApprove") btnApprove: MatButton;
  @ViewChild("stepper") private stepper: MatStepper;
  isLoading: boolean = false;
  isDialogOpen: boolean = false;
  isViewMode: boolean = true;
  isChecked: boolean;
  formTitle: string;
  billOfMaterialForm: FormGroup;
  finishedProducts: ProductView[];
  rawMaterials: ProductView[];
  filterRawMaterials: ProductView[];
  billOfMaterialDetailsData: any[] = [];
  data: BillOfMaterialResponseDTO;
  currentFinancialYearId: string;
  businessType: string;

  private path = {
    list: "production/bill-of-material",
    edit: "production/bill-of-material",
  };

  constructor(
    private fb: FormBuilder,
    private jwtAuth: JwtAuthService,
    private productService: ProductService,
    private billOfMaterialService: BillOfMaterialService,
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
      this.data = response?.billOfMaterial?.data;
    });
    this.getData();
    this.getCurrentFinancialYearId();
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
    }
    this.cdRef.detectChanges();
  }

  showEdit() {
    this.stepper.previous();
    this.isViewMode = false;
    this.initializeForm();
  }

  getData() {
    this.getAllFinishedProducts("");
    this.getAllRawMaterials();
  }

  initializeForm() {
    this.createForm();
    this.populateForm();
    this.setFormTitle();
  }

  createForm(): void {
    this.billOfMaterialForm = this.fb.group({
      id: [this.data?.id || null],
      bomNo: [this.data?.bomNo || ""],
      copiedFromBomNo: [this.data?.copiedFromBomNo || ""],
      finishedProductId: [this.data?.finishedProductId, Validators.required],
      formulationNo: [this.data?.formulationNo, Validators.required],
      dosageQuantity: [this.data?.dosageQuantity || 0, Validators.required],
      totalQuantity: [this.data?.totalQuantity || 0, Validators.required],
      remark: [this.data?.remark || ""],
      deletedBillOfMaterialDetailIds: [""],
      billOfMaterialDetails: this.fb.array([]),
    });
  }

  get billOfMaterialDetails(): FormArray {
    return this.billOfMaterialForm.get("billOfMaterialDetails") as FormArray;
  }

  populateForm(): void {
    if (this.billOfMaterialForm.get("id").value) {
      this.populateBillOfMaterialDetails(this.data);
    } else {
      this.addItem();
    }
  }

  setFormTitle(): void {
    if (this.billOfMaterialForm.get("id").value) {
      this.formTitle = "Edit Bill of Material";
    } else {
      this.formTitle = "Add Bill of Material";
    }
  }

  populateBillOfMaterialDetails(data: BillOfMaterialResponseDTO): void {
    data.billOfMaterialDetails.forEach((item: BillOfMaterialResponseDetail) =>
      this.addItem(item)
    );
  }

  addItem(item?: BillOfMaterialResponseDetail): void {
    this.billOfMaterialDetails.push(this.createBillOfMaterialDetail(item));
  }

  createBillOfMaterialDetail(item?: BillOfMaterialRequestDetail): FormGroup {
    return this.fb.group({
      id: [item?.id || null],
      rawMaterialId: [item?.rawMaterialId || "", Validators.required],
      measurementUnitName: [item?.rawMaterial?.measurementUnit?.name || ""],
      rawMaterial: [item?.rawMaterial || ""],
      quantity: [item?.quantity || 0, Validators.required],
      percentage: [item?.percentage || 0],
    });
  }

  isPassingDataToNextStep = false;
  handleStepChange(event: any) {
    this.isPassingDataToNextStep = event.selectedIndex === 1;
    if (this.isPassingDataToNextStep) {
      this.billOfMaterialDetailsData = this.billOfMaterialForm.get(
        "billOfMaterialDetails"
      ).value;
    }
  }

  private getCurrentFinancialYearId() {
    this.jwtAuth.userProfile.subscribe((res: UserProfile) => {
      this.currentFinancialYearId = res.fyid;
      this.businessType = res.businesstype;
    });
  }

  getBOMStatus(value) {
    return this.statusColorService.getBOMStatus(value);
  }

  getBOMStatusName(value: number) {
    return BOMStatus[value];
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

  getAllRawMaterials(): void {
    let productRequest = new ProductRequest();
    productRequest.inventoryTypeIds = [
      Inventory_Type_Id_Raw_Materials,
      Inventory_Type_Id_Other_Items,
    ];
    productRequest.page = -1;
    this.productService.getProducts(productRequest).subscribe((res) => {
      this.filterRawMaterials = this.rawMaterials = res?.data?.item1;
    });
  }

  getRawMaterialProductName(productId: string) {
    const product =
      this.rawMaterials?.find((product) => product?.id === productId) ||
      this.data?.billOfMaterialDetails?.find(
        (x) => x?.rawMaterialId == productId
      )?.rawMaterial;
    return product?.name;
  }

  findProductById(id: string) {
    return this.rawMaterials?.find((product) => product?.id === id);
  }

  clearInputFromDetails(evt: any, itemIndex: number): void {
    evt.stopPropagation();
    const particularDetail = this.billOfMaterialDetails.at(itemIndex);
    particularDetail?.patchValue({
      rawMaterialId: null,
    });
  }

  handleProductSearch(event: any, itemIndex: number) {
    const name = event?.target?.name;
    if (name === "rawMaterialId") {
      const term = this.billOfMaterialDetails
        .at(itemIndex)
        .get("rawMaterialId");
      this.filterProduct(term.value || "");
    }
  }

  filterProduct(searchTerm: string) {
    searchTerm = searchTerm.toLowerCase();
    this.filterRawMaterials = this.rawMaterials?.filter((product) =>
      product.name.toLowerCase().startsWith(searchTerm)
    );
  }

  // private filterProduct(value: string) {
  //   this.getAllRawMaterials(value.toLowerCase());
  // }

  handleProductSelection(event, index) {
    const particularDetail = this.billOfMaterialDetails.at(index);
    const productId = event?.option?.value;

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
      rawMaterialId: selectedProduct?.id,
      rawMaterial: selectedProduct,
      measurementUnitName: selectedProduct?.measurementUnit?.name,
    });
  }

  showSnackBar(message: string) {
    this.snackBarService.openSnackbar(message);
  }

  isExistSelectedProduct(productId: string, index: number): boolean {
    const isProductAdded = this.billOfMaterialDetails.value.some((item, i) => {
      return item.rawMaterialId === productId && i !== index;
    });
    return isProductAdded;
  }

  onControlChange(event: any, itemIndex?: number): void {
    const name = event?.target?.name;
    if (name === "quantity") {
      this.calculatePercentage(itemIndex);
      this.calculateTotalQuantity();
    } else if (name === "dosageQuantity") {
      this.billOfMaterialDetails.value.map((item, index) => {
        this.calculatePercentage(index);
      });
    }
  }

  calculatePercentage(itemIndex: number) {
    const item = this.billOfMaterialDetails.at(itemIndex);
    const quantity = item.get("quantity");
    const percentage = item.get("percentage");
    const dosageQuantity = this.billOfMaterialForm.get("dosageQuantity")?.value;
    percentage?.setValue(
      ((quantity?.value * 100) / dosageQuantity).toFixed(12)
    );
  }

  calculateTotalQuantity() {
    const sumQty = this.billOfMaterialDetails?.value?.reduce(
      (sum, item) => sum + item?.quantity,
      0
    );
    this.billOfMaterialForm.get("totalQuantity").setValue(sumQty);
  }

  private handleSuccessfulSave(res: GeneralResponse<string>): void {
    if (res?.succeeded) {
      this.handleBillOfMaterialResponse(res?.data);
      this.toastr.success(res?.message);
      this.deletedIds = "";
    } else {
      this.toastr.error(res?.message);
      this.isLoading = false;
    }
  }

  private handleBillOfMaterialResponse(billOfMaterialId: string): void {
    this.billOfMaterialService
      .getBillOfMaterialById(billOfMaterialId)
      .subscribe({
        next: (billOfMaterialResponse) => {
          this.data = billOfMaterialResponse.data;
          this.isViewMode = true;
          this.isLoading = false;
          this.navigateToView(billOfMaterialResponse?.data?.id);
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

  private addBillOfMaterial(body: BillOfMaterialRequestDTO): void {
    this.billOfMaterialService
      .createBillOfMaterial(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  private updateBillOfMaterial(body: BillOfMaterialRequestDTO): void {
    this.billOfMaterialService
      .updateBillOfMaterial(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  deletedIds: string = "";
  onDeleteItem(id: string, itemIndex: number): void {
    if (id) this.deletedIds += `${id},`;
    this.billOfMaterialDetails.removeAt(itemIndex);
    this.calculateTotalQuantity();
  }

  onSubmit(): void {
    if (this.billOfMaterialForm.valid) {
      this.isLoading = true;
      const formValue = this.billOfMaterialForm.value;
      if (!formValue.id) {
        this.addBillOfMaterial(formValue);
      } else {
        formValue.deletedBillOfMaterialDetailIds = this.deletedIds;
        this.updateBillOfMaterial(formValue);
      }
    }
  }

  check(id: string) {
    if (this.isLoading) return;
    this.isLoading = true;

    this.billOfMaterialService
      .checkBillOfMaterial(id)
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

    this.billOfMaterialService
      .approveBillOfMaterial(id)
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

    this.billOfMaterialService
      .unpostBillOfMaterial(id, status)
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

  confirmCheck($event, id: string, code: string = "") {
    $event.target.blur();
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

  confirmApprove($event, id: string, code: string = "") {
    $event.target.blur();
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

  //openBOMDialog
  openBOMDialog() {
    const dialogRef = this.dialog.open(BillOfMaterialListComponent, {
      disableClose: true,
      panelClass: "add-bill-container",
      minHeight: "auto",
      height: "auto",
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.billOfMaterialForm.setControl(
          "billOfMaterialDetails",
          this.fb.array([])
        );
        this.billOfMaterialForm.markAllAsTouched();
        this.billOfMaterialForm.patchValue({
          finishedProductId: result?.finishedProductId,
          bomNo: "",
          copiedFromBomNo: result?.bomNo,
          formulationNo: result?.formulationNo,
          dosageQuantity: result?.dosageQuantity,
          remark: result?.remark,
          totalQuantity: result?.totalQuantity,
        });

        result?.billOfMaterialDetails.forEach(
          (item: BillOfMaterialResponseDetail) => {
            let billOfMaterialResponseDetail: any = {
              rawMaterialId: item?.rawMaterialId,
              rawMaterial: item?.rawMaterial,
              measurementUnitName: item?.rawMaterial?.measurementUnit?.name,
              quantity: item?.quantity,
              percentage: item?.percentage,
            };
            this.addItem(billOfMaterialResponseDetail);
          }
        );
        this.billOfMaterialForm.markAsDirty();
        this.billOfMaterialForm.updateValueAndValidity();
      }
    });
  }

  generateReport(event: Event, id: number) {
    const targetName = (event.currentTarget as HTMLButtonElement).name;
    const reportType = targetName === "pdf" ? 1 : 2;

    const endpoint =
      environment.apiURL + `/ReportBillOfMaterial/${id}/${reportType}`;

    this.http
      .get(endpoint, { responseType: "blob" })
      .subscribe((response) => this.handleFileResponse(response, targetName));
  }

  private handleFileResponse(response: Blob, targetName: string): void {
    const fileType =
      targetName === "pdf" ? "application/pdf" : "application/octet-stream";
    // Create a Blob from the PDF Stream
    const file = new Blob([response], { type: fileType });
    const fileURL = URL.createObjectURL(file);
    if (targetName === "pdf") {
      // Open the URL in a new window
      const pdfWindow = window.open();
      pdfWindow.location.href = fileURL;
    } else {
      const link = document.createElement("a");
      link.href = fileURL;
      // Generate a dynamic filename based on the report type
      link.setAttribute("download", `Bill_Of_Material.xlsx`);
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link); // Cleanup after download
    }
  }

  printBillOfMaterialPdf(event: Event, id: number) {
    this.generateReport(event, id);
  }
}
