import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { InventoryTypeRequest } from "app/views/configuration/models/inventory-type/inventory-type-request.model";
import { InventoryType } from "app/views/configuration/models/inventory-type/inventory-type.model";
import { Store } from "app/views/configuration/models/store/store.model";
import { InventoryTypeService } from "app/views/configuration/services/inventory-type.service";
import { StoreService } from "app/views/configuration/services/store.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-store-form",
  templateUrl: "./store-form.component.html",
  styleUrls: ["./store-form.component.scss"],
})
export class StoreFormComponent implements OnInit {
  formTitle: string;
  storeForm: FormGroup;
  inventoryTypes: InventoryType[];

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: Store,
    private storeService: StoreService,
    private inventoryTypeServices: InventoryTypeService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.getAllInventoryTypes();
    this.initializeForm();
  }

  initializeForm() {
    this.storeForm = this.fb.group({
      id: [this.data?.id ?? null],
      code: [this.data?.code ?? ""],
      name: [this.data?.name, Validators.required],
      depoChargePerKg: [this.data?.depoChargePerKg ?? 0, Validators.required],
      inventoryTypeId: [this.data?.inventoryTypeId, Validators.required],
    });
    if (
      this.storeForm.get("id").value === "" ||
      this.storeForm.get("id").value == null
    ) {
      this.formTitle = "Add Store";
    } else {
      this.formTitle = "Edit Store";
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

  addStore(body): void {
    this.storeService.createStore(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  updateStore(body): void {
    this.storeService.updateStore(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  onSubmit() {
    if (this.storeForm.value) {
      if (
        this.storeForm.get("id").value === "" ||
        this.storeForm.get("id").value == null
      ) {
        this.addStore(this.storeForm.value);
      } else {
        this.updateStore(this.storeForm.value);
      }
    }
  }
}
