import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { Region } from "app/views/configuration/models/region/region.model";
import { RegionService } from "app/views/configuration/services/region.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-region-form",
  templateUrl: "./region-form.component.html",
  styleUrls: ["./region-form.component.scss"],
})
export class RegionFormComponent implements OnInit {
  formTitle: string;
  regionForm: FormGroup;

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: Region,
    private regionService: RegionService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm() {
    this.regionForm = this.fb.group({
      id: [this.data?.id ?? null],
      name: [this.data?.name, Validators.required],
    });
    if (
      this.regionForm.get("id").value === "" ||
      this.regionForm.get("id").value == null
    ) {
      this.formTitle = "Add Region";
    } else {
      this.formTitle = "Edit Region";
    }
  }

  addDepartment(body): void {
    this.regionService.createRegion(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  updateDepartment(body): void {
    this.regionService.updateRegion(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  onSubmit() {
    if (this.regionForm.value) {
      if (
        this.regionForm.get("id").value === "" ||
        this.regionForm.get("id").value == null
      ) {
        this.addDepartment(this.regionForm.value);
      } else {
        this.updateDepartment(this.regionForm.value);
      }
    }
  }
}
