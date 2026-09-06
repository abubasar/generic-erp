import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { FundTransferTransactionType } from "app/views/configuration/models/fund-transfer-transaction-type/fund-transfer-transaction-type.model";
import { FundTransferTransactionTypeService } from "app/views/configuration/services/fund-transfer-transaction-type.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-fund-transfer-transaction-type-form",
  templateUrl: "./fund-transfer-transaction-type-form.component.html",
  styleUrls: ["./fund-transfer-transaction-type-form.component.scss"],
})
export class FundTransferTransactionTypeFormComponent implements OnInit {
  formTitle: string;
  fundTransferTransactionTypeForm: FormGroup;

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: FundTransferTransactionType,
    private paymentMethodService: FundTransferTransactionTypeService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm() {
    this.fundTransferTransactionTypeForm = this.fb.group({
      id: [this.data?.id ?? null],
      name: [this.data?.name, Validators.required],
    });
    if (
      this.fundTransferTransactionTypeForm.get("id").value === "" ||
      this.fundTransferTransactionTypeForm.get("id").value == null
    ) {
      this.formTitle = "Add Fund Transfer Transaction Type";
    } else {
      this.formTitle = "Edit Fund Transfer Transaction Type";
    }
  }

  addFundTransferTransactionType(body): void {
    this.paymentMethodService
      .createFundTransferTransactionType(body)
      .subscribe((res) => {
        if (res?.succeeded) {
          this.toastr.success(res?.message);
        } else {
          this.toastr.error(res?.message);
        }
      });
  }

  updateFundTransferTransactionType(body): void {
    this.paymentMethodService
      .updateFundTransferTransactionType(body)
      .subscribe((res) => {
        if (res?.succeeded) {
          this.toastr.success(res?.message);
        } else {
          this.toastr.error(res?.message);
        }
      });
  }

  onSubmit() {
    if (this.fundTransferTransactionTypeForm.value) {
      if (
        this.fundTransferTransactionTypeForm.get("id").value === "" ||
        this.fundTransferTransactionTypeForm.get("id").value == null
      ) {
        this.addFundTransferTransactionType(
          this.fundTransferTransactionTypeForm.value
        );
      } else {
        this.updateFundTransferTransactionType(
          this.fundTransferTransactionTypeForm.value
        );
      }
    }
  }
}
