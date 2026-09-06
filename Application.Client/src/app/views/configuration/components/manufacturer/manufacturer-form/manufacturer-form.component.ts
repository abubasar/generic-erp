import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { Manufacturer } from "app/views/configuration/models/manufacturer/manufacturer.model";
import { ManufacturerService } from "app/views/configuration/services/manufacturer.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-manufacturer-form",
  templateUrl: "./manufacturer-form.component.html",
  styleUrls: ["./manufacturer-form.component.scss"],
})
export class ManufacturerFormComponent implements OnInit {
  formTitle: string;
  manufacturerForm: FormGroup;

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: Manufacturer,
    private manufacturerService: ManufacturerService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm() {
    this.manufacturerForm = this.fb.group({
      id: [this.data?.id ?? null],
      name: [this.data?.name, Validators.required],
    });
    if (
      this.manufacturerForm.get("id").value === "" ||
      this.manufacturerForm.get("id").value == null
    ) {
      this.formTitle = "Add Manufacturer";
    } else {
      this.formTitle = "Edit Manufacturer";
    }
  }

  addManufacturer(body): void {
    this.manufacturerService.createManufacturer(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  updateManufacturer(body): void {
    this.manufacturerService.updateManufacturer(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  onSubmit() {
    if (this.manufacturerForm.value) {
      if (
        this.manufacturerForm.get("id").value === "" ||
        this.manufacturerForm.get("id").value == null
      ) {
        this.addManufacturer(this.manufacturerForm.value);
      } else {
        this.updateManufacturer(this.manufacturerForm.value);
      }
    }
  }
}
