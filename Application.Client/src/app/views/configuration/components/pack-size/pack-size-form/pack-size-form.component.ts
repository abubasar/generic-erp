import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { PackSize } from "app/views/configuration/models/pack-size/pack-size.model";
import { PackSizeService } from "app/views/configuration/services/pack-size.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-pack-size-form",
  templateUrl: "./pack-size-form.component.html",
  styleUrls: ["./pack-size-form.component.scss"],
})
export class PackSizeFormComponent implements OnInit {
  formTitle: string;
  packSizeForm: FormGroup;

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: PackSize,
    private packSizeService: PackSizeService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm() {
    this.packSizeForm = this.fb.group({
      id: [this.data?.id ?? null],
      name: [this.data?.name, Validators.required],
    });
    if (
      this.packSizeForm.get("id").value === "" ||
      this.packSizeForm.get("id").value == null
    ) {
      this.formTitle = "Add Pack Size";
    } else {
      this.formTitle = "Edit Pack Size";
    }
  }

  addPackSize(body): void {
    this.packSizeService.createPackSize(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  updatePackSize(body): void {
    this.packSizeService.updatePackSize(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  onSubmit() {
    if (this.packSizeForm.value) {
      if (
        this.packSizeForm.get("id").value === "" ||
        this.packSizeForm.get("id").value == null
      ) {
        this.addPackSize(this.packSizeForm.value);
      } else {
        this.updatePackSize(this.packSizeForm.value);
      }
    }
  }
}
