import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { MatDialog } from "@angular/material/dialog";
import { MatTableDataSource } from "@angular/material/table";
import { Page_Size_Options } from "app/shared/consts/const";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { AccountTypeRequest } from "../../models/account-type/account-type-request.model";
import { AccountType } from "../../models/account-type/account-type.model";
import { AccountTypeService } from "../../services/account-type.service";
import { AccountTypeFormComponent } from "./account-type-form/account-type-form.component";

@Component({
  selector: "app-account-type",
  templateUrl: "./account-type.component.html",
  styleUrls: ["./account-type.component.scss"],
})
export class AccountTypeComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  displayedColumns: string[] = ["actions", "name", "startingNumber"];
  dataSource: MatTableDataSource<AccountType>;
  totalCount: number;
  accountTypeRequest = new AccountTypeRequest();
  pageSizeOptions: [] = Page_Size_Options;

  constructor(
    private accountTypeService: AccountTypeService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.getAccountTypes(this.accountTypeRequest);
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
    });
  }

  getAccountTypes(request: AccountTypeRequest): void {
    this.accountTypeService.getAccountTypes(request).subscribe((res) => {
      this.dataSource = new MatTableDataSource<AccountType>(res?.data?.item1);
      console.log(res?.data?.item1);
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  remove(id): void {
    this.accountTypeService.deleteAccountType(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getAccountTypes(this.accountTypeRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }
  openForm(accountType?: AccountType): void {
    const dialogRef = this.dialog.open(AccountTypeFormComponent, {
      disableClose: true,
      data: accountType,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getAccountTypes(this.accountTypeRequest);
      }
    });
  }

  // Function to confirm deletion before calling the remove function
  confirmDelete(id: string, code: string = "") {
    // Open a confirmation dialog with a custom message and callback function
    const data: ConfirmDialogModel = {
      title: "Confirm Remove",
      message: `Deleting this item ${code} will affect related data. Confirm deletion?`,
    };
    this.confirmDialogService.confirmDialog(
      id,
      this.remove.bind(this),
      data.message
    );
  }

  reload() {
    this.loading = true;
    this.searchForm.reset();
    this.accountTypeRequest = new AccountTypeRequest();
    this.getAccountTypes(this.accountTypeRequest);
  }

  onSearch() {
    this.loading = true;
    this.updateAccountTypeRequest();
    this.getAccountTypes(this.accountTypeRequest);
  }

  onPageChange(pageEvent) {
    this.updateAccountTypeRequest(pageEvent.pageIndex, pageEvent.pageSize);
    this.getAccountTypes(this.accountTypeRequest);
  }

  private updateAccountTypeRequest(
    pageIndex = this.accountTypeRequest.page,
    pageSize = this.accountTypeRequest.rowsPerPage
  ) {
    this.accountTypeRequest = {
      ...this.accountTypeRequest,
      ...this.searchForm.value,
      page: pageIndex,
      rowsPerPage: pageSize,
    };
  }
  printPdf() {
    this.http
      .post(environment.apiURL + "/AccountType/print", this.searchForm.value, {
        responseType: "blob",
      })
      .subscribe((response) => {
        //Create a Blob from the PDF Stream
        const file = new Blob([response], { type: "application/pdf" });
        //Build a URL from the file
        const fileURL = URL.createObjectURL(file);
        //Open the URL on new Window
        const pdfWindow = window.open();
        pdfWindow.location.href = fileURL;
      });
  }
}
