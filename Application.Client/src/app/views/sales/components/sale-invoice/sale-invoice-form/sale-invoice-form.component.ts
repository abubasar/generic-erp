import { HttpClient } from "@angular/common/http";
import { ChangeDetectorRef, Component, OnInit, ViewChild } from "@angular/core";
import { FormArray, FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MatButton } from "@angular/material/button";
import { MatDialog } from "@angular/material/dialog";
import { MatStepper } from "@angular/material/stepper";
import { ActivatedRoute, Router } from "@angular/router";
import { PaymentTerm } from "app/shared/enums/paymentTerm";
import { SaleInvoiceStatus } from "app/shared/enums/saleInvoiceStatus";
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
import { inFinancialYearValidator } from "app/shared/validators/in-financial-year-validator";
import { Customer } from "app/views/configuration/models/customer/customer.model";
import { ProductRequest } from "app/views/configuration/models/product/product-request.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { CustomerService } from "app/views/configuration/services/customer.service";
import { ProductService } from "app/views/configuration/services/product.service";
import { AddUpdateResponseDTO } from "app/views/purchase/models/add-update-response-dto.model";
import { DeliveryNoteResponseDetail } from "app/views/sales/models/delivery-note/delivery-note-response-dto.model";
import {
  SaleInvoiceResponseDetail,
  SaleInvoiceResponseDTO,
} from "app/views/sales/models/sale-invoice/sale-invoice-response-dto.model";
import {
  SaleOrderResponseDetail,
  SaleOrderResponseDTO,
} from "app/views/sales/models/sale-order/sale-order-response-dto.model";
import { SaleInvoiceService } from "app/views/sales/services/sale-invoice.service";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { finalize } from "rxjs";
import { SalesOrderListComponent } from "../../delivery-note/sales-order-list/sales-order-list.component";
import { DeliveryNoteListComponent } from "../delivery-note-list/delivery-note-list.component";

@Component({
  selector: "app-sale-invoice-form",
  templateUrl: "./sale-invoice-form.component.html",
  styleUrls: ["./sale-invoice-form.component.scss"],
})
export class SaleInvoiceFormComponent implements OnInit {
  @ViewChild("btnCheck") btnCheck: MatButton;
  @ViewChild("btnApprove") btnApprove: MatButton;
  @ViewChild("stepper") private stepper: MatStepper;
  isLoading: boolean = false;
  isDialogOpen: boolean = false;
  isViewMode: boolean = true;
  isChecked: boolean;
  formTitle: string;
  saleInvoiceForm: FormGroup;
  customers: Customer[];
  products: ProductView[];
  searchProducts: ProductView[];
  saleInvoiceDetailsData: any[] = [];
  cost = 0;
  paymentTerms: ENUM[];
  data: SaleInvoiceResponseDTO;
  transactionalJournalAccounts: any[];

  businessType: string;
  // Define financial year date range here
  financialYearStartDate: Date; //Example: Jul 1, 2023
  financialYearEndDate: Date; //Example: Jun 30, 2024
  currentFinancialYearId: string;
  applyMinMax: boolean = false;

  private path = {
    list: "sales/sale-invoice",
  };

  constructor(
    public dialog: MatDialog,
    private saleInvoiceService: SaleInvoiceService,
    private customerService: CustomerService,
    private productService: ProductService,
    private enumValueService: EnumValueService,
    private jwtAuth: JwtAuthService,
    private dateFormatService: DateTimeFormatService,
    private fb: FormBuilder,
    private toastr: ToastrService,
    private http: HttpClient,
    private activatedRoute: ActivatedRoute,
    private router: Router,
    private cdRef: ChangeDetectorRef,
    public statusColorService: StatusColorService,
    private snackBarService: SnackBarService,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.activatedRoute.data.subscribe((response: any) => {
      this.data = response?.saleInvoice?.data;
    });
    this.jwtAuth.userProfile.subscribe((res: UserProfile) => {
      this.financialYearStartDate = new Date(res.fystartdate);
      this.financialYearEndDate = new Date(res.fyenddate);
      this.currentFinancialYearId = res.fyid;
      this.businessType = res.businesstype;
    });
    this.getData();
    this.initializeForm();
  }

