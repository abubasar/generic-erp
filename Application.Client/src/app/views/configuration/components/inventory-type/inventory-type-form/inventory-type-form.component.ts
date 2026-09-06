import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { InventoryType } from "app/views/configuration/models/inventory-type/inventory-type.model";
import { InventoryTypeService } from "app/views/configuration/services/inventory-type.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-inventory-type-form",
  templateUrl: "./inventory-type-form.component.html",
  styleUrls: ["./inventory-type-form.component.scss"],
})
export class InventoryTypeFormComponent implements OnInit {
  formTitle: string;
  inventoryTypeForm: FormGroup;

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: InventoryType,
    private inventoryTypeService: InventoryTypeService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm() {
    this.inventoryTypeForm = this.fb.group({
      id: [this.data?.id ?? null],
      name: [this.data?.name, Validators.required],
    });
    if (
      this.inventoryTypeForm.get("id").value === "" ||
      this.inventoryTypeForm.get("id").value == null
    ) {
      this.formTitle = "Add Inventory Type";
    } else {
      this.formTitle = "Edit Inventory Type";
    }
  }

  addInventoryType(body): void {
    this.inventoryTypeService.createInventoryType(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  updateInventoryType(body): void {
    this.inventoryTypeService.updateInventoryType(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  onSubmit() {
    if (this.inventoryTypeForm.value) {
      if (
        this.inventoryTypeForm.get("id").value === "" ||
        this.inventoryTypeForm.get("id").value == null
      ) {
        this.addInventoryType(this.inventoryTypeForm.value);
      } else {
        this.updateInventoryType(this.inventoryTypeForm.value);
      }
    }
  }
}
