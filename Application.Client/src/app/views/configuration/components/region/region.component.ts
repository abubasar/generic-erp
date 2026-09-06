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
import { RegionRequest } from "../../models/region/region-request.model";
import { Region } from "../../models/region/region.model";
import { RegionService } from "../../services/region.service";
import { RegionFormComponent } from "./region-form/region-form.component";

@Component({
  selector: "app-region",
  templateUrl: "./region.component.html",
  styleUrls: ["./region.component.scss"],
})
export class RegionComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  displayedColumns: string[] = ["actions", "name"];
  dataSource: MatTableDataSource<Region>;
  totalCount: number;
  regionRequest = new RegionRequest();
  pageSizeOptions: [] = Page_Size_Options;

  constructor(
    private regionService: RegionService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.getRegions(this.regionRequest);
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
    });
  }

  getRegions(request: RegionRequest): void {
    this.regionService.getRegions(request).subscribe((res) => {
      this.dataSource = new MatTableDataSource<Region>(res?.data?.item1);
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  remove(id): void {
    this.regionService.deleteRegion(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getRegions(this.regionRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }
  openForm(region?: Region): void {
    const dialogRef = this.dialog.open(RegionFormComponent, {
      disableClose: true,
      data: region,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getRegions(this.regionRequest);
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
    this.regionRequest = new RegionRequest();
    this.getRegions(this.regionRequest);
  }

  onSearch() {
    this.loading = true;
    this.updateRegionRequest();
    this.getRegions(this.regionRequest);
  }

  onPageChange(pageEvent) {
    this.updateRegionRequest(pageEvent.pageIndex, pageEvent.pageSize);
    this.getRegions(this.regionRequest);
  }

  private updateRegionRequest(
    pageIndex = this.regionRequest.page,
    pageSize = this.regionRequest.rowsPerPage
  ) {
    this.regionRequest = {
      ...this.regionRequest,
      ...this.searchForm.value,
      page: pageIndex,
      rowsPerPage: pageSize,
    };
  }
  printPdf() {
    this.http
      .post(environment.apiURL + "/Region/print", this.searchForm.value, {
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
