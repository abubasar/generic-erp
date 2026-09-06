import { Component, OnInit } from "@angular/core";
import { FormArray, FormBuilder, FormGroup, Validators } from "@angular/forms";
import { Inventory_Type_Id_Finished_Goods } from "app/shared/consts/const";
import { DiscountProductWise } from "app/views/configuration/models/discount-product-wise/discount-product-wise.model";
import { ProductRequest } from "app/views/configuration/models/product/product-request.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { DiscountProductWiseService } from "app/views/configuration/services/discount-product-wise.service";
import { ProductService } from "app/views/configuration/services/product.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-discount-product-wise-add-form",
  templateUrl: "./discount-product-wise-add-form.component.html",
  styleUrls: ["./discount-product-wise-add-form.component.scss"],
})
export class DiscountProductWiseAddFormComponent implements OnInit {
  formTitle: string;
  discountProductWiseForm: FormGroup;
  products: ProductView[];
  discountProductWises: DiscountProductWise[];

  constructor(
    private discountProductWiseService: DiscountProductWiseService,
    private productServices: ProductService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit() {
    this.getAllDiscountProductWises();
    this.getAllProducts();
    this.initializeForm();
  }

  initializeForm() {
    this.createForm();
    this.setFormTitle();
  }

  createForm(): void {
    this.discountProductWiseForm = this.fb.group({
      id: [null],
      name: ["", Validators.required],
      startDate: [null, Validators.required],
      endDate: [null, Validators.required],
      isActive: [false],
      discountProductWiseDetails: this.fb.array([]),
    });
  }

  get discountProductWiseDetails(): FormArray {
    return this.discountProductWiseForm.get(
      "discountProductWiseDetails"
    ) as FormArray;
  }

  populateForm(): void {
    this.populateDiscountProductWiseDetailsFromProduct(this.products);
  }

  populateDiscountProductWiseDetailsFromProduct(products: ProductView[]): void {
    products?.forEach((item: ProductView) => this.addItem(item));
  }

  addItem(item?: ProductView): void {
    this.discountProductWiseDetails.push(
      this.createDiscountProductWiseDetail(item)
    );
  }

  createDiscountProductWiseDetail(item?: ProductView): FormGroup {
    return this.fb.group({
      id: [null],
      productId: [item?.id ?? null, Validators.required],
      product: [item ?? null],
      discountAmountPerKg: [0, Validators.required],
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
    this.formTitle = "Discount Product Wise";
  }

  getAllDiscountProductWises(): void {
    this.discountProductWiseService
      .getAllDiscountProductWises()
      .subscribe((res) => {
        this.discountProductWises = res?.data?.item1;
      });
  }

  handleActiveOffer(event: any) {
    const isActiveControl = this.discountProductWiseForm.get("isActive");

    const isActive = isActiveControl?.value;

    if (!isActive) {
      const isActiveOfferExist = this.discountProductWises.find(
        (item) => item.isActive === true
      );

      if (isActiveOfferExist) {
        setTimeout(() => {
          alert(
            "An active offer discount already exists. Deactivate the existing one."
          );

          // Set the value of isActive to false after showing the alert
          isActiveControl?.setValue(false);
        }, 0);

        return;
      }
    }
  }

  handleSuccessfulSave(res: any) {
    if (res?.succeeded) {
      this.toastr.success(res?.message);
    } else {
      this.toastr.error(res?.message);
    }
  }

  addDiscountProductWise(body): void {
    this.discountProductWiseService
      .createDiscountProductWise(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  onSubmit() {
    console.log(this.discountProductWiseForm.value);
    if (this.discountProductWiseForm.valid) {
      // this.isLoading = true;
      const formValue = this.discountProductWiseForm.value;
      this.addDiscountProductWise(formValue);
    }
  }
}
