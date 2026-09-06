import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { Role } from "app/views/configuration/models/role/role.model";
import { RoleService } from "app/views/configuration/services/role.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-role-form",
  templateUrl: "./role-form.component.html",
  styleUrls: ["./role-form.component.scss"],
})
export class RoleFormComponent implements OnInit {
  formTitle: string;
  roleForm: FormGroup;

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: Role,
    private roleService: RoleService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm() {
    this.roleForm = this.fb.group({
      id: [this.data?.id ?? null],
      name: [this.data?.name, Validators.required],
    });
    if (
      this.roleForm.get("id").value === "" ||
      this.roleForm.get("id").value == null
    ) {
      this.formTitle = "Add Role";
    } else {
      this.formTitle = "Edit Role";
    }
  }

  addRole(body): void {
    this.roleService.createRole(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  updateRole(body): void {
    this.roleService.updateRole(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  onSubmit() {
    if (this.roleForm.value) {
      if (
        this.roleForm.get("id").value === "" ||
        this.roleForm.get("id").value == null
      ) {
        this.addRole(this.roleForm.value);
      } else {
        this.updateRole(this.roleForm.value);
      }
    }
  }
}
