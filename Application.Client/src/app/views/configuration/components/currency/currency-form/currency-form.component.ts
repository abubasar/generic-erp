import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { Currency } from "app/views/configuration/models/currency/currency.model";
import { CurrencyService } from "app/views/configuration/services/currency.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-currency-form",
  templateUrl: "./currency-form.component.html",
  styleUrls: ["./currency-form.component.scss"],
})
export class CurrencyFormComponent implements OnInit {
  formTitle: string;
  currencyForm: FormGroup;

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: Currency,
    private currencyService: CurrencyService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm() {
    this.currencyForm = this.fb.group({
      id: [this.data?.id ?? null],
      name: [this.data?.name, Validators.required],
    });
    if (
      this.currencyForm.get("id").value === "" ||
      this.currencyForm.get("id").value == null
    ) {
      this.formTitle = "Add Currency";
    } else {
      this.formTitle = "Edit Currency";
    }
  }

  addCurrency(body): void {
    this.currencyService.createCurrency(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  updateCurrency(body): void {
    this.currencyService.updateCurrency(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  onSubmit() {
    if (this.currencyForm.value) {
      if (
        this.currencyForm.get("id").value === "" ||
        this.currencyForm.get("id").value == null
      ) {
        this.addCurrency(this.currencyForm.value);
      } else {
        this.updateCurrency(this.currencyForm.value);
      }
    }
  }
}
