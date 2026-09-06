import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { Country } from "app/views/configuration/models/country/country.model";
import { CountryService } from "app/views/configuration/services/country.service";

import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-country-form",
  templateUrl: "./country-form.component.html",
  styleUrls: ["./country-form.component.scss"],
})
export class CountryFormComponent implements OnInit {
  formTitle: string;
  countryForm: FormGroup;

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: Country,
    private countryService: CountryService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm() {
    this.countryForm = this.fb.group({
      id: [this.data?.id ?? null],
      name: [this.data?.name, Validators.required],
    });
    if (
      this.countryForm.get("id").value === "" ||
      this.countryForm.get("id").value == null
    ) {
      this.formTitle = "Add Country";
    } else {
      this.formTitle = "Edit Country";
    }
  }

  addCountry(body): void {
    this.countryService.createCountry(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  updateCountry(body): void {
    this.countryService.updateCountry(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  onSubmit() {
    if (this.countryForm.value) {
      if (
        this.countryForm.get("id").value === "" ||
        this.countryForm.get("id").value == null
      ) {
        this.addCountry(this.countryForm.value);
      } else {
        this.updateCountry(this.countryForm.value);
      }
    }
  }
}
