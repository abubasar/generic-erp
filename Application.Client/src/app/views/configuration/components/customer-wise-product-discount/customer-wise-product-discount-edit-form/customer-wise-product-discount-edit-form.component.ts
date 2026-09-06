import { ChangeDetectorRef, Component, OnInit, ViewChild } from "@angular/core";
import { FormArray, FormBuilder, FormGroup, Validators } from "@angular/forms";
import {
  CustomerWiseProductDiscount,
  CustomerWiseProductDiscountDetail,
} from "app/views/configuration/models/customer-wise-product-discount/customer-wise-product-discount.model";
import { Customer } from "app/views/configuration/models/customer/customer.model";
import { ProductRequest } from "app/views/configuration/models/product/product-request.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { CustomerWiseProductDiscountService } from "app/views/configuration/services/customer-wise-product-discount.service";
import { CustomerService } from "app/views/configuration/services/customer.service";
import { ProductService } from "app/views/configuration/services/product.service";
import { ToastrService } from "ngx-toastr";

import { HttpClient } from "@angular/common/http";
import { MatButton } from "@angular/material/button";
import { MatStepper } from "@angular/material/stepper";
import { ActivatedRoute, Router } from "@angular/router";
import { Inventory_Type_Id_Finished_Goods } from "app/shared/consts/const";
import { CustomerWiseProductDiscountStatus } from "app/shared/enums/customerWiseProductDiscountStatus";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { SnackBarService } from "app/shared/services/snack-bar.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { CustomerWiseProductDiscountRequest } from "app/views/configuration/models/customer-wise-product-discount/customer-wise-product-discount-request.model";
import { environment } from "environments/environment";
import { finalize } from "rxjs";

@Component({
  selector: "app-customer-wise-product-discount-edit-form",
  templateUrl: "./customer-wise-product-discount-edit-form.component.html",
  styleUrls: ["./customer-wise-product-discount-edit-form.component.scss"],
})
export class CustomerWiseProductDiscountEditFormComponent implements OnInit {
  @ViewChild("btnCheck") btnCheck: MatButton;
  @ViewChild("btnApprove") btnApprove: MatButton;
  @ViewChild("stepper") private stepper: MatStepper;
  isLoading: boolean = false;
  isDialogOpen: boolean = false;
  isViewMode: boolean = true;
  formTitle: string;
  customerWiseProductDiscountForm: FormGroup;
  customers: Customer[];
  products: ProductView[];
  productSearchResult: ProductView[];
  customerWiseProductDiscounts: CustomerWiseProductDiscount[];
  customerWiseProductDiscountDetailsData: any[] = [];

  data: CustomerWiseProductDiscount;

  private path = {
    list: "configuration/customer-wise-product-discount",
    edit: "configuration/customer-wise-product-discount",
  };

