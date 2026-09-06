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
import { UserRequest } from "../../models/user/user-request.model";
import { User } from "../../models/user/user.model";
import { UserService } from "../../services/user.service";
import { UserFormComponent } from "./user-form/user-form.component";

@Component({
  selector: "app-user",
  templateUrl: "./user.component.html",
  styleUrls: ["./user.component.scss"],
})
export class UserComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  displayedColumns: string[] = ["actions", "username", "roleName"];
  dataSource: MatTableDataSource<User>;
  totalCount: number;
  userRequest = new UserRequest();
  pageSizeOptions: [] = Page_Size_Options;

  constructor(
    private userService: UserService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.getUsers(this.userRequest);
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
    });
  }

  getUsers(request: UserRequest): void {
    this.userService.getUsers(request).subscribe((res) => {
      this.dataSource = new MatTableDataSource<User>(res?.data?.item1);
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  remove(id): void {
    this.userService.deleteUser(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getUsers(this.userRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }
  openForm(user?: User): void {
    const dialogRef = this.dialog.open(UserFormComponent, {
      disableClose: true,
      data: user,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getUsers(this.userRequest);
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
    this.userRequest = new UserRequest();
    this.getUsers(this.userRequest);
  }

  onSearch() {
    this.loading = true;
    this.updateUserRequest();
    this.getUsers(this.userRequest);
  }

  onPageChange(pageEvent) {
    this.updateUserRequest(pageEvent.pageIndex, pageEvent.pageSize);
    this.getUsers(this.userRequest);
  }

  private updateUserRequest(
    pageIndex = this.userRequest.page,
    pageSize = this.userRequest.rowsPerPage
  ) {
    this.userRequest = {
      ...this.userRequest,
      ...this.searchForm.value,
      page: pageIndex,
      rowsPerPage: pageSize,
    };
  }
  printPdf() {
    this.http
      .post(environment.apiURL + "/User/print", this.searchForm.value, {
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
