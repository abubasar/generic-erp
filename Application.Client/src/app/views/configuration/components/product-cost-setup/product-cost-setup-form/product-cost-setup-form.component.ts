import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { Inventory_Type_Id_Finished_Goods } from "app/shared/consts/const";
import { ProductCostSetup } from "app/views/configuration/models/product-cost-setup/product-cost-setup.model";
import { ProductRequest } from "app/views/configuration/models/product/product-request.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { ProductCostSetupService } from "app/views/configuration/services/product-cost-setup.service";
import { ProductService } from "app/views/configuration/services/product.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-product-cost-setup-form",
  templateUrl: "./product-cost-setup-form.component.html",
  styleUrls: ["./product-cost-setup-form.component.scss"],
})
export class ProductCostSetupFormComponent implements OnInit {
  formTitle: string;
  productCostSetupForm: FormGroup;
  products: ProductView[];

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: ProductCostSetup,
    private productCostSetupService: ProductCostSetupService,
    private productServices: ProductService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.getAllFinishedProducts("");
    this.initializeForm();
  }

  initializeForm() {
    this.productCostSetupForm = this.fb.group({
      id: [this.data?.id ?? null],
      productId: [this.data?.productId, Validators.required],
      directExpense: [this.data?.directExpense, Validators.required],
      factoryOverhead: [this.data?.factoryOverhead, Validators.required],
    });
    if (
      this.productCostSetupForm.get("id").value === "" ||
      this.productCostSetupForm.get("id").value == null
    ) {
      this.formTitle = "Add Product Cost Setup";
    } else {
      this.formTitle = "Edit Product Cost Setup";
    }
  }

  getAllFinishedProducts(keyword: string): void {
    let productRequest = new ProductRequest();
    productRequest.inventoryTypeId = Inventory_Type_Id_Finished_Goods;
    productRequest.keyword = keyword;
    productRequest.page = -1;
    this.productServices.getProducts(productRequest).subscribe((res) => {
      this.products = res?.data?.item1;
    });
  }
  addProductCostSetup(body): void {
    this.productCostSetupService
      .createProductCostSetup(body)
      .subscribe((res) => {
        if (res?.succeeded) {
          this.toastr.success(res?.message);
        } else {
          this.toastr.error(res?.message);
        }
      });
  }

  updateProductCostSetup(body): void {
    this.productCostSetupService
      .updateProductCostSetup(body)
      .subscribe((res) => {
        if (res?.succeeded) {
          this.toastr.success(res?.message);
        } else {
          this.toastr.error(res?.message);
        }
      });
  }

  onSubmit() {
    if (this.productCostSetupForm.value) {
      if (
        this.productCostSetupForm.get("id").value === "" ||
        this.productCostSetupForm.get("id").value == null
      ) {
        this.addProductCostSetup(this.productCostSetupForm.value);
      } else {
        this.updateProductCostSetup(this.productCostSetupForm.value);
      }
    }
  }
}
