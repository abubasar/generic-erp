import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { PaymentMethod } from "app/views/configuration/models/payment-method/payment-method.model";
import { PaymentMethodService } from "app/views/configuration/services/payment-method.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-payment-method-form",
  templateUrl: "./payment-method-form.component.html",
  styleUrls: ["./payment-method-form.component.scss"],
})
export class PaymentMethodFormComponent implements OnInit {
  formTitle: string;
  paymentMethodForm: FormGroup;

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: PaymentMethod,
    private paymentMethodService: PaymentMethodService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm() {
    this.paymentMethodForm = this.fb.group({
      id: [this.data?.id ?? null],
      name: [this.data?.name, Validators.required],
    });
    if (
      this.paymentMethodForm.get("id").value === "" ||
      this.paymentMethodForm.get("id").value == null
    ) {
      this.formTitle = "Add Payment Method";
    } else {
      this.formTitle = "Edit Payment Method";
    }
  }

  addPaymentMethod(body): void {
    this.paymentMethodService.createPaymentMethod(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  updatePaymentMethod(body): void {
    this.paymentMethodService.updatePaymentMethod(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  onSubmit() {
    if (this.paymentMethodForm.value) {
      if (
        this.paymentMethodForm.get("id").value === "" ||
        this.paymentMethodForm.get("id").value == null
      ) {
        this.addPaymentMethod(this.paymentMethodForm.value);
      } else {
        this.updatePaymentMethod(this.paymentMethodForm.value);
      }
    }
  }
}
