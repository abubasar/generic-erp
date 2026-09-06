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
import { ManufacturerRequest } from "../../models/manufacturer/manufacturer-request.model";
import { Manufacturer } from "../../models/manufacturer/manufacturer.model";
import { ManufacturerService } from "../../services/manufacturer.service";
import { ManufacturerFormComponent } from "./manufacturer-form/manufacturer-form.component";

@Component({
  selector: "app-manufacturer",
  templateUrl: "./manufacturer.component.html",
  styleUrls: ["./manufacturer.component.scss"],
})
export class ManufacturerComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  displayedColumns: string[] = ["actions", "name"];
  dataSource: MatTableDataSource<Manufacturer>;
  totalCount: number;
  manufacturerRequest = new ManufacturerRequest();
  pageSizeOptions: [] = Page_Size_Options;

  constructor(
    private manufacturerService: ManufacturerService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.getManufacturers(this.manufacturerRequest);
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
    });
  }

  getManufacturers(request: ManufacturerRequest): void {
    this.manufacturerService.getManufacturers(request).subscribe((res) => {
      this.dataSource = new MatTableDataSource<Manufacturer>(res?.data?.item1);
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  remove(id): void {
    this.manufacturerService.deleteManufacturer(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getManufacturers(this.manufacturerRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  openForm(manufacturer?: Manufacturer): void {
    const dialogRef = this.dialog.open(ManufacturerFormComponent, {
      disableClose: true,
      data: manufacturer,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getManufacturers(this.manufacturerRequest);
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
    this.manufacturerRequest = new ManufacturerRequest();
    this.getManufacturers(this.manufacturerRequest);
  }

  onSearch() {
    this.loading = true;
    this.updateManufacturerRequest();
    this.getManufacturers(this.manufacturerRequest);
  }

  onPageChange(pageEvent) {
    this.updateManufacturerRequest(pageEvent.pageIndex, pageEvent.pageSize);
    this.getManufacturers(this.manufacturerRequest);
  }

  private updateManufacturerRequest(
    pageIndex = this.manufacturerRequest.page,
    pageSize = this.manufacturerRequest.rowsPerPage
  ) {
    this.manufacturerRequest = {
      ...this.manufacturerRequest,
      ...this.searchForm.value,
      page: pageIndex,
      rowsPerPage: pageSize,
    };
  }
  printPdf() {
    this.http
      .post(environment.apiURL + "/Manufacturer/print", this.searchForm.value, {
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
