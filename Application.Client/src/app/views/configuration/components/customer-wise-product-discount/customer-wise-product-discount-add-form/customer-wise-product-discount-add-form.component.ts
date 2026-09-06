import { Component, OnInit } from "@angular/core";
import { FormArray, FormBuilder, FormGroup, Validators } from "@angular/forms";
import { CustomerWiseProductDiscount } from "app/views/configuration/models/customer-wise-product-discount/customer-wise-product-discount.model";
import { Customer } from "app/views/configuration/models/customer/customer.model";
import { ProductRequest } from "app/views/configuration/models/product/product-request.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { CustomerWiseProductDiscountService } from "app/views/configuration/services/customer-wise-product-discount.service";
import { CustomerService } from "app/views/configuration/services/customer.service";
import { ProductService } from "app/views/configuration/services/product.service";
import { ToastrService } from "ngx-toastr";

import { Router } from "@angular/router";
import { Inventory_Type_Id_Finished_Goods } from "app/shared/consts/const";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { CustomerWiseProductDiscountRequest } from "app/views/configuration/models/customer-wise-product-discount/customer-wise-product-discount-request.model";

@Component({
  selector: "app-customer-wise-product-discount-add-form",
  templateUrl: "./customer-wise-product-discount-add-form.component.html",
  styleUrls: ["./customer-wise-product-discount-add-form.component.scss"],
})
export class CustomerWiseProductDiscountAddFormComponent implements OnInit {
  isLoading: boolean = false;
  formTitle: string;
  customerWiseProductDiscountForm: FormGroup;
  customers: Customer[];
  filterCustomers: Customer[];
  products: ProductView[];
  customerWiseProductDiscounts: CustomerWiseProductDiscount[];
  customerWiseProductDiscountDetailsData: any[] = [];

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
    private router: Router
  ) {}

  ngOnInit() {
    this.getAllActiveCustomerWiseProductDiscounts();
    this.getAllCustomers();
    this.getAllProducts();
    this.initializeForm();
  }

  initializeForm() {
    this.createForm();
    this.setFormTitle();
  }

  createForm(): void {
    this.customerWiseProductDiscountForm = this.fb.group({
      id: [null],
      customerId: [null, Validators.required],
      applicableDate: [
        this.dateFormatService.getPresentDate(),
        Validators.required,
      ],
      isActive: [true],
      customer: [null],
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

  get customerWiseProductDiscountDetails(): FormArray {
    return this.customerWiseProductDiscountForm.get(
      "customerWiseProductDiscountDetails"
    ) as FormArray;
  }

  populateForm(): void {
    this.populateCustomerWiseProductDiscountDetailsFromProduct(this.products);
  }

  populateCustomerWiseProductDiscountDetailsFromProduct(
    products: ProductView[]
  ): void {
    products?.forEach((item: ProductView) => this.addItem(item));
  }

  addItem(item?: ProductView): void {
    this.customerWiseProductDiscountDetails.push(
      this.createCustomerWiseProductDiscountDetail(item)
    );
  }

  createCustomerWiseProductDiscountDetail(item?: ProductView): FormGroup {
    return this.fb.group({
      id: [null],
      productId: [item?.id ?? null, Validators.required],
      product: [item ?? null],
      salePrice: [
        item && item?.salePrice ? item?.salePrice : 0,
        Validators.required,
      ],
      invoiceDiscount: [0, Validators.required],
      cashDiscount: [0, Validators.required],
      specialDiscount: [0, Validators.required],
      monthlyDiscount: [0, Validators.required],
      yearlyDiscount: [0, Validators.required],
      targetDiscount: [0, Validators.required],
    });
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    if (fieldName === "customerId") {
      this.customerWiseProductDiscountForm?.get("customerId").setValue(null);
    }
  }

  handleCustomerSearch(event: any): void {
    const name = event.target?.name;
    if (name === "customerId") {
      const term = this.customerWiseProductDiscountForm.get("customerId");
      this.filterCustomer(term.value || "");
    }
  }

  private filterCustomer(value: string) {
    const filterValue = value.trim().toLowerCase();
    this.filterCustomers = this.customers?.filter(
      (option) =>
        option.name?.toLowerCase().includes(filterValue) ||
        option.code?.slice(-4).toLowerCase().includes(filterValue)
    );
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

  getAllCustomers() {
    this.customerServices.getAllCustomers().subscribe((res) => {
      this.filterCustomers = this.customers = res?.data?.item1;
    });
  }

  getAllProducts() {
    let productRequest = new ProductRequest();
    productRequest.inventoryTypeId = Inventory_Type_Id_Finished_Goods;
    productRequest.page = -1;
    this.productServices.getProducts(productRequest).subscribe((res) => {
      this.products = res?.data?.item1;
      console.log(this.products);
      this.populateForm(); // it's important
    });
  }

  setFormTitle(): void {
    this.formTitle = "Add Customer Wise Product Discount";
  }

  getAllActiveCustomerWiseProductDiscounts(): void {
    let customerWiseProductDiscountRequest =
      new CustomerWiseProductDiscountRequest();
    customerWiseProductDiscountRequest.isActive = true;
    this.customerWiseProductDiscountService
      .getCustomerWiseProductDiscounts(customerWiseProductDiscountRequest)
      .subscribe((res) => {
        this.customerWiseProductDiscounts = res?.data?.item1;
        console.log(
          "customerWiseProductDiscounts List: ",
          this.customerWiseProductDiscounts
        );
      });
  }

  handleCustomerSelection(event: any) {
    const selectedCustomerId =
      this.customerWiseProductDiscountForm.get("customerId")?.value;
    console.log("selectedCustomerId : ", selectedCustomerId);

    const isActiveCustomerExist = this.customerWiseProductDiscounts.find(
      (item) => item.customerId === selectedCustomerId && item.isActive === true
    );

    if (isActiveCustomerExist) {
      this.customerWiseProductDiscountForm.get("customerId")?.setValue("");
      alert(
        "Customer already has an active discount. Please deactivate it before adding a new one."
      );
      return;
    }
  }

  handleSuccessfulSave(res: any) {
    if (res?.succeeded) {
      this.toastr.success(res?.message);
      this.isLoading = false;
      this.navigateToList();
    } else {
      this.isLoading = false;
      this.toastr.error(res?.message);
    }
  }

  private navigateToList() {
    this.router.navigate([this.path.list]);
  }

  addCustomerWiseProductDiscount(body): void {
    this.customerWiseProductDiscountService
      .createCustomerWiseProductDiscount(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  onSubmit() {
    console.log(this.customerWiseProductDiscountForm.value);
    if (this.customerWiseProductDiscountForm.valid) {
      this.isLoading = true;
      const formValue = this.customerWiseProductDiscountForm.value;
      this.addCustomerWiseProductDiscount(formValue);
    }
  }
}
