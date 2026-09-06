import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { DepartmentRequest } from "app/views/configuration/models/department/department-request.model";
import { Department } from "app/views/configuration/models/department/department.model";
import { Designation } from "app/views/configuration/models/designation/designation.model";
import { DepartmentService } from "app/views/configuration/services/department.service";
import { DesignationService } from "app/views/configuration/services/designation.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-designation-form",
  templateUrl: "./designation-form.component.html",
  styleUrls: ["./designation-form.component.scss"],
})
export class DesignationFormComponent implements OnInit {
  formTitle: string;
  designationForm: FormGroup;
  departments: Department[];

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: Designation,
    private designationService: DesignationService,
    private departmentService: DepartmentService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.getAllDepartments();
    this.initializeForm();
  }

  initializeForm() {
    this.designationForm = this.fb.group({
      id: [this.data?.id ?? null],
      name: [this.data?.name, Validators.required],
      departmentId: [this.data?.departmentId, Validators.required],
    });
    if (
      this.designationForm.get("id").value === "" ||
      this.designationForm.get("id").value == null
    ) {
      this.formTitle = "Add Designation";
    } else {
      this.formTitle = "Edit Designation";
    }
  }

  getAllDepartments(): void {
    let departmentRequest = new DepartmentRequest();
    departmentRequest.page = -1;
    this.departmentService
      .getDepartments(departmentRequest)
      .subscribe((res) => {
        console.log(res?.data?.item1);
        this.departments = res?.data?.item1;
      });
  }

  addDesignation(body): void {
    this.designationService.createDesignation(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  updateDesignation(body): void {
    this.designationService.updateDesignation(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  onSubmit() {
    if (this.designationForm.value) {
      if (
        this.designationForm.get("id").value === "" ||
        this.designationForm.get("id").value == null
      ) {
        this.addDesignation(this.designationForm.value);
      } else {
        this.updateDesignation(this.designationForm.value);
      }
    }
  }
}