  ngAfterViewInit() {
    let id = this.activatedRoute.snapshot.paramMap.get("id");
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
    this.setMinMaxDates();
  }

  getData() {
    this.getAllCustomers();
    this.getPaymentTerms();
    //! getAllProducts() it's important after close openGRNDialog
    this.getAllProducts();
  }

  initializeForm() {
    this.createForm();
    this.populateForm();
    this.setFormTitle();
    this.saleInvoiceForm.get("invoiceDate").markAsTouched();
  }

  createForm() {
    this.saleInvoiceForm = this.fb.group({
      id: [this.data?.id ?? null],
      invoiceDate: [
        this.data?.invoiceDate || this.dateFormatService.getPresentDate(),
      ],
      deliveryNoteNo: [this.data?.deliveryNoteNo ?? ""],
      customerId: [this.data?.customerId, Validators.required],
      customerMarketingOfficerId: [
        this.data?.customerMarketingOfficerId ?? null,
      ],
      customerTerritoryId: [this.data?.customerTerritoryId ?? null],
      saleOrderNo: [this.data?.saleOrderNo ?? ""],
      orderDate: [this.data?.orderDate ?? null],
      referenceNo: [this.data?.referenceNo ?? ""],
      transport: [this.data?.transport, Validators.required],
      paymentTerm: [this.data?.paymentTerm, Validators.required],
      storeId: [this.data?.storeId, Validators.required],
      moneyReceiptNo: [this.data?.moneyReceiptNo ?? ""],
      bankDetails: [this.data?.bankDetails ?? ""],
      termAndCondition: [this.data?.termAndCondition ?? ""],
      subtotal: [this.data?.subtotal ?? 0, Validators.required],
      totalPercentageDiscountAmount: [
        this.data?.totalPercentageDiscountAmount ?? 0,
      ],
      discount: [this.data?.discount ?? 0],
      offerDiscount: [this.data?.offerDiscount ?? 0],
      otherDiscount: [this.data?.otherDiscount ?? 0],
      total: [this.data?.total ?? 0, Validators.required],
      transportationCost: [this.data?.transportationCost ?? 0],
      depoCharge: [this.data?.depoCharge ?? 0],
      netTotal: [this.data?.netTotal ?? 0],
      remark: [this.data?.remark ?? ""],
      deletedSaleInvoiceDetailIds: [""],
      saleInvoiceDetails: this.fb.array([]),
    });
    this.setMinMaxDates();
  }

  setMinMaxDates() {
    if (this.applyMinMax) {
      this.saleInvoiceForm
        .get("invoiceDate")
        .setValidators([
          Validators.required,
          inFinancialYearValidator(
            this.financialYearStartDate,
            this.financialYearEndDate
          ),
        ]);
    } else {
      this.saleInvoiceForm
        .get("invoiceDate")
        .setValidators([Validators.required]);
    }
    this.saleInvoiceForm.get("invoiceDate").updateValueAndValidity();
  }

  get saleInvoiceDetails(): FormArray {
    return this.saleInvoiceForm.get("saleInvoiceDetails") as FormArray;
  }

  populateForm(): void {
    if (this.saleInvoiceForm.get("id").value) {
      this.populateSaleInvoiceDetails(this.data);
    } else {
      this.addItem();
    }
  }

  setFormTitle(): void {
    if (this.saleInvoiceForm.get("id").value) {
      this.formTitle = "Edit Sale Invoice";
    } else {
      this.formTitle = "Add Sale Invoice";
    }
  }

  populateSaleInvoiceDetails(data: SaleInvoiceResponseDTO) {
    data.saleInvoiceDetails.forEach((item: SaleInvoiceResponseDetail) =>
      this.addItem(item)
    );
  }

  addItem(item?: SaleInvoiceResponseDetail): void {
    this.saleInvoiceDetails.push(this.createSaleInvoiceDetail(item));
  }

