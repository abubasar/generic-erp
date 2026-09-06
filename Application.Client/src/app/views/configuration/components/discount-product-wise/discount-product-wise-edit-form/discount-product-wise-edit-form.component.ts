import { Component, Inject, OnInit } from "@angular/core";
import { FormArray, FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { Inventory_Type_Id_Finished_Goods } from "app/shared/consts/const";
import { SnackBarService } from "app/shared/services/snack-bar.service";
import {
  DiscountProductWise,
  DiscountProductWiseDetail,
} from "app/views/configuration/models/discount-product-wise/discount-product-wise.model";
import { ProductRequest } from "app/views/configuration/models/product/product-request.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { DiscountProductWiseService } from "app/views/configuration/services/discount-product-wise.service";
import { ProductService } from "app/views/configuration/services/product.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-discount-product-wise-edit-form",
  templateUrl: "./discount-product-wise-edit-form.component.html",
  styleUrls: ["./discount-product-wise-edit-form.component.scss"],
})
export class DiscountProductWiseEditFormComponent implements OnInit {
  formTitle: string;
  discountProductWiseForm: FormGroup;
  products: ProductView[];
  productSearchResult: ProductView[];
  discountProductWises: DiscountProductWise[];

  constructor(
    @Inject(MAT_DIALOG_DATA)
    private data: DiscountProductWise,
    private discountProductWiseService: DiscountProductWiseService,
    private productServices: ProductService,
    private fb: FormBuilder,
    private toastr: ToastrService,
    private snackBarService: SnackBarService
  ) {}

  ngOnInit() {
    this.getAllProducts();
    this.initializeForm();
    this.getAllDiscountProductWises();
  }

  initializeForm() {
    this.createForm();
    this.populateForm();
    this.setFormTitle();
  }

  createForm(): void {
    this.discountProductWiseForm = this.fb.group({
      id: [this.data?.id ?? null],
      name: [this.data?.name, Validators.required],
      startDate: [this.data?.startDate, Validators.required],
      endDate: [this.data?.endDate, Validators.required],
      isActive: [this.data?.isActive],
      discountProductWiseDetails: this.fb.array([]),
    });
  }

  get discountProductWiseDetails(): FormArray {
    return this.discountProductWiseForm?.get(
      "discountProductWiseDetails"
    ) as FormArray;
  }

  populateForm(): void {
    if (this.discountProductWiseForm.get("id").value) {
      this.populateDiscountProductWiseDetails(this.data);
    }
  }

  populateDiscountProductWiseDetails(data: DiscountProductWise): void {
    data.discountProductWiseDetails.forEach((item: DiscountProductWiseDetail) =>
      this.addItem(item)
    );
  }

  addItem(item?: DiscountProductWiseDetail): void {
    this.discountProductWiseDetails.push(
      this.createDiscountProductWiseDetail(item)
    );
    this.filterProducts();
  }

  createDiscountProductWiseDetail(item?: DiscountProductWiseDetail): FormGroup {
    return this.fb.group({
      id: [item?.id ?? null],
      productId: [item?.productId ?? null, Validators.required],
      product: [item ?? null],
      discountAmountPerKg: [
        item?.discountAmountPerKg ?? 0,
        Validators.required,
      ],
    });
  }

  /**------------------- Autocomplete Code ---------------------  */
  onProductChange(event: any, itemIndex: number): void {
    const name = event.target.name;
    if (name === "productId") {
      const term = this.discountProductWiseDetails
        .at(itemIndex)
        .get("productId");
      this.searchProduct(term.value || "");
    }
  }

  private searchProduct(value: string) {
    console.log("search value", value);
    const searchValue = value?.toLowerCase();
    this.productSearchResult = this.filteredProducts?.filter((option) =>
      option.name?.toLowerCase()?.includes(searchValue)
    );
  }

  findProductById(id) {
    return this.products.find((product) => product.id === id);
  }

  handleProductSelection(event, index) {
    const particularDetail = this.discountProductWiseDetails.at(index);
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
    const isProductAdded = this.discountProductWiseDetails.value.some(
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
      this.data?.discountProductWiseDetails?.find(
        (x) => x?.productId == productId
      )?.product;
    return product?.name;
  }

  filteredProducts: ProductView[];
  filterProducts() {
    let formDetailsValue = this.discountProductWiseForm?.get(
      "discountProductWiseDetails"
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
    if (this.discountProductWiseForm.get("id").value) {
      this.formTitle = "Edit Discount Product Wise";
    }
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

  updateDiscountProductWise(body): void {
    this.discountProductWiseService
      .updateDiscountProductWise(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  deletedIds: string = "";
  onDeleteItem(id: string, itemIndex: number): void {
    if (id) this.deletedIds += `${id},`;
    this.discountProductWiseDetails.removeAt(itemIndex);
  }

  onSubmit() {
    if (this.discountProductWiseForm.valid) {
      // this.isLoading = true;
      const formValue = this.discountProductWiseForm.value;
      this.updateDiscountProductWise(formValue);
    }
  }
}
