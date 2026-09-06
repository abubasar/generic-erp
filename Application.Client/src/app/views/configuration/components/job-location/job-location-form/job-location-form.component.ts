import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { JobLocation } from "app/views/configuration/models/job-location/job-location.model";
import { JobLocationService } from "app/views/configuration/services/job-location.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-job-location-form",
  templateUrl: "./job-location-form.component.html",
  styleUrls: ["./job-location-form.component.scss"],
})
export class JobLocationFormComponent implements OnInit {
  formTitle: string;
  jobLocationForm: FormGroup;

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: JobLocation,
    private jobLocationService: JobLocationService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm() {
    this.jobLocationForm = this.fb.group({
      id: [this.data?.id ?? null],
      name: [this.data?.name, Validators.required],
    });
    if (
      this.jobLocationForm.get("id").value === "" ||
      this.jobLocationForm.get("id").value == null
    ) {
      this.formTitle = "Add Job Location";
    } else {
      this.formTitle = "Edit Job Location";
    }
  }

  addJobLocation(body): void {
    this.jobLocationService.createJobLocation(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  updateJobLocation(body): void {
    this.jobLocationService.updateJobLocation(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  onSubmit() {
    if (this.jobLocationForm.value) {
      if (
        this.jobLocationForm.get("id").value === "" ||
        this.jobLocationForm.get("id").value == null
      ) {
        this.addJobLocation(this.jobLocationForm.value);
      } else {
        this.updateJobLocation(this.jobLocationForm.value);
      }
    }
  }
}
