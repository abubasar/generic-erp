import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { Company } from "app/views/configuration/models/company/company.model";
import { CompanyService } from "app/views/configuration/services/company.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-company-form",
  templateUrl: "./company-form.component.html",
  styleUrls: ["./company-form.component.scss"],
})
export class CompanyFormComponent implements OnInit {
  formTitle: string;
  companyForm: FormGroup;

  constructor(
    @Inject(MAT_DIALOG_DATA)
    private data: Company,
    private companyService: CompanyService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit() {
    this.initializeForm();
  }

  initializeForm() {
    this.companyForm = this.fb.group({
      id: [this.data?.id ?? null],
      name: [this.data?.name, Validators.required],
    });

    if (
      this.companyForm.get("id").value === "" ||
      this.companyForm.get("id").value == null
    ) {
      this.formTitle = "Add Company";
    } else {
      this.formTitle = "Edit Company";
    }
  }

  addCompany(body): void {
    this.companyService.createCompany(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  updateCompany(body): void {
    this.companyService.updateCompany(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  onSubmit() {
    if (this.companyForm.value) {
      if (
        this.companyForm.get("id").value === "" ||
        this.companyForm.get("id").value == null
      ) {
        this.addCompany(this.companyForm.value);
      } else {
        this.updateCompany(this.companyForm.value);
      }
    }
  }
}