  createSaleInvoiceDetail(item?: SaleInvoiceResponseDetail): FormGroup {
    return this.fb.group({
      id: [item?.id ?? null],
      saleOrderDetailId: [item?.saleOrderDetailId || null],
      productId: [item?.productId ?? null, Validators.required],
      product: [item?.product ?? null],
      measurementUnitName: [item?.product.measurementUnit.name ?? ""],
      primaryQuantity: [item?.primaryQuantity ?? 0, Validators.required],
      quantity: [item?.quantity ?? 0, Validators.required],
      primaryBonusQuantity: [item?.primaryBonusQuantity ?? 0],
      bonusQuantity: [item?.bonusQuantity ?? 0],
      rate: [item?.rate ?? 0, Validators.required],
      vatPercentage: [item?.vatPercentage ?? 0],
      discountPercentage: [item?.discountPercentage ?? 0],
      percentageDiscountAmount: [item?.percentageDiscountAmount ?? 0],
      netRate: [item?.netRate ?? 0],
      discountPerUnit: [item?.discountPerUnit ?? 0],
      offerDiscountPerUnit: [item?.offerDiscountPerUnit ?? 0],
      discountAmount: [item?.discountAmount ?? 0],
      invoiceDiscountPerUnit: [item?.invoiceDiscountPerUnit ?? 0],
      cashDiscountPerUnit: [item?.cashDiscountPerUnit ?? 0],
      specialDiscountPerUnit: [item?.specialDiscountPerUnit ?? 0],
      amount: [item?.amount ?? 0],
      deliveryNoteNo: [item?.deliveryNoteNo ?? ""],
      deliveryDate: [item?.deliveryDate ?? null],
      deliveryPlace: [item?.deliveryPlace ?? ""],
      otherDiscountPerUnit: [item?.otherDiscountPerUnit ?? 0],
      transportationCostPerUnit: [item?.transportationCostPerUnit ?? 0],
      depoChargePerUnit: [item?.depoChargePerUnit ?? 0],
    });
  }

  getPaymentTerms() {
    return this.enumValueService.getPaymentTerms().subscribe((res) => {
      this.paymentTerms = res;
    });
  }

  getPaymentTermName(value: number) {
    return PaymentTerm[value];
  }

  getAllCustomers(): void {
    this.customerService.getAllCustomers().subscribe((res) => {
      this.customers = res?.data?.item1;
    });
  }

  getCustomerName(customerId) {
    return this.customers?.find((x) => x?.id === customerId)?.name;
  }

  isPassingDataToNextStep = false;
  handleStepChange(event: any) {
    this.isPassingDataToNextStep = event.selectedIndex === 1;
    if (this.isPassingDataToNextStep) {
      this.saleInvoiceDetailsData =
        this.saleInvoiceForm.get("saleInvoiceDetails").value;
      this.setTransactionalJournalAccounts();
    }
  }

  setTransactionalJournalAccounts() {
    if (this.saleInvoiceForm) {
      this.transactionalJournalAccounts = [
        {
          accountName: "Sale Account",
          column: "credit",
          value: this.saleInvoiceForm?.get("total")?.value,
        },
        {
          accountName: "Carrying Charge Account",
          column: "credit",
          value: this.saleInvoiceForm?.get("transportationCost")?.value,
        },
        {
          accountName: "Depo Charge Account",
          column: "credit",
          value: this.saleInvoiceForm?.get("depoCharge")?.value,
        },
        {
          accountName: "Customer Account",
          column: "debit",
          value: this.saleInvoiceForm?.get("netTotal")?.value,
        },
        {
          accountName: "",
          row: "total",
          value: this.saleInvoiceForm?.get("netTotal")?.value,
        },
      ];
    }
  }

  onProductChange(event: any, itemIndex: number): void {
    console.log(event.target?.name);
    const name = event.target.name;
    if (name === "productId") {
      const term = this.saleInvoiceDetails.at(itemIndex).get("productId");
      this.filterProduct(term.value || "");
    }
  }

