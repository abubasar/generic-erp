import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { Department } from "app/views/configuration/models/department/department.model";
import { DepartmentService } from "app/views/configuration/services/department.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-department-form",
  templateUrl: "./department-form.component.html",
  styleUrls: ["./department-form.component.scss"],
})
export class DepartmentFormComponent implements OnInit {
  formTitle: string;
  departmentForm: FormGroup;

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: Department,
    private departmentService: DepartmentService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm() {
    this.departmentForm = this.fb.group({
      id: [this.data?.id ?? null],
      name: [this.data?.name, Validators.required],
    });
    if (
      this.departmentForm.get("id").value === "" ||
      this.departmentForm.get("id").value == null
    ) {
      this.formTitle = "Add Department";
    } else {
      this.formTitle = "Edit Department";
    }
  }

  addDepartment(body): void {
    this.departmentService.createDepartment(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  updateDepartment(body): void {
    this.departmentService.updateDepartment(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  onSubmit() {
    if (this.departmentForm.value) {
      if (
        this.departmentForm.get("id").value === "" ||
        this.departmentForm.get("id").value == null
      ) {
        this.addDepartment(this.departmentForm.value);
      } else {
        this.updateDepartment(this.departmentForm.value);
      }
    }
  }
}
