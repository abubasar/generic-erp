import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { Tenant } from "app/views/configuration/models/tenant/tenant.model";
import { TenantService } from "app/views/configuration/services/tenant.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-tenant-form",
  templateUrl: "./tenant-form.component.html",
  styleUrls: ["./tenant-form.component.scss"],
})
export class TenantFormComponent implements OnInit {
  formTitle: string;
  tenantForm: FormGroup;

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: Tenant,
    private tenantService: TenantService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm() {
    this.tenantForm = this.fb.group({
      id: [this.data?.id ?? null],
      name: [this.data?.name, Validators.required],
      code: [this.data?.code],
      timeZoneId: [this.data?.timeZoneId],
      address: [this.data?.address],
      contactNo: [this.data?.contactNo],
      binno: [this.data?.binno],
      email: [this.data?.email],
    });
    if (
      this.tenantForm.get("id").value === "" ||
      this.tenantForm.get("id").value == null
    ) {
      this.formTitle = "Add Tenant";
    } else {
      this.formTitle = "Edit Tenant";
    }
  }

  addTenant(body): void {
    this.tenantService.createTenant(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  // updateTenant(body): void {
  //   this.tenantService.updateTenant(body).subscribe((res) => {
  //     if (res?.succeeded) {
  //       this.toastr.success(res?.message);
  //     } else {
  //       this.toastr.error(res?.message);
  //     }
  //   });
  // }

  onSubmit() {
    if (this.tenantForm.value) {
      if (
        this.tenantForm.get("id").value === "" ||
        this.tenantForm.get("id").value == null
      ) {
        this.addTenant(this.tenantForm.value);
      }
      // else {
      //   this.updateTenant(this.tenantForm.value);
      // }
    }
  }
}
