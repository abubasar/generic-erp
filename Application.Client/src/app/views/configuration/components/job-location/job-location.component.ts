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
import { JobLocationRequest } from "../../models/job-location/job-location-request.model";
import { JobLocation } from "../../models/job-location/job-location.model";
import { JobLocationService } from "../../services/job-location.service";
import { JobLocationFormComponent } from "./job-location-form/job-location-form.component";

@Component({
  selector: "app-job-location",
  templateUrl: "./job-location.component.html",
  styleUrls: ["./job-location.component.scss"],
})
export class JobLocationComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  displayedColumns: string[] = ["actions", "name"];
  dataSource: MatTableDataSource<JobLocation>;
  totalCount: number;
  jobLocationRequest = new JobLocationRequest();
  pageSizeOptions: [] = Page_Size_Options;

  constructor(
    private jobLocationService: JobLocationService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.getJobLocations(this.jobLocationRequest);
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
    });
  }

  getJobLocations(request: JobLocationRequest): void {
    this.jobLocationService.getJobLocations(request).subscribe((res) => {
      this.dataSource = new MatTableDataSource<JobLocation>(res?.data?.item1);
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  remove(id): void {
    this.jobLocationService.deleteJobLocation(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getJobLocations(this.jobLocationRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }
  openForm(jobLocation?: JobLocation): void {
    const dialogRef = this.dialog.open(JobLocationFormComponent, {
      disableClose: true,
      data: jobLocation,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getJobLocations(this.jobLocationRequest);
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
    this.jobLocationRequest = new JobLocationRequest();
    this.getJobLocations(this.jobLocationRequest);
  }
  onSearch() {
    this.loading = true;
    this.updateJobLocationRequest();
    this.getJobLocations(this.jobLocationRequest);
  }

  onPageChange(pageEvent) {
    this.updateJobLocationRequest(pageEvent.pageIndex, pageEvent.pageSize);
    this.getJobLocations(this.jobLocationRequest);
  }

  private updateJobLocationRequest(
    pageIndex = this.jobLocationRequest.page,
    pageSize = this.jobLocationRequest.rowsPerPage
  ) {
    this.jobLocationRequest = {
      ...this.jobLocationRequest,
      ...this.searchForm.value,
      page: pageIndex,
      rowsPerPage: pageSize,
    };
  }
  printPdf() {
    this.http
      .post(environment.apiURL + "/JobLocation/print", this.searchForm.value, {
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
