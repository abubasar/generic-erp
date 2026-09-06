import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { MeasurementUnit } from "app/views/configuration/models/measurement-unit/measurement-unit.model";
import { MeasurementUnitService } from "app/views/configuration/services/measurement-unit.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-measurement-unit-form",
  templateUrl: "./measurement-unit-form.component.html",
  styleUrls: ["./measurement-unit-form.component.scss"],
})
export class MeasurementUnitFormComponent implements OnInit {
  formTitle: string;
  measurementUnitForm: FormGroup;

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: MeasurementUnit,
    private measurementUnitService: MeasurementUnitService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm() {
    this.measurementUnitForm = this.fb.group({
      id: [this.data?.id ?? null],
      name: [this.data?.name, Validators.required],
    });
    if (
      this.measurementUnitForm.get("id").value === "" ||
      this.measurementUnitForm.get("id").value == null
    ) {
      this.formTitle = "Add Measurement Unit";
    } else {
      this.formTitle = "Edit Measurement Unit";
    }
  }

  addMeasurementUnit(body): void {
    this.measurementUnitService.createMeasurementUnit(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  updateMeasurementUnit(body): void {
    this.measurementUnitService.updateMeasurementUnit(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  onSubmit() {
    if (this.measurementUnitForm.value) {
      if (
        this.measurementUnitForm.get("id").value === "" ||
        this.measurementUnitForm.get("id").value == null
      ) {
        this.addMeasurementUnit(this.measurementUnitForm.value);
      } else {
        this.updateMeasurementUnit(this.measurementUnitForm.value);
      }
    }
  }
}