  filterProduct(searchTerm: string) {
    searchTerm = searchTerm.toLowerCase();
    this.searchProducts = this.products?.filter((product) =>
      product.name.toLowerCase().startsWith(searchTerm)
    );
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

  findProductById(id) {
    return this.products.find((product) => product.id === id);
  }

  /**
   * !handleProductSelection is not updated
   * TODO: Need modification according to sales order
   * @param event
   * @param index
   */
  handleProductSelection(event, index) {
    // const particularDetail = this.saleInvoiceDetails.at(index);
    // const productId = event?.option?.value;
    // if (this.isExistSelectedProduct(productId, index)) {
    //   this.showSnackBar("This Product already added!");
    //   particularDetail.reset();
    //   return;
    // }
    // const selectedProduct = this.findProductById(productId);
    // if (!selectedProduct) return;
    // particularDetail.patchValue({
    //   productId: selectedProduct?.id,
    //   product: selectedProduct,
    //   measurementUnitName: selectedProduct?.measurementUnit?.name,
    // });
    // this.calculateAmount(index);
  }

  showSnackBar(message: string) {
    this.snackBarService.openSnackbar(message);
  }

  // isExistSelectedProduct(productId: string, index: number): boolean {
  //   const isProductAdded = this.saleInvoiceDetails.value.some((item, i) => {
  //     return item.productId === productId && i !== index;
  //   });
  //   return isProductAdded;
  // }

  getProductName(productId: string) {
    if (!productId) {
      return;
    }
    const product =
      this.products?.find((product) => product?.id === productId) ||
      this.data?.saleInvoiceDetails?.find((x) => x?.productId == productId)
        ?.product;
    if (!product) return "";
    return (
      product?.name +
      (this.businessType === "2"
        ? ` (${product.bagWeight} ${product?.measurementUnit?.name})`
        : ` (${product?.packSize?.name})`)
    );
  }

  /**
   * !onControlChange method is being called when the business type is primary.
   * @param event
   * @param itemIndex
   */

  onControlChange(event: any, itemIndex?: number): void {
    const name = event?.target?.name;
    if (name === "primaryQuantity" || name === "discountPercentage") {
      this.calculateAmount(itemIndex);
    }
    if (name === "primaryBonusQuantity") {
      this.calculateBonusQty(itemIndex);
    }
  }

  calculateAmount(itemIndex: number) {
    const item = this.saleInvoiceDetails?.at(itemIndex);
    const primaryQuantity = item?.get("primaryQuantity")?.value;
    const quantity = item?.get("quantity");
    const discountPerUnit = item?.get("discountPerUnit")?.value;
    const discountAmount = item?.get("discountAmount");
    const vatPercentage = item?.get("vatPercentage")?.value;
    const discountPercentage = item?.get("discountPercentage")?.value;
    const percentageDiscountAmount = item?.get("percentageDiscountAmount");
    quantity?.setValue(primaryQuantity);
    const rate = item?.get("rate")?.value;
    const amount = item?.get("amount");
    discountAmount.setValue(quantity.value * discountPerUnit);
    amount?.setValue(quantity.value * rate);
    percentageDiscountAmount.setValue(
      parseFloat(
        ((amount.value * discountPercentage) / (100 + vatPercentage)).toFixed(2)
      )
    );
    this.saleInvoiceForm.get("discount").setValue(0);
    this.saleInvoiceForm.get("totalPercentageDiscountAmount").setValue(0);
    this.saleInvoiceForm.get("offerDiscount").setValue(0);
    /**
     * ! If We go for view or edit mode, other discount is being 0. As a result, the value of netTotal
     * ! (Total Payable) is being increased.
     * TODO: carefully otherDiscount 0 korte hobe.
     */
    //this.saleOrderForm.get("otherDiscount").setValue(0);
    this.calculateCost();
  }

  calculateCost() {
    const calculateTotal = (property) => {
      return this.saleInvoiceDetails?.value?.reduce(
        (sum, item) => sum + item?.quantity * item?.[property],
        0
      );
    };

    const totalPercentageDiscountAmount =
      this.saleInvoiceDetails?.value?.reduce(
        (sum, item) => sum + item?.percentageDiscountAmount,
        0
      );

    const subtotal = calculateTotal("rate");
    const discount = calculateTotal("discountPerUnit");
    const offerDiscount = calculateTotal("offerDiscountPerUnit");
    const depoCharge = calculateTotal("depoChargePerUnit");

    this.saleInvoiceForm?.get("subtotal")?.setValue(subtotal);
    this.saleInvoiceForm
      ?.get("totalPercentageDiscountAmount")
      ?.setValue(totalPercentageDiscountAmount);
    this.saleInvoiceForm?.get("discount")?.setValue(discount);
    this.saleInvoiceForm?.get("offerDiscount")?.setValue(offerDiscount);
    const otherDiscount = this.saleInvoiceForm?.get("otherDiscount")?.value;
    const transportationCost =
      this.saleInvoiceForm?.get("transportationCost")?.value;

    this.saleInvoiceForm
      ?.get("total")
      ?.setValue(
        subtotal -
          (totalPercentageDiscountAmount +
            discount +
            offerDiscount +
            otherDiscount)
      );
    this.saleInvoiceForm?.get("depoCharge")?.setValue(depoCharge);

    this.saleInvoiceForm
      ?.get("netTotal")
      ?.setValue(
        subtotal -
          (totalPercentageDiscountAmount +
            discount +
            otherDiscount +
            offerDiscount) +
          transportationCost +
          depoCharge
      );
  }

  calculateBonusQty(itemIndex: number) {
    const item = this.saleInvoiceDetails?.at(itemIndex);
    const primaryBonusQuantity = item?.get("primaryBonusQuantity")?.value;
    const bonusQuantity = item?.get("bonusQuantity");
    bonusQuantity?.setValue(primaryBonusQuantity);
  }

  deletedIds: string = "";
  onDeleteItem(id: string, itemIndex: number): void {
    if (id) this.deletedIds += `${id},`;
    this.saleInvoiceDetails.removeAt(itemIndex);
    this.calculateCost();
  }

  private handleSuccessfulSave(
    res: GeneralResponse<AddUpdateResponseDTO>,
    body: any
  ): void {
    if (res?.succeeded) {
      this.data = {
        ...this.data,
        status: res.data?.status,
        invoiceNo: res.data?.code,
        id: res?.data?.id,
      };
      // this.localStorageService.setItem(res?.data?.id, {
      //   ...body,
      //   status: res.data?.status,
      //   invoiceNo: res.data?.code,
      //   id: res?.data?.id,
      // });
      this.isLoading = false;
      this.isViewMode = true;
      this.toastr.success(res?.message);
      this.cdRef.detectChanges();
      this.btnCheck.focus();
    } else {
      this.isLoading = false;
      this.toastr.error(res?.message);
    }
  }

  private navigateToList() {
    this.router.navigate([this.path.list]);
  }

  backToView() {
    this.initializeForm();
    this.stepper.next();
    this.isViewMode = true;
  }

  addSaleInvoice(body): void {
    this.saleInvoiceService
      .createSaleInvoice(body)
      .subscribe((res) => this.handleSuccessfulSave(res, body));
  }

  updateSaleInvoice(body): void {
    this.saleInvoiceService
      .updateSaleInvoice(body)
      .subscribe((res) => this.handleSuccessfulSave(res, body));
  }

  onSubmit() {
    if (this.saleInvoiceForm.valid) {
      this.isLoading = true;
      const formValue = this.saleInvoiceForm.value;
      if (!formValue.id) {
        this.addSaleInvoice(formValue);
      } else {
        formValue.deletedSaleInvoiceDetailIds = this.deletedIds;
        this.updateSaleInvoice(formValue);
      }
    }
  }

  //openDeliveryNoteDialog
  openDeliveryNoteDialog() {
    const dialogRef = this.dialog.open(DeliveryNoteListComponent, {
      disableClose: true,
      panelClass: "add-bill-container",
      minHeight: "auto",
      height: "auto",
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        let commaSeparatedDeliveryNoteNo = result
          .map((deliveryNote) => deliveryNote.deliveryNoteNo)
          .join(",");
        let commaSeparatedReferenceNo = result
          .map((deliveryNote) => deliveryNote.referenceNo)
          .join(",");
        let resultCost = result.reduce(
          (prevVal, currVal) => {
            return {
              subtotal: prevVal.subtotal + currVal.subtotal,
              discount: prevVal.discount + currVal.discount,
              offerDiscount: prevVal.offerDiscount + currVal.offerDiscount,
              otherDiscount: prevVal.otherDiscount + currVal.otherDiscount,
              total: prevVal.total + currVal.total,
              transportationCost:
                prevVal.transportationCost + currVal.transportationCost,
              depoCharge: prevVal.depoCharge + currVal.depoCharge,
              netTotal: prevVal.netTotal + currVal.netTotal,
            };
          },
          {
            subtotal: 0,
            discount: 0,
            offerDiscount: 0,
            otherDiscount: 0,
            total: 0,
            transportationCost: 0,
            depoCharge: 0,
            netTotal: 0,
          }
        );

        this.saleInvoiceForm.reset();
        this.saleInvoiceForm.setControl(
          "saleInvoiceDetails",
          this.fb.array([])
        );
        this.saleInvoiceForm.markAllAsTouched();

        this.saleInvoiceForm.patchValue({
          deliveryNoteNo: commaSeparatedDeliveryNoteNo,
          referenceNo: commaSeparatedReferenceNo,
          saleOrderNo: result[0].saleOrderNo,
          moneyReceiptNo: result[0].moneyReceiptNo,
          transport: result[0].transport,
          storeId: result[0].storeId,
          orderDate: result[0].orderDate,
          invoiceDate: this.dateFormatService.getPresentDate(),
          customerId: result[0].customerId,
          customerTerritoryId: null,
          subtotal: resultCost.subtotal,
          totalPercentageDiscountAmount: 0,
          discount: resultCost.discount,
          offerDiscount: resultCost.offerDiscount,
          otherDiscount: resultCost.otherDiscount,
          total: resultCost.total,
          transportationCost: resultCost.transportationCost,
          depoCharge: resultCost.depoCharge,
          netTotal: resultCost.netTotal,
          remark: result[0]?.remark,
          paymentTerm: 0,
        });
        result.forEach((deliveryNote) => {
          deliveryNote.deliveryNoteDetails.forEach(
            (item: DeliveryNoteResponseDetail) => {
              let saleInvoiceResponseDetail: any = {
                saleOrderDetailId: item?.saleOrderDetailId, // The previous data of saleOrderDetailId field of SaleInvoiceDetail will be null.
                productId: item?.productId,
                product: item?.product,
                primaryQuantity: item?.deliveryPrimaryQuantity,
                quantity: item?.deliveryQuantity,
                primaryBonusQuantity: item?.deliveryPrimaryBonusQuantity,
                bonusQuantity:
                  item?.deliveryPrimaryBonusQuantity * item?.bagWeight,
                rate: item?.rate,
                netRate: item?.netRate,
                discountPerUnit: item?.discountPerUnit,
                offerDiscountPerUnit: item?.offerDiscountPerUnit,
                discountAmount: item?.discountAmount,
                invoiceDiscountPerUnit: item?.invoiceDiscountPerUnit,
                cashDiscountPerUnit: item?.cashDiscountPerUnit,
                specialDiscountPerUnit: item?.specialDiscountPerUnit,
                amount: item?.amount,
                vatPercentage: 0,
                deliveryNoteNo: deliveryNote?.deliveryNoteNo,
                deliveryDate: deliveryNote?.deliveryDate,
                deliveryPlace: deliveryNote?.deliveryPlace,
                otherDiscountPerUnit: item?.otherDiscountPerUnit,
                transportationCostPerUnit: item?.transportationCostPerUnit,
                depoChargePerUnit: item?.depoChargePerUnit,
              };
              this.addItem(saleInvoiceResponseDetail);
            }
          );
        });
      }
    });
  }

