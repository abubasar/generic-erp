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
import { EmailAccountRequest } from "../../models/email-account/email-account-request.model";
import { EmailAccount } from "../../models/email-account/email-account.model";
import { EmailAccountService } from "../../services/email-account.service";
import { EmailAccountFormComponent } from "./email-account-form/email-account-form.component";

@Component({
  selector: "app-email-account",
  templateUrl: "./email-account.component.html",
  styleUrls: ["./email-account.component.scss"],
})
export class EmailAccountComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  displayedColumns: string[] = [
    "actions",
    "displayName",
    "email",
    "host",
    "username",
    "password",
    "port",
    "enableSsl",
    "isDefaultEmailAccount",
  ];
  dataSource: MatTableDataSource<EmailAccount>;
  totalCount: number;
  emailAccountRequest = new EmailAccountRequest();
  pageSizeOptions: [] = Page_Size_Options;

  constructor(
    private emailAccountService: EmailAccountService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.getEmailAccounts(this.emailAccountRequest);
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [null],
    });
  }

  getEmailAccounts(request: EmailAccountRequest): void {
    this.emailAccountService.getEmailAccounts(request).subscribe((res) => {
      this.dataSource = new MatTableDataSource<EmailAccount>(res?.data?.item1);
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  remove(id): void {
    this.emailAccountService.deleteAccount(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getEmailAccounts(this.emailAccountRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }
  openForm(emailAccount?: EmailAccount): void {
    const dialogRef = this.dialog.open(EmailAccountFormComponent, {
      disableClose: true,
      data: emailAccount,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getEmailAccounts(this.emailAccountRequest);
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
    this.emailAccountRequest = new EmailAccountRequest();
    this.getEmailAccounts(this.emailAccountRequest);
  }

  onSearch() {
    this.loading = true;
    this.updateEmailAccountRequest();
    this.getEmailAccounts(this.emailAccountRequest);
  }

  onPageChange(pageEvent) {
    this.updateEmailAccountRequest(pageEvent.pageIndex, pageEvent.pageSize);
    this.getEmailAccounts(this.emailAccountRequest);
  }

  private updateEmailAccountRequest(
    pageIndex = this.emailAccountRequest.page,
    pageSize = this.emailAccountRequest.rowsPerPage
  ) {
    this.emailAccountRequest = {
      ...this.emailAccountRequest,
      ...this.searchForm.value,
      page: pageIndex,
      rowsPerPage: pageSize,
    };
  }
  printPdf() {
    this.http
      .post(environment.apiURL + "/EmailAccount/print", this.searchForm.value, {
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
