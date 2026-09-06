import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { InventoryTypeRequest } from "app/views/configuration/models/inventory-type/inventory-type-request.model";
import { InventoryType } from "app/views/configuration/models/inventory-type/inventory-type.model";
import { ProductType } from "app/views/configuration/models/product-type/product-type.model";
import { InventoryTypeService } from "app/views/configuration/services/inventory-type.service";
import { ProductTypeService } from "app/views/configuration/services/product-type.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-product-type-form",
  templateUrl: "./product-type-form.component.html",
  styleUrls: ["./product-type-form.component.scss"],
})
export class ProductTypeFormComponent implements OnInit {
  formTitle: string;
  productTypeForm: FormGroup;
  inventoryTypes: InventoryType[];

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: ProductType,
    private productTypeService: ProductTypeService,
    private inventoryTypeServices: InventoryTypeService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.getAllInventoryTypes();
    this.initializeForm();
  }

  initializeForm() {
    this.productTypeForm = this.fb.group({
      id: [this.data?.id ?? null],
      inventoryTypeId: [this.data?.inventoryTypeId ?? null],
      name: [this.data?.name, Validators.required],
    });
    if (
      this.productTypeForm.get("id").value === "" ||
      this.productTypeForm.get("id").value == null
    ) {
      this.formTitle = "Add Product Type";
    } else {
      this.formTitle = "Edit Product Type";
    }
  }

  getAllInventoryTypes(): void {
    let inventoryTypeRequest = new InventoryTypeRequest();
    inventoryTypeRequest.page = -1;
    this.inventoryTypeServices
      .getInventoryTypes(inventoryTypeRequest)
      .subscribe((res) => {
        this.inventoryTypes = res?.data?.item1;
      });
  }

  addProductType(body): void {
    this.productTypeService.createProductType(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  updateProductType(body): void {
    this.productTypeService.updateProductType(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  onSubmit() {
    if (this.productTypeForm.value) {
      if (
        this.productTypeForm.get("id").value === "" ||
        this.productTypeForm.get("id").value == null
      ) {
        this.addProductType(this.productTypeForm.value);
      } else {
        this.updateProductType(this.productTypeForm.value);
      }
    }
  }
}
