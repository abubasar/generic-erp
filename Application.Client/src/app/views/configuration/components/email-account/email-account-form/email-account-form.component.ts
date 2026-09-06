import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { EmailAccountRequest } from "app/views/configuration/models/email-account/email-account-request.model";
import { EmailAccount } from "app/views/configuration/models/email-account/email-account.model";
import { EmailAccountService } from "app/views/configuration/services/email-account.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-email-account-form",
  templateUrl: "./email-account-form.component.html",
  styleUrls: ["./email-account-form.component.scss"],
})
export class EmailAccountFormComponent implements OnInit {
  formTitle: string;
  emailAccountForm: FormGroup;
  emailAccountRequest = new EmailAccountRequest();

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: EmailAccount,
    private emailAccountService: EmailAccountService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm() {
    this.emailAccountForm = this.fb.group({
      id: [this.data?.id ?? null],
      displayName: [this.data?.displayName, Validators.required],
      email: [this.data?.email, Validators.required],
      host: [this.data?.host, Validators.required],
      username: [this.data?.username, Validators.required],
      password: [this.data?.password, Validators.required],
      port: [this.data?.port, Validators.required],
      enableSsl: [this.data?.enableSsl ?? false],
      isDefaultEmailAccount: [this.data?.isDefaultEmailAccount ?? false],
    });
    if (
      this.emailAccountForm.get("id").value === "" ||
      this.emailAccountForm.get("id").value == null
    ) {
      this.formTitle = "Add Email Account";
    } else {
      this.formTitle = "Edit Email Account";
    }
  }

  addEmailAccount(body): void {
    this.emailAccountService.createEmailAccount(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  updateEmailAccount(body): void {
    this.emailAccountService.updateEmailAccount(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  onSubmit() {
    if (this.emailAccountForm.value) {
      if (
        this.emailAccountForm.get("id").value === "" ||
        this.emailAccountForm.get("id").value == null
      ) {
        this.addEmailAccount(this.emailAccountForm.value);
      } else {
        this.updateEmailAccount(this.emailAccountForm.value);
      }
    }
  }
}
