import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { FinancialYear } from "app/views/configuration/models/financial-year/financial-year.model";
import { FinancialYearService } from "app/views/configuration/services/financial-year.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-financial-year-form",
  templateUrl: "./financial-year-form.component.html",
  styleUrls: ["./financial-year-form.component.scss"],
})
export class FinancialYearFormComponent implements OnInit {
  formTitle: string;
  financialYearForm: FormGroup;

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: FinancialYear,
    private financialYearService: FinancialYearService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm() {
    this.financialYearForm = this.fb.group({
      id: [this.data?.id ?? null],
      code: [this.data?.code ?? ""],
      name: [this.data?.name, Validators.required],
      startDate: [this.data?.startDate, Validators.required],
      endDate: [this.data?.endDate, Validators.required],
      isActive: [this.data?.isActive ?? false],
    });
    if (
      this.financialYearForm.get("id").value === "" ||
      this.financialYearForm.get("id").value == null
    ) {
      this.formTitle = "Add Financial Year";
    } else {
      this.formTitle = "Edit Financial Year";
    }
  }

  addFinancialYear(body): void {
    this.financialYearService.createFinancialYear(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  updateFinancialYear(body): void {
    this.financialYearService.updateFinancialYear(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  onSubmit() {
    if (this.financialYearForm.value) {
      if (
        this.financialYearForm.get("id").value === "" ||
        this.financialYearForm.get("id").value == null
      ) {
        this.addFinancialYear(this.financialYearForm.value);
      } else {
        this.updateFinancialYear(this.financialYearForm.value);
      }
    }
  }
}
