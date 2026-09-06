import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { AccountType } from "app/views/configuration/models/account-type/account-type.model";
import { AccountTypeService } from "app/views/configuration/services/account-type.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-account-type-form",
  templateUrl: "./account-type-form.component.html",
  styleUrls: ["./account-type-form.component.scss"],
})
export class AccountTypeFormComponent implements OnInit {
  formTitle: string;
  accountTypeForm: FormGroup;

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: AccountType,
    private accountTypeService: AccountTypeService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm() {
    this.accountTypeForm = this.fb.group({
      id: [this.data?.id ?? null],
      name: [this.data?.name, Validators.required],
      startingNumber: [this.data?.startingNumber, Validators.required],
    });

    if (
      this.accountTypeForm.get("id").value === "" ||
      this.accountTypeForm.get("id").value == null
    ) {
      this.formTitle = "Add Account Type";
    } else {
      this.formTitle = "Edit Account Type";
    }
  }

  addAccountType(body): void {
    this.accountTypeService.createAccountType(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  updateAccountType(body): void {
    this.accountTypeService.updateAccountType(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  onSubmit() {
    if (this.accountTypeForm.value) {
      if (
        this.accountTypeForm.get("id").value === "" ||
        this.accountTypeForm.get("id").value == null
      ) {
        this.addAccountType(this.accountTypeForm.value);
      } else {
        this.updateAccountType(this.accountTypeForm.value);
      }
    }
  }
}