  getSaleInvoiceStatus(value) {
    return this.statusColorService.getSaleInvoiceStatus(value);
  }

  getSaleInvoiceStatusName(value: number) {
    return SaleInvoiceStatus[value];
  }

  check(id: string) {
    if (this.isLoading) return;
    this.isLoading = true;

    this.saleInvoiceService
      .checkSaleInvoice(id)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (res) => {
          if (res?.succeeded) {
            this.data.status = res?.data as unknown as number;
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
    this.saleInvoiceService
      .approveSaleInvoice(id)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (res) => {
          if (res?.succeeded) {
            this.data.status = res?.data as unknown as number;
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

  send(id: string) {
    if (this.isLoading) return;
    this.isLoading = true;

    this.saleInvoiceService
      .sendSaleInvoice(id)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (res) => {
          if (res?.succeeded) {
            this.data.status = res?.data as unknown as number;
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

    this.saleInvoiceService
      .unpostSaleInvoice(id, status)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (res) => {
          if (res?.succeeded) {
            this.data.status = res?.data as unknown as number;
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

  confirmSendToCustomer(id: string, code: string = "") {
    if (this.isDialogOpen) return;
    this.isDialogOpen = true;

    const data: ConfirmDialogModel = {
      title: "Confirm Sent to Customer",
      message: `Confirm send sms to customer for item ${code}?`,
    };
    const dialogRef = this.confirmDialogService.confirmDialog(
      id,
      this.send.bind(this),
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

  // openSalesOrderDialog
  openSalesOrderDialog() {
    const dialogRef = this.dialog.open(SalesOrderListComponent, {
      disableClose: true,
      panelClass: "add-bill-container",
      minHeight: "auto",
      height: "auto",
      data: null,
    });
    dialogRef.afterClosed().subscribe((result: SaleOrderResponseDTO) => {
      if (!result) {
        return;
      }
      this.saleInvoiceForm.reset();
      this.saleInvoiceForm.setControl("saleInvoiceDetails", this.fb.array([]));
      this.saleInvoiceForm.markAllAsTouched();

      const formData = {
        deliveryNoteNo: "",
        saleOrderNo: result?.saleOrderNo,
        orderDate: result?.orderDate,
        referenceNo: result?.referenceNo,
        invoiceDate: this.dateFormatService.getPresentDate(),
        transport: result?.transport,
        paymentTerm: result?.paymentTerm,
        customerId: result?.customerId,
        customerTerritoryId: result?.customerTerritoryId,
        storeId: result?.storeId,
        moneyReceiptNo: result?.moneyReceiptNo,
        subtotal: result?.subtotal,
        totalPercentageDiscountAmount: result?.totalPercentageDiscountAmount,
        discount: result?.discount,
        offerDiscount: result?.offerDiscount,
        otherDiscount: result.otherDiscount,
        total: result?.total,
        transportationCost: result?.transportationCost,
        depoCharge: result?.depoCharge,
        netTotal: result?.netTotal,
        remark: result?.remark,
      };

      this.saleInvoiceForm.patchValue(formData);

      result?.saleOrderDetails.forEach((item: SaleOrderResponseDetail) => {
        const saleInvoiceResponseDetail: SaleInvoiceResponseDetail = {
          saleOrderDetailId: item?.id,
          productId: item?.productId,
          product: item?.product,
          primaryQuantity: item?.primaryQuantity,
          quantity: item?.quantity,
          primaryBonusQuantity: item?.primaryBonusQuantity,
          bonusQuantity: item?.bonusQuantity,
          rate: item?.rate,
          vatPercentage: item?.vatPercentage,
          discountPercentage: item?.discountPercentage,
          percentageDiscountAmount: item?.percentageDiscountAmount,
          netRate: item?.netRate,
          discountPerUnit: item?.discountPerUnit,
          offerDiscountPerUnit: item?.offerDiscountPerUnit,
          invoiceDiscountPerUnit: item?.invoiceDiscountPerUnit,
          cashDiscountPerUnit: item?.cashDiscountPerUnit,
          specialDiscountPerUnit: item?.specialDiscountPerUnit,
          discountAmount: item.quantity * item?.discountPerUnit,
          amount: item.quantity * item?.rate,
          otherDiscountPerUnit: item?.otherDiscountPerUnit,
          transportationCostPerUnit: item?.transportationCostPerUnit,
          depoChargePerUnit: item?.depoChargePerUnit,
          deliveryNoteNo: "",
          deliveryDate: null,
          deliveryPlace: "",
        };
        this.addItem(saleInvoiceResponseDetail);
      });
    });
  }
  printPdf(id) {
    this.http
      .get(environment.apiURL + "/ReportSaleInvoiceA5/" + id, {
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
      .get(environment.apiURL + "/ReportVatInvoiceA5/" + id, {
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
