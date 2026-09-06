import { Component, Inject, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Generic } from 'app/views/configuration/models/generic/generic.model';
import { ProductTypeRequest } from 'app/views/configuration/models/product-type/product-type-request.model';
import { ProductType } from 'app/views/configuration/models/product-type/product-type.model';
import { GenericService } from 'app/views/configuration/services/generic.service';
import { ProductTypeService } from 'app/views/configuration/services/product-type.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: "app-generic-form",
  templateUrl: "./generic-form.component.html",
  styleUrls: ["./generic-form.component.scss"],
})
export class GenericFormComponent implements OnInit {
  formTitle: string;
  genericForm: FormGroup;
  productTypes: ProductType[];

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: Generic,
    private genericService: GenericService,
    private productTypeService: ProductTypeService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.getAllProductTypes();
    this.initializeForm();
  }
  initializeForm() {
    this.genericForm = this.fb.group({
      id: [this.data?.id ?? null],
      name: [this.data?.name, Validators.required],
      productTypeId: [this.data?.productTypeId, Validators.required],
    });
    if (
      this.genericForm.get("id").value === "" ||
      this.genericForm.get("id").value == null
    ) {
      this.formTitle = "Add Generic";
    } else {
      this.formTitle = "Edit Generic";
    }
  }

  getAllProductTypes(): void {
    let productTypeRequest = new ProductTypeRequest();
    productTypeRequest.page = -1;
    this.productTypeService.getProductTypes(productTypeRequest).subscribe((res) => {
      console.log(res?.data?.item1);
      this.productTypes = res?.data?.item1;
    });
  }

  addGeneric(body): void {
    this.genericService.createGeneric(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  updateGeneric(body): void {
    this.genericService.updateGeneric(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  onSubmit() {
    if (this.genericForm.value) {
      if (
        this.genericForm.get("id").value === "" ||
        this.genericForm.get("id").value == null
      ) {
        this.addGeneric(this.genericForm.value);
      } else {
        this.updateGeneric(this.genericForm.value);
      }
    }
  }
}
