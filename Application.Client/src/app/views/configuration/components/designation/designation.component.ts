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
import { DesignationRequest } from "../../models/designation/designation-request.model";
import { Designation } from "../../models/designation/designation.model";
import { DesignationService } from "../../services/designation.service";
import { DesignationFormComponent } from "./designation-form/designation-form.component";

@Component({
  selector: "app-designation",
  templateUrl: "./designation.component.html",
  styleUrls: ["./designation.component.scss"],
})
export class DesignationComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  displayedColumns: string[] = ["actions", "name", "department"];
  dataSource: MatTableDataSource<Designation>;
  totalCount: number;
  designationRequest = new DesignationRequest();
  pageSizeOptions: [] = Page_Size_Options;

  constructor(
    private designationService: DesignationService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.getDesignations(this.designationRequest);
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
    });
  }

  getDesignations(request: DesignationRequest): void {
    this.designationService.getDesignations(request).subscribe((res) => {
      this.dataSource = new MatTableDataSource<Designation>(res?.data?.item1);
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  remove(id): void {
    this.designationService.deleteDesignation(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getDesignations(this.designationRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }
  openForm(designation?: Designation): void {
    const dialogRef = this.dialog.open(DesignationFormComponent, {
      disableClose: true,
      data: designation,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getDesignations(this.designationRequest);
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
    this.designationRequest = new DesignationRequest();
    this.getDesignations(this.designationRequest);
  }

  onSearch() {
    this.loading = true;
    this.updateDesignationRequest();
    this.getDesignations(this.designationRequest);
  }

  onPageChange(pageEvent) {
    this.updateDesignationRequest(pageEvent.pageIndex, pageEvent.pageSize);
    this.getDesignations(this.designationRequest);
  }

  private updateDesignationRequest(
    pageIndex = this.designationRequest.page,
    pageSize = this.designationRequest.rowsPerPage
  ) {
    this.designationRequest = {
      ...this.designationRequest,
      ...this.searchForm.value,
      page: pageIndex,
      rowsPerPage: pageSize,
    };
  }
  printPdf() {
    this.http
      .post(environment.apiURL + "/Designation/print", this.searchForm.value, {
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
