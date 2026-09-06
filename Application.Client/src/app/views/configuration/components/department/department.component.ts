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
import { DepartmentRequest } from "../../models/department/department-request.model";
import { Department } from "../../models/department/department.model";
import { DepartmentService } from "../../services/department.service";
import { DepartmentFormComponent } from "./department-form/department-form.component";

@Component({
  selector: "app-department",
  templateUrl: "./department.component.html",
  styleUrls: ["./department.component.scss"],
})
export class DepartmentComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  displayedColumns: string[] = ["actions", "name"];
  dataSource: MatTableDataSource<Department>;
  totalCount: number;
  departmentRequest = new DepartmentRequest();
  pageSizeOptions: [] = Page_Size_Options;

  constructor(
    private departmentService: DepartmentService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.getDepartments(this.departmentRequest);
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
    });
  }

  getDepartments(request: DepartmentRequest): void {
    this.departmentService.getDepartments(request).subscribe((res) => {
      this.dataSource = new MatTableDataSource<Department>(res?.data?.item1);
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  remove(id): void {
    this.departmentService.deleteDepartment(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getDepartments(this.departmentRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }
  openForm(department?: Department): void {
    const dialogRef = this.dialog.open(DepartmentFormComponent, {
      disableClose: true,
      data: department,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getDepartments(this.departmentRequest);
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
    this.departmentRequest = new DepartmentRequest();
    this.getDepartments(this.departmentRequest);
  }

  onSearch() {
    this.loading = true;
    this.updateDepartmentRequest();
    this.getDepartments(this.departmentRequest);
  }

  onPageChange(pageEvent) {
    this.updateDepartmentRequest(pageEvent.pageIndex, pageEvent.pageSize);
    this.getDepartments(this.departmentRequest);
  }

  private updateDepartmentRequest(
    pageIndex = this.departmentRequest.page,
    pageSize = this.departmentRequest.rowsPerPage
  ) {
    this.departmentRequest = {
      ...this.departmentRequest,
      ...this.searchForm.value,
      page: pageIndex,
      rowsPerPage: pageSize,
    };
  }
  printPdf() {
    this.http
      .post(environment.apiURL + "/Department/print", this.searchForm.value, {
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
