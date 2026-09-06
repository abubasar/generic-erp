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
import { RoleRequest } from "../../models/role/role-request.model";
import { Role } from "../../models/role/role.model";
import { RoleService } from "../../services/role.service";
import { RoleFormComponent } from "./role-form/role-form.component";
import { RolePermissionFormComponent } from "./role-permission-form/role-permission-form.component";

@Component({
  selector: "app-role",
  templateUrl: "./role.component.html",
  styleUrls: ["./role.component.scss"],
})
export class RoleComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  displayedColumns: string[] = ["actions", "name"];
  dataSource: MatTableDataSource<Role>;
  totalCount: number;
  roleRequest = new RoleRequest();
  pageSizeOptions: [] = Page_Size_Options;

  constructor(
    private roleService: RoleService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.getRoles(this.roleRequest);
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
    });
  }

  getRoles(request: RoleRequest): void {
    this.roleService.getRoles(request).subscribe((res) => {
      this.dataSource = new MatTableDataSource<Role>(res?.data?.item1);
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  remove(id): void {
    this.roleService.deleteRole(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getRoles(this.roleRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  openPermissionsForm(role: Role): void {
    const dialogRef = this.dialog.open(RolePermissionFormComponent, {
      data: role,
      panelClass: "mat-dialog-container-no-padding",
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getRoles(this.roleRequest);
      }
    });
  }

  openForm(role?: Role): void {
    const dialogRef = this.dialog.open(RoleFormComponent, {
      disableClose: true,
      data: role,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getRoles(this.roleRequest);
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
    this.roleRequest = new RoleRequest();
    this.getRoles(this.roleRequest);
  }

  onSearch() {
    this.loading = true;
    this.updateRoleRequest();
    this.getRoles(this.roleRequest);
  }

  onPageChange(pageEvent) {
    this.updateRoleRequest(pageEvent.pageIndex, pageEvent.pageSize);
    this.getRoles(this.roleRequest);
  }

  private updateRoleRequest(
    pageIndex = this.roleRequest.page,
    pageSize = this.roleRequest.rowsPerPage
  ) {
    this.roleRequest = {
      ...this.roleRequest,
      ...this.searchForm.value,
      page: pageIndex,
      rowsPerPage: pageSize,
    };
  }
  printPdf() {
    this.http
      .post(environment.apiURL + "/Role/print", this.searchForm.value, {
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
