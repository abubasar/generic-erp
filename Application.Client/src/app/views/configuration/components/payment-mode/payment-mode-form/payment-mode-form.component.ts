import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { PaymentMode } from "app/views/configuration/models/payment-mode/payment-mode.model";
import { PaymentModeService } from "app/views/configuration/services/payment-mode.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-payment-mode-form",
  templateUrl: "./payment-mode-form.component.html",
  styleUrls: ["./payment-mode-form.component.scss"],
})
export class PaymentModeFormComponent implements OnInit {
  formTitle: string;
  paymentModeForm: FormGroup;

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: PaymentMode,
    private paymentModeService: PaymentModeService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm() {
    this.paymentModeForm = this.fb.group({
      id: [this.data?.id ?? null],
      name: [this.data?.name, Validators.required],
    });
    if (
      this.paymentModeForm.get("id").value === "" ||
      this.paymentModeForm.get("id").value == null
    ) {
      this.formTitle = "Add Payment Mode";
    } else {
      this.formTitle = "Edit Payment Mode";
    }
  }

  addPaymentMode(body): void {
    this.paymentModeService.createPaymentMode(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  updatePaymentMode(body): void {
    this.paymentModeService.updatePaymentMode(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  onSubmit() {
    if (this.paymentModeForm.value) {
      if (
        this.paymentModeForm.get("id").value === "" ||
        this.paymentModeForm.get("id").value == null
      ) {
        this.addPaymentMode(this.paymentModeForm.value);
      } else {
        this.updatePaymentMode(this.paymentModeForm.value);
      }
    }
  }
}