  constructor(
    public dateFormatService: DateTimeFormatService,
    private customerWiseProductDiscountService: CustomerWiseProductDiscountService,
    private customerServices: CustomerService,
    private productServices: ProductService,
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

  ngOnInit() {
    this.activatedRoute.data.subscribe((response: any) => {
      this.data = response?.customerWiseProductDiscount?.data;
      console.log(this.data);
    });
    this.getData();
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
  }

  getData() {
    this.getAllCustomers();
    this.getAllProducts();
    this.initializeForm();
    this.getAllActiveCustomerWiseProductDiscounts();
  }

  initializeForm() {
    this.createForm();
    this.populateForm();
    this.setFormTitle();
  }

  createForm(): void {
    this.customerWiseProductDiscountForm = this.fb.group({
      id: [this.data?.id ?? null],
      customerId: [this.data?.customerId, Validators.required],
      applicableDate: [this.data?.applicableDate, Validators.required],
      isActive: [this.data?.isActive],
      customer: [this.data?.customer ?? null],
      customerWiseProductDiscountDetails: this.fb.array([]),
    });
  }

  isPassingDataToNextStep = false;
  handleStepChange(event: any) {
    this.isPassingDataToNextStep = event.selectedIndex === 1;
    if (this.isPassingDataToNextStep) {
      this.customerWiseProductDiscountDetailsData =
        this.customerWiseProductDiscountForm.get(
          "customerWiseProductDiscountDetails"
        ).value;
    }
  }

  getCustomerWiseProductDiscountStatus(value) {
    return this.statusColorService.getCustomerWiseProductDiscountStatus(value);
  }

  getCustomerWiseProductDiscountStatusName(value: number) {
    return CustomerWiseProductDiscountStatus[value];
  }

  get customerWiseProductDiscountDetails(): FormArray {
    return this.customerWiseProductDiscountForm?.get(
      "customerWiseProductDiscountDetails"
    ) as FormArray;
  }

  populateForm(): void {
    if (this.customerWiseProductDiscountForm.get("id").value) {
      this.populateCustomerWiseProductDiscountDetails(this.data);
    }
  }

  populateCustomerWiseProductDiscountDetails(
    data: CustomerWiseProductDiscount
  ): void {
    data.customerWiseProductDiscountDetails.forEach(
      (item: CustomerWiseProductDiscountDetail) => this.addItem(item)
    );
  }

  addItem(item?: CustomerWiseProductDiscountDetail): void {
    this.customerWiseProductDiscountDetails.push(
      this.createCustomerWiseProductDiscountDetail(item)
    );
    this.filterProducts();
  }

  createCustomerWiseProductDiscountDetail(
    item?: CustomerWiseProductDiscountDetail
  ): FormGroup {
    return this.fb.group({
      id: [item?.id ?? null],
      productId: [item?.productId ?? null, Validators.required],
      product: [item.product ?? null],
      salePrice: [item?.salePrice ?? 0, Validators.required],
      invoiceDiscount: [item?.invoiceDiscount ?? 0, Validators.required],
      cashDiscount: [item?.cashDiscount ?? 0, Validators.required],
      specialDiscount: [item?.specialDiscount ?? 0, Validators.required],
      monthlyDiscount: [item?.monthlyDiscount ?? 0, Validators.required],
      yearlyDiscount: [item?.yearlyDiscount ?? 0, Validators.required],
      targetDiscount: [item?.targetDiscount ?? 0, Validators.required],
    });
  }

  clearInputFromDetails(evt: any, itemIndex: number): void {
    evt.stopPropagation();
    const particularDetail =
      this.customerWiseProductDiscountDetails.at(itemIndex);
    particularDetail?.patchValue({
      productId: null,
    });
    this.filterProducts();
  }

  getAllCustomers() {
    this.customerServices.getAllCustomers().subscribe((res) => {
      this.customers = res?.data?.item1;
    });
  }

  getCustomerName(customerId: string) {
    if (!customerId) {
      return;
    }
    const customerAccount = this.customers?.find(
      (customer) => customer?.id === customerId
    );
    return customerAccount?.name;
  }

  /**------------------- Autocomplete Code ---------------------  */
  onProductChange(event: any, itemIndex: number): void {
    const name = event.target.name;
    if (name === "productId") {
      const term = this.customerWiseProductDiscountDetails
        .at(itemIndex)
        .get("productId");
      this.searchProduct(term.value || "");
    }
  }

  private searchProduct(value: string) {
    const searchValue = value?.toLowerCase();
    this.productSearchResult = this.filteredProducts?.filter((option) =>
      option.name?.toLowerCase()?.includes(searchValue)
    );
  }

  findProductById(id) {
    return this.products.find((product) => product.id === id);
  }

  handleProductSelection(event, index) {
    const particularDetail = this.customerWiseProductDiscountDetails.at(index);
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
      salePrice: selectedProduct?.salePrice,
    });
    this.filterProducts();
    this.searchProduct("");
  }

  showSnackBar(message: string) {
    this.snackBarService.openSnackbar(message);
  }

  isExistSelectedProduct(productId: string, index: number): boolean {
    const isProductAdded = this.customerWiseProductDiscountDetails.value.some(
      (item, i) => {
        return item.productId === productId && i !== index;
      }
    );
    return isProductAdded;
  }

  getProductName(productId: string) {
    if (!productId) {
      return;
    }
    const product =
      this.products?.find((product) => product?.id === productId) ||
      this.data?.customerWiseProductDiscountDetails?.find(
        (x) => x?.productId == productId
      )?.product;
    return product?.name;
  }

  filteredProducts: ProductView[];
  filterProducts() {
    let formDetailsValue = this.customerWiseProductDiscountForm?.get(
      "customerWiseProductDiscountDetails"
    )?.value;
    this.filteredProducts = this.products?.filter((product) => {
      return !formDetailsValue.find(
        (element) => element?.productId == product?.id
      );
    });
    console.log("filter Products", this.filteredProducts);
  }

  getAllProducts() {
    let productRequest = new ProductRequest();
    productRequest.inventoryTypeId = Inventory_Type_Id_Finished_Goods;
    productRequest.page = -1;
    this.productServices.getProducts(productRequest).subscribe((res) => {
      this.products = res?.data?.item1;
      console.log(this.products);
    });
  }
  /**---------------------------- End Autocomplete------------------- */

  setFormTitle(): void {
    if (this.customerWiseProductDiscountForm.get("id").value) {
      this.formTitle = "Edit Customer Wise Product Discount";
    }
  }

  getAllActiveCustomerWiseProductDiscounts(): void {
    let customerWiseProductDiscountRequest =
      new CustomerWiseProductDiscountRequest();
    customerWiseProductDiscountRequest.isActive = true;
    this.customerWiseProductDiscountService
      .getCustomerWiseProductDiscounts(customerWiseProductDiscountRequest)
      .subscribe((res) => {
        this.customerWiseProductDiscounts = res?.data?.item1;
      });
  }

  handleActiveCustomer(event: any) {
    const selectedCustomerId =
      this.customerWiseProductDiscountForm.get("customerId")?.value;
    const isActiveControl =
      this.customerWiseProductDiscountForm.get("isActive");

    const isActive = isActiveControl?.value;

    if (!isActive) {
      const isActiveCustomerExist = this.customerWiseProductDiscounts.find(
        (item) =>
          item.customerId === selectedCustomerId && item.isActive === true
      );

      if (isActiveCustomerExist) {
        setTimeout(() => {
          alert(
            "Customer already has an active discount. Please deactivate it before reactivating this one."
          );
          console.log("print Settimeout");

          // Set the value of isActive to false after showing the alert
          isActiveControl?.setValue(false);
        }, 0);

        return;
      }
    }
  }

  handleSuccessfulSave(res: any) {
    if (res?.succeeded) {
      this.isViewMode = true;
      this.navigateToView(res?.data?.id);
      this.toastr.success(res?.message);
      this.isLoading = false;
    } else {
      this.isLoading = false;
      this.toastr.error(res?.message);
    }
  }

  private navigateToView(id: string) {
    this.router.navigate([this.path.edit, id]);
    this.cdRef.detectChanges();
    this.btnCheck.focus();
  }

  private navigateToList() {
    this.router.navigate([this.path.list]);
  }

  backToView() {
    this.initializeForm();
    this.stepper.next();
    this.isViewMode = true;
  }

  updateCustomerWiseProductDiscount(body): void {
    this.customerWiseProductDiscountService
      .updateCustomerWiseProductDiscount(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  deletedIds: string = "";
  onDeleteItem(id: string, itemIndex: number): void {
    if (id) this.deletedIds += `${id},`;
    this.customerWiseProductDiscountDetails.removeAt(itemIndex);
  }

  onSubmit() {
    if (this.customerWiseProductDiscountForm.valid) {
      // this.isLoading = true;
      const formValue = this.customerWiseProductDiscountForm.value;
      this.updateCustomerWiseProductDiscount(formValue);
    }
  }

  check(id: string) {
    if (this.isLoading) return;
    this.isLoading = true;

    this.customerWiseProductDiscountService
      .checkCustomerWiseProductDiscount(id)
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

    this.customerWiseProductDiscountService
      .approveCustomerWiseProductDiscount(id)
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

    this.customerWiseProductDiscountService
      .unpostCustomerWiseProductDiscount(id, status)
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
      .get(environment.apiURL + "/ReportCustomerWiseProductDiscount/" + id, {
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
