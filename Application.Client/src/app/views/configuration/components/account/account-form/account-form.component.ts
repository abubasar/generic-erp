import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA, MatDialog } from "@angular/material/dialog";
import { AccountTypeRequest } from "app/views/configuration/models/account-type/account-type-request.model";
import { AccountType } from "app/views/configuration/models/account-type/account-type.model";
import { AccountRequest } from "app/views/configuration/models/account/account-request.model";
import { Account } from "app/views/configuration/models/account/account.model";
import { ParentAccount } from "app/views/configuration/models/account/parent-account.model";
import { AccountTypeService } from "app/views/configuration/services/account-type.service";
import { AccountService } from "app/views/configuration/services/account.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-account-form",
  templateUrl: "./account-form.component.html",
  styleUrls: ["./account-form.component.scss"],
})
export class AccountFormComponent implements OnInit {
  formTitle: string;
  isEditMode: boolean = false;
  accountForm: FormGroup;
  accountTypes: AccountType[];
  parentAccounts: ParentAccount[];
  parentNames: ParentAccount[];
  accountRequest = new AccountRequest();

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: Account,
    private dialog: MatDialog,
    private accountService: AccountService,
    private accountTypeService: AccountTypeService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.getAllAccountTypes();
    this.getParentAccounts();
  }

  initializeForm() {
    this.accountForm = this.fb.group({
      id: [this.data?.id ?? null],
      code: [this.data?.code ?? ""],
      name: [this.data?.name, Validators.required],
      accountTypeId: [this.data?.accountTypeId, Validators.required],
      level: [this.data?.level ?? 0],
      parentId: [this.data?.parentId ?? null],
      isControlAccount: [this.data?.isControlAccount ?? false],
    });

    if (
      this.accountForm.get("id").value === "" ||
      this.accountForm.get("id").value == null
    ) {
      this.formTitle = "Add Account";
      this.isEditMode = false;
    } else {
      this.formTitle = "Edit Account";
      this.isEditMode = true;
    }
  }

  getParentAccounts(): void {
    this.accountService.getParentAccounts().subscribe((res) => {
      this.parentAccounts = res?.data;
      if (this.parentAccounts && this.data?.accountTypeId) {
        this.onSelectedAccountType(this.data?.accountTypeId);
      }
    });
  }

  onSelectedAccountType(id: string) {
    this.parentNames = this.parentAccounts?.filter(
      (item) => item.accountTypeId === id
    );
  }

  getAllAccountTypes(): void {
    this.accountTypeService
      .getAllAccountTypes()
      .subscribe((res) => {
        this.accountTypes = res?.data?.item1;
      });
  }

  addAccount(body): void {
    this.accountService.createAccount(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
        this.dialog.closeAll();
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  updateAccount(body): void {
    this.accountService.updateAccount(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
        this.dialog.closeAll();
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  onSubmit() {
    if (this.accountForm.value) {
      if (
        this.accountForm.get("id").value === "" ||
        this.accountForm.get("id").value == null
      ) {
        this.addAccount(this.accountForm.value);
      } else {
        this.updateAccount(this.accountForm.value);
      }
    }
  }
}
